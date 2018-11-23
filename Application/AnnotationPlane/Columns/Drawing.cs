using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoreSampleAnnotation.AnnotationPlane.Columns
{
    public static class Drawing
    {
        public static IEnumerable<Point> GetRightPolyline(double width, double height, ISideCurveGenerator rightSideCurve)
        {
            List<Point> result = new List<Point>();
            IEnumerable<Point> rightSidePoints = rightSideCurve.GenerateSide(height).
                Select(p => new Point(p.Y + width, p.X)); // transposing, so that (0.0;0.0);(length;0.0) projected to (width;0.0);(width;length)

            result.AddRange(rightSidePoints);

            return result;
        }

        public static IEnumerable<Point> GetBottomPolyline(double xOffset, double width, double height, ISideCurveGenerator bottomSideCurve)
        {
            List<Point> result = new List<Point>();

            IEnumerable<Point> bottomSidePoints = bottomSideCurve.GenerateSide(width).
                Select(p => new Point(p.X + xOffset, p.Y + height)); // parallel shift, so that (0.0;0.0);(width;0.0) is shifted to (0.0;height);(width;height)

            result.AddRange(bottomSidePoints);

            return result;
        }

        
        public static IEnumerable<Point> GetBackgroundPolyline(double width, double height, ISideCurveGenerator rightSideCurve)
        {
            List<Point> result = new List<Point>();
            result.Add(new Point(0.0, 0.0));
            result.Add(new Point(width, 0.0));

            IEnumerable < Point > rightSidePoints = rightSideCurve.GenerateSide(height).
             Select(p => new Point(p.Y + width, p.X)); // transposing, so that (0.0;0.0);(length;0.0) projected to (width;0.0);(width;length)
            
            result.AddRange(rightSidePoints);

            result.Add(new Point(width, height));
            result.Add(new Point(0.0, height));

            return result;
        }

    }

    public interface ISideCurveGenerator
    {
        /// <summary>
        /// Generates a set of points along horizontal (Y = 0) axis from X=0 up to X=length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        IEnumerable<Point> GenerateSide(double length);
    }

    public class StraightSideCurveGenerator : ISideCurveGenerator
    {
        public IEnumerable<Point> GenerateSide(double length)
        {
            return new Point[] { new Point(0.0, 0.0), new Point(length, 0.0) };
        }
    }

    /// <summary>
    /// Generates one period of the signal
    /// </summary>
    public interface IOscillationGenerator
    {
        IEnumerable<Point> GeneratePeriod(double xPeriod, double signalMaxY);
    }

    public enum OscillationAlignment { Left, Center, Right }

    /// <summary>
    /// Generates a side curve by repeating integer repeating periods provided by oscillationGenerator
    /// </summary>
    public class OscillatingSignalCurveGenerator : ISideCurveGenerator
    {
        private double xPeriod, signalMaxY;
        private OscillationAlignment alignment;
        IOscillationGenerator generator;

        /// <param name="xPeriod">The period of the pattern allong X axis</param>
        /// <param name="signalMaxY">the absolute hight of the pattern along Y axis</param>
        /// <param name="generator">Which signal to generate</param>
        /// <param name="alignment">Where the straight part will be / where to stick the signal curve</param>
        public OscillatingSignalCurveGenerator(double xPeriod, double signalMaxY, IOscillationGenerator generator, OscillationAlignment alignment)
        {
            if (signalMaxY < 0)
                throw new ArgumentException("signalMaxY must be non-negative");
            this.xPeriod = xPeriod;
            this.signalMaxY = signalMaxY;
            this.generator = generator;
            this.alignment = alignment;
        }

        public IEnumerable<Point> GenerateSide(double length)
        {
            int periods = (int)Math.Floor(length / xPeriod);
            double straitEndsLength = length - periods * xPeriod;

            List<Point> result = new List<Point>();

            double leftOffset = 0.0;
            double rightOffset = 0.0;
            switch (alignment)
            {
                case OscillationAlignment.Center:
                    leftOffset = straitEndsLength / 2;
                    rightOffset = straitEndsLength / 2;
                    break;
                case OscillationAlignment.Left:
                    leftOffset = 0.0;
                    rightOffset = straitEndsLength;
                    break;
                case OscillationAlignment.Right:
                    leftOffset = straitEndsLength;
                    rightOffset = 0.0;
                    break;
            }


            //starting straight line
            result.Add(new Point(0.0, 0.0));
            result.Add(new Point(leftOffset, 0.0));

            //drawing curves
            double miniStep = xPeriod * 0.5;
            for (int i = 0; i < periods; i++)
            {
                double xOffset = leftOffset + i * xPeriod;
                IEnumerable<Point> periodPoints = generator.GeneratePeriod(xPeriod, signalMaxY).Select(p => new Point(p.X + xOffset, p.Y));

                result.AddRange(periodPoints);
            }

            //ending straight line

            result.Add(new Point(length, 0.0));
            return result;
        }
    }

    public class ZigZagOscillationGenerator : IOscillationGenerator
    {
        public IEnumerable<Point> GeneratePeriod(double xPeriod, double signalMaxY)
        {
            double halfPeriod = xPeriod * 0.5;
            double quaterPeriod = halfPeriod * 0.5;
            List<Point> result = new List<Point>();
            result.Add(new Point(quaterPeriod, signalMaxY));
            result.Add(new Point(quaterPeriod + halfPeriod, -signalMaxY));            
            result.Add(new Point(xPeriod, 0.0));
            return result;
        }
    }

    public class StepOscillationGenerator : IOscillationGenerator
    {
        public IEnumerable<Point> GeneratePeriod(double xPeriod, double signalMaxY)
        {
            double halfPeriod = xPeriod * 0.5;
            List<Point> result = new List<Point>();
            result.Add(new Point(0.0, signalMaxY));
            result.Add(new Point(0.0 + halfPeriod, signalMaxY));
            result.Add(new Point(0.0 + halfPeriod, -signalMaxY));
            result.Add(new Point(0.0 + xPeriod, -signalMaxY));
            result.Add(new Point(0.0 + xPeriod, 0.0));
            return result;
        }
    }

    public class SinOscillationGenerator : IOscillationGenerator
    {
        private int points;
        /// <param name="approxPointsCount">How many points to use for sin approximation</param>
        public SinOscillationGenerator(int approxPointsCount)
        {
            if (approxPointsCount < 2)
                throw new ArgumentException("approxPointsCount must be 2 or higher");
            this.points = approxPointsCount;
        }

        public IEnumerable<Point> GeneratePeriod(double xPeriod, double signalMaxY)
        {
            double step = Math.PI * 2.0 / (points - 1);
            Point[] result = new Point[points - 1];

            double xScaled = xPeriod / (Math.PI * 2.0);

            for (int i = 0; i < points - 1; i++)
            {
                double Xi = step * (i + 1);
                result[i] = new Point(Xi * xScaled, Math.Sin(Xi) * signalMaxY);
            }
            return result;
        }
    }    
}
