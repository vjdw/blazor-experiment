using LiteDB;
using System;

namespace blazor_experiment.Data
{
    public class Asset
    {
        [BsonId]
        public string Path { get; set; }
        public string Guid { get; set; } = System.Guid.NewGuid().ToString("N");
    }

    public class Thumbnail
    {
        [BsonId]
        public string AssetGuid { get; set; }
        public byte[] Raw { get; set; }
    }
}
