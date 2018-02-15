using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Persistence
{    
    public class FolderPersister : IProjectPersister
    {
        private string path;
        private string ProjectFilePath
        {
            get { return Path.Combine(path, "Project.json"); }
        }        

        private string ImagesFolderPath
        {
            get { return Path.Combine(path, "Photos"); }
        }

        public FolderPersister(string path)
        {
            this.path = path;
        }

        private JsonSerializerSettings newtonJsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,            
            Formatting = Formatting.Indented,
            SerializationBinder = new Newtonsoft.Json.Serialization.DefaultSerializationBinder()
        };

        public ProjectVM LoadProject()
        {
            string serialized;
            using (StreamReader sr = new StreamReader(ProjectFilePath))
            {
                serialized = sr.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<ProjectVM>(serialized, newtonJsonSettings);
        }

        public void SaveProject(ProjectVM project)
        {

            string serialized = JsonConvert.SerializeObject(project, newtonJsonSettings);
            using (StreamWriter sw = new StreamWriter(ProjectFilePath))
            {
                sw.Write(serialized);
            }            
        }
    }
}
