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

        /// <summary>
        /// Addts a point to points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="point"></param>
        private static void AddPointToCollection(SvgPointCollection points, Point point) {
            points.Add(Helpers.dtos(point.X));
            points.Add(Helpers.dtos(point.Y));
        }

        public override RenderedSvg RenderColumn()
        {
            var result = base.RenderColumn();
            

            SvgGroup group = new SvgGroup();
            VisualLayerPresentingVM[] layers = vm.Layers.ToArray();

            

            for (int i = 0; i < layers.Length; i++)
            {
                VisualLayerPresentingVM lvm = layers[i];
                if (lvm.BackgroundClass.CurrentClass != null)
                {
                    ISideCurveGenerator sideCurveGenerator = null;

                    if (lvm.RightSideClass.CurrentClass != null)
                        sideCurveGenerator = SideCurveGeneratorFactory.GetGeneratorFor(lvm.RightSideClass.CurrentClass.RightSideForm);
                    else
                        sideCurveGenerator = SideCurveGeneratorFactory.GetGeneratorFor(AnnotationPlane.Template.RightSideFormEnum.NotDefined);

                    SvgPatternServer sps = lvm.BackgroundClass.CurrentClass.BackgroundPattern;
                    sps.PatternContentUnits = SvgCoordinateUnits.ObjectBoundingBox;
                    sps.PatternUnits = SvgCoordinateUnits.UserSpaceOnUse;
                    float ratio = sps.Width.Value / 64f;
                    sps.Width /= ratio;
                    sps.Height /= ratio;

                    SvgPolygon poly = new SvgPolygon();
                    poly.Stroke = new SvgColourServer(System.Drawing.Color.Black);
                    poly.StrokeWidth = 1f;
                    poly.Fill = sps;

                    var points = Drawing.GetPolygon(lvm.Width, lvm.Height, sideCurveGenerator).ToArray();

                    SvgPointCollection svgPoints = new SvgPointCollection();
                    for (int j = 0; j < points.Length; j++)
                    {
                        var point = points[j];
                        point.Y += lvm.Y;
                        AddPointToCollection(svgPoints, point);
                    }
                    
                    poly.Points = svgPoints;
                    
                    group.Children.Add(poly);
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
                    if (lvm.BackgroundClass.CurrentClass != null)
                    {
                        defs.Children.Add(lvm.BackgroundClass.CurrentClass.BackgroundPattern);
                    }
                }
                return defs;
            }
        }
    }
}