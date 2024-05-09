using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts
{
    public interface IButtonStateChange : IEventSystemHandler
    {
        // functions that can be called via the messaging system
        void ButtonStateChange(ButtonState buttonState);
    }
}