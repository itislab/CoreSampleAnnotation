using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class AnnotationGridVM: ViewModel
    {


        private ObservableCollection<ColumnVM> columns = new ObservableCollection<ColumnVM>();

        public ObservableCollection<ColumnVM> Columns {
            get { return columns; }
            set {
                if (columns != value) {
                    columns = value;
                    RaisePropertyChanged(nameof(Columns));
                }
            }
        }

        private FrameworkElement draggedItem;
        public FrameworkElement DraggedItem
        {
            get {
                return draggedItem;
            }
            set {
                if (draggedItem != value) {
                    draggedItem = value;
                    RaisePropertyChanged(nameof(DraggedItem));
                }
            }
        }

        private FrameworkElement dragCandidateItem;
        public FrameworkElement DragCandidateItem
        {
            get
            {
                return dragCandidateItem;
            }
            set
            {
                if (dragCandidateItem != value)
                {
                    dragCandidateItem = value;
                    RaisePropertyChanged(nameof(DragCandidateItem));
                }
            }
        }

        //private ICommand touchAndHoldOnDraggableElement;
        //public ICommand touc

        private Point localDraggedItemOffset;
        public Point LocalDraggedItemOffset {
            get {
                return localDraggedItemOffset;
            }
            set {
                if (localDraggedItemOffset != value) {
                    localDraggedItemOffset = value;
                    RaisePropertyChanged(nameof(LocalDraggedItemOffset));
                }
            }
        }

        private Point dragItemInitialLocation;
        public Point DragItemInitialLocation {
            get {
                return dragItemInitialLocation;
            }
            set {
                if (dragItemInitialLocation != value) {
                    dragItemInitialLocation = value;
                    RaisePropertyChanged(nameof(DragItemInitialLocation));
                }
            }
        }        
    }
}
