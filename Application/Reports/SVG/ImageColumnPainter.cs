using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CoreSampleAnnotation.AnnotationPlane;
using System.IO;
using System.Windows.Media.Imaging;

namespace CoreSampleAnnotation.Reports.SVG
{
    public class ImageColumnPainter : ColumnPainter
    {
        protected new ImageColumnVM vm;

        public ImageColumnPainter(UIElement headerView, ColumnView view, ImageColumnVM vm) : base(headerView, view, vm)
        {
            this.vm = vm as ImageColumnVM;            
        }

        public override RenderedSvg RenderColumn()
        {
            RenderedSvg result = base.RenderColumn();

            Svg.SvgGroup group = new Svg.SvgGroup();

            foreach(Intervals.PhotoRegion region in vm.ImageRegions) {
                Svg.SvgImage image = new Svg.SvgImage();

                string base64;

                using (MemoryStream ms = new MemoryStream())
                {
                    WriteableBitmap wbm = new WriteableBitmap(region.BitmapImage);

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(wbm));
                    encoder.Save(ms);

                    byte[] bytes = ms.ToArray();
                    base64 = Convert.ToBase64String(bytes);
                }

                image.Href = string.Format("data:image/png;base64,{0}",base64);
                image.Width = Helpers.dtos(region.WpfWidth);

                Size imSize = region.ImageSize;

                //how many WPF units in one image unit
                double wpfScaleFactor = region.WpfWidth / imSize.Width;
                image.Height = Helpers.dtos(imSize.Height * wpfScaleFactor);

                //home many real meters in one image unit
                double depthScaleFactor = (region.ImageLowerDepth - region.ImageUpperDepth) / imSize.Height;

                image.Y = Helpers.dtos((region.ImageUpperDepth - vm.UpperBound) / depthScaleFactor * wpfScaleFactor);
                group.Children.Add(image);
            }
            result.SVG = group;
            return result;
        }
    }
}
