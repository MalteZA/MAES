//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.MaesInterface
{
    [Serializable]
    public class NearbyRobotMsg : Message
    {
        public const string k_RosMessageName = "maes_interface/NearbyRobot";
        public override string RosMessageName => k_RosMessageName;

        public double distance;
        public float angle;
        public long robot_id;

        public NearbyRobotMsg()
        {
            this.distance = 0.0;
            this.angle = 0.0f;
            this.robot_id = 0;
        }

        public NearbyRobotMsg(double distance, float angle, long robot_id)
        {
            this.distance = distance;
            this.angle = angle;
            this.robot_id = robot_id;
        }

        public static NearbyRobotMsg Deserialize(MessageDeserializer deserializer) => new NearbyRobotMsg(deserializer);

        private NearbyRobotMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.distance);
            deserializer.Read(out this.angle);
            deserializer.Read(out this.robot_id);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.distance);
            serializer.Write(this.angle);
            serializer.Write(this.robot_id);
        }

        public override string ToString()
        {
            return "NearbyRobotMsg: " +
            "\ndistance: " + distance.ToString() +
            "\nangle: " + angle.ToString() +
            "\nrobot_id: " + robot_id.ToString();
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
