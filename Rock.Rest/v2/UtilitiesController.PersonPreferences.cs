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
using System.Collections.Generic;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Utility;
using Rock.ViewModels.Core;
using Rock.ViewModels.Rest.Utilities;
using Rock.Web.Cache;

namespace Rock.Rest.v2
{
    public partial class UtilitiesController : ApiControllerBase
    {
        #region Person Prefernce API Methods

        /// <summary>
        /// Retrieves all preferences for the current person that are not
        /// attached to any specified entity.
        /// </summary>
        /// <returns>A collection of preference values.</returns>
        [Authenticate]
        [HttpGet]
        [System.Web.Http.Route( "api/v2/Utilities/PersonPreferences" )]
        [Rock.SystemGuid.RestActionGuid( "DCE21AB4-5A17-4C16-949B-4977265D48F0" )]
        public IHttpActionResult GetPersonPreferences()
        {
            var preferenceCollection = GetPersonOrVisitorPreferences( null, null );

            return Ok( preferenceCollection.GetAllValueBags() );
        }

        /// <summary>
        /// <para>
        /// Creates or updates a set of preferences for the current person
        /// that are not attached to a specific entity.
        /// </para>
        /// <para>
        /// Setting a value to an empty string will cause it to be deleted.
        /// </para>
        /// </summary>
        /// <param name="options">The options containing the preferences to be set.</param>
        /// <returns>A response that indicates if the preferences were set.</returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/v2/Utilities/PersonPreferences" )]
        [Rock.SystemGuid.RestActionGuid( "9117FB2B-C07C-4FB2-8409-37F596D6887F" )]
        public IHttpActionResult PostPersonPreferences( [FromBody] UpdatePersonPreferencesOptionsBag options )
        {
            var preferenceCollection = GetPersonOrVisitorPreferences( null, null );

            foreach ( var pref in options.Preferences )
            {
                preferenceCollection.SetValue( pref.Key, pref.Value );
            }

            preferenceCollection.Save();

            return Ok();
        }

        /// <summary>
        /// Updates the LastAccessedDateTime property for the specified keys.
        /// This will apply to the preferences for the current person that
        /// are not attached to a specific entity.
        /// </summary>
        /// <param name="options">The options describing which preferences were accessed.</param>
        /// <returns>A response that indicates if the request was successful.</returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/v2/Utilities/PersonPreferencesAccessed" )]
        [Rock.SystemGuid.RestActionGuid( "B2E6543E-FDE8-43B4-A563-162FB334800A" )]
        public IHttpActionResult PostPersonPreferencesAccessed( [FromBody] UpdatePersonPreferencesAccessedOptionsBag options )
        {
            var preferenceCollection = GetPersonOrVisitorPreferences( null, null );

            // Simply access all the keys. The collection will handle scheduling
            // the update to the last accessed property.
            foreach ( var key in options.Keys )
            {
                preferenceCollection.GetValue( key );
            }

            return Ok();
        }

        /// <summary>
        /// Retrieves all preferences for the current person attached to
        /// the specified entity.
        /// </summary>
        /// <param name="entityTypeKey">The identifier of the attached entity type. Can be a Guid, Id or IdKey value.</param>
        /// <param name="entityKey">The identifier of the attached entity. Can be a Guid, Id or IdKey value.</param>
        /// <returns>A collection of preference values.</returns>
        [Authenticate]
        [HttpGet]
        [System.Web.Http.Route( "api/v2/Utilities/PersonPreferences/{entityTypeKey}/{entityKey}" )]
        [Rock.SystemGuid.RestActionGuid( "A533319F-75A8-470F-95BD-691119F6B5F5" )]
        public IHttpActionResult GetPersonPreferences( string entityTypeKey, string entityKey )
        {
            EntityTypeCache entityType = null;

            // EntityTypeCache has an override on Get() that prevents the
            // standard Get method supporting IdKey from matching. Do the
            // translation manually.
            if ( int.TryParse( entityTypeKey, out var id ) )
            {
                entityType = EntityTypeCache.Get( id );
            }
            else if ( Guid.TryParse( entityTypeKey, out var guid ) )
            {
                entityType = EntityTypeCache.Get( guid );
            }
            else if ( IdHasher.Instance.TryGetId( entityKey, out id ) )
            {
                entityType = EntityTypeCache.Get( id );
            }

            if ( entityType == null )
            {
                return Ok( new List<PersonPreferenceValueBag>() );
            }

            var entityId = Reflection.GetEntityIdForEntityType( entityType.Id, entityKey, true );

            if ( !entityId.HasValue )
            {
                return Ok( new List<PersonPreferenceValueBag>() );
            }

            var preferenceCollection = GetPersonOrVisitorPreferences( entityType, entityId.Value );

            return Ok( preferenceCollection.GetAllValueBags() );
        }

