using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    public partial class PersonBookmarkService : Service<PersonBookmark>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBookmark"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public PersonBookmarkService( RockContext context ) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( PersonBookmark item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class PersonBookmarkExtensionMethods
    {
        /// <summary>
        /// Clones this PersonBookmark object to a new PersonBookmark object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static PersonBookmark Clone( this PersonBookmark source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as PersonBookmark;
            }
            else
            {
                var target = new PersonBookmark();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another PersonBookmark object to this PersonBookmark object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this PersonBookmark target, PersonBookmark source )
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Url = source.Url;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.PersonAliasId = source.PersonAliasId;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
