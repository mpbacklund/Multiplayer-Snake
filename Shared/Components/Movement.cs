
namespace Shared.Components
{
    public class Movement : Component
    {
        public Movement(float moveRate)
        {
            this.moveRate = moveRate;
        }

        public float moveRate { get; private set; }
    }
}
