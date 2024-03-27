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
using System.Linq;
using Rock.Data;
using Rock.Reporting;

namespace Rock.Lava
{
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Gets a flag indicating if the input entity exists in the result set of the specified Data View.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The filter input, a reference to an Entity.</param>
        /// <param name="dataViewIdentifier">A reference to a Data View.</param>
        /// <returns><c>true</c> if the entity exists in the Data View result set.</returns>
        public static bool IsInDataView( ILavaRenderContext context, object input, object dataViewIdentifier )
        {
            var dataContext = LavaHelper.GetRockContextFromLavaContext( context );
            var dataView = LavaHelper.GetDataViewDefinitionFromInputParameter( dataViewIdentifier, dataContext );

            if ( dataView == null )
            {
                return false;
            }

            var qry = DataViewQueryBuilder.Instance.GetDataViewQuery( dataView );

            // Apply a filter to limit the result set to the target entity.
            if ( input is IEntity e )
            {
                qry = qry.Where( p => p.Id == e.Id );
            }
            else
            {
                // Process the input as a string, and attempt to parse as an entity reference.
                var inputAsString = input.ToStringSafe();
                var inputAsGuid = inputAsString.AsGuidOrNull();
                if ( inputAsGuid != null )
                {
                    qry = qry.Where( p => p.Guid == inputAsGuid );
                }
                else
                {
                    var inputAsInt = inputAsString.AsIntegerOrNull();
                    if ( inputAsInt != null )
                    {
                        qry = qry.Where( p => p.Id == inputAsInt.Value );
                    }
                    else
                    {
                        // The input cannot be parsed as an entity reference, so it cannot be matched.
                        return false;
                    }
                }
            }

            var exists = qry.Any();
            return exists;
        }
    }
}
