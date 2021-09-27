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

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.ViewModel.Client
{
    /// <summary>
    /// Various methods to retrieve data that will be sent to the client. This
    /// applies standard security and filter options to help ensure a consistent
    /// experience between implementations.
    /// </summary>
    internal sealed class ClientHelper
    {
        #region Default Options

        // NOTE: These are here to save a few CPU cycles since these methods
        // will be called fairly regularly we don't want to create an empty
        // instance on every call if we don't need to.

        /// <summary>
        /// The default defined value options.
        /// </summary>
        private static readonly DefinedValueOptions DefaultDefinedValueOptions = new DefinedValueOptions();

        /// <summary>
        /// The default campus options.
        /// </summary>
        private static readonly CampusOptions DefaultCampusOptions = new CampusOptions();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether security checks are enabled.
        /// When enabled, any entity that implements <see cref="ISecured"/> will
        /// be checked to see if the person has view access to it.
        /// </summary>
        /// <value>
        ///   <c>true</c> if security checks are enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSecurity { get; set; } = true;

        #endregion

        #region Fields

        /// <summary>
        /// The rock context to use when accessing the database.
        /// </summary>
        private readonly RockContext _rockContext;

        /// <summary>
        /// The person to use for security checks.
        /// </summary>
        private readonly Person _person;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Rock.ViewModel.Client.ClientHelper" /> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks.</param>
        public ClientHelper( RockContext rockContext, Person person )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _person = person;
        }

        #endregion

        #region Client Data Methods

        /// <summary>
        /// Gets the defined values for the specified defined type as list items
        /// that can be sent to the client.
        /// </summary>
        /// <param name="definedTypeId">The defined type identifier.</param>
        /// <param name="options">The options that specify the behavior of which items to include.</param>
        /// <returns>A list of <see cref="ListItemViewModel"/> that represent the defined values.</returns>
        public List<ListItemViewModel> GetDefinedValuesAsListItems( int definedTypeId, DefinedValueOptions options = null )
        {
            var definedType = DefinedTypeCache.Get( definedTypeId, _rockContext );

            if ( definedType == null )
            {
                return new List<ListItemViewModel>();
            }

            return this.GetDefinedValuesAsListItems( definedType, options );
        }

        /// <summary>
        /// Gets the defined values for the specified defined type as list items
        /// that can be sent to the client.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="options">The options that specify the behavior of which items to include.</param>
        /// <returns>A list of <see cref="ListItemViewModel"/> that represent the defined values.</returns>
        public List<ListItemViewModel> GetDefinedValuesAsListItems( Guid definedTypeGuid, DefinedValueOptions options = null )
        {
            var definedType = DefinedTypeCache.Get( definedTypeGuid, _rockContext );

            if ( definedType == null )
            {
                return new List<ListItemViewModel>();
            }

            return GetDefinedValuesAsListItems( definedType, options );
        }

        /// <summary>
        /// Gets the defined values for the specified defined type as list items
        /// that can be sent to the client.
        /// </summary>
        /// <param name="definedType">The defined type.</param>
        /// <param name="options">The options that specify the behavior of which items to include.</param>
        /// <returns>A list of <see cref="ListItemViewModel"/> that represent the defined values.</returns>
        public List<ListItemViewModel> GetDefinedValuesAsListItems( DefinedTypeCache definedType, DefinedValueOptions options = null )
        {
            IEnumerable<DefinedValueCache> source = definedType.DefinedValues;

            options = options ?? DefaultDefinedValueOptions;

            if ( !options.IncludeInactive )
            {
                source = source.Where( v => v.IsActive );
            }

            source = CheckSecurity( source );

            return source.OrderBy( v => v.Order )
                .Select( v => new ListItemViewModel
                {
                    Value = v.Guid.ToString(),
                    Text = options.UseDescription ? v.Description : v.Value
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the campuses that can be sent to a client.
        /// </summary>
        /// <param name="options">The options that specify which campuses to include.</param>
        /// <returns>A list of <see cref="ListItemViewModel"/> that represent the campus values.</returns>
        public List<ListItemViewModel> GetCampusesAsListItems( CampusOptions options = null )
        {
            IEnumerable<CampusCache> source = CampusCache.All( _rockContext );

            options = options ?? DefaultCampusOptions;

            // Exclude inactive campuses unless we were asked to include them.
            if ( !options.IncludeInactive )
            {
                source = source.Where( c => c.IsActive == true );
            }

            // If they specified any campus types then limit the results to
            // only those campuses that fit the criteria.
            if ( options.LimitCampusTypes != null && options.LimitCampusTypes.Count > 0 )
            {
                source = source.Where( c =>
                {
                    if ( !c.CampusTypeValueId.HasValue )
                    {
                        return false;
                    }

                    var definedValueCache = DefinedValueCache.Get( c.CampusTypeValueId.Value );

                    return definedValueCache != null && options.LimitCampusTypes.Contains( definedValueCache.Guid );
                } );
            }

            // If they specified any campus status then limit the results to
            // only those campuses taht fit the criteria.
            if ( options.LimitCampusStatuses != null && options.LimitCampusStatuses.Count > 0 )
            {
                source = source.Where( c =>
                {
                    if ( !c.CampusStatusValueId.HasValue )
                    {
                        return false;
                    }

                    var definedValueCache = DefinedValueCache.Get( c.CampusStatusValueId.Value );

                    return definedValueCache != null && options.LimitCampusStatuses.Contains( definedValueCache.Guid );
                } );
            }

            source = CheckSecurity( source );

            return source.OrderBy( c => c.Order )
                .Select( c => new ListItemViewModel()
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name
                } ).ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks the security of the entities to ensure the person has access
        /// to view them. If <see cref="EnableSecurity"/> is <c>false</c> then
        /// this method simply returns the original enumerable. Any entities
        /// that the person does not have access to are filtered out.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="enumerable">The enumerable to be filtered.</param>
        /// <returns>An enumerable of entities which the person has view access to.</returns>
        private IEnumerable<TEntity> CheckSecurity<TEntity>( IEnumerable<TEntity> enumerable )
            where TEntity : ISecured
        {
            if ( !EnableSecurity )
            {
                return enumerable;
            }

            // Only do the ToList() call if we were passed a queryable since
            // we can't do IsAuthorized() checks on database queries. This
            // provides a small performance boost.
            if ( enumerable is IQueryable )
            {
                return enumerable.ToList()
                    .Where( e => e.IsAuthorized( Authorization.VIEW, _person ) );
            }

            return enumerable.Where( e => e.IsAuthorized( Authorization.VIEW, _person ) );
        }

        #endregion
    }
}
