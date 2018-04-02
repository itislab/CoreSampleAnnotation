using CoreSampleAnnotation.AnnotationPlane;
using CoreSampleAnnotation.AnnotationPlane.Columns;
using CoreSampleAnnotation.AnnotationPlane.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports.SVG
{
    public static class LegendFactory
    {
        public static Tuple<LegendItemKey, ILegendItem>[] GetLegendItemsForLayer(LayerVM layer)
        {
            Dictionary<LegendItemKey, ILegendItem> resultDict = new Dictionary<LegendItemKey, ILegendItem>();

            if (layer is MultiClassificationLayerIconPresentingVM)
            {
                PropertyRepresentation representation = PropertyRepresentation.SvgIcon;
                MultiClassificationLayerIconPresentingVM mclipVM = (MultiClassificationLayerIconPresentingVM)layer;
                if (mclipVM.ClassificationVM.CurrentClasses == null)
                    return new Tuple<LegendItemKey, ILegendItem>[0];
                var classes = mclipVM.ClassificationVM.CurrentClasses.ToArray();
                foreach (var c in classes)
                {
                    LegendItemKey key = new LegendItemKey(mclipVM.ClassificationVM.PropertyName, c.ID, representation);
                    if (!resultDict.ContainsKey(key))
                        resultDict.Add(key, new SvgIconLegendItem(c));
                }
            }

            return resultDict.Select(kvp => Tuple.Create(kvp.Key, kvp.Value)).ToArray();
        }

        public static Tuple<LegendItemKey, ILegendItem>[] GetLegendItemsForVisualLayer(VisualLayerPresentingVM layer)
        {
            if (layer.BackgroundBrush != null && layer.Origin.CurrentClass != null)
            {
                LegendItemKey key = new LegendItemKey(layer.Origin.PropertyName, layer.Origin.CurrentClass.ID, PropertyRepresentation.BackgroundPattern);
                return new Tuple<LegendItemKey, ILegendItem>[] { Tuple.Create(key, new BacgroundFillLegendItem(layer.Origin.CurrentClass) as ILegendItem) };
            }
            else
                return new Tuple<LegendItemKey, ILegendItem>[0];
        }
    }
}
