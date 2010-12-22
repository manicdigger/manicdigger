using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger
{
    public interface IInterpolation
    {
        object Interpolate(object a, object b, float progress);
    }
    public interface INetworkInterpolation
    {
        void AddNetworkPacket(object c, double time);
        object InterpolatedState(double time);
    }
    class NetworkInterpolation : INetworkInterpolation
    {
        struct Packet
        {
            public double timestamp;
            public object content;
        }
        public IInterpolation req { get; set; }
        public bool EXTRAPOLATE = false;
        public float DELAY = 0.2f;
        public float EXTRAPOLATION_TIME = 0.2f;
        List<Packet> received = new List<Packet>();
        public void AddNetworkPacket(object c, double time)
        {
            Packet p = new Packet();
            p.content = c;
            p.timestamp = time;
            received.Add(p);
            if (received.Count > 100)
            {
                received.RemoveRange(0, received.Count - 100);
            }
        }
        public object InterpolatedState(double time)
        {
            double curtime = time;
            double interpolationtime = curtime - DELAY;
            int p1;
            int p2;
            if (received.Count == 0)
            {
                return null;
            }
            object result;
            if (received.Count > 0 && interpolationtime < received[0].timestamp)
            {
                p1 = 0;
                p2 = 0;
            }
            //extrapolate
            else if (EXTRAPOLATE && (received.Count >= 2)
                && interpolationtime > received[received.Count - 1].timestamp)
            {
                p1 = received.Count - 2;
                p2 = received.Count - 1;
                interpolationtime = Math.Min(interpolationtime, received[received.Count - 1].timestamp + EXTRAPOLATION_TIME);
            }
            else
            {
                p1 = 0;
                for (int i = 0; i < received.Count; i++)
                {
                    if (received[i].timestamp <= interpolationtime)
                    {
                        p1 = i;
                    }
                }
                p2 = p1;
                if (received.Count - 1 > p1)
                {
                    p2++;
                }
            }
            if (p1 == p2)
            {
                result = received[p1].content;
            }
            else
            {
                result = req.Interpolate(received[p1].content, received[p2].content,
                    (float)((interpolationtime - received[p1].timestamp)
                    / (received[p2].timestamp - received[p1].timestamp)));
            }
            return result;
        }
    }
    public class AngleInterpolation
    {
        public static int InterpolateAngle256(int a, int b, float progress)
        {
            if (progress != 0 && b != a)
            {
                int diff = NormalizeAngle256(b - a);
                if (diff >= CircleHalf256)
                {
                    diff -= CircleFull256;
                }
                a += (int)(progress * diff);
            }
            return NormalizeAngle256(a);
        }
        static int CircleHalf256 = 256 / 2;
        static int CircleFull256 = 256;
        static private int NormalizeAngle256(int v)
        {
            return (v + int.MaxValue / 2) % 256;
        }
        public static double InterpolateAngle360(double a, double b, double progress)
        {
            if (progress != 0 && b != a)
            {
                double diff = NormalizeAngle360(b - a);
                if (diff >= CircleHalf360)
                {
                    diff -= CircleFull360;
                }
                a += (progress * diff);
            }
            return NormalizeAngle360(a);
        }
        static int CircleHalf360 = 360 / 2;
        static int CircleFull360 = 360;
        static private double NormalizeAngle360(double v)
        {
            return (v + ((int.MaxValue / 2) / 360) * 360) % 360;
        }
    }
}
