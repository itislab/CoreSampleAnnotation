using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreSampleAnnotation.AnnotationPlane.Template;
using Svg;
using CoreSampleAnnotation.AnnotationPlane;

namespace CoreSampleAnnotation.Reports.SVG
{
    public abstract class LayerClassVmBasedLegendItem {
        protected LayerClassVM LayerClass;
        public LayerClassVmBasedLegendItem(LayerClassVM layerClass)
        {
            LayerClass = layerClass;
        }

        public string Description
        {
            get
            {
                if(!string.IsNullOrEmpty(LayerClass.ShortName))                    
                    return LayerClass.ShortName;
                if (!string.IsNullOrEmpty(LayerClass.Acronym))
                    return LayerClass.Acronym;
                return LayerClass.ID;
            }
        }
    }


    public class SvgIconLegendItem : LayerClassVmBasedLegendItem, ILegendItem
    {
        public SvgIconLegendItem(LayerClassVM layerClass) : base(layerClass)
        {
        }        

        public SvgElement GetPresentation(double width, double height)
        {
            if (LayerClass.IconSvg == null)
                return null;

            SvgFragment copy = (SvgFragment)LayerClass.IconSvg.DeepCopy();

            float imageAspectRatio = copy.Bounds.Width / copy.Bounds.Height;
            float restrictionAspectRation = (float)(width / height);

            copy.ViewBox = copy.Bounds;

            if (imageAspectRatio > restrictionAspectRation)
            {
                //image shape is wider then restriction area.
                //thus width is effective restriction
                copy.Width = (float)width;
                copy.Height = (float)(width / imageAspectRatio);
            }
            else {
                //hight is effective restiction
                copy.Width = (float)(height * imageAspectRatio);
                copy.Height = (float)(height);
            }

            return copy;
        }
    }

    public class BacgroundFillLegendItem : LayerClassVmBasedLegendItem, ILegendItem
    {
        public BacgroundFillLegendItem(LayerClassVM layerClass) : base(layerClass)
        {
        }        

        public SvgElement GetPresentation(double width, double height)
        {
            if (LayerClass.BackgroundPattern == null)
                return null;

            SvgRectangle rect = new SvgRectangle();
            rect.Width = (float)width;
            rect.Height = (float)height;

            rect.Fill = LayerClass.BackgroundPattern;
            rect.Stroke = new SvgColourServer(System.Drawing.Color.Black);
            rect.StrokeWidth = 1;            

            return rect;
        }
    }

    public class LegendGroup : ILegendGroup
    {
        private string groupName;
        private ILegendItem[] items;

        public LegendGroup(string groupName, ILegendItem[] items) {
            this.groupName = groupName;
            this.items = items;
        }

        public string GroupName
        {
            get
            {
                return groupName;
            }
        }

        public ILegendItem[] Items
        {
            get
            {
                return items;
            }
        }
    }
}
