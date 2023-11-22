using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The safe area adjuster should be added to any UI object needs adjustments 
/// due to the device safe area.
/// 
/// After adding the compnenet, you can define custom adjustments based on your needs.
/// 
/// ExactMode: enabling this will factor the current canvas scalar with the SafeArea
/// size. This allows for exact positioning regardless of canvas scalar definition.
/// You may disable this option if exact positioning is not needed in order to save a
/// little performance.
/// 
/// Note: when <see cref="SafeAreaAdjuster"/> is active and RunInEditMode is enabled, the game object RectTransform is locked
/// for changes. This is to protected from unwanted adjustments. To update the object position,
/// first disable <see cref="SafeAreaAdjuster"/> script, then update the position, and then reenable.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaAdjuster : MonoBehaviour
{
	#region Component Settings
#if UNITY_EDITOR
	[Tooltip("Preview adjustments in the editor while in edit mode. When enabled you can't change the object transform.")]
	[SerializeField] bool m_RunInEditMode = false;
#else
	bool m_RunInEditMode = false;
#endif

	[Tooltip("Adjust values factored by canvas scalar")]
	[SerializeField] bool m_ExactMode = true;

	[Tooltip("Add one or more adjustments")]
	[SerializeField] List<SafeAreaAdjustment> m_Adjustments;
	#endregion

	#region Class Members
	RectTransform m_RectTransform;
	RectTransform m_CanvasRectTransform;
	int m_CanvasKnownHeight;
	int m_CanvasKnownWidth;

	// Backup variables used to restore original transform value
	[HideInInspector] [SerializeField] bool m_BackupRestoreRequired = false;
	[HideInInspector] [SerializeField] Vector3 m_BackupAnchoredPosition;
	[HideInInspector] [SerializeField] Vector2 m_BackupSizeDelta;
	[HideInInspector] [SerializeField] Vector2 m_BackupOffsetMin;
	[HideInInspector] [SerializeField] Vector2 m_BackupOffsetMax;
	[HideInInspector] [SerializeField] Vector2 m_BackupAnchorMin;
	[HideInInspector] [SerializeField] Vector2 m_BackupAnchorMax;
	[HideInInspector] [SerializeField] Quaternion m_BackupRotation;
	[HideInInspector] [SerializeField] Vector3 m_BackupScale;

	#endregion

	#region Unity Lifecycle
	void Awake()
	{
		m_RectTransform = GetComponent<RectTransform>();
	}

	void OnEnable()
	{
		SafeAreaUtility.OnSafeAreaChanged += Adjust;
		Adjust();
	}

	void Update()
	{
#if UNITY_EDITOR
		// This little hack is used to fight the console alert:
		// "SendMessage cannot be called during Awake, CheckConsistency, or OnValidate"
		// being sent by Unity for some calls made directly from OnValidate.
		// Only in editor, we simply "post" the action for the update loop. 
		if (m_UpdateAction != null)
		{
			m_UpdateAction.Invoke();
			m_UpdateAction = null;
		}
#endif

		if (m_ExactMode && m_CanvasRectTransform != null)
		{
			Rect canvasRect = m_CanvasRectTransform.rect;
			if (m_CanvasKnownHeight != (int)canvasRect.height || m_CanvasKnownWidth != (int)canvasRect.width)
			{
				Adjust();
			}
		}
	}

	void OnDisable()
	{
		SafeAreaUtility.OnSafeAreaChanged -= Adjust;
		RestoreTransform();
	}
	#endregion

	#region Unity Editor
#if UNITY_EDITOR

	Action m_UpdateAction = null;

	void Reset()
	{
		m_Adjustments = new List<SafeAreaAdjustment>()
		{
			new SafeAreaAdjustment()
		};
		m_UpdateAction = Adjust;
	}

	void OnValidate()
	{
		m_UpdateAction = Adjust;
	}

#endif
	#endregion

	#region Adjustment Functions
	void Adjust()
	{
		RestoreTransform();

#if UNITY_EDITOR
		if (!Application.isPlaying && !m_RunInEditMode)
		{ 
			// Don't update in edit mode
			return;
		}
#endif

		BackupTransform();

		// Lock Rect Transform
		float yFactor = 1f;
		float xFactor = 1f;
		if (m_ExactMode)
		{
			// In Exact Mode, we compansate for the canvas scalar change
			if (m_CanvasRectTransform == null)
			{
				CanvasScaler parentCanvasScalar = GetComponentInParent<CanvasScaler>();
				if (parentCanvasScalar != null)
				{
					m_CanvasRectTransform = parentCanvasScalar.GetComponent<RectTransform>();
				}
			}

			if (m_CanvasRectTransform != null)
			{
				Rect canvasRect = m_CanvasRectTransform.rect;
				m_CanvasKnownHeight = (int)canvasRect.height;
				m_CanvasKnownWidth = (int)canvasRect.width;
				if (m_CanvasKnownHeight > 0)
				{
					yFactor = canvasRect.height / SafeAreaUtility.ScreenHeight;
					xFactor = canvasRect.width / SafeAreaUtility.ScreenWidth;
				}
			}
		}

		foreach (var adjustment in m_Adjustments)
		{
			if (!adjustment.active)
			{
				continue;
			}

			// Get the change value
			float changeValue = 0f;
			switch (adjustment.area)
			{
				case SafeAreaAdjustment.Area.TopArea:
					changeValue = SafeAreaUtility.UnsafeSizeTop * yFactor;
					break;
				case SafeAreaAdjustment.Area.BottomArea:
					changeValue = SafeAreaUtility.UnsafeSizeBottom * yFactor;
					break;
				case SafeAreaAdjustment.Area.LeftArea:
					changeValue = SafeAreaUtility.UnsafeSizeLeft * xFactor;
					break;
				case SafeAreaAdjustment.Area.RightArea:
					changeValue = SafeAreaUtility.UnsafeSizeRight * xFactor;
					break;

				case SafeAreaAdjustment.Area.TightTopArea:
					changeValue = SafeAreaUtility.TightUnsafeSizeTop * yFactor;
					break;
				case SafeAreaAdjustment.Area.TightBottomArea:
					changeValue = SafeAreaUtility.TightUnsafeSizeBottom * yFactor;
					break;
				case SafeAreaAdjustment.Area.TightLeftArea:
					changeValue = SafeAreaUtility.TightUnsafeSizeLeft * xFactor;
					break;
				case SafeAreaAdjustment.Area.TightRightArea:
					changeValue = SafeAreaUtility.TightUnsafeSizeRight * xFactor;
					break;
			}

			// Update cahnge value based on the action + / -
			changeValue = (adjustment.action == SafeAreaAdjustment.Action.Plus) ? changeValue : -changeValue;

			// Apply value change on the target
			switch (adjustment.target)
			{
				case SafeAreaAdjustment.Target.PositionX:
					AdjustAnchoredPositionX(changeValue);
					break;
				case SafeAreaAdjustment.Target.PositionY:
					AdjustAnchoredPositionY(changeValue);
					break;
				case SafeAreaAdjustment.Target.Width:
					AdjustWidth(changeValue);
					break;
				case SafeAreaAdjustment.Target.Height:
					AdjustHeight(changeValue);
					break;
				case SafeAreaAdjustment.Target.TopAnchor:
					AdjustTopRightBottomLeft(changeValue, 0, 0, 0);
					break;
				case SafeAreaAdjustment.Target.RightAnchor:
					AdjustTopRightBottomLeft(0, changeValue, 0, 0);
					break;
				case SafeAreaAdjustment.Target.BottomAnchor:
					AdjustTopRightBottomLeft(0, 0, changeValue, 0);
					break;
				case SafeAreaAdjustment.Target.LeftAnchor:
					AdjustTopRightBottomLeft(0, 0, 0, changeValue);
					break;
			}
		}
	}

	void BackupTransform()
	{
		if (m_BackupRestoreRequired)
		{
			// Already have a backup
			return;
		}

#if UNITY_EDITOR
		if (!Application.isPlaying && !m_RunInEditMode)
		{
			// We are in edit mode, but we don't run in edit mode
			return;
		}
#endif

		m_BackupAnchoredPosition = m_RectTransform.anchoredPosition;
		m_BackupRotation = m_RectTransform.localRotation;
		m_BackupScale = m_RectTransform.localScale;
		m_BackupSizeDelta = m_RectTransform.sizeDelta;
		m_BackupOffsetMin = m_RectTransform.offsetMin;
		m_BackupOffsetMax = m_RectTransform.offsetMax;
		m_BackupAnchorMin = m_RectTransform.anchorMin;
		m_BackupAnchorMax = m_RectTransform.anchorMax;
		m_BackupRestoreRequired = true;
	}

	void RestoreTransform()
	{
		if (m_BackupRestoreRequired)
		{
			m_RectTransform.anchoredPosition = m_BackupAnchoredPosition;
			m_RectTransform.localRotation = m_BackupRotation;
			m_RectTransform.localScale = m_BackupScale;
			m_RectTransform.sizeDelta = m_BackupSizeDelta;
			m_RectTransform.offsetMin = m_BackupOffsetMin;
			m_RectTransform.offsetMax = m_BackupOffsetMax;
			m_RectTransform.anchorMin = m_BackupAnchorMin;
			m_RectTransform.anchorMax = m_BackupAnchorMax;
			m_BackupRestoreRequired = false;
		}
	}
#endregion

#region RectTransform Manipulations

	void AdjustWidth(float bySize)
	{
		var currentSizeDelta = m_RectTransform.sizeDelta;
		currentSizeDelta.x += bySize;
		m_RectTransform.sizeDelta = currentSizeDelta;
	}

	void AdjustHeight(float bySize)
	{
		var currentSizeDelta = m_RectTransform.sizeDelta;
		currentSizeDelta.y += bySize;
		m_RectTransform.sizeDelta = currentSizeDelta;
	}

	void AdjustAnchoredPositionX(float byValue)
	{
		var anchoredPosition = m_RectTransform.anchoredPosition;
		anchoredPosition.x += byValue;
		m_RectTransform.anchoredPosition = anchoredPosition;
	}

	void AdjustAnchoredPositionY(float byValue)
	{
		var anchoredPosition = m_RectTransform.anchoredPosition;
		anchoredPosition.y += byValue;
		m_RectTransform.anchoredPosition = anchoredPosition;
	}

	void AdjustTopRightBottomLeft(float byTop, float byRight, float byBottom, float byLeft)
	{
		var offsetMin = m_RectTransform.offsetMin;
		var offsetMax = m_RectTransform.offsetMax;

		offsetMin.x += byLeft;
		offsetMin.y += byBottom;

		offsetMax.x -= byRight;
		offsetMax.y -= byTop;

		m_RectTransform.offsetMin = offsetMin; // (left, bottom);
		m_RectTransform.offsetMax = offsetMax; // (right, top);
	}
#endregion
}
