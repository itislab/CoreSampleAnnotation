using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreSampleAnnotation.AnnotationPlane.Template;
using System.Windows.Media;

namespace CoreSampleAnnotation.Persistence
{
    [Obsolete("Used for early development")]
    public class HardcodedLayerTemplate
    {
        public static HardcodedLayerTemplate Instance { get; private set; }
        static HardcodedLayerTemplate() {
            Instance = new HardcodedLayerTemplate();
        }

        public Property[] Template
        {
            get
            {
                Property colourProperty = new Property();
                colourProperty.Name = "Цвет";
                colourProperty.IsMulticlass = false;
                colourProperty.ID = colourProperty.Name;
                colourProperty.Classes = new Class[] {
                new Class() { Acronym="c",  ShortName="серые" , Color= Colors.Gray},
                new Class() { Acronym="бж", ShortName= "бежевые", Color= Colors.Beige},
                new Class() { Acronym="бе", ShortName= "белесые", Color= Colors.AntiqueWhite},
                new Class() { Acronym="бу", ShortName= "бурые", Color= Colors.SaddleBrown},
                new Class() { Acronym="ч",  ShortName="чёрные", Color= Colors.Black},
                new Class() { Acronym="к",  ShortName="коричневые", Color= Colors.Brown},
                new Class() { Acronym="ж",  ShortName="жёлтые", Color= Colors.Yellow},
                new Class() { Acronym="ст", ShortName= "темно-серые", Color= Colors.DarkGray},
                new Class() { Acronym="сс", ShortName= "светло-серые", Color= Colors.LightGray},
                new Class() { Acronym="б",  ShortName="белые", Color= Colors.White},
                new Class() { Acronym="г",  ShortName="голубые", Color= Colors.LightBlue},
                new Class() { Acronym="з",  ShortName="зеленые", Color= Colors.Green},
                new Class() { Acronym="кр", ShortName= "красные", Color= Colors.Red},
                new Class() { Acronym="сн", ShortName= "сиреневые", Color= Colors.Violet},
                new Class() { Acronym="рз", ShortName= "розовые", Color= Colors.Pink},
                new Class() { Acronym="рж", ShortName= "рыжие", Color= Colors.OrangeRed},
                new Class() { Acronym="ф",  ShortName="фиолетовые", Color= Colors.Purple},
                new Class() { Acronym="х",  ShortName="хаки", Color= Colors.Khaki},
                new Class() { Acronym="тж", ShortName= "темно-желтые", Color= Colors.DarkGoldenrod},
                new Class() { Acronym="жс", ShortName= "светло-желтые", Color= Colors.LightYellow},
                new Class() { Acronym="зс", ShortName= "светло-зеленые", Color= Colors.LightGreen},
                new Class() { Acronym="нз", ShortName= "сине-зеленые", Color= Colors.Turquoise},
                new Class() { Acronym="гс", ShortName= "светло-голубые", Color= Colors.LightSkyBlue},
                new Class() { Acronym="гт", ShortName= "темно-голубые", Color= Colors.DarkBlue},
                new Class() { Acronym="кв", ShortName= "светло-коричневые", Color= Colors.SandyBrown},
                new Class() { Acronym="кт", ShortName= "темно-коричневые", Color= Colors.RosyBrown},
                new Class() { Acronym="хс", ShortName= "светлый хаки", Color= Colors.Khaki},
                new Class() { Acronym="тх", ShortName= "темный хаки", Color= Colors.DarkKhaki}
            };
                foreach (var c in colourProperty.Classes)
                    c.ID = c.Acronym;

                Property textureProperty = new Property();
                textureProperty.Name = "Текстура";
                textureProperty.IsMulticlass = false;
                textureProperty.ID = textureProperty.Name;
                textureProperty.Classes = new Class[] {
                new Class() { Acronym="б",  ShortName="Бугристая"},
                new Class() { Acronym="в",  ShortName="Восходящая рябь"},
                new Class() { Acronym="гв", ShortName= "Горизонтальная волнистая"},
                new Class() { Acronym="гл", ShortName= "Горизонтальная линзовидная"},
                new Class() { Acronym="гп", ShortName= "Горизонтальная параллельная"},
                new Class() { Acronym="гш", ShortName= "Градационная штормового накопления"},
                new Class() { Acronym="зв", ShortName= "Знаки волновой ряби"},
                new Class() { Acronym="зт", ShortName= "Знакопеременных течений"},
                new Class() { Acronym="кв", ShortName= "Косослоистая волнистая"},
                new Class() { Acronym="кл", ShortName= "Косослоистая линзовидная"},
                new Class() { Acronym="кп", ShortName= "Косослоистая параллельная однонаправленная"},
                new Class() { Acronym="кр", ShortName= "Косослоистая параллельная разнонаправленная"},
                new Class() { Acronym="м",  ShortName="Массивная"},
                new Class() { Acronym="му", ShortName= "Мульдообразная"},
                new Class() { Acronym="нб", ShortName= "Нарушенная брекчированием"},
                new Class() { Acronym="ил", ShortName= "Нарушенная илоедами", InputFields = new InputField[] {
                    new PropertyInputField() {
                        Property = new Property() {
                            Name = "Биотурбация",
                            IsMulticlass = false,
                            Classes = new Class[] {
                            new Class() { Acronym="н",  ShortName="незначительная" , Description = "единичные ходы"},
                            new Class() { Acronym="у", ShortName= "умеренная"},
                            new Class() { Acronym="и", ShortName= "интенсивная", Description = "порода почти полностью переработана илоедами"}
                            }
                        }
                    }
                } }
            };
                foreach (var c in textureProperty.Classes)
                    c.ID = c.ShortName;                               

                return new Property[] { colourProperty, textureProperty };
            }
        }
    }
}
