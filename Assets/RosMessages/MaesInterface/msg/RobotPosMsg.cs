//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.MaesInterface
{
    [Serializable]
    public class RobotPosMsg : Message
    {
        public const string k_RosMessageName = "maes_interface/RobotPos";
        public override string RosMessageName => k_RosMessageName;

        public double x;
        public double y;
        public float orientation;

        public RobotPosMsg()
        {
            this.x = 0.0;
            this.y = 0.0;
            this.orientation = 0.0f;
        }

        public RobotPosMsg(double x, double y, float orientation)
        {
            this.x = x;
            this.y = y;
            this.orientation = orientation;
        }

        public static RobotPosMsg Deserialize(MessageDeserializer deserializer) => new RobotPosMsg(deserializer);

        private RobotPosMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.x);
            deserializer.Read(out this.y);
            deserializer.Read(out this.orientation);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.x);
            serializer.Write(this.y);
            serializer.Write(this.orientation);
        }

        public override string ToString()
        {
            return "RobotPosMsg: " +
            "\nx: " + x.ToString() +
            "\ny: " + y.ToString() +
            "\norientation: " + orientation.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
