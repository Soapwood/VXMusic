using OscCore;
using System.Net.Sockets;

namespace VRChatOSCLib
{
    public partial class VRChatOSC
    {
        /// <summary>Sends a raw OSC Packet.</summary>
        /// <param name="packet">Raw OSC Packet.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="Exception">Object no started.</exception>
        public async Task SendAsync(OscPacket packet)
        {
            if (m_Disposed) throw new ObjectDisposedException("This object has been disposed.");
            if (!m_Started) throw new Exception("Trying to send data on an object that has not been started.\n\nPlase call <VRChatOSC>.Connect() after contructor.");

            var data = packet.ToByteArray();
            _ = await m_Client.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
        }

        /// <summary>Send an array of objects to an address.</summary>
        /// <param name="address">Address</param>
        /// <param name="args">Object Parameters</param>
        public async Task SendToAsync(string address, params object[] args) => await SendAsync(new OscMessage(address, args));

        /// <summary>Sends an Integer value to an Avatar parameter.</summary>
        /// <param name="name">The name of the Avatar parameter.</param>
        /// <param name="value">The value sent to this parameter.</param>
        public async Task SendParameterAsync(string name, int value) => await SendToAsync($"{avatar_parameters_address}/{name}", value);

        /// <summary>Sends an Float value to an Avatar parameter.</summary>
        /// <param name="name">The name of the Avatar parameter.</param>
        /// <param name="value">The value sent to this parameter.</param>
        public async Task SendParameterAsync(string name, float value) => await SendToAsync($"{avatar_parameters_address}/{name}", value);

        /// <summary>Sends an Boolean value to an Avatar parameter.</summary>
        /// <param name="name">The name of the Avatar parameter.</param>
        /// <param name="value">The value sent to this parameter.</param>
        public async Task SendParameterAsync(string name, bool value) => await SendToAsync($"{avatar_parameters_address}/{name}", value);



        /// <summary>Send inputs to VRChat and control things such as locomotion.</summary>
        /// <param name="input">Any VRCInput that inherits the IVRCInput interface.</param>
        /// <param name="value">
        /// A float (-1f to 1f) for Axis.<para />
        /// An int (0, 1) for buttons.
        /// </param>
        public async Task SendInputAsync(IVRCInput input, object value) => await SendToAsync(input.Value, value);

        /// <summary>
        /// Send Axes input to VRChat.<para />
        /// Axes expect a float from -1 to 1 to control things like movement.<para />
        /// They expect to reset to 0 when not in use - otherwise a 'MoveForward' message left at '1' will continue to move you forward forever!
        /// </summary>
        /// <param name="input">A VRCAxes property, for example: VRCAxes.Vertical</param>
        /// <param name="value">Value from -1f to 1f</param>
        public async Task SendInputAsync(VRCAxes input, float value) => await SendToAsync(input.Value, value);

        /// <summary>
        /// Send Button input to VRChat.<para />
        /// TRUE for 'pressed' and FALSE for 'released'.<para />
        /// They will not function correctly without resetting to FALSE first - sending VRCButton.Jump TRUE and then VRCButton.Jump FALSE will only result in a single jump.
        /// </summary>
        /// <param name="input">A VRCButton property, for example: VRCButton.Jump</param>
        /// <param name="value">Value true/false</param>
        public async Task SendInputAsync(VRCButton input, bool value) => await SendToAsync(input.Value, value ? 1 : 0);

        /// <summary>
        /// Send input to VRChat.<para />
        /// Do not use full paths.
        /// </summary>
        /// <param name="input">A VRChat input name, for example: Jump</param>
        /// <param name="value">
        /// For Buttons: 0 or 1<para />
        /// For Axes: -1 or 1
        /// </param>
        public async Task SendInputAsync(string input, object value) => await SendToAsync(input_address + input, value);


        /// <summary>
        /// Sends a message to the VRChat Chatbox.<para />
        /// The string must be ASCII text, and is what will appear in the chatbox.
        /// </summary>
        /// <param name="text">ASCII string to send.</param>
        /// <param name="bypassKeyboard">If true, bypass the VRChat Keyboard and send the string directly to the chatbox.</param>
        /// <param name="messageComplete">Used to trigger chatbox notification SFX.</param>
        public async Task SendChatboxAsync(string text, bool bypassKeyboard = true, bool messageComplete = true) => await SendToAsync(chatbox_input_address, text, bypassKeyboard, messageComplete);

        /// <summary>Toggle the typing indicator on and off.</summary>
        /// <param name="typing">Indicator state.</param>
        public async Task SendChatboxAsync(bool typing) => await SendToAsync(chatbox_typing_address, typing);
    }
}
