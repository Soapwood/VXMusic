namespace VRChatOSCLib
{
    /// <summary>VRChat OSC Message containing received data from parameters or others.</summary>
    public struct VRCMessage
    {
        private const string avatar_change_address = "/avatar/change";
        private const string avatar_parameters_address = "/avatar/parameters";

        private static readonly string[] default_avatar_parameters = new string[] { "Viseme", "Voice", "GestureLeft", "GestureLeftWeight", "GestureRight", "GestureRightWeight", "TrackingType", "VRMode", "MuteSelf", "Grounded", "AngularY", "Upright", "AFK", "Seated", "InStation", "VelocityX", "VelocityY", "VelocityZ", "IsLocal", "AvatarVersion", "VRCEmote", "VRCFaceBlendH", "VRCFaceBlendV" };

        /// <summary>Received message path type.</summary>
        public enum MessageType
        {
            /// <summary>Path type is Unknown</summary>
            Unknown,
            /// <summary>Path is a VRChat default avatar parameter.</summary>
            DefaultParameter,
            /// <summary>Path is a custom avatar parameter.</summary>
            AvatarParameter,
            /// <summary>Path is an avatar change event.</summary>
            AvatarChange
        }

        /// <summary>True if the path is an avatar parameter of any kind.</summary>
        public readonly bool IsParameter;
        /// <summary>The type of message contained by the address.</summary>
        public readonly MessageType Type;
        /// <summary>Original OscMessage object received.</summary>
        public readonly OscCore.OscMessage Original;
        /// <summary>Address received from this object.</summary>
        public readonly string Address;
        /// <summary>Address before last path separator '/'</summary>
        public readonly string Path;

        /// <summary>Access message arguments by index.</summary>
        /// <param name="index">The index of the message.</param>
        /// <returns>Value at the supplied index.</returns>
        public object this[int index] => Original[index];
        /// <summary>Number of arguments in the message.</summary>
        public int Count => Original.Count;
        /// <summary>Retrieve the first value arguments.</summary>
        /// <returns>Default if nothing was received.</returns>
        public object? GetValue() => Original.FirstOrDefault();
        /// <summary>Retrieve and cast the first value arguments to Type.</summary>
        /// <typeparam name="T">Type to cast the returned object into.</typeparam>
        /// <returns>Casted value, default if nothing was received.</returns>
        public T? GetValue<T>() {
            object? value = GetValue();
            return value == null ? default(T) : (T)value;
        }
        /// <summary>Access and cacst message arguments by index.</summary>
        /// <typeparam name="T">Type to cast the returned object into.</typeparam>
        /// <param name="index">The index of the message.</param>
        /// <returns>Value at the supplied index.</returns>
        public T GetValueAt<T>(int index) => (T)Original[index];

        internal VRCMessage(OscCore.OscMessage message)
        {
            Original = message;
            Address = message.Address;
            Path = Address.Substring(Address.LastIndexOf('/'));
            IsParameter = Address.StartsWith(avatar_parameters_address);

            if (IsParameter)
            {
                Type = Array.Exists(default_avatar_parameters, p => message.Address.EndsWith(p)) ?
                MessageType.DefaultParameter : MessageType.AvatarParameter;
            }
            else if (Address.StartsWith(avatar_change_address)) Type = MessageType.AvatarChange;
            else Type = MessageType.Unknown;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Original.ToString();

        /// <summary>Print this message's content to the console.</summary>
        public void Print()
        {
            string timestamp = $"\u001b[30;1m[\u001b[37;1m{DateTime.Now.ToString("HH:mm:ss")}\u001b[30;1m]\u001b[0m";
            string type = $"\u001b[36;1mTYPE\u001b[30;1m(\u001b[33;1m{Type}\u001b[30;1m)\u001b[0m";
            string endpoint = $"\u001b[36;1mADDRESS\u001b[30;1m(\u001b[37;1m{Address}\u001b[30;1m)\u001b[0m";
            string values = "";

            for (int i = 0; i < Original.Count; i++)
            {
                var v = Original[i];
                var t = v.GetType().Name.ToUpperInvariant();
                values += $"\u001b[36;1m{t}\u001b[30;1m(\u001b[37;1m{v}\u001b[30;1m)\u001b[0m ";
            }

            Console.WriteLine($"{timestamp} {type} {endpoint} {values}");
        }
    }
}
