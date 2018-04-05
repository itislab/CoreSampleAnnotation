using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports.CSV {
    /// <summary>
    /// Represents a single row
    /// </summary>
    public class ReportRow {
        public String[] Cells { get; private set; }
        public ReportRow(String[] cells) {
            this.Cells = cells;
        }
    }

    /// <summary>
    /// Table as an array of row
    /// </summary>
    public class ReportTable {
        public ReportRow[] Rows { get; private set; }

        public ReportTable(ReportRow[] rows) {
            Rows = rows;
        }
    }

    public class ReportCSV {
        public static void Generate(string fileName, ReportTable table) {
            using (var writer = new System.IO.StreamWriter(fileName))
            using (var csv = new CsvHelper.CsvWriter(writer)) {
                csv.Configuration.Delimiter = ",";
                csv.Configuration.QuoteAllFields = true;
                foreach (ReportRow row in table.Rows) {
                    foreach(string s in row.Cells)
                        csv.WriteField(s);
                    csv.NextRecord();
                }
            }
        }

        /*public void SaveToCSVFile(string fileName, RTF.LayerDescrition[] layers) {
            using (var writer = new System.IO.StreamWriter(fileName))
            using (var csv = new CsvHelper.CsvWriter(writer)) {
                    csv.WriteRecords(layers);
            }
        }*/
    }
}
