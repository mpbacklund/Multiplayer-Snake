
namespace Shared.Components
{
    public class Input : Component
    {
        public enum Type : UInt16
        {
            Left,
            Right,
            Up,
            Down
        }

        public Input(List<Type> inputs)
        {
            this.inputs = inputs;
        }

        public List<Type> inputs { get; private set; }
    }
}
