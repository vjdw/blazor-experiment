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
            var assets = db.GetCollection<Asset>("assets");
            assets.EnsureIndex(_ => _.Path);

            var thumbnails = db.GetCollection<Thumbnail>("thumbnails");
            thumbnails.EnsureIndex(_ => _.AssetGuid);
        }

        public bool AssetExists(string path)
        {
            var assets = db.GetCollection<Asset>("assets");
            return assets.Exists(_ => _.Path == path);
        }

        public void AddAsset(Asset asset, Thumbnail thumbnail)
        {
            var assets = db.GetCollection<Asset>("assets");
            assets.Insert(asset);

            var thumbnails = db.GetCollection<Thumbnail>("thumbnails");
            thumbnails.Insert(thumbnail);
        }

        public IEnumerable<Asset> GetAssets()
        {
            return db.GetCollection<Asset>("assets").FindAll();
        }

        public void DeleteAllAssets()
        {
            db.DropCollection("assets");
        }

        public byte[] GetThumbnail(string assetGuid)
        {
            var thumbnails = db.GetCollection<Thumbnail>("thumbnails");

            return thumbnails.FindOne(_ => _.AssetGuid == assetGuid).Raw;
        }
    }
}
