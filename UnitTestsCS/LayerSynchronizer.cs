using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoreSampleAnnotation.AnnotationPlane.LayerSyncronization;

namespace UnitTestsCS
{
    [TestClass]
    public class LayerSynchronizerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LayerLengthEditWithShift_negative_len()
        {
            double[] depths = new double[] { 0, 1, 2, 3 };
            Controller.LayerLengthEditWithShift(depths, 0, -2.0, CoreSampleAnnotation.AnnotationDirection.BottomToUp);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void LayerLengthEditWithShift_idx_OOR_1()
        {
            double[] depths = new double[] { 0, 1, 2, 3 };
            Controller.LayerLengthEditWithShift(depths, -2, 1.0, CoreSampleAnnotation.AnnotationDirection.BottomToUp);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void LayerLengthEditWithShift_idx_OOR_2()
        {
            double[] depths = new double[] { 0, 1, 2, 3 };
            Controller.LayerLengthEditWithShift(depths, 3, 1.0, CoreSampleAnnotation.AnnotationDirection.BottomToUp);
        }

        /// <summary>
        /// The last layer length change is not effective. the layer is retained as is
        /// </summary>
        [TestMethod]
        public void LayerLengthEditWithShift_0_lastLayer()
        {            
            double[] depths = new double[] { 0, 1, 2, 3 };
            var results = Controller.LayerLengthEditWithShift(depths, 2, 0, CoreSampleAnnotation.AnnotationDirection.UpToBottom);
            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.0, results[1]);
            Assert.AreEqual(2.0, results[2]);
            Assert.AreEqual(3.0, results[3]);

            results = Controller.LayerLengthEditWithShift(depths, 2, 4.0, CoreSampleAnnotation.AnnotationDirection.UpToBottom);
            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.0, results[1]);
            Assert.AreEqual(2.0, results[2]);
            Assert.AreEqual(3.0, results[3]);

