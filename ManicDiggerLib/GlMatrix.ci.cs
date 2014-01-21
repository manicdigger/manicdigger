//glMatrix license:
//Copyright (c) 2013, Brandon Jones, Colin MacKenzie IV. All rights reserved.

//Redistribution and use in source and binary forms, with or without modification,
//are permitted provided that the following conditions are met:

//  * Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation 
//    and/or other materials provided with the distribution.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
//ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

/// 2x2 Matrix
public class Mat2
{
    /// Creates a new identity mat2
    /// Returns a new 2x2 matrix
    public static float[] Create()
    {
        float[] output = new float[4];
        output[0] = 1;
        output[1] = 0;
        output[2] = 0;
        output[3] = 1;
        return output;
    }

    /// Creates a new mat2 initialized with values from an existing matrix
    /// Returns a new 2x2 matrix
    public static float[] CloneIt(
        /// matrix to clone
        float[] a)
    {
        float[] output = new float[4];
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        return output;
    }

    /// Copy the values from one mat2 to another
    /// Returns output
    public static float[] Copy(
        /// the receiving matrix
        float[] output,
        /// the source matrix
        float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        return output;
    }

    /// Set a mat2 to the identity matrix
    /// Returns output
    public static float[] Identity_(
        /// the receiving matrix
        float[] output)
    {
        output[0] = 1;
        output[1] = 0;
        output[2] = 0;
        output[3] = 1;
        return output;
    }

    /// Transpose the values of a mat2
    /// Returns output
    public static float[] Transpose(
        /// the receiving matrix
        float[] output,
        /// the source matrix
        float[] a)
    {
        // If we are transposing ourselves we can skip a few steps but have to cache some values
        //if (output === a) {
        //    var a1 = a[1];
        //    output[1] = a[2];
        //    output[2] = a1;
        //} else {
        output[0] = a[0];
        output[1] = a[2];
        output[2] = a[1];
        output[3] = a[3];
        //}

        return output;
    }

    /// Inverts a mat2
    /// Returns output
    public static float[] Invert(
        /// the receiving matrix
        float[] output,
        /// the source matrix
        float[] a)
    {
        float a0 = a[0]; float a1 = a[1]; float a2 = a[2]; float a3 = a[3];

        // Calculate the determinant
        float det = a0 * a3 - a2 * a1;

        if (det == 0)
        {
            return null;
        }
        float one = 1;
        det = one / det;

        output[0] = a3 * det;
        output[1] = -a1 * det;
        output[2] = -a2 * det;
        output[3] = a0 * det;

        return output;
    }

    /// Calculates the adjugate of a mat2
    /// Returns output
    public static float[] Adjoint(
        /// the receiving matrix
        float[] output,
        /// the source matrix
        float[] a)
    {
        // Caching this value is nessecary if output == a
        float a0 = a[0];
        output[0] = a[3];
        output[1] = -a[1];
        output[2] = -a[2];
        output[3] = a0;

        return output;
    }

    /// Calculates the determinant of a mat2
    /// Returns determinant of a
    public static float Determinant(
        /// the source matrix
        float[] a)
    {
        return a[0] * a[3] - a[2] * a[1];
    }

    /// Multiplies two mat2's
    /// Returns output
    public static float[] Multiply(
        /// the receiving matrix
        float[] output,
        /// the first operand
        float[] a,
        /// the second operand
        float[] b)
    {
        float a0 = a[0]; float a1 = a[1]; float a2 = a[2]; float a3 = a[3];
        float b0 = b[0]; float b1 = b[1]; float b2 = b[2]; float b3 = b[3];
        output[0] = a0 * b0 + a1 * b2;
        output[1] = a0 * b1 + a1 * b3;
        output[2] = a2 * b0 + a3 * b2;
        output[3] = a2 * b1 + a3 * b3;
        return output;
    }

    /// Alias for {@link mat2.multiply}
    public static float[] Mul(float[] output, float[] a, float[] b)
    {
        return Multiply(output, a, b);
    }

    /// Rotates a mat2 by the given angle
    /// Returns output
    public static float[] Rotate(
        /// the receiving matrix
        float[] output,
        /// the matrix to rotate
        float[] a,
        /// the angle to rotate the matrix by
        float rad)
    {
        float a0 = a[0]; float a1 = a[1]; float a2 = a[2]; float a3 = a[3];
        float s = Platform.Sin(rad);
        float c = Platform.Cos(rad);
        output[0] = a0 * c + a1 * s;
        output[1] = a0 * -s + a1 * c;
        output[2] = a2 * c + a3 * s;
        output[3] = a2 * -s + a3 * c;
        return output;
    }

    /// Scales the mat2 by the dimensions in the given vec2
    /// Returns output
    public static float[] Scale(
        /// the receiving matrix
        float[] output,
        /// the matrix to rotate
        float[] a,
        /// the vec2 to scale the matrix by
        float[] v)
    {
        float a0 = a[0]; float a1 = a[1]; float a2 = a[2]; float a3 = a[3];
        float v0 = v[0]; float v1 = v[1];
        output[0] = a0 * v0;
        output[1] = a1 * v1;
        output[2] = a2 * v0;
        output[3] = a3 * v1;
        return output;
    }

    ///**
    // * Returns a string representation of a mat2
    // *
    // * @param {mat2} mat matrix to represent as a string
    // * @returns {String} string representation of the matrix
    // */
    //mat2.str = function (a) {
    //    return 'mat2(' + a[0] + ', ' + a[1] + ', ' + a[2] + ', ' + a[3] + ')';
    //};

    //if(typeof(exports) !== 'undefined') {
    //    exports.mat2 = mat2;
    //}
    void f()
    {
    }
}

public class Mat2d
{
    //    /**
    // * @class 2x3 Matrix
    // * @name mat2d
    // * 
    // * @description 
    // * A mat2d contains six elements defined as:
    // * <pre>
    // * [a, b,
    // *  c, d,
    // *  tx,ty]
    // * </pre>
    // * This is a short form for the 3x3 matrix:
    // * <pre>
    // * [a, b, 0
    // *  c, d, 0
    // *  tx,ty,1]
    // * </pre>
    // * The last column is ignored so the array is shorter and operations are faster.
    // */
    //var mat2d = {};

    /// Creates a new identity mat2d
    /// Returns a new 2x3 matrix
    public static float[] Create()
    {
        float[] output = new float[6];
        output[0] = 1;
        output[1] = 0;
        output[2] = 0;
        output[3] = 1;
        output[4] = 0;
        output[5] = 0;
        return output;
    }

    /// Creates a new mat2d initialized with values from an existing matrix
    /// Returns a new 2x3 matrix
    public static float[] CloneIt(
        /// matrix to clone
        float[] a)
    {
        float[] output = new float[6];
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        output[4] = a[4];
        output[5] = a[5];
        return output;
    }

    /// Copy the values from one mat2d to another
    /// Returns output
    public static float[] Copy(
        /// the receiving matrix
        float[] output,
        /// the source matrix
        float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        output[4] = a[4];
        output[5] = a[5];
        return output;
    }

    /// Set a mat2d to the identity matrix
    /// Returns output
    public static float[] Identity_(
        /// the receiving matrix
        float[] output)
    {
        output[0] = 1;
        output[1] = 0;
        output[2] = 0;
        output[3] = 1;
        output[4] = 0;
        output[5] = 0;
        return output;
    }

    /// Inverts a mat2d
    /// Returns output
    public static float[] Invert(
        /// the receiving matrix
        float[] output,
        /// the source matrix
        float[] a)
    {
        float aa = a[0]; float ab = a[1]; float ac = a[2]; float ad = a[3];
        float atx = a[4]; float aty = a[5];

        float det = aa * ad - ab * ac;
        if (det == 0)
        {
            return null;
        }
        float one = 1;
        det = one / det;

        output[0] = ad * det;
        output[1] = -ab * det;
        output[2] = -ac * det;
        output[3] = aa * det;
        output[4] = (ac * aty - ad * atx) * det;
        output[5] = (ab * atx - aa * aty) * det;
        return output;
    }

    /// Calculates the determinant of a mat2d
    /// Returns determinant of a
    public static float Determinant(
        /// the source matrix
        float[] a)
    {
        return a[0] * a[3] - a[1] * a[2];
    }

    /// Multiplies two mat2d's
    /// Returns output
    public static float[] Multiply(
        /// the receiving matrix
        float[] output,
        /// the first operand
        float[] a,
        /// the second operand
        float[] b)
    {
        float aa = a[0]; float ab = a[1]; float ac = a[2]; float ad = a[3];
        float atx = a[4]; float aty = a[5];
        float ba = b[0]; float bb = b[1]; float bc = b[2]; float bd = b[3];
        float btx = b[4]; float bty = b[5];

        output[0] = aa * ba + ab * bc;
        output[1] = aa * bb + ab * bd;
        output[2] = ac * ba + ad * bc;
        output[3] = ac * bb + ad * bd;
        output[4] = ba * atx + bc * aty + btx;
        output[5] = bb * atx + bd * aty + bty;
        return output;
    }

    /// Alias for {@link mat2d.multiply} @function
    public static float[] Mul(float[] output, float[] a, float[] b)
    {
        return Multiply(output, a, b);
    }


    /// Rotates a mat2d by the given angle
    /// Returns output
    public static float[] Rotate(
        /// the receiving matrix
        float[] output,
        /// the matrix to rotate
        float[] a,
        /// the angle to rotate the matrix by
        float rad)
    {
        float aa = a[0];
        float ab = a[1];
        float ac = a[2];
        float ad = a[3];
        float atx = a[4];
        float aty = a[5];
        float st = Platform.Sin(rad);
        float ct = Platform.Cos(rad);

        output[0] = aa * ct + ab * st;
        output[1] = -aa * st + ab * ct;
        output[2] = ac * ct + ad * st;
        output[3] = -ac * st + ct * ad;
        output[4] = ct * atx + st * aty;
        output[5] = ct * aty - st * atx;
        return output;
    }

    /// Scales the mat2d by the dimensions in the given vec2
    /// Returns output
    public static float[] Scale(
        /// the receiving matrix
        float[] output,
        /// the matrix to translate
        float[] a,
        /// the vec2 to scale the matrix by
        float[] v)
    {
        float vx = v[0]; float vy = v[1];
        output[0] = a[0] * vx;
        output[1] = a[1] * vy;
        output[2] = a[2] * vx;
        output[3] = a[3] * vy;
        output[4] = a[4] * vx;
        output[5] = a[5] * vy;
        return output;
    }

    /// Translates the mat2d by the dimensions in the given vec2
    /// Returns output
    public static float[] Translate(
        /// the receiving matrix
        float[] output,
        /// the matrix to translate
        float[] a,
        /// the vec2 to translate the matrix by
        float[] v)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        output[4] = a[4] + v[0];
        output[5] = a[5] + v[1];
        return output;
    }

    ///**
    // * Returns a string representation of a mat2d
    // *
    // * @param {mat2d} a matrix to represent as a string
    // * @returns {String} string representation of the matrix
    // */
    //mat2d.str = function (a) {
    //    return 'mat2d(' + a[0] + ', ' + a[1] + ', ' + a[2] + ', ' + 
    //                    a[3] + ', ' + a[4] + ', ' + a[5] + ')';
    //};

    //if(typeof(exports) !== 'undefined') {
    //    exports.mat2d = mat2d;
    //}

    void f()
    {
    }
}

public class Mat3
{
    //    /**
    // * @class 3x3 Matrix
    // * @name mat3
    // */
    //var mat3 = {};

    ///**
    // * Creates a new identity mat3
    // *
    // * @returns {mat3} a new 3x3 matrix
    // */
    public static float[] Create()
    {
        float[] output = new float[9];
        output[0] = 1;
        output[1] = 0;
        output[2] = 0;
        output[3] = 0;
        output[4] = 1;
        output[5] = 0;
        output[6] = 0;
        output[7] = 0;
        output[8] = 1;
        return output;
    }

    ///**
    // * Copies the upper-left 3x3 values into the given mat3.
    // *
    // * @param {mat3} output the receiving 3x3 matrix
    // * @param {mat4} a   the source 4x4 matrix
    // * @returns {mat3} output
    // */
    public static float[] FromMat4(float[] output, float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[4];
        output[4] = a[5];
        output[5] = a[6];
        output[6] = a[8];
        output[7] = a[9];
        output[8] = a[10];
        return output;
    }

    ///**
    // * Creates a new mat3 initialized with values from an existing matrix
    // *
    // * @param {mat3} a matrix to clone
    // * @returns {mat3} a new 3x3 matrix
    // */
    public static float[] CloneIt(float[] a)
    {
        float[] output = new float[9];
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        output[4] = a[4];
        output[5] = a[5];
        output[6] = a[6];
        output[7] = a[7];
        output[8] = a[8];
        return output;
    }

