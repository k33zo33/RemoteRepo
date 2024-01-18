using Azure.Storage.Blobs.Models;
using RemoteRepo.Dal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteRepo.ViewModels
{
    internal class ItemsViewModel
    {
        public const string ForwardSlash = "/";
        private string? directory;

        public ObservableCollection<BlobItem> Items { get; }
        public ObservableCollection<string> Directories { get; }
        public string? Directory 
        {
            get => directory;
            set 
            {
                directory = value;
                Refresh();
            }
        }
        public ItemsViewModel()
        {
            Items = new ObservableCollection<BlobItem>();
            Directories = new ObservableCollection<string>();
            Refresh();
        }
        private void Refresh()
        {
            Directories.Clear();
            Items.Clear();
            
            Repository.Container.GetBlobs().ToList().ForEach(item =>
            {
                // handle directiories - popravi za ocjenu, sa setom
                if (item.Name.Contains(ForwardSlash))
                {
                    var dir = item.Name[..item.Name.LastIndexOf(ForwardSlash)];
                    if (!Directories.Contains(dir))
                    {
                        Directories.Add(dir);
                    }
                }
                //first, elements from the root
                if (string.IsNullOrEmpty(Directory)&&!item.Name.Contains(ForwardSlash))
                {
                    Items.Add(item);
                }
                //now handle all items in current directory
                else if (!string.IsNullOrEmpty(Directory) && item.Name.StartsWith($"{Directory}{ForwardSlash}"))
                {
                    Items.Add(item);
                }

            });
            
        }
        public async Task DownloadAsync(BlobItem item, string path)
        {
            using var fs = File.OpenWrite(path);
            await Repository.Container.GetBlobClient(item.Name).DownloadToAsync(fs);
            Refresh();
        }

        public async Task UploadAsync(string path)
        {
            var filename = path[(path.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];

            // Extract file extension
            string ext = Path.GetExtension(path).Substring(1);

            // Combine extension with blob name for new blob path
            string newBlobPath = Path.Combine(ext.ToLower(), filename);

            // Check if the "directory" (simulated using blob name prefix) exists, if not, create it
            if (!Repository.Container.GetBlobClient(newBlobPath).Exists())
            {
                Repository.Container.GetBlobClient(newBlobPath).UploadAsync(new MemoryStream(), true);
            }

            // Upload the file to the appropriate "directory"
            using var fs = File.OpenRead(path);
            await Repository.Container.GetBlobClient(newBlobPath).UploadAsync(fs, true);

            Refresh();
        }


        //public async Task UploadAsync(string path, string directory)
        //{
        //    var filename = path[(path.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
        //    if (!string.IsNullOrEmpty(directory))
        //    {
        //        filename = $"{directory}{ForwardSlash}{filename}";
        //    }
        //    using var fs = File.OpenRead(path);
        //    await Repository.Container.GetBlobClient(filename).UploadAsync(fs, true);
        //    Refresh();
        //}




        public async Task DeleteAsync(BlobItem item)
        {
            await Repository.Container.GetBlobClient(item.Name).DeleteAsync();
            Refresh();
        }
    }
}
