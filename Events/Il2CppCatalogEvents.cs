using System;

namespace AssetExporter
{
    public sealed class Il2CppCatalogExportedEvent : IModEvent
    {
        public Il2CppCatalogExportedEvent(DateTime occurredAtUtc, string filePath, int totalEntries)
        {
            OccurredAtUtc = occurredAtUtc;
            FilePath = filePath;
            TotalEntries = totalEntries;
        }

        public DateTime OccurredAtUtc { get; }
        public string FilePath { get; }
        public int TotalEntries { get; }
    }

    public sealed class Il2CppGameplayIndexExportedEvent : IModEvent
    {
        public Il2CppGameplayIndexExportedEvent(DateTime occurredAtUtc, string filePath)
        {
            OccurredAtUtc = occurredAtUtc;
            FilePath = filePath;
        }

        public DateTime OccurredAtUtc { get; }
        public string FilePath { get; }
    }
}
