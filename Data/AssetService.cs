using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Collections.Concurrent;

namespace blazor_experiment.Data
{
    public class AssetService : BackgroundService
    {
        private static Repository _repository;
        private static FileSystemWatcher _watcher;

        private Task _mediaProcessorTask;
        private ConcurrentQueue<string> _mediaSourceQueue = new ConcurrentQueue<string>();

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

            _mediaProcessorTask = Task.Run(async () => await MediaProcessor());

            foreach (var file in new DirectoryInfo(watchPath).GetFiles("*", SearchOption.AllDirectories))
            {
                _mediaSourceQueue.Enqueue(file.FullName);
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
            _mediaSourceQueue.Enqueue(e.FullPath);
        }

        private async Task MediaProcessor()
        {
            while (true)
            {
                if (_mediaSourceQueue.TryDequeue(out var imagePath))
                {
                    try
                    {
                        ProcessImage(imagePath);
                    }
                    catch (IOException)
                    {
                        // Retry later
                        _mediaSourceQueue.Enqueue(imagePath);
                    }
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        private void ProcessImage(string imagePath)
        {
            if (imagePath.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                && !_repository.AssetExists(imagePath))
            {
                Thumbnail thumbnail;
                using (Image<Rgba32> image = Image.Load(imagePath))
                {
                    image.Mutate(x => x
                         .Resize(image.Width / 16, image.Height / 16)
                         .Grayscale());

                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, JpegFormat.Instance);
                        thumbnail = new Thumbnail { Raw = ms.ToArray() };
                    }
                }

                var asset = new Asset { Path = imagePath };
                thumbnail.AssetGuid = asset.Guid;

                _repository.AddAsset(asset, thumbnail);
                if (Changed != null)
                    Changed.Invoke(asset, new EventArgs());
            }
        }
    }
}
