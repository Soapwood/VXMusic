// ***********************************************************************
// Assembly         : Assembly-CSharp-Editor
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 12-18-2017
//                    
// List of Unity built in Styles:
//http://stackoverflow.com/questions/43727199/is-there-a-resource-for-all-of-the-custom-gui-styles-that-come-with-unity
//https://gist.github.com/MadLittleMods/ea3e7076f0f59a702ecb
//
// ***********************************************************************
using UnityEditor;
using UnityEngine;

namespace AiUnity.Common.Editor.Styles
{
    /// <summary>
    /// Custom Styles used in editor windows.
    /// </summary>
    public static class CustomEditorStyles
    {
        #region Fields
        private static GUIStyle csScriptIconStyle;
        private static GUIStyle editorLine;
        private static GUIStyle helpIconStyle;
        private static GUIStyle infoIconStyle;
        private static GUIStyle minusIconStyle;
        private static GUIStyle plusIconStyle;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the cs script icon style.
        /// </summary>
        public static GUIStyle CSScriptIconStyle
        {
            get
            {
                if (CustomEditorStyles.csScriptIconStyle == null)
                {
                    CustomEditorStyles.csScriptIconStyle = new GUIStyle { fixedWidth = 15f, fixedHeight = 15f };
                    CustomEditorStyles.csScriptIconStyle.normal.background = EditorGUIUtility.Load("cs Script Icon") as Texture2D;
                    CustomEditorStyles.csScriptIconStyle.margin.top = 2;
                    CustomEditorStyles.csScriptIconStyle.margin.left = 4;
                    CustomEditorStyles.csScriptIconStyle.margin.right = 4;
                }
                return CustomEditorStyles.csScriptIconStyle;
            }
        }

        /// <summary>
        /// Gets the style for a editor separation line.
        /// </summary>
        public static GUIStyle EditorLine
        {
            get
            {
                if (CustomEditorStyles.editorLine == null)
                {
                    CustomEditorStyles.editorLine = new GUIStyle("box");
                    CustomEditorStyles.editorLine.border.top = CustomEditorStyles.editorLine.border.bottom = 1;
                    CustomEditorStyles.editorLine.margin.top = CustomEditorStyles.editorLine.margin.bottom = 1;
                    CustomEditorStyles.editorLine.padding.top = CustomEditorStyles.editorLine.padding.bottom = 1;
                }
                return CustomEditorStyles.editorLine;
            }
        }

        /// <summary>
        /// Gets the help icon style.
        /// </summary>
        public static GUIStyle HelpIconStyle
        {
            get
            {
                if (CustomEditorStyles.helpIconStyle == null)
                {
                    CustomEditorStyles.helpIconStyle = new GUIStyle { fixedWidth = 15f, fixedHeight = 15f };
                    CustomEditorStyles.helpIconStyle.normal.background = EditorGUIUtility.Load("_Help") as Texture2D;
                    CustomEditorStyles.helpIconStyle.margin.top = 2;
                    CustomEditorStyles.helpIconStyle.margin.left = 4;
                    CustomEditorStyles.helpIconStyle.margin.right = 4;
                }
                return CustomEditorStyles.helpIconStyle;
            }
        }

        /// <summary>
        /// Gets the minus icon style.
        /// </summary>
        public static GUIStyle MinusIconStyle
        {
            get
            {
                if (CustomEditorStyles.minusIconStyle == null)
                {
                    CustomEditorStyles.minusIconStyle = new GUIStyle { fixedWidth = 15f, fixedHeight = 15f };
                    //CustomEditorStyles.minusIconStyle.normal.background = EditorGUIUtility.Load("ol minus") as Texture2D;
                    //CustomEditorStyles.minusIconStyle.active.background = EditorGUIUtility.Load("ol minus act") as Texture2D;
                    CustomEditorStyles.minusIconStyle.normal.background = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Toolbar Minus.png" : "ol minus") as Texture2D;
                    CustomEditorStyles.minusIconStyle.active.background = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Toolbar Minus.png" : "ol minus act") as Texture2D;
                    CustomEditorStyles.minusIconStyle.margin.top = 3;
                }
                return CustomEditorStyles.minusIconStyle;
            }
        }

