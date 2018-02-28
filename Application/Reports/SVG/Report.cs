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
            SvgPaintServer blackPaint = new SvgColourServer(System.Drawing.Color.Black);            
            double horizontalOffset = 0.0;
            double headerHeight = 0;
            for (int i = 0; i < columns.Length; i++)
            {
                RenderedSvg heading = columns[i].RenderHeader();
                SvgRectangle rect = new SvgRectangle();
                headerHeight = heading.RenderedSize.Height;
                rect.Width = dtos(heading.RenderedSize.Width);
                rect.Height = dtos(heading.RenderedSize.Height);
                rect.X = dtos(horizontalOffset);
                rect.Y = dtos(0.0);
                rect.Stroke = blackPaint;

                heading.SVG.Transforms.Add(new SvgTranslate((float)(horizontalOffset + heading.RenderedSize.Width*0.5), (float)heading.RenderedSize.Height*0.9f));
                heading.SVG.Transforms.Add(new SvgRotate((float)-90.0));

                headerGroup.Children.Add(rect);                                
                headerGroup.Children.Add(heading.SVG);
                horizontalOffset += heading.RenderedSize.Width;


            }
            //generating columns
            SvgGroup columnsGroup = new SvgGroup();
            double columnHeight = 0.0;
            horizontalOffset = 0.0;
            columnsGroup.Transforms.Add(new SvgTranslate(0.0f, (float)headerHeight));
            for (int i = 0; i < columns.Length; i++)
            {
                RenderedSvg column = columns[i].RenderColumn();
                SvgRectangle rect = new SvgRectangle();
                columnHeight = column.RenderedSize.Height;
                rect.Width = dtos(column.RenderedSize.Width);
                rect.Height = dtos(column.RenderedSize.Height);
                rect.X = dtos(horizontalOffset);
                rect.Y = dtos(0.0);
                rect.Stroke = blackPaint;

                columnsGroup.Children.Add(rect);

                horizontalOffset += column.RenderedSize.Width;
            }

            SvgDocument result = new SvgDocument();
            result.Width = dtos(horizontalOffset);
            result.Height = dtos((headerHeight+columnHeight));
            result.Fill = new SvgColourServer(System.Drawing.Color.White);
            result.Children.Add(headerGroup);
            result.Children.Add(columnsGroup);
            return result;
        }
    }
}
