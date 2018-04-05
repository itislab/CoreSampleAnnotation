using CoreSampleAnnotation.PhotoMarkup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoreSampleAnnotation.Intervals
{
    public interface IImageStorage
    {
        /// <summary>
        /// Returns the absolute path that can be used to access the image file
        /// </summary>
        /// <param name="imageID"></param>
        /// <returns></returns>
        string GetFilePath(Guid imageID);
        /// <summary>
        /// Returns the idx of newly added image
        /// </summary>
        /// <returns></returns>
        Guid AddNewImage();

        /// <summary>
        /// Removes the image from storage
        /// </summary>
        /// <param name="id"></param>
        void RemoveImage(Guid id);
    }

    [Serializable]
    public class PhotoCalibratedBoreIntervalVM : BoreIntervalVM, ISerializable
    {
        /// <summary>
        /// A total length in meters of all annotated parts of the core sample
        /// </summary>
        public double AnnotatedLength
        {
            get { return imageRegions.Sum(arr => arr.Select(r => r.Length).Where(l => !double.IsNaN(l)).Sum()); }
        }

        /// <summary>
        /// How much of extracted interval is annotated
        /// </summary>
        public double AnnotatedPercentage
        {
            get
            {
                if (double.IsNaN(ExtractedLength) || double.IsNaN(AnnotatedLength) || (MaxPossibleExtractionLength == 0.0))
                    return 0.0;
                else
                    return AnnotatedLength / ExtractedLength * 100.0;
            }
        }

        private IImageStorage imageStorage;

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
        public bool IsImageTransformTracked
        {
            get { return isImageTransformTracked; }
            set
            {
                if (value != isImageTransformTracked)
                {
                    isImageTransformTracked = value;
                    RaisePropertyChanged(nameof(IsImageTransformTracked));
                }
            }
        }

        private Predicate<object> canMoveUp;
        private Predicate<object> canMoveDown;
        private DelegateCommand delComUp, delComDown, focusToNext;

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
                        foreach (CalibratedRegionVM crCM in value)
                        {
                            if (crCM.Order == -1)
                            {
                                //not assigned yet, assigning
                                crCM.Order = imageRegions.SelectMany(i => i).Where(vm => vm.Order != -1).Count() + 1;
                            }

                            AttachHandlersToRegionVM(crCM);

                            if (!canMoveRegionChangedActivations.ContainsKey(curImageIdx))
                                canMoveRegionChangedActivations[curImageIdx] = new List<Action>();
                            canMoveRegionChangedActivations[curImageIdx].Add(() => delComUp.RaiseCanExecuteChanged());
                            canMoveRegionChangedActivations[curImageIdx].Add(() => delComDown.RaiseCanExecuteChanged());
                        }
                    }

                    RecalculateRegionProperties();
                    RaisePropertyChanged(nameof(CalibratedRegions));
                    RaisePropertyChanged(nameof(ImagesButtonText));
                    RaisePropertyChanged(nameof(AnnotatedLength));
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                }
            }
        }

        /// <summary>
        /// Safely removes the calibrated regions by recalulating indices of other regions
        /// </summary>
        /// <param name="regionIdx"></param>
        private void RemoveRegionByIndex(int regionIdx)
        {
            List<IEnumerable<CalibratedRegionVM>> newImageRegs = new List<IEnumerable<CalibratedRegionVM>>();
            foreach (var imageRegs in imageRegions)
                newImageRegs.Add(imageRegs.Where(r => r.Order != regionIdx).ToArray()); //excluding VM with order==idx
            imageRegions = newImageRegs;

            foreach (var region in imageRegions.SelectMany(i => i).Where(r => r.Order > regionIdx))
                region.Order--;

            RaisePropertyChanged(nameof(CalibratedRegions));
        }

        private void AttachHandlersToRegionVM(CalibratedRegionVM crCM)
        {
            crCM.RemoveCommand = new DelegateCommand((obj) =>
            {
                int idx = (int)obj;

                var mbResult = MessageBox.Show("Удалить отмеченный участок керна?", "Подтверждение удаления участка", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (mbResult)
                {
                    case MessageBoxResult.Yes:
                        RemoveRegionByIndex(idx);
                        break;
                    case MessageBoxResult.No:
                        break;
                    default: throw new NotImplementedException();
                }


            });
            
            crCM.PropertyChanged += CalibratedRegion_PropertyChanged;

            crCM.MoveUp = delComUp;
            crCM.MoveDown = delComDown;
            crCM.FocusToNextCommand = focusToNext;
        }

        private Dictionary<int, List<Action>> canMoveRegionChangedActivations = new Dictionary<int, List<Action>>();

        private void RecalculateRegionProperties()
        {
            //recalculating depths
            double depth = UpperDepth;
            var regions = imageRegions.SelectMany(ir => ir).OrderBy(r => r.Order).ToArray();
            for (int i = 0; i < regions.Length; i++)
            {
                var region = regions[i];
                region.UpperDepth = depth;
                double len = region.Length;
                if (double.IsNaN(len))
                    len = 0.0;
                depth += len;
                region.LowerDepth = depth;
            }
            
            //firing that as the order could change, the can move up/down must be recalulated
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
            RecalculateRegionProperties();
            target.RaiseCanMoveChanged();
            toSwapWith.RaiseCanMoveChanged();

        }

        public PhotoRegion[] GetRegionImages()
        {
            List<PhotoRegion> gatheredRegions = new List<PhotoRegion>();
            int N = ImagesCount;            
            for (int i = 0; i < N; i++)
            {                
                var regions = imageRegions[i];
                var transform = imageTransforms[i];
                var backTransform = transform.Inverse;
                foreach (CalibratedRegionVM regVM in regions)
                {
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

                    tg.Children.Add(new TranslateTransform(-upperLeft.X, -upperLeft.Y));

                    if (sideV.X == 0)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        double angle = Vector.AngleBetween(sideV, new Vector(1.0, 0.0));
                        tg.Children.Add(new RotateTransform(angle));
                    }

                    Canvas canvas = new Canvas();
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(imageStorage.GetFilePath(imageIDs[i])));

                    canvas.Children.Add(img);

                    img.RenderTransform = tg;

                    canvas.Measure(new Size(width, height));
                    canvas.Arrange(new Rect(new Size(width, height)));

                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);

                    bmp.Render(canvas);

                    var encoder = new PngBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(bmp));

                    var bitmap = new BitmapImage();

                    using (MemoryStream stm = new MemoryStream())
                    {
                        encoder.Save(stm);
                        stm.Seek(0, SeekOrigin.Begin);

                        bitmap.BeginInit();
                        bitmap.StreamSource = stm;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    gatheredRegions.Add(new PhotoRegion(bitmap, new Size(width, height), regVM.Order, regVM.Length )); //for now upper and lower are just placeholders holding order and length
                }
            }

            List<PhotoRegion> result = new List<PhotoRegion>();

            //transforming order into real depth
            var reorderedRegions = gatheredRegions.OrderBy(r => r.ImageUpperDepth).ToArray(); //ordering by order
            double prevBound = UpperDepth;            
            for (int j = 0; j < reorderedRegions.Length; j++)
            {
                PhotoRegion curReg = reorderedRegions[j];
                result.Add(new PhotoRegion(curReg.BitmapImage, curReg.ImageSize, prevBound, prevBound + curReg.ImageLowerDepth));
                prevBound = prevBound + curReg.ImageLowerDepth;
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
                        RecalculateRegionProperties();                        
                        break;
                }
            }
        }

        public string ImagesButtonText
        {
            get
            {
                int count = imageIDs.Count;
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
        private List<Guid> imageIDs = new List<Guid>();

        public int ImagesCount
        {
            get { return imageIDs.Count; }
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
                    RaisePropertyChanged(nameof(ImageSource));
                    RaisePropertyChanged(nameof(ImageTransform));
                    RaisePropertyChanged(nameof(CalibratedRegions));
                    IsImageTransformTracked = true;
                }
            }
        }

        public ImageSource ImageSource
        {
            get
            {
                if (imageIDs.Count == 0) return null;
                else
                {
                    Uri uri = new Uri(ImagePath);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
        }

        public string ImagePath
        {
            get
            {
                if (imageIDs.Count == 0) return null;
                else
                {
                    return Path.GetFullPath(imageStorage.GetFilePath(imageIDs[curImageIdx]));

                }
            }
        }

        private ICommand addNewImageCommand = null;

        public ICommand AddNewImageCommand
        {
            get { return addNewImageCommand; }
        }

        public ICommand NextImageCommand { private set; get; }

        private CalibratedRegionVM focusedRegion = null;
        public CalibratedRegionVM FocusedRegion {
            get {
                return focusedRegion;
            }
            set {
                if (focusedRegion != value)
                {
                    focusedRegion = value;
                    RaisePropertyChanged(nameof(FocusedRegion));
                }
            }
        }
        
        public ICommand RotateCurrentImageCommand
        {
            get; private set;
        }

        public ICommand RemoveCurrentImageCommand { get; private set; }

        private void Initialize()
        {
            base.PropertyChanged += PhotoCalibratedBoreIntervalVM_PropertyChanged;

            canMoveUp = new Predicate<object>(obj1 =>
            {
                if (obj1 != null)
                {
                    int order = (int)obj1;
                    return order > 1;
                }
                else
                    return false;
            });

            canMoveDown = new Predicate<object>(obj1 =>
            {
                if (obj1 != null)
                {
                    int order = (int)obj1;
                    return order < imageRegions.SelectMany(i => i).Count();
                }
                else
                    return false;
            });

            delComUp = new DelegateCommand(obj1 => MoveRegionOrder((int)obj1, OrderMoveDirection.Up), canMoveUp);
            delComDown = new DelegateCommand(obj1 => MoveRegionOrder((int)obj1, OrderMoveDirection.Down), canMoveDown);
            focusToNext = new DelegateCommand((obj1) => {
                if (FocusedRegion != null) {
                    var array = CalibratedRegions.ToArray();
                    int idx = -1;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] == FocusedRegion) {
                            idx = i;
                            break;
                        }
                    }
                    if (idx != -1) {
                        idx = (idx + 1) % array.Length;
                        FocusedRegion = array[idx];
                    }
                }
            });

            addNewImageCommand = new DelegateCommand(() =>
            {
                Guid id = imageStorage.AddNewImage();
                string fileCandidate = imageStorage.GetFilePath(id);

                if (System.IO.File.Exists(fileCandidate))
                {
                    imageIDs.Add(id);
                    imageTransforms.Add(Transform.Identity);
                    imageRegions.Add(new List<CalibratedRegionVM>());

                    CurImageIdx = imageIDs.Count - 1;
                    RaisePropertyChanged(nameof(ImagesCount));
                    if (imageIDs.Count == 1)
                    {
                        RaisePropertyChanged(nameof(ImagePath));
                        RaisePropertyChanged(nameof(ImageSource));
                        RaisePropertyChanged(nameof(ImageTransform));
                        RaisePropertyChanged(nameof(CalibratedRegions));
                    }
                }
            });

            NextImageCommand = new DelegateCommand(() =>
            {
                if (imageIDs.Count > 0)
                {
                    CurImageIdx = (curImageIdx + 1) % imageIDs.Count;

                    FocusedRegion = null;
                }
            });

            RotateCurrentImageCommand = new DelegateCommand(() =>
            {                
                BitmapImage bi = new BitmapImage(new Uri(ImagePath), new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore));                

                Transform prevTransform = ImageTransform;

                Point imageCentre = new Point(bi.Width * 0.5, bi.Height * 0.5);

                Point movedCentre = prevTransform.Transform(imageCentre);

                Matrix orig = ImageTransform.Value;
                Matrix additional = new RotateTransform(90.0, movedCentre.X, movedCentre.Y).Value;
                ImageTransform = new MatrixTransform(orig * additional);
            });

            RemoveCurrentImageCommand = new DelegateCommand(() =>
            {
                var mbResult = MessageBox.Show("Удалить текущую фотографию из проекта? Все отмеченные на ней интервалы будут потеряны.", "Подтверждение удаления фотографии", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (mbResult)
                {
                    case MessageBoxResult.Yes:
                        int idxToRemove = curImageIdx;

                        var relatedRegions = imageRegions[idxToRemove];
                        foreach (var region in relatedRegions)
                        {
                            RemoveRegionByIndex(region.Order);
                        }

                        imageStorage.RemoveImage(imageIDs[idxToRemove]);

                        imageIDs.RemoveAt(idxToRemove);
                        imageTransforms.RemoveAt(idxToRemove);
                        imageRegions.RemoveAt(idxToRemove);

                        if (imageIDs.Count > 0)
                            CurImageIdx = (curImageIdx) % imageIDs.Count;

                        RaisePropertyChanged(nameof(ImagePath));
                        RaisePropertyChanged(nameof(ImageSource));
                        RaisePropertyChanged(nameof(ImageTransform));
                        RaisePropertyChanged(nameof(CalibratedRegions));

                        RaisePropertyChanged(nameof(ImagesCount));
                        break;
                    case MessageBoxResult.No:
                        break;
                    default: throw new NotImplementedException();
                }

            });
            RecalculateRegionProperties();
        }

        public PhotoCalibratedBoreIntervalVM(IImageStorage imageStorage) : base()
        {
            this.imageStorage = imageStorage;
            Initialize();
        }

        private void PhotoCalibratedBoreIntervalVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(base.UpperDepth):
                    RecalculateRegionProperties();
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                    break;
                case nameof(base.LowerDepth):
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                    break;
                case nameof(ExtractedLength):
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                    break;
            }
        }

        #region serialization

        protected PhotoCalibratedBoreIntervalVM(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Matrix[] matrices = (Matrix[])info.GetValue("Transforms", typeof(Matrix[]));
            imageTransforms = matrices.Select(m => (Transform)(new MatrixTransform(m))).ToList();

            List<List<CalibratedRegionVM>> regionsList = (List<List<CalibratedRegionVM>>)info.GetValue("Regions", typeof(List<List<CalibratedRegionVM>>));
            imageRegions = regionsList.Select(im => (IEnumerable<CalibratedRegionVM>)im).ToList();

            imageIDs = (List<Guid>)info.GetValue(nameof(imageIDs), typeof(List<Guid>));

            //TODO: avoid usage of explicit derived type here
            imageStorage = (IImageStorage)info.GetValue(nameof(imageStorage), typeof(Persistence.FolderImageStorage));

            //restoring commands and event subscriptions
            Initialize();

            foreach (var image in imageRegions)
                foreach (var region in image)
                    AttachHandlersToRegionVM(region);

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Matrix[] matrices = imageTransforms.Select(t => t.Value).ToArray();
            info.AddValue("Transforms", matrices);

            List<List<CalibratedRegionVM>> regionsList = imageRegions.Select(im => im.ToList()).ToList();
            info.AddValue("Regions", regionsList);

            info.AddValue(nameof(imageIDs), imageIDs);

            //TODO: avoid usage of explicit derived type here
            info.AddValue(nameof(imageStorage), (Persistence.FolderImageStorage)imageStorage);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
