using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Svg;
using CoreSampleAnnotation.AnnotationPlane;
using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;

namespace CoreSampleAnnotation.Reports.SVG
{
    public class BoundaryColumnPainter : ColumnPainter
    {
        private const float labelXoffset = 10.0f;
        private const float labelYoffset = 25.0f;
        private const float fonstSize = 15f;

        protected new ILayerBoundariesVM vm;

        public BoundaryColumnPainter(UIElement headerView, ColumnView view, ColumnVM vm, ILayerBoundariesVM boundariesVM) : base(headerView, view, vm)
        {
            this.vm = boundariesVM;
        }

        public override RenderedSvg RenderColumn()
        {
            RenderedSvg result = base.RenderColumn();
            double width = result.RenderedSize.Width;
            SvgGroup linesGroup = new SvgGroup();

            SvgColourServer blackPaint = new SvgColourServer(System.Drawing.Color.Black);

            int minRank = int.MaxValue;

            LayerBoundary[] boundaries = vm.Boundaries.OrderBy(b => b.Level).ToArray();            

            var rank = boundaries.Select(b => b.Rank).Min();

            for (int i = 0; i < boundaries.Length; i++)
            {                
                LayerBoundary boundary = boundaries[i];
                SvgLine line = new SvgLine();
                line.StartX = Helpers.dtos(0.0);
                line.EndX = Helpers.dtos(width);
                line.StartY = Helpers.dtos(boundary.Level);
                line.EndY = Helpers.dtos(boundary.Level);
                line.Stroke = blackPaint;
                linesGroup.Children.Add(line);

                minRank = Math.Min(minRank, boundary.Rank);                

                string textStr = string.Format("{0}",boundary.Numbers[rank]);

                //putting layer number as well
                SvgText text2 = new SvgText(textStr);
                text2.Transforms.Add(new Svg.Transforms.SvgTranslate(labelXoffset, (float)(boundary.Level + labelYoffset)));
                text2.FontSize = Helpers.dtos(10.0);
                text2.Fill = blackPaint;
                linesGroup.Children.Add(text2);
            }
            result.SVG = linesGroup;
            return result;
        }
    }
}
