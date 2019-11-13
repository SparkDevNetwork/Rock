using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for Related Entity
    /// </summary>
    public partial class RelatedEntityService
    {
        /// <summary>
        /// Returns a Queryable of related entity on the basis of purpose key.
        /// </summary>
        /// <param name="purposeKey">The purpose key.</param>
        public IQueryable<RelatedEntity> GetByPurposeKey( string purposeKey )
        {
            var query = Queryable();

            if ( purposeKey.IsNullOrWhiteSpace() )
            {
                query = query.Where( a => string.IsNullOrEmpty( a.PurposeKey ) );
            }
            else
            {
                query = query.Where( a => a.PurposeKey == purposeKey );
            }

            return query;
        }
    }
}
