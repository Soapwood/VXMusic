using OscCore;
using System.Net;
using System.Net.Sockets;

namespace VRChatOSCLib
{
    /// <summary>
    /// Main VRChat OSC class.
    /// </summary>
    public partial class VRChatOSC : IDisposable
    {
        private const string chatbox_input_address = "/chatbox/input";
        private const string chatbox_typing_address = "/chatbox/typing";
        private const string avatar_parameters_address = "/avatar/parameters";
        private const string input_address = "/input/";

        /// <summary>Pretty print received messages into the Console buffer.</summary>
        public bool LogMessages;

        private bool m_Listening;
        private bool m_Started;
        private bool m_Disposed;

        /// <summary>Remote end point used to send data to the server.</summary>
        public IPEndPoint? RemoteEndPoint { get; private set; }
        private Socket m_Client;

        /// <summary>Local end point used to receive data to the client.</summary>
        public IPEndPoint? LocalEndPoint { get; private set; }
        private Socket m_Server;
        private Thread? m_Thread;
        private int m_BufferLength = 4096;

        /// <summary>EventsHandler invoked on receiving VRChat OSC messages such as avatar parameters.</summary>
        public EventHandler<VRCMessage>? OnMessage;

        #region Constructor
        /// <summary>
        /// Initialize and instance of this object.<para />
        /// You will need to call Connect() to send data.
        /// </summary>
        public VRChatOSC() { }

        /// <summary>
        /// Initialize and instance of this object.<para />
        /// Optionally auto connect to send data.
        /// </summary>
        public VRChatOSC(bool connect) : this() { if (connect) Connect(); }

        /// <param name="remotePort">The remote port used by VRChat to receive data we send to the Client.</param>
        public VRChatOSC(int remotePort = 9000) : this() => Connect(remotePort);

        /// <param name="ipAddress">The remote IPAddress where VRChat is located and receiving data.</param>
        /// <param name="remotePort">The remote port used by VRChat to receive data we send to the Client.</param>
        public VRChatOSC(string ipAddress, int remotePort = 9000) : this() => Connect(ipAddress, remotePort);

        /// <param name="ipAddress">The remote IPAddress where VRChat is located and receiving data.</param>
        /// <param name="remotePort">The remote port used by VRChat to receive data we send to the Client.</param>
        public VRChatOSC(IPAddress ipAddress, int remotePort = 9000) : this() => Connect(ipAddress, remotePort);
        #endregion

        #region Connection
        /// <param name="remotePort">The remote port used by VRChat to receive data we send to the Client.</param>
        public void Connect(int remotePort = 9000) => Connect(IPAddress.Loopback, remotePort);

        /// <param name="ipAddress">The remote IPAddress where VRChat is located and receiving data.</param>
        /// <param name="remotePort">The remote port used by VRChat to receive data we send to the Client.</param>
        public void Connect(string ipAddress, int remotePort = 9000) => Connect(IPAddress.Parse(ipAddress), remotePort);

        /// <param name="ipAddress">The remote IPAddress where VRChat is located and receiving data.</param>
        /// <param name="remotePort">The remote port used by VRChat to receive data we send to the Client.</param>
        public void Connect(IPAddress ipAddress, int remotePort = 9000)
        {
            if (m_Started) return;

            m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (ipAddress.Equals(IPAddress.Broadcast))
                m_Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            RemoteEndPoint = new IPEndPoint(ipAddress, remotePort);
            m_Client.Connect(RemoteEndPoint);

            m_Started = true;
        }

        /// <summary>Listen for incoming messages from the VRChat client.</summary>
        /// <param name="localPort">The local port for receiving data on your network.</param>
        /// <param name="bufferLength">UDP buffer length, default is 4096</param>
        public void Listen(int localPort = 9001, int bufferLength = 4096)
        {
            if (m_Listening) return;

            LocalEndPoint = new IPEndPoint(IPAddress.Any, localPort);

            m_Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = int.MaxValue };
            // Allow other processes to reuse this ip-address/port pair.
            m_Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            m_Server.Bind(LocalEndPoint);

            m_Thread = new Thread(Serve);
            m_Thread.Start();

            m_BufferLength = bufferLength;

            m_Listening = true;
        }
        #endregion


        #region Methods
        /// <summary>Sends a raw OSC Packet.</summary>
        /// <param name="packet">Raw OSC Packet.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="Exception">Object not started properly.</exception>
        public void Send(OscPacket packet)
        {
            if (m_Disposed) throw new ObjectDisposedException("This object has been disposed.");
            if (!m_Started) throw new Exception("Trying to send data on an object that has not been started.\n\nPlase call <VRChatOSC>.Connect() after contructor.");

            var data = packet.ToByteArray();
            _ = m_Client.Send(data);
        }

        /// <summary>Send an array of objects to an address.</summary>
        /// <param name="address">Address</param>
        /// <param name="args">Object Parameters</param>
        public void SendTo(string address, params object[] args) => Send(new OscMessage(address, args));

