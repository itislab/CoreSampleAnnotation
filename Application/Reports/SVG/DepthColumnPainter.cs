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
    public class DepthColumnPainter : ColumnPainter
    {
        protected const float labelXoffset = 10f;
        protected const float labelYoffset = 5f;
        protected const float labelSize = 10f;

        //the tick resolution is 10^DepthResolutionOrder meters. -1 is 10^(-1)= 0.1 meter resolution
        protected const int TickDepthResolutionOrder = -1;

        //the label resolution is 10^LabelDepthResolutionOrder meters. 0 is 10^0 = 1 meter resolution
        protected const int LabelDepthResolutionOrder = -1;

        protected new DepthAxisColumnVM vm;

        public DepthColumnPainter(UIElement headerView, ColumnView view, DepthAxisColumnVM vm) : base(headerView, view, vm)
        {
            this.vm = vm;
        }

        public override RenderedSvg RenderColumn()
        {
            RenderedSvg result =  base.RenderColumn();
            SvgGroup group = new SvgGroup();

            SvgColourServer blackPiant = new SvgColourServer(System.Drawing.Color.Black);

            //first adding ticks
            double tickStep = Math.Pow(10.0, TickDepthResolutionOrder);
            double tickStart;
            if (TickDepthResolutionOrder <= 0)
                tickStart = Math.Round(vm.UpperBound, -TickDepthResolutionOrder);
            else
                tickStart = Math.Round(vm.UpperBound / tickStep)*tickStep;

            if (tickStart < vm.UpperBound)
                tickStart += tickStart;

            double tick = tickStart;

            //How many WPF units in 1 real meter of depth
            double depthScaleFactor = vm.ColumnHeight/(vm.LowerBound - vm.UpperBound);

            Func<double,double> depthToY = depth => (depth - vm.UpperBound) * depthScaleFactor;

            while (tick < vm.LowerBound) {
                SvgLine line = new SvgLine();
                line.Stroke = blackPiant;
                line.StartX = Helpers.dtos(0);
                line.EndX = Helpers.dtos(labelXoffset);
                line.StartY = Helpers.dtos(depthToY(tick));
                line.EndY = Helpers.dtos(depthToY(tick));
                group.Children.Add(line);

                tick += tickStep;

                //SvgText text = new SvgText(string.Format("{0}",tick));
                //text.Transforms.Add(new Svg.Transforms.SvgTranslate());
            }

            //now adding labels
            double labelStep = Math.Pow(10.0, LabelDepthResolutionOrder);
            double labelStart;
            if (LabelDepthResolutionOrder <= 0)
                labelStart = Math.Round(vm.UpperBound, -LabelDepthResolutionOrder);
            else
                labelStart = Math.Round(vm.UpperBound / labelStep) * labelStep;

            if (labelStart < vm.UpperBound)
                labelStart += labelStep;

            double label = labelStart;

            
            while (label < vm.LowerBound)
            {
                SvgText text = new SvgText(string.Format("{0}", label));
                text.Transforms.Add(new Svg.Transforms.SvgTranslate(labelXoffset, (float)depthToY(label) + labelYoffset));
                text.Fill = blackPiant;
                text.FontSize = Helpers.dtos(labelSize);
                group.Children.Add(text);

                label += labelStep;                
            }

            result.SVG = group;
            return result;
        }
    }
}
