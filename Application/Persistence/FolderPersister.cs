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
        private string IntervalsFilePath {
            get { return Path.Combine(path, "intervals.json"); }
        }

        public FolderPersister(string path)
        {
            this.path = path;
        }

        public ProjectVM LoadProject()
        {
            string serialized;        
            using (StreamReader sr = new StreamReader(IntervalsFilePath))
            {
                serialized = sr.ReadToEnd();
            }
            return (ProjectVM)JsonConvert.DeserializeObject(serialized, typeof(ProjectVM));
        }

        public void SaveProject(ProjectVM project)
        {
            string serialized = JsonConvert.SerializeObject(project,Formatting.Indented);            
            using (StreamWriter sw = new StreamWriter(IntervalsFilePath))
            {
                sw.Write(serialized);
            }
        }
    }
}