        public static GUIStyle MinusIconMiniStyle
        {
            get
            {
                if (CustomEditorStyles.minusIconStyle == null)
                {
                    CustomEditorStyles.minusIconStyle = new GUIStyle(EditorStyles.label) { fixedWidth = 13f, fixedHeight = 13f };
                    //CustomEditorStyles.minusIconStyle.normal.background = EditorGUIUtility.Load("ol minus") as Texture2D;
                    //CustomEditorStyles.minusIconStyle.active.background = EditorGUIUtility.Load("ol minus act") as Texture2D;
                    CustomEditorStyles.minusIconStyle.normal.background = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Toolbar Minus.png" : "ol minus") as Texture2D;
                    CustomEditorStyles.minusIconStyle.active.background = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Toolbar Minus.png" : "ol minus act") as Texture2D;
                    CustomEditorStyles.minusIconStyle.margin.top = 4;
                    CustomEditorStyles.minusIconStyle.margin.right = 8;
                }
                return CustomEditorStyles.minusIconStyle;
            }
        }

        /// <summary>
        /// Gets the plus icon style.
        /// </summary>
        public static GUIStyle PlusIconStyle
        {
            get
            {
                if (CustomEditorStyles.plusIconStyle == null)
                {
                    CustomEditorStyles.plusIconStyle = new GUIStyle { fixedWidth = 15f, fixedHeight = 15f };
                    //CustomEditorStyles.plusIconStyle.normal.background = EditorGUIUtility.Load("ol plus") as Texture2D;
                    //CustomEditorStyles.plusIconStyle.active.background = EditorGUIUtility.Load("ol plus act") as Texture2D;
                    CustomEditorStyles.plusIconStyle.normal.background = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Toolbar Plus.png" : "ol plus") as Texture2D;
                    CustomEditorStyles.plusIconStyle.active.background = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Toolbar Plus.png" : "ol plus act") as Texture2D;
                    CustomEditorStyles.plusIconStyle.margin.top = 3;
                    CustomEditorStyles.plusIconStyle.margin.left = 4;
                    CustomEditorStyles.plusIconStyle.margin.right = 4;
                }
                return CustomEditorStyles.plusIconStyle;
            }
        }

        /// <summary>
        /// Gets the plus icon style.
        /// </summary>
        public static GUIStyle PlusIconMiniStyle
        {
            get
            {
                if (CustomEditorStyles.plusIconStyle == null)
                {
                    CustomEditorStyles.plusIconStyle = new GUIStyle { fixedWidth = 13f, fixedHeight = 13f };
                    //CustomEditorStyles.plusIconStyle.normal.background = EditorGUIUtility.Load("ol plus") as Texture2D;
                    //CustomEditorStyles.plusIconStyle.active.background = EditorGUIUtility.Load("ol plus act") as Texture2D;
                    CustomEditorStyles.plusIconStyle.normal.background = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Toolbar Plus.png" : "ol plus") as Texture2D;
                    CustomEditorStyles.plusIconStyle.active.background = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_Toolbar Plus.png" : "ol plus act") as Texture2D;

                    CustomEditorStyles.plusIconStyle.margin.top = 4;
                    CustomEditorStyles.plusIconStyle.margin.left = 4;
                    CustomEditorStyles.plusIconStyle.margin.right = 4;
                }
                return CustomEditorStyles.plusIconStyle;
            }
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            //GUIStyle bbb = new GUIStyle("OL Minus");
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        private static Texture2D MakeTex2(int width, int height, Color textureColor, RectOffset border, Color borderColor)
        {
            int widthInner = width;

            //Texture2D minus = EditorGUIUtility.Load("ol minus") as Texture2D;
            width += border.left;
            width += border.right;

            Color[] pix = new Color[width * (height + border.top + border.bottom)];



            for (int i = 0; i < pix.Length; i++)
            {
                if (i < (border.bottom * width))
                    pix[i] = borderColor;
                else if (i >= ((border.bottom * width) + (height * width)))  //Border Top
                    pix[i] = borderColor;
                else
                { //Center of Texture

                    if ((i % width) < border.left) // Border left
                        pix[i] = borderColor;
                    else if ((i % width) >= (border.left + widthInner)) //Border right
                        pix[i] = borderColor;
                    else
                    pix[i] = textureColor;    //Color texture
                    //pix[i] = minus.GetPixel(i, 0);    //Color texture
                }
            }

            Texture2D result = new Texture2D(width, height + border.top + border.bottom);
            //Texture2D result = EditorGUIUtility.Load("ol minus") as Texture2D;
            result.SetPixels(pix);
            result.Apply();


            return result;

        }


        #endregion
    }
}
