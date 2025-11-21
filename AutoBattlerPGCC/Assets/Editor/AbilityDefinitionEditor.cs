using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

[CustomEditor(typeof(AbilityDefinition))]
public class AbilityDefinitionEditor : Editor
{
    private AbilityDefinition ability;

    private void OnEnable()
    {
        ability = (AbilityDefinition)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default fields up to conditions
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityType"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("trigger"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("intervalTime"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ability Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("additionalCost"));

        EditorGUILayout.Space();

        // CUSTOM CONDITIONS SECTION
        DrawConditionsList();

        EditorGUILayout.Space();

        // CUSTOM EFFECTS SECTION
        DrawEffectsList();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawConditionsList()
    {
        EditorGUILayout.LabelField("Conditions (ALL must be met)", EditorStyles.boldLabel);

        // Display all current conditions
        for (int i = 0; i < ability.conditions.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            if (ability.conditions[i] != null)
            {
                EditorGUILayout.LabelField(ability.conditions[i].GetType().Name, EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("NULL Condition", EditorStyles.boldLabel);
            }

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                ability.conditions.RemoveAt(i);
                EditorUtility.SetDirty(ability);
                return;
            }

            EditorGUILayout.EndHorizontal();

            // Draw fields for this condition
            if (ability.conditions[i] != null)
            {
                DrawObjectFields(ability.conditions[i]);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        // Add new condition button with dropdown
        if (GUILayout.Button("+ Add Condition"))
        {
            ShowConditionMenu();
        }
    }

    private void DrawEffectsList()
    {
        EditorGUILayout.LabelField("Effects (Executed in order)", EditorStyles.boldLabel);

        // Display all current effects
        for (int i = 0; i < ability.effects.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            if (ability.effects[i] != null)
            {
                EditorGUILayout.LabelField(ability.effects[i].GetType().Name, EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("NULL Effect", EditorStyles.boldLabel);
            }

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                ability.effects.RemoveAt(i);
                EditorUtility.SetDirty(ability);
                return;
            }

            EditorGUILayout.EndHorizontal();

            // Draw fields for this effect
            if (ability.effects[i] != null)
            {
                DrawObjectFields(ability.effects[i]);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        // Add new effect button with dropdown
        if (GUILayout.Button("+ Add Effect"))
        {
            ShowEffectMenu();
        }
    }

    private void ShowConditionMenu()
    {
        GenericMenu menu = new GenericMenu();

        // Get all types that inherit from AbilityCondition
        var conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(AbilityCondition)) && !type.IsAbstract);

        foreach (var type in conditionTypes)
        {
            menu.AddItem(new GUIContent(type.Name), false, () => AddCondition(type));
        }

        menu.ShowAsContext();
    }

    private void ShowEffectMenu()
    {
        GenericMenu menu = new GenericMenu();

        // Get all types that inherit from AbilityEffect
        var effectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(AbilityEffect)) && !type.IsAbstract);

        foreach (var type in effectTypes)
        {
            menu.AddItem(new GUIContent(type.Name), false, () => AddEffect(type));
        }

        menu.ShowAsContext();
    }

    private void AddCondition(Type conditionType)
    {
        var newCondition = (AbilityCondition)Activator.CreateInstance(conditionType);
        ability.conditions.Add(newCondition);
        EditorUtility.SetDirty(ability);
    }

    private void AddEffect(Type effectType)
    {
        var newEffect = (AbilityEffect)Activator.CreateInstance(effectType);
        ability.effects.Add(newEffect);
        EditorUtility.SetDirty(ability);
    }

    // UPDATED METHOD - Now handles Lists, Sprites, Colors, and more!
    private void DrawObjectFields(object obj)
    {
        EditorGUI.indentLevel++;

        // Use reflection to draw all public fields
        var fields = obj.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            object newValue = null;

            // Handle basic types
            if (field.FieldType == typeof(int))
            {
                newValue = EditorGUILayout.IntField(ObjectNames.NicifyVariableName(field.Name), (int)value);
            }
            else if (field.FieldType == typeof(float))
            {
                newValue = EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(field.Name), (float)value);
            }
            else if (field.FieldType == typeof(bool))
            {
                newValue = EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(field.Name), (bool)value);
            }
            else if (field.FieldType == typeof(string))
            {
                newValue = EditorGUILayout.TextField(ObjectNames.NicifyVariableName(field.Name), (string)value);
            }
            else if (field.FieldType.IsEnum)
            {
                newValue = EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName(field.Name), (Enum)value);
            }
            // ADDED: Handle Sprite
            else if (field.FieldType == typeof(Sprite))
            {
                newValue = EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(field.Name), (Sprite)value, typeof(Sprite), false);
            }
            // ADDED: Handle Color
            else if (field.FieldType == typeof(Color))
            {
                newValue = EditorGUILayout.ColorField(ObjectNames.NicifyVariableName(field.Name), (Color)value);
            }
            else if (field.FieldType == typeof(GameObject))
            {
                newValue = EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(field.Name), (GameObject)value, typeof(GameObject), false);
            }
            else if (field.FieldType == typeof(LayerMask))
            {
                LayerMask tempMask = (LayerMask)value;
                newValue = EditorGUILayout.MaskField(ObjectNames.NicifyVariableName(field.Name), tempMask.value, UnityEditorInternal.InternalEditorUtility.layers);
                newValue = (LayerMask)(int)newValue;
            }
            // ADDED: Handle Vector2
            else if (field.FieldType == typeof(Vector2))
            {
                newValue = EditorGUILayout.Vector2Field(ObjectNames.NicifyVariableName(field.Name), (Vector2)value);
            }
            // ADDED: Handle Vector3
            else if (field.FieldType == typeof(Vector3))
            {
                newValue = EditorGUILayout.Vector3Field(ObjectNames.NicifyVariableName(field.Name), (Vector3)value);
            }
            // Handle List<Enum> (like List<StatType>)
            else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = field.FieldType.GetGenericArguments()[0];

                // Handle List<Enum>
                if (elementType.IsEnum)
                {
                    IList list = (IList)value;

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(field.Name), EditorStyles.boldLabel);

                    // Display list items
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        Enum enumValue = (Enum)list[i];
                        Enum newEnumValue = EditorGUILayout.EnumPopup($"Element {i}", enumValue);

                        if (!newEnumValue.Equals(enumValue))
                        {
                            list[i] = newEnumValue;
                            EditorUtility.SetDirty(ability);
                        }

                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            list.RemoveAt(i);
                            EditorUtility.SetDirty(ability);
                            break;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    // Add button
                    if (GUILayout.Button("+ Add Element"))
                    {
                        var defaultValue = Activator.CreateInstance(elementType);
                        list.Add(defaultValue);
                        EditorUtility.SetDirty(ability);
                    }

                    EditorGUILayout.EndVertical();
                }
                // Handle other list types
                else
                {
                    EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(field.Name), $"List<{elementType.Name}> (not fully supported)");
                }
            }
            // Fallback for unsupported types
            else
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(field.Name), $"{field.FieldType.Name} (not supported in editor)");
            }

            if (newValue != null && !newValue.Equals(value))
            {
                field.SetValue(obj, newValue);
                EditorUtility.SetDirty(ability);
            }
        }

        EditorGUI.indentLevel--;
    }
}