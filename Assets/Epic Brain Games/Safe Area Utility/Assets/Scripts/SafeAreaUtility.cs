using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SafeAreaUtility is mostly a static class.
/// It will lazily calculate the safe area size based on Unitys' SafeArea.
/// All SafeAreaAdjuster instances will then use this data - thus preventing
/// recalculations and improving performance.
/// 
/// Add SafeAreaUtility to a game object to:
///
/// 1) Automatically test for screen resolution/orintation changes. If detected,
/// the values will be recalculated and event will be raised forcing a recalculation
/// for all SafeAreaAdjuster. Note: if you know that the screen resolution/orintation
/// will not change during game play, no need to have SafeAreaUtility in your scene.
/// 
/// 2) Debuging: you can force and show spesific device SafeArea in the editor.
/// This will allow for fast iterations and debugging.
/// 
/// 3) Override SafeArea definitions with custom values per device.
/// 
/// See documentation for more detailes.
/// </summary>
[ExecuteInEditMode]
public class SafeAreaUtility : MonoBehaviour
{
	static SafeAreaUtility s_Instance = null;
	static SafeAreaDevice s_CurrentSafeAreaDevice = null;

	static bool s_Initialized;
	static bool s_InitializedFirst = false;
	static bool s_DeviceOverrideInitialized = false;
	static Rect s_SafeAreaRect = Rect.zero;
	static int s_KnownWidth;
	static int s_KnownHeight;
	static ScreenOrientation s_KnownOrientation;

	static int s_UnsafeSizeTop;
	static int s_UnsafeSizeBottom;
	static int s_UnsafeSizeLeft;
	static int s_UnsafeSizeRight;

	static int s_TightUnsafeSizeTop;
	static int s_TightUnsafeSizeBottom;
	static int s_TightUnsafeSizeLeft;
	static int s_TightUnsafeSizeRight;

	[Header("Custom SafeArea Definitions")]
	[SerializeField] List<SafeAreaDevice> m_DeviceOverrides = null;

#if UNITY_EDITOR
	[Header("Debug (Editor Only)")]
	[SerializeField] SafeAreaDevice m_EmulatedDevice = null;
	[SerializeField] bool m_UnsafeAreaShow = true;
	[SerializeField] Color m_UnsafeAreaColor = new Color(1f, 0, 0, 0.6f);
	[SerializeField] bool m_TightUnsafeAreaShow = true;
	[SerializeField] Color m_TightUnsafeAreaColor = new Color(.6f, 0f, 1f, 0.6f);
	[SerializeField] bool m_CutoffAreaShow = true;
	[SerializeField] Color m_CutoffAreaColor = new Color(0, 0, 0, 0.8f);

	Texture2D m_RectTexture;
	GUIStyle m_RectStyle;
	int m_ErrorMessagePrintCountdown = 3;

	Action m_UpdatePostAction;
#endif

