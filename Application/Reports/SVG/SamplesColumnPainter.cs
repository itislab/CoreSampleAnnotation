using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CoreSampleAnnotation.AnnotationPlane;
using Svg;

namespace CoreSampleAnnotation.Reports.SVG
{
    public class SamplesColumnPainter : ColumnPainter
    {
        protected new SamplesColumnVM vm;

        protected const float textXoffset = 10.0f;
        protected const float textYoffset = 10.0f;
        protected const double textFontSize = 10.0;
        protected const double circleXoffset = 10.0;
        protected const double circleRadius = 3.0;
        protected SvgColourServer blackPaint = new SvgColourServer(System.Drawing.Color.Black);

        public SamplesColumnPainter(UIElement headerView, ColumnView view, SamplesColumnVM vm) : base(headerView, view, vm)
        {
            this.vm = vm;
        }

        public override RenderedSvg RenderColumn()
        {
            RenderedSvg result = base.RenderColumn();

            SvgGroup group = new SvgGroup();
            for (int i = 0; i < vm.Samples.Length; i++)
            {
                SampleVM sample = vm.Samples[i];
                //sample circle sign
                SvgCircle circle = new SvgCircle();
                circle.CenterX = Helpers.dtos(circleXoffset);
                circle.CenterY = Helpers.dtos(sample.Level);
                circle.Radius = Helpers.dtos(circleRadius);
                circle.Stroke = blackPaint;
                group.Children.Add(circle);

                //sample depth label
                SvgText depthText = new SvgText(string.Format("{0:0.##} м", sample.Depth));
                depthText.Transforms.Add(new Svg.Transforms.SvgTranslate(textXoffset,(float)(sample.Level + 2.0*circleRadius + textFontSize*0.5)));
                depthText.Fill = blackPaint;
                depthText.FontSize = Helpers.dtos(textFontSize);
                group.Children.Add(depthText);
            }

            result.SVG = group;

            return result;
        }
    }
}
