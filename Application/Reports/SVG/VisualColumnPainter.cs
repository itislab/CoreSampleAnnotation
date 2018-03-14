using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CoreSampleAnnotation.AnnotationPlane;
using Svg;
using CoreSampleAnnotation.AnnotationPlane.Columns;

namespace CoreSampleAnnotation.Reports.SVG
{
    public class VisualColumnPainter : ColumnPainter
    {
        protected new VisualColumnVM vm;

        public VisualColumnPainter(UIElement headerView, ColumnView view, VisualColumnVM vm) : base(headerView, view, vm)
        {
            this.vm = vm;
        }

        public override RenderedSvg RenderColumn()
        {
            var result = base.RenderColumn();

            

            SvgGroup group = new SvgGroup();
            VisualLayerPresentingVM[] layers = vm.Layers.ToArray();
            for (int i = 0; i < layers.Length; i++)
            {
                VisualLayerPresentingVM lvm = layers[i];
                if (lvm.Origin.CurrentClass != null)
                {
                    SvgPatternServer sps = lvm.Origin.CurrentClass.BackgroundPattern;
                    sps.PatternContentUnits = SvgCoordinateUnits.ObjectBoundingBox;
                    sps.PatternUnits = SvgCoordinateUnits.UserSpaceOnUse;
                    float ratio = sps.Width.Value / 64f;
                    sps.Width /= ratio;
                    sps.Height /= ratio;

                    SvgRectangle rect = new SvgRectangle();
                    rect.Fill = sps;
                    rect.X = 0;
                    rect.Y = Helpers.dtos(lvm.Y);
                    rect.Width = Helpers.dtos(lvm.AvailableWidth);
                    rect.Height = Helpers.dtos(lvm.Height);
                    //rect.Stroke = new SvgColourServer(System.Drawing.Color.Red);
                    //rect.StrokeWidth = 2;
                    group.Children.Add(rect);
                }
            }

            result.SVG = group;

            return result;
        }

        public override SvgDefinitionList Definitions
        {
            get
            {
                var defs = base.Definitions;
                VisualLayerPresentingVM[] layers = vm.Layers.ToArray();
                for (int i = 0; i < layers.Length; i++)
                {
                    VisualLayerPresentingVM lvm = layers[i];
                    if (lvm.Origin.CurrentClass != null)
                    {
                        defs.Children.Add(lvm.Origin.CurrentClass.BackgroundPattern);
                    }
                }
                return defs;
            }
        }
    }
}