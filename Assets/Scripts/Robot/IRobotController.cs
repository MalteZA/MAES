namespace Dora.Robot
{
    public interface IRobotController
    {

        void UpdateMotorPhysics(SimulationConfiguration config);
        void UpdateLogic(SimulationConfiguration config);

        // Returns true if the robot has encountered a new collision since the previous logic update
        bool HasCollided();
        
        RobotStatus GetStatus();
        //void SenseSurroundings();
        void MoveForward();
        void MoveBackwards();
        
        /* Keep rotating the robot until the robot has rotated approximately the given amount of degrees */ 
        void Rotate(float degrees);
        
        /* Will start automatically rotating every tick until stopped through StopCurrentAction() */
        void StartRotating(bool counterClockwise=false);

        /* Stops performing the current action (such as rotating or moving) */
        void StopCurrentAction();
    }
    
}
