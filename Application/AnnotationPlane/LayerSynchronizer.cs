using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerSyncronization
{
    public interface ILayersColumn {
        /// <summary>
        /// Sets the height of he layer in WPF units
        /// </summary>
        /// <param name="layerIdx">index of the layer in a layer column to change the height to</param>
        /// <param name="height">the height value to set</param>
        void SetLayerHeight(int layerIdx, double height);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetIdx">The index at which the new added layer will be available </param>
        /// <param name="templateIdx">The index (befor addition of the new layer) of layer o copy the data from</param>
        void InsertLayer(int targetIdx, int templateIdx);
        
        void RemoveLayer(int tagetIdx);

        /// <summary>
        /// Returns the array of layer heights (in WPF units). The length of the array is the number of layers
        /// </summary>
        double[] GetLayerHeights();
    }

    /// <summary>
    /// Performs syncronious operations on a group of layered columns
    /// </summary>
    public class Controller : ViewModel
    {
        private List<ILayersColumn> columns = new List<ILayersColumn>();
        
        private double upperDepth = 0.0;
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double UpperDepth {
            get { return upperDepth; }
            set
            {
                if (upperDepth != value) {
                    upperDepth = value;
                    ResetAllcolumns();
                    RaisePropertyChanged(nameof(UpperDepth));
                }
            }
        }
        
        private double lowerDepth = 0.0;
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double LowerDepth {
            get { return lowerDepth; }
            set {
                if (lowerDepth != value) {
                    lowerDepth = value;
                    ResetAllcolumns();
                    RaisePropertyChanged(nameof(LowerDepth));
                }
            }
        }

        /// <summary>
        /// Points in (meters, positive value) where the layer boundary lay
        /// </summary>
        private double[] depthBoundaries = null;

        /// <summary>
        /// Points in (meters, positive value) where the layer boundary lay
        /// </summary>
        public double[] DepthBoundaries
        {
            get
            {
                return depthBoundaries;
            }
        }

        private double scaleFactor = 1.0;
        /// <summary>
        /// How many WPF units take 1 real meter of depth
        /// </summary>
        public double ScaleFactor {
            get {
                return scaleFactor;
            }
            set {
                if (value != scaleFactor) {
                    scaleFactor = value;
                    ResetAllcolumns();
                    RaisePropertyChanged(nameof(ScaleFactor));
                }
            }
        }

        private void ResetColumn(ILayersColumn column) {
            double[] wpfBoundaries = depthBoundaries.Select(b => (b - UpperDepth) * ScaleFactor).ToArray();            
            for (int i = 0; i < wpfBoundaries.Length-1; i++) {                
                column.SetLayerHeight(i, wpfBoundaries[i + 1] - wpfBoundaries[i]);
                }
        }

        private void ResetAllcolumns() {
            foreach (var column in columns)
                ResetColumn(column);
        }
        /// <summary>
        /// Returns the index of the layer that contains the point with Y = wpf offset from the upper bound from the column
        /// </summary>
        /// <param name="wpfOffset"></param>
        /// <returns></returns>
        public int GetLayerIndex(double wpfOffset) {
            var wpfBoundaries = depthBoundaries.Select(b => (b - UpperDepth) * ScaleFactor).ToArray();
            int idx = Array.BinarySearch(wpfBoundaries, wpfOffset);
            if (idx < 0)
            {
                idx = ~idx;
                idx--;
            }
            return idx;
        }

        public void UnregisterLayer(ILayersColumn column) {
            columns.Remove(column);
        }

        public void RegisterLayer(ILayersColumn column) {
            double[] layerHeights = column.GetLayerHeights();
            if (depthBoundaries == null)
            {
                double[] layerHeightsMeters = layerHeights.Select(h => h / ScaleFactor).ToArray();
                depthBoundaries = new double[layerHeightsMeters.Length + 1];
                depthBoundaries[0] = UpperDepth;
                for (var i = 0; i < layerHeightsMeters.Length; i++) {
                    depthBoundaries[i + 1] = depthBoundaries[i] + layerHeightsMeters[i];
                }

            }
            else {
                if (layerHeights.Length + 1 != depthBoundaries.Length)
                    throw new InvalidOperationException(string.Format("Layer syncronizer controls {0} layers, but the layer being regestered contains {1} layers", depthBoundaries.Length-1, layerHeights.Length));
                ResetColumn(column);
            }
            columns.Add(column);

        }

        /// <summary>
        /// Converts real depth in meters to WPF units on the annotation plane
        /// </summary>
        /// <param name="depth">in meters (positive value)</param>
        /// <returns></returns>
        public double DepthToWPF(double depth) {
            return (depth - upperDepth) * scaleFactor;
        }

        /// <summary>
        /// Converts the length in WPF coords on the annotation plane to the real length in meters
        /// </summary>
        /// <param name="depth">in WPF units</param>
        /// <returns></returns>
        public double WpfToLength(double length) {
            return (length / scaleFactor);
        }

        /// <summary>
        /// Converts real length in meters to length WPF units on the annotation plane
        /// </summary>
        /// <param name="depth">in meters (positive value)</param>
        /// <returns></returns>
        public double LengthToWPF(double length)
        {
            return (length) * scaleFactor;
        }

        /// <summary>
        /// Converts WPF vertical coord on the annotation plane to the real depth in meters
        /// </summary>
        /// <param name="depth">in WPF units</param>
        /// <returns></returns>
        public double WpfToDepth(double depth)
        {
            return (depth / scaleFactor) + upperDepth;
        }
        
        /// <summary>
        /// Split the layer which contains wpfOffset into 2 separate layers (the content in both layers are copied, it is identical)
        /// </summary>
        /// <param name="wpfOffset"></param>
        public void SplitLayer(double wpfOffset) {
            double splitDepth = wpfOffset / ScaleFactor + UpperDepth;
            System.Diagnostics.Debug.WriteLine("new layer split at {0} meters",splitDepth);
            int idx = Array.BinarySearch(this.depthBoundaries, splitDepth);
            if (idx < 0) {
                idx = ~idx;
                idx--;
                System.Diagnostics.Debug.WriteLine("Splitting {0}th layer", idx);

                double layerTopDepth = depthBoundaries[idx];
                double layerBottomDepth = depthBoundaries[idx+1];

                double layerTopWpfOffset = (layerTopDepth-UpperDepth)*ScaleFactor;                
                double layerBottomWpfOffset = (layerBottomDepth-UpperDepth)*ScaleFactor;

                System.Diagnostics.Debug.Assert(layerTopWpfOffset < wpfOffset);
                System.Diagnostics.Debug.Assert(wpfOffset < layerBottomWpfOffset);

                double newTopLayerHeight = wpfOffset - layerTopWpfOffset;
                double newLowerLayerHeight = layerBottomWpfOffset - wpfOffset;

                foreach (var column in columns)
                {
                    column.SetLayerHeight(idx, newTopLayerHeight);
                    column.InsertLayer(idx + 1, idx);
                    column.SetLayerHeight(idx + 1, newLowerLayerHeight);
                }

                var beforNew = depthBoundaries.Take(idx+1).ToArray();
                var afterNew = depthBoundaries.Skip(idx+1).ToArray();
                var newBoundaries = new List<double>();
                newBoundaries.AddRange(beforNew);
                newBoundaries.Add(splitDepth);
                newBoundaries.AddRange(afterNew);
                depthBoundaries = newBoundaries.ToArray();
            }
        }
    }
}
