using CoreSampleAnnotation.AnnotationPlane;
using CsvHelper;
using FileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports.SamplesCSV
{
    [DelimitedRecord(",")]
    public class SampleVMCSV
    {
        public int Number;
        public double Depth;

        public SampleVMCSV(int number, double depth)
        {
            Number = number;
            Depth = depth;
        }

        public SampleVMCSV() { }
    }

    public static class Report
    {
        public static string ClassToString(LayerClassVM lcVM)
        {
            if (lcVM.ShortName != null)
                return lcVM.ShortName;
            if (lcVM.Acronym != null)
                return lcVM.Acronym;
            return lcVM.ID;

        }

        public static void Generate(string fileName, SamplesColumnVM samplesCol, Intervals.BoreIntervalVM[] intervals, Layer[] layers, string[] propNames)
        {
            using (TextWriter textWriter = File.CreateText(fileName))
            {
                var csv = new CsvWriter(textWriter);

                //writing header
                csv.WriteField("Глубина (м)");
                csv.WriteField("Описание");
                csv.WriteField("Верх интервала (м)");
                csv.WriteField("Низ интервала (м)");
                for (int j = 0; j < propNames.Length; j++)
                    csv.WriteField(propNames[j]);
                csv.NextRecord();


                for (int i = 0; i < samplesCol.Samples.Length; i++)
                {
                    SampleVM sVM = samplesCol.Samples[i];

                    //searching for correspondence
                    Intervals.BoreIntervalVM containingInterval = null;
                    for (int j = 0; j < intervals.Length; j++)
                    {
                        var int1 = intervals[j];
                        if (sVM.Depth > int1.UpperDepth && sVM.Depth < int1.LowerDepth)
                        {
                            containingInterval = int1;
                            break;
                        }
                    }

                    Layer containingLayer = null;
                    for (int j = 0; j < layers.Length; j++)
                    {
                        Layer l = layers[j];
                        if (sVM.Depth > l.TopDepth && sVM.Depth < l.BottomDepth)
                        {
                            containingLayer = l;
                            break;
                        }
                    }

                    //fields to write
                    string depth = string.Format("{0:0.##}", sVM.Depth);
                    string name = string.Format("{0}", sVM.Comment);
                    string intTop = (containingInterval == null) ? "" : string.Format("{0:0.##}", containingInterval.UpperDepth);
                    string intBottom = (containingInterval == null) ? "" : string.Format("{0:0.##}", containingInterval.LowerDepth);
                    string[] props = null;
                    if (containingLayer == null)
                        props = Enumerable.Repeat("", propNames.Length).ToArray();
                    else
                    {
                        props = new string[propNames.Length];
                        for (int j = 0; j < containingLayer.Classifications.Length; j++)
                        {
                            ClassificationLayerVM clVM = containingLayer.Classifications[j];

                            SingleClassificationLayerVM sclVM = clVM as SingleClassificationLayerVM;
                            if (sclVM != null)
                            {
                                if (sclVM.CurrentClass != null)
                                {
                                    props[j] = ClassToString(sclVM.CurrentClass);
                                }
                            }
                            MultiClassificationLayerVM mclVM = clVM as MultiClassificationLayerVM;
                            if (mclVM != null)
                            {
                                if (mclVM.CurrentClasses != null)
                                {
                                    props[j] = string.Join(", ", mclVM.CurrentClasses.Select(c => ClassToString(c)).ToArray());
                                }
                            }
                        }
                    }

                    //writing fields
                    csv.WriteField(depth);
                    csv.WriteField(name);
                    csv.WriteField(intTop);
                    csv.WriteField(intBottom);
                    for (int j = 0; j < props.Length; j++)
                        csv.WriteField(props[j]);
                    csv.NextRecord();
                }
            }
        }
    }
}
