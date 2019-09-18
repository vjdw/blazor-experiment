using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace blazor_experiment.Data
{
    public class Repository
    {
        private LiteDatabase db;

        public Repository(LiteDatabase db)
        {
            this.db = db;
            AddAsset(new Asset {Path="cat"});
            AddAsset(new Asset {Path="dog"});
        }

        public void AddAsset(Asset asset)
        {
            var assets = db.GetCollection<Asset>("assets");
            assets.Insert(asset);
        }

        public IEnumerable<Asset> GetAssets()
        {
            return db.GetCollection<Asset>("assets").FindAll();
        }
    }
}
