//using UnityEditor;
//using UnityEngine;
//using System.Reflection;
//using System.Linq;
//using System.Threading.Tasks;
//using EditorButton;

//namespace EditorButton.Editor
//{
//    [CustomEditor(typeof(MonoBehaviour), true)]
//    public class EditorButtonAttributeDrawer : UnityEditor.Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            base.OnInspectorGUI();

//            MonoBehaviour monoBehaviour = (MonoBehaviour)target;

//            var methods = monoBehaviour.GetType()
//                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
//                .Where(m => m.GetCustomAttributes(typeof(Button), false).Length > 0)
//                .ToArray();

//            foreach (var method in methods)
//            {
//                var attribute = (Button)method.GetCustomAttributes(typeof(Button), false)[0];

//                string buttonLabel = string.IsNullOrEmpty(attribute.ButtonLabel) ? method.Name : attribute.ButtonLabel;

//                Color originalColor = GUI.backgroundColor;
//                GUI.backgroundColor = attribute.ButtonColor;

//                if (GUILayout.Button(buttonLabel))
//                {
//                    if (method.ReturnType == typeof(Task))
//                    {
//                        _ = InvokeAsyncMethod(method, monoBehaviour);
//                    }
//                    else
//                    {
//                        method.Invoke(monoBehaviour, null);
//                    }
//                }

//                GUI.backgroundColor = originalColor;
//            }
//        }

//        private async Task InvokeAsyncMethod(MethodInfo method, MonoBehaviour monoBehaviour)
//        {
//            await (Task)method.Invoke(monoBehaviour, null);
//        }
//    }
//}