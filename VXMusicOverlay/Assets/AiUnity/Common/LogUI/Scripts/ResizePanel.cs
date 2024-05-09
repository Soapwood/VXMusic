// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 05-06-2017
// ***********************************************************************
using UnityEngine;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class ResizePanel.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    /// <seealso cref="UnityEngine.EventSystems.IPointerDownHandler" />
    /// <seealso cref="UnityEngine.EventSystems.IDragHandler" />
    public class ResizePanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region Fields
        public Vector2 maxSize;
        public Vector2 minSize;
        public Texture2D cursorTexture;
        private Vector2 cursorHotspot;
        private Vector2 currentPointerPosition;
        private Vector2 previousPointerPosition;
        private RectTransform rectTransform;
        #endregion

        #region Methods
        /// <summary>
        /// Called when [drag].
        /// </summary>
        /// <param name="data">The data.</param>
        public void OnDrag(PointerEventData data)
        {
            if (this.rectTransform == null)
            {
                return;
            }

            Vector2 sizeDelta = this.rectTransform.sizeDelta;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, data.position, data.pressEventCamera, out this.currentPointerPosition);
            Vector2 resizeValue = this.currentPointerPosition - this.previousPointerPosition;

            sizeDelta += new Vector2(resizeValue.x, -resizeValue.y);
            sizeDelta = new Vector2(
                Mathf.Clamp(sizeDelta.x, this.minSize.x, this.maxSize.x),
                Mathf.Clamp(sizeDelta.y, this.minSize.y, this.maxSize.y)
                );

            this.rectTransform.sizeDelta = sizeDelta;

            this.previousPointerPosition = this.currentPointerPosition;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// Called when [pointer down].
        /// </summary>
        /// <param name="data">The data.</param>
        public void OnPointerDown(PointerEventData data)
        {
            this.rectTransform.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, data.position, data.pressEventCamera, out this.previousPointerPosition);
        }

        /// <summary>
        /// Awakes this instance.
        /// </summary>
        void Awake()
        {
            this.rectTransform = transform.parent.GetComponent<RectTransform>();
            cursorHotspot = new Vector2(cursorTexture.width / 4, cursorTexture.height / 4);
        }
        #endregion
    }
}