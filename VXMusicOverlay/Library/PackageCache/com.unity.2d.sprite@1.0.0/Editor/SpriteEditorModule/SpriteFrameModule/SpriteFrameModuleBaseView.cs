using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace UnityEditor.U2D.Sprites
{
    internal abstract partial class SpriteFrameModuleBase : SpriteEditorModuleBase
    {
        protected enum GizmoMode
        {
            BorderEditing,
            RectEditing
        }

        protected class Styles
        {
            public readonly GUIStyle dragdot = "U2D.dragDot";
            public readonly GUIStyle dragdotactive = "U2D.dragDotActive";
            public readonly GUIStyle createRect = "U2D.createRect";
            public readonly GUIStyle pivotdotactive = "U2D.pivotDotActive";
            public readonly GUIStyle pivotdot = "U2D.pivotDot";

            public readonly GUIStyle dragBorderdot = new GUIStyle();
            public readonly GUIStyle dragBorderDotActive = new GUIStyle();

            public readonly GUIStyle toolbar;

            public Styles()
            {
                toolbar = new GUIStyle(EditorStyles.inspectorBig);
                toolbar.margin.top = 0;
                toolbar.margin.bottom = 0;
                createRect.border = new RectOffset(3, 3, 3, 3);

                dragBorderdot.fixedHeight = 5f;
                dragBorderdot.fixedWidth = 5f;
                dragBorderdot.normal.background = EditorGUIUtility.whiteTexture;

                dragBorderDotActive.fixedHeight = dragBorderdot.fixedHeight;
                dragBorderDotActive.fixedWidth = dragBorderdot.fixedWidth;
                dragBorderDotActive.normal.background = EditorGUIUtility.whiteTexture;
            }
        }

        private static Styles s_Styles;

        protected static Styles styles
        {
            get
            {
                if (s_Styles == null)
                    s_Styles = new Styles();
                return s_Styles;
            }
        }

        private const float kInspectorWidth = 330f;
        private const float kInspectorHeight = 170;
        private const float kPivotFieldPrecision = 0.0001f;
        private float m_Zoom = 1.0f;
        private GizmoMode m_GizmoMode;

        private VisualElement m_NameElement;
        private TextField m_NameField;
        private VisualElement m_PositionElement;
        private IntegerField m_PositionFieldX;
        private IntegerField m_PositionFieldY;
        private IntegerField m_PositionFieldW;
        private IntegerField m_PositionFieldH;
        private IntegerField m_BorderFieldL;
        private IntegerField m_BorderFieldT;
        private IntegerField m_BorderFieldR;
        private IntegerField m_BorderFieldB;
        private EnumField m_PivotField;
        private EnumField m_PivotUnitModeField;
        private VisualElement m_CustomPivotElement;
        private FloatField m_CustomPivotFieldX;
        private FloatField m_CustomPivotFieldY;
        private VisualElement m_SelectedFrameInspector;

        private bool ShouldShowRectScaling()
        {
            return hasSelected && m_GizmoMode == GizmoMode.RectEditing;
        }

        private static Rect inspectorRect
        {
            get
            {
                return new Rect(
                    0, 0,
                    kInspectorWidth,
                    kInspectorHeight);
            }
        }

        private void RemoveMainUI(VisualElement mainView)
        {
            if (mainView.Contains(m_SelectedFrameInspector))
                mainView.Remove(m_SelectedFrameInspector);
            mainView.UnregisterCallback<SpriteSelectionChangeEvent>(SelectionChange);
        }

        protected void UpdatePositionField(FocusOutEvent evt)
        {
            if (hasSelected)
            {
                m_PositionFieldX.SetValueWithoutNotify((int)selectedSpriteRect.x);
                m_PositionFieldY.SetValueWithoutNotify((int)selectedSpriteRect.y);
                m_PositionFieldW.SetValueWithoutNotify((int)selectedSpriteRect.width);
                m_PositionFieldH.SetValueWithoutNotify((int)selectedSpriteRect.height);
            }
        }

        private void UpdateBorderField(FocusOutEvent evt)
        {
            if (hasSelected)
            {
                m_BorderFieldL.SetValueWithoutNotify((int)selectedSpriteBorder.x);
                m_BorderFieldB.SetValueWithoutNotify((int)selectedSpriteBorder.y);
                m_BorderFieldR.SetValueWithoutNotify((int)selectedSpriteBorder.z);
                m_BorderFieldT.SetValueWithoutNotify((int)selectedSpriteBorder.w);
            }
        }

        void SetupIntegerField(IntegerField field, EventCallback<FocusOutEvent> onFocusOutEvent, EventCallback<ChangeEvent<int>> onChangeEvent)
        {
            field.RegisterCallback(onFocusOutEvent);
            field.RegisterValueChangedCallback(onChangeEvent);
        }

        void SetDragFieldLimit(IntegerField field, int value)
        {
            // The only way to know if value change is due to dragger or text input
            var t = field.Q("unity-text-input");
            if (!t.focusController.IsFocused(t))
            {
                // Value changed due to drag. We set back the field so to show the drag limit
                field.SetValueWithoutNotify(value);
            }
        }

        void OnPositionIntXChange(ChangeEvent<int> evt)
        {
            if (hasSelected)
            {
                var rect = selectedSpriteRect;
                rect.x = evt.newValue;
                selectedSpriteRect = rect;
                SetDragFieldLimit(m_PositionFieldX, (int)selectedSpriteRect.x);
                m_PositionFieldW.SetValueWithoutNotify((int)selectedSpriteRect.width);
            }
        }

        void OnPositionIntYChange(ChangeEvent<int> evt)
        {
            if (hasSelected)
            {
                var rect = selectedSpriteRect;
                rect.y = evt.newValue;
                selectedSpriteRect = rect;
                SetDragFieldLimit(m_PositionFieldY, (int)selectedSpriteRect.y);
                m_PositionFieldH.SetValueWithoutNotify((int)selectedSpriteRect.height);
            }
        }

        void OnPositionIntWChange(ChangeEvent<int> evt)
        {
            if (hasSelected)
            {
                var rect = selectedSpriteRect;
                rect.width = evt.newValue;
                selectedSpriteRect = rect;
                SetDragFieldLimit(m_PositionFieldW, (int)selectedSpriteRect.width);
                m_PositionFieldX.SetValueWithoutNotify((int)selectedSpriteRect.x);
            }
        }

        void OnPositionIntHChange(ChangeEvent<int> evt)
        {
            if (hasSelected)
            {
                var rect = selectedSpriteRect;
                rect.height = evt.newValue;
                selectedSpriteRect = rect;
                SetDragFieldLimit(m_PositionFieldH, (int)selectedSpriteRect.height);
                m_PositionFieldY.SetValueWithoutNotify((int)selectedSpriteRect.y);
            }
        }

        void OnBorderIntLChange(ChangeEvent<int> evt)
        {
            if (hasSelected)
            {
                var border = selectedSpriteBorder;
                border.x = evt.newValue;
                selectedSpriteBorder = border;
                SetDragFieldLimit(m_BorderFieldL, (int)selectedSpriteBorder.x);
            }
        }

        void OnBorderIntBChange(ChangeEvent<int> evt)
        {
            if (hasSelected)
            {
                var border = selectedSpriteBorder;
                border.y = evt.newValue;
                selectedSpriteBorder = border;
                SetDragFieldLimit(m_BorderFieldB, (int)selectedSpriteBorder.y);
            }
        }

        void OnBorderIntRChange(ChangeEvent<int> evt)
        {
            if (hasSelected)
            {
                var border = selectedSpriteBorder;
                border.z = (evt.newValue + border.x) <= selectedSpriteRect.width ? evt.newValue : selectedSpriteRect.width - border.x;
                selectedSpriteBorder = border;
                SetDragFieldLimit(m_BorderFieldR, (int)selectedSpriteBorder.z);
            }
        }

        void OnBorderIntTChange(ChangeEvent<int> evt)
        {
            if (hasSelected)
            {
                var border = selectedSpriteBorder;
                border.w = (evt.newValue + border.y) <= selectedSpriteRect.height ? evt.newValue : selectedSpriteRect.height - border.y;
                selectedSpriteBorder = border;
                SetDragFieldLimit(m_BorderFieldT, (int)selectedSpriteBorder.w);
            }
        }

        private void AddMainUI(VisualElement mainView)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.2d.sprite/Editor/UI/SpriteEditor/SpriteFrameModuleInspector.uxml") as VisualTreeAsset;
            m_SelectedFrameInspector = visualTree.CloneTree().Q("spriteFrameModuleInspector");

            m_NameElement = m_SelectedFrameInspector.Q("name");
            m_NameField = m_SelectedFrameInspector.Q<TextField>("spriteName");
            m_NameField.RegisterValueChangedCallback((evt) =>
            {
                if (hasSelected)
                {
                    selectedSpriteName = evt.newValue;
                }
            });

            m_NameField.RegisterCallback<FocusOutEvent>((focus) =>
            {
                if (hasSelected)
                {
                    m_NameField.SetValueWithoutNotify(selectedSpriteName);
                }
            });


            m_PositionElement = m_SelectedFrameInspector.Q("position");
            m_PositionFieldX = m_PositionElement.Q<IntegerField>("positionX");
            SetupIntegerField(m_PositionFieldX, UpdatePositionField, OnPositionIntXChange);

            m_PositionFieldY = m_PositionElement.Q<IntegerField>("positionY");
            SetupIntegerField(m_PositionFieldY, UpdatePositionField, OnPositionIntYChange);

            m_PositionFieldW = m_PositionElement.Q<IntegerField>("positionW");
            SetupIntegerField(m_PositionFieldW,  UpdatePositionField, OnPositionIntWChange);

            m_PositionFieldH = m_PositionElement.Q<IntegerField>("positionH");
            SetupIntegerField(m_PositionFieldH, UpdatePositionField, OnPositionIntHChange);

            var borderElement = m_SelectedFrameInspector.Q("border");
            m_BorderFieldL = borderElement.Q<IntegerField>("borderL");
            SetupIntegerField(m_BorderFieldL, UpdateBorderField, OnBorderIntLChange);

            m_BorderFieldT = borderElement.Q<IntegerField>("borderT");
            SetupIntegerField(m_BorderFieldT, UpdateBorderField, OnBorderIntTChange);

            m_BorderFieldR = borderElement.Q<IntegerField>("borderR");
            SetupIntegerField(m_BorderFieldR, UpdateBorderField, OnBorderIntRChange);

            m_BorderFieldB = borderElement.Q<IntegerField>("borderB");
            SetupIntegerField(m_BorderFieldB, UpdateBorderField, OnBorderIntBChange);

            m_PivotField = m_SelectedFrameInspector.Q<EnumField>("pivotField");
            m_PivotField.Init(SpriteAlignment.Center);
            m_PivotField.label = L10n.Tr("Pivot");
            m_PivotField.RegisterValueChangedCallback((evt) =>
            {
                if (hasSelected)
                {
                    SpriteAlignment alignment = (SpriteAlignment)evt.newValue;
                    SetSpritePivotAndAlignment(selectedSpritePivot, alignment);
                    m_CustomPivotElement.SetEnabled(selectedSpriteAlignment == SpriteAlignment.Custom);
                    Vector2 pivot = selectedSpritePivotInCurUnitMode;
                    m_CustomPivotFieldX.SetValueWithoutNotify(pivot.x);
                    m_CustomPivotFieldY.SetValueWithoutNotify(pivot.y);
                }
            });


            m_PivotUnitModeField = m_SelectedFrameInspector.Q<EnumField>("pivotUnitModeField");
            m_PivotUnitModeField.Init(PivotUnitMode.Normalized);
            m_PivotUnitModeField.label = L10n.Tr("Pivot Unit Mode");
            m_PivotUnitModeField.RegisterValueChangedCallback((evt) =>
            {
                if (hasSelected)
                {
                    m_PivotUnitMode = (PivotUnitMode)evt.newValue;

                    Vector2 pivot = selectedSpritePivotInCurUnitMode;
                    m_CustomPivotFieldX.SetValueWithoutNotify(pivot.x);
                    m_CustomPivotFieldY.SetValueWithoutNotify(pivot.y);
                }
            });


            m_CustomPivotElement = m_SelectedFrameInspector.Q("customPivot");
            m_CustomPivotFieldX = m_CustomPivotElement.Q<FloatField>("customPivotX");
            m_CustomPivotFieldX.RegisterValueChangedCallback((evt) =>
            {
                if (hasSelected)
                {
                    float newValue = (float)evt.newValue;
                    float pivotX = m_PivotUnitMode == PivotUnitMode.Pixels
                        ? ConvertFromRectToNormalizedSpace(new Vector2(newValue, 0.0f), selectedSpriteRect).x
                        : newValue;

                    var pivot = selectedSpritePivot;
                    pivot.x = pivotX;
                    SetSpritePivotAndAlignment(pivot, selectedSpriteAlignment);
                }
            });

            m_CustomPivotFieldY = m_CustomPivotElement.Q<FloatField>("customPivotY");
            m_CustomPivotFieldY.RegisterValueChangedCallback((evt) =>
            {
                if (hasSelected)
                {
                    float newValue = (float)evt.newValue;
                    float pivotY = m_PivotUnitMode == PivotUnitMode.Pixels
                        ? ConvertFromRectToNormalizedSpace(new Vector2(0.0f, newValue), selectedSpriteRect).y
                        : newValue;

                    var pivot = selectedSpritePivot;
                    pivot.y = pivotY;
                    SetSpritePivotAndAlignment(pivot, selectedSpriteAlignment);
                }
            });

            //// Force an update of all the fields.
            PopulateSpriteFrameInspectorField();

            mainView.RegisterCallback<SpriteSelectionChangeEvent>(SelectionChange);

            // Stop mouse events from reaching the main view.
            m_SelectedFrameInspector.pickingMode = PickingMode.Ignore;
            m_SelectedFrameInspector.RegisterCallback<MouseDownEvent>((e) => { e.StopPropagation(); });
            m_SelectedFrameInspector.RegisterCallback<MouseUpEvent>((e) => { e.StopPropagation(); });
            m_SelectedFrameInspector.AddToClassList("moduleWindow");
            m_SelectedFrameInspector.AddToClassList("bottomRightFloating");
            mainView.Add(m_SelectedFrameInspector);
        }

        private void SelectionChange(SpriteSelectionChangeEvent evt)
        {
            m_SelectedFrameInspector.style.display = hasSelected ? DisplayStyle.Flex : DisplayStyle.None;
            PopulateSpriteFrameInspectorField();
        }

        private void UIUndoCallback()
        {
            PopulateSpriteFrameInspectorField();
        }

        protected void PopulateSpriteFrameInspectorField()
        {
            m_SelectedFrameInspector.style.display = hasSelected ?  DisplayStyle.Flex : DisplayStyle.None;
            if (!hasSelected)
                return;
            m_NameElement.SetEnabled(containsMultipleSprites);
            m_NameField.SetValueWithoutNotify(selectedSpriteName);
            m_PositionElement.SetEnabled(containsMultipleSprites);
            var spriteRect = selectedSpriteRect;
            m_PositionFieldX.SetValueWithoutNotify(Mathf.RoundToInt(spriteRect.x));
            m_PositionFieldY.SetValueWithoutNotify(Mathf.RoundToInt(spriteRect.y));
            m_PositionFieldW.SetValueWithoutNotify(Mathf.RoundToInt(spriteRect.width));
            m_PositionFieldH.SetValueWithoutNotify(Mathf.RoundToInt(spriteRect.height));
            var spriteBorder = selectedSpriteBorder;
            m_BorderFieldL.SetValueWithoutNotify(Mathf.RoundToInt(spriteBorder.x));
            m_BorderFieldT.SetValueWithoutNotify(Mathf.RoundToInt(spriteBorder.w));
            m_BorderFieldR.SetValueWithoutNotify(Mathf.RoundToInt(spriteBorder.z));
            m_BorderFieldB.SetValueWithoutNotify(Mathf.RoundToInt(spriteBorder.y));
            m_PivotField.SetValueWithoutNotify(selectedSpriteAlignment);
            m_PivotUnitModeField.SetValueWithoutNotify(m_PivotUnitMode);
            Vector2 pivot = selectedSpritePivotInCurUnitMode;
            m_CustomPivotFieldX.SetValueWithoutNotify(pivot.x);
            m_CustomPivotFieldY.SetValueWithoutNotify(pivot.y);

            m_CustomPivotElement.SetEnabled(hasSelected && selectedSpriteAlignment == SpriteAlignment.Custom);
        }

        private static Vector2 ApplySpriteAlignmentToPivot(Vector2 pivot, Rect rect, SpriteAlignment alignment)
        {
            if (alignment != SpriteAlignment.Custom)
            {
                Vector2[] snapPoints = GetSnapPointsArray(rect);
                Vector2 texturePos = snapPoints[(int)alignment];
                return ConvertFromTextureToNormalizedSpace(texturePos, rect);
            }
            return pivot;
        }

        private static Vector2 ConvertFromTextureToNormalizedSpace(Vector2 texturePos, Rect rect)
        {
            return new Vector2((texturePos.x - rect.xMin) / rect.width, (texturePos.y - rect.yMin) / rect.height);
        }

        private static Vector2 ConvertFromNormalizedToRectSpace(Vector2 normalizedPos, Rect rect)
        {
            Vector2 rectPos = new Vector2(rect.width * normalizedPos.x, rect.height * normalizedPos.y);

            // This is to combat the lack of precision formating on the UI controls.
            rectPos.x = Mathf.Round(rectPos.x / kPivotFieldPrecision) * kPivotFieldPrecision;
            rectPos.y = Mathf.Round(rectPos.y / kPivotFieldPrecision) * kPivotFieldPrecision;

            return rectPos;
        }

        private static Vector2 ConvertFromRectToNormalizedSpace(Vector2 rectPos, Rect rect)
        {
            return new Vector2(rectPos.x / rect.width, rectPos.y / rect.height);
        }

        private static Vector2[] GetSnapPointsArray(Rect rect)
        {
            Vector2[] snapPoints = new Vector2[9];
            snapPoints[(int)SpriteAlignment.TopLeft] = new Vector2(rect.xMin, rect.yMax);
            snapPoints[(int)SpriteAlignment.TopCenter] = new Vector2(rect.center.x, rect.yMax);
            snapPoints[(int)SpriteAlignment.TopRight] = new Vector2(rect.xMax, rect.yMax);
            snapPoints[(int)SpriteAlignment.LeftCenter] = new Vector2(rect.xMin, rect.center.y);
            snapPoints[(int)SpriteAlignment.Center] = new Vector2(rect.center.x, rect.center.y);
            snapPoints[(int)SpriteAlignment.RightCenter] = new Vector2(rect.xMax, rect.center.y);
            snapPoints[(int)SpriteAlignment.BottomLeft] = new Vector2(rect.xMin, rect.yMin);
            snapPoints[(int)SpriteAlignment.BottomCenter] = new Vector2(rect.center.x, rect.yMin);
            snapPoints[(int)SpriteAlignment.BottomRight] = new Vector2(rect.xMax, rect.yMin);
            return snapPoints;
        }

        protected void Repaint()
        {
            spriteEditor.RequestRepaint();
        }

        protected void HandleGizmoMode()
        {
            GizmoMode oldGizmoMode = m_GizmoMode;
            var evt = eventSystem.current;
            if (evt.control)
                m_GizmoMode = GizmoMode.BorderEditing;
            else
                m_GizmoMode = GizmoMode.RectEditing;

            if (oldGizmoMode != m_GizmoMode && (evt.type == EventType.KeyDown || evt.type == EventType.KeyUp) && (evt.keyCode == KeyCode.LeftControl || evt.keyCode == KeyCode.RightControl || evt.keyCode == KeyCode.LeftAlt || evt.keyCode == KeyCode.RightAlt))
                Repaint();
        }

        protected bool MouseOnTopOfInspector()
        {
            if (hasSelected == false)
                return false;

            var point = GUIClip.Unclip(eventSystem.current.mousePosition);
            point = m_SelectedFrameInspector.parent.LocalToWorld(point);

            var selectedElement = m_SelectedFrameInspector.panel.Pick(point);
            if (selectedElement != null
                && selectedElement.pickingMode != PickingMode.Ignore
                && selectedElement.FindCommonAncestor(m_SelectedFrameInspector) == m_SelectedFrameInspector)
                return true;

            return false;
        }

        protected void HandlePivotHandle()
        {
            if (!hasSelected)
                return;

            EditorGUI.BeginChangeCheck();

            SpriteAlignment alignment = selectedSpriteAlignment;
            Vector2 pivot = selectedSpritePivot;
            Rect rect = selectedSpriteRect;
            pivot = ApplySpriteAlignmentToPivot(pivot, rect, alignment);
            Vector2 pivotHandlePosition = SpriteEditorHandles.PivotSlider(rect, pivot, styles.pivotdot, styles.pivotdotactive);

            if (EditorGUI.EndChangeCheck())
            {
                // Pivot snapping only happen when ctrl is press. Same as scene view snapping move
                if (eventSystem.current.control)
                    SnapPivotToSnapPoints(pivotHandlePosition, out pivot, out alignment);
                else if (m_PivotUnitMode == PivotUnitMode.Pixels)
                    SnapPivotToPixels(pivotHandlePosition, out pivot, out alignment);
                else
                {
                    pivot = pivotHandlePosition;
                    alignment = SpriteAlignment.Custom;
                }
                SetSpritePivotAndAlignment(pivot, alignment);
                PopulateSpriteFrameInspectorField();
            }
        }

        protected void HandleBorderSidePointScalingSliders()
        {
            if (!hasSelected)
                return;

            GUIStyle dragDot = styles.dragBorderdot;
            GUIStyle dragDotActive = styles.dragBorderDotActive;
            var color = new Color(0f, 1f, 0f);

            Rect rect = selectedSpriteRect;
            Vector4 border = selectedSpriteBorder;

            float left = rect.xMin + border.x;
            float right = rect.xMax - border.z;
            float top = rect.yMax - border.w;
            float bottom = rect.yMin + border.y;

            EditorGUI.BeginChangeCheck();

            float horizontal = bottom - (bottom - top) / 2;
            float vertical = left - (left - right) / 2;

            float center = horizontal;
            HandleBorderPointSlider(ref left, ref center, MouseCursor.ResizeHorizontal, false, dragDot, dragDotActive, color);

            center = horizontal;
            HandleBorderPointSlider(ref right, ref center, MouseCursor.ResizeHorizontal, false, dragDot, dragDotActive, color);

            center = vertical;
            HandleBorderPointSlider(ref center, ref top, MouseCursor.ResizeVertical, false, dragDot, dragDotActive, color);

            center = vertical;
            HandleBorderPointSlider(ref center, ref bottom, MouseCursor.ResizeVertical, false, dragDot, dragDotActive, color);

            if (EditorGUI.EndChangeCheck())
            {
                border.x = left - rect.xMin;
                border.z = rect.xMax - right;
                border.w = rect.yMax - top;
                border.y = bottom - rect.yMin;
                selectedSpriteBorder = border;
                PopulateSpriteFrameInspectorField();
            }
        }

        protected void HandleBorderCornerScalingHandles()
        {
            if (!hasSelected)
                return;

            GUIStyle dragDot = styles.dragBorderdot;
            GUIStyle dragDotActive = styles.dragBorderDotActive;
            var color = new Color(0f, 1f, 0f);

            Rect rect = selectedSpriteRect;
            Vector4 border = selectedSpriteBorder;

            float left = rect.xMin + border.x;
            float right = rect.xMax - border.z;
            float top = rect.yMax - border.w;
            float bottom = rect.yMin + border.y;

            EditorGUI.BeginChangeCheck();

            // Handle corner points, but hide them if border values are below 1
            HandleBorderPointSlider(ref left, ref top, MouseCursor.ResizeUpLeft, border.x < 1 && border.w < 1, dragDot, dragDotActive, color);
            HandleBorderPointSlider(ref right, ref top, MouseCursor.ResizeUpRight, border.z < 1 && border.w < 1, dragDot, dragDotActive, color);
            HandleBorderPointSlider(ref left, ref bottom, MouseCursor.ResizeUpRight, border.x < 1 && border.y < 1, dragDot, dragDotActive, color);
            HandleBorderPointSlider(ref right, ref bottom, MouseCursor.ResizeUpLeft, border.z < 1 && border.y < 1, dragDot, dragDotActive, color);

            if (EditorGUI.EndChangeCheck())
            {
                border.x = left - rect.xMin;
                border.z = rect.xMax - right;
                border.w = rect.yMax - top;
                border.y = bottom - rect.yMin;
                selectedSpriteBorder = border;
                PopulateSpriteFrameInspectorField();
            }
        }

        protected void HandleBorderSideScalingHandles()
        {
            if (hasSelected == false)
                return;

            Rect rect = new Rect(selectedSpriteRect);
            Vector4 border = selectedSpriteBorder;

            float left = rect.xMin + border.x;
            float right = rect.xMax - border.z;
            float top = rect.yMax - border.w;
            float bottom = rect.yMin + border.y;

            Vector2 screenRectTopLeft = Handles.matrix.MultiplyPoint(new Vector3(rect.xMin, rect.yMin));
            Vector2 screenRectBottomRight = Handles.matrix.MultiplyPoint(new Vector3(rect.xMax, rect.yMax));

            float screenRectWidth = Mathf.Abs(screenRectBottomRight.x - screenRectTopLeft.x);
            float screenRectHeight = Mathf.Abs(screenRectBottomRight.y - screenRectTopLeft.y);

            EditorGUI.BeginChangeCheck();

            left = HandleBorderScaleSlider(left, rect.yMax, screenRectWidth, screenRectHeight, true);
            right = HandleBorderScaleSlider(right, rect.yMax, screenRectWidth, screenRectHeight, true);

            top = HandleBorderScaleSlider(rect.xMin, top, screenRectWidth, screenRectHeight, false);
            bottom = HandleBorderScaleSlider(rect.xMin, bottom, screenRectWidth, screenRectHeight, false);

            if (EditorGUI.EndChangeCheck())
            {
                border.x = left - rect.xMin;
                border.z = rect.xMax - right;
                border.w = rect.yMax - top;
                border.y = bottom - rect.yMin;

                selectedSpriteBorder = border;
                PopulateSpriteFrameInspectorField();
            }
        }

        protected void HandleBorderPointSlider(ref float x, ref float y, MouseCursor mouseCursor, bool isHidden, GUIStyle dragDot, GUIStyle dragDotActive, Color color)
        {
            var originalColor = GUI.color;

            if (isHidden)
                GUI.color = new Color(0, 0, 0, 0);
            else
                GUI.color = color;

            Vector2 point = SpriteEditorHandles.PointSlider(new Vector2(x, y), mouseCursor, dragDot, dragDotActive);
            x = point.x;
            y = point.y;

            GUI.color = originalColor;
        }

        protected float HandleBorderScaleSlider(float x, float y, float width, float height, bool isHorizontal)
        {
            float handleSize = styles.dragBorderdot.fixedWidth;
            Vector2 point = Handles.matrix.MultiplyPoint(new Vector2(x, y));
            float result;

            EditorGUI.BeginChangeCheck();

            if (isHorizontal)
            {
                Rect newRect = new Rect(point.x - handleSize * .5f, point.y, handleSize, height);
                result = SpriteEditorHandles.ScaleSlider(point, MouseCursor.ResizeHorizontal, newRect).x;
            }
            else
            {
                Rect newRect = new Rect(point.x, point.y - handleSize * .5f, width, handleSize);
                result = SpriteEditorHandles.ScaleSlider(point, MouseCursor.ResizeVertical, newRect).y;
            }

            if (EditorGUI.EndChangeCheck())
                return result;

            return isHorizontal ? x : y;
        }

        protected void DrawSpriteRectGizmos()
        {
            if (eventSystem.current.type != EventType.Repaint)
                return;

            SpriteEditorUtility.BeginLines(new Color(0f, 1f, 0f, 0.7f));
            var selectedGUID = selected != null ? selected.spriteID : new GUID();
            for (int i = 0; i < spriteCount; i++)
            {
                Vector4 border = GetSpriteBorderAt(i);
                if (m_GizmoMode != GizmoMode.BorderEditing && (m_RectsCache != null && m_RectsCache.spriteRects[i].spriteID != selectedGUID))
                {
                    if (Mathf.Approximately(border.sqrMagnitude, 0))
                        continue;
                }

                var rect = GetSpriteRectAt(i);
                SpriteEditorUtility.DrawLine(new Vector3(rect.xMin + border.x, rect.yMin), new Vector3(rect.xMin + border.x, rect.yMax));
                SpriteEditorUtility.DrawLine(new Vector3(rect.xMax - border.z, rect.yMin), new Vector3(rect.xMax - border.z, rect.yMax));

                SpriteEditorUtility.DrawLine(new Vector3(rect.xMin, rect.yMin + border.y), new Vector3(rect.xMax, rect.yMin + border.y));
                SpriteEditorUtility.DrawLine(new Vector3(rect.xMin, rect.yMax - border.w), new Vector3(rect.xMax, rect.yMax - border.w));
            }
            SpriteEditorUtility.EndLines();

            if (ShouldShowRectScaling())
            {
                Rect r = selectedSpriteRect;
                SpriteEditorUtility.BeginLines(new Color(0f, 0.1f, 0.3f, 0.25f));
                SpriteEditorUtility.DrawBox(new Rect(r.xMin + 1f / m_Zoom, r.yMin + 1f / m_Zoom, r.width, r.height));
                SpriteEditorUtility.EndLines();
                SpriteEditorUtility.BeginLines(new Color(0.25f, 0.5f, 1f, 0.75f));
                SpriteEditorUtility.DrawBox(r);
                SpriteEditorUtility.EndLines();
            }
        }

        // implements ISpriteEditorModule

        public override void DoMainGUI()
        {
            m_Zoom = Handles.matrix.GetColumn(0).magnitude;
        }

        public override void DoPostGUI()
        {
        }
    }
}
