using System;
using UnityEngine;

namespace EditorButton
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Button : Attribute
    {
        public string ButtonLabel;
        public Color ButtonColor;

        public Button(string buttonLabel = "", string buttonColor = "")
        {
            ButtonLabel = buttonLabel;
            ButtonColor = ParseColor(buttonColor);
        }
        
        private Color ParseColor(string colorString)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(colorString, out color))
            {
                return color;
            }
            return Color.white; // Default color if the provided color string is invalid or empty
        }
    }
}
