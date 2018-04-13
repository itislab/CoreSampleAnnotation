using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane.Template
{
    public enum RightSideFormEnum { NotDefined, Straight, Steps, Wave, ZigZag}

    /// <summary>
    /// A class that one property can take
    /// </summary>
    public class Class
    {
        public string ID { get; set; }

        public Color? Color { get; set; }

        public string IconSVG { get; set; }

        /// <summary>
        /// Крап
        /// </summary>
        public string BackgroundPatternSVG { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }

        public string Acronym { get; set; }

        /// <summary>
        /// 0.0 - 1.0
        /// </summary>
        public double WidthRatio { get; set; }

        public RightSideFormEnum RightSideForm { get; set; }

        public ImageSource ExampleImage { get; set; }

        public InputField[] InputFields { get; set; }
        
    }
}
