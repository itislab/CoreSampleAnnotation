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
    public interface ILayerPainter {
        SvgElement Paint(LayerVM vm, double availableWidth, double availableHeight);
    }

    public class LayeredColumnPainter : ColumnPainter
    {
        protected new ILayerColumn columnVm;
        protected readonly ILayerPainter layerPainter;

        public LayeredColumnPainter(UIElement headerView, ColumnView view, ColumnVM vm, ILayerColumn columnVm, ILayerPainter layerPainter) : base(headerView, view, vm)
        {
            this.columnVm = columnVm;
            this.layerPainter = layerPainter;
        }

        public override RenderedSvg RenderColumn()
        {
            RenderedSvg result = base.RenderColumn();

            SvgGroup group = new SvgGroup();
            SvgColourServer blackPaint = new SvgColourServer(System.Drawing.Color.Black);

            double width = view.RenderSize.Width;

            //drawing layer boundaries
            LayerVM[] layers = columnVm.Layers.ToArray();
            double boundary = 0.0;
            for (int i = 0; i < layers.Length; i++)
            {               
                LayerVM lVM = layers[i];
                boundary += lVM.Length; 
                if (i < layers.Length - 1) {
                    SvgLine line = new SvgLine();
                    line.Stroke = blackPaint;
                    line.StartX = Helpers.dtos(0.0);
                    line.EndX = Helpers.dtos(width);
                    line.StartY = Helpers.dtos(boundary);
                    line.EndY = Helpers.dtos(boundary);
                    group.Children.Add(line);
                }
                // drawing layer itself
                var layerSvg = layerPainter.Paint(layers[i], width, lVM.Length);
                layerSvg.Transforms.Add(new Svg.Transforms.SvgTranslate(0.0f, (float)(boundary - lVM.Length)));
                group.Children.Add(layerSvg);
            }

            result.SVG = group;
            return result;
        }
    }
}