        /// <summary>
        /// <para>
        /// Creates or updates a set of preferences for the current person
        /// attached to the specified entity.
        /// </para>
        /// <para>
        /// Setting a value to an empty string will cause it to be deleted.
        /// </para>
        /// </summary>
        /// <param name="entityTypeKey">The identifier of the attached entity type. Can be a Guid, Id or IdKey value.</param>
        /// <param name="entityKey">The identifier of the attached entity. Can be a Guid, Id or IdKey value.</param>
        /// <param name="options">The options containing the preferences to be set.</param>
        /// <returns>A response that indicates if the preferences were set.</returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/v2/Utilities/PersonPreferences/{entityTypeKey}/{entityKey}" )]
        [Rock.SystemGuid.RestActionGuid( "99BCED4F-5804-4E51-A88F-CD82B2FF5D18" )]
        public IHttpActionResult PostPersonPreferences( string entityTypeKey, string entityKey, [FromBody] UpdatePersonPreferencesOptionsBag options )
        {
            EntityTypeCache entityType = null;

            // EntityTypeCache has an override on Get() that prevents the
            // standard Get method supporting IdKey from matching. Do the
            // translation manually.
            if ( int.TryParse( entityTypeKey, out var id ) )
            {
                entityType = EntityTypeCache.Get( id );
            }
            else if ( Guid.TryParse( entityTypeKey, out var guid ) )
            {
                entityType = EntityTypeCache.Get( guid );
            }
            else if ( IdHasher.Instance.TryGetId( entityTypeKey, out id ) )
            {
                entityType = EntityTypeCache.Get( id );
            }

            if ( entityType == null )
            {
                return Ok();
            }

            var entityId = Reflection.GetEntityIdForEntityType( entityType.Id, entityKey, true );

            if ( !entityId.HasValue )
            {
                return Ok();
            }

            var preferenceCollection = GetPersonOrVisitorPreferences( entityType, entityId );

            foreach ( var pref in options.Preferences )
            {
                preferenceCollection.SetValue( pref.Key, pref.Value );
            }

            preferenceCollection.Save();

            return Ok();
        }

        /// <summary>
        /// Updates the LastAccessedDateTime property for the specified keys.
        /// This will apply to the preferences for the current person that
        /// are attached to the specified entity.
        /// </summary>
        /// <param name="entityTypeKey">The identifier of the attached entity type. Can be a Guid, Id or IdKey value.</param>
        /// <param name="entityKey">The identifier of the attached entity. Can be a Guid, Id or IdKey value.</param>
        /// <param name="options">The options describing which preferences were accessed.</param>
        /// <returns>A response that indicates if the request was successful.</returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/v2/Utilities/PersonPreferencesAccessed/{entityTypeKey}/{entityKey}" )]
        [Rock.SystemGuid.RestActionGuid( "511F4E1B-B583-4B3E-9E84-7E7B797C38D8" )]
        public IHttpActionResult PostPersonPreferencesAccessed( string entityTypeKey, string entityKey, [FromBody] UpdatePersonPreferencesAccessedOptionsBag options )
        {
            EntityTypeCache entityType = null;

            // EntityTypeCache has an override on Get() that prevents the
            // standard Get method supporting IdKey from matching. Do the
            // translation manually.
            if ( int.TryParse( entityTypeKey, out var id ) )
            {
                entityType = EntityTypeCache.Get( id );
            }
            else if ( Guid.TryParse( entityTypeKey, out var guid ) )
            {
                entityType = EntityTypeCache.Get( guid );
            }
            else if ( IdHasher.Instance.TryGetId( entityTypeKey, out id ) )
            {
                entityType = EntityTypeCache.Get( id );
            }

            if ( entityType == null )
            {
                return Ok();
            }

            var entityId = Reflection.GetEntityIdForEntityType( entityType.Id, entityKey, true );

            if ( !entityId.HasValue )
            {
                return Ok();
            }

            var preferenceCollection = GetPersonOrVisitorPreferences( entityType, entityId );

            // Simply access all the keys. The collection will handle scheduling
            // the update to the last accessed property.
            foreach ( var key in options.Keys )
            {
                preferenceCollection.GetValue( key );
            }

            return Ok();
        }

