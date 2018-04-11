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

        private static List<int> GetGroupPackNumbersForLowerLaBound(RTF.LayerBoundary lowerLaBound, int laIndex, RTF.LayerBoundary[] boundaries, double curIntUpper, string[] rankNames) {
            List<int> rrl = new List<int>();
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
                            rrl.Add(lowerLaBound.OrderNumbers[2] + 1);
                            break;
                        case 1:
                            //rrl.Add(GetRankDescrRow(lowerLaBound.OrderNumbers[0] + 1, rankNames[1], length));
                            rrl.Add(lowerLaBound.OrderNumbers[0] + 1);
                            break;
                        default:
                            break;
                    }
                }
                else {
                    rrl.Add(1);
                }
            }

            return rrl;
        }


        /// <summary>
        /// Generates row deorcting specific layer
        /// </summary>                    
        /// <returns></returns>
        public static ReportRow GetLayerDescrRow(double lower, double upper, RTF.LayerDescrition description, int[] layerNumbers, AnnotationPlane.Template.Property[] allProperties) {
            List<string> row = new List<string>();

            for (int i = layerNumbers.Length-1; i >= 0; i--)            
                row.Add(layerNumbers[i].ToString());
                        
            row.Add(Math.Round(upper, 4).ToString());
            row.Add(Math.Round(lower, 4).ToString());

            foreach (AnnotationPlane.Template.Property propFromAllProps in allProperties) {
                RTF.PropertyDescription propDescr = description.Properties.FirstOrDefault(p => string.Equals(p.Name, propFromAllProps.Name, StringComparison.CurrentCulture));
                if (propDescr != null) {
                    string entries = "";
                    if (propDescr.Values == null) {
                        row.Add("");
                        continue;
                    }
                    ///if property is set for current layer
                    entries += string.Join(", ", propDescr.Values.Select(v => v.ToLower()));
                    if (!string.IsNullOrEmpty(propDescr.Comment)) {
                        entries += string.Format(". {0}", propDescr.Comment);
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
            }

            return new CSV.ReportRow(row.ToArray());

        }

        /// <summary>
        /// Forms a table by filling it with supplied data
        /// </summary>
        /// <param name="boundaries">including the "outer boundaries" of bounding layer</param>    
        /// <param name="layers"></param>
        /// <param name="isDepthIncreases">true if depth increases (other parameter arrays are sorted so the depth increases)</param>        
        /// <returns></returns>
        public static CSV.ReportTable GenerateCVSTableContents(            
            RTF.LayerBoundary[] boundaries,
            RTF.LayerDescrition[] layers,            
            AnnotationDirection annotationDirection,
            AnnotationPlane.Template.Property[] allProperties,
            string[] genRankNames
            ) {                        
            
            List<CSV.ReportRow> rows = new List<CSV.ReportRow>();

            if (boundaries.Length - 1 != layers.Length)
                throw new ArgumentException("Layers count must be exactly one more than boundary count");            

            List<string> allColumnsList = new List<string>();

            for (int i = genRankNames.Length-1; i >=0; i--)
            {
                allColumnsList.Add(string.Format("№ {0}",genRankNames[i]));
            }
            allColumnsList.Add("Верхняя граница слоя (м)");
            allColumnsList.Add("Нижняя граница слоя (м)");

            foreach (AnnotationPlane.Template.Property prop in allProperties) {
                allColumnsList.Add(string.IsNullOrEmpty(prop.Name) ? prop.Name : prop.ID);
            }

            CSV.ReportRow header = new CSV.ReportRow(allColumnsList.ToArray());
            rows.Add(header);
            
            int[] ranksNumbers = Enumerable.Repeat(1, genRankNames.Length).ToArray();
            
            switch (annotationDirection) {
                case AnnotationDirection.UpToBottom:
                    for (int i = 0; i < layers.Length; i++)
                    {
                        for (int j = 0; j < boundaries[i].OrderNumbers.Length; j++) {
                            ranksNumbers[j] = boundaries[i].OrderNumbers[j];
                        }
                        rows.Add(GetLayerDescrRow(boundaries[i+1].Depth, boundaries[i].Depth, layers[i], ranksNumbers, allProperties));
                    }
                            
                      
                    break;
                case AnnotationDirection.BottomToUp:
                    for (int i = layers.Length - 1; i >= 0 ; i--)
                    {
                        rows.Add(GetLayerDescrRow(boundaries[i + 1].Depth, boundaries[i].Depth, layers[i], ranksNumbers, allProperties));
                        ranksNumbers[boundaries[i].OrderNumbers.Length - 1] = boundaries[i].OrderNumbers[boundaries[i].OrderNumbers.Length - 1] + 1;
                        for (int j = 0; j < boundaries[i].OrderNumbers.Length - 1; j++)
                        {
                            ranksNumbers[j] = 1;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new CSV.ReportTable(rows.ToArray());
        }
    }

}