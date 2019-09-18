using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazor_experiment.Data
{
    public class AssetService
    {
        private Repository repository;

        private static readonly List<Asset> Assets = new List<Asset>
        {
            new Asset {Path = "one"}, new Asset { Path = "two"}
        };

        public AssetService(Repository repository)
        {
            Assets.Add(new Asset{Path = "three"});

            this.repository = repository;
        }

        public Task<Asset[]> GetAssets()
        {
            //return Task.FromResult(Assets.ToArray());
            return Task.FromResult(repository.GetAssets().ToArray());
        }
    }
}
