using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.PastoralCare.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class CareTypeItemService : Service<CareTypeItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareTypeItemService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CareTypeItemService( RockContext context ) : base( context ) { }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( CareTypeItem item, out string errorMessage )
        {
            errorMessage = string.Empty;

            return true;
        }
    }
    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class CareTypeItemExtensionMethods
    {
        /// <summary>
        /// Clones this CareTypeItem object to a new CareTypeItem object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static CareTypeItem Clone( this CareTypeItem source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as CareTypeItem;
            }
            else
            {
                var target = new CareTypeItem();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another CareTypeItem object to this CareTypeItem object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this CareTypeItem target, CareTypeItem source )
        {
            target.Id = source.Id;
            target.CareTypeId = source.CareTypeId;
            target.CareItemId = source.CareItemId;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
