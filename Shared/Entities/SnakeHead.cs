using Microsoft.Xna.Framework;
using Shared.Components;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Shared.Messages;


namespace Shared.Entities
{
    public class SnakeHead
    {
        public static Entity create(uint snakeId, string texture, string name, Vector2 position, float size, float moveRate)
        {
            Entity entity = new Entity(); // creates new entity and assigns the entity an id

            entity.add(new Name(name));
            entity.add(new Appearance(texture));

            entity.add(new Position(position));
            entity.add(new Size(new Vector2(size, size)));
            entity.add(new Movement(moveRate));
            entity.add(new SnakeId(snakeId));
            entity.add(new SnakeData());
            entity.add(new Shared.Components.Collision());

            List<Shared.Components.Input.Type> inputs = new List<Shared.Components.Input.Type>();
            inputs.Add(Shared.Components.Input.Type.Left);
            inputs.Add(Shared.Components.Input.Type.Right);
            inputs.Add(Shared.Components.Input.Type.Up);
            inputs.Add(Shared.Components.Input.Type.Down);
            entity.add(new Shared.Components.Input(inputs));

            // Start a timer to change the snake head state to "alive" after 3 seconds
            Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith((task) =>
            {
                var snakeData = entity.get<SnakeData>();
                snakeData.state = SnakeData.snakeState.alive;
            });

            return entity;
        }
    }

    // applies to all entities
    public class Utility
    {
        public static void thrust(Entity entity, TimeSpan elapsedTime)
        {
            var movement = entity.get<Movement>();

            if (entity.contains<TurnQueue>())
            {
                var turnQueue = entity.get<TurnQueue>().queue;

                double totalDistanceToMove = movement.moveRate * elapsedTime.TotalMilliseconds;
                double totalDistanceMoved = 0;

                while (totalDistanceMoved < totalDistanceToMove && turnQueue.Count > 0)
                {
                    var position = entity.get<Position>();
                    var nextTurn = turnQueue.Peek();

                    // Calculate the distance to the next turn
                    double distanceToNextTurn = Vector2.Distance(position.position, nextTurn.Item2);

                    // Calculate the remaining distance to move in this update cycle
                    double remainingDistance = totalDistanceToMove - totalDistanceMoved;

                    double distanceToMove = Math.Min(distanceToNextTurn, remainingDistance);

                    // Move entity towards the next turn point
                    position.position += Vector2.Normalize(nextTurn.Item2 - position.position) * (float)distanceToMove;

                    totalDistanceMoved += distanceToMove;

                    // Check if the entity reached the turn point
                    if (Math.Abs(distanceToMove - distanceToNextTurn) < 0.001) // Considering floating-point precision
                    {
                        position.orientation = nextTurn.Item1;
                        turnQueue.Dequeue();
                    }
                }

                // If the entity hasn't moved the full distance, move it the remaining distance
                if (totalDistanceMoved < totalDistanceToMove)
                {
                    move(entity, totalDistanceToMove - totalDistanceMoved);
                }
            }
            else
            {
                move(entity, movement.moveRate * elapsedTime.TotalMilliseconds);
            }
        }

        private static void move(Entity entity, double distance)
        {
            var position = entity.get<Position>();
            var vectorX = Math.Cos(position.orientation);
            var vectorY = Math.Sin(position.orientation);

            position.position += new Vector2(
                (float)(vectorX * distance),
                (float)(vectorY * distance));
        }

        public static bool rotate(Entity entity, List<Shared.Components.Input.Type> inputs)
        {
            var position = entity.get<Position>();
            var movement = entity.get<Movement>();

            float oldFacing = position.orientation;
            float newFacing = position.orientation;

            if (inputs.Contains(Shared.Components.Input.Type.Up))
            {
                if (inputs.Contains(Shared.Components.Input.Type.Left))
                    newFacing = 5 * MathHelper.PiOver4;
                else if (inputs.Contains(Shared.Components.Input.Type.Right))
                    newFacing = 7 * MathHelper.PiOver4;
                else if (oldFacing == MathHelper.PiOver2) // prevents snake from turning back on itself
                    newFacing = oldFacing;
                else
                    newFacing = 3 * MathHelper.PiOver2;
            }
            else if (inputs.Contains(Shared.Components.Input.Type.Down))
            {
                if (inputs.Contains(Shared.Components.Input.Type.Left))
                    newFacing = 3 * MathHelper.PiOver4;
                else if (inputs.Contains(Shared.Components.Input.Type.Right))
                    newFacing = MathHelper.PiOver4;
                else if (oldFacing == 3 * MathHelper.PiOver2) // prevents snake from turning back on itself
                    newFacing = oldFacing;
                else
                    newFacing = MathHelper.PiOver2;
            }
            else if (inputs.Contains(Shared.Components.Input.Type.Left) && oldFacing != 0 && oldFacing != 2 * MathHelper.Pi)
            {
                newFacing = MathHelper.Pi;
            }
            else if (inputs.Contains(Shared.Components.Input.Type.Right) && oldFacing != MathHelper.Pi)
            {
                newFacing = 0;
            }

            position.orientation = newFacing;

            // returns true if the snake rotated
            return newFacing != oldFacing;
        }
    }
}
