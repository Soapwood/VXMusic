namespace VXMusic.Overlay
{
    public static class VXMMessage
    {
        public const string CONNECTION_HEARTBEAT_REQUEST = "VX_HEARTBEAT_REQ";
        public const string CONNECTION_HEARTBEAT_ACKNOWLEDGE = "VX_HEARTBEAT_ACK";
        
        public const string CONNECTION_TERMINATION = "VX_CONNECTION_TERM";
        
        public const string CONNECTION_REQUEST = "VX_CONNECT_REQ";
        public const string CONNECTION_ACKNOWLEDGE = "VX_CONNECT_ACK";
        public const string CONNECTION_FINISH = "VX_CONNECT_FIN";
        
        public const string RECOGNITION_REQUEST = "VX_RECOGNITION_REQ";
        public const string RECOGNITION_ACKNOWLEDGE = "VX_RECOGNITION_ACK";
        public const string RECOGNITION_FINISH = "VX_RECOGNITION_FIN";
    }
}