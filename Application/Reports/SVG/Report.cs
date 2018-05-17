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
    public enum PropertyRepresentation { SvgIcon, BackgroundPattern }

    /// <summary>
    /// Uniqely identifies a pair: particular class of particular property
    /// </summary>
    public struct LegendItemKey
    {
        public readonly string PropID;
        public readonly string ClassID;
        public readonly PropertyRepresentation Representation;

        public LegendItemKey(string propID, string classID, PropertyRepresentation representation) {
            PropID = propID;
            ClassID = classID;
            Representation = representation;
        }
    }

    public struct RenderedSvg
    {
        public SvgElement SVG;
        public Size RenderedSize;
    }

    public interface ILegendItem
    {
        SvgElement GetPresentation(double width, double height);
        string Description { get; }
    }

    /// <summary>
    /// Draw the report column as SVG
    /// </summary>
    public interface ISvgRenderableColumn
    {
        RenderedSvg RenderHeader();
        RenderedSvg RenderColumn();
        SvgDefinitionList Definitions { get; }
    }

    /// <summary>
    /// Represents a legend for single representation
    /// </summary>
    public interface ILegendGroup
    {
        string GroupName { get; }
        ILegendItem[] Items { get; }

    }

    public static class Report
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="legend"></param>
        /// <param name="oneMeterLength">How long is the one real meter of length in SVG</param>
        /// <returns></returns>
        public static SvgDocument Generate(ISvgRenderableColumn[] columns, ILegendGroup[] legend, float oneMeterLength)
        {
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
                rect.Width = Helpers.dtos(heading.RenderedSize.Width);
                rect.Height = Helpers.dtos(heading.RenderedSize.Height);
                rect.X = Helpers.dtos(horizontalOffset);
                rect.Y = Helpers.dtos(0.0);
                rect.Stroke = blackPaint;                

                heading.SVG.Transforms.Add(new SvgTranslate((float)(horizontalOffset + heading.RenderedSize.Width * 0.3), (float)heading.RenderedSize.Height * 0.9f));
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
                rect.Width = Helpers.dtos(column.RenderedSize.Width);
                rect.Height = Helpers.dtos(column.RenderedSize.Height);
                rect.X = Helpers.dtos(horizontalOffset);
                rect.Y = Helpers.dtos(0.0);
                rect.Stroke = blackPaint;

                column.SVG.Transforms.Add(new SvgTranslate((float)(horizontalOffset)));

                columnsGroup.Children.Add(rect);
                columnsGroup.Children.Add(column.SVG);

                horizontalOffset += column.RenderedSize.Width;
            }

            //generating legend group
            SvgGroup legendGroup = new SvgGroup();

            const float legendYGap = 30.0f;
            const float legendXOffset = 10.0f;

            
            legendGroup.Transforms.Add(new SvgTranslate(legendXOffset, Helpers.dtos(headerHeight + columnHeight + legendYGap)));
            
            const float titleYoffset = 60.0f;
            const float itemsYoffset = 20.0f;
            const float itemYgap = 20.0f;
            const float interGroupYgap = 15.0f;
            const float itemImageWidth = 64.0f;
            const float itemImageHeight = 32.0f;
            const float descrXoffset = 150.0f;

            SvgText legendTitle = new SvgText("Условные обозначения");
            legendTitle.FontSize = 22;
            legendTitle.Fill = new SvgColourServer(System.Drawing.Color.Black);
            legendTitle.Transforms.Add(new SvgTranslate(30.0f, Helpers.dtos(titleYoffset * 0.25f)));
            legendGroup.Children.Add(legendTitle);

            float currGroupOffset = 0.0f;
            int k = 0;
            foreach (ILegendGroup group in legend)
            {
                //title
                SvgText groupTitle = new SvgText(group.GroupName);
                groupTitle.FontSize = new SvgUnit((float)18);
                groupTitle.Fill = new SvgColourServer(System.Drawing.Color.Black);
                groupTitle.Transforms.Add(new SvgTranslate(0.0f, currGroupOffset + titleYoffset));
                legendGroup.Children.Add(groupTitle);

                //items
                var items = group.Items;

                int j = -1;

                SvgElement[] fragments = items.Select(item => item.GetPresentation(itemImageWidth, itemImageHeight)).ToArray();

                int drawnItems = 0;

                foreach (var item in items)
                {
                    j++;
                    if (fragments[j] == null)
                        continue;

                    float yOffset = currGroupOffset + titleYoffset + itemsYoffset + drawnItems * (itemYgap+ itemImageHeight);

                    if (fragments[j] is SvgFragment)
                    {
                        SvgFragment fragment = (SvgFragment)fragments[j];
                        fragment.X = 0;
                        fragment.Y = yOffset;
                    }
                    else
                        fragments[j].Transforms.Add(new SvgTranslate(0, yOffset));

                    legendGroup.Children.Add(fragments[j]);

                    SvgText text = new SvgText(item.Description);
                    text.FontSize = new SvgUnit((float)14);
                    text.Fill = new SvgColourServer(System.Drawing.Color.Black);
                    text.Transforms.Add(new SvgTranslate(descrXoffset, yOffset+ itemImageHeight * 0.5f));
                    legendGroup.Children.Add(text);
                    drawnItems++;
                }
                currGroupOffset += titleYoffset + itemsYoffset + (itemYgap + itemImageHeight) * drawnItems + interGroupYgap;
                k++;
            }

            //generating one meter length sample
            SvgGroup meterGroup = new SvgGroup();

            SvgGroup meter = new SvgGroup();
            SvgLine line1 = new SvgLine();
            line1.StartX = 0;
            line1.StartY = 10;
            line1.EndX = oneMeterLength;
            line1.EndY = 10;
            line1.Stroke = blackPaint;
            SvgLine line2 = new SvgLine();
            line2.StartX = 0;
            line2.EndX = 0;
            line2.StartY = 0;
            line2.EndY = 20;
            line2.Stroke = blackPaint;
            SvgLine line3 = new SvgLine();
            line3.StartX = oneMeterLength;
            line3.EndX = oneMeterLength;
            line3.StartY = 0;
            line3.EndY = 20;
            line3.Stroke = blackPaint;
            meter.Children.Add(line1);
            meter.Children.Add(line2);
            meter.Children.Add(line3);
            meter.Transforms.Add(new SvgTranslate(30.0f, Helpers.dtos(titleYoffset)));
            meterGroup.Children.Add(meter);
            meterGroup.Transforms.Add(new SvgTranslate(300, Helpers.dtos(headerHeight + columnHeight + legendYGap)));
            SvgText meterTitle = new SvgText("Масштаб (1 метр)");
            meterTitle.FontSize = 22;
            meterTitle.Fill = new SvgColourServer(System.Drawing.Color.Black);
            meterTitle.Transforms.Add(new SvgTranslate(30, Helpers.dtos(titleYoffset * 0.25f)));
            meterGroup.Children.Add(meterTitle);


            //gathering definitions
            SvgDefinitionList allDefs = new SvgDefinitionList();
            for (int i = 0; i < columns.Length; i++)
            {
                SvgDefinitionList defs = columns[i].Definitions;
                foreach (SvgPatternServer def in defs.Children)
                {
                    //overridings tile size                    
                    allDefs.Children.Add(def);
                }
            }

            SvgDocument result = new SvgDocument();
            result.Children.Add(allDefs);
            result.Width = Helpers.dtos(horizontalOffset);
            result.Height = Helpers.dtos((headerHeight + columnHeight + legendYGap + currGroupOffset));
            result.Fill = new SvgColourServer(System.Drawing.Color.White);
            result.Children.Add(headerGroup);
            result.Children.Add(columnsGroup);
            result.Children.Add(legendGroup);
            result.Children.Add(meterGroup);

            return result;
        }
    }
}