            results = Controller.LayerLengthEditWithShift(depths, 0, 0, CoreSampleAnnotation.AnnotationDirection.BottomToUp);
            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.0, results[1]);
            Assert.AreEqual(2.0, results[2]);
            Assert.AreEqual(3.0, results[3]);
            


        }

        [TestMethod]
        public void LayerLengthEditWithShift_0_cercion_upbottom()
        {
            double[] depths = new double[] { 0, 1, 2, 3 };
            var results = Controller.LayerLengthEditWithShift(depths, 0, 0, CoreSampleAnnotation.AnnotationDirection.UpToBottom);
            Assert.AreEqual(4, results.Length); //boundaries len
            
            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(Controller.MinLayerLength, results[1],1e-5);
            Assert.AreEqual(1.0 + Controller.MinLayerLength, results[2], 1e-5);
            Assert.AreEqual(3.0, results[3]);


            results = Controller.LayerLengthEditWithShift(depths, 1, 0, CoreSampleAnnotation.AnnotationDirection.UpToBottom);
            
            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.0, results[1]);
            Assert.AreEqual(1.0+ Controller.MinLayerLength, results[2], 1e-5);
            Assert.AreEqual(3.0, results[3]);            
        }

        [TestMethod]
        public void LayerLengthEditWithShift_0_cercion_bottomup()
        {
            double[] depths = new double[] { 0, 1, 2, 3, 4 };
            double[] results = Controller.LayerLengthEditWithShift(depths, 2, 0, CoreSampleAnnotation.AnnotationDirection.BottomToUp);
            Assert.AreEqual(5, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(2.0 - Controller.MinLayerLength, results[1], 1e-5);
            Assert.AreEqual(3.0 - Controller.MinLayerLength, results[2], 1e-5);
            Assert.AreEqual(3.0, results[3]);
            Assert.AreEqual(4.0, results[4]);


            results = Controller.LayerLengthEditWithShift(depths, 3, 0, CoreSampleAnnotation.AnnotationDirection.BottomToUp);

            Assert.AreEqual(5, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(2.0 - Controller.MinLayerLength, results[1], 1e-5);
            Assert.AreEqual(3.0 - Controller.MinLayerLength, results[2], 1e-5);
            Assert.AreEqual(4.0 - Controller.MinLayerLength, results[3], 1e-5);
            Assert.AreEqual(4.0, results[4]);
        }

        [TestMethod]
        public void LayerLengthEditWithShift_delete_through_expand_ub()
        {
            double[] depths = new double[] { 0, 1, 2, 3 };
            var results = Controller.LayerLengthEditWithShift(depths, 0, 4, CoreSampleAnnotation.AnnotationDirection.UpToBottom);
            Assert.AreEqual(2, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(3.0, results[1]);
            
            results = Controller.LayerLengthEditWithShift(depths, 1, 2.2, CoreSampleAnnotation.AnnotationDirection.UpToBottom);

            Assert.AreEqual(3, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.0, results[1]);            
            Assert.AreEqual(3.0, results[2]);

            results = Controller.LayerLengthEditWithShift(depths, 1, 10.0, CoreSampleAnnotation.AnnotationDirection.UpToBottom);

            Assert.AreEqual(3, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.0, results[1]);
            Assert.AreEqual(3.0, results[2]);
        }

        [TestMethod]
        public void LayerLengthEditWithShift_delete_through_expand_bu()
        {
            double[] depths = new double[] { 0, 1, 2, 3, 4 };
            var results = Controller.LayerLengthEditWithShift(depths, 3, 5, CoreSampleAnnotation.AnnotationDirection.BottomToUp);
            Assert.AreEqual(2, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(4.0, results[1]);

            results = Controller.LayerLengthEditWithShift(depths, 2, 2.2, CoreSampleAnnotation.AnnotationDirection.BottomToUp);

            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(0.8, results[1],1e-5);
            Assert.AreEqual(3.0, results[2]);
            Assert.AreEqual(4.0, results[3]);

            results = Controller.LayerLengthEditWithShift(depths, 2, 10.0, CoreSampleAnnotation.AnnotationDirection.BottomToUp);

            Assert.AreEqual(3, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(3.0, results[1], 1e-5);
            Assert.AreEqual(4.0, results[2]);            
        }

        [TestMethod]
        public void LayerLengthEditWithShift_regular_ub()
        {
            double[] depths = new double[] { 0, 1, 2, 3 };
            var results = Controller.LayerLengthEditWithShift(depths, 0, 1.2, CoreSampleAnnotation.AnnotationDirection.UpToBottom);
            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.2, results[1]);
            Assert.AreEqual(2.2, results[2]);
            Assert.AreEqual(3.0, results[3]);

            results = Controller.LayerLengthEditWithShift(depths, 0, 0.8, CoreSampleAnnotation.AnnotationDirection.UpToBottom);

            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(0.8, results[1]);
            Assert.AreEqual(1.8, results[2]);
            Assert.AreEqual(3.0, results[3]);

            results = Controller.LayerLengthEditWithShift(depths, 1, 1.2, CoreSampleAnnotation.AnnotationDirection.UpToBottom);
            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.0, results[1]);
            Assert.AreEqual(2.2, results[2]);
            Assert.AreEqual(3.0, results[3]);

            results = Controller.LayerLengthEditWithShift(depths, 1, 0.8, CoreSampleAnnotation.AnnotationDirection.UpToBottom);

            Assert.AreEqual(4, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.0, results[1]);
            Assert.AreEqual(1.8, results[2]);
            Assert.AreEqual(3.0, results[3]);
        }

        [TestMethod]
        public void LayerLengthEditWithShift_regular_bu()
        {
            double[] depths = new double[] { 0, 1, 2, 3, 4 };
            var results = Controller.LayerLengthEditWithShift(depths, 3, 1.2, CoreSampleAnnotation.AnnotationDirection.BottomToUp);
            Assert.AreEqual(5, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(0.8, results[1]);
            Assert.AreEqual(1.8, results[2]);
            Assert.AreEqual(2.8, results[3]);
            Assert.AreEqual(4.0, results[4]);

            results = Controller.LayerLengthEditWithShift(depths, 3, 0.8, CoreSampleAnnotation.AnnotationDirection.BottomToUp);

            Assert.AreEqual(5, results.Length); //boundaries len

            Assert.AreEqual(0.0, results[0]);
            Assert.AreEqual(1.2, results[1]);
            Assert.AreEqual(2.2, results[2]);
            Assert.AreEqual(3.2, results[3]);
            Assert.AreEqual(4.0, results[4]);
        }
    }
}
