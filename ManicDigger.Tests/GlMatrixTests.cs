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

using NUnit.Framework;

namespace ManicDigger.Tests.GlMatrix
{
	[TestFixture]
	public class TestVec3
	{
		float[] vecA;
		float[] vecB;
		float[] output;

		[SetUp]
		public void ResetTests()
		{
			vecA = new float[] { 1, 2, 3 };
			vecB = new float[] { 4, 5, 6 };
			output = new float[] { 0, 0, 0 };
		}

		[Test]
		public void TransformMat4WithAnIdentity()
		{
			float[] matr = Arr16(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
			float[] result = Vec3.TransformMat4(output, vecA, matr);
			Assert.AreEqual(output, vecA); // TransformMat4WithAnIdentity should produce the input
			Assert.AreEqual(result, vecA); // TransformMat4WithAnIdentity should return output
		}

		[Test]
		public void TransformMat4WithALookAt()
		{
			float[] matr = Mat4.LookAt(Mat4.Create(), Arr3(5, 6, 7), Arr3(2, 6, 7), Arr3(0, 1, 0));
			float[] result = Vec3.TransformMat4(output, vecA, matr);
			Assert.AreEqual(output, Arr3(4, -4, -4)); // TransformMat4WithALookAt should rotate and translate the input
			Assert.AreEqual(result, output); // TransformMat4WithALookAt should return out
		}

		[Test]
		public void TransformMat3WithAnIdentity()
		{
			float[] matr = Arr9(1, 0, 0, 0, 1, 0, 0, 0, 1);
			float[] result = Vec3.TransformMat3(output, vecA, matr);
			Assert.AreEqual(output, Arr3(1, 2, 3), "TransformMat3WithAnIdentity should produce the input");
			Assert.AreEqual(result, output, "TransformMat3WithAnIdentity should return output");
		}

		[Test]
		public void TransformMat3With90DegAboutX()
		{
			float[] result = Vec3.TransformMat3(output, Arr3(0, 1, 0), Arr9(1, 0, 0, 0, 0, 1, 0, -1, 0));
			Assert.AreEqual(output, Arr3(0, 0, 1), "TransformMat3With90DegAboutX should produce correct output");
		}

		[Test]
		public void TransformMat3With90DegAboutY()
		{
			float[] result = Vec3.TransformMat3(output, Arr3(1, 0, 0), Arr9(0, 0, -1, 0, 1, 0, 1, 0, 0));
			Assert.AreEqual(output, Arr3(0, 0, -1), "TransformMat3With90DegAboutU should produce correct output");
		}

		[Test]
		public void TransformMat3With90DegAboutZ()
		{
			float[] result = Vec3.TransformMat3(output, Arr3(1, 0, 0), Arr9(0, 1, 0, -1, 0, 0, 0, 0, 1));
			Assert.AreEqual(output, Arr3(0, 1, 0), "TransformMat3With90DegAboutZ should produce correct output");
		}

		[Test]
		public void TransformMat3WithALookAtNormalMatrix()
		{
			float[] matr = Mat4.LookAt(Mat4.Create(), Arr3(5, 6, 7), Arr3(2, 6, 7), Arr3(0, 1, 0));
			float[] n = Mat3.Create();
			matr = Mat3.Transpose(n, Mat3.Invert(n, Mat3.FromMat4(n, matr)));
			float[] result = Vec3.TransformMat3(output, Arr3(1, 0, 0), matr);

			Assert.AreEqual(output, Arr3(0, 0, 1), "TransformMat3WithALookAtNormalMatrix should rotate the input");
			Assert.AreEqual(result, output, "TransformMat3WithALookAtNormalMatrix should return output");
		}

		[Test]
		public void Create()
		{
			float[] result = Vec3.Create();
			Assert.AreEqual(result, Arr3(0, 0, 0), "Create should return a 3 element array initialized to 0s");
		}

		[Test]
		public void CloneIt()
		{
			float[] result = Vec3.CloneIt(vecA);
			Assert.AreEqual(result, vecA, "Clone should return a 3 element array initialized to the values in vecA");
		}

		[Test]
		public void FromValues()
		{
			float[] result = Vec3.FromValues(1, 2, 3);
			Assert.AreEqual(result, Arr3(1, 2, 3), "FromValues should return a 3 element array initialized to the values passed");
		}

		[Test]
		public void Copy()
		{
			float[] result = Vec3.Copy(output, vecA);
			Assert.AreEqual(output, Arr3(1, 2, 3), "Copy should place values into out");
			Assert.AreEqual(result, output, "Copy should return output");
		}

		[Test]
		public void Set()
		{
			float[] result = Vec3.Set(output, 1, 2, 3);
			Assert.AreEqual(output, Arr3(1, 2, 3), "Set should place values into output");
			Assert.AreEqual(result, output, "Set should return output");
		}

		[Test]
		public void AddWithASeparateOutputVector()
		{
			float[] result = Vec3.Add(output, vecA, vecB);
			Assert.AreEqual(output, Arr3(5, 7, 9), "Add should place values into out");
			Assert.AreEqual(result, output, "Add should return out");
			Assert.AreEqual(vecA, Arr3(1, 2, 3), "Add should not modify vecA");
			Assert.AreEqual(vecB, Arr3(4, 5, 6), "Add should not modify vecB");
		}

		[Test]
		public void Distance()
		{
			float result = Vec3.Distance(vecA, vecB);
			float r = 5196152;
			r /= 1000 * 1000; // 5.196152
			Assert.AreEqual(result, r, 0.000001, "Distance should return the distance");
		}

		[Test]
		public void SquaredDistance()
		{
			float result = Vec3.SquaredDistance(vecA, vecB);
			Assert.AreEqual(result, 27, "SquaredDistance should return the squared distance");
		}

		[Test]
		public void Length_()
		{
			float result = Vec3.Length_(vecA);
			float r = 3741657;
			r /= 1000 * 1000;// 3.741657
			Assert.AreEqual(result, r, 0.000001, "Length should return the length");
		}

		[Test]
		public void SquaredLength()
		{
			float result = Vec3.SquaredLength(vecA);
			Assert.AreEqual(result, 14, "SquaredLength should return the squared length");
		}

		[Test]
		public void NegateWithASeparateOutputVector()
		{
			float[] result = Vec3.Negate(output, vecA);
			Assert.AreEqual(output, Arr3(-1, -2, -3), "NegateWithASeparateOutputVector should place values into out");
			Assert.AreEqual(result, output, "NegateWithASeparateOutputVector should should return out");
			Assert.AreEqual(vecA, Arr3(1, 2, 3), "NegateWithASeparateOutputVector should not modify vecA");
		}

		[Test]
		public void NegateWhenVecAIsTheOutputVector()
		{
			float[] result = Vec3.Negate(vecA, vecA);
			Assert.AreEqual(vecA, Arr3(-1, -2, -3), "NegateWhenVecAIsTheOutputVector should place values into vecA");
			Assert.AreEqual(result, vecA, "NegateWhenVecAIsTheOutputVector should return vecA");
		}

		[Test]
		public void NormalizeWithASeparateOutputVector()
		{
			vecA = Arr3(5, 0, 0);
			float[] result = Vec3.Normalize(output, vecA);
			Assert.AreEqual(output, Arr3(1, 0, 0), "NormalizeWithASeparateOutputVector should place values into out");
			Assert.AreEqual(result, output, "NormalizeWithASeparateOutputVector should return out");
			Assert.AreEqual(vecA, Arr3(5, 0, 0), "NormalizeWithASeparateOutputVector should not modify vecA");
		}

		[Test]
		public void NormalizeWhenVecAIsTheOutputVector()
		{
			float[] vecA = Arr3(5, 0, 0);
			float[] result = Vec3.Normalize(vecA, vecA);
			Assert.AreEqual(vecA, Arr3(1, 0, 0), "NormalizeWhenVecAIsTheOutputVector should place values into vecA");
			Assert.AreEqual(result, vecA, "NormalizeWhenVecAIsTheOutputVector should return vecA");
		}

		[Test]
		public void Dot()
		{
			float result = Vec3.Dot(vecA, vecB);
			Assert.AreEqual(result, 32, "Dot should return the dot product");
			Assert.AreEqual(vecA, Arr3(1, 2, 3), "Dot should not modify vecA");
			Assert.AreEqual(vecB, Arr3(4, 5, 6), "Dot should not modify vecB");
		}

		float[] Arr3(float p, float p_2, float p_3)
		{
			return new float[] { p, p_2, p_3 };
		}
		float[] Arr9(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9)
		{
			return new float[] { p, p_2, p_3, p_4, p_5, p_6, p_7, p_8, p_9 };
		}
		float[] Arr16(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9, int p_10, int p_11, int p_12, int p_13, int p_14, int p_15, int p_16)
		{
			return new float[] { p, p_2, p_3, p_4, p_5, p_6, p_7, p_8, p_9, p_10, p_11, p_12, p_13, p_14, p_15, p_16 };
		}
	}