    ///**
    // * Copy the values from one mat3 to another
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat3} a the source matrix
    // * @returns {mat3} output
    // */
    public static float[] Copy(float[] output, float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        output[4] = a[4];
        output[5] = a[5];
        output[6] = a[6];
        output[7] = a[7];
        output[8] = a[8];
        return output;
    }

    ///**
    // * Set a mat3 to the identity matrix
    // *
    // * @param {mat3} output the receiving matrix
    // * @returns {mat3} output
    // */
    public static float[] Identity_(float[] output)
    {
        output[0] = 1;
        output[1] = 0;
        output[2] = 0;
        output[3] = 0;
        output[4] = 1;
        output[5] = 0;
        output[6] = 0;
        output[7] = 0;
        output[8] = 1;
        return output;
    }

    ///**
    // * Transpose the values of a mat3
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat3} a the source matrix
    // * @returns {mat3} output
    // */
    public static float[] Transpose(float[] output, float[] a)
    {
        // If we are transposing ourselves we can skip a few steps but have to cache some values
        if (output == a)
        {
            float a01 = a[1];
            float a02 = a[2];
            float a12 = a[5];
            output[1] = a[3];
            output[2] = a[6];
            output[3] = a01;
            output[5] = a[7];
            output[6] = a02;
            output[7] = a12;
        }
        else
        {
            output[0] = a[0];
            output[1] = a[3];
            output[2] = a[6];
            output[3] = a[1];
            output[4] = a[4];
            output[5] = a[7];
            output[6] = a[2];
            output[7] = a[5];
            output[8] = a[8];
        }

        return output;
    }

    ///**
    // * Inverts a mat3
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat3} a the source matrix
    // * @returns {mat3} output
    // */
    public static float[] Invert(float[] output, float[] a)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2];
        float a10 = a[3]; float a11 = a[4]; float a12 = a[5];
        float a20 = a[6]; float a21 = a[7]; float a22 = a[8];

        float b01 = a22 * a11 - a12 * a21;
        float b11 = -a22 * a10 + a12 * a20;
        float b21 = a21 * a10 - a11 * a20;

        // Calculate the determinant
        float det = a00 * b01 + a01 * b11 + a02 * b21;

        if (det == 0)
        {
            return null;
        }
        float one = 1;
        det = one / det;

        output[0] = b01 * det;
        output[1] = (-a22 * a01 + a02 * a21) * det;
        output[2] = (a12 * a01 - a02 * a11) * det;
        output[3] = b11 * det;
        output[4] = (a22 * a00 - a02 * a20) * det;
        output[5] = (-a12 * a00 + a02 * a10) * det;
        output[6] = b21 * det;
        output[7] = (-a21 * a00 + a01 * a20) * det;
        output[8] = (a11 * a00 - a01 * a10) * det;
        return output;
    }

    ///**
    // * Calculates the adjugate of a mat3
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat3} a the source matrix
    // * @returns {mat3} output
    // */
    public static float[] Adjoint(float[] output, float[] a)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2];
        float a10 = a[3]; float a11 = a[4]; float a12 = a[5];
        float a20 = a[6]; float a21 = a[7]; float a22 = a[8];

        output[0] = (a11 * a22 - a12 * a21);
        output[1] = (a02 * a21 - a01 * a22);
        output[2] = (a01 * a12 - a02 * a11);
        output[3] = (a12 * a20 - a10 * a22);
        output[4] = (a00 * a22 - a02 * a20);
        output[5] = (a02 * a10 - a00 * a12);
        output[6] = (a10 * a21 - a11 * a20);
        output[7] = (a01 * a20 - a00 * a21);
        output[8] = (a00 * a11 - a01 * a10);
        return output;
    }

    ///**
    // * Calculates the determinant of a mat3
    // *
    // * @param {mat3} a the source matrix
    // * @returns {Number} determinant of a
    // */
    public static float Determinant(float[] a)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2];
        float a10 = a[3]; float a11 = a[4]; float a12 = a[5];
        float a20 = a[6]; float a21 = a[7]; float a22 = a[8];

        return a00 * (a22 * a11 - a12 * a21) + a01 * (-a22 * a10 + a12 * a20) + a02 * (a21 * a10 - a11 * a20);
    }

    ///**
    // * Multiplies two mat3's
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat3} a the first operand
    // * @param {mat3} b the second operand
    // * @returns {mat3} output
    // */
    public static float[] Multiply(float[] output, float[] a, float[] b)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2];
        float a10 = a[3]; float a11 = a[4]; float a12 = a[5];
        float a20 = a[6]; float a21 = a[7]; float a22 = a[8];

        float b00 = b[0]; float b01 = b[1]; float b02 = b[2];
        float b10 = b[3]; float b11 = b[4]; float b12 = b[5];
        float b20 = b[6]; float b21 = b[7]; float b22 = b[8];

        output[0] = b00 * a00 + b01 * a10 + b02 * a20;
        output[1] = b00 * a01 + b01 * a11 + b02 * a21;
        output[2] = b00 * a02 + b01 * a12 + b02 * a22;

        output[3] = b10 * a00 + b11 * a10 + b12 * a20;
        output[4] = b10 * a01 + b11 * a11 + b12 * a21;
        output[5] = b10 * a02 + b11 * a12 + b12 * a22;

        output[6] = b20 * a00 + b21 * a10 + b22 * a20;
        output[7] = b20 * a01 + b21 * a11 + b22 * a21;
        output[8] = b20 * a02 + b21 * a12 + b22 * a22;
        return output;
    }

    ///**
    // * Alias for {@link mat3.multiply}
    // * @function
    // */
    public static float[] Mul(float[] output, float[] a, float[] b)
    {
        return Multiply(output, a, b);
    }
    ///**
    // * Translate a mat3 by the given vector
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat3} a the matrix to translate
    // * @param {vec2} v vector to translate by
    // * @returns {mat3} output
    // */
    public static float[] Translate(float[] output, float[] a, float[] v)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2];
        float a10 = a[3]; float a11 = a[4]; float a12 = a[5];
        float a20 = a[6]; float a21 = a[7]; float a22 = a[8];
        float x = v[0]; float y = v[1];

        output[0] = a00;
        output[1] = a01;
        output[2] = a02;

        output[3] = a10;
        output[4] = a11;
        output[5] = a12;

        output[6] = x * a00 + y * a10 + a20;
        output[7] = x * a01 + y * a11 + a21;
        output[8] = x * a02 + y * a12 + a22;
        return output;
    }

    ///**
    // * Rotates a mat3 by the given angle
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat3} a the matrix to rotate
    // * @param {Number} rad the angle to rotate the matrix by
    // * @returns {mat3} output
    // */
    public static float[] Rotate(float[] output, float[] a, float rad)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2];
        float a10 = a[3]; float a11 = a[4]; float a12 = a[5];
        float a20 = a[6]; float a21 = a[7]; float a22 = a[8];

        float s = Platform.Sin(rad);
        float c = Platform.Cos(rad);

        output[0] = c * a00 + s * a10;
        output[1] = c * a01 + s * a11;
        output[2] = c * a02 + s * a12;

        output[3] = c * a10 - s * a00;
        output[4] = c * a11 - s * a01;
        output[5] = c * a12 - s * a02;

        output[6] = a20;
        output[7] = a21;
        output[8] = a22;
        return output;
    }

    ///**
    // * Scales the mat3 by the dimensions in the given vec2
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat3} a the matrix to rotate
    // * @param {vec2} v the vec2 to scale the matrix by
    // * @returns {mat3} output
    // **/
    public static float[] Scale(float[] output, float[] a, float[] v)
    {
        float x = v[0]; float y = v[1];

        output[0] = x * a[0];
        output[1] = x * a[1];
        output[2] = x * a[2];

        output[3] = y * a[3];
        output[4] = y * a[4];
        output[5] = y * a[5];

        output[6] = a[6];
        output[7] = a[7];
        output[8] = a[8];
        return output;
    }

    ///**
    // * Copies the values from a mat2d into a mat3
    // *
    // * @param {mat3} output the receiving matrix
    // * @param {mat2d} a the matrix to copy
    // * @returns {mat3} output
    // **/
    public static float[] FromMat2d(float[] output, float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = 0;

        output[3] = a[2];
        output[4] = a[3];
        output[5] = 0;

        output[6] = a[4];
        output[7] = a[5];
        output[8] = 1;
        return output;
    }

    ///**
    //* Calculates a 3x3 matrix from the given quaternion
    //*
    //* @param {mat3} output mat3 receiving operation result
    //* @param {quat} q Quaternion to create matrix from
    //*
    //* @returns {mat3} output
    //*/
    public static float[] FromQuat(float[] output, float[] q)
    {
        float x = q[0]; float y = q[1]; float z = q[2]; float w = q[3];
        float x2 = x + x;
        float y2 = y + y;
        float z2 = z + z;

        float xx = x * x2;
        float xy = x * y2;
        float xz = x * z2;
        float yy = y * y2;
        float yz = y * z2;
        float zz = z * z2;
        float wx = w * x2;
        float wy = w * y2;
        float wz = w * z2;

        output[0] = 1 - (yy + zz);
        output[3] = xy + wz;
        output[6] = xz - wy;

        output[1] = xy - wz;
        output[4] = 1 - (xx + zz);
        output[7] = yz + wx;

        output[2] = xz + wy;
        output[5] = yz - wx;
        output[8] = 1 - (xx + yy);

        return output;
    }

    ///**
    //* Calculates a 3x3 normal matrix (transpose inverse) from the 4x4 matrix
    //*
    //* @param {mat3} output mat3 receiving operation result
    //* @param {mat4} a Mat4 to derive the normal matrix from
    //*
    //* @returns {mat3} output
    //*/
    public static float[] NormalFromMat4(float[] output, float[] a)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2]; float a03 = a[3];
        float a10 = a[4]; float a11 = a[5]; float a12 = a[6]; float a13 = a[7];
        float a20 = a[8]; float a21 = a[9]; float a22 = a[10]; float a23 = a[11];
        float a30 = a[12]; float a31 = a[13]; float a32 = a[14]; float a33 = a[15];

        float b00 = a00 * a11 - a01 * a10;
        float b01 = a00 * a12 - a02 * a10;
        float b02 = a00 * a13 - a03 * a10;
        float b03 = a01 * a12 - a02 * a11;
        float b04 = a01 * a13 - a03 * a11;
        float b05 = a02 * a13 - a03 * a12;
        float b06 = a20 * a31 - a21 * a30;
        float b07 = a20 * a32 - a22 * a30;
        float b08 = a20 * a33 - a23 * a30;
        float b09 = a21 * a32 - a22 * a31;
        float b10 = a21 * a33 - a23 * a31;
        float b11 = a22 * a33 - a23 * a32;

        // Calculate the determinant
        float det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;

        if (det == 0)
        {
            return null;
        }
        float one = 1;
        det = one / det;

        output[0] = (a11 * b11 - a12 * b10 + a13 * b09) * det;
        output[1] = (a12 * b08 - a10 * b11 - a13 * b07) * det;
        output[2] = (a10 * b10 - a11 * b08 + a13 * b06) * det;

        output[3] = (a02 * b10 - a01 * b11 - a03 * b09) * det;
        output[4] = (a00 * b11 - a02 * b08 + a03 * b07) * det;
        output[5] = (a01 * b08 - a00 * b10 - a03 * b06) * det;

        output[6] = (a31 * b05 - a32 * b04 + a33 * b03) * det;
        output[7] = (a32 * b02 - a30 * b05 - a33 * b01) * det;
        output[8] = (a30 * b04 - a31 * b02 + a33 * b00) * det;

        return output;
    }

    ///**
    // * Returns a string representation of a mat3
    // *
    // * @param {mat3} mat matrix to represent as a string
    // * @returns {String} string representation of the matrix
    // */
    //mat3.str = function (a) {
    //    return 'mat3(' + a[0] + ', ' + a[1] + ', ' + a[2] + ', ' + 
    //                    a[3] + ', ' + a[4] + ', ' + a[5] + ', ' + 
    //                    a[6] + ', ' + a[7] + ', ' + a[8] + ')';
    //};

    //if(typeof(exports) !== 'undefined') {
    //    exports.mat3 = mat3;
    //}
    void f()
    {
    }
}

/// 4x4 Matrix
public class Mat4
{
    /// Creates a new identity mat4
    /// Returns {mat4} a new 4x4 matrix
    public static float[] Create()
    {
        float[] output = new float[16];
        output[0] = 1;
        output[1] = 0;
        output[2] = 0;
        output[3] = 0;
        output[4] = 0;
        output[5] = 1;
        output[6] = 0;
        output[7] = 0;
        output[8] = 0;
        output[9] = 0;
        output[10] = 1;
        output[11] = 0;
        output[12] = 0;
        output[13] = 0;
        output[14] = 0;
        output[15] = 1;
        return output;
    }

    /// Creates a new mat4 initialized with values from an existing matrix
    /// Returns {mat4} a new 4x4 matrix
    public static float[] CloneIt(
        /// {mat4} a matrix to clone
        float[] a)
    {
        float[] output = new float[16];
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        output[4] = a[4];
        output[5] = a[5];
        output[6] = a[6];
        output[7] = a[7];
        output[8] = a[8];
        output[9] = a[9];
        output[10] = a[10];
        output[11] = a[11];
        output[12] = a[12];
        output[13] = a[13];
        output[14] = a[14];
        output[15] = a[15];
        return output;
    }

    /// Copy the values from one mat4 to another
    /// Returns {mat4} out
    public static float[] Copy(
        /// {mat4} out the receiving matrix
        float[] output,
        /// {mat4} a the source matrix
        float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        output[4] = a[4];
        output[5] = a[5];
        output[6] = a[6];
        output[7] = a[7];
        output[8] = a[8];
        output[9] = a[9];
        output[10] = a[10];
        output[11] = a[11];
        output[12] = a[12];
        output[13] = a[13];
        output[14] = a[14];
        output[15] = a[15];
        return output;
    }

    /// Set a mat4 to the identity matrix
    /// Returns {mat4} out
    public static float[] Identity_(
        /// {mat4} out the receiving matrix
        float[] output)
    {
        output[0] = 1;
        output[1] = 0;
        output[2] = 0;
        output[3] = 0;
        output[4] = 0;
        output[5] = 1;
        output[6] = 0;
        output[7] = 0;
        output[8] = 0;
        output[9] = 0;
        output[10] = 1;
        output[11] = 0;
        output[12] = 0;
        output[13] = 0;
        output[14] = 0;
        output[15] = 1;
        return output;
    }

    /// Transpose the values of a mat4
    /// @returns {mat4} out
    //mat4.transpose = function(output, a) {
    public static float[] Transpose(
        /// @param {mat4} out the receiving matrix
        float[] output,
        /// @param {mat4} a the source matrix
        float[] a)
    {
        // If we are transposing ourselves we can skip a few steps but have to cache some values
        if (output == a)
        {
            float a01 = a[1]; float a02 = a[2]; float a03 = a[3];
            float a12 = a[6]; float a13 = a[7];
            float a23 = a[11];

            output[1] = a[4];
            output[2] = a[8];
            output[3] = a[12];
            output[4] = a01;
            output[6] = a[9];
            output[7] = a[13];
            output[8] = a02;
            output[9] = a12;
            output[11] = a[14];
            output[12] = a03;
            output[13] = a13;
            output[14] = a23;
        }
        else
        {
            output[0] = a[0];
            output[1] = a[4];
            output[2] = a[8];
            output[3] = a[12];
            output[4] = a[1];
            output[5] = a[5];
            output[6] = a[9];
            output[7] = a[13];
            output[8] = a[2];
            output[9] = a[6];
            output[10] = a[10];
            output[11] = a[14];
            output[12] = a[3];
            output[13] = a[7];
            output[14] = a[11];
            output[15] = a[15];
        }

        return output;
    }

