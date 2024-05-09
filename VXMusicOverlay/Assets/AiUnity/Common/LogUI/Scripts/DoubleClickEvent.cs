// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 10-31-2017
// ***********************************************************************
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class DoubleClickEvent.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    /// <seealso cref="UnityEngine.EventSystems.IPointerClickHandler" />
    public class DoubleClickEvent : MonoBehaviour, IPointerClickHandler
    {
        #region Fields
        public UnityEvent doubleClickEvent;
        #endregion

        #region Methods
        /// <summary>
        /// Called when [pointer click].
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount > 1)
            {
                //this.doubleClickEvent.Invoke();
            }
        }
        #endregion
    }
}