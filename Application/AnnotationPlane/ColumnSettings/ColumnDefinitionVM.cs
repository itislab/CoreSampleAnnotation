using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    public class ColumnDefinitionVM : ViewModel
    {
        private ICommand moveLeft;
        public ICommand MoveLeft
        {
            get
            {
                return moveLeft;
            }
            set
            {
                if (moveLeft != value)
                {
                    moveLeft = value;
                    RaisePropertyChanged(nameof(MoveLeft));
                }
            }
        }

        private ICommand moveRight;
        public ICommand MoveRight
        {
            get
            {
                return moveRight;
            }
            set
            {
                if (moveRight != value)
                {
                    moveRight = value;
                    RaisePropertyChanged(nameof(MoveRight));
                }
            }
        }


        private ICommand remove;

        public ICommand RemoveCommand {
            get {
                return remove;
            }
            set {
                if (remove != value) {
                    remove = value;
                    RaisePropertyChanged(nameof(RemoveCommand));
                }
            }
        }

        public bool CanMoveLeft
        {
            get
            {
                if (MoveLeft != null)
                    return MoveLeft.CanExecute(ColumnOrder);
                else
                    return true;
            }
        }

        public bool CanMoveRight
        {
            get
            {
                if (MoveRight != null)
                    return MoveRight.CanExecute(ColumnOrder);
                else
                    return true;
            }
        }

        private int columnOrder;
        public int ColumnOrder
        {
            get { return columnOrder; }
            set
            {
                if (columnOrder != value)
                {
                    columnOrder = value;
                    RaisePropertyChanged(nameof(ColumnOrder));
                }
            }
        }        
    }
}
