using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreSampleAnnotation.AnnotationPlane;
using Svg;

namespace CoreSampleAnnotation.Reports.SVG
{
    public class LayerPainter : ILayerPainter
    {
        private SvgColourServer blackPaint = new SvgColourServer(System.Drawing.Color.Black);
        private const double textXoffset = 10.0;
        private const double textYoffset = 4.0;
        private const double fontSize = 10.0;

        public SvgElement Paint(LayerVM vm, double availableWidth, double availableHeight)
        {
            if (vm is ClassificationLayerTextPresentingVM)
            {
                ClassificationLayerTextPresentingVM cltpVM = (ClassificationLayerTextPresentingVM)vm;
                SvgText text = new SvgText(cltpVM.Text);
                text.FontSize = Helpers.dtos(fontSize);
                text.Fill = blackPaint;
                text.Transforms.Add(new Svg.Transforms.SvgTranslate((float)textXoffset, (float)(availableHeight * 0.5 + textYoffset)));
                return text;
            }
            else
                return new SvgGroup();
        }
    }
}
