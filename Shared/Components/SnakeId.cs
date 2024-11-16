namespace Shared.Components
{
    public class SnakeId : Component
    {
        public SnakeId(uint id)
        {
            this.id = id;
        }

        public uint id {  get; private set; }
    }
}
