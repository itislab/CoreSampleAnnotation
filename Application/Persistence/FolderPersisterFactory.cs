using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoreSampleAnnotation.Persistence
{
    class FolderPersisterFactory : IProjectPersisterFactory
    {
        public bool TryCreateNew(out IProjectPersister persister)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Выберите папку для нового проекта";

            bool done = false;

            while (!done)
            {
                CommonFileDialogResult result = dialog.ShowDialog();
                switch (result)
                {
                    case CommonFileDialogResult.Ok:

                        string path = dialog.FileName;
                        if (!System.IO.Directory.Exists(path))
                            System.IO.Directory.CreateDirectory(path);
                        else
                        {
                            var files = System.IO.Directory.GetFiles(path);
                            var dirs = System.IO.Directory.GetDirectories(path);
                            if (files.Length + dirs.Length > 0)
                            {
                                MessageBox.Show("Выбранная папка не пуста. Выберите пустую папку для нового проекта", "Папка не пуста", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                continue;
                            }                            
                        }
                        persister = new FolderPersister(path);
                        persister.SaveProject(new ProjectVM()); //new clean project

                        return true;
                    default:
                        done = true;
                        break;
                }
            }
            persister = null;            
            return false;
        }

        public bool TryRestoreExisting(out IProjectPersister persister)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Выберите папку сохраненного ранее проекта";

            CommonFileDialogResult result = dialog.ShowDialog();
            switch (result)
            {
                case CommonFileDialogResult.Ok:

                    string path = dialog.FileName;
                    
                    persister = new FolderPersister(path);                    

                    return true;
                default:
                    persister = null;
                    return false;
            }
        }
    }
}
