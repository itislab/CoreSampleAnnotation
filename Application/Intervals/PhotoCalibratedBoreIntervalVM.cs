using CoreSampleAnnotation.PhotoMarkup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace CoreSampleAnnotation.Intervals
{
    public class PhotoCalibratedBoreIntervalVM : BoreIntervalVM
    {
        private double annotatedLength = 0;
        /// <summary>
        /// A total length in meters of all annotated parts of the core sample
        /// </summary>
        public double AnnotatedLength
        {
            get { return regions.Sum(r => r.Length)*0.01; }            
        }

        public double AnnotatedPercentage
        {
            get
            {
                return AnnotatedLength / ExtractionLength * 100.0;
            }
        }

        private IEnumerable<CalibratedRegionVM> regions = new List<CalibratedRegionVM>();

        private List<Transform> imageTransforms = new List<Transform>();

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
            get { return regions; }
            set
            {
                if (regions != value)
                {
                    regions = value;
                    RaisePropertyChanged(nameof(CalibratedRegions));
                    RaisePropertyChanged(nameof(ImagesButtonText));
                    RaisePropertyChanged(nameof(AnnotatedLength));
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
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

        public int CurImageIdx {
            get { return curImageIdx; }
            set {
                if (curImageIdx != value) {
                    curImageIdx = value;
                    RaisePropertyChanged(nameof(CurImageIdx));
                    RaisePropertyChanged(nameof(ImagePath));
                    RaisePropertyChanged(nameof(ImageTransform));
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

        public ICommand AddNewImageCommand
        {
            get { return addNewImageCommand; }
        }

        public ICommand NextImageCommand { private set; get; }

        public int AvailableImagesCount
        {
            get { return imagePaths.Count; }
        }

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
                    RaisePropertyChanged(nameof(AvailableImagesCount));

                    curImageIdx = imagePaths.Count - 1;
                    RaisePropertyChanged(nameof(ImagePath));
                    RaisePropertyChanged(nameof(ImageTransform));
                }
            });

            NextImageCommand = new DelegateCommand(() => {                
                CurImageIdx = (curImageIdx + 1) % imagePaths.Count;
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
