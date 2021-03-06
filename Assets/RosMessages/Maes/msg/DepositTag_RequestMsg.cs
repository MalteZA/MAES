//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Maes
{
    [Serializable]
    public class DepositTag_RequestMsg : Message
    {
        public const string k_RosMessageName = "maes_msgs/DepositTag_Request";
        public override string RosMessageName => k_RosMessageName;

        public string msg;

        public DepositTag_RequestMsg()
        {
            this.msg = "";
        }

        public DepositTag_RequestMsg(string msg)
        {
            this.msg = msg;
        }

        public static DepositTag_RequestMsg Deserialize(MessageDeserializer deserializer) => new DepositTag_RequestMsg(deserializer);

        private DepositTag_RequestMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.msg);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.msg);
        }

        public override string ToString()
        {
            return "DepositTag_RequestMsg: " +
            "\nmsg: " + msg.ToString();
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
