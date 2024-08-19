using System;

namespace Rock.ViewModels.Blocks.Core.TaggedItemList
{
    public class TaggedItemListBag
    {
        public int Id { get; set; }

        public int EntityTypeId { get; set; }

        public Guid EntityGuid { get; set; }

        public int EntityId { get; set; }

        public string EntityName { get; set; }

        public DateTime? CreatedDateTime { get; set; }
    }
}
