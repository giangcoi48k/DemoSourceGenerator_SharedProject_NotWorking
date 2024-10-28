using BlazorApp.Shared;

namespace BlazorApp.Services
{
    [NeedGenerator]
    public interface ISomeService1
    {
        List<string> GetSomeValues();
    }

    class SomeService1 : ISomeService1
    {
        public List<string> GetSomeValues()
        {
            return ["1", "2", "3", "4"];
        }
    }
}
