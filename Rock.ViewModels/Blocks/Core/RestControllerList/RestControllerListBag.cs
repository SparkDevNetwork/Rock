using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.RestControllerList
{
    public class RestControllerListBag : EntityBagBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public int Actions { get; set; }
        public int ActionsWithPublicCachingHeaders { get; set; }
    }
}


