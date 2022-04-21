//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Maes
{
    [Serializable]
    public class StateMsg : Message
    {
        public const string k_RosMessageName = "maes_msgs/State";
        public override string RosMessageName => k_RosMessageName;

        public string status;
        public bool colliding;
        public BroadcastMsg[] incoming_broadcast_msgs;
        public EnvironmentTagMsg[] tags_nearby;
        public NearbyRobotMsg[] nearby_robots;

        public StateMsg()
        {
            this.status = "";
            this.colliding = false;
            this.incoming_broadcast_msgs = new BroadcastMsg[0];
            this.tags_nearby = new EnvironmentTagMsg[0];
            this.nearby_robots = new NearbyRobotMsg[0];
        }

        public StateMsg(string status, bool colliding, BroadcastMsg[] incoming_broadcast_msgs, EnvironmentTagMsg[] tags_nearby, NearbyRobotMsg[] nearby_robots)
        {
            this.status = status;
            this.colliding = colliding;
            this.incoming_broadcast_msgs = incoming_broadcast_msgs;
            this.tags_nearby = tags_nearby;
            this.nearby_robots = nearby_robots;
        }

        public static StateMsg Deserialize(MessageDeserializer deserializer) => new StateMsg(deserializer);

        private StateMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.status);
            deserializer.Read(out this.colliding);
            deserializer.Read(out this.incoming_broadcast_msgs, BroadcastMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.tags_nearby, EnvironmentTagMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.nearby_robots, NearbyRobotMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.status);
            serializer.Write(this.colliding);
            serializer.WriteLength(this.incoming_broadcast_msgs);
            serializer.Write(this.incoming_broadcast_msgs);
            serializer.WriteLength(this.tags_nearby);
            serializer.Write(this.tags_nearby);
            serializer.WriteLength(this.nearby_robots);
            serializer.Write(this.nearby_robots);
        }

        public override string ToString()
        {
            return "StateMsg: " +
            "\nstatus: " + status.ToString() +
            "\ncolliding: " + colliding.ToString() +
            "\nincoming_broadcast_msgs: " + System.String.Join(", ", incoming_broadcast_msgs.ToList()) +
            "\ntags_nearby: " + System.String.Join(", ", tags_nearby.ToList()) +
            "\nnearby_robots: " + System.String.Join(", ", nearby_robots.ToList());
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
