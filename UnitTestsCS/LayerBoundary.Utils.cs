using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;

namespace UnitTestsCS
{
    [TestClass]
    public class LayerBoundaryUtils
    {
        private LayerBoundary[] GetTestInputData()
        {
            return new LayerBoundary[] {
                new LayerBoundary(0,2),
                new LayerBoundary(1,0),
                new LayerBoundary(2,1),
                new LayerBoundary(3,1),
                new LayerBoundary(4,0),
                new LayerBoundary(5,0),
                new LayerBoundary(6,2),
                new LayerBoundary(7,0),
                new LayerBoundary(8,2),
                new LayerBoundary(9,0),
                new LayerBoundary(10,2)
            };
        }

        static void AssertNumbers(LayerBoundary lb, int[] expectedNumbers)
        {
            Assert.AreEqual(lb.Numbers.Length, expectedNumbers.Length);
            for (int i = 0; i < expectedNumbers.Length; i++)
                Assert.AreEqual(expectedNumbers[i], lb.Numbers[i]);
        }

        [TestMethod]
        public void UpToBottomNumberingTest()
        {
            var data = GetTestInputData();
             Utils.RecalcBoundaryNumbers(data, CoreSampleAnnotation.AnnotationDirection.UpToBottom);

            AssertNumbers(data[0], new int[] { 1, 1, 1 });
            AssertNumbers(data[1], new int[] { 2 });
            AssertNumbers(data[2], new int[] { 1, 2 });
            AssertNumbers(data[3], new int[] { 1, 3 });
            AssertNumbers(data[4], new int[] { 2 });
            AssertNumbers(data[5], new int[] { 3 });
            AssertNumbers(data[6], new int[] { 1, 1, 2 });
            AssertNumbers(data[7], new int[] { 2 });
            AssertNumbers(data[8], new int[] { 1, 1, 3 });
            AssertNumbers(data[9], new int[] { 2 });
            AssertNumbers(data[10], new int[] { 1, 1, 4 });


        }
    }
}
