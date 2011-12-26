using System.Collections.Generic;
using NUnit.Framework;

namespace ManicDigger.Tests
{

   [TestFixture]
   public class TestMyLinq
   {
      [Test]
      public void Skip()
      {
         var a = new int[] { 1, 1, 2, 3, 5, 8, 13 };
         var a1 = new List<int>( MyLinq.Skip(a, 2)).ToArray();
         Assert.AreEqual(new int[] { 2, 3, 5, 8, 13 }, a1);
         var a2 = new List<int>(MyLinq.Skip(a1, 1)).ToArray();
         Assert.AreEqual(new int[] {  3, 5, 8, 13 }, a2);
         Assert.AreEqual(new int[] { 1, 1, 2, 3, 5, 8, 13 }, new List<int>(MyLinq.Skip(a, 0)).ToArray());
         Assert.AreEqual(new int[0], new List<int>(MyLinq.Skip(a, 100)).ToArray());
      }

      [Test]
      public void Take()
      {
         var a = new int[] { 1, 1, 2, 3, 5, 8, 13 };
         var a1 = new List<int>(MyLinq.Take(a, 2)).ToArray();
         Assert.AreEqual(new int[] { 1, 1 }, a1);
         var a2 = new List<int>(MyLinq.Take(a1, 1)).ToArray();
         Assert.AreEqual(new int[] { 1 }, a2);
         Assert.AreEqual(new int[0], new List<int>(MyLinq.Take(a, 0)).ToArray());
         Assert.AreEqual(new int[] { 1, 1, 2, 3, 5, 8, 13 }, new List<int>(MyLinq.Take(a, 100)).ToArray());
      }
   }
}