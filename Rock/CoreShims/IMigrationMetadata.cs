namespace System.Data.Entity.Migrations.Infrastructure
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public interface IMigrationMetadata
    {
        string Id { get; }

        string Source { get; }

        string Target { get; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