    /// Inverts a mat4
    /// @returns {mat4} out
    public static float[] Invert(
        /// {mat4} out the receiving matrix
        float[] output,
        /// {mat4} a the source matrix
        float[] a)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2]; float a03 = a[3];
        float a10 = a[4]; float a11 = a[5]; float a12 = a[6]; float a13 = a[7];
        float a20 = a[8]; float a21 = a[9]; float a22 = a[10]; float a23 = a[11];
        float a30 = a[12]; float a31 = a[13]; float a32 = a[14]; float a33 = a[15];

        float b00 = a00 * a11 - a01 * a10;
        float b01 = a00 * a12 - a02 * a10;
        float b02 = a00 * a13 - a03 * a10;
        float b03 = a01 * a12 - a02 * a11;
        float b04 = a01 * a13 - a03 * a11;
        float b05 = a02 * a13 - a03 * a12;
        float b06 = a20 * a31 - a21 * a30;
        float b07 = a20 * a32 - a22 * a30;
        float b08 = a20 * a33 - a23 * a30;
        float b09 = a21 * a32 - a22 * a31;
        float b10 = a21 * a33 - a23 * a31;
        float b11 = a22 * a33 - a23 * a32;

        // Calculate the determinant
        float det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;

        if (det == 0)
        {
            return null;
        }
        float one = 1;
        det = one / det;

        output[0] = (a11 * b11 - a12 * b10 + a13 * b09) * det;
        output[1] = (a02 * b10 - a01 * b11 - a03 * b09) * det;
        output[2] = (a31 * b05 - a32 * b04 + a33 * b03) * det;
        output[3] = (a22 * b04 - a21 * b05 - a23 * b03) * det;
        output[4] = (a12 * b08 - a10 * b11 - a13 * b07) * det;
        output[5] = (a00 * b11 - a02 * b08 + a03 * b07) * det;
        output[6] = (a32 * b02 - a30 * b05 - a33 * b01) * det;
        output[7] = (a20 * b05 - a22 * b02 + a23 * b01) * det;
        output[8] = (a10 * b10 - a11 * b08 + a13 * b06) * det;
        output[9] = (a01 * b08 - a00 * b10 - a03 * b06) * det;
        output[10] = (a30 * b04 - a31 * b02 + a33 * b00) * det;
        output[11] = (a21 * b02 - a20 * b04 - a23 * b00) * det;
        output[12] = (a11 * b07 - a10 * b09 - a12 * b06) * det;
        output[13] = (a00 * b09 - a01 * b07 + a02 * b06) * det;
        output[14] = (a31 * b01 - a30 * b03 - a32 * b00) * det;
        output[15] = (a20 * b03 - a21 * b01 + a22 * b00) * det;

        return output;
    }

    /// Calculates the adjugate of a mat4
    /// @returns {mat4} out
    public static float[] Adjoint(
        /// @param {mat4} out the receiving matrix
        float[] output,
        /// @param {mat4} a the source matrix
        float[] a)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2]; float a03 = a[3];
        float a10 = a[4]; float a11 = a[5]; float a12 = a[6]; float a13 = a[7];
        float a20 = a[8]; float a21 = a[9]; float a22 = a[10]; float a23 = a[11];
        float a30 = a[12]; float a31 = a[13]; float a32 = a[14]; float a33 = a[15];

        output[0] = (a11 * (a22 * a33 - a23 * a32) - a21 * (a12 * a33 - a13 * a32) + a31 * (a12 * a23 - a13 * a22));
        output[1] = -(a01 * (a22 * a33 - a23 * a32) - a21 * (a02 * a33 - a03 * a32) + a31 * (a02 * a23 - a03 * a22));
        output[2] = (a01 * (a12 * a33 - a13 * a32) - a11 * (a02 * a33 - a03 * a32) + a31 * (a02 * a13 - a03 * a12));
        output[3] = -(a01 * (a12 * a23 - a13 * a22) - a11 * (a02 * a23 - a03 * a22) + a21 * (a02 * a13 - a03 * a12));
        output[4] = -(a10 * (a22 * a33 - a23 * a32) - a20 * (a12 * a33 - a13 * a32) + a30 * (a12 * a23 - a13 * a22));
        output[5] = (a00 * (a22 * a33 - a23 * a32) - a20 * (a02 * a33 - a03 * a32) + a30 * (a02 * a23 - a03 * a22));
        output[6] = -(a00 * (a12 * a33 - a13 * a32) - a10 * (a02 * a33 - a03 * a32) + a30 * (a02 * a13 - a03 * a12));
        output[7] = (a00 * (a12 * a23 - a13 * a22) - a10 * (a02 * a23 - a03 * a22) + a20 * (a02 * a13 - a03 * a12));
        output[8] = (a10 * (a21 * a33 - a23 * a31) - a20 * (a11 * a33 - a13 * a31) + a30 * (a11 * a23 - a13 * a21));
        output[9] = -(a00 * (a21 * a33 - a23 * a31) - a20 * (a01 * a33 - a03 * a31) + a30 * (a01 * a23 - a03 * a21));
        output[10] = (a00 * (a11 * a33 - a13 * a31) - a10 * (a01 * a33 - a03 * a31) + a30 * (a01 * a13 - a03 * a11));
        output[11] = -(a00 * (a11 * a23 - a13 * a21) - a10 * (a01 * a23 - a03 * a21) + a20 * (a01 * a13 - a03 * a11));
        output[12] = -(a10 * (a21 * a32 - a22 * a31) - a20 * (a11 * a32 - a12 * a31) + a30 * (a11 * a22 - a12 * a21));
        output[13] = (a00 * (a21 * a32 - a22 * a31) - a20 * (a01 * a32 - a02 * a31) + a30 * (a01 * a22 - a02 * a21));
        output[14] = -(a00 * (a11 * a32 - a12 * a31) - a10 * (a01 * a32 - a02 * a31) + a30 * (a01 * a12 - a02 * a11));
        output[15] = (a00 * (a11 * a22 - a12 * a21) - a10 * (a01 * a22 - a02 * a21) + a20 * (a01 * a12 - a02 * a11));
        return output;
    }

    /// Calculates the determinant of a mat4
    /// @returns {Number} determinant of a
    public static float Determinant(
        /// @param {mat4} a the source matrix
        float[] a)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2]; float a03 = a[3];
        float a10 = a[4]; float a11 = a[5]; float a12 = a[6]; float a13 = a[7];
        float a20 = a[8]; float a21 = a[9]; float a22 = a[10]; float a23 = a[11];
        float a30 = a[12]; float a31 = a[13]; float a32 = a[14]; float a33 = a[15];

        float b00 = a00 * a11 - a01 * a10;
        float b01 = a00 * a12 - a02 * a10;
        float b02 = a00 * a13 - a03 * a10;
        float b03 = a01 * a12 - a02 * a11;
        float b04 = a01 * a13 - a03 * a11;
        float b05 = a02 * a13 - a03 * a12;
        float b06 = a20 * a31 - a21 * a30;
        float b07 = a20 * a32 - a22 * a30;
        float b08 = a20 * a33 - a23 * a30;
        float b09 = a21 * a32 - a22 * a31;
        float b10 = a21 * a33 - a23 * a31;
        float b11 = a22 * a33 - a23 * a32;

        // Calculate the determinant
        return b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;
    }

    /// Multiplies two mat4's
    /// @returns {mat4} out
    public static float[] Multiply(
        /// @param {mat4} out the receiving matrix
        float[] output,
        /// @param {mat4} a the first operand
        float[] a,
        /// @param {mat4} b the second operand
        float[] b)
    {
        float a00 = a[0]; float a01 = a[1]; float a02 = a[2]; float a03 = a[3];
        float a10 = a[4]; float a11 = a[5]; float a12 = a[6]; float a13 = a[7];
        float a20 = a[8]; float a21 = a[9]; float a22 = a[10]; float a23 = a[11];
        float a30 = a[12]; float a31 = a[13]; float a32 = a[14]; float a33 = a[15];

        // Cache only the current line of the second matrix
        float b0 = b[0]; float b1 = b[1]; float b2 = b[2]; float b3 = b[3];
        output[0] = b0 * a00 + b1 * a10 + b2 * a20 + b3 * a30;
        output[1] = b0 * a01 + b1 * a11 + b2 * a21 + b3 * a31;
        output[2] = b0 * a02 + b1 * a12 + b2 * a22 + b3 * a32;
        output[3] = b0 * a03 + b1 * a13 + b2 * a23 + b3 * a33;

        b0 = b[4]; b1 = b[5]; b2 = b[6]; b3 = b[7];
        output[4] = b0 * a00 + b1 * a10 + b2 * a20 + b3 * a30;
        output[5] = b0 * a01 + b1 * a11 + b2 * a21 + b3 * a31;
        output[6] = b0 * a02 + b1 * a12 + b2 * a22 + b3 * a32;
        output[7] = b0 * a03 + b1 * a13 + b2 * a23 + b3 * a33;

        b0 = b[8]; b1 = b[9]; b2 = b[10]; b3 = b[11];
        output[8] = b0 * a00 + b1 * a10 + b2 * a20 + b3 * a30;
        output[9] = b0 * a01 + b1 * a11 + b2 * a21 + b3 * a31;
        output[10] = b0 * a02 + b1 * a12 + b2 * a22 + b3 * a32;
        output[11] = b0 * a03 + b1 * a13 + b2 * a23 + b3 * a33;

        b0 = b[12]; b1 = b[13]; b2 = b[14]; b3 = b[15];
        output[12] = b0 * a00 + b1 * a10 + b2 * a20 + b3 * a30;
        output[13] = b0 * a01 + b1 * a11 + b2 * a21 + b3 * a31;
        output[14] = b0 * a02 + b1 * a12 + b2 * a22 + b3 * a32;
        output[15] = b0 * a03 + b1 * a13 + b2 * a23 + b3 * a33;
        return output;
    }

    /// Alias for {@link mat4.multiply}
    public static float[] Mul(float[] output, float[] a, float[] b)
    {
        return Multiply(output, a, b);
    }

    /// Translate a mat4 by the given vector
    /// @returns {mat4} out
    public static float[] Translate(
        /// {mat4} out the receiving matrix
        float[] output,
        /// {mat4} a the matrix to translate
        float[] a,
        /// {vec3} v vector to translate by
        float[] v)
    {
        float x = v[0]; float y = v[1]; float z = v[2];
        float a00; float a01; float a02; float a03;
        float a10; float a11; float a12; float a13;
        float a20; float a21; float a22; float a23;

        if (a == output)
        {
            output[12] = a[0] * x + a[4] * y + a[8] * z + a[12];
            output[13] = a[1] * x + a[5] * y + a[9] * z + a[13];
            output[14] = a[2] * x + a[6] * y + a[10] * z + a[14];
            output[15] = a[3] * x + a[7] * y + a[11] * z + a[15];
        }
        else
        {
            a00 = a[0]; a01 = a[1]; a02 = a[2]; a03 = a[3];
            a10 = a[4]; a11 = a[5]; a12 = a[6]; a13 = a[7];
            a20 = a[8]; a21 = a[9]; a22 = a[10]; a23 = a[11];

            output[0] = a00; output[1] = a01; output[2] = a02; output[3] = a03;
            output[4] = a10; output[5] = a11; output[6] = a12; output[7] = a13;
            output[8] = a20; output[9] = a21; output[10] = a22; output[11] = a23;

            output[12] = a00 * x + a10 * y + a20 * z + a[12];
            output[13] = a01 * x + a11 * y + a21 * z + a[13];
            output[14] = a02 * x + a12 * y + a22 * z + a[14];
            output[15] = a03 * x + a13 * y + a23 * z + a[15];
        }

        return output;
    }

    /// Scales the mat4 by the dimensions in the given vec3
    /// @returns {mat4} out
    public static float[] Scale(
        /// {mat4} out the receiving matrix
        float[] output,
        /// {mat4} a the matrix to scale
        float[] a,
        /// {vec3} v the vec3 to scale the matrix by
        float[] v)
    {
        float x = v[0]; float y = v[1]; float z = v[2];

        output[0] = a[0] * x;
        output[1] = a[1] * x;
        output[2] = a[2] * x;
        output[3] = a[3] * x;
        output[4] = a[4] * y;
        output[5] = a[5] * y;
        output[6] = a[6] * y;
        output[7] = a[7] * y;
        output[8] = a[8] * z;
        output[9] = a[9] * z;
        output[10] = a[10] * z;
        output[11] = a[11] * z;
        output[12] = a[12];
        output[13] = a[13];
        output[14] = a[14];
        output[15] = a[15];
        return output;
    }

    /// Rotates a mat4 by the given angle
    /// @returns {mat4} out
    public static float[] Rotate(
        /// {mat4} out the receiving matrix
        float[] output,
        /// {mat4} a the matrix to rotate
        float[] a,
        /// {Number} rad the angle to rotate the matrix by
        float rad,
        /// {vec3} axis the axis to rotate around
        float[] axis)
    {
        float x = axis[0]; float y = axis[1]; float z = axis[2];
        float len = Platform.Sqrt(x * x + y * y + z * z);
        float s; float c; float t;
        float a00; float a01; float a02; float a03;
        float a10; float a11; float a12; float a13;
        float a20; float a21; float a22; float a23;
        float b00; float b01; float b02;
        float b10; float b11; float b12;
        float b20; float b21; float b22;

        if (GlMatrixMath.Abs(len) < GlMatrixMath.GLMAT_EPSILON()) { return null; }

        len = 1 / len;
        x *= len;
        y *= len;
        z *= len;

        s = Platform.Sin(rad);
        c = Platform.Cos(rad);
        t = 1 - c;

        a00 = a[0]; a01 = a[1]; a02 = a[2]; a03 = a[3];
        a10 = a[4]; a11 = a[5]; a12 = a[6]; a13 = a[7];
        a20 = a[8]; a21 = a[9]; a22 = a[10]; a23 = a[11];

        // Construct the elements of the rotation matrix
        b00 = x * x * t + c; b01 = y * x * t + z * s; b02 = z * x * t - y * s;
        b10 = x * y * t - z * s; b11 = y * y * t + c; b12 = z * y * t + x * s;
        b20 = x * z * t + y * s; b21 = y * z * t - x * s; b22 = z * z * t + c;

        // Perform rotation-specific matrix multiplication
        output[0] = a00 * b00 + a10 * b01 + a20 * b02;
        output[1] = a01 * b00 + a11 * b01 + a21 * b02;
        output[2] = a02 * b00 + a12 * b01 + a22 * b02;
        output[3] = a03 * b00 + a13 * b01 + a23 * b02;
        output[4] = a00 * b10 + a10 * b11 + a20 * b12;
        output[5] = a01 * b10 + a11 * b11 + a21 * b12;
        output[6] = a02 * b10 + a12 * b11 + a22 * b12;
        output[7] = a03 * b10 + a13 * b11 + a23 * b12;
        output[8] = a00 * b20 + a10 * b21 + a20 * b22;
        output[9] = a01 * b20 + a11 * b21 + a21 * b22;
        output[10] = a02 * b20 + a12 * b21 + a22 * b22;
        output[11] = a03 * b20 + a13 * b21 + a23 * b22;

        if (a != output)
        {
            // If the source and destination differ, copy the unchanged last row
            output[12] = a[12];
            output[13] = a[13];
            output[14] = a[14];
            output[15] = a[15];
        }
        return output;
    }

    /// Rotates a matrix by the given angle around the X axis
    /// @returns {mat4} out
    public static float[] RotateX(
        /// {mat4} out the receiving matrix
        float[] output,
        /// {mat4} a the matrix to rotate
        float[] a,
        /// {Number} rad the angle to rotate the matrix by
        float rad)
    {
        float s = Platform.Sin(rad);
        float c = Platform.Cos(rad);
        float a10 = a[4];
        float a11 = a[5];
        float a12 = a[6];
        float a13 = a[7];
        float a20 = a[8];
        float a21 = a[9];
        float a22 = a[10];
        float a23 = a[11];

        if (a != output)
        {
            // If the source and destination differ, copy the unchanged rows
            output[0] = a[0];
            output[1] = a[1];
            output[2] = a[2];
            output[3] = a[3];
            output[12] = a[12];
            output[13] = a[13];
            output[14] = a[14];
            output[15] = a[15];
        }

        // Perform axis-specific matrix multiplication
        output[4] = a10 * c + a20 * s;
        output[5] = a11 * c + a21 * s;
        output[6] = a12 * c + a22 * s;
        output[7] = a13 * c + a23 * s;
        output[8] = a20 * c - a10 * s;
        output[9] = a21 * c - a11 * s;
        output[10] = a22 * c - a12 * s;
        output[11] = a23 * c - a13 * s;
        return output;
    }

    /// Rotates a matrix by the given angle around the Y axis
    /// @returns {mat4} out
    public static float[] RotateY(
        /// {mat4} out the receiving matrix
        float[] output,
        /// {mat4} a the matrix to rotate
        float[] a,
        /// {Number} rad the angle to rotate the matrix by
        float rad)
    {
        float s = Platform.Sin(rad);
        float c = Platform.Cos(rad);
        float a00 = a[0];
        float a01 = a[1];
        float a02 = a[2];
        float a03 = a[3];
        float a20 = a[8];
        float a21 = a[9];
        float a22 = a[10];
        float a23 = a[11];

        if (a != output)
        {
            // If the source and destination differ, copy the unchanged rows
            output[4] = a[4];
            output[5] = a[5];
            output[6] = a[6];
            output[7] = a[7];
            output[12] = a[12];
            output[13] = a[13];
            output[14] = a[14];
            output[15] = a[15];
        }

        // Perform axis-specific matrix multiplication
        output[0] = a00 * c - a20 * s;
        output[1] = a01 * c - a21 * s;
        output[2] = a02 * c - a22 * s;
        output[3] = a03 * c - a23 * s;
        output[8] = a00 * s + a20 * c;
        output[9] = a01 * s + a21 * c;
        output[10] = a02 * s + a22 * c;
        output[11] = a03 * s + a23 * c;
        return output;
    }

    /// Rotates a matrix by the given angle around the Z axis
    /// @returns {mat4} out
    public static float[] RotateZ(
        /// {mat4} out the receiving matrix
        float[] output,
        /// {mat4} a the matrix to rotate
        float[] a,
        /// {Number} rad the angle to rotate the matrix by
        float rad)
    {
        float s = Platform.Sin(rad);
        float c = Platform.Cos(rad);
        float a00 = a[0];
        float a01 = a[1];
        float a02 = a[2];
        float a03 = a[3];
        float a10 = a[4];
        float a11 = a[5];
        float a12 = a[6];
        float a13 = a[7];

        if (a != output)
        {
            // If the source and destination differ, copy the unchanged last row
            output[8] = a[8];
            output[9] = a[9];
            output[10] = a[10];
            output[11] = a[11];
            output[12] = a[12];
            output[13] = a[13];
            output[14] = a[14];
            output[15] = a[15];
        }

        // Perform axis-specific matrix multiplication
        output[0] = a00 * c + a10 * s;
        output[1] = a01 * c + a11 * s;
        output[2] = a02 * c + a12 * s;
        output[3] = a03 * c + a13 * s;
        output[4] = a10 * c - a00 * s;
        output[5] = a11 * c - a01 * s;
        output[6] = a12 * c - a02 * s;
        output[7] = a13 * c - a03 * s;
        return output;
    }

    /// Creates a matrix from a quaternion rotation and vector translation
    /// This is equivalent to (but much faster than):
    ///     mat4.identity(dest);
    ///     mat4.translate(dest, vec);
    ///     var quatMat = mat4.create();
    ///     quat4.toMat4(quat, quatMat);
    ///     mat4.multiply(dest, quatMat);
    /// @returns {mat4} out
    public static float[] FromRotationTranslation(
        /// {mat4} out mat4 receiving operation result
        float[] output,
        /// {quat4} q Rotation quaternion
        float[] q,
        /// {vec3} v Translation vector
        float[] v)
    {
        // Quaternion math
        float x = q[0]; float y = q[1]; float z = q[2]; float w = q[3];
        float x2 = x + x;
        float y2 = y + y;
        float z2 = z + z;

        float xx = x * x2;
        float xy = x * y2;
        float xz = x * z2;
        float yy = y * y2;
        float yz = y * z2;
        float zz = z * z2;
        float wx = w * x2;
        float wy = w * y2;
        float wz = w * z2;

        output[0] = 1 - (yy + zz);
        output[1] = xy + wz;
        output[2] = xz - wy;
        output[3] = 0;
        output[4] = xy - wz;
        output[5] = 1 - (xx + zz);
        output[6] = yz + wx;
        output[7] = 0;
        output[8] = xz + wy;
        output[9] = yz - wx;
        output[10] = 1 - (xx + yy);
        output[11] = 0;
        output[12] = v[0];
        output[13] = v[1];
        output[14] = v[2];
        output[15] = 1;

        return output;
    }

    /// Calculates a 4x4 matrix from the given quaternion
    /// @returns {mat4} out
    public static float[] FromQuat(
        /// {mat4} out mat4 receiving operation result
        float[] output,
        /// {quat} q Quaternion to create matrix from
        float[] q)
    {
        float x = q[0]; float y = q[1]; float z = q[2]; float w = q[3];
        float x2 = x + x;
        float y2 = y + y;
        float z2 = z + z;

        float xx = x * x2;
        float xy = x * y2;
        float xz = x * z2;
        float yy = y * y2;
        float yz = y * z2;
        float zz = z * z2;
        float wx = w * x2;
        float wy = w * y2;
        float wz = w * z2;

        output[0] = 1 - (yy + zz);
        output[1] = xy + wz;
        output[2] = xz - wy;
        output[3] = 0;

        output[4] = xy - wz;
        output[5] = 1 - (xx + zz);
        output[6] = yz + wx;
        output[7] = 0;

        output[8] = xz + wy;
        output[9] = yz - wx;
        output[10] = 1 - (xx + yy);
        output[11] = 0;

        output[12] = 0;
        output[13] = 0;
        output[14] = 0;
        output[15] = 1;

        return output;
    }

    /// Generates a frustum matrix with the given bounds
    /// @returns {mat4} out
    public static float[] Frustum(
        /// {mat4} out mat4 frustum matrix will be written into
        float[] output,
        /// {Number} left Left bound of the frustum
        float left,
        /// {Number} right Right bound of the frustum
        float right,
        /// {Number} bottom Bottom bound of the frustum
        float bottom,
        /// {Number} top Top bound of the frustum
        float top,
        /// {Number} near Near bound of the frustum
        float near,
        /// {Number} far Far bound of the frustum
        float far)
    {
        float rl = 1 / (right - left);
        float tb = 1 / (top - bottom);
        float nf = 1 / (near - far);
        output[0] = (near * 2) * rl;
        output[1] = 0;
        output[2] = 0;
        output[3] = 0;
        output[4] = 0;
        output[5] = (near * 2) * tb;
        output[6] = 0;
        output[7] = 0;
        output[8] = (right + left) * rl;
        output[9] = (top + bottom) * tb;
        output[10] = (far + near) * nf;
        output[11] = -1;
        output[12] = 0;
        output[13] = 0;
        output[14] = (far * near * 2) * nf;
        output[15] = 0;
        return output;
    }

    /// Generates a perspective projection matrix with the given bounds
    /// @returns {mat4} out
    public static float[] Perspective(
        /// {mat4} out mat4 frustum matrix will be written into
        float[] output,
        /// {number} fovy Vertical field of view in radians
        float fovy,
        /// {number} aspect Aspect ratio. typically viewport width/height
        float aspect,
        /// {number} near Near bound of the frustum
        float near,
        /// {number} far Far bound of the frustum
        float far)
    {
        float one = 1;
        float f = one / Platform.Tan(fovy / 2);
        float nf = 1 / (near - far);
        output[0] = f / aspect;
        output[1] = 0;
        output[2] = 0;
        output[3] = 0;
        output[4] = 0;
        output[5] = f;
        output[6] = 0;
        output[7] = 0;
        output[8] = 0;
        output[9] = 0;
        output[10] = (far + near) * nf;
        output[11] = -1;
        output[12] = 0;
        output[13] = 0;
        output[14] = (2 * far * near) * nf;
        output[15] = 0;
        return output;
    }

    /// Generates a orthogonal projection matrix with the given bounds
    /// @returns {mat4} out
    public static float[] Ortho(
        /// {mat4} out mat4 frustum matrix will be written into
        float[] output,
        /// {number} left Left bound of the frustum
        float left,
        /// {number} right Right bound of the frustum
        float right,
        /// {number} bottom Bottom bound of the frustum
        float bottom,
        /// {number} top Top bound of the frustum
        float top,
        /// {number} near Near bound of the frustum
        float near,
        /// {number} far Far bound of the frustum
        float far)
    {
        float lr = 1 / (left - right);
        float bt = 1 / (bottom - top);
        float nf = 1 / (near - far);
        output[0] = -2 * lr;
        output[1] = 0;
        output[2] = 0;
        output[3] = 0;
        output[4] = 0;
        output[5] = -2 * bt;
        output[6] = 0;
        output[7] = 0;
        output[8] = 0;
        output[9] = 0;
        output[10] = 2 * nf;
        output[11] = 0;
        output[12] = (left + right) * lr;
        output[13] = (top + bottom) * bt;
        output[14] = (far + near) * nf;
        output[15] = 1;
        return output;
    }

    /// Generates a look-at matrix with the given eye position, focal point, and up axis
    /// @returns {mat4} out
    public static float[] LookAt(
        /// {mat4} out mat4 frustum matrix will be written into
        float[] output,
        /// {vec3} eye Position of the viewer
        float[] eye,
        /// {vec3} center Point the viewer is looking at
        float[] center,
        /// {vec3} up vec3 pointing up
        float[] up)
    {
        float x0; float x1; float x2; float y0; float y1; float y2; float z0; float z1; float z2; float len;
        float eyex = eye[0];
        float eyey = eye[1];
        float eyez = eye[2];
        float upx = up[0];
        float upy = up[1];
        float upz = up[2];
        float centerx = center[0];
        float centery = center[1];
        float centerz = center[2];

        if (GlMatrixMath.Abs(eyex - centerx) < GlMatrixMath.GLMAT_EPSILON() &&
            GlMatrixMath.Abs(eyey - centery) < GlMatrixMath.GLMAT_EPSILON() &&
            GlMatrixMath.Abs(eyez - centerz) < GlMatrixMath.GLMAT_EPSILON())
        {
            return Mat4.Identity_(output);
        }

        z0 = eyex - centerx;
        z1 = eyey - centery;
        z2 = eyez - centerz;

        len = 1 / Platform.Sqrt(z0 * z0 + z1 * z1 + z2 * z2);
        z0 *= len;
        z1 *= len;
        z2 *= len;

        x0 = upy * z2 - upz * z1;
        x1 = upz * z0 - upx * z2;
        x2 = upx * z1 - upy * z0;
        len = Platform.Sqrt(x0 * x0 + x1 * x1 + x2 * x2);
        if (len == 0)
        {
            x0 = 0;
            x1 = 0;
            x2 = 0;
        }
        else
        {
            len = 1 / len;
            x0 *= len;
            x1 *= len;
            x2 *= len;
        }

        y0 = z1 * x2 - z2 * x1;
        y1 = z2 * x0 - z0 * x2;
        y2 = z0 * x1 - z1 * x0;

        len = Platform.Sqrt(y0 * y0 + y1 * y1 + y2 * y2);
        if (len == 0)
        {
            y0 = 0;
            y1 = 0;
            y2 = 0;
        }
        else
        {
            len = 1 / len;
            y0 *= len;
            y1 *= len;
            y2 *= len;
        }

        output[0] = x0;
        output[1] = y0;
        output[2] = z0;
        output[3] = 0;
        output[4] = x1;
        output[5] = y1;
        output[6] = z1;
        output[7] = 0;
        output[8] = x2;
        output[9] = y2;
        output[10] = z2;
        output[11] = 0;
        output[12] = -(x0 * eyex + x1 * eyey + x2 * eyez);
        output[13] = -(y0 * eyex + y1 * eyey + y2 * eyez);
        output[14] = -(z0 * eyex + z1 * eyey + z2 * eyez);
        output[15] = 1;

        return output;
    }

    ///**
    // * Returns a string representation of a mat4
    // *
    // * @param {mat4} mat matrix to represent as a string
    // * @returns {String} string representation of the matrix
    // */
    //mat4.str = function (a) {
    //    return 'mat4(' + a[0] + ', ' + a[1] + ', ' + a[2] + ', ' + a[3] + ', ' +
    //                    a[4] + ', ' + a[5] + ', ' + a[6] + ', ' + a[7] + ', ' +
    //                    a[8] + ', ' + a[9] + ', ' + a[10] + ', ' + a[11] + ', ' + 
    //                    a[12] + ', ' + a[13] + ', ' + a[14] + ', ' + a[15] + ')';
    //};

    //if(typeof(exports) !== 'undefined') {
    //    exports.mat4 = mat4;
    //}

    void f()
    {
    }
}

