using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    public enum Presentation { BackgroundImage, Acronym, ShortName, Description, Icon, RightSide, BottomSide, Width }

    public class Variant
    {
        public string PropID { get; private set; }
        public string PropertyName { get; private set; }
        public Presentation Presentation { get; private set; }

        public Variant(string propID, string propName, Presentation presentation)
        {
            PropID = propID;
            PropertyName = propName;
            Presentation = presentation;
        }

        public string TexturalString
        {
            get
            {
                string enumText;
                switch (Presentation)
                {
                    case Presentation.Acronym:
                        enumText = "Текстовое сокращение"; break;
                    case Presentation.BackgroundImage:
                        enumText = "Крап"; break;
                    case Presentation.Description:
                        enumText = "Расширеное текстовое описание"; break;
                    case Presentation.ShortName:
                        enumText = "Короткое имя"; break;
                    case Presentation.Icon:
                        enumText = "Значок"; break;
                    case Presentation.RightSide:
                        enumText = "Форма правой границы"; break;
                    case Presentation.BottomSide:
                        enumText = "Форма нижней границы"; break;
                    case Presentation.Width:
                        enumText = "Ширина крапа"; break;
                    default:
                        throw new NotImplementedException();
                }

                return string.Format("{0}: {1}", PropertyName, enumText);
            }
        }
    }
}
