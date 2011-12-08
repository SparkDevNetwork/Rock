//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Linq;

namespace Rock.CMS
{
    public partial class HtmlContentService
    {
        /// <summary>
        /// Gets the active content by block id and entity value.
        /// </summary>
        /// <param name="blockId">The block id.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        public Rock.CMS.HtmlContent GetActiveContentByBlockKey( int blockId, string entityValue )
        {
            // return the most recently approved item
            IQueryable<HtmlContent> content = Repository.AsQueryable().Where( c => c.BlockId == blockId && c.Approved == true && c.EntityValue == entityValue );

            return content.OrderBy( c => c.Id ).FirstOrDefault();
        }
    }
}
