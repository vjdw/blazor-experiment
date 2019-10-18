using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace blazor_experiment.Data
{
    public class AssetService : BackgroundService
    {
        private static Repository _repository;
        private static FileSystemWatcher _watcher;
        private static Queue<FileSystemEventArgs> _newFileQueue;

        public event EventHandler Changed;

        public AssetService(Repository repository)
        {
            _repository = repository;
        }

        public static Task<Asset[]> GetAssets()
        {
            return Task.FromResult(_repository.GetAssets().ToArray());
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _repository.DeleteAllAssets();

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var watchPath = @"C:\Users\vince\Desktop\scratch";

            foreach (var file in new DirectoryInfo(watchPath).GetFiles("*", SearchOption.AllDirectories))
            {
                ProcessFile(file.Name, file.FullName);
            }

            _watcher = new FileSystemWatcher(watchPath);
            _watcher.Created += File_Created;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            throw new NotImplementedException();
        }

        private void File_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                //_newFileQueue.Enqueue(e);  // TODO: do something with this
                ProcessFile(e.Name, e.FullPath);
            }
            catch (IOException)
            {
                // Add to queue for processing later?
            }
        }



        private void ProcessFile(string fileName, string filePath)
        {
            if (filePath.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
            {
                Thumbnail thumbnail;
                using (Image<Rgba32> image = Image.Load(filePath))
                {
                    image.Mutate(x => x
                         .Resize(image.Width / 16, image.Height / 16)
                         .Grayscale());
                    //image.Save("bar.jpg"); // Automatic encoder selected based on extension.
                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, JpegFormat.Instance);
                        thumbnail = new Thumbnail { Raw = ms.ToArray() };
                    }
                }

                var asset = new Asset { Path = fileName };
                thumbnail.AssetGuid = asset.Guid;

                _repository.AddAsset(asset, thumbnail);
                if (Changed != null)
                    Changed.Invoke(asset, new EventArgs());
            }
        }
    }
}
