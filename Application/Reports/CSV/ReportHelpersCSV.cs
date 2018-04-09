using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports.CSV {
    public static class CSVReportHelpers {
        /// <summary>
        /// Generates the row depicting group of layers with specific rank
        /// </summary>
        /// <param name="length">group total length (in meters)</param>
        /// <returns></returns>
        public static string GetRankDescrRow(int orderNumber, string rankName, double length) {
            return string.Format("{0} {1}", orderNumber, rankName);
        }

        private static List<string> GetGroupPackNumbersForUpperLaBound(RTF.LayerBoundary upperLaBound, int laIndex, RTF.LayerBoundary[] boundaries, double curIntLower, string[] rankNames) {
            List<string> rrl = new List<string>();
            for (int rank = upperLaBound.Rank; rank > 0; rank--) {
                double length = 0.0;
                for (int i = laIndex + 1; i < boundaries.Length; i++) {
                    int curRank = boundaries[i].Rank;
                    length += boundaries[i].Depth - boundaries[i - 1].Depth;
                    if (curRank >= rank)
                        break;
                }

                length = Math.Min(length, curIntLower - upperLaBound.Depth);
                //rrl.Add(GetRankDescrRow(upperLaBound.OrderNumbers[rank], rankNames[rank], length));
                rrl.Add(upperLaBound.OrderNumbers[rank].ToString());
            }

            return rrl;
        }

        private static List<string> GetGroupPackNumbersForLowerLaBound(RTF.LayerBoundary lowerLaBound, int laIndex, RTF.LayerBoundary[] boundaries, double curIntUpper, string[] rankNames) {
            List<string> rrl = new List<string>();
            for (int rank = lowerLaBound.Rank; rank > 0; rank--) {
                double length = 0.0;
                for (int i = laIndex; i >= 0; i--) {
                    int curRank = boundaries[i].Rank;
                    length += boundaries[i + 1].Depth - boundaries[i].Depth;
                    if (curRank >= rank)
                        break;
                }
                
                length = Math.Min(length, lowerLaBound.Depth - curIntUpper);
                if (rank == lowerLaBound.Rank) {
                    switch (rank) {
                        case 2:
                            //rrl.Add(GetRankDescrRow(lowerLaBound.OrderNumbers[2] + 1, rankNames[2], length));
                            rrl.Add((lowerLaBound.OrderNumbers[2] + 1).ToString());
                            break;
                        case 1:
                            //rrl.Add(GetRankDescrRow(lowerLaBound.OrderNumbers[0] + 1, rankNames[1], length));
                            rrl.Add((lowerLaBound.OrderNumbers[0] + 1).ToString());
                            break;
                        default:
                            break;
                    }
                }
                else {
                    //rrl.Add(GetRankDescrRow(1, rankNames[rank], length));
                    rrl.Add("1");
                }
            }

            return rrl;
        }


        /// <summary>
        /// Generates row deorcting specific layer
        /// </summary>        
        /// <param name="length">in meters</param>        
        /// <returns></returns>
        public static ReportRow GetLayerDescrRow(int orderNum, double lower, double upper, RTF.LayerDescrition description, RTF.Sample[] samples, string group_number, string pack_number, AnnotationPlane.Template.Property[] allProperties) {
            List<string> row = new List<string>();

            row.Add(group_number);
            row.Add(pack_number);
            row.Add(orderNum.ToString());
            row.Add(lower.ToString());
            row.Add(upper.ToString());

            foreach (AnnotationPlane.Template.Property propFromAllProps in allProperties) {
                RTF.PropertyDescription propDescr = description.Properties.FirstOrDefault(p => string.Equals(p.Name, propFromAllProps.Name, StringComparison.CurrentCulture));
                if (propDescr != null) {
                    ///if property is set for current layer
                    string entries = "";
                    if (propDescr.Values == null) {
                        row.Add("");
                        continue;
                    }
                    entries += string.Join(", ", propDescr.Values.Select(v => v.ToLower()));
                    if (!string.IsNullOrEmpty(propDescr.Comment)) {
                        entries += string.Format(" ({0})", propDescr.Comment);
                    }
                    row.Add(entries);
                }
                else {
                    ///property is not set for the layer
                    row.Add("");
                }
            }

            if (description.Properties.Count() == 0) {
                row.Add("");
                row.Add("");
            }

            return new CSV.ReportRow(row.ToArray());

        }

        /// <summary>
        /// Forms a table by filling it with supplied data
        /// </summary>
        /// <param name="intervals"></param>
        /// <param name="boundaries">including the "outer boundaries" of bounding layer</param>    
        /// <param name="layers"></param>
        /// <param name="isDepthIncreases">true if depth increases (other parameter arrays are sorted so the depth increases)</param>
        /// <param name="samples">All samples in the project</param>
        /// <returns></returns>
        public static CSV.ReportTable GenerateCVSTableContents(
            Intervals.BoreIntervalVM[] intervals,
            RTF.LayerBoundary[] boundaries,
            RTF.LayerDescrition[] layers,
            string[] rankNames,
            RTF.Sample[] samples,
            AnnotationDirection annotationDirection,
            AnnotationPlane.Template.Property[] allProperties,
            string[] genRankNames
            ) {
            List<KeyValuePair<double, int>> intBounds = new List<KeyValuePair<double, int>>();
            for (int i = 0; i < intervals.Length; i++) {
                Intervals.BoreIntervalVM interval = intervals[i];
                if (interval.LowerDepth < interval.UpperDepth)
                    throw new ArgumentException("The upper bound is lower than lower bound");
                intBounds.Add(new KeyValuePair<double, int>(interval.UpperDepth, 1));
                intBounds.Add(new KeyValuePair<double, int>(interval.LowerDepth, -1));
            }
            double[] keys = intBounds.Select(kvp => kvp.Key).ToArray();
            foreach (int key in keys) {
                int curKeyValsSum = intBounds.Where(kvp => kvp.Key == key).Sum(kvp => kvp.Value);
                if (curKeyValsSum == 0) {
                    intBounds = intBounds.Where(kvp => kvp.Key != key).ToList();
                }
            }

            int[] events = intBounds.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
            int sum = 0;
            for (int i = 0; i < events.Length; i++) {
                sum += events[i];
                if (Math.Abs(sum) > 1)
                    throw new InvalidOperationException("You've passed intersecting intervals");
            }


            List<CSV.ReportRow> rows = new List<CSV.ReportRow>();

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

            List<string> allColumnsList = new List<string>();

            allColumnsList.Add("№ " + genRankNames[0]);
            allColumnsList.Add("№ " + genRankNames[1]);
            allColumnsList.Add("№ " + genRankNames[2]);
            allColumnsList.Add("Верхняя граница слоя (м)");
            allColumnsList.Add("Нижняя граница слоя (м)");

            foreach (AnnotationPlane.Template.Property prop in allProperties) {
                allColumnsList.Add(string.IsNullOrEmpty(prop.Name) ? prop.Name : prop.ID);
            }

            CSV.ReportRow header = new CSV.ReportRow(allColumnsList.ToArray());
            rows.Add(header);

            int reportedInIndex;
            int inIndex;
            int laIndex;
            int layerOrderNum;
            string group_number = "";
            string pack_number = "";
            
            switch (annotationDirection) {
                case AnnotationDirection.UpToBottom:
                    intervals = intervals.OrderBy(i => i.UpperDepth).ToArray();
                    for (int i = 0; i < boundaries.Length - 1; i++)
                        if (boundaries[i + 1].Depth < boundaries[i].Depth)
                            throw new NotSupportedException("Boundary depths must increase");

                    reportedInIndex = -1;
                    inIndex = 0;
                    laIndex = 0;
                    layerOrderNum = 1;
                    while (inIndex < intervals.Length && laIndex < layers.Length) {
                        if (reportedInIndex != inIndex) {
                            reportedInIndex = inIndex;
                        }
                        RTF.LayerBoundary upperLabound = boundaries[laIndex];
                        double curLaUpper = upperLabound.Depth;
                        double curLaLower = boundaries[laIndex + 1].Depth;
                        double curIntUpper = intervals[inIndex].UpperDepth;
                        double curIntLower = intervals[inIndex].UpperDepth + intervals[inIndex].ExtractedLength;
                        bool doInInc = false;
                        bool doLaInc = false;
                        bool skipReporting = false;
                        
                        if (curIntLower >= curLaLower) {
                            if (curIntUpper > curLaLower) {
                                skipReporting = true;
                            }
                            else {
                                if (curIntUpper > curLaUpper) {
                                    curLaUpper = curIntUpper;
                                }
                            }
                            doLaInc = true;
                        }
                        else {
                            curLaLower = curIntLower;
                            if (curIntUpper > curLaUpper) {
                                curLaUpper = curIntUpper;
                            }

                            doInInc = true;
                        }

                        List<string> group_pack = GetGroupPackNumbersForUpperLaBound(upperLabound, laIndex, boundaries, curIntLower, rankNames);

                        switch (upperLabound.Rank) {
                            case 2:
                                group_number = group_pack[0];
                                pack_number = "1";
                                layerOrderNum = 1;
                                break;
                            case 1:
                                pack_number = group_pack[0];
                                layerOrderNum = 1;
                                break;
                            default:
                                break;
                        }

                        if (!skipReporting) {
                            var layerSamples = samples.Where(s => (s.Depth > curLaUpper) && (s.Depth < curLaLower)).ToArray();
                            rows.Add(GetLayerDescrRow(layerOrderNum++, curLaLower, curLaUpper, layers[laIndex], layerSamples, group_number, pack_number, allProperties));
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
                    while (inIndex >= 0 && laIndex >= 0) {
                        if (reportedInIndex != inIndex) {
                            reportedInIndex = inIndex;
                        }
                        RTF.LayerBoundary upperLabound = boundaries[laIndex];
                        RTF.LayerBoundary lowerLaBound = boundaries[laIndex + 1];
                        double curLaUpper = upperLabound.Depth;
                        double curLaLower = boundaries[laIndex + 1].Depth;
                        double curIntUpper = intervals[inIndex].UpperDepth;
                        double curIntLower = intervals[inIndex].UpperDepth + intervals[inIndex].ExtractedLength;
                        bool doInDec = false;
                        bool doLaDec = false;
                        bool skipReporting = false;
                        
                        if (curIntUpper <= curLaUpper) {
                            if (curIntLower < curLaUpper) {
                                skipReporting = true;
                            }
                            else {
                                if (curIntLower < curLaLower) {
                                    curLaLower = curIntLower;
                                }
                            }
                            doLaDec = true;
                        }
                        else {
                            curLaUpper = curIntUpper;
                            if (curIntLower < curLaLower) {
                                curLaLower = curIntLower;
                            }

                            doInDec = true;
                        }

                        if (!skipReporting) {
                            List<string> group_pack = GetGroupPackNumbersForLowerLaBound(lowerLaBound, laIndex, boundaries, curIntUpper, rankNames);

                            switch (lowerLaBound.Rank) {
                                case 2:
                                    group_number = group_pack[0];
                                    pack_number = group_pack[1];
                                    layerOrderNum = 1;
                                    break;
                                case 1:
                                    pack_number = group_pack[0];
                                    layerOrderNum = 1;
                                    break;
                                default:
                                    break;
                            }

                            var layerSamples = samples.Where(s => (s.Depth > curLaUpper) && (s.Depth < curLaLower)).ToArray();
                            rows.Add(GetLayerDescrRow(layerOrderNum++, curLaLower, curLaUpper, layers[laIndex], layerSamples, group_number, pack_number, allProperties));
                        }
                        if (doLaDec)
                            laIndex--;
                        if (doInDec)
                            inIndex--;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new CSV.ReportTable(rows.ToArray());
        }
    }

}