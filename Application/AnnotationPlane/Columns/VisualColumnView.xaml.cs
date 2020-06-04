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
        private static ISideCurveGenerator stepsVertical = new OscillatingSignalCurveGenerator(20, 3, new StepOscillationGenerator(), OscillationAlignment.Center);
        private static ISideCurveGenerator waveVertical = new OscillatingSignalCurveGenerator(20, 3, new SinOscillationGenerator(11), OscillationAlignment.Center);
        private static ISideCurveGenerator zigzagVertical = new OscillatingSignalCurveGenerator(20, 3, new ZigZagOscillationGenerator(), OscillationAlignment.Center);
        private static ISideCurveGenerator waveHorizontalLeft = new OscillatingSignalCurveGenerator(20, 3, new SinOscillationGenerator(11), OscillationAlignment.Left);
        private static ISideCurveGenerator waveHorizontalRight = new OscillatingSignalCurveGenerator(20, 3, new SinOscillationGenerator(11), OscillationAlignment.Right);
        private static ISideCurveGenerator waveHorizontalCenter = new OscillatingSignalCurveGenerator(20, 3, new SinOscillationGenerator(11), OscillationAlignment.Center);


        public static ISideCurveGenerator GetGeneratorFor(Template.RightSideFormEnum rightSideForm) {
            switch (rightSideForm) {
                case Template.RightSideFormEnum.NotDefined: return straight;
                case Template.RightSideFormEnum.Straight: return straight;
                case Template.RightSideFormEnum.Steps: return stepsVertical;
                case Template.RightSideFormEnum.Wave: return waveVertical;
                case Template.RightSideFormEnum.ZigZag: return zigzagVertical;
                default:
                    throw new NotSupportedException("Unknown right side form");
            }
        }

        public static ISideCurveGenerator GetGeneratorFor(Template.BottomSideFormEnum bottomSideForm, OscillationAlignment alignment)
        {
            switch (bottomSideForm)
            {
                case Template.BottomSideFormEnum.NotDefined: return straight;
                case Template.BottomSideFormEnum.Straight: return straight;
                case Template.BottomSideFormEnum.Wave:
                    switch (alignment)
                    {
                        case OscillationAlignment.Left: return waveHorizontalLeft;
                        case OscillationAlignment.Right: return waveHorizontalRight;
                        case OscillationAlignment.Center: return waveHorizontalCenter;
                        default:
                            throw new NotSupportedException("Unknown wave alignment");
                    }
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
            if (values[0] == DependencyProperty.UnsetValue) return new PointCollection();
            Template.BottomSideFormEnum bottomSideForm = (Template.BottomSideFormEnum)values[0];
            ISideCurveGenerator bottomRightAlignedSideGenerator = SideCurveGeneratorFactory.GetGeneratorFor(bottomSideForm, OscillationAlignment.Right);
            ISideCurveGenerator bottomLeftAlignedSideGenerator = SideCurveGeneratorFactory.GetGeneratorFor(bottomSideForm, OscillationAlignment.Left);
            double currentWidth = (double)values[1];
            double previousWidth = (double)values[2];
            // splitting the layer boundary into two fragments
            // getting the minimum length between the two layers
            double minCrepWidth = (currentWidth < previousWidth) ? currentWidth : previousWidth;
            // getting the difference between the lengths
            double crepWidthDiff = (currentWidth > previousWidth) ? currentWidth - minCrepWidth : previousWidth - minCrepWidth;
            var bottomBoundaryRightAlign = Drawing.GetBottomPolyline(0.0, minCrepWidth, 0.0, bottomRightAlignedSideGenerator);
            var bottomBoundaryLeftAlign = Drawing.GetBottomPolyline(minCrepWidth, crepWidthDiff, 0.0, bottomLeftAlignedSideGenerator);
            var bottomBoundary = new PointCollection(bottomBoundaryRightAlign.Concat(bottomBoundaryLeftAlign));
            return bottomBoundary;
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
                    strokeArr = new DoubleCollection() { 5, 0 };
                    break;
                case Template.BottomSideFormEnum.Dotted:
                    strokeArr = new DoubleCollection() { 5, 5 };
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