	private void Awake()
	{
		if (Application.isPlaying)
		{
			// Singleton pattern
			if (s_Instance != null && s_Instance != this)
			{

				Destroy(gameObject);
				return;
			}
			s_Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		InitializeDeviceOverride();
	}

	/// <summary>
	/// Resets the device override and forces a new initialization.
	/// </summary>
	private void ResetDeviceOverride()
	{
		s_DeviceOverrideInitialized = false;
		InitializeDeviceOverride();
	}

	/// <summary>
	/// Initializes the device override to force spesific SafeArea values
	/// based on the override devices list defined in the inspector.
	/// 
	/// SafeAreaDevice will be used as an override only if it matches the 
	/// current device model and screen orintation.
	/// </summary>
	void InitializeDeviceOverride()
	{
		if (s_DeviceOverrideInitialized)
		{
			// Already initialized
			return;
		}

		// Mark as initialized - even if we don't find an override
		s_DeviceOverrideInitialized = true;
		if (m_DeviceOverrides == null)
		{
			// No overrides defined
			return;
		}

		// Check if we want to override this device model
		string thisDeviceModel = SystemInfo.deviceModel;
		ScreenOrientation screenOrientation = Screen.orientation;
		foreach (var device in m_DeviceOverrides)
		{
			if (screenOrientation != device.Orientation)
			{
				// This device is not relevant - Orientation missmatch
				continue;
			}

			var deviceModelNames = device.ModelNames;
			if (deviceModelNames == null)
			{
				// This device is not relevant - No model names available
				continue;
			}

			foreach (var deviceModelName in deviceModelNames)
			{
				if (deviceModelName != null && deviceModelName.Equals(thisDeviceModel))
				{
					// This is a valid override, use it
					s_CurrentSafeAreaDevice = device;
					// Only reset if we are already initialized (Lazy initialization)
					if (s_Initialized)
					{
						ResetSafeArea();
					}
					return;
				}
			}
		}
	}

	/// <summary>
	/// Unity Update, used to check a change in the screen resolution / orintation.
	/// Such a change will force a reset to the safe area values.
	/// </summary>
	void Update()
	{
		if (s_KnownWidth != Screen.width || s_KnownHeight != Screen.height || s_KnownOrientation != Screen.orientation)
		{
			if (s_KnownOrientation != Screen.orientation)
			{
				// Device override depends on the orientation
				// Since it was changed, we need to reset it.
				ResetDeviceOverride();
			}

			ResetSafeArea();
		}

#if UNITY_EDITOR
		if (m_UpdatePostAction != null)
		{
			m_UpdatePostAction();
			m_UpdatePostAction = null;
		}
#endif
	}

#if UNITY_EDITOR
	void Reset()
	{
		s_CurrentSafeAreaDevice = null;
		ResetSafeArea();
	}

	void OnValidate()
	{
		m_ErrorMessagePrintCountdown = 3;
		if (gameObject.activeInHierarchy)
		{
			StartCoroutine(ValidateSafeAreaDevice(.1f));
		}

	}

	System.Collections.IEnumerator ValidateSafeAreaDevice(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);

		if (m_EmulatedDevice != s_CurrentSafeAreaDevice)
		{
			bool screenSizeValid = VerifyScreenSize(m_EmulatedDevice);
			if (screenSizeValid)
			{
				s_CurrentSafeAreaDevice = m_EmulatedDevice;
				m_UpdatePostAction = ResetSafeArea;
				m_ErrorMessagePrintCountdown = 3;
			}
			else
			{
				// Try Again Later
				StartCoroutine(ValidateSafeAreaDevice(.3f));
			}
		}
	}

	/// <summary>
	/// Verifies that the size of the game view screen matches the given device resolution.
	/// </summary>
	/// <returns><c>true</c>, if screen size was verifyed, <c>false</c> otherwise.</returns>
	/// <param name="device">Device to test</param>
	bool VerifyScreenSize(SafeAreaDevice device)
	{
		if (device == null)
		{
			// No requirement if we don't have a spesific device
			return true;
		}

		string[] resWidthAndHeight = UnityStats.screenRes.Split('x');

		int screenWidth = Convert.ToInt32(resWidthAndHeight[0]);
		int screenHeight = Convert.ToInt32(resWidthAndHeight[1]);

		if (screenWidth == device.Width && screenHeight == device.Height)
		{
			// Device requirements are met
			m_ErrorMessagePrintCountdown = 3;
			return true;
		}

		// Game view does not match device resulotion
		m_ErrorMessagePrintCountdown--;
		if (m_ErrorMessagePrintCountdown == 0)
		{
			Debug.LogWarning("Change Game View resolution to match " + device.DisplayName + " (width=" + device.Width + ", height=" + device.Height + ")");
		}
		return false;
	}

	void OnGUI()
	{
		if (m_EmulatedDevice == null)
		{
			return;
		}

		float screenHeight = Screen.height;
		float screenWidth = Screen.width;

		if (m_UnsafeAreaShow)
		{
			if (!VerifyScreenSize(m_EmulatedDevice))
			{
				// Not valid, don't draw
				return;
			}

			// Draw Safe Area
			int topSize = UnsafeSizeTop;
			int bottomSize = UnsafeSizeBottom;
			int leftSize = UnsafeSizeLeft;
			int rightSize = UnsafeSizeRight;

			// Top Unsafe Area
			GUIDrawUnsafeArea(new Rect(0, 0, screenWidth, topSize));

			// Bottom Unsafe Area
			GUIDrawUnsafeArea(new Rect(0, screenHeight - bottomSize, screenWidth, bottomSize));

			// Left Unsafe Area
			GUIDrawUnsafeArea(new Rect(0, topSize, leftSize, screenHeight - topSize - bottomSize));

			// Right Safe Area
			GUIDrawUnsafeArea(new Rect(screenWidth - rightSize, topSize, rightSize, screenHeight - topSize - bottomSize));
		}

		if (m_TightUnsafeAreaShow)
		{
			if (!VerifyScreenSize(m_EmulatedDevice))
			{
				// Not valid, don't draw
				return;
			}

			// Draw Safe Area
			int topSize = TightUnsafeSizeTop;
			int bottomSize = TightUnsafeSizeBottom;
			int leftSize = TightUnsafeSizeLeft;
			int rightSize = TightUnsafeSizeRight;

			// Top Unsafe Area
			GUIDrawTightUnsafeArea(new Rect(0, 0, screenWidth, topSize));

			// Bottom Unsafe Area
			GUIDrawTightUnsafeArea(new Rect(0, screenHeight - bottomSize, screenWidth, bottomSize));

			// Left Unsafe Area
			GUIDrawTightUnsafeArea(new Rect(0, topSize, leftSize, screenHeight - topSize - bottomSize));

			// Right Safe Area
			GUIDrawTightUnsafeArea(new Rect(screenWidth - rightSize, topSize, rightSize, screenHeight - topSize - bottomSize));
		}

		if (m_CutoffAreaShow)
		{
			if (!VerifyScreenSize(m_EmulatedDevice))
			{
				// Not valid, don't draw
				return;
			}

			var cutouts = m_EmulatedDevice.Cutouts;
			if (cutouts.Length > 0)
			{
				foreach (var c in cutouts)
				{
					GUIDrawCutoff(new Rect(c.x, screenHeight - c.y - c.height, c.width, c.height));
				}
			}
		}
	}

