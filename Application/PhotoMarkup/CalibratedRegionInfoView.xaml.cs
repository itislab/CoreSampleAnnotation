using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoreSampleAnnotation.PhotoMarkup
{
    /// <summary>
    /// Interaction logic for CalibratedRegionInfoView.xaml
    /// </summary>
    public partial class CalibratedRegionInfoView : UserControl
    {
        public CalibratedRegionInfoView()
        {
            InitializeComponent();
        }
    }

    public class NonZeroDoubleValidationRule : ValidationRule
    {
        public NonZeroDoubleValidationRule()
        {
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            double result = 0;

            try
            {
                if (((string)value).Length > 0) {
                    result = Double.Parse((String)value);
                }
                else
                    return new ValidationResult(false, "Пустое значение длины");
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Вы ввели не число: " + e.Message+". ВВедите число");
            }
            if (result <= 0.0)
                return new ValidationResult(false,"Длина должна быть положительна");
            return ValidationResult.ValidResult;
        }
    }
}
