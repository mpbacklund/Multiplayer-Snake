namespace Shared.Components
{
    public class SnakeData : Component
    {
        public enum snakeState
        {
            alive,
            dead,
            invincible
        }

        public int tailId;
        public int score;
        public int foodToNextSegment;
        public int kills;
        public snakeState state;

        public SnakeData()
        {
            score = 0;
            foodToNextSegment = 0;
            kills = 0;
            state = snakeState.invincible;
        }
    }
}
