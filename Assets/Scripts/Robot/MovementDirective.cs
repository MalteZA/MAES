namespace Dora.Robot
{
    
    public class MovementDirective
    {
        public readonly float LeftWheelSpeed;
        public readonly float RightWheelSpeed;

        public static readonly MovementDirective Left = new MovementDirective(-1f, 1f);
        public static readonly MovementDirective Right = new MovementDirective(1f, -1f);
        public static readonly MovementDirective Forward = new MovementDirective(1f, 1f);
        public static readonly MovementDirective Reverse = new MovementDirective(-1f, -1f);
        public static readonly MovementDirective NoMovement = new MovementDirective(0f, 0f);
        

        public MovementDirective(float leftWheelSpeed, float rightWheelSpeed)
        {
            RightWheelSpeed = rightWheelSpeed;
            LeftWheelSpeed = leftWheelSpeed;
        }
    }

}