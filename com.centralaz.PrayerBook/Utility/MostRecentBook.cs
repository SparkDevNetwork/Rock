using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.Data;

namespace com.centralaz.Prayerbook.Utility
{
    public class MostRecentBook
    {
        /// <summary>
        /// Gets the most recent book entry in the db.
        /// </summary>
        /// <returns>a Book object.</returns>
        public static Group Get(RockContext context)
        {
            int groupTypeId = GroupTypeIdByGuid.Get( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE );
            
            Group book = new GroupService(context).Queryable()
                .Where( t => t.GroupTypeId == groupTypeId )
                .OrderByDescending( t => t.CreatedDateTime )
                .FirstOrDefault();

            if ( book == null )
            {
                book = new Group();
                book.GroupTypeId = groupTypeId;
            }
            return book;
        }
    }
}