public class Quat
{
    //    /**
    // * @class Quaternion
    // * @name quat
    // */
    //var quat = {};

    ///**
    // * Creates a new identity quat
    // *
    // * @returns {quat} a new quaternion
    // */
    public static float[] Create()
    {
        float[] output = new float[4];
        output[0] = 0;
        output[1] = 0;
        output[2] = 0;
        output[3] = 1;
        return output;
    }

    ///**
    // * Sets a quaternion to represent the shortest rotation from one
    // * vector to another.
    // *
    // * Both vectors are assumed to be unit length.
    // *
    // * @param {quat} output the receiving quaternion.
    // * @param {vec3} a the initial vector
    // * @param {vec3} b the destination vector
    // * @returns {quat} output
    // */
    public static float[] RotationTo(float[] output, float[] a, float[] b)
    {
        float[] tmpvec3 = Vec3.Create();
        float[] xUnitVec3 = Vec3.FromValues(1, 0, 0);
        float[] yUnitVec3 = Vec3.FromValues(0, 1, 0);

        //    return function(output, a, b) {
        float dot = Vec3.Dot(a, b);

        float nines = 999999; // 0.999999
        nines /= 1000000;

        float epsilon = 1; // 0.000001
        epsilon /= 1000000;

        if (dot < -nines)
        {
            Vec3.Cross(tmpvec3, xUnitVec3, a);
            if (Vec3.Length_(tmpvec3) < epsilon)
                Vec3.Cross(tmpvec3, yUnitVec3, a);
            Vec3.Normalize(tmpvec3, tmpvec3);
            Quat.SetAxisAngle(output, tmpvec3, GlMatrixMath.PI());
            return output;
        }
        else if (dot > nines)
        {
            output[0] = 0;
            output[1] = 0;
            output[2] = 0;
            output[3] = 1;
            return output;
        }
        else
        {
            Vec3.Cross(tmpvec3, a, b);
            output[0] = tmpvec3[0];
            output[1] = tmpvec3[1];
            output[2] = tmpvec3[2];
            output[3] = 1 + dot;
            return Quat.Normalize(output, output);
        }
        //    };
    }

    ///**
    // * Sets the specified quaternion with values corresponding to the given
    // * axes. Each axis is a vec3 and is expected to be unit length and
    // * perpendicular to all other specified axes.
    // *
    // * @param {vec3} view  the vector representing the viewing direction
    // * @param {vec3} right the vector representing the local "right" direction
    // * @param {vec3} up    the vector representing the local "up" direction
    // * @returns {quat} output
    // */
    public static float[] SetAxes(float[] output, float[] view, float[] right, float[] up)
    {
        float[] matr = Mat3.Create();

        //    return function(output, view, right, up) {
        matr[0] = right[0];
        matr[3] = right[1];
        matr[6] = right[2];

        matr[1] = up[0];
        matr[4] = up[1];
        matr[7] = up[2];

        matr[2] = view[0];
        matr[5] = view[1];
        matr[8] = view[2];

        return Quat.Normalize(output, Quat.FromMat3(output, matr));
        //    };
    }

    ///**
    // * Creates a new quat initialized with values from an existing quaternion
    // *
    // * @param {quat} a quaternion to clone
    // * @returns {quat} a new quaternion
    // * @function
    // */
    public static float[] CloneIt(float[] a)
    {
        return Vec4.CloneIt(a);
    }

    ///**
    // * Creates a new quat initialized with the given values
    // *
    // * @param {Number} x X component
    // * @param {Number} y Y component
    // * @param {Number} z Z component
    // * @param {Number} w W component
    // * @returns {quat} a new quaternion
    // * @function
    // */
    public static float[] FromValues(float x, float y, float z, float w)
    {
        return Vec4.FromValues(x, y, z, w);
    }

    ///**
    // * Copy the values from one quat to another
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a the source quaternion
    // * @returns {quat} output
    // * @function
    // */
    public static float[] Copy(float[] output, float[] a)
    {
        return Vec4.Copy(output, a);
    }

    ///**
    // * Set the components of a quat to the given values
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {Number} x X component
    // * @param {Number} y Y component
    // * @param {Number} z Z component
    // * @param {Number} w W component
    // * @returns {quat} output
    // * @function
    // */
    public static float[] Set(float[] output, float x, float y, float z, float w)
    {
        return Vec4.Set(output, x, y, z, w);
    }

    ///**
    // * Set a quat to the identity quaternion
    // *
    // * @param {quat} output the receiving quaternion
    // * @returns {quat} output
    // */
    public static float[] Identity_(float[] output)
    {
        output[0] = 0;
        output[1] = 0;
        output[2] = 0;
        output[3] = 1;
        return output;
    }

    ///**
    // * Sets a quat from the given angle and rotation axis,
    // * then returns it.
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {vec3} axis the axis around which to rotate
    // * @param {Number} rad the angle in radians
    // * @returns {quat} output
    // **/
    public static float[] SetAxisAngle(float[] output, float[] axis, float rad)
    {
        rad = rad / 2;
        float s = Platform.Sin(rad);
        output[0] = s * axis[0];
        output[1] = s * axis[1];
        output[2] = s * axis[2];
        output[3] = Platform.Cos(rad);
        return output;
    }

    ///**
    // * Adds two quat's
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a the first operand
    // * @param {quat} b the second operand
    // * @returns {quat} output
    // * @function
    // */
    //quat.add = vec4.add;
    public static float[] Add(float[] output, float[] a, float[] b)
    {
        return Vec4.Add(output, a, b);
    }

    ///**
    // * Multiplies two quat's
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a the first operand
    // * @param {quat} b the second operand
    // * @returns {quat} output
    // */
    public static float[] Multiply(float[] output, float[] a, float[] b)
    {
        float ax = a[0]; float ay = a[1]; float az = a[2]; float aw = a[3];
        float bx = b[0]; float by = b[1]; float bz = b[2]; float bw = b[3];

        output[0] = ax * bw + aw * bx + ay * bz - az * by;
        output[1] = ay * bw + aw * by + az * bx - ax * bz;
        output[2] = az * bw + aw * bz + ax * by - ay * bx;
        output[3] = aw * bw - ax * bx - ay * by - az * bz;
        return output;
    }

    ///**
    // * Alias for {@link quat.multiply}
    // * @function
    // */
    public static float[] Mul(float[] output, float[] a, float[] b)
    {
        return Multiply(output, a, b);
    }

    ///**
    // * Scales a quat by a scalar number
    // *
    // * @param {quat} output the receiving vector
    // * @param {quat} a the vector to scale
    // * @param {Number} b amount to scale the vector by
    // * @returns {quat} output
    // * @function
    // */
    //quat.scale = vec4.scale;
    public static float[] Scale(float[] output, float[] a, float b)
    {
        return Vec4.Scale(output, a, b);
    }

    ///**
    // * Rotates a quaternion by the given angle aboutput the X axis
    // *
    // * @param {quat} output quat receiving operation result
    // * @param {quat} a quat to rotate
    // * @param {number} rad angle (in radians) to rotate
    // * @returns {quat} output
    // */
    public static float[] RotateX(float[] output, float[] a, float rad)
    {
        rad /= 2;

        float ax = a[0]; float ay = a[1]; float az = a[2]; float aw = a[3];
        float bx = Platform.Sin(rad); float bw = Platform.Cos(rad);

        output[0] = ax * bw + aw * bx;
        output[1] = ay * bw + az * bx;
        output[2] = az * bw - ay * bx;
        output[3] = aw * bw - ax * bx;
        return output;
    }

    ///**
    // * Rotates a quaternion by the given angle aboutput the Y axis
    // *
    // * @param {quat} output quat receiving operation result
    // * @param {quat} a quat to rotate
    // * @param {number} rad angle (in radians) to rotate
    // * @returns {quat} output
    // */
    public static float[] RotateY(float[] output, float[] a, float rad)
    {
        rad /= 2;

        float ax = a[0]; float ay = a[1]; float az = a[2]; float aw = a[3];
        float by = Platform.Sin(rad); float bw = Platform.Cos(rad);

        output[0] = ax * bw - az * by;
        output[1] = ay * bw + aw * by;
        output[2] = az * bw + ax * by;
        output[3] = aw * bw - ay * by;
        return output;
    }

    ///**
    // * Rotates a quaternion by the given angle aboutput the Z axis
    // *
    // * @param {quat} output quat receiving operation result
    // * @param {quat} a quat to rotate
    // * @param {number} rad angle (in radians) to rotate
    // * @returns {quat} output
    // */
    public static float[] RotateZ(float[] output, float[] a, float rad)
    {
        rad /= 2;

        float ax = a[0]; float ay = a[1]; float az = a[2]; float aw = a[3];
        float bz = Platform.Sin(rad); float bw = Platform.Cos(rad);

        output[0] = ax * bw + ay * bz;
        output[1] = ay * bw - ax * bz;
        output[2] = az * bw + aw * bz;
        output[3] = aw * bw - az * bz;
        return output;
    }

    ///**
    // * Calculates the W component of a quat from the X, Y, and Z components.
    // * Assumes that quaternion is 1 unit in length.
    // * Any existing W component will be ignored.
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a quat to calculate W component of
    // * @returns {quat} output
    // */
    public static float[] CalculateW(float[] output, float[] a)
    {
        float x = a[0]; float y = a[1]; float z = a[2];

        output[0] = x;
        output[1] = y;
        output[2] = z;
        float one = 1;
        output[3] = -Platform.Sqrt(GlMatrixMath.Abs(one - x * x - y * y - z * z));
        return output;
    }

    ///**
    // * Calculates the dot product of two quat's
    // *
    // * @param {quat} a the first operand
    // * @param {quat} b the second operand
    // * @returns {Number} dot product of a and b
    // * @function
    // */
    public static float Dot(float[] a, float[] b)
    {
        return Vec4.Dot(a, b);
    }

    ///**
    // * Performs a linear interpolation between two quat's
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a the first operand
    // * @param {quat} b the second operand
    // * @param {Number} t interpolation amount between the two inputs
    // * @returns {quat} output
    // * @function
    // */
    public static float[] Lerp(float[] output, float[] a, float[] b, float t)
    {
        return Vec4.Lerp(output, a, b, t);
    }

    ///**
    // * Performs a spherical linear interpolation between two quat
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a the first operand
    // * @param {quat} b the second operand
    // * @param {Number} t interpolation amount between the two inputs
    // * @returns {quat} output
    // */
    //quat.slerp = function (output, a, b, t) {
    public static float[] Slerp(float[] output, float[] a, float[] b, float t)
    {
        //    // benchmarks:
        //    //    http://jsperf.com/quaternion-slerp-implementations

        float ax = a[0]; float ay = a[1]; float az = a[2]; float aw = a[3];
        float bx = b[0]; float by = b[1]; float bz = b[2]; float bw = b[3];

        float omega; float cosom; float sinom; float scale0; float scale1;

        // calc cosine
        cosom = ax * bx + ay * by + az * bz + aw * bw;
        // adjust signs (if necessary)
        if (cosom < 0)
        {
            cosom = -cosom;
            bx = -bx;
            by = -by;
            bz = -bz;
            bw = -bw;
        }
        float one = 1;
        float epsilon = one / 1000000;
        // calculate coefficients
        if ((one - cosom) > epsilon)
        {
            // standard case (slerp)
            omega = Platform.Acos(cosom);
            sinom = Platform.Sin(omega);
            scale0 = Platform.Sin((one - t) * omega) / sinom;
            scale1 = Platform.Sin(t * omega) / sinom;
        }
        else
        {
            // "from" and "to" quaternions are very close 
            //  ... so we can do a linear interpolation
            scale0 = one - t;
            scale1 = t;
        }
        // calculate final values
        output[0] = scale0 * ax + scale1 * bx;
        output[1] = scale0 * ay + scale1 * by;
        output[2] = scale0 * az + scale1 * bz;
        output[3] = scale0 * aw + scale1 * bw;

        return output;
    }

    ///**
    // * Calculates the inverse of a quat
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a quat to calculate inverse of
    // * @returns {quat} output
    // */
    public float[] Invert(float[] output, float[] a)
    {
        float a0 = a[0]; float a1 = a[1]; float a2 = a[2]; float a3 = a[3];
        float dot = a0 * a0 + a1 * a1 + a2 * a2 + a3 * a3;
        float one = 1;
        float invDot = (dot != 0) ? one / dot : 0;

        // TODO: Would be faster to return [0,0,0,0] immediately if dot == 0

        output[0] = -a0 * invDot;
        output[1] = -a1 * invDot;
        output[2] = -a2 * invDot;
        output[3] = a3 * invDot;
        return output;
    }

    ///**
    // * Calculates the conjugate of a quat
    // * If the quaternion is normalized, this function is faster than quat.inverse and produces the same result.
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a quat to calculate conjugate of
    // * @returns {quat} output
    // */
    public float[] Conjugate(float[] output, float[] a)
    {
        output[0] = -a[0];
        output[1] = -a[1];
        output[2] = -a[2];
        output[3] = a[3];
        return output;
    }

    ///**
    // * Calculates the length of a quat
    // *
    // * @param {quat} a vector to calculate length of
    // * @returns {Number} length of a
    // * @function
    // */
    //quat.length = vec4.length;
    public static float Length_(float[] a)
    {
        return Vec4.Length_(a);
    }

    ///**
    // * Alias for {@link quat.length}
    // * @function
    // */
    public static float Len(float[] a)
    {
        return Length_(a);
    }

    ///**
    // * Calculates the squared length of a quat
    // *
    // * @param {quat} a vector to calculate squared length of
    // * @returns {Number} squared length of a
    // * @function
    // */
    public static float SquaredLength(float[] a)
    {
        return Vec4.SquaredLength(a);
    }

    ///**
    // * Alias for {@link quat.squaredLength}
    // * @function
    // */
    public static float SqrLen(float[] a)
    {
        return SquaredLength(a);
    }

    ///**
    // * Normalize a quat
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {quat} a quaternion to normalize
    // * @returns {quat} output
    // * @function
    // */
    public static float[] Normalize(float[] output, float[] a)
    {
        return Vec4.Normalize(output, a);
    }

