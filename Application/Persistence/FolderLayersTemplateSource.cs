using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreSampleAnnotation.AnnotationPlane.Template;
using System.Runtime.Serialization;
using System.IO;
using System.Windows;
using FileHelpers;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoreSampleAnnotation.Persistence
{
    [DelimitedRecord("|")]
    [IgnoreFirst]
    public class NamesFileRow
    {
        public string ID;
        public string Acronym;
        public string Name;
        public string Description;
        [OptionalField]
        public double? WidthPercentage;
        [OptionalField]
        public string RightSideForm;
    }

    [Serializable]
    public class FolderLayersTemplateSource : ILayersTemplateSource, ISerializable
    {
        private const string NamesFile = "Names.csv";
        private const string MulticlassFile = "Multiclass";
        private const string BackgroundFillFolder = "BackgroundFill";
        private const string IconsFolder = "Icons";
        private const string ExampleImagesFolder = "ExampleImages";
        private string path;

        /// <summary>
        /// Where the template is stored (absolute path)
        /// </summary>
        public string FolderPath { get { return path; } set { path = value; } }

        public FolderLayersTemplateSource(string templateFolderPath)
        {
            path = templateFolderPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Path to property definition folder</param>
        /// <returns></returns>
        private Property LoadPropertyTemplate(string path)
        {
            string propID = Path.GetFileName(path).ToLowerInvariant();
            propID = propID.ToLowerInvariant();
            string namesFileFullPath = Path.Combine(path, NamesFile);
            string muticlassFileFullPath = Path.Combine(path, MulticlassFile);
            string backgroundImagesDirFullPath = Path.Combine(path, BackgroundFillFolder);
            string iconDirFullPath = Path.Combine(path, IconsFolder);
            string exampleImagesDirFullPath = Path.Combine(path, ExampleImagesFolder);

            Dictionary<string, Class> loadedClasses = new Dictionary<string, Class>();

            if (File.Exists(namesFileFullPath))
            {
                var engine = new FileHelperEngine<NamesFileRow>();
                var rows = engine.ReadFile(namesFileFullPath);

                bool backgroundBrushFolderAvailable = Directory.Exists(backgroundImagesDirFullPath);

                foreach (NamesFileRow row in rows)
                {
                    string classID = row.ID.ToLowerInvariant();
                    if (!row.WidthPercentage.HasValue)
                        row.WidthPercentage = 100.0;
                    else
                    if (row.WidthPercentage < 0 || row.WidthPercentage > 100.0)
                    {
                        MessageBox.Show(string.Format("значение ширна крапа вне допустимых значений. допустимый интервал 0 - 100 (%), в файле задано {0}. Будет использовано значение 100%", row.WidthPercentage), "Ширина крапа", MessageBoxButton.OK, MessageBoxImage.Warning);
                        row.WidthPercentage = 100.0;
                    }

                    //tring to load corresponding background SVG image
                    string backgroundPatternSVG = null;
                    string bgClassBgPatternPath = Path.Combine(backgroundImagesDirFullPath, string.Format("{0}.svg", classID));
                    if (File.Exists(bgClassBgPatternPath))
                    {
                        backgroundPatternSVG = File.ReadAllText(bgClassBgPatternPath);
                    }

                    //tring to load corresponding icon SVG
                    string iconSVG = null;
                    string iconClassPath = Path.Combine(iconDirFullPath, string.Format("{0}.svg", classID));
                    if (File.Exists(iconClassPath))
                    {
                        iconSVG = File.ReadAllText(iconClassPath);
                    }

                    ImageSource exampleImage = null;
                    string exampleImagePath = Path.Combine(exampleImagesDirFullPath, string.Format("{0}.jpg", classID));
                    if (File.Exists(exampleImagePath))
                    {
                        exampleImage = new BitmapImage(new Uri(Path.GetFullPath(exampleImagePath)));
                    }

                    RightSideFormEnum rightSide = RightSideFormEnum.Straight;
                    if (!string.IsNullOrEmpty(row.RightSideForm))
                        switch (row.RightSideForm.ToLowerInvariant())
                        {                            
                            case "прямая": rightSide = RightSideFormEnum.Straight; break;
                            case "ступеньки": rightSide = RightSideFormEnum.Steps; break;
                            case "волна": rightSide = RightSideFormEnum.Wave; break;
                            default:
                                MessageBox.Show(
                                    string.Format("Форма правой границы крапа \"{0}\", указанная в шаблоне, не поддерживается. Будет использована прямая форма. ({1})", row.RightSideForm, row.ToString()),
                                    "Форма правой границы крапа",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                                break;

                        }

                    loadedClasses.Add(row.ID.ToLowerInvariant(),
                        new Class()
                        {
                            ID = classID,
                            Acronym = row.Acronym,
                            ShortName = row.Name,
                            BackgroundPatternSVG = backgroundPatternSVG,
                            IconSVG = iconSVG,
                            Description = row.Description,
                            WidthRatio = row.WidthPercentage.Value * 0.01,// percent to ratio
                            ExampleImage = exampleImage,
                            RightSideForm = rightSide
                        });
                }



                return new Property()
                {
                    ID = propID,
                    Classes = loadedClasses.Select(p => p.Value).ToArray(),
                    Name = propID,
                    IsMulticlass = File.Exists(muticlassFileFullPath)
                };
            }
            else
                throw new InvalidDataException(String.Format("Не найден файл {0} с именами классов свойства слоя {1}", namesFileFullPath, propID));
        }

        public Property[] Template
        {
            get
            {
                List<Property> loadedProperties = new List<Property>();
                string[] folders = Directory.GetDirectories(FolderPath);
                foreach (string folder in folders)
                {
                    try
                    {
                        loadedProperties.Add(LoadPropertyTemplate(folder));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Ошибка чтения шаблона свойства слоя керна. Свойство не загружено! (путь {0}):{1}", folder, ex.ToString()), "Не удалось загрузить шаблон свойства слоя", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                return loadedProperties.ToArray();
            }
        }

        #region serialization
        protected FolderLayersTemplateSource(SerializationInfo info, StreamingContext context)
        {
            FolderPath = Path.Combine(Directory.GetCurrentDirectory(), info.GetString("Folder"));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string relativePath = PathUtils.MakeRelativePath(Directory.GetCurrentDirectory() + "\\", FolderPath);
            info.AddValue("Folder", relativePath);
        }
        #endregion
    }
}
