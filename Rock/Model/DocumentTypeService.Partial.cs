using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DocumentTypeService
    {
        /// <summary>
        /// Gets the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <returns></returns>
        public IQueryable<DocumentType> Get( int entityTypeId, string entityQualifierColumn, string entityQualifierValue )
        {
            var qry = Queryable().Where( d => d.EntityTypeId == entityTypeId );

            if ( entityQualifierColumn.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( t => t.EntityTypeQualifierColumn == entityQualifierColumn );
            }

            if ( entityQualifierValue.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( t => t.EntityTypeQualifierValue == entityQualifierValue );
            }

            return qry.OrderBy( t => t.Name );
        }
    }
}
