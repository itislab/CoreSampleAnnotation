using CoreSampleAnnotation.PhotoMarkup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoreSampleAnnotation.Intervals
{
    public class PhotoCalibratedBoreIntervalVM : BoreIntervalVM
    {
        /// <summary>
        /// A total length in meters of all annotated parts of the core sample
        /// </summary>
        public double AnnotatedLength
        {
            get { return imageRegions.Sum(arr => arr.Sum(r => r.Length)) * 0.01; }
        }

        public double AnnotatedPercentage
        {
            get
            {
                if (double.IsNaN(ExtractedLength) || double.IsNaN(AnnotatedLength) || (MaxPossibleExtractionLength == 0.0))
                    return 0.0;
                else
                    return AnnotatedLength / MaxPossibleExtractionLength * 100.0;
            }
        }

        private List<Transform> imageTransforms = new List<Transform>();
        private List<IEnumerable<CalibratedRegionVM>> imageRegions = new List<IEnumerable<CalibratedRegionVM>>();

        public Transform ImageTransform
        {
            get
            {
                if (imageTransforms.Count == 0)
                    return null;
                else
                    return imageTransforms[curImageIdx];
            }
            set
            {
                if (imageTransforms.Count > 0)
                {
                    if (imageTransforms[curImageIdx] != value)
                    {
                        imageTransforms[curImageIdx] = value;
                        RaisePropertyChanged(nameof(ImageTransform));
                    }
                }
            }
        }

        private bool isImageTransformTracked = true;
        /// <summary>
        /// Whether the image transforms happen due to user manipulation
        /// </summary>
        public bool IsImageTransformTracked {
            get { return isImageTransformTracked; }
            set {
                if (value != isImageTransformTracked) {
                    isImageTransformTracked = value;
                    RaisePropertyChanged(nameof(IsImageTransformTracked));
                }
            }
        }

        public IEnumerable<CalibratedRegionVM> CalibratedRegions
        {
            get
            {
                if (imageRegions.Count > 0)
                    return imageRegions[curImageIdx];
                else
                    return null;
            }
            set
            {
                if (imageRegions[curImageIdx] != value)
                {
                    //unsubscribing from old regions
                    if (imageRegions[curImageIdx] != null)
                    {
                        if (canMoveRegionChangedActivations.ContainsKey(curImageIdx))
                            canMoveRegionChangedActivations[curImageIdx].Clear();

                        foreach (CalibratedRegionVM crCM in imageRegions[curImageIdx])
                            crCM.PropertyChanged -= CalibratedRegion_PropertyChanged;
                    }

                    imageRegions[curImageIdx] = value;

                    //subscribing to new regions
                    if (value != null)
                    {
                        var canMoveUp = new Predicate<object>(obj1 =>
                        {
                            if (obj1 != null)
                            {
                                int order = (int)obj1;
                                return order > 1;
                            }
                            else
                                return false;
                        });

                        var canMoveDown = new Predicate<object>(obj1 =>
                        {
                            if (obj1 != null)
                            {
                                int order = (int)obj1;
                                return order < imageRegions.SelectMany(i => i).Count();
                            }
                            else
                                return false;
                        });
                        foreach (CalibratedRegionVM crCM in value)
                        {
                            if (crCM.Order == -1)
                            {
                                //not assigned yet, assigning
                                crCM.Order = imageRegions.SelectMany(i => i).Where(vm => vm.Order != -1).Count() + 1;
                            }

                            crCM.RemoveCommand = new DelegateCommand((obj) =>
                            {
                                int idx = (int)obj;

                                List<IEnumerable<CalibratedRegionVM>> newImageRegs = new List<IEnumerable<CalibratedRegionVM>>();
                                foreach (var imageRegs in imageRegions)
                                    newImageRegs.Add(imageRegs.Where(r => r.Order != idx).ToArray()); //excluding VM with order==idx
                                imageRegions = newImageRegs;

                                foreach (var region in imageRegions.SelectMany(i => i).Where(r => r.Order > idx))
                                    region.Order--;

                                RaisePropertyChanged(nameof(CalibratedRegions));
                            });

                            crCM.PropertyChanged += CalibratedRegion_PropertyChanged;

                            var delComUp = new DelegateCommand(obj1 => MoveRegionOrder((int)obj1, OrderMoveDirection.Up), canMoveUp);
                            var delComDown = new DelegateCommand(obj1 => MoveRegionOrder((int)obj1, OrderMoveDirection.Down), canMoveDown);
                            crCM.MoveUp = delComUp;
                            crCM.MoveDown = delComDown;

                            if (!canMoveRegionChangedActivations.ContainsKey(curImageIdx))
                                canMoveRegionChangedActivations[curImageIdx] = new List<Action>();
                            canMoveRegionChangedActivations[curImageIdx].Add(() => delComUp.RaiseCanExecuteChanged());
                            canMoveRegionChangedActivations[curImageIdx].Add(() => delComDown.RaiseCanExecuteChanged());
                        }
                    }

                    RaiseCanOrderMoveChanged();
                    RaisePropertyChanged(nameof(CalibratedRegions));
                    RaisePropertyChanged(nameof(ImagesButtonText));
                    RaisePropertyChanged(nameof(AnnotatedLength));
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                }
            }
        }

        private Dictionary<int, List<Action>> canMoveRegionChangedActivations = new Dictionary<int, List<Action>>();

        private void RaiseCanOrderMoveChanged()
        {
            foreach (var pair in canMoveRegionChangedActivations)
                foreach (Action act in pair.Value)
                    act();
        }

        public void MoveRegionOrder(int regionToMove, OrderMoveDirection direction)
        {
            int addition = (direction == OrderMoveDirection.Up) ? (-1) : 1;
            int toSwapWithIdx = regionToMove + addition;
            CalibratedRegionVM toSwapWith = null, target = null;
            foreach (CalibratedRegionVM vm in imageRegions.SelectMany(im => im))
            {
                if (vm.Order == regionToMove)
                {
                    target = vm;
                }
                else if (vm.Order == toSwapWithIdx)
                {
                    toSwapWith = vm;
                }
            }

            target.Order = toSwapWithIdx;
            toSwapWith.Order = regionToMove;
            System.Diagnostics.Debug.WriteLine("swapped {0} and {1}", toSwapWithIdx, regionToMove);
            RaiseCanOrderMoveChanged();
            target.RecalcMoveRelatedProps();
            toSwapWith.RecalcMoveRelatedProps();

        }

        public PhotoRegion[] GetRegions() {
            List<PhotoRegion> result = new List<PhotoRegion>();
            int N = ImagesCount;
            int counter = 0;
            for (int i = 0; i < N; i++) {
                var regions = imageRegions[i];
                var transform = imageTransforms[i];
                var backTransform = transform.Inverse;
                foreach (CalibratedRegionVM regVM in regions) {
                    Point up = backTransform.Transform(regVM.Up);
                    Point down = backTransform.Transform(regVM.Bottom);
                    Point side = backTransform.Transform(regVM.Side);
                    
                    Point M = Calc.FindNormalIntersection(down, up, side);
                    Vector sideV = side - M;
                    //todo: coerce side point side

                    double height = (up - down).Length;
                    double width = (sideV * 2.0).Length;

                    Vector downUp = up - down;
                    Vector downUpNorm = new Vector(downUp.Y, -downUp.X);

                    if (sideV * downUpNorm > 0.0)
                        sideV.Negate();
                    

                    TransformGroup tg = new TransformGroup();
                    
                    Point upperLeft = up - sideV;

                    tg.Children.Add(new TranslateTransform(-upperLeft.X,-upperLeft.Y));

                    if (sideV.X == 0)
                    {
                        throw new NotImplementedException();
                    }
                    else {
                        double angle = Vector.AngleBetween(sideV, new Vector(1.0, 0.0));
                        tg.Children.Add(new RotateTransform(angle));
                    }

                    Canvas canvas = new Canvas();
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(imagePaths[i]));

                    canvas.Children.Add(img);

                    img.RenderTransform = tg;

                    canvas.Measure(new Size(width, height));
                    canvas.Arrange(new Rect(new Size(width, height)));
                    
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);

                    bmp.Render(canvas);

                    var encoder = new PngBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(bmp));

                    MemoryStream stm = new MemoryStream();
                    using (Stream stm2 = File.Create(@"test"+(counter++)+".png"))
                        encoder.Save(stm2);

                    result.Add(new PhotoRegion(stm, new Size(width, height), regVM.Order, regVM.Order));
                }

                //transforming order into real depth
                var orederedRegions = result.OrderBy(r => r.LowerBound).ToList();
                double prevBound = UpperDepth;
                result = new List<PhotoRegion>();
                //for(int i=0;i< orederedRegions.Count;i+)
            }
            return result.ToArray();
        }

        private void CalibratedRegion_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CalibratedRegionVM vm = sender as CalibratedRegionVM;
            if (vm != null)
            {
                switch (e.PropertyName)
                {
                    case nameof(vm.Length):
                        RaisePropertyChanged(nameof(AnnotatedLength));
                        RaisePropertyChanged(nameof(AnnotatedPercentage));
                        break;
                    case nameof(vm.Order):
                        RaiseCanOrderMoveChanged();
                        vm.RecalcMoveRelatedProps();
                        break;
                }
            }
        }

        public string ImagesButtonText
        {
            get
            {
                int count = imagePaths.Count;
                string ending = "";
                int mod = count % 10;
                switch (mod)
                {
                    case 1: ending = "я"; break;
                    case 2:
                    case 3:
                    case 4: ending = "и"; break;
                    default: ending = "й"; break;

                }
                if (count > 0)
                    return string.Format("{0} фотографи{1}", count, ending);
                else
                    return "Добавьте фотографии";
            }
        }

        private int curImageIdx = 0;
        private List<string> imagePaths = new List<string>();

        public int ImagesCount
        {
            get { return imagePaths.Count; }
        }

        public int CurImageIdx
        {
            get { return curImageIdx; }
            set
            {
                if (curImageIdx != value)
                {
                    curImageIdx = value;
                    IsImageTransformTracked = false;
                    RaisePropertyChanged(nameof(CurImageIdx));
                    RaisePropertyChanged(nameof(ImagePath));
                    RaisePropertyChanged(nameof(ImageTransform));
                    RaisePropertyChanged(nameof(CalibratedRegions));
                    IsImageTransformTracked = true;
                }
            }
        }

        public string ImagePath
        {
            get
            {
                if (imagePaths.Count == 0) return null;
                else
                    return imagePaths[curImageIdx];
            }
        }

        private ICommand addNewImageCommand = null;

        private ICommand rotateCurrentImageCommand = null;

        //private ICommand removeCurrentImageCommand = null;

        public ICommand AddNewImageCommand
        {
            get { return addNewImageCommand; }
        }

        public ICommand NextImageCommand { private set; get; }

        public ICommand RotateCurrentImageCommand
        {
            get { return rotateCurrentImageCommand; }
        }

        public ICommand RemoveCurrentImageCommand { get; private set; }

        public PhotoCalibratedBoreIntervalVM()
        {
            base.PropertyChanged += PhotoCalibratedBoreIntervalVM_PropertyChanged;

            addNewImageCommand = new DelegateCommand(() =>
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.ShowDialog();
                string fileCandidate = ofd.FileName;
                if (System.IO.File.Exists(fileCandidate))
                {
                    imagePaths.Add(fileCandidate);
                    imageTransforms.Add(Transform.Identity);
                    imageRegions.Add(new List<CalibratedRegionVM>());

                    CurImageIdx = imagePaths.Count - 1;
                    RaisePropertyChanged(nameof(ImagesCount));
                    if (imagePaths.Count == 1)
                    {
                        RaisePropertyChanged(nameof(ImagePath));
                        RaisePropertyChanged(nameof(ImageTransform));
                        RaisePropertyChanged(nameof(CalibratedRegions));
                    }
                }
            });

            NextImageCommand = new DelegateCommand(() =>
            {
                if (imagePaths.Count > 0)
                {
                    CurImageIdx = (curImageIdx + 1) % imagePaths.Count;
                }
            });

            rotateCurrentImageCommand = new DelegateCommand(() =>
            {
                //Image img = new Image();
                //img.Source = new BitmapImage(new Uri(ImagePath));

                Matrix orig = ImageTransform.Value;
                Matrix additional = new RotateTransform(-90.0).Value;
                ImageTransform = new MatrixTransform(orig * additional);
            });

            RemoveCurrentImageCommand = new DelegateCommand(() =>
            {
                int idxToRemove = curImageIdx;
                imagePaths.RemoveAt(idxToRemove);
                imageTransforms.RemoveAt(idxToRemove);
                imageRegions.RemoveAt(idxToRemove);

                if (imagePaths.Count > 0)
                    CurImageIdx = (curImageIdx) % imagePaths.Count;

                RaisePropertyChanged(nameof(ImagePath));
                RaisePropertyChanged(nameof(ImageTransform));
                RaisePropertyChanged(nameof(CalibratedRegions));

                RaisePropertyChanged(nameof(ImagesCount));
            });
        }


        private void PhotoCalibratedBoreIntervalVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(base.LowerDepth):
                case nameof(base.UpperDepth):
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                    break;
            }
        }
    }
}
