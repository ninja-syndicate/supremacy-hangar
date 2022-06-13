using System.Text;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor
{
    public class LogWidget : ILogInterface
    {
        private static GUIStyle textStyle = null;
        private readonly StringBuilder log = new StringBuilder();

        public void Reset()
        {
            log.Clear();
        }
        
        public void Render(params GUILayoutOption[] options)
        {
            if (textStyle == null) SetupTextStyle();
            GUILayout.TextArea(log.ToString(), textStyle, options);
        }

        public void LogNormal(string text)
        {
            log.Append($"{text}\n");
        }

        public void LogError(string text)
        {
            log.Append($"<color='red'>{text}</color>\n");
        }

        public void LogWarning(string text)
        {
            log.Append($"<color='yellow'>{text}</color>\n");
        }

        public void LogStyled(string text)
        {
            log.Append(text);
        }
        
        private void SetupTextStyle()
        {
            textStyle = new GUIStyle(GUI.skin.textArea)
            {
                richText = true,
            };
        }
    }
}