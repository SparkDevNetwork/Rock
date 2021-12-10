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
using System.Linq;

using Rock.ClientService.Core.DefinedValue.Options;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

namespace Rock.ClientService.Core.DefinedValue
{
    /// <summary>
    /// Provides methods to work with <see cref="DefinedValue"/> and translate
    /// information into data that can be consumed by the clients.
    /// </summary>
    /// <seealso cref="Rock.ClientService.ClientServiceBase" />
    public class DefinedValueClientService : ClientServiceBase
    {
        #region Default Options

        /// <summary>
        /// The default defined value options.
        /// </summary>
        private static readonly DefinedValueOptions DefaultDefinedValueOptions = new DefinedValueOptions();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueClientService"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks.</param>
        public DefinedValueClientService( RockContext rockContext, Person person )
            : base( rockContext, person )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the defined values for the specified defined type as list items
        /// that can be sent to the client.
        /// </summary>
        /// <param name="definedTypeId">The defined type identifier.</param>
        /// <param name="options">The options that specify the behavior of which items to include.</param>
        /// <returns>A list of <see cref="ListItemViewModel"/> that represent the defined values.</returns>
        public List<ListItemViewModel> GetDefinedValuesAsListItems( int definedTypeId, DefinedValueOptions options = null )
        {
            var definedType = DefinedTypeCache.Get( definedTypeId, RockContext );

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
            var definedType = DefinedTypeCache.Get( definedTypeGuid, RockContext );

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

        #endregion
    }
}
