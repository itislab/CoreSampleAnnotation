using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreSampleAnnotation.AnnotationPlane;
using System.Windows;
using Svg;

namespace CoreSampleAnnotation.Reports.SVG
{
    /// <summary>
    /// Capable of generating SVG for the column header and for the column body
    /// </summary>
    public class ColumnPainter : ISvgRenderableColumn
    {
        protected readonly UIElement headerView;
        protected readonly ColumnVM vm;
        protected readonly ColumnView view;

        public ColumnPainter(UIElement headerView, ColumnView view, ColumnVM vm) {
            this.headerView = headerView;
            this.vm = vm;
            this.view = view;
        }

        public virtual RenderedSvg RenderColumn()
        {
            RenderedSvg result = new RenderedSvg();
            result.RenderedSize = view.RenderSize;
            result.SVG = new Svg.SvgGroup();
            return result;
        }

        public RenderedSvg RenderHeader()
        {            
            RenderedSvg result = new RenderedSvg();

            SvgPaintServer blackPaint = new SvgColourServer(System.Drawing.Color.Black);

            string[] spans = vm.Heading.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            SvgText text = new SvgText();
            float fontSize = 10.0f;
                        
            foreach (var span in spans)
            {
                SvgTextSpan tspan = new SvgTextSpan();
                tspan.Text = span;
                tspan.FontSize = new SvgUnit(fontSize);
                tspan.Dy.Add(Helpers.dtos(fontSize * 1.2));
                tspan.X.Add(0);
                tspan.Fill = blackPaint;
                text.Children.Add(tspan);
            }
            //text.Transforms.Add(new Svg.Transforms.SvgTranslate(0, (float)(- (fontSize * 1.2) * spans.Length * 0.5)));            

            result.RenderedSize = headerView.RenderSize;
            result.SVG = text;
            return result;
        }

        public virtual SvgDefinitionList Definitions {
            get {
                return new SvgDefinitionList();
            }
        }
    }
}
