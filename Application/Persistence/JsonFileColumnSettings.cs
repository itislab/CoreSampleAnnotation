using CoreSampleAnnotation.AnnotationPlane.ColumnSettings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Persistence
{
    /// <summary>
    /// Persists columns settings in the JSON file chosen by the user through windows file dialog
    /// Default files location is special folder inside the project folder
    /// </summary>
    [Serializable]
    public class JsonFileColumnSettings : IColumnSettingsPersistence, ISerializable
    {
        public const string TamplatesDir = "ColumnSettingTemplates";
        public const string DefaultSettingsFile = "default.json";

        private JsonSerializerSettings newtonJsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            SerializationBinder = new Newtonsoft.Json.Serialization.DefaultSerializationBinder()
        };

        public bool Load(out ColumnDefinitionVM[] definitions)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            string templateDir = Path.Combine(Directory.GetCurrentDirectory(), TamplatesDir);
            if (Directory.Exists(templateDir))
                dlg.InitialDirectory = templateDir;
            dlg.DefaultExt = ".json";
            dlg.Filter = "JSON files|*.json";
            dlg.Title = "Выберите файл с настройками колонок";
            dlg.Multiselect = false;

            definitions = null;
            var res = dlg.ShowDialog();
            if (res.HasValue && res.Value)
            {
                string path = dlg.FileName;
                try
                {
                    string serialized = File.ReadAllText(path);
                    definitions = JsonConvert.DeserializeObject<ColumnDefinitionVM[]>(serialized, newtonJsonSettings);
                    System.Windows.MessageBox.Show("Настройки колонок успешно загружены", "Успешно", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        string.Format("Не удалось загрузить настройки колонок: {0}",ex.ToString()),
                        "Не удалось", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);                    
                    return false;
                }
            }
            return false;
        }

        public bool LoadDefaults(out ColumnDefinitionVM[] definitions)
        {
            string templateFile = Path.Combine(Directory.GetCurrentDirectory(), TamplatesDir, DefaultSettingsFile);
            if (File.Exists(templateFile))
            {
                string serialized = File.ReadAllText(templateFile);
                definitions = JsonConvert.DeserializeObject<ColumnDefinitionVM[]>(serialized, newtonJsonSettings);
                return true;
            }
            else
            {
                definitions = new ColumnDefinitionVM[0];
                return false;
            }
        }

        public void Persist(ColumnDefinitionVM[] definitions)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            string templateDir = Path.Combine(Directory.GetCurrentDirectory(), TamplatesDir);
            if (Directory.Exists(templateDir))
                dlg.InitialDirectory = templateDir;
            dlg.DefaultExt = ".json";
            dlg.Filter = "JSON files|*.json";
            dlg.Title = "Выберите файл с настройками колонок";
            
            var res = dlg.ShowDialog();
            if (res.HasValue && res.Value)
            {
                string path = dlg.FileName;
                string serialized = JsonConvert.SerializeObject(definitions, newtonJsonSettings);
                File.WriteAllText(path, serialized);
                System.Windows.MessageBox.Show("Настройки колонок успешно сохранены", "Успешно", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        public JsonFileColumnSettings()
        {
        }

        #region serialization
        protected JsonFileColumnSettings(SerializationInfo info, StreamingContext context) {            
        }        

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {            
            //only class type is needed by serialization. Thus nothing is put into info.
        }
        #endregion
    }
}
