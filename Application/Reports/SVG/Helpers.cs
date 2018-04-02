using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Reports.SVG
{
    public static class Helpers
    {
        public static SvgUnit dtos(double value) { return new SvgUnit((float)value); }

        public static SvgDocument SvgFromString(string str) {
            SvgDocument doc = SvgDocument.FromSvg<SvgDocument>(str);
            return doc;
        }
    }
}
