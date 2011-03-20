#region Using Statements
using System;
#endregion

namespace ManicDigger.MapTools
{
    /// <summary>
    /// Provides methods that assist when dealing with noise.
    /// </summary>
    public static class NoiseTools
    {

        /// <summary>
        /// Returns number between -1 and 1.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static double FindNoise2(double x, double y, int seed)
        {
            int n = (int)x + (int)y * 57;
            return FindNoise1(n, seed);
        }

        /// <summary>
        /// Finds the next noise.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static double FindNoise1(int n, int seed)
        {
            n += seed;
            n = (n << 13) ^ n;
            int nn = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
            return 1.0 - ((double)nn / 1073741824.0);
        }

        /// <summary>
        /// Interpolates using a, b and x.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Interpolate(double a, double b, double x)
        {
            double ft = x * 3.1415927;
            double f = (1.0 - Math.Cos(ft)) * 0.5;
            return a * (1.0 - f) + b * f;
        }

        /// <summary>
        /// Runs the Noise function for the given x, y and seed values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static double Noise(double x, double y, int seed)
        {
            double floorx = (double)((int)x);//This is kinda a cheap way to floor a double integer.
            double floory = (double)((int)y);
            double s, t, u, v;//Integer declaration
            s = FindNoise2(floorx, floory, seed);
            t = FindNoise2(floorx + 1, floory, seed);
            u = FindNoise2(floorx, floory + 1, seed);//Get the surrounding pixels to calculate the transition.
            v = FindNoise2(floorx + 1, floory + 1, seed);
            double int1 = Interpolate(s, t, x - floorx);//Interpolate between the values.
            double int2 = Interpolate(u, v, x - floorx);//Here we use x-floorx, to get 1st dimension. Don't mind the x-floorx thingie, it's part of the cosine formula.
            return Interpolate(int1, int2, y - floory);//Here we use y-floory, to get the 2nd dimension.
        }

