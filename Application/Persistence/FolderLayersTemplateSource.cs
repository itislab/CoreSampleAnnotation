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

namespace CoreSampleAnnotation.Persistence
{
    [DelimitedRecord(",")]
    public class NamesFileRow {
        public string ID;
        public string Acronym;
        public string Name;
        public string Description;
    }

    [Serializable]
    public class FolderLayersTemplateSource : ILayersTemplateSource, ISerializable
    {
        private const string NamesFile = "Names.csv";
        private const string MulticlassFile = "Multiclass";
        private const string BackgroundFillFolder = "BackgroundFill";
        private string path;

        /// <summary>
        /// Where the template is stored (absolute path)
        /// </summary>
        public string FolderPath { get { return path; } set { path = value; } }

        public FolderLayersTemplateSource(string templateFolderPath) {
            path = templateFolderPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Path to property definition folder</param>
        /// <returns></returns>
        private Property LoadPropertyTemplate(string path) {
            string propID = Path.GetFileName(path).ToLowerInvariant();
            propID = propID.ToLowerInvariant();
            string namesFileFullPath = Path.Combine(path, NamesFile);
            string muticlassFileFullPath = Path.Combine(path, MulticlassFile);
            string backgroundImagesDirFullPath = Path.Combine(path,BackgroundFillFolder);

            Dictionary<string, Class> loadedClasses = new Dictionary<string, Class>();

            if (File.Exists(namesFileFullPath))
            {
                var engine = new FileHelperEngine<NamesFileRow>();
                var rows = engine.ReadFile(namesFileFullPath);

                bool backgroundBrushFolderAvailable = Directory.Exists(backgroundImagesDirFullPath);

                foreach (NamesFileRow row in rows)
                {
                    string classID = row.ID.ToLowerInvariant();

                    //tring to load corresponding background SVG image
                    string backgroundPatternSVG = null;
                    string bgClassBgPatternPath = Path.Combine(backgroundImagesDirFullPath, string.Format("{0}.svg", classID));
                    if (File.Exists(bgClassBgPatternPath)) {
                        backgroundPatternSVG = File.ReadAllText(bgClassBgPatternPath);
                    }

                    loadedClasses.Add(row.ID.ToLowerInvariant(),
                        new Class()
                        {
                            ID = classID,
                            Acronym = row.Acronym,
                            ShortName = row.Name,
                            BackgroundPatternSVG = backgroundPatternSVG,
                            Description = row.Description
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
                throw new InvalidDataException(String.Format("Не найден файл {0} с именами классов свойства слоя {1}", namesFileFullPath,propID));
        }

        public Property[] Template
        {            
            get
            {
                List<Property> loadedProperties = new List<Property>();
                string[] folders = Directory.GetDirectories(FolderPath);
                foreach (string folder in folders) {
                    try {                        
                        loadedProperties.Add(LoadPropertyTemplate(folder));
                    }
                    catch (Exception ex) {
                        MessageBox.Show(string.Format("Ошибка чтения шаблона свойства слоя керна. Свойство не загружено! (путь {0}):{1}", folder, ex.ToString()), "Не удалось загрузить шаблон свойства слоя", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                return loadedProperties.ToArray();
            }
        }

        #region serialization
        protected FolderLayersTemplateSource(SerializationInfo info, StreamingContext context) {
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
