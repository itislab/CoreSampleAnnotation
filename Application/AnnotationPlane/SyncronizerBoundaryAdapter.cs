using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class SyncronizerBoundaryAdapter : LayerSyncronization.ILayersColumn, ColumnScale.IColumn
    {
        private double columnBottum;
        private LayerBoundaryEditorVM boundariesVM;

        public double ColumnHeight
        {
            set
            {
                columnBottum = value;
            }
        }

        public double UpperDepth
        {
            set
            { }
        }

        public double LowerDepth
        {
            set
            { }
        }

        public SyncronizerBoundaryAdapter(LayerBoundaryEditorVM lbeVM)
        {
            boundariesVM = lbeVM;
        }

        /// <summary>
        /// In WPF units
        /// </summary>
        /// <returns></returns>
        public double[] GetLayerHeights()
        {
            List<double> heights = new List<double>();

            LayerBoundary[] lbVMs = boundariesVM.Boundaries;

            if (lbVMs.Length > 0)
            {
                heights.Add(lbVMs[0].Level);
                for (int i = 0; i < lbVMs.Length - 1; i++)
                {
                    heights.Add(lbVMs[i + 1].Level - lbVMs[i].Level);
                }
            }

            heights.Add(columnBottum);

            return heights.ToArray();
        }

        public void InsertLayer(int targetIdx, int templateIdx)
        {
            List<LayerBoundary> res = new List<LayerBoundary>(boundariesVM.Boundaries);
            
            
            if (targetIdx >= boundariesVM.Boundaries.Length)
            {
                LayerBoundary newVM = new LayerBoundary(columnBottum,0);
                res.Add(newVM);
            }
            else {
                double up;
                if (targetIdx == 0)
                    up = 0.0;
                else
                    up = boundariesVM.Boundaries[targetIdx].Level; ;
                LayerBoundary newVM = new LayerBoundary(up, 0);                
                res.Insert(targetIdx, newVM);
            }
            boundariesVM.Boundaries = res.ToArray();
        }

        public void RemoveLayer(int tagetIdx)
        {
            List<LayerBoundary> res = new List<LayerBoundary>(boundariesVM.Boundaries);
            res.RemoveAt(tagetIdx);
            boundariesVM.Boundaries = res.ToArray();
        }


        /// <summary>
        /// in WPF units
        /// </summary>
        /// <param name="layerIdx"></param>
        /// <param name="height"></param>
        public void SetLayerHeight(int layerIdx, double height)
        {
            LayerBoundary[] lbVMs = boundariesVM.Boundaries;
            double up;
            if (layerIdx == 0)
                up = 0.0;
            else
                up = lbVMs[layerIdx - 1].Level;
            if (layerIdx == lbVMs.Length)
            {
                columnBottum = height + up;
            }
            else
            {
                LayerBoundary[] res = lbVMs.ToArray();
                res[layerIdx] = new LayerBoundary(height + up, res[layerIdx].Rank);
                boundariesVM.Boundaries = res;
            }
        }
    }
}