    ///**
    // * Creates a quaternion from the given 3x3 rotation matrix.
    // *
    // * NOTE: The resultant quaternion is not normalized, so you should be sure
    // * to renormalize the quaternion yourself where necessary.
    // *
    // * @param {quat} output the receiving quaternion
    // * @param {mat3} m rotation matrix
    // * @returns {quat} output
    // * @function
    // */
    public static float[] FromMat3(float[] output, float[] m)
    {
        // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
        // article "Quaternion Calculus and Fast Animation".
        float fTrace = m[0] + m[4] + m[8];
        float fRoot;

        float zero = 0;
        float one = 1;
        float half = one / 2;
        if (fTrace > zero)
        {
            // |w| > 1/2, may as well choose w > 1/2
            fRoot = Platform.Sqrt(fTrace + one);  // 2w
            output[3] = half * fRoot;
            fRoot = half / fRoot;  // 1/(4w)
            output[0] = (m[7] - m[5]) * fRoot;
            output[1] = (m[2] - m[6]) * fRoot;
            output[2] = (m[3] - m[1]) * fRoot;
        }
        else
        {
            // |w| <= 1/2
            int i = 0;
            if (m[4] > m[0])
                i = 1;
            if (m[8] > m[i * 3 + i])
                i = 2;
            int j = (i + 1) % 3;
            int k = (i + 2) % 3;

            fRoot = Platform.Sqrt(m[i * 3 + i] - m[j * 3 + j] - m[k * 3 + k] + one);
            output[i] = half * fRoot;
            fRoot = half / fRoot;
            output[3] = (m[k * 3 + j] - m[j * 3 + k]) * fRoot;
            output[j] = (m[j * 3 + i] + m[i * 3 + j]) * fRoot;
            output[k] = (m[k * 3 + i] + m[i * 3 + k]) * fRoot;
        }

        return output;
    }

    ///**
    // * Returns a string representation of a quatenion
    // *
    // * @param {quat} vec vector to represent as a string
    // * @returns {String} string representation of the vector
    // */
    //quat.str = function (a) {
    //    return 'quat(' + a[0] + ', ' + a[1] + ', ' + a[2] + ', ' + a[3] + ')';
    //};

    //if(typeof(exports) !== 'undefined') {
    //    exports.quat = quat;
    //}
    void f()
    {
    }
}

public class Vec2
{
    //    /**
    // * @class 2 Dimensional Vector
    // * @name vec2
    // */
    //var vec2 = {};

    ///**
    // * Creates a new, empty vec2
    // *
    // * @returns {vec2} a new 2D vector
    // */
    public static float[] Create()
    {
        float[] output = new float[2];
        output[0] = 0;
        output[1] = 0;
        return output;
    }

    ///**
    // * Creates a new vec2 initialized with values from an existing vector
    // *
    // * @param {vec2} a vector to clone
    // * @returns {vec2} a new 2D vector
    // */
    public static float[] CloneIt(float[] a)
    {
        float[] output = new float[2];
        output[0] = a[0];
        output[1] = a[1];
        return output;
    }

    ///**
    // * Creates a new vec2 initialized with the given values
    // *
    // * @param {Number} x X component
    // * @param {Number} y Y component
    // * @returns {vec2} a new 2D vector
    // */
    public static float[] FromValues(float x, float y)
    {
        float[] output = new float[2];
        output[0] = x;
        output[1] = y;
        return output;
    }

    ///**
    // * Copy the values from one vec2 to another
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the source vector
    // * @returns {vec2} output
    // */
    public static float[] Copy(float[] output, float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        return output;
    }

    ///**
    // * Set the components of a vec2 to the given values
    // *
    // * @param {vec2} output the receiving vector
    // * @param {Number} x X component
    // * @param {Number} y Y component
    // * @returns {vec2} output
    // */
    public static float[] Set(float[] output, float x, float y)
    {
        output[0] = x;
        output[1] = y;
        return output;
    }

    ///**
    // * Adds two vec2's
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {vec2} output
    // */
    public static float[] Add(float[] output, float[] a, float[] b)
    {
        output[0] = a[0] + b[0];
        output[1] = a[1] + b[1];
        return output;
    }

    ///**
    // * Subtracts vector b from vector a
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {vec2} output
    // */
    public static float[] Subtract(float[] output, float[] a, float[] b)
    {
        output[0] = a[0] - b[0];
        output[1] = a[1] - b[1];
        return output;
    }

    ///**
    // * Alias for {@link vec2.subtract}
    // * @function
    // */
    public static float[] Sub(float[] output, float[] a, float[] b)
    {
        return Subtract(output, a, b);
    }

    ///**
    // * Multiplies two vec2's
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {vec2} output
    // */
    public static float[] Multiply(float[] output, float[] a, float[] b)
    {
        output[0] = a[0] * b[0];
        output[1] = a[1] * b[1];
        return output;
    }

    ///**
    // * Alias for {@link vec2.multiply}
    // * @function
    // */
    public static float[] Mul(float[] output, float[] a, float[] b)
    {
        return Multiply(output, a, b);
    }

    ///**
    // * Divides two vec2's
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {vec2} output
    // */
    public static float[] Divide(float[] output, float[] a, float[] b)
    {
        output[0] = a[0] / b[0];
        output[1] = a[1] / b[1];
        return output;
    }

    ///**
    // * Alias for {@link vec2.divide}
    // * @function
    // */
    public static float[] Div(float[] output, float[] a, float[] b)
    {
        return Divide(output, a, b);
    }

    ///**
    // * Returns the minimum of two vec2's
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {vec2} output
    // */
    public static float[] Min(float[] output, float[] a, float[] b)
    {
        output[0] = GlMatrixMath.min(a[0], b[0]);
        output[1] = GlMatrixMath.min(a[1], b[1]);
        return output;
    }

    ///**
    // * Returns the maximum of two vec2's
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {vec2} output
    // */
    public static float[] Max(float[] output, float[] a, float[] b)
    {
        output[0] = GlMatrixMath.max(a[0], b[0]);
        output[1] = GlMatrixMath.max(a[1], b[1]);
        return output;
    }

    ///**
    // * Scales a vec2 by a scalar number
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the vector to scale
    // * @param {Number} b amount to scale the vector by
    // * @returns {vec2} output
    // */
    public static float[] Scale(float[] output, float[] a, float b)
    {
        output[0] = a[0] * b;
        output[1] = a[1] * b;
        return output;
    }

    ///**
    // * Adds two vec2's after scaling the second operand by a scalar value
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @param {Number} scale the amount to scale b by before adding
    // * @returns {vec2} output
    // */
    public static float[] ScaleAndAdd(float[] output, float[] a, float[] b, float scale)
    {
        output[0] = a[0] + (b[0] * scale);
        output[1] = a[1] + (b[1] * scale);
        return output;
    }

    ///**
    // * Calculates the euclidian distance between two vec2's
    // *
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {Number} distance between a and b
    // */
    public static float Distance(float[] a, float[] b)
    {
        float x = b[0] - a[0];
        float y = b[1] - a[1];
        return Platform.Sqrt(x * x + y * y);
    }

    ///**
    // * Alias for {@link vec2.distance}
    // * @function
    // */
    public static float Dist(float[] a, float[] b)
    {
        return Distance(a, b);
    }

    ///**
    // * Calculates the squared euclidian distance between two vec2's
    // *
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {Number} squared distance between a and b
    // */
    public static float SquaredDistance(float[] a, float[] b)
    {
        float x = b[0] - a[0];
        float y = b[1] - a[1];
        return x * x + y * y;
    }

    ///**
    // * Alias for {@link vec2.squaredDistance}
    // * @function
    // */
    //vec2.sqrDist = vec2.squaredDistance;
    public static float SqrDist(float[] a, float[] b)
    {
        return SquaredDistance(a, b);
    }

    ///**
    // * Calculates the length of a vec2
    // *
    // * @param {vec2} a vector to calculate length of
    // * @returns {Number} length of a
    // */
    public static float Length_(float[] a)
    {
        float x = a[0];
        float y = a[1];
        return Platform.Sqrt(x * x + y * y);
    }

    ///**
    // * Alias for {@link vec2.length}
    // * @function
    // */
    public static float Len(float[] a)
    {
        return Length_(a);
    }

    ///**
    // * Calculates the squared length of a vec2
    // *
    // * @param {vec2} a vector to calculate squared length of
    // * @returns {Number} squared length of a
    // */
    public static float SquaredLength(float[] a)
    {
        float x = a[0];
        float y = a[1];
        return x * x + y * y;
    }

    ///**
    // * Alias for {@link vec2.squaredLength}
    // * @function
    // */
    public static float SqrLen(float[] a)
    {
        return SquaredLength(a);
    }

    ///**
    // * Negates the components of a vec2
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a vector to negate
    // * @returns {vec2} output
    // */
    public static float[] Negate(float[] output, float[] a)
    {
        output[0] = -a[0];
        output[1] = -a[1];
        return output;
    }

    ///**
    // * Normalize a vec2
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a vector to normalize
    // * @returns {vec2} output
    // */
    public static float[] Normalize(float[] output, float[] a)
    {
        float x = a[0];
        float y = a[1];
        float len = x * x + y * y;
        if (len > 0)
        {
            //TODO: evaluate use of glm_invsqrt here?
            len = 1 / Platform.Sqrt(len);
            output[0] = a[0] * len;
            output[1] = a[1] * len;
        }
        return output;
    }

    ///**
    // * Calculates the dot product of two vec2's
    // *
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {Number} dot product of a and b
    // */
    public static float Dot(float[] a, float[] b)
    {
        return a[0] * b[0] + a[1] * b[1];
    }

    ///**
    // * Computes the cross product of two vec2's
    // * Note that the cross product must by definition produce a 3D vector
    // *
    // * @param {vec3} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @returns {vec3} output
    // */
    public static float[] Cross(float[] output, float[] a, float[] b)
    {
        float z = a[0] * b[1] - a[1] * b[0];
        output[0] = output[1] = 0;
        output[2] = z;
        return output;
    }

    ///**
    // * Performs a linear interpolation between two vec2's
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the first operand
    // * @param {vec2} b the second operand
    // * @param {Number} t interpolation amount between the two inputs
    // * @returns {vec2} output
    // */
    public static float[] Lerp(float[] output, float[] a, float[] b, float t)
    {
        float ax = a[0];
        float ay = a[1];
        output[0] = ax + t * (b[0] - ax);
        output[1] = ay + t * (b[1] - ay);
        return output;
    }

    //**
    // * Generates a random vector with the given scale
    // *
    // * @param {vec2} output the receiving vector
    // * @param {Number} [scale] Length of the resulting vector. If ommitted, a unit vector will be returned
    // * @returns {vec2} output
    // */
    //public static float[] Random(float[] output, float scale)
    //{
    //    //scale = scale || 1.0;
    //    float r = Platform.Random() * 2 * GlMatrixMath.PI();
    //    output[0] = Platform.Cos(r) * scale;
    //    output[1] = Platform.Sin(r) * scale;
    //    return output;
    //}

    ///**
    // * Transforms the vec2 with a mat2
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the vector to transform
    // * @param {mat2} m matrix to transform with
    // * @returns {vec2} output
    // */
    public static float[] TransformMat2(float[] output, float[] a, float[] m)
    {
        float x = a[0];
        float y = a[1];
        output[0] = m[0] * x + m[2] * y;
        output[1] = m[1] * x + m[3] * y;
        return output;
    }

    ///**
    // * Transforms the vec2 with a mat2d
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the vector to transform
    // * @param {mat2d} m matrix to transform with
    // * @returns {vec2} output
    // */
    public static float[] TransformMat2d(float[] output, float[] a, float[] m)
    {
        float x = a[0];
        float y = a[1];
        output[0] = m[0] * x + m[2] * y + m[4];
        output[1] = m[1] * x + m[3] * y + m[5];
        return output;
    }

    ///**
    // * Transforms the vec2 with a mat3
    // * 3rd vector component is implicitly '1'
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the vector to transform
    // * @param {mat3} m matrix to transform with
    // * @returns {vec2} output
    // */
    public static float[] TransformMat3(float[] output, float[] a, float[] m)
    {
        float x = a[0];
        float y = a[1];
        output[0] = m[0] * x + m[3] * y + m[6];
        output[1] = m[1] * x + m[4] * y + m[7];
        return output;
    }

    ///**
    // * Transforms the vec2 with a mat4
    // * 3rd vector component is implicitly '0'
    // * 4th vector component is implicitly '1'
    // *
    // * @param {vec2} output the receiving vector
    // * @param {vec2} a the vector to transform
    // * @param {mat4} m matrix to transform with
    // * @returns {vec2} output
    // */
    public static float[] TransformMat4(float[] output, float[] a, float[] m)
    {
        float x = a[0];
        float y = a[1];
        output[0] = m[0] * x + m[4] * y + m[12];
        output[1] = m[1] * x + m[5] * y + m[13];
        return output;
    }

    ///**
    // * Perform some operation over an array of vec2s.
    // *
    // * @param {Array} a the array of vectors to iterate over
    // * @param {Number} stride Number of elements between the start of each vec2. If 0 assumes tightly packed
    // * @param {Number} offset Number of elements to skip at the beginning of the array
    // * @param {Number} count Number of vec2s to iterate over. If 0 iterates over entire array
    // * @param {Function} fn Function to call for each vector in the array
    // * @param {Object} [arg] additional argument to pass to fn
    // * @returns {Array} a
    // * @function
    // */
    //vec2.forEach = (function() {
    //    var vec = vec2.create();

    //    return function(a, stride, offset, count, fn, arg) {
    //        var i, l;
    //        if(!stride) {
    //            stride = 2;
    //        }

    //        if(!offset) {
    //            offset = 0;
    //        }

    //        if(count) {
    //            l = Math.min((count * stride) + offset, a.length);
    //        } else {
    //            l = a.length;
    //        }

    //        for(i = offset; i < l; i += stride) {
    //            vec[0] = a[i]; vec[1] = a[i+1];
    //            fn(vec, vec, arg);
    //            a[i] = vec[0]; a[i+1] = vec[1];
    //        }

    //        return a;
    //    };
    //})();

    ///**
    // * Returns a string representation of a vector
    // *
    // * @param {vec2} vec vector to represent as a string
    // * @returns {String} string representation of the vector
    // */
    //vec2.str = function (a) {
    //    return 'vec2(' + a[0] + ', ' + a[1] + ')';
    //};

    //if(typeof(exports) !== 'undefined') {
    //    exports.vec2 = vec2;
    //}
    void f()
    {
    }
}

/// 3 Dimensional Vector
public class Vec3
{
    /// Creates a new, empty vec3
    /// Returns {vec3} a new 3D vector.
    public static float[] Create()
    {
        float[] output = new float[3];
        output[0] = 0;
        output[1] = 0;
        output[2] = 0;
        return output;
    }

    /// Creates a new vec3 initialized with values from an existing vector
    /// Returns {vec3} a new 3D vector
    public static float[] CloneIt(
        /// a vector to clone
        float[] a)
    {
        float[] output = new float[3];
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        return output;
    }

    /// Creates a new vec3 initialized with the given values
    /// Returns {vec3} a new 3D vector
    public static float[] FromValues(
        /// X component
        float x,
        /// Y component
        float y,
        /// Z component
        float z)
    {
        float[] output = new float[3];
        output[0] = x;
        output[1] = y;
        output[2] = z;
        return output;
    }

    /// Copy the values from one vec3 to another
    ///@returns {vec3} out
    public static float[] Copy(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the source vector
        float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        return output;
    }

    ///Set the components of a vec3 to the given values
    ///@returns {vec3} out
    public static float[] Set(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {Number} x X component
        float x,
        ////@param {Number} y Y component
        float y,
        ////@param {Number} z Z component
        float z)
    {
        output[0] = x;
        output[1] = y;
        output[2] = z;
        return output;
    }

    ///Adds two vec3's
    ///@returns {vec3} out
    public static float[] Add(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        output[0] = a[0] + b[0];
        output[1] = a[1] + b[1];
        output[2] = a[2] + b[2];
        return output;
    }

    ///Subtracts vector b from vector a
    ///@returns {vec3} out
    public static float[] Substract(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        output[0] = a[0] - b[0];
        output[1] = a[1] - b[1];
        output[2] = a[2] - b[2];
        return output;
    }

    ///Alias for {@link vec3.subtract}
    ///@function
    //vec3.sub = vec3.subtract;
    public static float[] Sub(float[] output, float[] a, float[] b)
    {
        return Substract(output, a, b);
    }

    ///Multiplies two vec3's
    ///@returns {vec3} out
    public static float[] Multiply(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        output[0] = a[0] * b[0];
        output[1] = a[1] * b[1];
        output[2] = a[2] * b[2];
        return output;
    }

    ///Alias for {@link vec3.multiply}
    public static float[] Mul(float[] output, float[] a, float[] b)
    {
        return Multiply(output, a, b);
    }

    ///Divides two vec3's
    ///@returns {vec3} out
    public static float[] Divide(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        output[0] = a[0] / b[0];
        output[1] = a[1] / b[1];
        output[2] = a[2] / b[2];
        return output;
    }

    ///Alias for {@link vec3.divide}
    public static float[] Div(float[] output, float[] a, float[] b)
    {
        return Divide(output, a, b);
    }

    ///Returns the minimum of two vec3's
    ///@returns {vec3} out
    public static float[] Min(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        output[0] = GlMatrixMath.min(a[0], b[0]);
        output[1] = GlMatrixMath.min(a[1], b[1]);
        output[2] = GlMatrixMath.min(a[2], b[2]);
        return output;
    }

    ///Returns the maximum of two vec3's
    ///@returns {vec3} out
    public static float[] Max(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        output[0] = GlMatrixMath.max(a[0], b[0]);
        output[1] = GlMatrixMath.max(a[1], b[1]);
        output[2] = GlMatrixMath.max(a[2], b[2]);
        return output;
    }

    ///Scales a vec3 by a scalar number
    ///@returns {vec3} out
    public static float[] Scale(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the vector to scale
        float[] a,
        ////@param {Number} b amount to scale the vector by
        float b)
    {
        output[0] = a[0] * b;
        output[1] = a[1] * b;
        output[2] = a[2] * b;
        return output;
    }

    ///Adds two vec3's after scaling the second operand by a scalar value
    ///@returns {vec3} out
    public static float[] ScaleAndAdd(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b,
        ////@param {Number} scale the amount to scale b by before adding
        float scale)
    {
        output[0] = a[0] + (b[0] * scale);
        output[1] = a[1] + (b[1] * scale);
        output[2] = a[2] + (b[2] * scale);
        return output;
    }

    ///Calculates the euclidian distance between two vec3's
    ///@returns {Number} distance between a and b
    public static float Distance(
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        float x = b[0] - a[0];
        float y = b[1] - a[1];
        float z = b[2] - a[2];
        return Platform.Sqrt(x * x + y * y + z * z);
    }

