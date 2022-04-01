//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.MaesInterface
{
    [Serializable]
    public class RobotMsgMsg : Message
    {
        public const string k_RosMessageName = "maes_interface/RobotMsg";
        public override string RosMessageName => k_RosMessageName;

        public string msg;
        public long sender_id;

        public RobotMsgMsg()
        {
            this.msg = "";
            this.sender_id = 0;
        }

        public RobotMsgMsg(string msg, long sender_id)
        {
            this.msg = msg;
            this.sender_id = sender_id;
        }

        public static RobotMsgMsg Deserialize(MessageDeserializer deserializer) => new RobotMsgMsg(deserializer);

        private RobotMsgMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.msg);
            deserializer.Read(out this.sender_id);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.msg);
            serializer.Write(this.sender_id);
        }

        public override string ToString()
        {
            return "RobotMsgMsg: " +
            "\nmsg: " + msg.ToString() +
            "\nsender_id: " + sender_id.ToString();
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