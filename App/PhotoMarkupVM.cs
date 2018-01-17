using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace All
{
    public class PhotoMarkupVM : ViewModel
    {
        private string imagePath = string.Empty;
        public string ImagePath
        {
            get
            {
                return this.imagePath;
            }
            set
            {
                if (value != this.imagePath)
                {
                    this.imagePath = value;
                    this.RaisePropertyChanged(nameof(ImagePath));
                }
            }
        }


    }
}