    ///Alias for {@link vec3.distance}
    public static float Dist(float[] a, float[] b)
    {
        return Distance(a, b);
    }

    ///Calculates the squared euclidian distance between two vec3's
    ///@returns {Number} squared distance between a and b
    public static float SquaredDistance(
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        float x = b[0] - a[0];
        float y = b[1] - a[1];
        float z = b[2] - a[2];
        return x * x + y * y + z * z;
    }

    ///Alias for {@link vec3.squaredDistance}
    ///@function
    //vec3.sqrDist = vec3.squaredDistance;
    public static float SqrDist(float[] a, float[] b)
    {
        return SquaredDistance(a, b);
    }

    ///Calculates the length of a vec3
    ///@returns {Number} length of a
    public static float Length_(
        ////@param {vec3} a vector to calculate length of
        float[] a)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];
        return Platform.Sqrt(x * x + y * y + z * z);
    }

    ///Alias for {@link vec3.length}
    public static float Len(float[] a)
    {
        return Length_(a);
    }

    ///Calculates the squared length of a vec3
    ///@returns {Number} squared length of a
    public static float SquaredLength(
        ////@param {vec3} a vector to calculate squared length of
        float[] a)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];
        return x * x + y * y + z * z;
    }

    ///Alias for {@link vec3.squaredLength}
    public static float SqrLen(float[] a)
    {
        return SquaredLength(a);
    }

    ///Negates the components of a vec3
    ///@returns {vec3} out
    public static float[] Negate(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a vector to negate
        float[] a)
    {
        output[0] = 0 - a[0];
        output[1] = 0 - a[1];
        output[2] = 0 - a[2];
        return output;
    }

    ///Normalize a vec3
    ///@returns {vec3} out
    public static float[] Normalize(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a vector to normalize
        float[] a)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];
        float len = x * x + y * y + z * z;
        if (len > 0)
        {
            //TODO: evaluate use of glm_invsqrt here?
            float one = 1;
            len = one / Platform.Sqrt(len);
            output[0] = a[0] * len;
            output[1] = a[1] * len;
            output[2] = a[2] * len;
        }
        return output;
    }

    ///Calculates the dot product of two vec3's
    ///@returns {Number} dot product of a and b
    public static float Dot(
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    ///Computes the cross product of two vec3's
    ///@returns {vec3} out
    public static float[] Cross(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b)
    {
        float ax = a[0];
        float ay = a[1];
        float az = a[2];
        float bx = b[0];
        float by = b[1];
        float bz = b[2];

        output[0] = ay * bz - az * by;
        output[1] = az * bx - ax * bz;
        output[2] = ax * by - ay * bx;

        return output;
    }

    ///Performs a linear interpolation between two vec3's
    ///@returns {vec3} out
    public static float[] Lerp(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the first operand
        float[] a,
        ////@param {vec3} b the second operand
        float[] b,
        ////@param {Number} t interpolation amount between the two inputs
        float t)
    {
        float ax = a[0];
        float ay = a[1];
        float az = a[2];
        output[0] = ax + t * (b[0] - ax);
        output[1] = ay + t * (b[1] - ay);
        output[2] = az + t * (b[2] - az);
        return output;
    }

    //Generates a random vector with the given scale
    //@returns {vec3} out
    //public static float[] Random(
    //    ////@param {vec3} out the receiving vector
    //    float[] output,
    //    ////@param {Number} [scale] Length of the resulting vector. If ommitted, a unit vector will be returned
    //    float scale)
    //{
    //    //float scale = scale || 1.0;
    //    float one = 1;
    //    float two = 2;

    //    float r = Platform.Random() * two * GlMatrixMath.PI();
    //    float z = (Platform.Random() * two) - one;
    //    float zScale = Platform.Sqrt(one - z * z) * scale;

    //    output[0] = Platform.Cos(r) * zScale;
    //    output[1] = Platform.Sin(r) * zScale;
    //    output[2] = z * scale;
    //    return output;
    //}

    ////Transforms the vec3 with a mat4.
    ////4th vector component is implicitly '1'
    ////@returns {vec3} out
    public static float[] TransformMat4(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the vector to transform
        float[] a,
        ////@param {mat4} m matrix to transform with
        float[] m)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];
        output[0] = m[0] * x + m[4] * y + m[8] * z + m[12];
        output[1] = m[1] * x + m[5] * y + m[9] * z + m[13];
        output[2] = m[2] * x + m[6] * y + m[10] * z + m[14];
        return output;
    }

    ///Transforms the vec3 with a mat3.
    ///@returns {vec3} out
    public static float[] TransformMat3(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the vector to transform
        float[] a,
        ////@param {mat4} m the 3x3 matrix to transform with
        float[] m)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];
        output[0] = x * m[0] + y * m[3] + z * m[6];
        output[1] = x * m[1] + y * m[4] + z * m[7];
        output[2] = x * m[2] + y * m[5] + z * m[8];
        return output;
    }

    ///Transforms the vec3 with a quat
    ///@returns {vec3} out
    //    // benchmarks: http://jsperf.com/quaternion-transform-vec3-implementations
    public static float[] TransformQuat(
        ////@param {vec3} out the receiving vector
        float[] output,
        ////@param {vec3} a the vector to transform
        float[] a,
        ////@param {quat} q quaternion to transform with
        float[] q)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];

        float qx = q[0];
        float qy = q[1];
        float qz = q[2];
        float qw = q[3];

        // calculate quat * vec
        float ix = qw * x + qy * z - qz * y;
        float iy = qw * y + qz * x - qx * z;
        float iz = qw * z + qx * y - qy * x;
        float iw = (0 - qx) * x - qy * y - qz * z;

        // calculate result * inverse quat
        output[0] = ix * qw + iw * (0 - qx) + iy * (0 - qz) - iz * (0 - qy);
        output[1] = iy * qw + iw * (0 - qy) + iz * (0 - qx) - ix * (0 - qz);
        output[2] = iz * qw + iw * (0 - qz) + ix * (0 - qy) - iy * (0 - qx);
        return output;
    }

    ////Perform some operation over an array of vec3s.
    ////@param {Array} a the array of vectors to iterate over
    ////@param {Number} stride Number of elements between the start of each vec3. If 0 assumes tightly packed
    ////@param {Number} offset Number of elements to skip at the beginning of the array
    ////@param {Number} count Number of vec3s to iterate over. If 0 iterates over entire array
    ////@param {Function} fn Function to call for each vector in the array
    ////@param {Object} [arg] additional argument to pass to fn
    ////@returns {Array} a
    //vec3.forEach = (function() {
    //    var vec = vec3.create();

    //    return function(a, stride, offset, count, fn, arg) {
    //        var i, l;
    //        if(!stride) {
    //            stride = 3;
    //        }

    //        if(!offset) {
    //            offset = 0;
    //        }

    //        if(count) {
    //            l = Math.min((count * stride) + offset, a.length);
    //        } else {
    //            l = a.length;
    //        }

    //        for(i = offset; i < l; i += stride) {
    //            vec[0] = a[i]; vec[1] = a[i+1]; vec[2] = a[i+2];
    //            fn(vec, vec, arg);
    //            a[i] = vec[0]; a[i+1] = vec[1]; a[i+2] = vec[2];
    //        }

    //        return a;
    //    };
    //})();

    ////
    //// Returns a string representation of a vector
    ////
    //// @param {vec3} vec vector to represent as a string
    //// @returns {String} string representation of the vector
    //vec3.str = function (a) {
    //    return 'vec3(' + a[0] + ', ' + a[1] + ', ' + a[2] + ')';
    //};
    public static string str(float[] a)
    {
        return "";
    }

    //if(typeof(exports) !== 'undefined') {
    //    exports.vec3 = vec3;
    //}
}

public class Vec4
{
    //    /**
    // * @class 4 Dimensional Vector
    // * @name vec4
    // */
    //var vec4 = {};

    ///**
    // * Creates a new, empty vec4
    // *
    // * @returns {vec4} a new 4D vector
    // */
    public static float[] Create()
    {
        float[] output = new float[4];
        output[0] = 0;
        output[1] = 0;
        output[2] = 0;
        output[3] = 0;
        return output;
    }

    ///**
    // * Creates a new vec4 initialized with values from an existing vector
    // *
    // * @param {vec4} a vector to clone
    // * @returns {vec4} a new 4D vector
    // */
    public static float[] CloneIt(float[] a)
    {
        float[] output = new float[4];
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        return output;
    }

    ///**
    // * Creates a new vec4 initialized with the given values
    // *
    // * @param {Number} x X component
    // * @param {Number} y Y component
    // * @param {Number} z Z component
    // * @param {Number} w W component
    // * @returns {vec4} a new 4D vector
    // */
    public static float[] FromValues(float x, float y, float z, float w)
    {
        float[] output = new float[4];
        output[0] = x;
        output[1] = y;
        output[2] = z;
        output[3] = w;
        return output;
    }

    ///**
    // * Copy the values from one vec4 to another
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the source vector
    // * @returns {vec4} output
    // */
    public static float[] Copy(float[] output, float[] a)
    {
        output[0] = a[0];
        output[1] = a[1];
        output[2] = a[2];
        output[3] = a[3];
        return output;
    }

    ///**
    // * Set the components of a vec4 to the given values
    // *
    // * @param {vec4} output the receiving vector
    // * @param {Number} x X component
    // * @param {Number} y Y component
    // * @param {Number} z Z component
    // * @param {Number} w W component
    // * @returns {vec4} output
    // */
    public static float[] Set(float[] output, float x, float y, float z, float w)
    {
        output[0] = x;
        output[1] = y;
        output[2] = z;
        output[3] = w;
        return output;
    }

    ///**
    // * Adds two vec4's
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {vec4} output
    // */
    public static float[] Add(float[] output, float[] a, float[] b)
    {
        output[0] = a[0] + b[0];
        output[1] = a[1] + b[1];
        output[2] = a[2] + b[2];
        output[3] = a[3] + b[3];
        return output;
    }

    ///**
    // * Subtracts vector b from vector a
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {vec4} output
    // */
    public static float[] Subtract(float[] output, float[] a, float[] b)
    {
        output[0] = a[0] - b[0];
        output[1] = a[1] - b[1];
        output[2] = a[2] - b[2];
        output[3] = a[3] - b[3];
        return output;
    }

    ///**
    // * Alias for {@link vec4.subtract}
    // * @function
    // */
    public static float[] Sub(float[] output, float[] a, float[] b)
    {
        return Subtract(output, a, b);
    }

    ///**
    // * Multiplies two vec4's
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {vec4} output
    // */
    public static float[] Multiply(float[] output, float[] a, float[] b)
    {
        output[0] = a[0] * b[0];
        output[1] = a[1] * b[1];
        output[2] = a[2] * b[2];
        output[3] = a[3] * b[3];
        return output;
    }

    ///**
    // * Alias for {@link vec4.multiply}
    // * @function
    // */
    //vec4.mul = vec4.multiply;
    public static float[] Mul(float[] output, float[] a, float[] b)
    {
        return Multiply(output, a, b);
    }

    ///**
    // * Divides two vec4's
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {vec4} output
    // */
    public static float[] Divide(float[] output, float[] a, float[] b)
    {
        output[0] = a[0] / b[0];
        output[1] = a[1] / b[1];
        output[2] = a[2] / b[2];
        output[3] = a[3] / b[3];
        return output;
    }

    ///**
    // * Alias for {@link vec4.divide}
    // * @function
    // */
    //vec4.div = vec4.divide;
    public static float[] Div(float[] output, float[] a, float[] b)
    {
        return Divide(output, a, b);
    }

    ///**
    // * Returns the minimum of two vec4's
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {vec4} output
    // */
    public static float[] Min(float[] output, float[] a, float[] b)
    {
        output[0] = GlMatrixMath.min(a[0], b[0]);
        output[1] = GlMatrixMath.min(a[1], b[1]);
        output[2] = GlMatrixMath.min(a[2], b[2]);
        output[3] = GlMatrixMath.min(a[3], b[3]);
        return output;
    }

    ///**
    // * Returns the maximum of two vec4's
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {vec4} output
    // */
    public static float[] Max(float[] output, float[] a, float[] b)
    {
        output[0] = GlMatrixMath.max(a[0], b[0]);
        output[1] = GlMatrixMath.max(a[1], b[1]);
        output[2] = GlMatrixMath.max(a[2], b[2]);
        output[3] = GlMatrixMath.max(a[3], b[3]);
        return output;
    }

    ///**
    // * Scales a vec4 by a scalar number
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the vector to scale
    // * @param {Number} b amount to scale the vector by
    // * @returns {vec4} output
    // */
    public static float[] Scale(float[] output, float[] a, float b)
    {
        output[0] = a[0] * b;
        output[1] = a[1] * b;
        output[2] = a[2] * b;
        output[3] = a[3] * b;
        return output;
    }

    ///**
    // * Adds two vec4's after scaling the second operand by a scalar value
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @param {Number} scale the amount to scale b by before adding
    // * @returns {vec4} output
    // */
    public static float[] ScaleAndAdd(float[] output, float[] a, float[] b, float scale)
    {
        output[0] = a[0] + (b[0] * scale);
        output[1] = a[1] + (b[1] * scale);
        output[2] = a[2] + (b[2] * scale);
        output[3] = a[3] + (b[3] * scale);
        return output;
    }

    ///**
    // * Calculates the euclidian distance between two vec4's
    // *
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {Number} distance between a and b
    // */
    public static float Distance(float[] a, float[] b)
    {
        float x = b[0] - a[0];
        float y = b[1] - a[1];
        float z = b[2] - a[2];
        float w = b[3] - a[3];
        return Platform.Sqrt(x * x + y * y + z * z + w * w);
    }

    ///**
    // * Alias for {@link vec4.distance}
    // * @function
    // */
    //vec4.dist = vec4.distance;
    public static float Dist(float[] a, float[] b)
    {
        return Distance(a, b);
    }

    ///**
    // * Calculates the squared euclidian distance between two vec4's
    // *
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {Number} squared distance between a and b
    // */
    public static float SquaredDistance(float[] a, float[] b)
    {
        float x = b[0] - a[0];
        float y = b[1] - a[1];
        float z = b[2] - a[2];
        float w = b[3] - a[3];
        return x * x + y * y + z * z + w * w;
    }

    ///**
    // * Alias for {@link vec4.squaredDistance}
    // * @function
    // */
    public static float SqrDist(float[] a, float[] b)
    {
        return SquaredDistance(a, b);
    }
    ///**
    // * Calculates the length of a vec4
    // *
    // * @param {vec4} a vector to calculate length of
    // * @returns {Number} length of a
    // */
    public static float Length_(float[] a)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];
        float w = a[3];
        return Platform.Sqrt(x * x + y * y + z * z + w * w);
    }

    ///**
    // * Alias for {@link vec4.length}
    // * @function
    // */
    public static float Len(float[] a)
    {
        return Length_(a);
    }

    ///**
    // * Calculates the squared length of a vec4
    // *
    // * @param {vec4} a vector to calculate squared length of
    // * @returns {Number} squared length of a
    // */
    public static float SquaredLength(float[] a)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];
        float w = a[3];
        return x * x + y * y + z * z + w * w;
    }

    ///**
    // * Alias for {@link vec4.squaredLength}
    // * @function
    // */
    //vec4.sqrLen = vec4.squaredLength;
    public static float SqrLen(float[] a)
    {
        return SquaredLength(a);
    }

    ///**
    // * Negates the components of a vec4
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a vector to negate
    // * @returns {vec4} output
    // */
    public static float[] Negate(float[] output, float[] a)
    {
        output[0] = -a[0];
        output[1] = -a[1];
        output[2] = -a[2];
        output[3] = -a[3];
        return output;
    }

    ///**
    // * Normalize a vec4
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a vector to normalize
    // * @returns {vec4} output
    // */
    public static float[] Normalize(float[] output, float[] a)
    {
        float x = a[0];
        float y = a[1];
        float z = a[2];
        float w = a[3];
        float len = x * x + y * y + z * z + w * w;
        if (len > 0)
        {
            float one = 1;
            len = one / Platform.Sqrt(len);
            output[0] = a[0] * len;
            output[1] = a[1] * len;
            output[2] = a[2] * len;
            output[3] = a[3] * len;
        }
        return output;
    }

    ///**
    // * Calculates the dot product of two vec4's
    // *
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @returns {Number} dot product of a and b
    // */
    public static float Dot(float[] a, float[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2] + a[3] * b[3];
    }

    ///**
    // * Performs a linear interpolation between two vec4's
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the first operand
    // * @param {vec4} b the second operand
    // * @param {Number} t interpolation amount between the two inputs
    // * @returns {vec4} output
    // */
    public static float[] Lerp(float[] output, float[] a, float[] b, float t)
    {
        float ax = a[0];
        float ay = a[1];
        float az = a[2];
        float aw = a[3];
        output[0] = ax + t * (b[0] - ax);
        output[1] = ay + t * (b[1] - ay);
        output[2] = az + t * (b[2] - az);
        output[3] = aw + t * (b[3] - aw);
        return output;
    }

    //**
    // * Generates a random vector with the given scale
    // *
    // * @param {vec4} output the receiving vector
    // * @param {Number} [scale] Length of the resulting vector. If ommitted, a unit vector will be returned
    // * @returns {vec4} output
    // */
    //public static float[] Random(float[] output, float scale)
    //{
    //    //scale = scale || 1.0;

    //    //TODO: This is a pretty awful way of doing this. Find something better.
    //    output[0] = Platform.Random();
    //    output[1] = Platform.Random();
    //    output[2] = Platform.Random();
    //    output[3] = Platform.Random();
    //    Vec4.Normalize(output, output);
    //    Vec4.Scale(output, output, scale);
    //    return output;
    //}

    ///**
    // * Transforms the vec4 with a mat4.
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the vector to transform
    // * @param {mat4} m matrix to transform with
    // * @returns {vec4} output
    // */
    public static float[] TransformMat4(float[] output, float[] a, float[] m)
    {
        float x = a[0]; float y = a[1]; float z = a[2]; float w = a[3];
        output[0] = m[0] * x + m[4] * y + m[8] * z + m[12] * w;
        output[1] = m[1] * x + m[5] * y + m[9] * z + m[13] * w;
        output[2] = m[2] * x + m[6] * y + m[10] * z + m[14] * w;
        output[3] = m[3] * x + m[7] * y + m[11] * z + m[15] * w;
        return output;
    }

    ///**
    // * Transforms the vec4 with a quat
    // *
    // * @param {vec4} output the receiving vector
    // * @param {vec4} a the vector to transform
    // * @param {quat} q quaternion to transform with
    // * @returns {vec4} output
    // */
    public static float[] transformQuat(float[] output, float[] a, float[] q)
    {
        float x = a[0]; float y = a[1]; float z = a[2];
        float qx = q[0]; float qy = q[1]; float qz = q[2]; float qw = q[3];

        // calculate quat * vec
        float ix = qw * x + qy * z - qz * y;
        float iy = qw * y + qz * x - qx * z;
        float iz = qw * z + qx * y - qy * x;
        float iw = -qx * x - qy * y - qz * z;

        // calculate result * inverse quat
        output[0] = ix * qw + iw * -qx + iy * -qz - iz * -qy;
        output[1] = iy * qw + iw * -qy + iz * -qx - ix * -qz;
        output[2] = iz * qw + iw * -qz + ix * -qy - iy * -qx;
        return output;
    }

    ///**
    // * Perform some operation over an array of vec4s.
    // *
    // * @param {Array} a the array of vectors to iterate over
    // * @param {Number} stride Number of elements between the start of each vec4. If 0 assumes tightly packed
    // * @param {Number} offset Number of elements to skip at the beginning of the array
    // * @param {Number} count Number of vec2s to iterate over. If 0 iterates over entire array
    // * @param {Function} fn Function to call for each vector in the array
    // * @param {Object} [arg] additional argument to pass to fn
    // * @returns {Array} a
    // * @function
    // */
    //vec4.forEach = (function() {
    //    var vec = vec4.create();

    //    return function(a, stride, offset, count, fn, arg) {
    //        var i, l;
    //        if(!stride) {
    //            stride = 4;
    //        }

    //        if(!offset) {
    //            offset = 0;
    //        }

    //        if(count) {
    //            l = Math.min((count * stride) + offset, a.length);
    //        } else {
    //            l = a.length;
    //        }

    //        for(i = offset; i < l; i += stride) {
    //            vec[0] = a[i]; vec[1] = a[i+1]; vec[2] = a[i+2]; vec[3] = a[i+3];
    //            fn(vec, vec, arg);
    //            a[i] = vec[0]; a[i+1] = vec[1]; a[i+2] = vec[2]; a[i+3] = vec[3];
    //        }

    //        return a;
    //    };
    //})();

    ///**
    // * Returns a string representation of a vector
    // *
    // * @param {vec4} vec vector to represent as a string
    // * @returns {String} string representation of the vector
    // */
    //vec4.str = function (a) {
    //    return 'vec4(' + a[0] + ', ' + a[1] + ', ' + a[2] + ', ' + a[3] + ')';
    //};

    //if(typeof(exports) !== 'undefined') {
    //    exports.vec4 = vec4;
    //}

    void f()
    {
    }
}

