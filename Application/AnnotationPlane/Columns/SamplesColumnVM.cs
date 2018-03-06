using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane
{
    [Serializable]
    public class SampleVM : ViewModel, ISerializable {
        private double level;
        /// <summary>
        /// Depth in WPF units
        /// </summary>
        public double Level {
            get {
                return level;
            }
            set {
                if (level != value) {
                    level = value;
                    RaisePropertyChanged(nameof(Level));
                }
            }
        }

        private int number;
        /// <summary>
        /// Order number of the sample
        /// </summary>
        public int Number {
            get { return number; }
            set {
                if (number != value) {
                    number = value;
                    RaisePropertyChanged(nameof(Number));
                }
            }
        }

        private double depth;
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double Depth {
            get {
                return depth;
            }
            set {
                if (depth != value) {
                    depth = value;
                    RaisePropertyChanged(nameof(Depth));
                }
            }
        }

        public ICommand DragStarted { get; set; }        

        /// <param name="depth">in meters (positive value)</param>
        public SampleVM(double depth) {
            this.depth = depth;
        }        

        #region serialization
        protected SampleVM(SerializationInfo info, StreamingContext context) {
            Depth = info.GetDouble("Depth");
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Depth",Depth);
        }
        #endregion
    }

    [Serializable]
    public class SamplesColumnVM : ColumnVM, ISerializable
    {
        private SampleVM[] samples;
        public SampleVM[] Samples {
            get {
                return samples;
            }
            set {
                if (samples != value) {
                    samples = value.OrderBy(s => s.Depth).ToArray();
                    //assigning count number and WPF Level
                    int counter = 1;
                    foreach (SampleVM vm in samples) {
                        vm.Number = counter++;
                        vm.Level = RealDepthToWPF(vm.Depth);
                        if (DragStart != null)
                            vm.DragStarted = DragStart;
                    }

                    RaisePropertyChanged(nameof(Samples));
                }
            }
        }

        public SamplesColumnVM() : base("Образцы")
        {
            Samples = new SampleVM[0];
        }

        private ICommand dragStart;

        public ICommand DragStart
        {
            get
            {
                return dragStart;
            }
            set
            {
                if (dragStart != value)
                {
                    dragStart = value;
                    RaisePropertyChanged(nameof(DragStart));

                    if (samples != null)
                    {
                        foreach (SampleVM vm in samples)
                        {
                            vm.DragStarted = dragStart;
                        }
                    }
                }
            }
        }

        #region serialization
        protected SamplesColumnVM(SerializationInfo info, StreamingContext context): base("Образцы")
        {
            UpperBound = info.GetDouble("Top");
            LowerBound = info.GetDouble("Bottom");
            ColumnHeight = info.GetDouble("Height");
            Samples = (SampleVM[])info.GetValue("Samples",typeof(SampleVM[]));
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Samples", Samples);
            info.AddValue("Top", UpperBound);
            info.AddValue("Bottom", LowerBound);
            info.AddValue("Height", ColumnHeight);
        }
        #endregion
    }
}
