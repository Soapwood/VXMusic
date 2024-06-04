using System;
using System.Collections.Generic;
using System.Text;

namespace VRChatOSCLib
{
    /// <summary>Base interface for VRCInput.</summary>
    public interface IVRCInput
    {
        /// <summary>Get the raw string value from this Input.</summary>
        string Value { get; }
    }

    /// <summary>
    /// Axes expect a float from -1 to 1 to control things like movement.
    /// They expect to reset to 0 when not in use - otherwise a 'MoveForward' message left at '1' will continue to move you forward forever!
    /// </summary>
    public struct VRCAxes : IVRCInput
    {
        private VRCAxes(string value) => Value = value;
        /// <summary>Get the raw string value from this Input.</summary>
        public string Value { get; private set; }

        /// <summary>Move forwards (1) or Backwards (-1)</summary>
        public static VRCAxes Vertical => new VRCAxes("/input/Vertical");
        /// <summary>Move right (1) or left (-1)</summary>
        public static VRCAxes Horizontal => new VRCAxes("/input/Horizontal");
        /// <summary>Look Left and Right. Smooth in Desktop, VR will do a snap-turn when the value is 1 if Comfort Turning is on.</summary>
        public static VRCAxes LookHorizontal => new VRCAxes("/input/LookHorizontal");
        /// <summary>Look Up and Down.<summary>
        public static VRCAxes LookVertical => new VRCAxes("/input/LookVertical");
        /// <summary>Use held item - not sure if this works</summary>
        public static VRCAxes UseAxisRight => new VRCAxes("/input/UseAxisRight");
        /// <summary>Grab item - not sure if this works</summary>
        public static VRCAxes GrabAxisRight => new VRCAxes("/input/GrabAxisRight");
        /// <summary>Move a held object forwards (1) and backwards (-1)</summary>
        public static VRCAxes MoveHoldFB => new VRCAxes("/input/MoveHoldFB");
        /// <summary>Spin a held object Clockwise or Counter-Clockwise</summary>
        public static VRCAxes SpinHoldCwCcw => new VRCAxes("/input/SpinHoldCwCcw");
        /// <summary>Spin a held object Up or Down</summary>
        public static VRCAxes SpinHoldUD => new VRCAxes("/input/SpinHoldUD");
        /// <summary>Spin a held object Left or Right</summary>
        public static VRCAxes SpinHoldLR => new VRCAxes("/input/SpinHoldLR");
    }

    /// <summary>
    /// Buttons expect an int of 1 for 'pressed' and 0 for 'released'.
    /// They will not function correctly without resetting to 0 first - sending VRCButton.Jump 1 and then VRCButton.Jump 1 will only result in a single jump.
    /// </summary>
    public struct VRCButton : IVRCInput
    {
        private VRCButton(string value) => Value = value;
        /// <summary>Get the raw string value from this Input.</summary>
        public string Value { get; private set; }

        /// <summary>Move forward while this is 1.</summary>
        public static VRCButton MoveForward => new VRCButton("/input/MoveForward");
        /// <summary>Move backwards while this is 1.</summary>
        public static VRCButton MoveBackward => new VRCButton("/input/MoveBackward");
        /// <summary>Strafe left while this is 1.</summary>
        public static VRCButton MoveLeft => new VRCButton("/input/MoveLeft");
        /// <summary>Strafe right while this is 1.</summary>
        public static VRCButton MoveRight => new VRCButton("/input/MoveRight");
        /// <summary>
        /// Turn to the left while this is 1.
        /// Smooth in Desktop, VR will do a snap-turn if Comfort Turning is on.
        /// </summary>
        public static VRCButton LookLeft => new VRCButton("/input/LookLeft");
        /// <summary>
        /// Turn to the right while this is 1.
        /// Smooth in Desktop, VR will do a snap-turn if Comfort Turning is on.
        /// </summary>
        public static VRCButton LookRight => new VRCButton("/input/LookRight");
        /// <summary>Jump if the world supports it.</summary>
        public static VRCButton Jump => new VRCButton("/input/Jump");
        /// <summary>Walk faster if the world supports it.</summary>
        public static VRCButton Run => new VRCButton("/input/Run");
        /// <summary>Snap-Turn to the left - VR Only.</summary>
        public static VRCButton ComfortLeft => new VRCButton("/input/ComfortLeft");
        /// <summary>Snap-Turn to the right - VR Only.</summary>
        public static VRCButton ComfortRight => new VRCButton("/input/ComfortRight");
        /// <summary>Drop the item held in your right hand - VR Only.</summary>
        public static VRCButton DropRight => new VRCButton("/input/DropRight");
        /// <summary>Use the item highlighted by your right hand - VR Only.</summary>
        public static VRCButton UseRight => new VRCButton("/input/UseRight");
        /// <summary>Grab the item highlighted by your right hand - VR Only.</summary>
        public static VRCButton GrabRight => new VRCButton("/input/GrabRight");
        /// <summary>Drop the item held in your left hand - VR Only.</summary>
        public static VRCButton DropLeft => new VRCButton("/input/DropLeft");
        /// <summary>Use the item highlighted by your left hand - VR Only.</summary>
        public static VRCButton UseLeft => new VRCButton("/input/UseLeft");
        /// <summary>Grab the item highlighted by your left hand - VR Only.</summary>
        public static VRCButton GrabLeft => new VRCButton("/input/GrabLeft");
        /// <summary>Turn on Safe Mode.</summary>
        public static VRCButton PanicButton => new VRCButton("/input/PanicButton");
        /// <summary>
        /// Toggle QuickMenu On/Off.
        /// Will toggle upon receiving '1' if it's currently '0'.
        /// </summary>
        public static VRCButton QuickMenuToggleLeft => new VRCButton("/input/QuickMenuToggleLeft");
        /// <summary>
        /// Toggle QuickMenu On/Off.
        /// Will toggle upon receiving '1' if it's currently '0'.
        /// </summary>
        public static VRCButton QuickMenuToggleRight => new VRCButton("/input/QuickMenuToggleRight");
        /// <summary>
        /// Toggle Voice - the action will depend on whether "Toggle Voice" is turned on in your Settings.
        /// If so, then changing from 0 to 1 will toggle the state of mute.
        /// If "Toggle Voice" is turned off, then it functions like Push-To-Mute - 1 is muted, 0 is unmuted.
        /// </summary>
        public static VRCButton Voice => new VRCButton("/input/Voice");
    }
}
