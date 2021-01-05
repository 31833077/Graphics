using System.Runtime.InteropServices;
using Object = UnityEngine.Object;
using UnityEngine.Scripting;
using UnityEngine.Bindings;
using UnityEngine;
using UnityEditor;
using System;

namespace Unity.Assets.MaterialVariant.Editor
{
    // Defines a single modified property.
    [System.Serializable]
    public struct MaterialPropertyModification
    {
        enum SerializedType
        {
            Scalar,
            Color,      // will be decomposed as r g b a scalars
            Vector,     // will be decomposed as r g b a scalars
            Texture,    // will be decomposed as m_Texture ObjectReference and m_Scale and m_Offset Vector2s, and each vector2 will be split into x and y
            NonMaterialProperty,    //only handle int for now. Should be enough. It is for properties outside of maps.
        }

        // Property path of the property being modified (Matches as SerializedProperty.propertyPath)
        [SerializeField] string m_PropertyPath;
        // The value being applied
        [SerializeField] float m_Value;
        // The value being applied when it is a object reference (which can not be represented as a string)
        [SerializeField] Object m_ObjectReference;

        internal string propertyPath => m_PropertyPath;
        internal string key
        {
            get
            {
                (SerializedType type, string[] pathParts) = RecreateType(this);
                if (type == SerializedType.NonMaterialProperty)
                    return $"::{pathParts[0]}:";
                else
                    return pathParts[0];
            }
        }

        private MaterialPropertyModification(string propertyPath, float value, Object objectReference)
        {
            m_PropertyPath = propertyPath;
            m_Value = value;
            m_ObjectReference = objectReference;
        }

        public static System.Collections.Generic.IEnumerable<MaterialPropertyModification> CreateMaterialPropertyModifications(MaterialProperty property)
        {
            SerializedType type = ResolveType(property);
            switch (type)
            {
                case SerializedType.Scalar:
                    return new[] { new MaterialPropertyModification(property.name, property.floatValue, null) };
                case SerializedType.Color:
                    return new[]
                    {
                        new MaterialPropertyModification(property.name + ".r", property.colorValue.r, null),
                        new MaterialPropertyModification(property.name + ".g", property.colorValue.g, null),
                        new MaterialPropertyModification(property.name + ".b", property.colorValue.b, null),
                        new MaterialPropertyModification(property.name + ".a", property.colorValue.a, null)
                    };
                case SerializedType.Vector:
                    return new[]
                    {
                        new MaterialPropertyModification(property.name + ".x", property.vectorValue.x, null),
                        new MaterialPropertyModification(property.name + ".y", property.vectorValue.y, null),
                        new MaterialPropertyModification(property.name + ".z", property.vectorValue.z, null),
                        new MaterialPropertyModification(property.name + ".w", property.vectorValue.w, null)
                    };
                case SerializedType.Texture:
                    return new[]
                    {
                        new MaterialPropertyModification(property.name + ".m_Texture", 0f, property.textureValue),
                        new MaterialPropertyModification(property.name + ".m_Scale.x", property.textureScaleAndOffset.x, null),
                        new MaterialPropertyModification(property.name + ".m_Scale.y", property.textureScaleAndOffset.y, null),
                        new MaterialPropertyModification(property.name + ".m_Offset.x", property.textureScaleAndOffset.z, null),
                        new MaterialPropertyModification(property.name + ".m_Offset.y", property.textureScaleAndOffset.w, null)
                    };
                default:
                    throw new Exception("Unhandled type in Material");
            }
        }

