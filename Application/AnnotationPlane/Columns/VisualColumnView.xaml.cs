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
        private static ISideCurveGenerator wave = new OscillatingSignalCurveGenerator(20, 3, new SinOscillationGenerator(10));
        private static ISideCurveGenerator zigzag = new OscillatingSignalCurveGenerator(20, 3, new ZigZagOscillationGenerator());


        public static ISideCurveGenerator GetGeneratorFor(Template.RightSideFormEnum rightSideForm) {
            switch (rightSideForm) {
                case Template.RightSideFormEnum.NotDefined: return straight;
                case Template.RightSideFormEnum.Straight: return straight;
                case Template.RightSideFormEnum.Steps: return steps;
                case Template.RightSideFormEnum.Wave: return wave;
                case Template.RightSideFormEnum.ZigZag: return zigzag;
                default:
                    throw new NotSupportedException("Unknown right side form");
            }
        }

        public static ISideCurveGenerator GetGeneratorFor(Template.BottomSideFormEnum bottomSideForm)
        {
            switch (bottomSideForm)
            {
                case Template.BottomSideFormEnum.NotDefined: return straight;
                case Template.BottomSideFormEnum.Straight: return straight;
                case Template.BottomSideFormEnum.Wave: return wave;
                case Template.BottomSideFormEnum.Dotted: return straight;
                default:
                    throw new NotSupportedException("Unknown bottom side form");
            }
        }
    }

    public class RightSidePointsConverter : IMultiValueConverter
    {        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return null;
            double width = (double)values[0];
            double height = (double)values[1];
            Template.RightSideFormEnum rightSideForm = (Template.RightSideFormEnum)values[2];
            ISideCurveGenerator rightSideGenerator = SideCurveGeneratorFactory.GetGeneratorFor(rightSideForm);
            return new PointCollection(Drawing.GetRightPolyline(width, height, rightSideGenerator));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BackgroundPolygonPointsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return null;
            double width = (double)values[0];
            double height = (double)values[1];
            Template.RightSideFormEnum rightSideForm = (Template.RightSideFormEnum)values[2];
            ISideCurveGenerator rightSideGenerator = SideCurveGeneratorFactory.GetGeneratorFor(rightSideForm);
            return new PointCollection(Drawing.GetBackgroundPolyline(width, height, rightSideGenerator));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BottomSidePointsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return null;
            double width = (double)values[0];
            double height = (double)values[1];
            Template.BottomSideFormEnum bottomSideForm = (Template.BottomSideFormEnum)values[2];
            ISideCurveGenerator bottomSideGenerator = SideCurveGeneratorFactory.GetGeneratorFor(bottomSideForm);
            return new PointCollection(Drawing.GetBottomPolyline(width, height, bottomSideGenerator));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BottomSideStrokeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Template.BottomSideFormEnum bottomSideForm = (Template.BottomSideFormEnum)value;
            DoubleCollection strokeArr = new DoubleCollection() {};
            switch (bottomSideForm)
            {
                case Template.BottomSideFormEnum.NotDefined:
                case Template.BottomSideFormEnum.Straight:
                case Template.BottomSideFormEnum.Wave:
                    strokeArr = new DoubleCollection() { 3, 0 };
                    break;
                case Template.BottomSideFormEnum.Dotted:
                    strokeArr = new DoubleCollection() { 3, 3 };
                    break;
                default:
                    break;
            }
            return strokeArr;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
