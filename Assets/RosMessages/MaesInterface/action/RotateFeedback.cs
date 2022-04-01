//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.MaesInterface
{
    [Serializable]
    public class RotateFeedback : Message
    {
        public const string k_RosMessageName = "maes_interface/Rotate";
        public override string RosMessageName => k_RosMessageName;

        //  Feedback
        public float remaining;

        public RotateFeedback()
        {
            this.remaining = 0.0f;
        }

        public RotateFeedback(float remaining)
        {
            this.remaining = remaining;
        }

        public static RotateFeedback Deserialize(MessageDeserializer deserializer) => new RotateFeedback(deserializer);

        private RotateFeedback(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.remaining);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.remaining);
        }

        public override string ToString()
        {
            return "RotateFeedback: " +
            "\nremaining: " + remaining.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Feedback);
        }
    }
}