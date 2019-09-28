using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blazor_experiment.Data
{
    public class AssetService : BackgroundService
    {
        private static Repository _repository;
        private static FileSystemWatcher _watcher;

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
            ProcessFile(e.Name, e.FullPath);
        }

        private void ProcessFile(string fileName, string filePath)
        {
            if (filePath.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
            {
                var asset = new Asset { Path = fileName };
                _repository.AddAsset(asset);
                if (Changed != null)
                    Changed.Invoke(asset, new EventArgs());
            }
        }
    }
}
