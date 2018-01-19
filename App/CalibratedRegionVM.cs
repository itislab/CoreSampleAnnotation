using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace All
{
    /// <summary>
    /// Represents a region on the photo that correponds to the part of the core sample
    /// </summary>
    public class CalibratedRegionVM :ViewModel
    {
        private Point up;
        public Point Up {
            get {
                return up;
            }
            set {
                if (up != value) {
                    up = value;
                    RaisePropertyChanged(nameof(Up));
                }
            }
        }

        private Point bottom;
        public Point Bottom {
            get {
                return bottom;
            }
            set {
                if (bottom != value) {
                    bottom = value;
                    RaisePropertyChanged(nameof(Bottom));
                }
            }
        }

        private Point side;
        public Point Side {
            get {
                return side;
            }
            set {
                if (side != value) {
                    side = value;
                    RaisePropertyChanged(nameof(Side));
                }
            }
        }

        private bool isFocused = false;
        public bool IsFocused {
            get {
                return isFocused;
            }
            set {
                if (isFocused != value) {
                    isFocused = value;
                    RaisePropertyChanged(nameof(IsFocused));
                }
            }
        }

        private int order = 0;
        /// <summary>
        /// The order according to which the core sample parts are aligned between each other
        /// </summary>
        public int Order {
            get {
                return order;
            }
            set {
                if (order != value) {
                    order = value;
                    RaisePropertyChanged(nameof(Order));
                }
            }
        }

        private double length;
        /// <summary>
        /// The real (measured with ruller) length (in cm) of current core sample part
        /// </summary>
        public double Length {
            get {
                return length;
            }
            set {
                if (length != value) {
                    length = value;
                    RaisePropertyChanged(nameof(Length));
                }
            }
        }

        private ICommand moveUp;
        public ICommand MoveUp {
            get { return moveUp; }
            set {
                if (moveUp != value) {
                    moveUp = value;
                    RaisePropertyChanged(nameof(MoveUp));
                }
            }
        }

        private ICommand moveDown;
        public ICommand MoveDown {
            get { return moveDown; }
            set {
                if (moveDown != value) {
                    moveDown = value;
                    RaisePropertyChanged(nameof(MoveDown));
                }
            }
        }

        public CalibratedRegionVM()
        {

        }
    }
}
