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
                SvgGroup levelGroup = new SvgGroup();
                if (lvm.BackgroundClass.CurrentClass != null)
                {
                    ISideCurveGenerator rightSideCurveGenerator = null;
                    if ((lvm.RightSideClass != null) && (lvm.RightSideClass.CurrentClass != null))
                        rightSideCurveGenerator = SideCurveGeneratorFactory.GetGeneratorFor(lvm.RightSideClass.CurrentClass.RightSideForm);
                    else
                        rightSideCurveGenerator = SideCurveGeneratorFactory.GetGeneratorFor(AnnotationPlane.Template.RightSideFormEnum.NotDefined);

                    SvgPolyline rightEdge = new SvgPolyline
                    {
                        Stroke = new SvgColourServer(System.Drawing.Color.Black),
                        StrokeWidth = 1f
                    };

                    var rightPoints = Drawing.GetRightPolyline(lvm.Width, lvm.Height, rightSideCurveGenerator).ToArray();

                    SvgPointCollection svgPoints = new SvgPointCollection();
                    for (int j = 0; j < rightPoints.Length; j++)
                    {
                        var point = rightPoints[j];
                        point.Y += lvm.Y;
                        AddPointToCollection(svgPoints, point);
                    }

                    rightEdge.Points = svgPoints;

                    levelGroup.Children.Add(rightEdge);


                    ISideCurveGenerator bottomSideCurveGenerator = null;
                    if ((lvm.BottomSideClass != null) && (lvm.BottomSideClass.CurrentClass != null))
                        bottomSideCurveGenerator = SideCurveGeneratorFactory.GetGeneratorFor(lvm.BottomSideClass.CurrentClass.BottomSideForm);
                    else
                        bottomSideCurveGenerator = SideCurveGeneratorFactory.GetGeneratorFor(AnnotationPlane.Template.BottomSideFormEnum.NotDefined);

                    SvgPolyline bottomEdge = new SvgPolyline
                    {
                        Stroke = new SvgColourServer(System.Drawing.Color.Black),
                        StrokeLineCap = SvgStrokeLineCap.Round,
                        StrokeWidth = 1f
                    };

                    if (lvm.BottomSideClass.CurrentClass.BottomSideForm == AnnotationPlane.Template.BottomSideFormEnum.Dotted) {
                        bottomEdge.StrokeDashArray = new List<float>() { 3, 3 }.
                            Select(p => new SvgUnit(p)) as SvgUnitCollection;
                    }

                    var bottomPoints = Drawing.GetBottomPolyline(lvm.Width, lvm.Height, bottomSideCurveGenerator).ToArray();

                    SvgPointCollection svgBottomPoints = new SvgPointCollection();
                    for (int j = 0; j < bottomPoints.Length; j++)
                    {
                        var point = bottomPoints[j];
                        point.Y += lvm.Y;
                        AddPointToCollection(svgBottomPoints, point);
                    }

                    bottomEdge.Points = svgBottomPoints;

                    levelGroup.Children.Add(bottomEdge);


                    SvgPolygon bckgrPolygon = new SvgPolygon
                    {
                        StrokeWidth = 0f
                    };

                    SvgPatternServer sps = lvm.BackgroundClass.CurrentClass.BackgroundPattern;
                    sps.PatternContentUnits = SvgCoordinateUnits.ObjectBoundingBox;
                    sps.PatternUnits = SvgCoordinateUnits.UserSpaceOnUse;
                    float ratio = sps.Width.Value / 64f;
                    sps.Width /= ratio;
                    sps.Height /= ratio;

                    bckgrPolygon.Fill = sps;

                    var bckgrPoints = Drawing.GetBackgroundPolyline(lvm.Width, lvm.Height, rightSideCurveGenerator).ToArray();

                    SvgPointCollection svgBckgrPoints = new SvgPointCollection();
                    for (int j = 0; j < bckgrPoints.Length; j++)
                    {
                        var point = bckgrPoints[j];
                        point.Y += lvm.Y;
                        AddPointToCollection(svgBckgrPoints, point);
                    }

                    bckgrPolygon.Points = svgBckgrPoints;

                    levelGroup.Children.Add(bckgrPolygon);


                    group.Children.Add(levelGroup);
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