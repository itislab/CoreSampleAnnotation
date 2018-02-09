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
        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        private static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }


        private string path;

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
                return id;
            }
            else return Guid.Empty;

        }

        public void RemoveImage(Guid id)
        {
            string path = GetFilePath(id);
            if (File.Exists(path))
                File.Delete(path);
        }

        #region serialization

        protected FolderImageStorage(SerializationInfo info, StreamingContext context) {
            FolderPath = Path.Combine(Directory.GetCurrentDirectory(), info.GetString("Folder"));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string relativePath = MakeRelativePath(Directory.GetCurrentDirectory() + "\\", FolderPath);
            info.AddValue("Folder", relativePath);

        }
        #endregion
    }
}