public class Platform
{
    public static float Sqrt(float a)
    {
#if CS
        native
        {
            return (float)System.Math.Sqrt(a);
        }
        return 0;
#elif JS
        native
        {
            return Math.sqrt(a);
        }
        return 0;
#elif C
        native
        {
            return sqrt(a);
        }
        return 0;
#elif C99
        native
        {
            return sqrt(a);
        }
        return 0;
#elif PHP
        native
        {
            return sqrt("$a");
        }
        return 0;
#elif JAVA
        float ret = 0;
        native
        {
            ret = (float)Math.sqrt(a);
        }
        return ret;
#elif D
        float ret = 0;
        native
        {
            ret = std.math.sqrt(a);
        }
        return ret;
#elif AS
        float ret = 0;
        native
        {
            ret = Math.sqrt(a);
        }
        return ret;
#else
#if CITO
        return 0;
#else
        return (float)System.Math.Sqrt(a);
#endif
#endif
    }

    public static float Cos(float a)
    {
#if CS
        native
        {
            return (float)System.Math.Cos(a);
        }
        return 0;
#elif JS
        native
        {
            return Math.cos(a);
        }
        return 0;
#elif C
        native
        {
            return cos(a);
        }
        return 0;
#elif C99
        native
        {
            return cos(a);
        }
        return 0;
#elif PHP
        native
        {
            return cos("$a");
        }
        return 0;
#elif JAVA
        float ret = 0;
        native
        {
            ret = (float)Math.cos(a);
        }
        return ret;
#elif D
        float ret = 0;
        native
        {
            ret = std.math.cos(a);
        }
        return ret;
#elif AS
        float ret = 0;
        native
        {
            ret = Math.cos(a);
        }
        return ret;
#else
#if CITO
        return 0;
#else
        return (float)System.Math.Cos(a);
#endif
#endif
    }

    public static float Sin(float a)
    {
#if CS
        native
        {
            return (float)System.Math.Sin(a);
        }
        return 0;
#elif JS
        native
        {
            return Math.sin(a);
        }
        return 0;
#elif C
        native
        {
            return sin(a);
        }
        return 0;
#elif C99
        native
        {
            return sin(a);
        }
        return 0;
#elif PHP
        native
        {
            return sin("$a");
        }
        return 0;
#elif JAVA
        float ret = 0;
        native
        {
            ret = (float)Math.sin(a);
        }
        return ret;
#elif D
        float ret = 0;
        native
        {
            ret = std.math.sin(a);
        }
        return ret;
#elif AS
        float ret = 0;
        native
        {
            ret = Math.sin(a);
        }
        return ret;
#else
#if CITO
        return 0;
#else
        return (float)System.Math.Sin(a);
#endif
#endif
    }

    //public static float Random()
    //{
    //    return 0;
    //}

    public static float Tan(float a)
    {
#if CS
        native
        {
            return (float)System.Math.Tan(a);
        }
        return 0;
#elif JS
        native
        {
            return Math.tan(a);
        }
        return 0;
#elif C
        native
        {
            return tan(a);
        }
        return 0;
#elif C99
        native
        {
            return tan(a);
        }
        return 0;
#elif PHP
        native
        {
            return tan("$a");
        }
        return 0;
#elif JAVA
        float ret = 0;
        native
        {
            ret = (float)Math.tan(a);
        }
        return ret;
#elif D
        float ret = 0;
        native
        {
            ret = std.math.tan(a);
        }
        return ret;
#elif AS
        float ret = 0;
        native
        {
            ret = Math.tan(a);
        }
        return ret;
#else
#if CITO
        return 0;
#else
        return (float)System.Math.Tan(a);
#endif
#endif
    }

    public static float Acos(float a)
    {
#if CS
        native
        {
            return (float)System.Math.Acos(a);
        }
        return 0;
#elif JS
        native
        {
            return Math.acos(a);
        }
        return 0;
#elif C
        native
        {
            return acos(a);
        }
        return 0;
#elif C99
        native
        {
            return acos(a);
        }
        return 0;
#elif PHP
        native
        {
            return acos("$a");
        }
        return 0;
#elif JAVA
        float ret = 0;
        native
        {
            ret = (float)Math.acos(a);
        }
        return ret;
#elif D
        float ret = 0;
        native
        {
            ret = std.math.acos(a);
        }
        return ret;
#elif AS
        float ret = 0;
        native
        {
            ret = Math.acos(a);
        }
        return ret;
#else
#if CITO
        return 0;
#else
        return (float)System.Math.Acos(a);
#endif
#endif
    }

    public static void WriteString(string a)
    {
#if CS
        native
        {
            System.Console.Write(a);
        }
#elif JS
        native
        {
            console.log(a);
        }
#elif C
        native
        {
            printf("%s", a);
        }
#elif C99
        native
        {
            printf("%s", a);
        }
#elif PHP
        native
        {
            echo("$a");
        }
#elif JAVA
        native
        {
            System.out.println(a);
        }
#elif D
        native
        {
            std.stdio.write(a);
        }
#elif AS
        native
        {
            trace(a);
        }
#else
#if CITO
#else
        System.Console.Write(a);
#endif
#endif
    }

    public static void WriteInt(int a)
    {
#if CS
        native
        {
            System.Console.Write(a);
        }
#elif JS
        native
        {
            console.log(a);
        }
#elif C
        native
        {
            printf("%i", a);
        }
#elif C99
        native
        {
            printf("%i", a);
        }
#elif PHP
        native
        {
            echo("$a");
        }
#elif JAVA
        native
        {
            System.out.println(a);
        }
#elif D
        native
        {
            std.stdio.write(a);
        }
#elif AS
        native
        {
            trace(a);
        }
#else
#if CITO
#else
        System.Console.Write(a);
#endif
#endif
    }
}