        public static void RevertModification(MaterialProperty property, string rootGUID)
        {
            Object parent = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(rootGUID));
            if (parent is Material material)
            {
                int nameId = Shader.PropertyToID(property.name);
                if (material.HasProperty(nameId))
                {
                    switch (property.type)
                    {
                        case MaterialProperty.PropType.Float:
                        case MaterialProperty.PropType.Range:
                            property.floatValue = material.GetFloat(nameId);
                            break;
                        case MaterialProperty.PropType.Vector:
                            property.vectorValue = material.GetVector(nameId);
                            break;
                        case MaterialProperty.PropType.Color:
                            property.colorValue = material.GetColor(nameId);
                            break;
                        case MaterialProperty.PropType.Texture:
                            property.textureValue = material.GetTexture(nameId);
                            Vector2 scale = material.GetTextureScale(nameId), offset = material.GetTextureOffset(nameId);
                            property.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                            break;
                    }
                }
            }
            else if (parent is Shader shader)
            {
                int nameId = shader.FindPropertyIndex(property.name);
                switch (property.type)
                {
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        property.floatValue = shader.GetPropertyDefaultFloatValue(nameId);
                        break;
                    case MaterialProperty.PropType.Vector:
                        property.vectorValue = shader.GetPropertyDefaultVectorValue(nameId);
                        break;
                    case MaterialProperty.PropType.Color:
                        property.colorValue = shader.GetPropertyDefaultVectorValue(nameId);
                        break;
                    case MaterialProperty.PropType.Texture:
                        var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(shader)) as ShaderImporter;
                        property.textureValue = importer.GetDefaultTexture(property.name);
                        property.textureScaleAndOffset = new Vector4(1, 1, 0, 0);
                        break;
                }
            }
        }

        public static System.Collections.Generic.IEnumerable<MaterialPropertyModification> CreateMaterialPropertyModificationsForNonMaterialProperty<T>(string key, T value)
            where T : struct
        {
            if (typeof(T) == typeof(int))
                return new[] { new MaterialPropertyModification($"::{key}:{value.ToString()}", default, null) };
            if (typeof(T) == typeof(float))
                return new[] { new MaterialPropertyModification(key, (float)(object)value, null) };
            if (typeof(T) == typeof(Color))
            {
                Color colValue = (Color)(object)value;
                return new[]
                {
                    new MaterialPropertyModification($"{key}.r", colValue.r, null),
                    new MaterialPropertyModification($"{key}.g", colValue.g, null),
                    new MaterialPropertyModification($"{key}.b", colValue.b, null),
                    new MaterialPropertyModification($"{key}.a", colValue.a, null),
                };
            }
            if (typeof(T) == typeof(Vector4))
            {
                Vector4 vecValue = (Vector4)(object)value;
                return new[]
                {
                    new MaterialPropertyModification($"{key}.x", vecValue.x, null),
                    new MaterialPropertyModification($"{key}.y", vecValue.y, null),
                    new MaterialPropertyModification($"{key}.z", vecValue.z, null),
                    new MaterialPropertyModification($"{key}.w", vecValue.w, null)
                };
            }
            return new MaterialPropertyModification[0];
        }

        public static void ApplyPropertyModificationsToMaterial(Material material, System.Collections.Generic.IEnumerable<MaterialPropertyModification> propertyModifications)
        {
            SerializedObject serializedMaterial = new SerializedObject(material);
            foreach (MaterialPropertyModification propertyModification in propertyModifications)
                ApplyOnePropertyModificationToSerializedObject(serializedMaterial, propertyModification);
            serializedMaterial.ApplyModifiedProperties();
        }

        static void ApplyOnePropertyModificationToSerializedObject(SerializedObject serializedMaterial, MaterialPropertyModification propertyModificaton)
        {
            (SerializedType type, string[] pathParts) = RecreateType(propertyModificaton);

            if (type != SerializedType.NonMaterialProperty)
            {
                (SerializedProperty property, int index, SerializedProperty parent) = FindProperty(serializedMaterial, pathParts[0], type);
                for (int i = 1; i < pathParts.Length; ++i)
                    property = property.FindPropertyRelative(pathParts[i]);

                if (property.propertyType == SerializedPropertyType.ObjectReference)
                    property.objectReferenceValue = propertyModificaton.m_ObjectReference;
                else
                    property.floatValue = propertyModificaton.m_Value;
            }
            else
            {
                SerializedProperty property = serializedMaterial.FindProperty(pathParts[0]);
                // int should be enough. If we need to handle any other type here, we need to write it on the line like ::name:string:somevalue for retrieving where to assign it.
                property.intValue = int.Parse(pathParts[1]);
            }
        }

        static SerializedType ResolveType(MaterialProperty value)
        {
            switch (value.type)
            {
                case MaterialProperty.PropType.Float:   return SerializedType.Scalar;
                case MaterialProperty.PropType.Range:   return SerializedType.Scalar;
                case MaterialProperty.PropType.Color:   return SerializedType.Color;
                case MaterialProperty.PropType.Vector:  return SerializedType.Vector;
                case MaterialProperty.PropType.Texture: return SerializedType.Texture;
                default:
                    throw new ArgumentException("Unhandled MaterialProperty Type", "value");
            }
        }

        static (SerializedType type, string[] pathParts) RecreateType(MaterialPropertyModification propertyModification)
        {
            if (propertyModification.m_PropertyPath.StartsWith("::"))
            {
                string[] nonMaterialPropertyParts = propertyModification.m_PropertyPath.TrimStart(':').Split(new[] { ':' });
                return (SerializedType.NonMaterialProperty, nonMaterialPropertyParts);
            }

            string[] parts = propertyModification.m_PropertyPath.Split(new[] { '.' });
            if (parts.Length == 1)
                return (SerializedType.Scalar, parts);

            if (propertyModification.m_ObjectReference != null)
                return (SerializedType.Texture, parts);

            if (parts.Length == 2)
            {
                string sub = parts[1];
                if (sub == "r" || sub == "g" || sub == "b" || sub == "a")
                    return (SerializedType.Color, parts);
                else
                {
                    // replace on the fly the sub path as in YAML vector are stored as r g b a
                    if (sub == "x") { parts[1] = "r"; return (SerializedType.Vector, parts); }
                    else if (sub == "y") { parts[1] = "g"; return (SerializedType.Vector, parts); }
                    else if (sub == "z") { parts[1] = "b"; return (SerializedType.Vector, parts); }
                    else if (sub == "w") { parts[1] = "a"; return (SerializedType.Vector, parts); }
                    // else it is a texture object name
                }
            }

            return (SerializedType.Texture, parts);  //could be length 2 only if object reference, else length is 3
        }

        static SerializedProperty FindBase(SerializedObject material, SerializedType type)
        {
            var propertyBase = material.FindProperty("m_SavedProperties");

            switch (type)
            {
                case SerializedType.Scalar:
                    propertyBase = propertyBase.FindPropertyRelative("m_Floats");
                    break;
                case SerializedType.Vector:
                case SerializedType.Color:
                    propertyBase = propertyBase.FindPropertyRelative("m_Colors");
                    break;
                case SerializedType.Texture:
                    propertyBase = propertyBase.FindPropertyRelative("m_TexEnvs");
                    break;
                default:
                    throw new ArgumentException($"Unknown SerializedType {type}");
            }

            return propertyBase;
        }

        static (SerializedProperty property, int index, SerializedProperty parent) FindProperty(SerializedObject material, string propertyName, SerializedType type)
        {
            var propertyBase = FindBase(material, type);

            SerializedProperty property = null;
            int maxSearch = propertyBase.arraySize;
            int indexOf = 0;
            for (; indexOf < maxSearch; ++indexOf)
            {
                property = propertyBase.GetArrayElementAtIndex(indexOf);
                if (property.FindPropertyRelative("first").stringValue == propertyName)
                    break;
            }
            if (indexOf == maxSearch)
                throw new ArgumentException($"Unknown property: {propertyName}");

            property = property.FindPropertyRelative("second");
            return (property, indexOf, propertyBase);
        }

        public static bool operator==(MaterialPropertyModification mpm1, MaterialPropertyModification mpm2)
        {
            return mpm1.m_PropertyPath == mpm2.m_PropertyPath
                && mpm1.m_Value == mpm2.m_Value
                && mpm1.m_ObjectReference == mpm2.m_ObjectReference;
        }

        public static bool operator!=(MaterialPropertyModification mpm1, MaterialPropertyModification mpm2)
        {
            return mpm1.m_PropertyPath != mpm2.m_PropertyPath
                || mpm1.m_Value != mpm2.m_Value
                || mpm1.m_ObjectReference != mpm2.m_ObjectReference;
        }

        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            var second = (MaterialPropertyModification)o;
            return this == second;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + m_PropertyPath.GetHashCode();
            hash = hash * 23 + m_Value.GetHashCode();
            hash = hash * 23 + m_ObjectReference.GetHashCode();
            return hash;
        }
    }
}