	void GUIDrawUnsafeArea(Rect rect)
	{
		GUIDrawArea(rect, m_UnsafeAreaColor);
	}

	void GUIDrawTightUnsafeArea(Rect rect)
	{
		GUIDrawArea(rect, m_TightUnsafeAreaColor);
	}

	void GUIDrawCutoff(Rect rect)
	{
		GUIDrawArea(rect, m_CutoffAreaColor);
	}

	/// <summary>
	/// Draw a GUI box.
	/// </summary>
	/// <param name="position">The rect to draw</param>
	void GUIDrawArea(Rect position, Color color)
	{
		if (m_RectTexture == null || m_RectStyle == null)
		{
			m_RectTexture = new Texture2D(1, 1);
			m_RectStyle = new GUIStyle();
		}

		m_RectTexture.SetPixel(0, 0, color);
		m_RectTexture.Apply();
		m_RectStyle.normal.background = m_RectTexture;
		GUI.Box(position, GUIContent.none, m_RectStyle);
	}
#endif

	/// <summary>
	/// Resets the safe area.
	/// </summary>
	public static void ResetSafeArea()
	{
		s_Initialized = false;
		InitializeSafeArea();
	}

	/// <summary>
	/// Initializes the safe area values.
	/// </summary>
	static void InitializeSafeArea()
	{
		if (s_Initialized)
		{
			return;
		}

		// Collect Information
		int screenWidth = Screen.width;
		int screenHeight = Screen.height;
		ScreenOrientation screenOrientation = Screen.orientation;
		Rect safeArea = GetSafeArea(s_CurrentSafeAreaDevice);
		Rect[] cutouts = GetCutouts(s_CurrentSafeAreaDevice);

		// Setup unsafe areas
		s_UnsafeSizeTop = Mathf.RoundToInt(screenHeight - safeArea.yMax);
		s_UnsafeSizeBottom = Mathf.RoundToInt(safeArea.y);
		s_UnsafeSizeLeft = Mathf.RoundToInt(safeArea.x);
		s_UnsafeSizeRight = Mathf.RoundToInt(screenWidth - safeArea.xMax);

		// Setup tight unsafe areas
		s_TightUnsafeSizeTop = 0;
		s_TightUnsafeSizeBottom = 0;
		s_TightUnsafeSizeLeft = 0;
		s_TightUnsafeSizeRight = 0;
		if (cutouts != null && cutouts.Length > 0)
		{
			foreach (var c in cutouts)
			{
				// Check Top Area
				if (c.yMin >= safeArea.yMax && (screenHeight - c.yMin) > s_TightUnsafeSizeTop)
				{
					s_TightUnsafeSizeTop = Mathf.RoundToInt(screenHeight - c.yMin);
				}

				// Check Bottom Area
				if (c.yMax <= safeArea.yMin && c.yMax > s_TightUnsafeSizeBottom)
				{
					s_TightUnsafeSizeBottom = Mathf.RoundToInt(c.yMax);
				}

				// Check Right Area
				if (c.xMin >= safeArea.xMax && (screenWidth - c.xMin) > s_TightUnsafeSizeRight)
				{
					s_TightUnsafeSizeRight = Mathf.RoundToInt(screenWidth - c.xMin);
				}

				// Check Left Area
				if (c.xMax <= safeArea.xMin && c.xMax > s_TightUnsafeSizeLeft)
				{
					s_TightUnsafeSizeRight = Mathf.RoundToInt(c.xMin);
				}
			}
		}

		// Mark current setup, if it changes we will reset everything
		s_KnownWidth = screenWidth;
		s_KnownHeight = screenHeight;
		s_KnownOrientation = screenOrientation;
		s_Initialized = true;

		if (s_InitializedFirst)
		{
			DispatchOnSafeAreaChanged();
		}

		// Don't call the event on the first setup
		s_InitializedFirst = true;
	}

