namespace api.Models
{
    public class ImportRequestObject
    {
        public string DbName { get; init; } = "";
        public List<string> CheckedCollectionNames { get; init; } = [];
        public Dictionary<string, string> CollectionNameChanges { get; init; } = [];
        public bool AdoptOid { get; init; } = false;
        public string FileName { get; init; } = "";
        public Guid Guid { get; init; } = Guid.Empty;
    }

}
