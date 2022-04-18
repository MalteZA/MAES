//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.MaesInterface
{
    [Serializable]
    public class Broadcast_ResponseMsg : Message
    {
        public const string k_RosMessageName = "maes_interface/Broadcast_Response";
        public override string RosMessageName => k_RosMessageName;

        public string success_status;

        public Broadcast_ResponseMsg()
        {
            this.success_status = "";
        }

        public Broadcast_ResponseMsg(string success_status)
        {
            this.success_status = success_status;
        }

        public static Broadcast_ResponseMsg Deserialize(MessageDeserializer deserializer) => new Broadcast_ResponseMsg(deserializer);

        private Broadcast_ResponseMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.success_status);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.success_status);
        }

        public override string ToString()
        {
            return "Broadcast_ResponseMsg: " +
            "\nsuccess_status: " + success_status.ToString();
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
