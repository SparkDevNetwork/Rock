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

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// Change detection for the <see cref="AttachmentRenderer"/> block type.
    /// </summary>
    /// <seealso cref="Rock.Cms.StructuredContent.StructuredContentBlockChangeHandler{TData}" />
    [StructuredContentBlock( "attachment" )]
    public class AttachmentChangeHandler : StructuredContentBlockChangeHandler<AttachmentData>
    {
        /// <inheritdoc/>
        protected override void DetectBlockChanges( AttachmentData newData, AttachmentData oldData, StructuredContentChanges changes )
        {
            var attachmentChanges = changes.GetData<AttachmentChangeData>();

            if ( attachmentChanges == null )
            {
                attachmentChanges = new AttachmentChangeData();
                changes.AddOrReplaceData( attachmentChanges );
            }

            // If the new data has a file identifier then store it for later review.
            if ( newData?.File != null )
            {
                attachmentChanges.NewFileGuids.Add( newData.File.FileGuid );
            }

            // If the old data has a file identifier then store it for later review.
            if ( oldData?.File != null )
            {
                attachmentChanges.OldFileGuids.Add( oldData.File.FileGuid );
            }
        }

        /// <inheritdoc/>
        public override bool ApplyDatabaseChanges( StructuredContentHelper helper, StructuredContentChanges changes, RockContext rockContext )
        {
            var attachmentChanges = changes.GetData<AttachmentChangeData>();

            if ( attachmentChanges == null )
            {
                return false;
            }

            var addedBinaryFileGuids = attachmentChanges?.NewFileGuids?.Except( attachmentChanges.OldFileGuids )?.ToList();

            bool needSave = false;
            var binaryFileService = new BinaryFileService( rockContext );

            // If there are any newly added binary files then mark them as
            // permanent so they will persist in the database.
            if ( addedBinaryFileGuids != null && addedBinaryFileGuids.Count > 0 )
            {
                var filesToAdd = binaryFileService.Queryable()
                    .Where( b => addedBinaryFileGuids.Contains( b.Guid ) )
                    .ToList();

                filesToAdd.ForEach( b => b.IsTemporary = false );
                needSave = true;
            }

            /*
             * 2025-08-25 - DSH
             * 
             * We used to mark removed binary files as temporary so they would
             * be cleaned up by the Rock Cleanup job. However, this is no longer
             * the case. Instead, we will just leave them in the database.
             *
             * REASON: Sometimes structured content will be copied from a template
             * to an instance. When that happens, the same binary file is
             * referenced in both. So if we then remove the file from the editor
             * in the instance, it effectively removes it from the template too.
             */

            return needSave;
        }

        /// <summary>
        /// The changes related to the attachment blocks.
        /// </summary>
        private class AttachmentChangeData
        {
            /// <summary>
            /// Gets the binary file unique identifiers that exist in the new data
            /// about to be saved.
            /// </summary>
            public List<Guid> NewFileGuids { get; } = new List<Guid>();

            /// <summary>
            /// Gets the binary file unique identifiers that existed in the old
            /// content before the save.
            /// </summary>
            public List<Guid> OldFileGuids { get; } = new List<Guid>();
        }
    }
}