	[TestFixture]
	public class TestMat4
	{
		float[] matA;
		float[] matB;
		float[] output;
		float[] identity;

		[SetUp]
		public void ResetTests()
		{
			// Attempting to portray a semi-realistic transform matrix
			matA = Arr16(1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					1, 2, 1, 3);
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

		[Test]
		public void Create()
		{
			float[] result = Mat4.Create();
			Assert.AreEqual(result, identity, "Create should return a 16 element array initialized to a 4x4 identity matrix");
		}

		[Test]
		public void CloneIt()
		{
			float[] result = Mat4.CloneIt(matA);
			Assert.AreEqual(result, matA, "Clone should return a 16 element array initialized to the values in matA");
		}

		[Test]
		public void Copy()
		{
			float[] result = Mat4.Copy(output, matA);
			Assert.AreEqual(output, matA, "Copy should place values into out");
			Assert.AreEqual(result, output, "Copy should return out");
		}

		[Test]
		public void Identity_()
		{
			float[] result = Mat4.Identity_(output);
			Assert.AreEqual(output, identity, "Copy should place values into out");
			Assert.AreEqual(result, output, "Copy should return out");
		}

		[Test]
		public void Frustum()
		{
			float[] result = Mat4.Frustum(output, -1, 1, -1, 1, -1, 1);
			Assert.AreEqual(result, Arr16(-1, 0, 0, 0,
					0, -1, 0, 0,
					0, 0, 0, -1,
					0, 0, 1, 0), "Frustum should place values into out");
			Assert.AreEqual(result, output, "Frustum should return out");
		}

		[Test]
		public void Ortho()
		{
			float[] result = Mat4.Ortho(output, -1, 1, -1, 1, -1, 1);
			Assert.AreEqual(result, Arr16(1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, -1, 0,
					0, 0, 0, 1), "Ortho should place values into out");
			Assert.AreEqual(result, output, "Ortho should return out");
		}

		[Test]
		public void LookAtLookingDown()
		{
			float[] view = Arr3(0, -1, 0);
			float[] up = Arr3(0, 0, -1);
			float[] right = Arr3(1, 0, 0);

			float[] result = Mat4.LookAt(output, Arr3(0, 0, 0), view, up);

			result = Vec3.TransformMat4(Vec3.Create(), view, output);
			Assert.AreEqual(result, Arr3(0, 0, -1), "LookAtLookingDown should transform view into local -Z");

			result = Vec3.TransformMat4(Vec3.Create(), up, output);
			Assert.AreEqual(result, Arr3(0, 1, 0), "LookAtLookingDownshould transform up into local +Y");

			result = Vec3.TransformMat4(Vec3.Create(), right, output);
			Assert.AreEqual(result, Arr3(1, 0, 0), "LookAtLookingDownshould transform right into local +X");

			float[] truncated = new float[3];
			System.Array.Copy(output, truncated, 3);
			Assert.AreEqual(result, truncated, "LookAtLookingDown should return out");
		}

		[Test]
		public void LookAt74()
		{
			float six = 6;
			Mat4.LookAt(output, Arr3(0, 2, 0), Arr3(0, six / 10, 0), Arr3(0, 0, -1));

			float[] result = Vec3.TransformMat4(Vec3.Create(), Arr3(0, 2, -1), output);
			Assert.AreEqual(result, Arr3(0, 1, 0), "LookAt74 should transform a point 'above' into local +Y");

			result = Vec3.TransformMat4(Vec3.Create(), Arr3(1, 2, 0), output);
			Assert.AreEqual(result, Arr3(1, 0, 0), "LookAt74 should transform a point 'right of' into local +X");

			result = Vec3.TransformMat4(Vec3.Create(), Arr3(0, 1, 0), output);
			Assert.AreEqual(result, Arr3(0, 0, -1), "LookAt74 should transform a point 'in front of' into local -Z");
		}

		float[] Arr3(float p, float p_2, float p_3)
		{
			return new float[] { p, p_2, p_3 };
		}
		float[] Arr9(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9)
		{
			return new float[] { p, p_2, p_3, p_4, p_5, p_6, p_7, p_8, p_9 };
		}
		float[] Arr16(int p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, int p_8, int p_9, int p_10, int p_11, int p_12, int p_13, int p_14, int p_15, int p_16)
		{
			return new float[] { p, p_2, p_3, p_4, p_5, p_6, p_7, p_8, p_9, p_10, p_11, p_12, p_13, p_14, p_15, p_16 };
		}
	}
}
