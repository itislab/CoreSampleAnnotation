using CoreSampleAnnotation.AnnotationPlane;
using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports
{
    public class Layer {
        public ClassificationLayerVM[] Classifications { get; set; }
        public double TopDepth { get; set; }
        public double BottomDepth { get; set; }        
    }

    public static class Helpers
    {
        /// <summary>
        /// Forms helper structure gholding the data in a convenient for the reporting way
        /// </summary>
        /// <param name="template">Possible properties</param>
        /// <param name="layers">Clasification layers</param>
        /// <param name="boundaryDepths">(im meters, positive) one more than layers count</param>
        /// <returns></returns>
        public static Layer[] FormLayers(AnnotationPlane.Template.Property[] template, double[] boundaryDepths, LayeredColumnVM[] classifications) {
            if (template.Length != classifications.Length)
                throw new InvalidOperationException("Templates contains different number of properties than classification columns");
            List<Layer> result = new List<Layer>();
            for (int i = 0; i < boundaryDepths.Length-1; i++)
            {
                Layer l = new Layer();
                List<ClassificationLayerVM> layerClassifications = new List<ClassificationLayerVM>();
                for (int j = 0; j < classifications.Length; j++)
                {
                    layerClassifications.Add(classifications[j].Layers[i] as ClassificationLayerVM);
                }
                l.Classifications = layerClassifications.ToArray();
                l.TopDepth = boundaryDepths[i];
                l.BottomDepth = boundaryDepths[i + 1];
                result.Add(l);
            }
            return result.ToArray();
        }
    }
}
