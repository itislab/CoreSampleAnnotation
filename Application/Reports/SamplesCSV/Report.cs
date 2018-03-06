using CoreSampleAnnotation.AnnotationPlane;
using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports.SamplesCSV {
    [DelimitedRecord(",")]
    public class SampleVMCSV {
        public int Number;
        public double Depth;

        public SampleVMCSV(int number, double depth) {
            Number = number;
            Depth = depth;
        }

        public SampleVMCSV() { }
    }

    public static class Report {
        public static void Generate(string fileName, SamplesColumnVM samplesCol) {
            var engine = new FileHelperEngine<SampleVMCSV>();
            var SampleVMCSVs = new List<SampleVMCSV>();
            engine.WriteFile(fileName, samplesCol.Samples.Select(x => new SampleVMCSV(x.Number, x.Depth)));
        }
    }
}
