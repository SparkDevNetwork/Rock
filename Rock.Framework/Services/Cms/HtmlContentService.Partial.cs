using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Rock.Services.Cms
{
    public partial class HtmlContentService
    {
        public Rock.Models.Cms.HtmlContent GetActiveContentByBlockKey( int BlockId )
        {
            // return the most recently approved item
            return _repository.AsQueryable().Where( t => t.BlockId == BlockId && t.Approved == true ).OrderBy( t => t.Id ).FirstOrDefault();
        }
    }
}
