using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace BlazorApp.SourceGenerator.ServerSide
{
    [Generator]
    public class ControllerSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var interfaces = context.SyntaxProvider.ForAttributeWithMetadataName(
                "BlazorApp.Shared.NeedGeneratorAttribute",
                predicate: static (node, _) => node is InterfaceDeclarationSyntax,
                transform: static (context, _) => (InterfaceDeclarationSyntax)context.TargetNode
            )
            .Where(x => x != null)
            .Collect();

            context.RegisterSourceOutput(interfaces, (context, interfaces) =>
            {
                foreach (var @interface in interfaces)
                {
                    var controllerSource = GenerateController(@interface);
                    var fileName = $"{@interface.Identifier.Text}Controller.g.cs";
                    context.AddSource(fileName, SourceText.From(controllerSource, Encoding.UTF8));
                }
            });
        }

        private static string GenerateController(InterfaceDeclarationSyntax interfaceSyntax)
        {
            var interfaceName = interfaceSyntax.Identifier.Text;
            var controllerName = interfaceName.Replace("I", "") + "Controller";

            // Get the namespace of the interface
            var interfaceNamespace = GetNamespace(interfaceSyntax);

            var methods = interfaceSyntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(GenerateMethod)
                .ToList();

            var methodsCode = string.Join("\n", methods);
            var wrappersCode = string.Join("\n", interfaceSyntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(GenerateWrapperClass));

            return $$"""
            using Microsoft.AspNetCore.Mvc;
            using {{interfaceNamespace}};  // Ensure the interface's namespace is imported

            namespace BlazorApp.Controllers
            {
                [Route("api/[controller]")]
                [ApiController]
                public class {{controllerName}} : ControllerBase
                {
                    private readonly {{interfaceName}} _service;

                    public {{controllerName}}({{interfaceName}} service)
                    {
                        _service = service;
                    }

                    {{methodsCode}}
                }

                {{wrappersCode}}
            }
            """;
        }

        private static string GenerateMethod(MethodDeclarationSyntax methodSyntax)
        {
            var methodName = methodSyntax.Identifier.Text;
            var isAsync = methodSyntax.ReturnType.ToString().StartsWith("Task");
            var hasParameters = methodSyntax.ParameterList.Parameters.Count > 0;

            // Check if there is a CancellationToken
            var parameters = methodSyntax.ParameterList.Parameters;
            var hasCancellationToken = parameters.Any(p => p.Type.ToString() == "CancellationToken");

            // Generate the list of parameters passed to the service
            var paramNames = string.Join(", ", parameters.Select(p =>
                p.Identifier.Text == "cancellationToken" ? "cancellationToken" : $"request.{p.Identifier.Text}"));

            // Determine if a request wrapper is needed
            var requiresWrapper = parameters.Count > 1 || (parameters.Count == 1 && !hasCancellationToken);

            // Generate async or sync code based on the return type
            var asyncModifier = isAsync ? "async " : "";
            var awaitModifier = isAsync ? "await " : "";
            var actionReturnType = isAsync ? "Task<IActionResult>" : "IActionResult";

            // Generate the action
            return $$"""
            [HttpPost("{{methodName}}")]
            public {{asyncModifier}}{{actionReturnType}} {{methodName}}(
                {{(requiresWrapper ? $"[FromBody] {methodName}Request request" : "")}}
                {{(hasCancellationToken ? (requiresWrapper ? ", " : "") + "CancellationToken cancellationToken" : "")}})
            {
                var result = {{awaitModifier}}_service.{{methodName}}({{paramNames}});
                return {{(isAsync ? "Ok(await result)" : "Ok(result)")}};
            }
            """;
        }

        // Function to get the actual result type of Task<T> or Task
        private static string GetTaskResultType(string returnType)
        {
            if (returnType.StartsWith("Task<") && returnType.EndsWith(">"))
            {
                return returnType.Substring(5, returnType.Length - 6); // Extract the part inside <>
            }
            return "void"; // If it's a Task without T, treat it as void
        }

        private static string GenerateWrapperClass(MethodDeclarationSyntax methodSyntax)
        {
            // Skip if no wrapper is needed
            var parameters = methodSyntax.ParameterList.Parameters
                .Where(p => p.Type.ToString() != "CancellationToken").ToList();
            if (parameters.Count <= 1) return "";

            var className = $"{methodSyntax.Identifier.Text}Request";
            var properties = string.Join("\n", parameters.Select(p =>
                $"public {p.Type} {p.Identifier.Text} {{ get; set; }}"));

            return $$"""
            public class {{className}}
            {
                {{properties}}
            }
            """;
        }

        // Helper function to get the namespace of an interface
        private static string GetNamespace(InterfaceDeclarationSyntax interfaceSyntax)
        {
            var parent = interfaceSyntax.Parent;
            while (parent is not NamespaceDeclarationSyntax && parent is not FileScopedNamespaceDeclarationSyntax)
            {
                parent = parent.Parent;
            }

            return parent is BaseNamespaceDeclarationSyntax namespaceDeclaration
                ? namespaceDeclaration.Name.ToString()
                : string.Empty;
        }
    }
}

