﻿// <copyright>
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

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Rock.Cms;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RestController
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return RestControllerCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            RestControllerCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

        #region ISecured

        /// <inheritdoc/>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var metadata = GetMetadata();

                if ( metadata?.SupportedActions != null )
                {
                    return new Dictionary<string, string>( metadata.SupportedActions );
                }

                return CalculateSupportedActions( metadata, out _ );
            }
        }

        /// <inheritdoc/>
        public override ISecured ParentAuthority
        {
            get
            {
                var metadata = GetMetadata();

                // Version 2 endpoints are restricted by default. Explicit
                // permissions must be granted to each controller or endpoint.
                if ( metadata?.Version == 2 )
                {
                    return new GlobalRestrictedDefault();
                }

                return base.ParentAuthority;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the support actions of this controller.
        /// </summary>
        /// <param name="metadata">The existing metadata that has been calculated on this controller.</param>
        /// <param name="controllerType">On return this will contain the controller type if one was found.</param>
        /// <returns>A dictionary of the supported actions.</returns>
        internal Dictionary<string, string> CalculateSupportedActions( RestControllerMetadata metadata, out System.Type controllerType )
        {
            var actions = new Dictionary<string, string>( base.SupportedActions );

            if ( metadata?.Version == 2 )
            {
                // We need to add new items before we remove the old ones
                // otherwise the order gets screwed up.
                actions.TryAdd( Authorization.EXECUTE_READ, "Allows execution of API endpoints in the context of viewing data." );
                actions.TryAdd( Authorization.EXECUTE_WRITE, "Allows execution of API endpoints in the context of editing data." );
                actions.TryAdd( Authorization.EXECUTE_UNRESTRICTED_READ, "Allows execution of API endpoints in the context of viewing data without performing per-entity security checks." );
                actions.TryAdd( Authorization.EXECUTE_UNRESTRICTED_WRITE, "Allows execution of API endpoints in the context of editing data without performing per-entity security checks." );

                actions.Remove( Authorization.VIEW );
                actions.Remove( Authorization.EDIT );

                // Use a new dictionary to force remove the holes left behind
                // after we removed the VIEW and EDIt permissions so any Adds
                // below go in the right place.
                actions = new Dictionary<string, string>( actions );
            }

            controllerType = Reflection.FindType( typeof( object ), ClassName );

            if ( controllerType != null )
            {
                foreach ( var sa in controllerType.GetCustomAttributes<SecurityActionAttribute>() )
                {
                    actions.TryAdd( sa.Action, sa.Description );
                }

                foreach ( var action in controllerType.GetCustomAttributes<ExcludeSecurityActionsAttribute>().SelectMany( esa => esa.Actions ) )
                {
                    actions.Remove( action );
                }
            }

            return actions;
        }

        /// <summary>
        /// Gets the metadata for this instance.
        /// </summary>
        /// <returns>An instance of <see cref="RestControllerMetadata"/> or <c>null</c>.</returns>
        internal RestControllerMetadata GetMetadata()
        {
            return this.GetAdditionalSettings<RestControllerMetadata>();
        }

        /// <summary>
        /// Updates this instance to use the specified metadata.
        /// </summary>
        /// <param name="data">The metadata or <c>null</c>.</param>
        internal void SetMetadata( RestControllerMetadata data )
        {
            this.SetAdditionalSettings( data );
        }

        #endregion
    }
}
