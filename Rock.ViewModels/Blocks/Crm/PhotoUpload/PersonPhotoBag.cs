using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PhotoUpload
{
    public class PersonPhotoBag
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public ListItemBag ProfilePhoto { get; set; }

        public string NoPhotoUrl { get; set; }
        public bool IsStaffMember { get; internal set; }
    }
}
