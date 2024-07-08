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

using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// Defines filter methods available for use with the Lava library.
    /// </summary>
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Converts one or more Entity Guid references to the corresponding Entity Id values.
        /// </summary>
        /// <param name="context">The Lava render context.</param>
        /// <param name="input">A single Guid, an array of Guid values, or a comma-delimited list of Guid values.</param>
        /// <param name="entityType">The Entity Type of the supplied Guid, specified as an Id value, a Guid value or a Name.</param>
        /// <returns></returns>
        public static object GuidToId( ILavaRenderContext context, object input, string entityType )
        {
            if ( input == null )
            {
                return input;
            }

            var returnTypeIsCollection = true;

            // Parse the input as a Guid collection.
            List<Guid> entityGuidList;
            bool isValidInput;
            if ( input is IEnumerable<object> inputList )
            {
                isValidInput = InputParser.TryConvertToGuidList( inputList, out entityGuidList );
            }
            else
            {
                var inputString = input.ToString().Trim();

                isValidInput = InputParser.TryConvertToGuidList( input.ToString(), out entityGuidList, "," );

                if ( !inputString.Contains( "," ) )
                {
                    returnTypeIsCollection = false;
                }
            }

            if ( !isValidInput )
            {
                throw LavaElementRenderException.New( nameof( GuidToId ), "Invalid Input Guid Value." )
                    .WithParameter( "Input", input.ToString() );
            }

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            EntityTypeCache entityTypeCache = null;

            // Parse the entity type parameter.
            var inputInteger = InputParser.ConvertToIntegerOrDefault( entityType, null, null );
            if ( inputInteger != null )
            {
                entityTypeCache = EntityTypeCache.Get( inputInteger.Value, rockContext );
            }
            else
            {
                // Parse input as Guid value.
                Guid? entityTypeGuid;
                var isGuid = InputParser.TryConvertToNullableGuid( entityType, out entityTypeGuid );
                if ( isGuid )
                {
                    entityTypeCache = EntityTypeCache.Get( entityTypeGuid.Value, rockContext );
                }
                else if ( entityType.IsNotNullOrWhiteSpace() )
                {
                    // Parse input as Name (or FriendlyName).
                    var inputName = entityType.Trim();
                    if ( inputName.Contains( "." ) )
                    {
                        // Assume the provided name is the fully qualified domain name.
                        entityTypeCache = EntityTypeCache.Get( inputName, false, rockContext );
                    }
                    else
                    {
                        // Assume the provided name is the friendly name.
                        var inputNameLower = inputName.RemoveSpaces().ToLower();
                        entityTypeCache = EntityTypeCache.All()
                            .Where( et => et.IsEntity )
                            .FirstOrDefault( et => et.FriendlyName.RemoveSpaces().ToLower() == inputNameLower );
                    }
                }
            }

            if ( entityTypeCache == null )
            {
                throw LavaElementRenderException.New( nameof( GuidToId ), "Invalid Entity Type." )
                    .WithParameter( "entityType", entityType );
            }

            // Get the Id Values associated with the list of Guids.
            var entityGuidStringList = entityGuidList.Select( g => g.ToString() ).ToList();

            var entityIdList = Reflection.GetEntityIdsForEntityType( entityTypeCache,
                entityGuidStringList,
                allowIntegerIdentifier: false,
                dbContext: rockContext );

            if ( returnTypeIsCollection )
            {
                // Return a collection of Guids in the same order as the input list.
                var idList = new List<string>();
                foreach ( var guid in entityGuidStringList )
                {
                    if ( entityIdList.ContainsKey( guid ) )
                    {
                        idList.Add( entityIdList[guid].ToString() );
                    }
                    else
                    {
                        idList.Add( "0" );
                    }
                }
                return idList;
            }
            else
            {
                if ( entityIdList.Any() )
                {
                    return entityIdList.First().Value;
                }

                return null;
            }
        }
    }
}
