using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SafeAreaAdjustment))]
public class SafeAreaAdjustmentDrawer : PropertyDrawer
{
	// Draw the property inside the given rect
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);

		// Calculate rects
		int space = 5;
		int activeWidth = 30;
		int targetWidth = 100;
		int actionWidth = 60;
		var activeRect = new Rect(position.x, position.y, activeWidth, position.height);
		var targetRect = new Rect(position.x + activeWidth + space, position.y, targetWidth, position.height);
		var actionRect = new Rect(position.x + activeWidth + targetWidth + 2 * space, position.y, actionWidth, position.height);
		var areaRect = new Rect(position.x + + activeWidth + targetWidth + actionWidth + 3 * space, position.y, position.width - activeWidth - targetWidth - actionWidth - 3 * space, position.height);

		// Draw fields - passs GUIContent.none to each so they are drawn without labels
		EditorGUI.PropertyField(activeRect, property.FindPropertyRelative("active"), GUIContent.none);

		// Don't make child fields be indented
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		EditorGUI.PropertyField(targetRect, property.FindPropertyRelative("target"), GUIContent.none);
		EditorGUI.PropertyField(actionRect, property.FindPropertyRelative("action"), GUIContent.none);
		EditorGUI.PropertyField(areaRect, property.FindPropertyRelative("area"), GUIContent.none);

		// Set indent back to what it was
		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty();
	}
}
