public class CameraMove
{
	internal bool TurnLeft;
	internal bool TurnRight;
	internal bool DistanceUp;
	internal bool DistanceDown;
	internal bool AngleUp;
	internal bool AngleDown;
	internal int MoveX;
	internal int MoveY;
	internal float Distance;
}

public class Kamera
{
	public Kamera()
	{
		one = 1;
		distance = 5;
		Angle = 45;
		MinimumDistance = 2;
		tt = 0;
		MaximumAngle = 89;
		MinimumAngle = 0;
		Center = new Vector3Ref();
	}
	float one;
	public void GetPosition(GamePlatform platform, Vector3Ref ret)
	{
		float cx = platform.MathCos(tt * one / 2) * GetFlatDistance(platform) + Center.X;
		float cy = platform.MathSin(tt * one / 2) * GetFlatDistance(platform) + Center.Z;
		ret.X = cx;
		ret.Y = Center.Y + GetCameraHeightFromCenter(platform);
		ret.Z = cy;
	}
	float distance;
	public float GetDistance() { return distance; }
	public void SetDistance(float value)
	{
		distance = value;
		if (distance < MinimumDistance)
		{
			distance = MinimumDistance;
		}
	}
	internal float Angle;
	internal float MinimumDistance;
	float GetCameraHeightFromCenter(GamePlatform platform)
	{
		return platform.MathSin(Angle * Game.GetPi() / 180) * distance;
	}
	float GetFlatDistance(GamePlatform platform)
	{
		return platform.MathCos(Angle * Game.GetPi() / 180) * distance;
	}
	internal Vector3Ref Center;
	internal float tt;
	public float GetT()
	{
		return tt;
	}
	public void SetT(float value)
	{
		tt = value;
	}
	public void TurnLeft(float p)
	{
		tt += p;
	}
	public void TurnRight(float p)
	{
		tt -= p;
	}
	public void Move(CameraMove camera_move, float p)
	{
		p *= 2;
		p *= 2;
		if (camera_move.TurnLeft)
		{
			TurnLeft(p);
		}
		if (camera_move.TurnRight)
		{
			TurnRight(p);
		}
		if (camera_move.DistanceUp)
		{
			SetDistance(GetDistance() + p);
		}
		if (camera_move.DistanceDown)
		{
			SetDistance(GetDistance() - p);
		}
		if (camera_move.AngleUp)
		{
			Angle += p * 10;
		}
		if (camera_move.AngleDown)
		{
			Angle -= p * 10;
		}
		SetDistance(camera_move.Distance);
		//if (MaximumAngle < MinimumAngle) { throw new Exception(); }
		SetValidAngle();
	}

	void SetValidAngle()
	{
		if (Angle > MaximumAngle) { Angle = MaximumAngle; }
		if (Angle < MinimumAngle) { Angle = MinimumAngle; }
	}

	internal int MaximumAngle;
	internal int MinimumAngle;

	public float GetAngle()
	{
		return Angle;
	}

	public void SetAngle(float value)
	{
		Angle = value;
	}

	public void GetCenter(Vector3Ref ret)
	{
		ret.X = Center.X;
		ret.Y = Center.Y;
		ret.Z = Center.Z;
	}

	public void TurnUp(float p)
	{
		Angle += p;
		SetValidAngle();
	}
}
