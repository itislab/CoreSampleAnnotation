using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreSampleAnnotation.Intervals;
using System.Runtime.Serialization;

namespace CoreSampleAnnotation.Persistence
{        
    [Serializable]
    public class FolderImageStorage : IImageStorage, ISerializable
    {
        
        private string path;
        private Dictionary<Guid, string> imageNames = new Dictionary<Guid, string>();

        /// <summary>
        /// Where the images are stored (absolute path)
        /// </summary>
        public string FolderPath { get { return path; } set { path = value; } }

        public FolderImageStorage() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath">Where the images are stored (absolute path)</param>        
        public FolderImageStorage(string folderPath) {
            path = folderPath;
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public string GetFilePath(Guid imageID)
        {
            return Path.Combine(path, imageID.ToString() + ".jpg");
        }

        public Guid AddNewImage()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.ShowDialog();
            if (File.Exists(ofd.FileName))
            {
                Guid id = Guid.NewGuid();
                File.Copy(ofd.FileName, GetFilePath(id));
                imageNames.Add(id, ofd.SafeFileName);
                return id;
            }
            else return Guid.Empty;

        }

        public void RemoveImage(Guid id)
        {
            string path = GetFilePath(id);
            if (File.Exists(path))
                File.Delete(path);
            if (imageNames.ContainsKey(id))
                imageNames.Remove(id);
        }

        public string GetImageName(Guid imageID)
        {
            if (imageNames.ContainsKey(imageID))
                return imageNames[imageID];
            else
                return string.Empty;
        }

        #region serialization

        protected FolderImageStorage(SerializationInfo info, StreamingContext context) {
            FolderPath = Path.Combine(Directory.GetCurrentDirectory(), info.GetString("Folder"));
            imageNames = info.GetValue("Names", typeof(Dictionary<Guid, string>)) as Dictionary<Guid, string>;
            if (imageNames == null)
                imageNames = new Dictionary<Guid, string>();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string relativePath = PathUtils.MakeRelativePath(Directory.GetCurrentDirectory() + "\\", FolderPath);
            info.AddValue("Folder", relativePath);
            info.AddValue("Names", imageNames);
        }        
        #endregion
    }
}
