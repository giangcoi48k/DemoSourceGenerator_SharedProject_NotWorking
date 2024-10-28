using BlazorApp.Shared;

namespace BlazorApp.Shared.Services
{
    [NeedGenerator]
    public interface ISomeService2
    {
        List<string> GetSomeValues();
    }

    public class SomeService2 : ISomeService2
    {
        public List<string> GetSomeValues()
        {
            return ["5", "6", "7", "8"];
        }
    }
}
