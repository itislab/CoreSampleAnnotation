using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports.CSV {
        public static class CSVReportHelpers {
            /// <summary>
            /// Generates the row depicting extraction interval
            /// </summary>
            /// <param name="upperDepth">in meters (positive)</param>
            /// <param name="lowerDepth">in meters (positive)</param>
            /// <param name="extractedLength">in meters (positive)</param>
            /// <returns></returns>
            public static CSV.ReportRow GetIntervalRow(double upperDepth, double lowerDepth, double extractedLength) {
                return new CSV.ReportRow(new String[] { string.Format("Интервал {0:0.#}-{1:0.##}/{2:0.##} м (присутствует {3:0.##} м)", upperDepth, lowerDepth, lowerDepth - upperDepth, extractedLength), "" });
            }

            /// <summary>
            /// Generates the row depicting group of layers with specific rank
            /// </summary>
            /// <param name="length">group total length (in meters)</param>
            /// <returns></returns>
            public static CSV.ReportRow GetRankDescrRow(int orderNumber, string rankName, double length) {
                return new CSV.ReportRow(new String[] { string.Format("{0} {1} ({2:0.## м})", orderNumber, rankName, length), "" });
            }

            /// <summary>
            /// Generates row deorcting specific layer
            /// </summary>        
            /// <param name="length">in meters</param>        
            /// <returns></returns>
            public static ReportRow GetLayerDescrRow(int orderNum, double lower, double upper, RTF.LayerDescrition description, RTF.Sample[] samples) {
                List<string> row = new List<string>();
                row.Add(string.Format("{0} слой", orderNum));
                row.Add(lower.ToString());
                row.Add(upper.ToString());

                /*StringBuilder samplesSb = new StringBuilder();
                List<string> sampleStrings = new List<string>();
                foreach (var sample in samples)
                    sampleStrings.Add(string.Format("{0}/{1:0.##} м; {2}", sample.ID, sample.Depth, sample.Purpose));*/

                foreach (RTF.PropertyDescription property in description.Properties) {
                    string entries = "";
                    if (property.Values == null)
                        continue;
                    entries += string.Join(", ", property.Values.Select(v => v.ToLower()));
                    if (!string.IsNullOrEmpty(property.Comment)) {
                        entries += string.Format(" ({0})", property.Comment);
                    }
                    row.Add(entries);
                }

            if (description.Properties.Count() == 0) {
                row.Add("");
                row.Add("");
            }

                return new CSV.ReportRow( row.ToArray() );

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
                bool isDepthIncreases) {
                //asserting intervals
                //lower - upper bounds order
                //intersection check
                List<KeyValuePair<double, int>> intBounds = new List<KeyValuePair<double, int>>();
                for (int i = 0; i < intervals.Length; i++) {
                    Intervals.BoreIntervalVM interval = intervals[i];
                    if (interval.LowerDepth < interval.UpperDepth)
                        throw new ArgumentException("The upper bound is lower than lower bound");
                    intBounds.Add(new KeyValuePair<double, int>(interval.UpperDepth, 1)); // +1 opens the interval
                    intBounds.Add(new KeyValuePair<double, int>(interval.LowerDepth, -1)); // -1 closes the interval
                }
                // detecting intervals with touching bounds; removing these bounds
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

                CSV.ReportRow header = new CSV.ReportRow(
                    new String[] { "№ слоя", "Верхняя граница слоя, м", "Нижняя граница слоя, м", "Органогенные включения", "Текстура" });

                rows.Add(header);

                //for now works with increasing depths order
                if (isDepthIncreases) {
                    intervals = intervals.OrderBy(i => i.UpperDepth).ToArray();
                    for (int i = 0; i < boundaries.Length - 1; i++)
                        if (boundaries[i + 1].Depth < boundaries[i].Depth)
                            throw new NotSupportedException("Boundary depths must increase");

                    int reportedInIndex = -1;
                    int inIndex = 0;
                    int laIndex = 0;
                    int layerOrderNum = 1;
                    while (inIndex < intervals.Length && laIndex < layers.Length) {
                        if (reportedInIndex != inIndex) {
                            //adding row depicting interval start
                            rows.Add(GetIntervalRow(intervals[inIndex].UpperDepth, intervals[inIndex].LowerDepth, intervals[inIndex].ExtractedLength));
                            reportedInIndex = inIndex;
                        }
                        RTF.LayerBoundary upperLabound = boundaries[laIndex];
                        double curLaUpper = upperLabound.Depth;
                        double curLaLower = boundaries[laIndex + 1].Depth; //boundraies array always contains one more element than layers
                        double curIntUpper = intervals[inIndex].UpperDepth;
                        double curIntLower = intervals[inIndex].UpperDepth + intervals[inIndex].ExtractedLength;
                        bool doInInc = false;
                        bool doLaInc = false;
                        bool skipReporting = false;

                        //analysing the relative position of the interval and the layer
                        if (curIntLower >= curLaLower) {
                            //the interval includes the end of the layer or interval is entirly below the layer
                            if (curIntUpper > curLaLower) {
                                //the entire interal is below the layer
                                skipReporting = true; //the layer is not reported
                            }
                            else {
                                if (curIntUpper > curLaUpper) {
                                    //coersing layer upper bound to match interval
                                    curLaUpper = curIntUpper;
                                }
                            }
                            doLaInc = true;
                        }
                        else {
                            //the layer includes the end of the interval
                            curLaLower = curIntLower; //coersing layer lower bound to match interval
                            if (curIntUpper > curLaUpper) {
                                //coersing layer upper bound to match interval
                                curLaUpper = curIntUpper;
                            }

                            doInInc = true;
                        }

                        if (!skipReporting) {
                            if (upperLabound.Rank > 0) {
                                //we need to add rank row
                                for (int rank = upperLabound.Rank; rank > 0; rank--) {
                                    double length = 0.0;
                                    //calculating total length. starting from cur bound till the next bound of the same rank or higher
                                    for (int i = laIndex + 1; i < boundaries.Length; i++) {
                                        int curRank = boundaries[i].Rank;
                                        length += boundaries[i].Depth - boundaries[i - 1].Depth;
                                        if (curRank >= rank)
                                            break;
                                    }
                                    int curOrder = 1;
                                    if (rank == upperLabound.Rank) //the outer (highest) rank is kept. All smaller ranks reset to 1
                                        curOrder = upperLabound.OrderNumber;
                                    length = Math.Min(length, curIntLower - upperLabound.Depth);
                                    rows.Add(GetRankDescrRow(curOrder, rankNames[rank], length));
                                }
                                layerOrderNum = 1; //ranks higher than 0 reset the numbering of layers
                            }
                            var layerSamples = samples.Where(s => (s.Depth > curLaUpper) && (s.Depth < curLaLower)).ToArray();
                            rows.Add(GetLayerDescrRow(layerOrderNum++, curLaLower, curLaUpper, layers[laIndex], layerSamples));
                        }
                        if (doLaInc)
                            laIndex++;
                        if (doInInc)
                            inIndex++;
                    }
                }

                return new CSV.ReportTable(rows.ToArray());
            }
        }

    }