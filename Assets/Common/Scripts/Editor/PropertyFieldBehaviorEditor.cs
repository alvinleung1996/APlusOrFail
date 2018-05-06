using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace APlusOrFail
{
    // http://wiki.unity3d.com/index.php?title=Expose_properties_in_inspector
    [CustomEditor(typeof(PropertyFieldBehavior), true)]
    public class PropertyFieldBehaviorEditor : Editor
    {
        private static readonly GUILayoutOption[] emptyLayoutOptions = Array.Empty<GUILayoutOption>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical(emptyLayoutOptions);
                
            foreach (PropertyInfo info in target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                EditorPropertyFieldAttribute attr = (EditorPropertyFieldAttribute)Attribute.GetCustomAttribute(info, typeof(EditorPropertyFieldAttribute));
                MethodInfo getter;

                if (attr != null && (getter = info.GetGetMethod(true)) != null && (getter.IsPublic || attr.forceGet))
                {
                    Type type = info.PropertyType;
                    string nickName = ObjectNames.NicifyVariableName(info.Name);

                    EditorGUILayout.BeginHorizontal(emptyLayoutOptions);

                    MethodInfo setter = info.GetSetMethod(true);

                    using (new EditorGUI.DisabledScope(setter == null || (!setter.IsPublic && !attr.forceSet)))
                    {
                        EditorGUI.BeginChangeCheck();

                        if (type == typeof(AnimationCurve))
                        {
                            SafeSetValue(info, target, EditorGUILayout.CurveField(nickName, (AnimationCurve)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(bool))
                        {
                            SafeSetValue(info, target, EditorGUILayout.Toggle(nickName, (bool)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(Bounds))
                        {
                            SafeSetValue(info, target, EditorGUILayout.BoundsField(nickName, (Bounds)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(BoundsInt))
                        {
                            SafeSetValue(info, target, EditorGUILayout.BoundsIntField(nickName, (BoundsInt)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(Color))
                        {
                            SafeSetValue(info, target, EditorGUILayout.ColorField(nickName, (Color)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type.IsEnum)
                        {
                            if (Attribute.IsDefined(type, typeof(FlagsAttribute)))
                            {
                                SafeSetValue(info, target, EditorGUILayout.EnumFlagsField(nickName, (Enum)info.GetValue(target), emptyLayoutOptions));
                            }
                            else
                            {
                                SafeSetValue(info, target, EditorGUILayout.EnumPopup(nickName, (Enum)info.GetValue(target), emptyLayoutOptions));
                            }
                        }
                        else if (type == typeof(float))
                        {
                            SafeSetValue(info, target, EditorGUILayout.FloatField(nickName, (float)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(int))
                        {
                            SafeSetValue(info, target, EditorGUILayout.IntField(nickName, (int)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(LayerMask))
                        {
                            // TODO: check
                            string[] layerNames = new string[32];
                            for (int i = 0; i < 32; ++i)
                            {
                                layerNames[i] = LayerMask.LayerToName(i);
                                if (layerNames[i].Length == 0) layerNames[i] = null;
                            }
                            SafeSetValue(info, target, EditorGUILayout.MaskField(nickName, (LayerMask)info.GetValue(target), layerNames, emptyLayoutOptions));
                        }
                        else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                        {
                            SafeSetValue(info, target, EditorGUILayout.ObjectField(nickName, (UnityEngine.Object)info.GetValue(target), type, true, emptyLayoutOptions));
                        }
                        else if (type == typeof(Rect))
                        {
                            SafeSetValue(info, target, EditorGUILayout.RectField(nickName, (Rect)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(RectInt))
                        {
                            SafeSetValue(info, target, EditorGUILayout.RectIntField(nickName, (RectInt)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(string))
                        {
                            SafeSetValue(info, target, EditorGUILayout.TextField(nickName, (string)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(Vector2))
                        {
                            SafeSetValue(info, target, EditorGUILayout.Vector2Field(nickName, (Vector2)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(Vector2Int))
                        {
                            SafeSetValue(info, target, EditorGUILayout.Vector2IntField(nickName, (Vector2Int)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(Vector3))
                        {
                            SafeSetValue(info, target, EditorGUILayout.Vector3Field(nickName, (Vector3)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(Vector3Int))
                        {
                            SafeSetValue(info, target, EditorGUILayout.Vector3IntField(nickName, (Vector3Int)info.GetValue(target), emptyLayoutOptions));
                        }
                        else if (type == typeof(Vector4))
                        {
                            SafeSetValue(info, target, EditorGUILayout.Vector4Field(nickName, (Vector4)info.GetValue(target), emptyLayoutOptions));
                        }

                        if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                        {
                            Undo.RecordObject(target, $"Changed {nickName}");
                            EditorUtility.SetDirty(target);
                            EditorSceneManager.MarkSceneDirty(((Component)target).gameObject.scene);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private static void SafeSetValue(PropertyInfo info, object obj, object value)
        {
            MethodInfo setter = info.GetSetMethod(true);
            if (setter != null && (setter.IsPublic || ((EditorPropertyFieldAttribute)Attribute.GetCustomAttribute(info, typeof(EditorPropertyFieldAttribute))).forceSet))
            {
                info.SetValue(obj, value);
            }
        }
    }
}
