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
using System.Linq;

using Rock;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// LavaEndpoint Service class
    /// </summary>
    public partial class LavaEndpointService : Service<LavaEndpoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LavaEndpointService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public LavaEndpointService( RockContext context ) : base( context )
        {
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.LavaEndpoint">LavaEndpoints</see> that belong to a specified <see cref="Rock.Model.LavaApplication"/> retrieved by the LavaEndpoint's LavaApplicationId.
        /// </summary>
        /// <param name="lavaApplicationId">A <see cref="System.Int32"/> representing the LavaApplicationId of the <see cref="Rock.Model.LavaApplication"/> to retrieve <see cref="Rock.Model.LavaEndpoint">LavaEndPoints</see> for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.LavaEndpoint">LavaEndpoints</see> that belong to the specified <see cref="Rock.Model.LavaApplication"/>. The <see cref="Rock.Model.LavaEndpoint">LavaEndpoints</see> will 
        /// be ordered by the <see cref="LavaEndpoint">LavaEndpoint's</see> LavaApplicationId property.</returns>
        public IOrderedQueryable<LavaEndpoint> GetByLavApplicationId( int lavaApplicationId )
        {
            return Queryable()
                .Where( t => t.LavaApplicationId == lavaApplicationId )
                .OrderBy( t => t.LavaApplicationId );
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( LavaEndpoint item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        #region IHasAdditionalSettings Models

        /// <summary>
        /// Endpoint intent settings.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        public class EndpointSettings
        {
            /// <summary>
            /// Determines if cross-site forgery protection should be enabled.
            /// </summary>
            public bool EnableCrossSiteForgeryProtection { get; set; } = true;
        }

        #endregion IHasAdditionalSettings Models
    }


    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class LavaEndpointExtensionMethods
    {
        /// <summary>
        /// Clones this LavaEndpoint object to a new LavaEndpoint object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static LavaEndpoint Clone( this LavaEndpoint source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as LavaEndpoint;
            }
            else
            {
                var target = new LavaEndpoint();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this LavaEndpoint object to a new LavaEndpoint object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static LavaEndpoint CloneWithoutIdentity( this LavaEndpoint source )
        {
            var target = new LavaEndpoint();
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
            target.CacheControlHeaderSettings = null;
            target.RateLimitPeriodDurationSeconds = null;
            target.RateLimitRequestPerPeriod = null;

            return target;
        }

        /// <summary>
        /// Copies the properties from another LavaEndpoint object to this LavaEndpoint object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this LavaEndpoint target, LavaEndpoint source )
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Description = source.Description;
            target.IsActive = source.IsActive;
            target.IsSystem = source.IsSystem;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.HttpMethod = source.HttpMethod;
            target.EnabledLavaCommands = source.EnabledLavaCommands;
            target.LavaApplicationId = source.LavaApplicationId;
            target.Slug = source.Slug;
            target.AdditionalSettingsJson = source.AdditionalSettingsJson;
            target.CodeTemplate = source.CodeTemplate;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
            target.RateLimitRequestPerPeriod = source.RateLimitRequestPerPeriod;
            target.RateLimitPeriodDurationSeconds = source.RateLimitPeriodDurationSeconds;
            target.SecurityMode = source.SecurityMode;
        }
    }

}