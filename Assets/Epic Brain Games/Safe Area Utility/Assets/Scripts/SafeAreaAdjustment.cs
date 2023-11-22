using System;

/// <summary>
/// Adjustment definition used by <see cref="SafeAreaAdjuster"/>
/// </summary>
[Serializable]
public class SafeAreaAdjustment
{
	public enum Target
	{
		PositionX, PositionY,
		Height, Width,
		TopAnchor, BottomAnchor, RightAnchor, LeftAnchor
	}

	public enum Action
	{
		Plus, Minus
	}

	public enum Area
	{
		TopArea, BottomArea, LeftArea, RightArea,
		TightTopArea, TightBottomArea, TightLeftArea, TightRightArea,
	}

	public bool active = true;
	public Target target = Target.PositionY;
	public Action action = Action.Minus;
	public Area area = Area.TopArea;
}
