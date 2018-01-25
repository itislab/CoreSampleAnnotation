using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationPlane
{
    public class SyncronizerColumnAdapter : LayerSyncronization.ILayersColumn
    {
        private LayeredColumnVM target;
        public SyncronizerColumnAdapter(LayeredColumnVM target) {
            this.target = target;
        }

        public double[] GetLayerHeights()
        {
            return target.Layers.Select(l => l.Length).ToArray();
        }

        public void InsertLayer(int targetIdx, int templateIdx)
        {
            var newLayer = target.Layers[templateIdx].DeepClone();
            target.Layers.Insert(targetIdx, newLayer);
        }

        public void RemoveLayer(int tagetIdx)
        {
            target.Layers.RemoveAt(tagetIdx);
        }

        public void SetLayerHeight(int layerIdx, double height)
        {
            target.Layers[layerIdx].Length = height;
        }
    }
}
