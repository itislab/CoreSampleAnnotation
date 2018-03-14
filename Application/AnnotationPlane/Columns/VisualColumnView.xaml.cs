using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoreSampleAnnotation.AnnotationPlane.Columns
{

    public class ViewPortConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (ImageSource)value;
            return new Rect(0, 0, source.Width, source.Height);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public static class SideCurveGeneratorFactory
    {
        private static ISideCurveGenerator straight = new StraightSideCurveGenerator();
        private static ISideCurveGenerator steps = new OscillatingSignalCurveGenerator(20, 3, new StepOscillationGenerator());
        private static ISideCurveGenerator wave = new OscillatingSignalCurveGenerator(20,3,new SinOscillationGenerator(10));


        public static ISideCurveGenerator GetGeneratorFor(Template.RightSideFormEnum rightSideForm) {
            switch (rightSideForm) {
                case Template.RightSideFormEnum.Straight: return straight;
                case Template.RightSideFormEnum.Steps: return steps;
                case Template.RightSideFormEnum.Wave: return wave;
                default:
                    throw new NotSupportedException("Unknown right side form");
            }
        }
    }

    public class PolygonPointsConverter : IMultiValueConverter
    {        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return null;
            double width = (double)values[0];
            double height = (double)values[1];
            Template.RightSideFormEnum rightSideForm = (Template.RightSideFormEnum)values[2];
            ISideCurveGenerator rightSideGenerator = SideCurveGeneratorFactory.GetGeneratorFor(rightSideForm);
            return new PointCollection(Drawing.GetPolygon(width,height, rightSideGenerator));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for VisualColumnView.xaml
    /// </summary>
    public partial class VisualColumnView : UserControl
    {
        public VisualColumnView()
        {
            InitializeComponent();
        }
    }
}