        /// <summary>
        /// Returns the interpolated three-dimensional noise for the given values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="chunksize"></param>
        /// <returns></returns>
        public static double[, ,] InterpolateNoise3d(double x, double y, double z, int chunksize)
        {
            double[, ,] noise = new double[chunksize, chunksize, chunksize];/*
            for (int xx = 0; xx < chunksize; xx += 1)
            {
                for (int yy = 0; yy < chunksize; yy += 1)
                {
                    for (int zz = 0; zz < chunksize; zz += 1)
                    {
                        noise[xx, yy, zz] = GetNoise(x + xx, y + yy, z + zz);
                    }
                }
            }
            return noise;*/
            int n = 8;
            for (int xx = 0; xx < chunksize; xx += n)
            {
                for (int yy = 0; yy < chunksize; yy += n)
                {
                    for (int zz = 0; zz < chunksize; zz += n)
                    {
                        double f000 = GetNoise(x + xx, y + yy, z + zz);
                        double f100 = GetNoise(x + xx + (n - 1), y + yy, z + zz);
                        double f010 = GetNoise(x + xx, y + yy + (n - 1), z + zz);
                        double f110 = GetNoise(x + xx + (n - 1), y + yy + (n - 1), z + zz);
                        double f001 = GetNoise(x + xx, y + yy, z + zz + (n - 1));
                        double f101 = GetNoise(x + xx + (n - 1), y + yy, z + zz + (n - 1));
                        double f011 = GetNoise(x + xx, y + yy + (n - 1), z + zz + (n - 1));
                        double f111 = GetNoise(x + xx + (n - 1), y + yy + (n - 1), z + zz + (n - 1));
                        for (int ix = 0; ix < n; ix++)
                        {
                            for (int iy = 0; iy < n; iy++)
                            {
                                for (int iz = 0; iz < n; iz++)
                                {
                                    noise[xx + ix, yy + iy, zz + iz] = Trilinear((double)ix / (n - 1), (double)iy / (n - 1), (double)iz / (n - 1),
                                        f000, f010, f100, f110, f001, f011, f101, f111);

                                    //noise[xx + ix, yy + iy, zz + iz] = f000;
                                }
                            }
                        }/*
                        noise[xx + 0, yy + 0, zz + 0] = f000;
                        noise[xx + 1, yy + 0, zz + 0] = f100;
                        noise[xx + 0, yy + 1, zz + 0] = f010;
                        noise[xx + 1, yy + 1, zz + 0] = f110;
                        noise[xx + 0, yy + 0, zz + 1] = f001;
                        noise[xx + 1, yy + 0, zz + 1] = f101;
                        noise[xx + 0, yy + 1, zz + 1] = f011;
                        noise[xx + 1, yy + 1, zz + 1] = f111;*/
                    }
                }
            }
            return noise;
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="f000"></param>
        /// <param name="f010"></param>
        /// <param name="f100"></param>
        /// <param name="f110"></param>
        /// <param name="f001"></param>
        /// <param name="f011"></param>
        /// <param name="f101"></param>
        /// <param name="f111"></param>
        /// <returns></returns>
        public static double Trilinear(double x, double y, double z,
            double f000, double f010, double f100, double f110,
            double f001, double f011, double f101, double f111)
        {
            double up0 = (f100 - f000) * x + f000;
            double down0 = (f110 - f010) * x + f010;
            double all0 = (down0 - up0) * y + up0;

            double up1 = (f101 - f001) * x + f001;
            double down1 = (f111 - f011) * x + f011;
            double all1 = (down1 - up1) * y + up1;

            return (all1 - all0) * z + all0;
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static double GetNoise(double x, double y, double z)
        {
            return Noise3.noise((double)(x) / 50, (double)(y) / 50, (double)(z) / 50);
        }
    }

    /// <summary>
    /// Ken Perlin, Noise hardware. In Real-Time Shading SIGGRAPH Course Notes (2001), Olano M., (Ed.).
    /// http://www.csee.umbc.edu/~olano/s2002c36/ch02.pdf
    /// </summary>
    public static class Noise3
    {
        static int i, j, k;
        static int[] A = new[] { 0, 0, 0 };
        static double u, v, w;
        public static double noise(double x, double y, double z)
        {
            double s = (x + y + z) / 3;
            i = (int)Math.Floor(x + s); j = (int)Math.Floor(y + s); k = (int)Math.Floor(z + s);
            s = (i + j + k) / 6.0; u = x - i + s; v = y - j + s; w = z - k + s;
            A[0] = A[1] = A[2] = 0;
            int hi = u >= w ? u >= v ? 0 : 1 : v >= w ? 1 : 2;
            int lo = u < w ? u < v ? 0 : 1 : v < w ? 1 : 2;
            return K(hi) + K(3 - hi - lo) + K(lo) + K(0);
        }
        static double K(int a)
        {
            double s = (A[0] + A[1] + A[2]) / 6.0;
            double x = u - A[0] + s, y = v - A[1] + s, z = w - A[2] + s, t = .6 - x * x - y * y - z * z;
            int h = shuffle(i + A[0], j + A[1], k + A[2]);
            A[a]++;
            if (t < 0)
                return 0;
            int b5 = h >> 5 & 1, b4 = h >> 4 & 1, b3 = h >> 3 & 1, b2 = h >> 2 & 1, b = h & 3;
            double p = b == 1 ? x : b == 2 ? y : z, q = b == 1 ? y : b == 2 ? z : x, r = b == 1 ? z : b == 2 ? x : y;
            p = (b5 == b3 ? -p : p); q = (b5 == b4 ? -q : q); r = (b5 != (b4 ^ b3) ? -r : r);
            t *= t;
            return 8 * t * t * (p + (b == 0 ? q + r : b2 == 0 ? q : r));
        }
        static int shuffle(int i, int j, int k)
        {
            return b(i, j, k, 0) + b(j, k, i, 1) + b(k, i, j, 2) + b(i, j, k, 3) +
                   b(j, k, i, 4) + b(k, i, j, 5) + b(i, j, k, 6) + b(j, k, i, 7);
        }
        static int b(int i, int j, int k, int B) { return T[b(i, B) << 2 | b(j, B) << 1 | b(k, B)]; }
        static int b(int N, int B) { return N >> B & 1; }
        static int[] T = new[] { 0x15, 0x38, 0x32, 0x2c, 0x0d, 0x13, 0x07, 0x2a };
    }
}
