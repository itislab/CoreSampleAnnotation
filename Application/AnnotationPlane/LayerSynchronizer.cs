using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerSyncronization
{
    public interface ILayersColumn
    {
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

    public enum FreeSpaceAccepter { UpperLayer, LowerLayer };

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
        public double UpperDepth
        {
            get { return upperDepth; }
            set
            {
                if (upperDepth != value)
                {
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
        public double LowerDepth
        {
            get { return lowerDepth; }
            set
            {
                if (lowerDepth != value)
                {
                    lowerDepth = value;
                    ResetAllcolumns();
                    RaisePropertyChanged(nameof(LowerDepth));
                }
            }
        }

        /// <summary>
        /// Points in (meters, positive value) where the layer boundary lay (including outer boundaries; boundaries count is one more than layers count)
        /// </summary>
        private double[] depthBoundaries = null;

        /// <summary>
        /// Points in (meters, positive value) where the layer boundary lay (including outer boundaries; boundaries count is one more than layers count)
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
        public double ScaleFactor
        {
            get
            {
                return scaleFactor;
            }
            set
            {
                if (value != scaleFactor)
                {
                    scaleFactor = value;
                    ResetAllcolumns();
                    RaisePropertyChanged(nameof(ScaleFactor));
                }
            }
        }

        private void ResetColumn(ILayersColumn column)
        {
            double[] wpfBoundaries = depthBoundaries.Select(b => (b - UpperDepth) * ScaleFactor).ToArray();
            for (int i = 0; i < wpfBoundaries.Length - 1; i++)
            {
                column.SetLayerHeight(i, wpfBoundaries[i + 1] - wpfBoundaries[i]);
            }
        }

        private void ResetAllcolumns()
        {
            foreach (var column in columns)
                ResetColumn(column);
        }
        /// <summary>
        /// Returns the index of the layer that contains the point with Y = wpf offset from the upper bound from the column
        /// </summary>
        /// <param name="wpfOffset"></param>
        /// <returns></returns>
        public int GetLayerIndex(double wpfOffset)
        {
            var wpfBoundaries = depthBoundaries.Select(b => (b - UpperDepth) * ScaleFactor).ToArray();
            int idx = Array.BinarySearch(wpfBoundaries, wpfOffset);
            if (idx < 0)
            {
                idx = ~idx;
                idx--;
            }
            return idx;
        }

        private void AssertLayerIdx(int idx)
        {
            if (idx < 0)
                throw new ArgumentException("layer index is negative");
            if (idx >= depthBoundaries.Length - 1)
                throw new ArgumentException("layer index is out of bounds");
        }

        public double GetLayerWPFHeight(int idx)
        {
            AssertLayerIdx(idx);
            return LengthToWPF(depthBoundaries[idx + 1] - depthBoundaries[idx]);
        }

        public double GetLayerWPFTop(int idx)
        {
            AssertLayerIdx(idx);
            return DepthToWPF(depthBoundaries[idx]);
        }

        public double GetLayerWPFBottom(int idx)
        {
            AssertLayerIdx(idx);
            return DepthToWPF(depthBoundaries[idx + 1]);
        }

        /// <summary>
        /// Removes the layer
        /// </summary>
        /// <param name="idx">Which layer to remove</param>
        /// <param name="freeSpaceAccepter">Where to attach the freed space</param>
        public void RemoveLayer(int idx, FreeSpaceAccepter freeSpaceAccepter)
        {
            AssertLayerIdx(idx);
            if (idx == 0 && freeSpaceAccepter == FreeSpaceAccepter.UpperLayer)
                throw new ArgumentException("There is no upper layer to attach free space");
            if (idx >= depthBoundaries.Length - 2 && freeSpaceAccepter == FreeSpaceAccepter.LowerLayer)
                throw new ArgumentException("There is no lower layer to attach free space");

            double freedSpace = GetLayerWPFHeight(idx);
            int spaceAcceptorIdx;
            switch (freeSpaceAccepter)
            {
                case FreeSpaceAccepter.UpperLayer:
                    spaceAcceptorIdx = idx - 1;
                    break;
                case FreeSpaceAccepter.LowerLayer:
                    spaceAcceptorIdx = idx + 1;
                    break;
                default: throw new NotImplementedException();
            }
            double spaceAcceptorOldHeight = GetLayerWPFHeight(spaceAcceptorIdx);
            double spaceAcceptorNewHeight = spaceAcceptorOldHeight + freedSpace;
            foreach (var column in columns)
            {
                column.SetLayerHeight(spaceAcceptorIdx, spaceAcceptorNewHeight);
                column.RemoveLayer(idx);
            }

            int boundaryIdxToRemove;
            switch (freeSpaceAccepter)
            {
                case FreeSpaceAccepter.UpperLayer:
                    boundaryIdxToRemove = idx; break;
                case FreeSpaceAccepter.LowerLayer:
                    boundaryIdxToRemove = idx + 1; break;
                default: throw new NotImplementedException();
            }
            depthBoundaries = depthBoundaries.Take(boundaryIdxToRemove).Concat(depthBoundaries.Skip(boundaryIdxToRemove + 1)).ToArray();
        }

        /// <summary>
        /// Sets the layer idx Height to newHight, compensting the change by the hight of the lext layer (so total column height is unchanged)
        /// </summary>
        /// <param name="idx">the layer to set the new height</param>
        /// <param name="newHeight">the new hight to set (in WPF units)</param>
        public void MoveBoundary(int idx, double newHeight)
        {
            if (idx >= depthBoundaries.Length - 2)
                throw new InvalidOperationException("can not move boundary of the lat layer");
            if (newHeight <= 0.0)
                throw new ArgumentException("height can't be negative");
            double[] wpfDepthBoundaries = depthBoundaries.Select(b => DepthToWPF(b)).ToArray();
            double oldHeight = wpfDepthBoundaries[idx + 1] - wpfDepthBoundaries[idx];
            double nextLayerHeigth = wpfDepthBoundaries[idx + 2] - wpfDepthBoundaries[idx + 1];
            double nextLayerAddition = oldHeight - newHeight;
            if (nextLayerAddition < 0.0)
            {
                //the layer grows. Checking that it does not overgrow the next layer                
                if (newHeight - oldHeight >= nextLayerHeigth)
                    throw new InvalidOperationException("the layer overgrowth the next layer");
            }
            foreach (var column in columns)
            {
                column.SetLayerHeight(idx, newHeight);
                column.SetLayerHeight(idx + 1, nextLayerHeigth + nextLayerAddition);
            }
            depthBoundaries[idx + 1] = depthBoundaries[idx + 1] - WpfToLength(nextLayerAddition);
        }

        public const double MinLayerLength = 1e-4;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="depths"></param>
        /// <param name="idx"></param>
        /// <param name="newLength"></param>
        /// <param name="direction"></param>
        /// <returns>Layer indices to remove</returns>
        public static double[] LayerLengthEditWithShift(double[] depths, int idx, double newLength, AnnotationDirection direction)
        {
            double[] results;
            if (newLength < 0)
                throw new ArgumentException("newLength must be non-negative");
            if (newLength == 0.0)
                newLength = MinLayerLength;
            switch (direction)
            {
                case AnnotationDirection.UpToBottom:
                    double oldLength = depths[idx + 1] - depths[idx];

                    int initialBoundaryCount = depths.Length;
                    double lowerDepth = depths[initialBoundaryCount - 1];

                    double addition = newLength - oldLength;

                    int toRemove = 0;
                    double removedLayersLength = 0.0;
                    while ((depths.Length - 2 - toRemove > 0) && (depths[depths.Length - 1 - toRemove] - depths[depths.Length - 2 - toRemove] + removedLayersLength <= addition))
                    {                        
                        removedLayersLength += depths[depths.Length - 1 - toRemove] - depths[depths.Length - 2 - toRemove];
                        toRemove++;
                    }
                    System.Diagnostics.Trace.WriteLine("{0} layers to remove");

                    results = depths.Take(initialBoundaryCount - toRemove).ToArray();
                    for (int i = idx + 1; i < initialBoundaryCount - toRemove - 1; i++)
                        results[i] += addition;
                    results[initialBoundaryCount - toRemove - 1] = lowerDepth;
                    break;
                default:
                    throw new NotSupportedException("unexpected annotatiuon direction");
            }

            return results;
        }
        /// <summary>
        /// Resizing the <paramref name="idx"/> layer (0 is top most layer) to have new length of <paramref name="newLength"/>
        /// </summary>
        /// <param name="idx">zero-based. 0 is on top, regardless annotation direction</param>
        /// <param name="newLength">in meteres</param>
        /// <param name="fromButtomToUp"></param>
        public void SetLayerLength(int idx, double newLength, AnnotationDirection direction)
        {

        }

        public void UnregisterColumn(ILayersColumn column)
        {
            columns.Remove(column);
        }

        public void RegisterColumn(ILayersColumn column)
        {
            double[] layerHeights = column.GetLayerHeights();
            if (depthBoundaries == null)
            {
                double[] layerHeightsMeters = layerHeights.Select(h => h / ScaleFactor).ToArray();
                depthBoundaries = new double[layerHeightsMeters.Length + 1];
                depthBoundaries[0] = UpperDepth;
                for (var i = 0; i < layerHeightsMeters.Length; i++)
                {
                    depthBoundaries[i + 1] = depthBoundaries[i] + layerHeightsMeters[i];
                }

            }
            else
            {
                if (layerHeights.Length + 1 != depthBoundaries.Length)
                    throw new InvalidOperationException(string.Format("Layer syncronizer controls {0} layers, but the layer being regestered contains {1} layers", depthBoundaries.Length - 1, layerHeights.Length));
                ResetColumn(column);
            }
            columns.Add(column);

        }

        /// <summary>
        /// Converts real depth in meters to WPF units on the annotation plane
        /// </summary>
        /// <param name="depth">in meters (positive value)</param>
        /// <returns></returns>
        public double DepthToWPF(double depth)
        {
            return (depth - upperDepth) * scaleFactor;
        }

        /// <summary>
        /// Converts the length in WPF coords on the annotation plane to the real length in meters
        /// </summary>
        /// <param name="depth">in WPF units</param>
        /// <returns></returns>
        public double WpfToLength(double length)
        {
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
        public void SplitLayer(double wpfOffset)
        {
            double splitDepth = wpfOffset / ScaleFactor + UpperDepth;
            System.Diagnostics.Debug.WriteLine("new layer split at {0} meters", splitDepth);
            int idx = Array.BinarySearch(this.depthBoundaries, splitDepth);
            if (idx < 0)
            {
                idx = ~idx;
                idx--;
                System.Diagnostics.Debug.WriteLine("Splitting {0}th layer", idx);

                double layerTopDepth = depthBoundaries[idx];
                double layerBottomDepth = depthBoundaries[idx + 1];

                double layerTopWpfOffset = (layerTopDepth - UpperDepth) * ScaleFactor;
                double layerBottomWpfOffset = (layerBottomDepth - UpperDepth) * ScaleFactor;

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

                var beforNew = depthBoundaries.Take(idx + 1).ToArray();
                var afterNew = depthBoundaries.Skip(idx + 1).ToArray();
                var newBoundaries = new List<double>();
                newBoundaries.AddRange(beforNew);
                newBoundaries.Add(splitDepth);
                newBoundaries.AddRange(afterNew);
                depthBoundaries = newBoundaries.ToArray();
            }
        }
    }
}