        /// <summary>
        /// Retrieves all preferences for the current person attached to
        /// the specified block.
        /// </summary>
        /// <param name="blockKey">The identifier of the block. Can be a Guid, Id or IdKey value.</param>
        /// <returns>A collection of preference values.</returns>
        [Authenticate]
        [HttpGet]
        [System.Web.Http.Route( "api/v2/Utilities/PersonBlockPreferences/{blockKey}" )]
        [Rock.SystemGuid.RestActionGuid( "6CC39A4D-784F-4E07-A943-1FEC145387CF" )]
        public IHttpActionResult GetPersonBlockPreferences( string blockKey )
        {
            var block = BlockCache.Get( blockKey, true );

            if ( block == null )
            {
                return Ok( new List<PersonPreferenceValueBag>() );
            }

            var preferenceCollection = GetPersonOrVisitorPreferences( EntityTypeCache.Get<Block>(), block.Id );

            return Ok( preferenceCollection.GetAllValueBags() );
        }

        /// <summary>
        /// <para>
        /// Creates or updates a set of preferences for the current person
        /// attached to the specified block.
        /// </para>
        /// <para>
        /// Setting a value to an empty string will cause it to be deleted.
        /// </para>
        /// </summary>
        /// <param name="blockKey">The identifier of the block. Can be a Guid, Id or IdKey value.</param>
        /// <param name="options">The options containing the preferences to be set.</param>
        /// <returns>A response that indicates if the preferences were set.</returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/v2/Utilities/PersonBlockPreferences/{blockKey}" )]
        [Rock.SystemGuid.RestActionGuid( "9354C523-8632-4AEC-8917-3FDE432967ED" )]
        public IHttpActionResult PostPersonBlockPreferences( string blockKey, [FromBody] UpdatePersonPreferencesOptionsBag options )
        {
            var block = BlockCache.Get( blockKey, true );

            if ( options.Preferences.Count == 0 || block == null )
            {
                return Ok();
            }

            var preferenceCollection = GetPersonOrVisitorPreferences( EntityTypeCache.Get<Block>(), block.Id );

            foreach ( var pref in options.Preferences )
            {
                preferenceCollection.SetValue( pref.Key, pref.Value );
            }

            preferenceCollection.Save();

            return Ok();
        }

        /// <summary>
        /// Updates the LastAccessedDateTime property for the specified keys.
        /// This will apply to the preferences for the current person that
        /// are attached to the specified block.
        /// </summary>
        /// <param name="blockKey">The identifier of the block. Can be a Guid, Id or IdKey value.</param>
        /// <param name="options">The options describing which preferences were accessed.</param>
        /// <returns>A response that indicates if the request was successful.</returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/v2/Utilities/PersonBlockPreferencesAccessed/{blockKey}" )]
        [Rock.SystemGuid.RestActionGuid( "6681241B-F882-4334-B6DF-C04E931E7E02" )]
        public IHttpActionResult PostPersonBlockPreferencesAccessed( string blockKey, [FromBody] UpdatePersonPreferencesAccessedOptionsBag options )
        {
            var block = BlockCache.Get( blockKey, true );

            if ( block == null )
            {
                return Ok();
            }

            var preferenceCollection = GetPersonOrVisitorPreferences( EntityTypeCache.Get<Block>(), block.Id );

            // Simply access all the keys. The collection will handle scheduling
            // the update to the last accessed property.
            foreach ( var key in options.Keys )
            {
                preferenceCollection.GetValue( key );
            }

            return Ok();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper method to get the person or visitor preferences depending on
        /// the logged in state of the request.
        /// </summary>
        /// <param name="entityTypeCache">The entity type cache.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/>.</returns>
        private PersonPreferenceCollection GetPersonOrVisitorPreferences( EntityTypeCache entityTypeCache, int? entityId )
        {
            if ( entityTypeCache == null || !entityId.HasValue )
            {
                return RockRequestContext.GetGlobalPersonPreferences();
            }
            else
            {
                if ( RockRequestContext.CurrentVisitorId.HasValue )
                {
                    return PersonPreferenceCache.GetVisitorPreferenceCollection( RockRequestContext.CurrentVisitorId.Value, entityTypeCache, entityId.Value );
                }
                else if ( RockRequestContext.CurrentPerson != null )
                {
                    return PersonPreferenceCache.GetPersonPreferenceCollection( RockRequestContext.CurrentPerson, entityTypeCache, entityId.Value );
                }
                else
                {
                    return new PersonPreferenceCollection();
                }
            }
        }

        #endregion
    }
}
