
namespace Shared.Components
{
    public class Name : Component
    {
        public Name(string name)
        {
            this.name = name;
        }

        public string name { get; private set; }
    }
}
