using System;

namespace WirexApp.Infrastructure.CDC.Events
{
    public class CDCEvent<TData> where TData : class
    {
        public string EventId { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public CDCOperationType Operation { get; set; }

        public TData Data { get; set; }

        public TData OldData { get; set; }

        public DateTime Timestamp { get; set; }

        public string Source { get; set; }

        public int Version { get; set; }

        public CDCEvent()
        {
            EventId = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
            Source = "WirexApp.WriteService";
        }

        public static CDCEvent<TData> Create(string entityType, string entityId, TData data, int version = 0)
        {
            return new CDCEvent<TData>
            {
                EntityType = entityType,
                EntityId = entityId,
                Operation = CDCOperationType.Create,
                Data = data,
                Version = version
            };
        }

        public static CDCEvent<TData> Update(string entityType, string entityId, TData newData, TData oldData, int version)
        {
            return new CDCEvent<TData>
            {
                EntityType = entityType,
                EntityId = entityId,
                Operation = CDCOperationType.Update,
                Data = newData,
                OldData = oldData,
                Version = version
            };
        }

        public static CDCEvent<TData> Delete(string entityType, string entityId, int version)
        {
            return new CDCEvent<TData>
            {
                EntityType = entityType,
                EntityId = entityId,
                Operation = CDCOperationType.Delete,
                Version = version
            };
        }
    }

    public enum CDCOperationType
    {
        Create,
        Update,
        Delete
    }
}
