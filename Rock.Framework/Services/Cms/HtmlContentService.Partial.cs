using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

using Rock.Models.Cms;

namespace Rock.Services.Cms
{
    public partial class HtmlContentService
    {
        /// <summary>
        /// Gets the active content by block id and entity value.
        /// </summary>
        /// <param name="blockId">The block id.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        public Rock.Models.Cms.HtmlContent GetActiveContentByBlockKey( int blockId, string entityValue )
        {
            // return the most recently approved item
            IQueryable<HtmlContent> content = Repository.AsQueryable().Where( c => c.BlockId == blockId && c.Approved == true && c.EntityValue == entityValue );

            return content.OrderBy( c => c.Id ).FirstOrDefault();
        }
    }
}