        /// <summary>Sends an Integer value to an Avatar parameter.</summary>
        /// <param name="name">The name of the Avatar parameter.</param>
        /// <param name="value">The value sent to this parameter.</param>
        public void SendParameter(string name, int value) => SendTo($"{avatar_parameters_address}/{name}", value);

        /// <summary>Sends an Float value to an Avatar parameter.</summary>
        /// <param name="name">The name of the Avatar parameter.</param>
        /// <param name="value">The value sent to this parameter.</param>
        public void SendParameter(string name, float value) => SendTo($"{avatar_parameters_address}/{name}", value);

        /// <summary>Sends an Boolean value to an Avatar parameter.</summary>
        /// <param name="name">The name of the Avatar parameter.</param>
        /// <param name="value">The value sent to this parameter.</param>
        public void SendParameter(string name, bool value) => SendTo($"{avatar_parameters_address}/{name}", value);

        /// <summary>Toggles a referenced Boolean value and sends it to an Avatar parameter.</summary>
        /// <param name="name">The name of the Avatar parameter.</param>
        /// <param name="value">The referenced value to toggle.</param>
        public void SendParameter(string name, ref bool value) => SendParameter(name, value = !value);


        /// <summary>Send inputs to VRChat and control things such as locomotion.</summary>
        /// <param name="input">Any VRCInput that inherits the IVRCInput interface.</param>
        /// <param name="value">
        /// A float (-1f to 1f) for Axis.<para />
        /// An int (0, 1) for buttons.
        /// </param>
        public void SendInput(IVRCInput input, object value) => SendTo(input.Value, value);

        /// <summary>
        /// Send Axes input to VRChat.<para />
        /// Axes expect a float from -1 to 1 to control things like movement.<para />
        /// They expect to reset to 0 when not in use - otherwise a 'MoveForward' message left at '1' will continue to move you forward forever!
        /// </summary>
        /// <param name="input">A VRCAxes property, for example: VRCAxes.Vertical</param>
        /// <param name="value">Value from -1f to 1f</param>
        public void SendInput(VRCAxes input, float value) => SendTo(input.Value, value);

        /// <summary>
        /// Send Button input to VRChat.<para />
        /// TRUE for 'pressed' and FALSE for 'released'.<para />
        /// They will not function correctly without resetting to FALSE first - sending VRCButton.Jump TRUE and then VRCButton.Jump FALSE will only result in a single jump.
        /// </summary>
        /// <param name="input">A VRCButton property, for example: VRCButton.Jump</param>
        /// <param name="value">Value true/false</param>
        public void SendInput(VRCButton input, bool value) => SendTo(input.Value, value ? 1 : 0);

        /// <summary>
        /// Send input to VRChat.<para />
        /// Do not use full paths.
        /// </summary>
        /// <param name="input">A VRChat input name, for example: Jump</param>
        /// <param name="value">
        /// For Buttons: 0 or 1<para />
        /// For Axes: -1 or 1
        /// </param>
        public void SendInput(string input, object value) => SendTo(input_address + input, value);


        /// <summary>
        /// Sends a message to the VRChat Chatbox.<para />
        /// The string must be ASCII text, and is what will appear in the chatbox.
        /// </summary>
        /// <param name="text">ASCII string to send.</param>
        /// <param name="bypassKeyboard">If true, bypass the VRChat Keyboard and send the string directly to the chatbox.</param>
        /// <param name="messageComplete">Used to trigger chatbox notification SFX.</param>
        public void SendChatbox(string text, bool bypassKeyboard = false, bool messageComplete = true) => SendTo(chatbox_input_address, text, bypassKeyboard, messageComplete);

        /// <summary>Toggle the typing indicator on and off.</summary>
        /// <param name="typing">Indicator state.</param>
        public void SendChatbox(bool typing) => SendTo(chatbox_typing_address, typing);
        #endregion

        private void Serve()
        {
            byte[] buffer = new byte[m_BufferLength];
            if (m_Server == null) return;

            while (!m_Disposed)
            {
                try
                {
                    int receivedByteCount = m_Server.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    if (receivedByteCount == 0) continue;

                    if (OnMessage != null)
                    {
                        OscMessage parsed = OscMessage.Read(buffer, 0, receivedByteCount);
                        VRCMessage message = new VRCMessage(parsed);
                        if (LogMessages) message.Print();
                        OnMessage.Invoke(this, message);
                    }
                }
                catch (SocketException) { }
                catch (ThreadAbortException) { }
                catch (Exception e)
                {
                    if (!m_Disposed) Console.WriteLine(e);
                    break;
                }
            }
        }

        /// <summary>Release all resources used by this class.</summary>
        public void Dispose()
        {
            if (m_Disposed) return;
            m_Client?.Close();
            m_Server?.Close();
            m_Client?.Dispose();
            m_Server?.Dispose();
            m_Disposed = true;
            m_Started = false;
        }
    }
}