public class GlMatrixMath
{
    public static float min(float a, float b)
    {
        if (a < b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static float max(float a, float b)
    {
        if (a > b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static float PI()
    {
        float a = 3141592;
        return a / 1000000;
    }

    public static float Abs(float len)
    {
        if (len < 0)
        {
            return -len;
        }
        else
        {
            return len;
        }
    }

    public static float GLMAT_EPSILON()
    {
        float one = 1;
        return one / 1000000;
    }
}

#if TESTS

public class Tests
{
    public static void RunAll()
    {
        TestVec3 testvec3 = new TestVec3();
        testvec3.Test();
        TestMat4 testmat4 = new TestMat4();
        testmat4.Test();
    }
}

public class TestVec3
{
    public void Test()
    {
        citoassert = new CitoAssert();
        ResetTests();
        TransformMat4(); ResetTests();
        Create(); ResetTests();
        CloneIt(); ResetTests();
        FromValues(); ResetTests();
        Copy(); ResetTests();
        Set(); ResetTests();
        Add(); ResetTests();
        Subtract(); ResetTests();
        Multiply(); ResetTests();
        Divide(); ResetTests();
        Min(); ResetTests();
        Max(); ResetTests();
        Scale(); ResetTests();
        ScaleAndAdd(); ResetTests();
        Distance(); ResetTests();
        SquaredDistance(); ResetTests();
        Length_(); ResetTests();
        SquaredLength(); ResetTests();
        Negate(); ResetTests();
        Normalize(); ResetTests();
        Dot(); ResetTests();
        Cross(); ResetTests();
        Lerp(); ResetTests();
        //Random(); ResetTests();
        ForEachDo(); ResetTests();
        Str(); ResetTests();
    }

    void ResetTests()
    {
        vecA = Arr3(1, 2, 3);
        vecB = Arr3(4, 5, 6);
        output = Arr3(0, 0, 0);
    }

    float[] vecA;
    float[] vecB;
    float[] output;

    void TransformMat4()
    {
        TransformMat4WithAnIdentity();
        TransformMat4WithALookAt();
        TransformMat3WithAnIdentity();
        TransformMat3With90DegAboutX();
        TransformMat3With90DegAboutY();
        TransformMat3With90DegAboutZ();
        TransformMat3WithALookAtNormalMatrix();
    }

    void TransformMat4WithAnIdentity()
    {
        float[] matr = Arr16(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        float[] result = Vec3.TransformMat4(output, vecA, matr);
        AssertArrayEqual(output, Arr3(1, 2, 3), 3, "TransformMat4WithAnIdentity should produce the input");
        AssertArrayEqual(result, output, 3, "TransformMat4WithAnIdentity should return output");
    }

    void TransformMat4WithALookAt()
    {
        float[] matr = Mat4.LookAt(Mat4.Create(), Arr3(5, 6, 7), Arr3(2, 6, 7), Arr3(0, 1, 0));
        float[] result = Vec3.TransformMat4(output, vecA, matr);
        AssertArrayEqual(output, Arr3(4, -4, -4), 3, "TransformMat4WithALookAt should rotate and translate the input");
        AssertArrayEqual(result, output, 3, "TransformMat4WithALookAt should return out");
    }

    void TransformMat3WithAnIdentity()
    {
        float[] matr = Arr9(1, 0, 0, 0, 1, 0, 0, 0, 1);
        float[] result = Vec3.TransformMat3(output, vecA, matr);
        AssertArrayEqual(output, Arr3(1, 2, 3), 3, "TransformMat3WithAnIdentity should produce the input");
        AssertArrayEqual(result, output, 3, "TransformMat3WithAnIdentity should return output");
    }

    void TransformMat3With90DegAboutX()
    {
        float[] result = Vec3.TransformMat3(output, Arr3(0, 1, 0), Arr9(1, 0, 0, 0, 0, 1, 0, -1, 0));
        AssertArrayEqual(output, Arr3(0, 0, 1), 3, "TransformMat3With90DegAboutX should produce correct output");
    }

    void TransformMat3With90DegAboutY()
    {
        float[] result = Vec3.TransformMat3(output, Arr3(1, 0, 0), Arr9(0, 0, -1, 0, 1, 0, 1, 0, 0));
        AssertArrayEqual(output, Arr3(0, 0, -1), 3, "TransformMat3With90DegAboutU should produce correct output");
    }

    void TransformMat3With90DegAboutZ()
    {
        float[] result = Vec3.TransformMat3(output, Arr3(1, 0, 0), Arr9(0, 1, 0, -1, 0, 0, 0, 0, 1));
        AssertArrayEqual(output, Arr3(0, 1, 0), 3, "TransformMat3With90DegAboutZ should produce correct output");
    }

    void TransformMat3WithALookAtNormalMatrix()
    {
        float[] matr = Mat4.LookAt(Mat4.Create(), Arr3(5, 6, 7), Arr3(2, 6, 7), Arr3(0, 1, 0));
        float[] n = Mat3.Create();
        matr = Mat3.Transpose(n, Mat3.Invert(n, Mat3.FromMat4(n, matr)));
        float[] result = Vec3.TransformMat3(output, Arr3(1, 0, 0), matr);

        AssertArrayEqual(output, Arr3(0, 0, 1), 3, "TransformMat3WithALookAtNormalMatrix should rotate the input");
        AssertArrayEqual(result, output, 3, "TransformMat3WithALookAtNormalMatrix should return output");
    }

    void Create()
    {
        float[] result = Vec3.Create();
        AssertArrayEqual(result, Arr3(0, 0, 0), 3, "Create should return a 3 element array initialized to 0s");
    }

    void CloneIt()
    {
        float[] result = Vec3.CloneIt(vecA);
        AssertArrayEqual(result, vecA, 3, "Clone should return a 3 element array initialized to the values in vecA");
    }

    void FromValues()
    {
        float[] result = Vec3.FromValues(1, 2, 3);
        AssertArrayEqual(result, Arr3(1, 2, 3), 3, "FromValues should return a 3 element array initialized to the values passed");
    }

    void Copy()
    {
        float[] result = Vec3.Copy(output, vecA);
        AssertArrayEqual(output, Arr3(1, 2, 3), 3, "Copy should place values into out");
        AssertArrayEqual(result, output, 3, "Copy should return output");
    }

    void Set()
    {
        float[] result = Vec3.Set(output, 1, 2, 3);
        AssertArrayEqual(output, Arr3(1, 2, 3), 3, "Set should place values into output");
        AssertArrayEqual(result, output, 3, "Set should return output");
    }

    void Add()
    {
        AddWithASeparateOutputVector();
        AddWhenVecAIsTheOutputVector();
        AddWhenVecBIsTheOutputVector();
    }

    void AddWithASeparateOutputVector()
    {
        float[] result = Vec3.Add(output, vecA, vecB);
        AssertArrayEqual(output, Arr3(5, 7, 9), 3, "Add should place values into out");
        AssertArrayEqual(result, output, 3, "Add should return out");
        AssertArrayEqual(vecA, Arr3(1, 2, 3), 3, "Add should not modify vecA");
        AssertArrayEqual(vecB, Arr3(4, 5, 6), 3, "Add should not modify vecB");
    }

    void AddWhenVecAIsTheOutputVector()
    {
    }

    void AddWhenVecBIsTheOutputVector()
    {
    }

    void Subtract()
    {
        SubtractShouldHaveAnAliasCalledSub();
        SubtractWithASeparateOutputVector();
        SubtractWhenVecAIsTheOutputVector();
        SubtractWhenVecBIsTheOutputVector();
    }

    void SubtractShouldHaveAnAliasCalledSub()
    {
    }

    void SubtractWithASeparateOutputVector()
    {
    }

    void SubtractWhenVecAIsTheOutputVector()
    {
    }

    void SubtractWhenVecBIsTheOutputVector()
    {
    }

    void Multiply()
    {
        MultiplyWithASeparateOutputVector();
        MultiplyWhenVecAIsTheOutputVector();
        MultiplyWhenVecBIsTheOutputVector();
    }

    void MultiplyWithASeparateOutputVector()
    {
    }

    void MultiplyWhenVecAIsTheOutputVector()
    {
    }

    void MultiplyWhenVecBIsTheOutputVector()
    {
    }

    void Divide()
    {
        DivideWithASeparateOutputVector();
        DivideWhenVecAIsTheOutputVector();
        DivideWhenVecBIsTheOutputVector();
    }

    void DivideWithASeparateOutputVector()
    {
    }

    void DivideWhenVecAIsTheOutputVector()
    {
    }

    void DivideWhenVecBIsTheOutputVector()
    {
    }

    void Min()
    {
        MinWithASeparateOutputVector();
        MinWhenVecAIsTheOutputVector();
        MinWhenVecBIsTheOutputVector();
    }

    void MinWithASeparateOutputVector()
    {
    }

    void MinWhenVecAIsTheOutputVector()
    {
    }

    void MinWhenVecBIsTheOutputVector()
    {
    }

    void Max()
    {
        MaxWithASeparateOutputVector();
        MaxWhenVecAIsTheOutputVector();
        MaxWhenVecBIsTheOutputVector();
    }

    void MaxWithASeparateOutputVector()
    {
    }

    void MaxWhenVecAIsTheOutputVector()
    {
    }

    void MaxWhenVecBIsTheOutputVector()
    {
    }

    void Scale()
    {
        ScaleWithASeparateOutputVector();
        ScaleWhenVecAIsTheOutputVector();
    }

    void ScaleWithASeparateOutputVector()
    {
    }

    void ScaleWhenVecAIsTheOutputVector()
    {
    }

    void ScaleAndAdd()
    {
        ScaleAndAddWithASeparateOutputVector();
        ScaleAndAddWhenVecAIsTheOutputVector();
        ScaleAndAddWhenVecBIsTheOutputVector();
    }

    void ScaleAndAddWithASeparateOutputVector()
    {
    }

    void ScaleAndAddWhenVecAIsTheOutputVector()
    {
    }

    void ScaleAndAddWhenVecBIsTheOutputVector()
    {
    }

    void Distance()
    {
        float result = Vec3.Distance(vecA, vecB);
        float r = 5196152;
        r /= 1000 * 1000; // 5.196152
        AssertCloseTo(result, r, "Distance should return the distance");
    }

    void SquaredDistance()
    {
        float result = Vec3.SquaredDistance(vecA, vecB);
        AssertEqual(result, 27, "SquaredDistance should return the squared distance");
    }

    void Length_()
    {
        float result = Vec3.Length_(vecA);
        float r = 3741657;
        r /= 1000 * 1000;// 3.741657
        AssertCloseTo(result, r, "Length should return the length");
    }

    void SquaredLength()
    {
        float result = Vec3.SquaredLength(vecA);
        AssertEqual(result, 14, "SquaredLength should return the squared length");
    }

    void Negate()
    {
        NegateWithASeparateOutputVector();
        NegateWhenVecAIsTheOutputVector();
    }

    void NegateWithASeparateOutputVector()
    {
        float[] result = Vec3.Negate(output, vecA);
        AssertArrayEqual(output, Arr3(-1, -2, -3), 3, "NegateWithASeparateOutputVector should place values into out");
        AssertArrayEqual(result, output, 3, "NegateWithASeparateOutputVector should should return out");
        AssertArrayEqual(vecA, Arr3(1, 2, 3), 3, "NegateWithASeparateOutputVector should not modify vecA");
    }

    void NegateWhenVecAIsTheOutputVector()
    {
        float[] result = Vec3.Negate(vecA, vecA);
        AssertArrayEqual(vecA, Arr3(-1, -2, -3), 3, "NegateWhenVecAIsTheOutputVector should place values into vecA");
        AssertArrayEqual(result, vecA, 3, "NegateWhenVecAIsTheOutputVector should return vecA");
    }

    void Normalize()
    {
        NormalizeWithASeparateOutputVector();
        NormalizeWhenVecAIsTheOutputVector();
    }

    void NormalizeWithASeparateOutputVector()
    {
        vecA = Arr3(5, 0, 0);
        float[] result = Vec3.Normalize(output, vecA);
        AssertArrayEqual(output, Arr3(1, 0, 0), 3, "NormalizeWithASeparateOutputVector should place values into out");
        AssertArrayEqual(result, output, 3, "NormalizeWithASeparateOutputVector should return out");
        AssertArrayEqual(vecA, Arr3(5, 0, 0), 3, "NormalizeWithASeparateOutputVector should not modify vecA");
    }

    void NormalizeWhenVecAIsTheOutputVector()
    {
        float[] vecA1 = Arr3(5, 0, 0);
        float[] result = Vec3.Normalize(vecA, vecA);
        AssertArrayEqual(vecA, Arr3(1, 0, 0), 3, "NormalizeWhenVecAIsTheOutputVector should place values into vecA");
        AssertArrayEqual(result, vecA, 3, "NormalizeWhenVecAIsTheOutputVector should return vecA");
    }

    void Dot()
    {
        float result = Vec3.Dot(vecA, vecB);
        AssertEqual(result, 32, "Dot should return the dot product");
        AssertArrayEqual(vecA, Arr3(1, 2, 3), 3, "Dot should not modify vecA");
        AssertArrayEqual(vecB, Arr3(4, 5, 6), 3, "Dot should not modify vecB");
    }

    void Cross()
    {
        CrossWithASeparateOutputVector();
        CrossWhenVecAIsTheOutputVector();
        CrossWhenVecBIsTheOutputVector();
    }

    void CrossWithASeparateOutputVector()
    {
    }

    void CrossWhenVecAIsTheOutputVector()
    {
    }

    void CrossWhenVecBIsTheOutputVector()
    {
    }

    void Lerp()
    {
        LerpWithASeparateOutputVector();
        LerpWhenVecAIsTheOutputVector();
        LerpWhenVecBIsTheOutputVector();
    }

    void LerpWithASeparateOutputVector()
    {
    }

    void LerpWhenVecAIsTheOutputVector()
    {
    }

    void LerpWhenVecBIsTheOutputVector()
    {
    }

    //void Random()
    //{
    //}

    void ForEachDo()
    {
    }

    void Str()
    {
    }

    void AssertEqual(float actual, float expected, string msg)
    {
        citoassert.AssertEqual(actual, expected, msg);
    }

    void AssertCloseTo(float actual, float expected, string msg)
    {
        citoassert.AssertCloseTo(actual, expected, msg);
    }

    void AssertArrayEqual(float[] actual, float[] expected, int length, string msg)
    {
        citoassert.AssertArrayEqual(actual, expected, length, msg);
    }

    float[] Arr3(float p, float p_2, float p_3)
    {
        return citoassert.Arr3(p, p_2, p_3);
    }

    float[] Arr9(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9)
    {
        return citoassert.Arr9(p, p_2, p_3, p_4, p_5, p_6, p_7, p_8, p_9);
    }

    float[] Arr16(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9, int p_10, int p_11, int p_12, int p_13, int p_14, int p_15, int p_16)
    {
        return citoassert.Arr16(p, p_2, p_3, p_4, p_5, p_6, p_7, p_8, p_9, p_10, p_11, p_12, p_13, p_14, p_15, p_16);
    }

    CitoAssert citoassert;
}

public class TestMat4
{
    public void Test()
    {
        citoassert = new CitoAssert();
        ResetTests();
        Create(); ResetTests();
        CloneIt(); ResetTests();
        Copy(); ResetTests();
        Identity_(); ResetTests();
        Transpose(); ResetTests();
        Invert(); ResetTests();
        Adjoint(); ResetTests();
        Determinant(); ResetTests();
        Multiply(); ResetTests();
        Translate(); ResetTests();
        Scale(); ResetTests();
        Rotate(); ResetTests();
        RotateX(); ResetTests();
        RotateY(); ResetTests();
        RotateZ(); ResetTests();
        Frustum(); ResetTests();
        Perspective(); ResetTests();
        Ortho(); ResetTests();
        LookAt(); ResetTests();
        Str(); ResetTests();
    }

    CitoAssert citoassert;
    float[] matA;
    float[] matB;
    float[] output;
    float[] identity;

    void ResetTests()
    {
        // Attempting to portray a semi-realistic transform matrix
        matA = Arr16(1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                1, 2, 3, 1);
        matB = Arr16(1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                4, 5, 6, 1);

        output = Arr16(0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0);

        identity = Arr16(1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);
    }

    void Create()
    {
        float[] result = Mat4.Create();
        AssertArrayEqual(result, identity, 16, "Create should return a 16 element array initialized to a 4x4 identity matrix");
    }

    void CloneIt()
    {
        float[] result = Mat4.CloneIt(matA);
        AssertArrayEqual(result, matA, 16, "Clone should return a 16 element array initialized to the values in matA");
    }

    void Copy()
    {
        float[] result = Mat4.Copy(output, matA);
        AssertArrayEqual(output, matA, 16, "Copy should place values into out");
        AssertArrayEqual(result, output, 16, "Copy should return out");
    }

    void Identity_()
    {
        float[] result = Mat4.Identity_(output);
        AssertArrayEqual(output, identity, 16, "Copy should place values into out");
        AssertArrayEqual(result, output, 16, "Copy should return out");
    }

    void Transpose()
    {
        TransposeWithASeparateOutputMatrix();
        TransposeWhenMatAIsTheOutputMatrix();
    }

    void TransposeWithASeparateOutputMatrix()
    {
    }

    void TransposeWhenMatAIsTheOutputMatrix()
    {
    }

    void Invert()
    {
        InvertWithASeparateOutputMatrix();
        InvertWhenMatAIsTheOutputMatrix();
    }

    void InvertWithASeparateOutputMatrix()
    {

    }

    void InvertWhenMatAIsTheOutputMatrix()
    {

    }

    void Adjoint()
    {
        AdjointWithASeparateOutputMatrix();
        AdjointWhenMatAIsTheOutputMatrix();
    }

    void AdjointWithASeparateOutputMatrix()
    {

    }

    void AdjointWhenMatAIsTheOutputMatrix()
    {

    }

    void Determinant()
    {

    }

    void Multiply()
    {
        MultiplyWithASeparateOutputMatrix();
        MultiplyWhenMatAIsTheOutputMatrix();
        MultiplyWhenMatBIsTheOutputMatrix();
    }

    void MultiplyWithASeparateOutputMatrix()
    {

    }

    void MultiplyWhenMatAIsTheOutputMatrix()
    {

    }

    void MultiplyWhenMatBIsTheOutputMatrix()
    {

    }

    void Translate()
    {
        TranslateWithASeparateOutputMatrix();
        TranslateWhenMatAIsTheOutputMatrix();
    }

    void TranslateWithASeparateOutputMatrix()
    {

    }

    void TranslateWhenMatAIsTheOutputMatrix()
    {

    }

    void Scale()
    {
        ScaleWithASeparateOutputMatrix();
        ScaleWhenMatAIsTheOutputMatrix();
    }

    void ScaleWithASeparateOutputMatrix()
    {

    }

    void ScaleWhenMatAIsTheOutputMatrix()
    {

    }

    void Rotate()
    {
        RotateWithASeparateOutputMatrix();
        RotateWhenMatAIsTheOutputMatrix();
    }

    void RotateWithASeparateOutputMatrix()
    {

    }

    void RotateWhenMatAIsTheOutputMatrix()
    {

    }

    void RotateX()
    {
        RotateXWithASeparateOutputMatrix();
        RotateXWhenMatAIsTheOutputMatrix();
    }

    void RotateXWithASeparateOutputMatrix()
    {

    }

    void RotateXWhenMatAIsTheOutputMatrix()
    {

    }

    void RotateY()
    {
        RotateYWithASeparateOutputMatrix();
        RotateYWhenMatAIsTheOutputMatrix();
    }

    void RotateYWithASeparateOutputMatrix()
    {

    }

    void RotateYWhenMatAIsTheOutputMatrix()
    {

    }

    void RotateZ()
    {
        RotateZWithASeparateOutputMatrix();
        RotateZWhenMatAIsTheOutputMatrix();
    }

    void RotateZWithASeparateOutputMatrix()
    {

    }

    void RotateZWhenMatAIsTheOutputMatrix()
    {

    }

    void Frustum()
    {
        float[] result = Mat4.Frustum(output, -1, 1, -1, 1, -1, 1);
        AssertArrayEqual(result, Arr16(-1, 0, 0, 0,
                0, -1, 0, 0,
                0, 0, 0, -1,
                0, 0, 1, 0), 16, "Frustum should place values into out");
        AssertArrayEqual(result, output, 16, "Frustum should return out");
    }

    void Perspective()
    {
        Perspective1();
        PerspectiveWithNonzeroNear45degFovyAndRealisticAspectRatio();
    }

    void Perspective1()
    {

    }

    void PerspectiveWithNonzeroNear45degFovyAndRealisticAspectRatio()
    {

    }

    void Ortho()
    {
        float[] result = Mat4.Ortho(output, -1, 1, -1, 1, -1, 1);
        AssertArrayEqual(result, Arr16(1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, -1, 0,
                0, 0, 0, 1), 16, "Ortho should place values into out");
        AssertArrayEqual(result, output, 16, "Ortho should return out");
    }

    void LookAt()
    {
        eye = Arr3(0, 0, 1);
        center = Arr3(0, 0, -1);
        up = Arr3(0, 1, 0);

        LookAtLookingDown();
        LookAt74();
        LookAt3();
    }

    float[] eye;
    float[] center;
    float[] up;
    float[] view;
    float[] right;

    void LookAtLookingDown()
    {
        view = Arr3(0, -1, 0);
        up = Arr3(0, 0, -1);
        right = Arr3(1, 0, 0);

        float[] result = Mat4.LookAt(output, Arr3(0, 0, 0), view, up);

        result = Vec3.TransformMat4(Vec3.Create(), view, output);
        AssertArrayEqual(result, Arr3(0, 0, -1), 3, "LookAtLookingDown should transform view into local -Z");

        result = Vec3.TransformMat4(Vec3.Create(), up, output);
        AssertArrayEqual(result, Arr3(0, 1, 0), 3, "LookAtLookingDownshould transform up into local +Y");

        result = Vec3.TransformMat4(Vec3.Create(), right, output);
        AssertArrayEqual(result, Arr3(1, 0, 0), 3, "LookAtLookingDownshould transform right into local +X");

        AssertArrayEqual(result, output, 3, "LookAtLookingDown should return out");
    }

    void LookAt74()
    {
        float six = 6;
        Mat4.LookAt(output, Arr3(0, 2, 0), Arr3(0, six / 10, 0), Arr3(0, 0, -1));

        float[] result = Vec3.TransformMat4(Vec3.Create(), Arr3(0, 2, -1), output);
        AssertArrayEqual(result, Arr3(0, 1, 0), 3, "LookAt74 should transform a point 'above' into local +Y");

        result = Vec3.TransformMat4(Vec3.Create(), Arr3(1, 2, 0), output);
        AssertArrayEqual(result, Arr3(1, 0, 0), 3, "LookAt74 should transform a point 'right of' into local +X");

        result = Vec3.TransformMat4(Vec3.Create(), Arr3(0, 1, 0), output);
        AssertArrayEqual(result, Arr3(0, 0, -1), 3, "LookAt74 should transform a point 'in front of' into local -Z");
    }

    void LookAt3()
    {

    }

    void Str()
    {

    }


    void AssertEqual(float actual, float expected, string msg)
    {
        citoassert.AssertEqual(actual, expected, msg);
    }

    void AssertCloseTo(float actual, float expected, string msg)
    {
        citoassert.AssertCloseTo(actual, expected, msg);
    }

    void AssertArrayEqual(float[] actual, float[] expected, int length, string msg)
    {
        citoassert.AssertArrayEqual(actual, expected, length, msg);
    }

    float[] Arr3(float p, float p_2, float p_3)
    {
        float[] arr = citoassert.Arr3(p, p_2, p_3);
        arr[0] = arr[0]; // fix for a problem with Cito D generator
        return arr;
    }

    float[] Arr9(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9)
    {
        return citoassert.Arr9(p, p_2, p_3, p_4, p_5, p_6, p_7, p_8, p_9);
    }

    float[] Arr16(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9, int p_10, int p_11, int p_12, int p_13, int p_14, int p_15, int p_16)
    {
        float[] arr = citoassert.Arr16(p, p_2, p_3, p_4, p_5, p_6, p_7, p_8, p_9, p_10, p_11, p_12, p_13, p_14, p_15, p_16);
        arr[0] = arr[0]; // fix for a problem with Cito D generator
        return arr;
    }
}

public class CitoAssert
{
    public CitoAssert()
    {
        errors = new string[1024];
        errorsCount = 0;
        testI = 0;
    }

    string[] errors;
    int errorsCount;

    int testI;

    public void AssertEqual(float actual, float expected, string msg)
    {
        Platform.WriteString("Test ");
        Platform.WriteInt(testI);
        if (actual != expected)
        {
            errors[errorsCount++] = msg;
            Platform.WriteString(" error: ");
            Platform.WriteString(msg);
        }
        else
        {
            Platform.WriteString(" ok");
        }
        Platform.WriteString("\n");
        testI++;
    }

    public void AssertCloseTo(float actual, float expected, string msg)
    {
        Platform.WriteString("Test ");
        Platform.WriteInt(testI);
        if (GlMatrixMath.Abs(actual - expected) > GlMatrixMath.GLMAT_EPSILON())
        {
            errors[errorsCount++] = msg;
            Platform.WriteString(" error: ");
            Platform.WriteString(msg);
        }
        else
        {
            Platform.WriteString(" ok");
        }
        Platform.WriteString("\n");
        testI++;
    }

    public void AssertArrayEqual(float[] actual, float[] expected, int length, string msg)
    {
        Platform.WriteString("Test ");
        Platform.WriteInt(testI);
        bool isequal = true;
        for (int i = 0; i < length; i++)
        {
            if (actual[i] != expected[i])
            {
                isequal = false;
            }
        }
        if (!isequal)
        {
            errors[errorsCount++] = msg;
            Platform.WriteString(" error: ");
            Platform.WriteString(msg);
        }
        else
        {
            Platform.WriteString(" ok");
        }
        Platform.WriteString("\n");
        testI++;
    }

    public float[] Arr3(float p, float p_2, float p_3)
    {
        float[] arr = new float[3];
        arr[0] = p;
        arr[1] = p_2;
        arr[2] = p_3;
        return arr;
    }

    public float[] Arr9(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9)
    {
        float[] arr = new float[16];
        arr[0] = p;
        arr[1] = p_2;
        arr[2] = p_3;
        arr[3] = p_4;
        arr[4] = p_5;
        arr[5] = p_6;
        arr[6] = p_7;
        arr[7] = p_8;
        arr[8] = p_9;
        return arr;
    }

    public float[] Arr16(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9, int p_10, int p_11, int p_12, int p_13, int p_14, int p_15, int p_16)
    {
        float[] arr = new float[16];
        arr[0] = p;
        arr[1] = p_2;
        arr[2] = p_3;
        arr[3] = p_4;
        arr[4] = p_5;
        arr[5] = p_6;
        arr[6] = p_7;
        arr[7] = p_8;
        arr[8] = p_9;
        arr[9] = p_10;
        arr[10] = p_11;
        arr[11] = p_12;
        arr[12] = p_13;
        arr[13] = p_14;
        arr[14] = p_15;
        arr[15] = p_16;
        return arr;
    }
}
#endif