	/// <summary>
	/// Gets the safe area from the given <paramref name="device"/>, or the default Screen.safeArea
	/// </summary>
	/// <returns>The safe area rect</returns>
	/// <param name="device">Device</param>
	static Rect GetSafeArea(SafeAreaDevice device)
	{
		if (device == null)
		{
			// Use the default safe area
			return Screen.safeArea;
		}

		return device.GetSafeArea(s_SafeAreaRect);
	}

	/// <summary>
	/// Native Unity support only from 2019-2 or newer
	/// Returns a list of screen areas that are not functional for displaying content 
	/// Gets the the list from the given <paramref name="device"/>, or the default Screen.cutouts
	/// </summary>
	/// <returns>The safe area rect</returns>
	/// <param name="device">Device</param>
	static Rect[] GetCutouts(SafeAreaDevice device)
	{
		if (device == null)
		{
#if UNITY_2019_2_OR_NEWER
			// Use the default cutouts
			return Screen.cutouts;
#else
			return null;
#endif
		}

		return device.Cutouts;
	}

	/// <summary>
	/// Gets the height of the screen.
	/// </summary>
	/// <value>The height of the screen.</value>
	public static int ScreenHeight
	{
		get
		{
			InitializeSafeArea();
			return s_KnownHeight;
		}
	}

	/// <summary>
	/// Gets the width of the screen.
	/// </summary>
	/// <value>The width of the screen.</value>
	public static int ScreenWidth
	{
		get
		{
			InitializeSafeArea();
			return s_KnownWidth;
		}
	}

	/// <summary>
	/// Gets the unsafe size top.
	/// </summary>
	/// <value>The unsafe size top.</value>
	public static int UnsafeSizeTop
	{
		get
		{
			InitializeSafeArea();
			return s_UnsafeSizeTop;
		}
	}

	/// <summary>
	/// Gets the unsafe size bottom.
	/// </summary>
	/// <value>The unsafe size bottom.</value>
	public static int UnsafeSizeBottom
	{
		get
		{
			InitializeSafeArea();
			return s_UnsafeSizeBottom;
		}
	}

	/// <summary>
	/// Gets the unsafe size left.
	/// </summary>
	/// <value>The unsafe size left.</value>
	public static int UnsafeSizeLeft
	{
		get
		{
			InitializeSafeArea();
			return s_UnsafeSizeLeft;
		}
	}

	/// <summary>
	/// Gets the unsafe size right.
	/// </summary>
	/// <value>The unsafe size right.</value>
	public static int UnsafeSizeRight
	{
		get
		{
			InitializeSafeArea();
			return s_UnsafeSizeRight;
		}
	}

	/// <summary>
	/// Gets the tight unsafe size top.
	/// </summary>
	/// <value>The unsafe size top.</value>
	public static int TightUnsafeSizeTop
	{
		get
		{
			InitializeSafeArea();
			return s_TightUnsafeSizeTop;
		}
	}

	/// <summary>
	/// Gets the tight unsafe size bottom.
	/// </summary>
	/// <value>The unsafe size bottom.</value>
	public static int TightUnsafeSizeBottom
	{
		get
		{
			InitializeSafeArea();
			return s_TightUnsafeSizeBottom;
		}
	}

	/// <summary>
	/// Gets the tight unsafe size left.
	/// </summary>
	/// <value>The unsafe size left.</value>
	public static int TightUnsafeSizeLeft
	{
		get
		{
			InitializeSafeArea();
			return s_TightUnsafeSizeLeft;
		}
	}

	/// <summary>
	/// Gets the tight unsafe size right.
	/// </summary>
	/// <value>The unsafe size right.</value>
	public static int TightUnsafeSizeRight
	{
		get
		{
			InitializeSafeArea();
			return s_TightUnsafeSizeRight;
		}
	}

	#region Events

	public delegate void SafeAreaChanged();
	public static event SafeAreaChanged OnSafeAreaChanged;

	public static void DispatchOnSafeAreaChanged()
	{
		if (OnSafeAreaChanged != null)
		{
			OnSafeAreaChanged();
		}
	}

#endregion
}
