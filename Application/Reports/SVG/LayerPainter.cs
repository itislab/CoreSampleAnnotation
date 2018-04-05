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
                if (cltpVM.Text != null)
                {
                    string[] spans = cltpVM.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                    SvgText text = new SvgText();
                    text.Transforms.Add(new Svg.Transforms.SvgTranslate((float)textXoffset, (float)(availableHeight * 0.5 + textYoffset)));
                    foreach (var span in spans)
                    {
                        SvgTextSpan tspan = new SvgTextSpan();
                        tspan.Text = span;
                        tspan.FontSize = Helpers.dtos(fontSize);
                        tspan.Dy.Add(Helpers.dtos(fontSize*1.2));
                        tspan.X.Add(0);
                        tspan.Fill = blackPaint;
                        text.Children.Add(tspan);
                    }
                    return text;
                }
                else
                    return new SvgGroup();
            }
            else if (vm is LengthLayerVM)
            {
                LengthLayerVM lrlcVM = (LengthLayerVM)vm;
                SvgText text = new SvgText(string.Format("{0:0.##} м", lrlcVM.RealLength));
                text.FontSize = Helpers.dtos(fontSize);
                text.Fill = blackPaint;
                text.Transforms.Add(new Svg.Transforms.SvgTranslate((float)textXoffset, (float)(availableHeight * 0.5 + textYoffset)));
                return text;
            }
            else if (vm is MultiClassificationLayerIconPresentingVM) {
                MultiClassificationLayerIconPresentingVM mclipVM = (MultiClassificationLayerIconPresentingVM)vm;
                SvgGroup group = new SvgGroup();                
                float iconWidth = 32.0f;                

                if (mclipVM.IconsSVG != null)
                {
                    SvgFragment[] fragments = mclipVM.IconsSVG.Where(f => f != null).ToArray();

                    if (fragments.Length == 0)
                        return group;

                    float maxHeight = fragments.Select(f => f.Bounds.Height).Max();

                    float offset = (float)((availableWidth - fragments.Length*iconWidth)*0.5);

                    foreach (var item in fragments)
                    {
                        SvgFragment copy = (SvgFragment)item.DeepCopy();

                        float aspectRatio = copy.Bounds.Width / copy.Bounds.Height;
                        
                        copy.ViewBox = copy.Bounds;
                        copy.X += offset;
                        copy.Y = (float)(availableHeight - maxHeight) * 0.5f;
                        //copy.Transforms.Add(new Svg.Transforms.SvgScale(ratio));
                        copy.Width = iconWidth;
                        copy.Height = iconWidth / aspectRatio;
                        group.Children.Add(copy);
                        offset += iconWidth;
                    }
                }
                return group;
            }
            else
                return new SvgGroup();
        }
    }
}
