namespace EFCore.Migrations.Triggers.Models
{
    public abstract record TriggerObject
    {
        public string Name { get; init; }

        public string Table { get; init; }

        public string Body { get; init; }
    }
}
