using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Renderers;

namespace ManicDigger
{
    public static class DirectionUtils
    {
        /// <summary>
        /// VehicleDirection12.UpRightRight -> returns Direction4.Right
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static TileExitDirection ResultExit(VehicleDirection12 direction)
        {
            switch (direction)
            {
                case VehicleDirection12.HorizontalLeft:
                    return TileExitDirection.Left;
                case VehicleDirection12.HorizontalRight:
                    return TileExitDirection.Right;
                case VehicleDirection12.VerticalUp:
                    return TileExitDirection.Up;
                case VehicleDirection12.VerticalDown:
                    return TileExitDirection.Down;

                case VehicleDirection12.UpLeftUp:
                    return TileExitDirection.Up;
                case VehicleDirection12.UpLeftLeft:
                    return TileExitDirection.Left;
                case VehicleDirection12.UpRightUp:
                    return TileExitDirection.Up;
                case VehicleDirection12.UpRightRight:
                    return TileExitDirection.Right;

                case VehicleDirection12.DownLeftDown:
                    return TileExitDirection.Down;
                case VehicleDirection12.DownLeftLeft:
                    return TileExitDirection.Left;
                case VehicleDirection12.DownRightDown:
                    return TileExitDirection.Down;
                case VehicleDirection12.DownRightRight:
                    return TileExitDirection.Right;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }
        public static RailDirection ToRailDirection(VehicleDirection12 direction)
        {
            switch (direction)
            {
                case VehicleDirection12.HorizontalLeft:
                    return RailDirection.Horizontal;
                case VehicleDirection12.HorizontalRight:
                    return RailDirection.Horizontal;
                case VehicleDirection12.VerticalUp:
                    return RailDirection.Vertical;
                case VehicleDirection12.VerticalDown:
                    return RailDirection.Vertical;

                case VehicleDirection12.UpLeftUp:
                    return RailDirection.UpLeft;
                case VehicleDirection12.UpLeftLeft:
                    return RailDirection.UpLeft;
                case VehicleDirection12.UpRightUp:
                    return RailDirection.UpRight;
                case VehicleDirection12.UpRightRight:
                    return RailDirection.UpRight;

                case VehicleDirection12.DownLeftDown:
                    return RailDirection.DownLeft;
                case VehicleDirection12.DownLeftLeft:
                    return RailDirection.DownLeft;
                case VehicleDirection12.DownRightDown:
                    return RailDirection.DownRight;
                case VehicleDirection12.DownRightRight:
                    return RailDirection.DownRight;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }
        public static int ToRailDirectionFlags(RailDirection direction)
        {
            switch (direction)
            {
                case RailDirection.DownLeft:
                    return (int)RailDirectionFlags.DownLeft;
                case RailDirection.DownRight:
                    return (int)RailDirectionFlags.DownRight;
                case RailDirection.Horizontal:
                    return (int)RailDirectionFlags.Horizontal;
                case RailDirection.UpLeft:
                    return (int)RailDirectionFlags.UpLeft;
                case RailDirection.UpRight:
                    return (int)RailDirectionFlags.UpRight;
                case RailDirection.Vertical:
                    return (int)RailDirectionFlags.Vertical;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }
        public static IEnumerable<RailDirection> ToRailDirections(int rail) // RailDirectionFlags
        {
            foreach (RailDirection d in AllRailDirections)
            {
                if ((rail & ToRailDirectionFlags(d)) != 0)
                {
                    yield return d;
                }
            }
        }
        public static VehicleDirection12 Reverse(VehicleDirection12 direction)
        {
            switch (direction)
            {
                case VehicleDirection12.HorizontalLeft:
                    return VehicleDirection12.HorizontalRight;
                case VehicleDirection12.HorizontalRight:
                    return VehicleDirection12.HorizontalLeft;
                case VehicleDirection12.VerticalUp:
                    return VehicleDirection12.VerticalDown;
                case VehicleDirection12.VerticalDown:
                    return VehicleDirection12.VerticalUp;

                case VehicleDirection12.UpLeftUp:
                    return VehicleDirection12.UpLeftLeft;
                case VehicleDirection12.UpLeftLeft:
                    return VehicleDirection12.UpLeftUp;
                case VehicleDirection12.UpRightUp:
                    return VehicleDirection12.UpRightRight;
                case VehicleDirection12.UpRightRight:
                    return VehicleDirection12.UpRightUp;

                case VehicleDirection12.DownLeftDown:
                    return VehicleDirection12.DownLeftLeft;
                case VehicleDirection12.DownLeftLeft:
                    return VehicleDirection12.DownLeftDown;
                case VehicleDirection12.DownRightDown:
                    return VehicleDirection12.DownRightRight;
                case VehicleDirection12.DownRightRight:
                    return VehicleDirection12.DownRightDown;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }
        public static VehicleDirection12Flags ToVehicleDirection12Flags(VehicleDirection12 direction)
        {
            switch (direction)
            {
                case VehicleDirection12.HorizontalLeft:
                    return VehicleDirection12Flags.HorizontalLeft;
                case VehicleDirection12.HorizontalRight:
                    return VehicleDirection12Flags.HorizontalRight;
                case VehicleDirection12.VerticalUp:
                    return VehicleDirection12Flags.VerticalUp;
                case VehicleDirection12.VerticalDown:
                    return VehicleDirection12Flags.VerticalDown;

                case VehicleDirection12.UpLeftUp:
                    return VehicleDirection12Flags.UpLeftUp;
                case VehicleDirection12.UpLeftLeft:
                    return VehicleDirection12Flags.UpLeftLeft;
                case VehicleDirection12.UpRightUp:
                    return VehicleDirection12Flags.UpRightUp;
                case VehicleDirection12.UpRightRight:
                    return VehicleDirection12Flags.UpRightRight;

                case VehicleDirection12.DownLeftDown:
                    return VehicleDirection12Flags.DownLeftDown;
                case VehicleDirection12.DownLeftLeft:
                    return VehicleDirection12Flags.DownLeftLeft;
                case VehicleDirection12.DownRightDown:
                    return VehicleDirection12Flags.DownRightDown;
                case VehicleDirection12.DownRightRight:
                    return VehicleDirection12Flags.DownRightRight;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }
        public static VehicleDirection12Flags ToVehicleDirection12Flags(IEnumerable<VehicleDirection12> directions)
        {
            VehicleDirection12Flags flags = VehicleDirection12Flags.None;
            foreach (VehicleDirection12 d in directions)
            {
                flags = flags | ToVehicleDirection12Flags(d);
            }
            return flags;
        }
        public static IEnumerable<VehicleDirection12> ToVehicleDirection12s(VehicleDirection12Flags directions)
        {
            foreach (VehicleDirection12 d in AllVehicleDirections)
            {
                if ((directions & ToVehicleDirection12Flags(d)) != 0)
                {
                    yield return d;
                }
            }
        }
        public static IEnumerable<RailDirection> AllRailDirections
        {
            get
            {
                yield return RailDirection.DownLeft;
                yield return RailDirection.DownRight;
                yield return RailDirection.Horizontal;
                yield return RailDirection.UpLeft;
                yield return RailDirection.UpRight;
                yield return RailDirection.Vertical;
            }
        }
        public static int ToRailDirectionFlags(IEnumerable<RailDirection> directions)
        {
            int rail = 0;
            foreach (RailDirection dir in directions)
            {
                rail |= ToRailDirectionFlags(dir);
            }
            return rail;
        }
        /// <summary>
        /// Enter at TileEnterDirection.Left -> yields VehicleDirection12.UpLeftUp,
        /// VehicleDirection12.HorizontalRight,
        /// VehicleDirection12.DownLeftDown
        /// </summary>
        /// <param name="enter_at"></param>
        /// <returns></returns>
        public static IEnumerable<VehicleDirection12> PossibleNewRails(TileEnterDirection enter_at)
        {
            switch (enter_at)
            {
                case TileEnterDirection.Left:
                    yield return VehicleDirection12.UpLeftUp;
                    yield return VehicleDirection12.HorizontalRight;
                    yield return VehicleDirection12.DownLeftDown;
                    break;
                case TileEnterDirection.Down:
                    yield return VehicleDirection12.DownLeftLeft;
                    yield return VehicleDirection12.VerticalUp;
                    yield return VehicleDirection12.DownRightRight;
                    break;
                case TileEnterDirection.Up:
                    yield return VehicleDirection12.UpLeftLeft;
                    yield return VehicleDirection12.VerticalDown;
                    yield return VehicleDirection12.UpRightRight;
                    break;
                case TileEnterDirection.Right:
                    yield return VehicleDirection12.UpRightUp;
                    yield return VehicleDirection12.HorizontalLeft;
                    yield return VehicleDirection12.DownRightDown;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("enter_at");
            }
        }
        public static IEnumerable<VehicleDirection12> AllVehicleDirections
        {
            get
            {
                yield return VehicleDirection12.HorizontalLeft;
                yield return VehicleDirection12.HorizontalRight;
                yield return VehicleDirection12.VerticalUp;
                yield return VehicleDirection12.VerticalDown;
                yield return VehicleDirection12.UpLeftLeft;
                yield return VehicleDirection12.UpLeftUp;
                yield return VehicleDirection12.UpRightRight;
                yield return VehicleDirection12.UpRightUp;
                yield return VehicleDirection12.DownLeftDown;
                yield return VehicleDirection12.DownLeftLeft;
                yield return VehicleDirection12.DownRightDown;
                yield return VehicleDirection12.DownRightRight;
            }
        }
        public static TileEnterDirection ResultEnter(TileExitDirection direction)
        {
            switch (direction)
            {
                case TileExitDirection.Up:
                    return TileEnterDirection.Down;
                case TileExitDirection.Down:
                    return TileEnterDirection.Up;
                case TileExitDirection.Left:
                    return TileEnterDirection.Right;
                case TileExitDirection.Right:
                    return TileEnterDirection.Left;
                default:
                    throw new Exception();
            }
        }
    }
    public enum TileExitDirection : byte
    {
        Up,
        Down,
        Left,
        Right,
    }
    public enum TileEnterDirection : byte
    {
        Up,
        Down,
        Left,
        Right,
    }
    /// <summary>
    /// Each RailDirection on tile can be traversed by train in two directions.
    /// </summary>
    /// <example>
    /// RailDirection.Horizontal -> VehicleDirection12.HorizontalLeft (vehicle goes left and decreases x position),
    /// and VehicleDirection12.HorizontalRight (vehicle goes right and increases x position).
    /// </example>
    public enum VehicleDirection12 : byte
    {
        HorizontalLeft,
        HorizontalRight,
        VerticalUp,
        VerticalDown,

        UpLeftUp,
        UpLeftLeft,
        UpRightUp,
        UpRightRight,

        DownLeftDown,
        DownLeftLeft,
        DownRightDown,
        DownRightRight,
    }
    public enum VehicleDirection12Flags
    {
        None = 0,
        HorizontalLeft = 1 << 0,
        HorizontalRight = 1 << 1,
        VerticalUp = 1 << 2,
        VerticalDown = 1 << 3,

        UpLeftUp = 1 << 4,
        UpLeftLeft = 1 << 5,
        UpRightUp = 1 << 6,
        UpRightRight = 1 << 7,

        DownLeftDown = 1 << 8,
        DownLeftLeft = 1 << 9,
        DownRightDown = 1 << 10,
        DownRightRight = 1 << 11,
    }
}
