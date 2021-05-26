namespace System.Data.Entity.Migrations.Infrastructure
{
    public interface IMigrationMetadata
    {
        string Id { get; }

        string Source { get; }

        string Target { get; }
    }
}
