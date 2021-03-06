//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Maes
{
    [Serializable]
    public class Vector2DMsg : Message
    {
        public const string k_RosMessageName = "maes_msgs/Vector2D";
        public override string RosMessageName => k_RosMessageName;

        public double x;
        public double y;

        public Vector2DMsg()
        {
            this.x = 0.0;
            this.y = 0.0;
        }

        public Vector2DMsg(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2DMsg Deserialize(MessageDeserializer deserializer) => new Vector2DMsg(deserializer);

        private Vector2DMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.x);
            deserializer.Read(out this.y);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.x);
            serializer.Write(this.y);
        }

        public override string ToString()
        {
            return "Vector2DMsg: " +
            "\nx: " + x.ToString() +
            "\ny: " + y.ToString();
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
