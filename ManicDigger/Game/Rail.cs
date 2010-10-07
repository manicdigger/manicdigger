using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger
{
    [Flags]
    public enum RailDirectionFlags : byte
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        UpLeft = 4,
        UpRight = 8,
        DownLeft = 16,
        DownRight = 32,

        Full = Horizontal | Vertical | UpLeft | UpRight | DownLeft | DownRight,


        TwoHorizontalVertical = Horizontal | Vertical,
        TwoHorizontalUpLeft = Horizontal | UpLeft,
        TwoHorizontalUpRight = Horizontal | UpRight,
        TwoHorizontalDownLeft = Horizontal | DownLeft,
        TwoHorizontalDownRight = Horizontal | DownRight,

        TwoVerticalUpLeft = Vertical | UpLeft,
        TwoVerticalUpRight = Vertical | UpRight,
        TwoVerticalDownLeft = Vertical | DownLeft,
        TwoVerticalDownRight = Vertical | DownRight,

        TwoUpLeftUpRight = UpLeft | UpRight,
        TwoUpLeftDownLeft = UpLeft | DownLeft,

        TwoDisjointUpLeftDownRight = UpLeft | DownRight,

        TwoDownLeftDownRight = DownLeft | DownRight,

        TwoDisjointDownLeftUpRight = DownLeft | UpRight,


        ThreeHorizontalVerticalUpLeft = Horizontal | Vertical | UpLeft,
        ThreeHorizontalVerticalUpRight = Horizontal | Vertical | UpRight,
        ThreeHorizontalVerticalDownLeft = Horizontal | Vertical | DownLeft,
        ThreeHorizontalVerticalDownRight = Horizontal | Vertical | DownRight,

        ThreeHorizontalUpLeftUpRight = Horizontal | UpLeft | UpRight,
        ThreeHorizontalUpLeftDownLeft = Horizontal | UpLeft | DownLeft,
        ThreeHorizontalUpLeftDownRight = Horizontal | UpLeft | DownRight,

        ThreeVerticalUpLeftUpRight = Vertical | UpLeft | UpRight,
        ThreeVerticalUpLeftDownLeft = Vertical | UpLeft | DownLeft,
        ThreeVerticalUpLeftDownRight = Vertical | UpLeft | DownRight,

        ThreeUpLeftUpRightDownLeft = UpLeft | UpRight | DownLeft,
        ThreeUpLeftUpRightDownRight = UpLeft | UpRight | DownRight,
        ThreeUpRightDownLeftDownRight = UpRight | DownLeft | DownRight,

        Corners = UpLeft | UpRight | DownLeft | DownRight,
    }
    public enum RailDirection
    {
        Horizontal = 0,
        Vertical = 1,
        UpLeft = 2,
        UpRight = 3,
        DownLeft = 4,
        DownRight = 5,
    }
    public static class DirectionUtils
    {
        /// <summary>
        /// TileExitDirection.Left => TileEnterDirection.Left
        /// </summary>
        /// <param name="exit"></param>
        /// <returns></returns>
        public static TileEnterDirection ToEnter(TileExitDirection exit)
        {
            switch (exit)
            {
                case TileExitDirection.Down:
                    return TileEnterDirection.Down;
                case TileExitDirection.Left:
                    return TileEnterDirection.Left;
                case TileExitDirection.Right:
                    return TileEnterDirection.Right;
                case TileExitDirection.Up:
                    return TileEnterDirection.Up;
                default:
                    throw new Exception();
            }
        }
        public static bool IsDisjoint(SignalDirection18 a, SignalDirection18 b)
        {
            return AreTwoDisjointRailTracks((ToRailDirectionFlags(RailDirectionUnderSignals(a))
                | ToRailDirectionFlags(RailDirectionUnderSignals(b)))
                );
        }
        public static RailConfiguration GetRailConfiguration(RailDirectionFlags rail_direction)
        {
            if (rail_direction == RailDirectionFlags.None)
            {
                return RailConfiguration.NoRail;
            }
            int rail_count = 0;
            foreach (RailDirection dir in DirectionUtils.AllRailDirections)
            {
                if ((rail_direction & ToRailDirectionFlags(dir)) != 0)
                {
                    rail_count++;
                }
            }
            if (rail_count == 1)
            {
                return RailConfiguration.SingleRail;
            }
            if (AreTwoDisjointRailTracks(rail_direction))
            {
                return RailConfiguration.TwoDisjointRailTracks;
            }
            return RailConfiguration.JoinedTracks;
        }
        public static bool AreTwoDisjointRailTracks(RailDirectionFlags rail_direction)
        {
            return (
                (rail_direction == (RailDirectionFlags.UpLeft | RailDirectionFlags.DownRight))
                || (rail_direction == (RailDirectionFlags.DownLeft | RailDirectionFlags.UpRight))
                );
        }
        public static RailDirection RailDirectionUnderSignals(SignalDirection18 signalDirection)
        {
            switch (signalDirection)
            {
                case SignalDirection18.HorizontalBoth:
                    return RailDirection.Horizontal;
                case SignalDirection18.HorizontalLeft:
                    return RailDirection.Horizontal;
                case SignalDirection18.HorizontalRight:
                    return RailDirection.Horizontal;

                case SignalDirection18.VerticalBoth:
                    return RailDirection.Vertical;
                case SignalDirection18.VerticalDown:
                    return RailDirection.Vertical;
                case SignalDirection18.VerticalUp:
                    return RailDirection.Vertical;

                case SignalDirection18.UpLeftBoth:
                    return RailDirection.UpLeft;
                case SignalDirection18.UpLeftLeft:
                    return RailDirection.UpLeft;
                case SignalDirection18.UpLeftUp:
                    return RailDirection.UpLeft;

                case SignalDirection18.UpRightBoth:
                    return RailDirection.UpRight;
                case SignalDirection18.UpRightRight:
                    return RailDirection.UpRight;
                case SignalDirection18.UpRightUp:
                    return RailDirection.UpRight;

                case SignalDirection18.DownLeftBoth:
                    return RailDirection.DownLeft;
                case SignalDirection18.DownLeftDown:
                    return RailDirection.DownLeft;
                case SignalDirection18.DownLeftLeft:
                    return RailDirection.DownLeft;

                case SignalDirection18.DownRightBoth:
                    return RailDirection.DownRight;
                case SignalDirection18.DownRightDown:
                    return RailDirection.DownRight;
                case SignalDirection18.DownRightRight:
                    return RailDirection.DownRight;
                default:
                    throw new ArgumentOutOfRangeException("signalDirection");
            }
        }
        static SignalDirection18[] SignalsCycleHorizontal = new SignalDirection18[]
        {
            SignalDirection18.HorizontalBoth,
            SignalDirection18.HorizontalLeft,
            SignalDirection18.HorizontalRight,
        };
        static SignalDirection18[] SignalsCycleVertical = new SignalDirection18[]
        {
            SignalDirection18.VerticalBoth,
            SignalDirection18.VerticalDown,
            SignalDirection18.VerticalUp,
        };
        static SignalDirection18[] SignalsCycleUpLeft = new SignalDirection18[]
        {
            SignalDirection18.UpLeftBoth,
            SignalDirection18.UpLeftLeft,
            SignalDirection18.UpLeftUp,
        };
        static SignalDirection18[] SignalsCycleUpRight = new SignalDirection18[]
        {
            SignalDirection18.UpRightBoth,
            SignalDirection18.UpRightRight,
            SignalDirection18.UpRightUp,
        };
        static SignalDirection18[] SignalsCycleDownLeft = new SignalDirection18[]
        {
            SignalDirection18.DownLeftBoth,
            SignalDirection18.DownLeftDown,
            SignalDirection18.DownLeftLeft,
        };
        static SignalDirection18[] SignalsCycleDownRight = new SignalDirection18[]
        {
            SignalDirection18.DownRightBoth,
            SignalDirection18.DownRightDown,
            SignalDirection18.DownRightRight,
        };
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
        /// <summary>
        /// Get vehicle direction not including information
        /// about RailDirection. (for graphical image)
        /// </summary>
        /// <remarks>
        /// Can be helpful because out of 12 VehicleDirections,
        /// there are 4 vehicle image duplicates:
        /// VehicleDirection12.UpLeftUp and VehicleDirection12.DownRightRight
        /// use the same Direction8.UpRight vehicle image.
        /// </remarks>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Direction8 ToDirection8(VehicleDirection12 direction)
        {
            switch (direction)
            {
                case VehicleDirection12.HorizontalLeft:
                    return Direction8.Left;
                case VehicleDirection12.HorizontalRight:
                    return Direction8.Right;
                case VehicleDirection12.VerticalUp:
                    return Direction8.Up;
                case VehicleDirection12.VerticalDown:
                    return Direction8.Down;

                case VehicleDirection12.UpLeftUp:
                    return Direction8.UpRight;
                case VehicleDirection12.UpLeftLeft:
                    return Direction8.DownLeft;
                case VehicleDirection12.UpRightUp:
                    return Direction8.UpLeft;
                case VehicleDirection12.UpRightRight:
                    return Direction8.DownRight;

                case VehicleDirection12.DownLeftDown:
                    return Direction8.DownRight;
                case VehicleDirection12.DownLeftLeft:
                    return Direction8.UpLeft;
                case VehicleDirection12.DownRightDown:
                    return Direction8.DownLeft;
                case VehicleDirection12.DownRightRight:
                    return Direction8.UpRight;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }
        public static RailDirectionFlags ToRailDirectionFlags(RailDirection direction)
        {
            switch (direction)
            {
                case RailDirection.DownLeft:
                    return RailDirectionFlags.DownLeft;
                case RailDirection.DownRight:
                    return RailDirectionFlags.DownRight;
                case RailDirection.Horizontal:
                    return RailDirectionFlags.Horizontal;
                case RailDirection.UpLeft:
                    return RailDirectionFlags.UpLeft;
                case RailDirection.UpRight:
                    return RailDirectionFlags.UpRight;
                case RailDirection.Vertical:
                    return RailDirectionFlags.Vertical;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }
        public static IEnumerable<RailDirection> ToRailDirections(RailDirectionFlags rail)
        {
            foreach (RailDirection d in AllRailDirections)
            {
                if ((rail & ToRailDirectionFlags(d)) != 0)
                {
                    yield return d;
                }
            }
        }
        public static TileEnterDirection StartEnter(VehicleDirection12 direction)
        {
            switch (direction)
            {
                case VehicleDirection12.HorizontalLeft:
                    return TileEnterDirection.Right;
                case VehicleDirection12.HorizontalRight:
                    return TileEnterDirection.Left;
                case VehicleDirection12.VerticalUp:
                    return TileEnterDirection.Down;
                case VehicleDirection12.VerticalDown:
                    return TileEnterDirection.Up;

                case VehicleDirection12.DownLeftDown:
                    return TileEnterDirection.Left;
                case VehicleDirection12.DownLeftLeft:
                    return TileEnterDirection.Down;
                case VehicleDirection12.DownRightDown:
                    return TileEnterDirection.Right;
                case VehicleDirection12.DownRightRight:
                    return TileEnterDirection.Down;

                case VehicleDirection12.UpLeftLeft:
                    return TileEnterDirection.Up;
                case VehicleDirection12.UpLeftUp:
                    return TileEnterDirection.Left;
                case VehicleDirection12.UpRightRight:
                    return TileEnterDirection.Up;
                case VehicleDirection12.UpRightUp:
                    return TileEnterDirection.Right;

                default:
                    throw new Exception();
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
        public static VehicleDirection12 ToVehicleDirection12(Direction4 direction)
        {
            switch (direction)
            {
                case Direction4.Up:
                    return VehicleDirection12.VerticalUp;
                case Direction4.Down:
                    return VehicleDirection12.VerticalDown;
                case Direction4.Left:
                    return VehicleDirection12.HorizontalLeft;
                case Direction4.Right:
                    return VehicleDirection12.HorizontalRight;
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
        public static RailDirectionFlags ToRailDirectionFlags(IEnumerable<RailDirection> directions)
        {
            RailDirectionFlags rail = RailDirectionFlags.None;
            foreach (RailDirection dir in directions)
            {
                rail |= ToRailDirectionFlags(dir);
            }
            return rail;
        }
        public static IEnumerable<VehicleDirection12> ToVehicleDirections(RailDirection direction)
        {
            switch (direction)
            {
                case RailDirection.DownLeft:
                    yield return VehicleDirection12.DownLeftDown;
                    yield return VehicleDirection12.DownLeftLeft;
                    break;
                case RailDirection.DownRight:
                    yield return VehicleDirection12.DownRightDown;
                    yield return VehicleDirection12.DownRightRight;
                    break;
                case RailDirection.Horizontal:
                    yield return VehicleDirection12.HorizontalLeft;
                    yield return VehicleDirection12.HorizontalRight;
                    break;
                case RailDirection.UpLeft:
                    yield return VehicleDirection12.UpLeftLeft;
                    yield return VehicleDirection12.UpLeftUp;
                    break;
                case RailDirection.UpRight:
                    yield return VehicleDirection12.UpRightRight;
                    yield return VehicleDirection12.UpRightUp;
                    break;
                case RailDirection.Vertical:
                    yield return VehicleDirection12.VerticalDown;
                    yield return VehicleDirection12.VerticalUp;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }
        public static IEnumerable<TileEnterDirection> ToTileEnterDirections(RailDirection rail_direction)
        {
            switch (rail_direction)
            {
                case RailDirection.Horizontal:
                    yield return TileEnterDirection.Left;
                    yield return TileEnterDirection.Right;
                    break;
                case RailDirection.Vertical:
                    yield return TileEnterDirection.Left;
                    yield return TileEnterDirection.Right;
                    break;
                case RailDirection.DownLeft:
                    yield return TileEnterDirection.Down;
                    yield return TileEnterDirection.Left;
                    break;
                case RailDirection.DownRight:
                    yield return TileEnterDirection.Down;
                    yield return TileEnterDirection.Right;
                    break;
                case RailDirection.UpLeft:
                    yield return TileEnterDirection.Up;
                    yield return TileEnterDirection.Left;
                    break;
                case RailDirection.UpRight:
                    yield return TileEnterDirection.Up;
                    yield return TileEnterDirection.Right;
                    break;
                default:
                    throw new Exception();
            }
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
        public static VehicleDirection12 StraightToVehicleDirection12(TileExitDirection direction)
        {
            switch (direction)
            {
                case TileExitDirection.Up:
                    return VehicleDirection12.VerticalUp;
                case TileExitDirection.Down:
                    return VehicleDirection12.VerticalDown;
                case TileExitDirection.Left:
                    return VehicleDirection12.HorizontalLeft;
                case TileExitDirection.Right:
                    return VehicleDirection12.HorizontalRight;
                default:
                    throw new Exception();
            }
        }
        /// <summary>
        /// TileEnterDirection.Left -> TileEnterDirection.Right
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static TileEnterDirection BackEnter(TileEnterDirection direction)
        {
            switch (direction)
            {
                case TileEnterDirection.Down:
                    return TileEnterDirection.Up;
                case TileEnterDirection.Left:
                    return TileEnterDirection.Right;
                case TileEnterDirection.Right:
                    return TileEnterDirection.Left;
                case TileEnterDirection.Up:
                    return TileEnterDirection.Down;
                default:
                    throw new Exception();
            }
        }
        public static TileExitDirection BackExit(TileEnterDirection direction)
        {
            switch (direction)
            {
                case TileEnterDirection.Down:
                    return TileExitDirection.Up;
                case TileEnterDirection.Up:
                    return TileExitDirection.Down;
                case TileEnterDirection.Left:
                    return TileExitDirection.Right;
                case TileEnterDirection.Right:
                    return TileExitDirection.Left;
                default:
                    throw new ArgumentException("direction");
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
        public static IEnumerable<TileEnterDirection> AllTileEnterDirections
        {
            get
            {
                yield return TileEnterDirection.Down;
                yield return TileEnterDirection.Left;
                yield return TileEnterDirection.Right;
                yield return TileEnterDirection.Up;
            }
        }
    }
    public enum RailConfiguration : byte
    {
        NoRail,
        SingleRail,
        TwoDisjointRailTracks,
        JoinedTracks,
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
    /// Green HorizontalLeft signal allows train of VehicleDirection12.HorizontalRight to move.
    /// 18 directions because in addition to 12 one-way signals (same as vehicle directions)
    /// there are 6 two-way signals.
    /// </summary>
    /// <remarks>
    /// Sygnały na jednym tile można zapisać jako SignalDirection18 a, b;
    /// tylko wtedy None trzeba dodać?
    /// </remarks>
    public enum SignalDirection18 : byte
    {
        HorizontalLeft,
        HorizontalRight,
        HorizontalBoth,
        VerticalUp,
        VerticalDown,
        VerticalBoth,

        UpLeftUp,
        UpLeftLeft,
        UpLeftBoth,
        UpRightUp,
        UpRightRight,
        UpRightBoth,

        DownLeftDown,
        DownLeftLeft,
        DownLeftBoth,
        DownRightDown,
        DownRightRight,
        DownRightBoth,
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
    public enum Direction8
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        UpRight = 4,
        DownRight = 5,
        DownLeft = 6,
        UpLeft = 7,
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
