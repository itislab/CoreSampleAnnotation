using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports.RTF
{
    public class PropertyDescription
    {
        public string Name { get; private set; }
        public string[] Values { get; private set; }
        public string Comment { get; private set; }

        public PropertyDescription(string name, string[] values, string comment = "")
        {
            Name = name;
            Values = values;
            Comment = comment;
        }
    }

    public class LayerDescrition
    {
        public PropertyDescription[] Properties { get; private set; }
        public LayerDescrition(PropertyDescription[] properties)
        {
            Properties = properties;
        }
    }

    public class Sample
    {
        public string ID { get; private set; }
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double Depth { get; private set; }
        public string Purpose { get; private set; }

        public Sample(string id, double depth, string purpose)
        {
            ID = id;
            Depth = depth;
            Purpose = purpose;
        }
    }


    public class LayerBoundary
    {
        public int[] OrderNumbers { get; private set; }

        public double Depth { get; private set; }

        public int Rank { get; private set; }

        public LayerBoundary(int[] orderNumbers, double depth, int rank)
        {
            OrderNumbers = orderNumbers;
            Depth = depth;
            Rank = rank;
        }
    }

    public static class ReportHelpers
    {
        private static int LeftColWidth = 7000;
        private static int RightcolWidth = 3000;

        /// <summary>
        /// Generates the row depicting extraction interval
        /// </summary>
        /// <param name="upperDepth">in meters (positive)</param>
        /// <param name="lowerDepth">in meters (positive)</param>
        /// <param name="extractedLength">in meters (positive)</param>
        /// <returns></returns>
        public static ReportRow GetIntervalRow(double upperDepth, double lowerDepth, double extractedLength)
        {
            return new ReportRow(new TextCell[] {
                        new TextCell(string.Format("Интервал {0:0.#}-{1:0.##}/{2:0.##} м (присутствует {3:0.##} м)",upperDepth,lowerDepth,lowerDepth-upperDepth,extractedLength),LeftColWidth,isBold: true),
                        new TextCell("",RightcolWidth)
            });
        }

        /// <summary>
        /// Generates the row depicting group of layers with specific rank
        /// </summary>
        /// <param name="length">group total length (in meters)</param>
        /// <returns></returns>
        public static ReportRow GetRankDescrRow(int orderNumber, string rankName, double length)
        {
            return new ReportRow(new TextCell[] {
                        new TextCell(string.Format("{0} {1} ({2:0.## м})",orderNumber,rankName,length),LeftColWidth, isBold: true),
                        new TextCell("",RightcolWidth)
            });
        }

        /// <summary>
        /// Generates row deorcting specific layer
        /// </summary>        
        /// <param name="length">in meters</param>        
        /// <returns></returns>
        public static ReportRow GetLayerDescrRow(int orderNum, double length, LayerDescrition description, Sample[] samples)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} слой ({1:0.##} м).\n", orderNum, length);
            List<string> propStrings = new List<string>();
            foreach (PropertyDescription property in description.Properties)
            {
                if (property.Values == null)
                    continue;
                StringBuilder sb2 = new StringBuilder();
                sb2.AppendFormat("{0} - {1}", property.Name, string.Join(", ", property.Values.Select(v => v.ToLower())));
                if (!string.IsNullOrEmpty(property.Comment))
                {
                    sb2.AppendFormat(". {0}", property.Comment);
                }
                propStrings.Add(sb2.ToString());
            }

            sb.Append(string.Join(". ", propStrings.ToArray()));

            if (propStrings.Count > 0)
                sb.Append(".");

            StringBuilder samplesSb = new StringBuilder();
            List<string> sampleStrings = new List<string>();
            foreach (var sample in samples)
                sampleStrings.Add(string.Format("{1:0.##} м; {2}", sample.ID, sample.Depth, sample.Purpose));

            return new ReportRow(new TextCell[] {
                        new TextCell(sb.ToString(),LeftColWidth),
                        new TextCell(string.Join("\n",sampleStrings),RightcolWidth)
                    }
                );

        }

        /// <summary>
        /// Forms a table by filling it with supplied data
        /// </summary>        
        /// <param name="boundaries">including the "outer boundaries" of bounding layer</param>            
        /// <param name="samples">All samples in the project</param>
        /// <returns></returns>
        public static ReportTable GenerateTableContents(
        Intervals.BoreIntervalVM[] intervals,
        LayerBoundary[] boundaries,
        LayerDescrition[] layers,
        string[] rankNames,
        Sample[] samples,
        AnnotationDirection annotationDirection)
        {
            //asserting intervals
            //lower - upper bounds order
            //intersection check
            List<KeyValuePair<double, int>> intBounds = new List<KeyValuePair<double, int>>();
            for (int i = 0; i < intervals.Length; i++)
            {
                Intervals.BoreIntervalVM interval = intervals[i];
                if (interval.LowerDepth < interval.UpperDepth)
                    throw new ArgumentException("The upper bound is lower than lower bound");
                intBounds.Add(new KeyValuePair<double, int>(interval.UpperDepth, 1)); // +1 opens the interval
                intBounds.Add(new KeyValuePair<double, int>(interval.LowerDepth, -1)); // -1 closes the interval
            }
            // detecting intervals with touching bounds; removing these bounds
            double[] keys = intBounds.Select(kvp => kvp.Key).ToArray();
            foreach (int key in keys)
            {
                int curKeyValsSum = intBounds.Where(kvp => kvp.Key == key).Sum(kvp => kvp.Value);
                if (curKeyValsSum == 0)
                {
                    intBounds = intBounds.Where(kvp => kvp.Key != key).ToList();
                }
            }

            int[] events = intBounds.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
            int sum = 0;
            for (int i = 0; i < events.Length; i++)
            {
                sum += events[i];
                if (Math.Abs(sum) > 1)
                    throw new InvalidOperationException("You've passed intersecting intervals");
            }


            List<ReportRow> rows = new List<ReportRow>();

            if (boundaries.Length - 1 != layers.Length)
                throw new ArgumentException("Layers count must be exactly one more than boundary count");

            string directionString = string.Empty;
            switch (annotationDirection) {
                case AnnotationDirection.UpToBottom:
                    directionString = "сверху вниз";
                    break;
                case AnnotationDirection.BottomToUp:
                    directionString = "снизу вверх";
                    break;
            }

            ReportRow header = new ReportRow(
                new TextCell[] {
                new TextCell(
                    string.Format("Описание керна {0}.\nИнтервал / выход керна в м.",directionString),
                    LeftColWidth, horizontalAlignement:TextAlignement.Centered, isBold:true),
                new TextCell("место отбора от начала керна, м; описание", RightcolWidth, horizontalAlignement: TextAlignement.Centered, isBold: true)
                });

            rows.Add(header);

            int reportedInIndex;
            int inIndex;
            int laIndex;
            int layerOrderNum;

            //for now works with increasing depths order
            switch (annotationDirection)
            {
                case AnnotationDirection.UpToBottom:
                    intervals = intervals.OrderBy(i => i.UpperDepth).ToArray();
                    for (int i = 0; i < boundaries.Length - 1; i++)
                        if (boundaries[i + 1].Depth < boundaries[i].Depth)
                            throw new NotSupportedException("Boundary depths must increase");

                    reportedInIndex = -1;
                    inIndex = 0;
                    laIndex = 0;
                    layerOrderNum = 1;
                    while (inIndex < intervals.Length && laIndex < layers.Length)
                    {
                        if (reportedInIndex != inIndex)
                        {
                            //adding row depicting interval start
                            rows.Add(GetIntervalRow(intervals[inIndex].UpperDepth, intervals[inIndex].LowerDepth, intervals[inIndex].ExtractedLength));
                            reportedInIndex = inIndex;
                        }
                        LayerBoundary upperLabound = boundaries[laIndex];
                        double curLaUpper = upperLabound.Depth;
                        double curLaLower = boundaries[laIndex + 1].Depth; //boundraies array always contains one more element than layers
                        double curIntUpper = intervals[inIndex].UpperDepth;
                        double curIntLower = intervals[inIndex].UpperDepth + intervals[inIndex].ExtractedLength;
                        bool doInInc = false;
                        bool doLaInc = false;
                        bool skipReporting = false;

                        //analysing the relative position of the interval and the layer
                        if (curIntLower >= curLaLower)
                        {
                            //the interval includes the end of the layer or interval is entirly below the layer
                            if (curIntUpper > curLaLower)
                            {
                                //the entire interal is below the layer
                                skipReporting = true; //the layer is not reported
                            }
                            else
                            {
                                if (curIntUpper > curLaUpper)
                                {
                                    //coersing layer upper bound to match interval
                                    curLaUpper = curIntUpper;
                                }
                            }
                            doLaInc = true;
                        }
                        else
                        {
                            //the layer includes the end of the interval
                            curLaLower = curIntLower; //coersing layer lower bound to match interval
                            if (curIntUpper > curLaUpper)
                            {
                                //coersing layer upper bound to match interval
                                curLaUpper = curIntUpper;
                            }

                            doInInc = true;
                        }

                        if (!skipReporting)
                        {
                            if (upperLabound.Rank > 0)
                            {
                                //we need to add rank row
                                for (int rank = upperLabound.Rank; rank > 0; rank--)
                                {
                                    double length = 0.0;
                                    //calculating total length. starting from cur bound till the next bound of the same rank or higher
                                    for (int i = laIndex + 1; i < boundaries.Length; i++)
                                    {
                                        int curRank = boundaries[i].Rank;
                                        length += boundaries[i].Depth - boundaries[i - 1].Depth;
                                        if (curRank >= rank)
                                            break;
                                    }

                                    length = Math.Min(length, curIntLower - upperLabound.Depth);
                                    rows.Add(GetRankDescrRow(upperLabound.OrderNumbers[rank], rankNames[rank], length));
                                }
                                layerOrderNum = 1; //ranks higher than 0 reset the numbering of layers
                            }
                            var layerSamples = samples.Where(s => (s.Depth > curLaUpper) && (s.Depth < curLaLower)).ToArray();
                            rows.Add(GetLayerDescrRow(layerOrderNum++, curLaLower - curLaUpper, layers[laIndex], layerSamples));
                        }
                        if (doLaInc)
                            laIndex++;
                        if (doInInc)
                            inIndex++;
                    }
                    break;
                case AnnotationDirection.BottomToUp:
                    intervals = intervals.OrderBy(i => i.UpperDepth).ToArray();
                    for (int i = boundaries.Length - 2; i >= 0; i--)
                        if (boundaries[i + 1].Depth < boundaries[i].Depth)
                            throw new NotSupportedException("Boundary depths must increase");

                    reportedInIndex = intervals.Length;
                    inIndex = intervals.Length - 1;
                    laIndex = layers.Length - 1;
                    layerOrderNum = 1;
                    //while (inIndex < intervals.Length && laIndex < layers.Length)
                    while (inIndex >= 0 && laIndex >= 0)
                    {
                        if (reportedInIndex != inIndex)
                        {
                            //adding row depicting interval start
                            rows.Add(GetIntervalRow(intervals[inIndex].UpperDepth, intervals[inIndex].LowerDepth, intervals[inIndex].ExtractedLength));
                            reportedInIndex = inIndex;
                        }
                        LayerBoundary upperLabound = boundaries[laIndex];
                        LayerBoundary lowerLaBound = boundaries[laIndex + 1];
                        double curLaUpper = upperLabound.Depth;
                        double curLaLower = boundaries[laIndex + 1].Depth; //boundraies array always contains one more element than layers
                        double curIntUpper = intervals[inIndex].UpperDepth;
                        double curIntLower = intervals[inIndex].UpperDepth + intervals[inIndex].ExtractedLength;
                        bool doInDec = false;
                        bool doLaDec = false;
                        bool skipReporting = false;

                        //analysing the relative position of the interval and the layer
                        //if (curIntLower >= curLaLower)
                        if (curIntUpper <= curLaUpper)
                        {
                            //the interval includes the end of the layer or interval is entirly alte the layer

                            //if (curIntUpper > curLaLower)
                            if (curIntLower < curLaUpper)
                            {
                                //the entire interal is after the layer
                                skipReporting = true; //the layer is not reported
                            }
                            else
                            {
                                //if (curIntUpper > curLaUpper)
                                if (curIntLower < curLaLower)
                                {
                                    //coersing layer lower bound to match interval

                                    //curLaUpper = curIntUpper;
                                    curLaLower = curIntLower;
                                }
                            }
                            doLaDec = true;
                        }
                        else
                        {
                            //the layer includes the end of the interval
                            //curLaLower = curIntLower; //coersing layer lower bound to match interval
                            curLaUpper = curIntUpper; //coersing layer upper bound to match interval
                            //if (curIntUpper > curLaUpper)
                            if (curIntLower < curLaLower)
                            {
                                //coersing layer lower bound to match interval

                                //curLaUpper = curIntUpper;
                                curLaLower = curIntLower;
                            }

                            doInDec = true;
                        }

                        if (!skipReporting)
                        {
                            //if (upperLabound.Rank > 0)
                            if (lowerLaBound.Rank > 0)
                            {
                                //we need to add rank row
                                for (int rank = lowerLaBound.Rank; rank > 0; rank--)
                                {
                                    double length = 0.0;
                                    //calculating total length. starting from cur bound till the next bound of the same rank or higher
                                    //for (int i = laIndex + 1; i < boundaries.Length; i++)
                                    for (int i = laIndex; i >= 0; i--)
                                    {
                                        int curRank = boundaries[i].Rank;
                                        //length += boundaries[i].Depth - boundaries[i - 1].Depth;
                                        length += boundaries[i+1].Depth - boundaries[i].Depth;
                                        if (curRank >= rank)
                                            break;
                                    }

                                    //length = Math.Min(length, curIntLower - upperLabound.Depth);
                                    length = Math.Min(length, lowerLaBound.Depth - curIntUpper);
                                    if(rank == lowerLaBound.Rank)
                                        //as the lower boundary contains passed order number, we need to repot the next numbe (incremented by one)
                                        rows.Add(GetRankDescrRow(lowerLaBound.OrderNumbers[rank]+1, rankNames[rank], length));
                                    else
                                        //passing hight rank bundary means that lower rank numberings reset
                                        rows.Add(GetRankDescrRow(1, rankNames[rank], length));
                                }
                                layerOrderNum = 1; //ranks higher than 0 reset the numbering of layers
                            }
                            var layerSamples = samples.Where(s => (s.Depth > curLaUpper) && (s.Depth < curLaLower)).ToArray();
                            rows.Add(GetLayerDescrRow(layerOrderNum++, curLaLower - curLaUpper, layers[laIndex], layerSamples));
                        }
                        if (doLaDec)
                            laIndex--;
                        if (doInDec)
                            inIndex--;
                    }
                    break;
                default:
                    throw new NotSupportedException();
                    break;
            }

            return new ReportTable(rows.ToArray());
        }

    }
}
