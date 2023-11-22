using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SafeAreaAdjuster))]
[CanEditMultipleObjects]
public class SafeAreaAdjusterEditor : Editor
{

	SafeAreaAdjuster m_Script;
	RectTransform m_RectTransform;

	SerializedProperty m_RunInEditMode;

	DrivenRectTransformTracker m_DrivenRectTransformTracker = new DrivenRectTransformTracker();

	void OnEnable()
	{
		// Setup the SerializedProperties.
		m_RunInEditMode = serializedObject.FindProperty("m_RunInEditMode");
		m_Script = (SafeAreaAdjuster)target;
		m_RectTransform = m_Script.gameObject.GetComponent<RectTransform>();
	}

	public override void OnInspectorGUI()
	{
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		DrawDefaultInspector();

		if (m_RunInEditMode.boolValue)
		{
			EditorGUILayout.HelpBox("While \"Run In Edit Mode\" is enabled you can't update the object transform.", MessageType.Warning);
		}

		// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// We only want to lock in the Repaint GUI event
		// otherwise we are going to get an alert in the console.
		if (Event.current.type == EventType.Repaint)
		{
			LockRectTransformIfNeeded();
		}
	}

	void LockRectTransformIfNeeded()
	{
		m_DrivenRectTransformTracker.Clear();
		Tools.hidden = false;
		if (m_RunInEditMode.boolValue)
		{
			Tools.hidden = true;
			m_DrivenRectTransformTracker.Add(this, m_RectTransform, DrivenTransformProperties.All);
		}
	}
}
