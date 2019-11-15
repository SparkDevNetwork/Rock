using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    public partial class DocumentTypeService
    {
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
