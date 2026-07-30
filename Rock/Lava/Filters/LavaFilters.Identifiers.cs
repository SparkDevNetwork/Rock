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
        /// Creates a deterministic RFC 4122 version 5 (name-based, SHA-1) Guid from the input string and
        /// the supplied namespace Guid. The same input and namespace always produce the same Guid.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This filter should NOT be added to the community Lava documentation. It is intentionally
        /// unpublished while the behavior is still being settled. That is a documentation decision only:
        /// the filter is registered globally like any other and there is no restriction on who may call it.
        /// </para>
        /// <para>
        /// The purpose is to obscure a well-known Guid, such as the organization Guid, so it can be shared
        /// externally without revealing the original value. The result is stable, so the same organization
        /// always maps to the same hashed Guid.
        /// </para>
        /// <para>
        /// Casing is normalized, so a Guid string supplied in upper, lower, or mixed case yields the same
        /// result. Other formatting differences are not normalized: a value wrapped in braces or missing its
        /// hyphens counts as a different name and produces a different Guid.
        /// </para>
        /// <para>
        /// This is a hash, not encryption. Anyone who knows the namespace and has a candidate Guid can
        /// confirm a match by hashing it, so treat the output as obfuscation rather than a secret.
        /// </para>
        /// <example><code>
        /// {% assign hashedGuid = '7e6286f7-0297-41ff-bdf6-bd5656e1bc53' | ToGuidV5:'d70b48fc-3b6a-4d05-9b0e-6bcb0d2b7a6f' %}
        /// </code></example>
        /// </remarks>
        /// <param name="input">The name to hash. Returns null when this is null or whitespace.</param>
        /// <param name="namespaceGuid">The namespace that scopes the generated Guid.</param>
        /// <returns>A version 5 Guid, or null if the input is null or whitespace.</returns>
        public static object ToGuidV5( object input, string namespaceGuid )
        {
            var name = input?.ToString();

            if ( name.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // A missing or malformed namespace would still hash to a valid-looking Guid, which would be
            // silently wrong, so fail loudly instead of returning a meaningless identifier.
            var namespaceGuidValue = namespaceGuid.AsGuidOrNull();

            if ( !namespaceGuidValue.HasValue )
            {
                throw LavaElementRenderException.New( nameof( ToGuidV5 ), "Invalid Namespace Guid Value." )
                    .WithParameter( "namespaceGuid", namespaceGuid ?? string.Empty );
            }

            return name.ToGuidV5( namespaceGuidValue.Value );
        }

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
                            .Where( et => et.IsEntity && et.FriendlyName != null )
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
