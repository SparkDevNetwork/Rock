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

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Deletes a <seealso cref="BinaryFile"/>. Use this to avoid waiting for the binary file to get deleted, which could take a little while depending on how the binary data is stored.
    /// </summary>
    public sealed class DeleteBinaryFileAttribute : BusStartedTask<DeleteBinaryFileAttribute.Message>
    {
        private const string FieldTypeIdsCacheKey = "Rock.Tasks.DeleteBinaryFileAttribute.FieldTypeIds";

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( message.BinaryFileGuid );
                if ( binaryFile == null )
                {
                    return;
                }

                var fieldTypeIds = GetFieldTypeIds( rockContext );
                var guidAsString = binaryFile.Guid.ToString();

                /*
                 * 2026-06-22 - DSH
                 * 
                 * Do not use the Checksum values to determine if the BinaryFile
                 * is still being used as an Attribute DefaultValue or AttributeValue.
                 * The Checksum values are case-sensitive which means a value
                 * may exist in a different case that will not be found. This
                 * would cause an accidental deletion that shouldn't happen.
                 */

                if ( message.UseContainsSearch )
                {
                    // If any attribute still has this file as a default value, don't delete it
                    var hasAttribute = new AttributeService( rockContext ).Queryable()
                        .Any( a => fieldTypeIds.Contains( a.FieldTypeId )
                            && a.DefaultValue.Contains( guidAsString ) );
                    if ( hasAttribute )
                    {
                        return;
                    }

                    // If any attribute value still has this file as a value, don't delete it
                    var hasAttributeValue = new AttributeValueService( rockContext ).Queryable()
                        .Any( a => fieldTypeIds.Contains( a.Attribute.FieldTypeId )
                            && a.Value.Contains( guidAsString ) );
                    if ( hasAttributeValue )
                    {
                        return;
                    }
                }
                else
                {
                    // If any attribute still has this file as a default value, don't delete it
                    var hasAttribute = new AttributeService( rockContext ).Queryable()
                        .Any( a => fieldTypeIds.Contains( a.FieldTypeId )
                            && a.DefaultValue == guidAsString );
                    if ( hasAttribute )
                    {
                        return;
                    }

                    // If any attribute value still has this file as a value, don't delete it
                    var hasAttributeValue = new AttributeValueService( rockContext ).Queryable()
                        .Any( a => fieldTypeIds.Contains( a.Attribute.FieldTypeId )
                            && a.Value == guidAsString );
                    if ( hasAttributeValue )
                    {
                        return;
                    }
                }

                binaryFileService.Delete( binaryFile );

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the field type identifiers that are used for binary files. This
        /// is cached so that it doesn't have to be queried multiple times.
        /// </summary>
        /// <param name="rockContext">The context to use if database access is required.</param>
        /// <returns>A read-only collection of field type identifiers.</returns>
        private static IReadOnlyCollection<int> GetFieldTypeIds( RockContext rockContext )
        {
            return ( IReadOnlyCollection<int> ) RockCache.GetOrAddExisting( FieldTypeIdsCacheKey, () =>
            {
                var fieldTypes = FieldTypeCache.GetMany( new Guid[]
                {
                    SystemGuid.FieldType.AUDIO_FILE.AsGuid(),
                    SystemGuid.FieldType.BACKGROUNDCHECK.AsGuid(),
                    SystemGuid.FieldType.BINARY_FILE.AsGuid(),
                    SystemGuid.FieldType.FILE.AsGuid(),
                    SystemGuid.FieldType.IMAGE.AsGuid(),
                    SystemGuid.FieldType.VIDEO_FILE.AsGuid(),
                }, rockContext );

                return fieldTypes.Select( f => f.Id ).ToList();
            } );
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the binary file unique identifier.
            /// </summary>
            /// <value>
            /// The binary file unique identifier.
            /// </value>
            public Guid BinaryFileGuid { get; set; }

            /// <summary>
            /// Instructs the task to use a substring search for <see cref="BinaryFileGuid"/>.
            /// </summary>
            public bool UseContainsSearch { get; set; }
        }
    }
}