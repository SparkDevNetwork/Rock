// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// LavaApplication Service class
    /// </summary>
    public partial class LavaApplicationService : Service<LavaApplication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LavaApplicationService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public LavaApplicationService( RockContext context ) : base( context )
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
        public bool CanDelete( LavaApplication item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets the Guid for the DefinedType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = LavaApplicationCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;

        }
    }


    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class LavaApplicationExtensionMethods
    {
        /// <summary>
        /// Clones this LavaApplication object to a new LavaApplication object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static LavaApplication Clone( this LavaApplication source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as LavaApplication;
            }
            else
            {
                var target = new LavaApplication();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this LavaApplication object to a new LavaApplication object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static LavaApplication CloneWithoutIdentity( this LavaApplication source )
        {
            var target = new LavaApplication();
            target.CopyPropertiesFrom( source );

            target.Id = 0;
            target.Guid = Guid.NewGuid();
            target.ForeignKey = null;
            target.ForeignId = null;
            target.ForeignGuid = null;
            target.CreatedByPersonAliasId = null;
            target.CreatedDateTime = RockDateTime.Now;
            target.ModifiedByPersonAliasId = null;
            target.ModifiedDateTime = RockDateTime.Now;

            return target;
        }

        /// <summary>
        /// Copies the properties from another LavaApplication object to this LavaApplication object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this LavaApplication target, LavaApplication source )
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Description = source.Description;
            target.IsActive = source.IsActive;
            target.IsSystem = source.IsSystem;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.Slug = source.Slug;
            target.AdditionalSettingsJson = source.AdditionalSettingsJson;
            target.ConfigurationRiggingJson = source.ConfigurationRiggingJson;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
        }
    }

}