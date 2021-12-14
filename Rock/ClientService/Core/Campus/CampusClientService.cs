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

using System.Collections.Generic;
using System.Linq;

using Rock.ClientService.Core.Campus.Options;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

namespace Rock.ClientService.Core.Campus
{
    /// <summary>
    /// Provides methods to work with <see cref="Campus"/> and translate
    /// information into data that can be consumed by the clients.
    /// </summary>
    /// <seealso cref="Rock.ClientService.ClientServiceBase" />
    public class CampusClientService : ClientServiceBase
    {
        #region Default Options

        /// <summary>
        /// The default campus options.
        /// </summary>
        private static readonly CampusOptions DefaultCampusOptions = new CampusOptions();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CampusClientService"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks.</param>
        public CampusClientService( RockContext rockContext, Person person )
            : base( rockContext, person )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the campuses that can be sent to a client.
        /// </summary>
        /// <param name="options">The options that specify which campuses to include.</param>
        /// <returns>A list of <see cref="ListItemViewModel"/> that represent the campus values.</returns>
        public List<ListItemViewModel> GetCampusesAsListItems( CampusOptions options = null )
        {
            IEnumerable<CampusCache> source = CampusCache.All( RockContext );

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
            // only those campuses that fit the criteria.
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
    }
}
