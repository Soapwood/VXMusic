using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AiUnity.Common.LogUI.Scripts
{
    public enum ButtonState
    {
        /// <summary> Error logging level </summary>
        Disabled = 4,

        /// <summary> Fatal logging level </summary>
        Highlighted = 2,

        /// <summary> Assert logging level </summary>
        Pressed = 1,
    }

}
