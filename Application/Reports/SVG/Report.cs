using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Transforms;
using System.Windows;

namespace CoreSampleAnnotation.Reports.SVG
{
    public struct RenderedSvg {
        public SvgElement SVG;
        public Size RenderedSize;
    }

    /// <summary>
    /// Draw the report column as SVG
    /// </summary>
    public interface ISvgRenderableColumn {
        RenderedSvg RenderHeader();
        RenderedSvg RenderColumn();
    }

    public static class Report
    {
        private static SvgUnit dtos(double value) { return new SvgUnit((float)value); }

        public static SvgDocument Generate(ISvgRenderableColumn[] columns) {
            //generating headers
            SvgGroup headerGroup = new SvgGroup();
            SvgPaintServer paintServer = new SvgColourServer(System.Drawing.Color.Black);            
            double horizontalOffset = 0.0;
            double height = 0;
            for (int i = 0; i < columns.Length; i++)
            {
                RenderedSvg heading = columns[i].RenderHeader();
                SvgRectangle rect = new SvgRectangle();
                height = heading.RenderedSize.Height;
                rect.Width = dtos(heading.RenderedSize.Width);
                rect.Height = dtos(heading.RenderedSize.Height);
                rect.X = dtos(horizontalOffset);
                rect.Y = dtos(0.0);
                rect.Stroke = paintServer;

                heading.SVG.Transforms.Add(new SvgTranslate((float)(horizontalOffset + heading.RenderedSize.Width*0.5), (float)heading.RenderedSize.Height*0.9f));
                heading.SVG.Transforms.Add(new SvgRotate((float)-90.0));

                headerGroup.Children.Add(rect);                                
                headerGroup.Children.Add(heading.SVG);
                horizontalOffset += heading.RenderedSize.Width;


            }
            SvgDocument result = new SvgDocument();
            result.Width = dtos(horizontalOffset*1.1);
            result.Height = dtos(height*1.1);
            result.Fill = new SvgColourServer(System.Drawing.Color.White);
            result.Children.Add(headerGroup);
            return result;
        }
    }
}
