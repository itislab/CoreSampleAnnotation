using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class AnnotationGridVM: ViewModel
    {
        /// <summary>
        /// How long the user needs to hold draggable element on the same place to trigger LongHold event
        /// </summary>
        private const int LongHoldDuration = 1000;
        private Timer ElementHoldTimer;
        

        private ObservableCollection<ColumnVM> columns = new ObservableCollection<ColumnVM>();

        private void ClearTimer() {
            if (ElementHoldTimer != null)
            {
                ElementHoldTimer.Elapsed -= ElementHoldTimer_Elapsed;
                ElementHoldTimer.Dispose();
                ElementHoldTimer = null;
            }
        }

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


                    if (ElementHoldTimer != null) {
                        //element started do be dragged before the long hold timer ticked.
                        //That means that it is not long hold, that is a drag, so deactivateing timer.
                        ClearTimer();
                    }
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

                    if (value != null)
                    {
                        //activateing the timer
                        ClearTimer();
                        ElementHoldTimer = new Timer(LongHoldDuration);
                        ElementHoldTimer.Elapsed += ElementHoldTimer_Elapsed;
                        ElementHoldTimer.Start();
                    }
                }
            }
        }

        private void ElementHoldTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (LongHoldOnDragabaleElementCommand != null && DragCandidateItem != null)
            {
                DragCandidateItem.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LongHoldOnDragabaleElementCommand.Execute(DragCandidateItem.DataContext);
                    DragCandidateItem = null;
                    ClearTimer();
                }));
                
            }
        }

        private ICommand longHoldOnDraggableElementCommand;
        public ICommand LongHoldOnDragabaleElementCommand {
            get {
                return longHoldOnDraggableElementCommand;
            }
            set {
                if (longHoldOnDraggableElementCommand != value) {
                    longHoldOnDraggableElementCommand = value;
                    RaisePropertyChanged(nameof(LongHoldOnDragabaleElementCommand));
                }
            }
        }

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


    public class FrameworkElementEventArgs : EventArgs
    {
        public FrameworkElement Element { get; private set; }
    }
}
