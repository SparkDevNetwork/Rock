using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace com.centralaz.Prayerbook.Utility
{
    public class GroupTypeIdByGuid
    {
        /// <summary>
        /// Returns the Id of the GroupType with the guid provided
        /// </summary>
        /// <param name="guid">The guid (as a string) of the GroupType you want the Id for.</param>
        /// <returns>The Id of the GroupType.</returns>
        public static int Get( string guid )
        {
            return new GroupTypeService( new RockContext() ).Get( Guid.Parse( guid ) ).Id;
        }
    }
}
