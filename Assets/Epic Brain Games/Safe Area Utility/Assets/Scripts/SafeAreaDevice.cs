using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SafeAreaDevice", menuName = "EpicBrainGames/SafeAreaUtility/SafeAreaDevice", order = 1)]
public class SafeAreaDevice : ScriptableObject
{
	/// <summary>
	/// The display name - a user friendly name for the device.
	/// </summary>
	public string DisplayName = null;
	/// <summary>
	/// The orientation of the device.
	/// </summary>
	public ScreenOrientation Orientation = ScreenOrientation.Portrait;
	/// <summary>
	/// The device model name, a device might have more then one model name.
	/// This is used for device overrides.
	/// </summary>
	public List<string> ModelNames = null;

	[Space]
	[Header("Screen Resolution")]
	public int Width = 0;
	public int Height = 0;

	[Space]
	[Header("Screen Unsafe Areas")]
	public int UnsafeAreaTop = 0;
	public int UnsafeAreaBottom = 0;
	public int UnsafeAreaLeft = 0;
	public int UnsafeAreaRight = 0;

	[Space]
	[Header("Screen Cutouts")]
	public Rect[] Cutouts = new Rect[0];

	public Rect GetSafeArea()
	{
		return GetSafeArea(Rect.zero);
	}

	/// <summary>
	/// Gets the safe area for this device
	/// </summary>
	/// <returns>The safe area.</returns>
	/// <param name="safeArea">Rect to reuse</param>
	public Rect GetSafeArea(Rect safeArea)
	{
		safeArea.x = UnsafeAreaLeft;
		safeArea.width = Width - UnsafeAreaLeft - UnsafeAreaRight;

		safeArea.y = UnsafeAreaBottom;
		safeArea.height = Height - UnsafeAreaTop - UnsafeAreaBottom;

		return safeArea;
	}

#if UNITY_EDITOR
	void OnValidate()
	{
		// Reset the values to recalculate unsafe areas
		SafeAreaUtility.ResetSafeArea();
	}
#endif

}
