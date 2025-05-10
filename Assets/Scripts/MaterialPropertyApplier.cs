using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MaterialPropertyApplier : MonoBehaviour
{
    [SerializeField] private List<MaterialProperty> _shaderProperties = new();

    [MenuItem("MyTools/ShaderPropertyApplier.Apply")]
    public static void Apply()
    {
        var objs = FindObjectsByType<MaterialPropertyApplier>(FindObjectsSortMode.None);
        foreach (var obj in objs)
        {
            obj.ApplyProperty();
        }
    }

    [MenuItem("MyTools/ShaderPropertyApplier.Clear")]
    public static void Clear()
    {
        var objs = FindObjectsByType<MaterialPropertyApplier>(FindObjectsSortMode.None);
        foreach (var obj in objs)
        {
            obj.ClearProperty();
        }
    }

    private void Awake()
    {
        ApplyProperty();
    }

    // �v���p�e�B��K�p
    public void ApplyProperty()
    {
        var renderer = GetComponent<Renderer>();
        var block = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(block);

        foreach (var prop in _shaderProperties)
        {
            switch (prop.type)
            {
                case MaterialProperty.PropertyType.Int:
                    block.SetInt(prop.name, prop.intValue);
                    break;

                case MaterialProperty.PropertyType.Float:
                    block.SetFloat(prop.name, prop.floatValue);
                    break;

                case MaterialProperty.PropertyType.FloatArray:
                    block.SetFloatArray(prop.name, prop.floatArray);
                    break;

                case MaterialProperty.PropertyType.Texture:
                    block.SetTexture(prop.name, prop.texture);
                    break;

                case MaterialProperty.PropertyType.Color:
                    block.SetColor(prop.name, prop.color);
                    break;
            }
        }

        renderer.SetPropertyBlock(block);
    }

    // �v���p�e�B���N���A
    public void ClearProperty()
    {
        var renderer = GetComponent<Renderer>();
        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.Clear();
        renderer.SetPropertyBlock(block);
    }

    // �v���p�e�B�̕ύX
    public void ChangeProperty<T>(string name, T value)
    {
        var idx = _shaderProperties.FindIndex(x => x.name == name);
        var prop = _shaderProperties[idx];
        switch (prop.type)
        {
            case MaterialProperty.PropertyType.Int:
                if (value is int intValue)
                    prop.intValue = intValue;
                break;
            case MaterialProperty.PropertyType.Float:
                if (value is float floatValue)
                    prop.floatValue = floatValue;
                break;
            case MaterialProperty.PropertyType.FloatArray:
                if (value is float[] floatArray)
                    prop.floatArray = floatArray;
                break;
            case MaterialProperty.PropertyType.Texture:
                if (value is Texture2D texture)
                    prop.texture = texture;
                break;
            case MaterialProperty.PropertyType.Color:
                if (value is Color color)
                    prop.color = color;
                break;
        }
        _shaderProperties[idx] = prop;

        ApplyProperty();  // �ύX�𔽉f
    }

    // �v���p�e�B�̎擾
    public T GetProperty<T>(string name)
    {
        var idx = _shaderProperties.FindIndex(x => x.name == name);
        var prop = _shaderProperties[idx];
        switch (prop.type)
        {
            case MaterialProperty.PropertyType.Int:
                if (prop.intValue is T intValue)
                    return intValue;
                break;
            case MaterialProperty.PropertyType.Float:
                if (prop.floatValue is T floatValue)
                    return floatValue;
                break;
            case MaterialProperty.PropertyType.FloatArray:
                if (prop.floatArray is T floatArray)
                    return floatArray;
                break;
            case MaterialProperty.PropertyType.Texture:
                if (prop.texture is T texture)
                    return texture;
                break;
            case MaterialProperty.PropertyType.Color:
                if (prop.color is T color)
                    return color;
                break;
        }

        return default;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class MaterialProperty
{
    public enum PropertyType { Int, Float, FloatArray, Texture, Color }
    public PropertyType type;
    public string name;
    [HideInInspector] public int intValue;
    [HideInInspector] public float floatValue;
    [HideInInspector] public float[] floatArray;
    [HideInInspector] public Texture2D texture;
    [HideInInspector] public Color color;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(MaterialProperty))]
public class ShaderPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // �e�v���p�e�B���擾
        var typeProperty = property.FindPropertyRelative("type");
        var nameProperty = property.FindPropertyRelative("name");
        var intValueProperty = property.FindPropertyRelative("intValue");
        var floatValueProperty = property.FindPropertyRelative("floatValue");
        var floatArrayProperty = property.FindPropertyRelative("floatArray");
        var textureProperty = property.FindPropertyRelative("texture");
        var colorProperty = property.FindPropertyRelative("color");

        // �s�̍����ݒ�
        var singleLineHeight = EditorGUIUtility.singleLineHeight;

        // name�t�B�[���h�̕`��
        position.height = singleLineHeight;
        EditorGUI.PropertyField(position, nameProperty, new GUIContent("Name"));

        // type�t�B�[���h�̕`��
        position.y += singleLineHeight;
        EditorGUI.PropertyField(position, typeProperty, new GUIContent("Type"));

        // type�ɉ������t�B�[���h�̕`��
        position.y += singleLineHeight;
        switch ((MaterialProperty.PropertyType)typeProperty.enumValueIndex)
        {
            case MaterialProperty.PropertyType.Int:
                EditorGUI.PropertyField(position, intValueProperty, new GUIContent("Value (Int)"));
                break;

            case MaterialProperty.PropertyType.Float:
                EditorGUI.PropertyField(position, floatValueProperty, new GUIContent("Value (Float)"));
                break;

            case MaterialProperty.PropertyType.FloatArray:
                if (floatArrayProperty.arraySize == 0) floatArrayProperty.arraySize = 1;
                EditorGUI.PropertyField(position, floatArrayProperty, new GUIContent("Value (Float Array)"));
                break;

            case MaterialProperty.PropertyType.Texture:
                EditorGUI.PropertyField(position, textureProperty, new GUIContent("Value (Texture)"));
                break;

            case MaterialProperty.PropertyType.Color:
                EditorGUI.PropertyField(position, colorProperty, new GUIContent("Value (Color)"));
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // ��{�s�̍�����ݒ�
        var singleLineHeight = EditorGUIUtility.singleLineHeight;
        var height = singleLineHeight * 2; // name��type��2�s���̍���

        // type �ɂ���č�����ǉ�
        var typeProperty = property.FindPropertyRelative("type");

        switch ((MaterialProperty.PropertyType)typeProperty.enumValueIndex)
        {
            case MaterialProperty.PropertyType.Int:
            case MaterialProperty.PropertyType.Float:
            case MaterialProperty.PropertyType.Texture:
            case MaterialProperty.PropertyType.Color:
                height += singleLineHeight; // 1�s���ǉ�
                break;

            case MaterialProperty.PropertyType.FloatArray:
                var floatArrayProperty = property.FindPropertyRelative("floatArray");
                height += EditorGUI.GetPropertyHeight(floatArrayProperty, true); // �z��̍����𓮓I�Ɏ擾
                break;
        }

        return height;
    }
}

#endif