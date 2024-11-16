namespace Shared.Systems
{
    public class Movement : System
    {
        public Movement() 
            : base(
                  typeof(Shared.Components.Movement),
                  typeof(Shared.Components.Position))
        { 
        }

        public override void update(TimeSpan elapsedTime)
        {
            foreach (var entity in m_entities.Values)
            {
                Shared.Entities.Utility.thrust(entity, elapsedTime);
            }
        }

    }
}
