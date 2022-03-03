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

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

namespace Rock.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Collection of utility methods to help in Form Builder logic.
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// Gets the unique identifier from the <see cref="DefinedValue"/>
        /// identifier.
        /// </summary>
        /// <param name="id">The <see cref="DefinedValue"/> identifier.</param>
        /// <returns>The unique identifier or <c>null</c> if not found.</returns>
        internal static Guid? GetDefinedValueGuid( int? id )
        {
            if ( !id.HasValue )
            {
                return null;
            }

            return DefinedValueCache.Get( id.Value )?.Guid;
        }

        /// <summary>
        /// Gets the identifier from the <see cref="DefinedValue"/> unique
        /// identifier.
        /// </summary>
        /// <param name="guid">The <see cref="DefinedValue"/> unique identifier.</param>
        /// <returns>The identifier or <c>null</c> if not found.</returns>
        internal static int? GetDefinedValueId( Guid? guid )
        {
            if ( !guid.HasValue )
            {
                return null;
            }

            return DefinedValueCache.Get( guid.Value )?.Id;
        }

        /// <summary>
        /// Gets the e-mail template option choices available to the individual.
        /// </summary>
        /// <param name="rockContext">The database context to use for data lookup.</param>
        /// <returns>A collection of view models that represent the e-mail templates.</returns>
        internal static List<ListItemViewModel> GetEmailTemplateOptions( RockContext rockContext, RockRequestContext requestContext )
        {
            return new SystemCommunicationService( rockContext )
                .Queryable()
                .Where( c => c.IsActive == true )
                .ToList()
                .Where( c => c.IsAuthorized( Rock.Security.Authorization.VIEW, requestContext.CurrentPerson ) )
                .OrderBy( c => c.Title )
                .Select( c => new ListItemViewModel
                {
                    Value = c.Guid.ToString(),
                    Text = c.Title
                } )
                .ToList();
        }
    }
}
