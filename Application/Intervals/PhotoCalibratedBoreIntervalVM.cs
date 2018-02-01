using CoreSampleAnnotation.PhotoMarkup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                if (double.IsNaN(ExtractedLength) || double.IsNaN(AnnotatedLength) || (ExtractionLength==0.0))
                    return 0.0;
                else
                    return AnnotatedLength / ExtractionLength * 100.0;
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
                    if (imageRegions[curImageIdx] != null)
                        foreach(CalibratedRegionVM crCM in imageRegions[curImageIdx]) 
                            crCM.PropertyChanged -= CalibratedRegion_PropertyChanged;                        
                    if(value != null)
                        foreach (CalibratedRegionVM crCM in value)
                            crCM.PropertyChanged += CalibratedRegion_PropertyChanged;

                    imageRegions[curImageIdx] = value;
                    RaisePropertyChanged(nameof(CalibratedRegions));
                    RaisePropertyChanged(nameof(ImagesButtonText));
                    RaisePropertyChanged(nameof(AnnotatedLength));
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                }
            }
        }

        private void CalibratedRegion_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CalibratedRegionVM vm = sender as CalibratedRegionVM;
            if(vm != null)
            {
                switch (e.PropertyName) {
                    case nameof(vm.Length):
                        RaisePropertyChanged(nameof(AnnotatedLength));
                        RaisePropertyChanged(nameof(AnnotatedPercentage));
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
                    RaisePropertyChanged(nameof(CurImageIdx));
                    RaisePropertyChanged(nameof(ImagePath));
                    RaisePropertyChanged(nameof(ImageTransform));
                    RaisePropertyChanged(nameof(CalibratedRegions));
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
                    CurImageIdx = (curImageIdx + 1) % imagePaths.Count;
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
