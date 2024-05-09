// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 11-02-2017
// ***********************************************************************
using UnityEngine;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class DragPanel.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    /// <seealso cref="UnityEngine.EventSystems.IPointerDownHandler" />
    /// <seealso cref="UnityEngine.EventSystems.IDragHandler" />
    public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        #region Fields
        public RectTransform panelRectTransform;
        private RectTransform canvasRectTransform;
        private Vector2 pointerOffset;
        #endregion

        #region Methods
        /// <summary>
        /// Called when [drag].
        /// </summary>
        /// <param name="data">The data.</param>
        public void OnDrag(PointerEventData data)
        {
            if (enabled)
            {
                if (this.panelRectTransform == null)
                {
                    return;
                }

                Vector2 pointerPosition = ClampToWindow(data);

                Vector2 localPointerPosition;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    this.canvasRectTransform, pointerPosition, data.pressEventCamera, out localPointerPosition
                ))
                {
                    this.panelRectTransform.localPosition = localPointerPosition - this.pointerOffset;
                }
            }
        }

        /// <summary>
        /// Called when [pointer down].
        /// </summary>
        /// <param name="data">The data.</param>
        public void OnPointerDown(PointerEventData data)
        {
            if (enabled)
            {
                this.panelRectTransform.SetAsLastSibling();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(this.panelRectTransform, data.position, data.pressEventCamera, out this.pointerOffset);
            }
        }

        /// <summary>
        /// Awakes this instance.
        /// </summary>
        void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                this.canvasRectTransform = canvas.transform as RectTransform;
                //panelRectTransform = transform.parent.parent as RectTransform;
            }
        }

        /// <summary>
        /// Clamps to window.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Vector2.</returns>
        Vector2 ClampToWindow(PointerEventData data)
        {
            Vector2 rawPointerPosition = data.position;

            Vector3[] canvasCorners = new Vector3[4];
            this.canvasRectTransform.GetWorldCorners(canvasCorners);

            float clampedX = Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x);
            float clampedY = Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y);

            Vector2 newPointerPosition = new Vector2(clampedX, clampedY);
            return newPointerPosition;
        }
        #endregion
    }
}