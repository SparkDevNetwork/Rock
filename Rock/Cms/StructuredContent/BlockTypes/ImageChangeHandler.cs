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

using Rock.Data;
using Rock.Model;

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// Change detection for the <see cref="ImageRenderer"/> block type.
    /// </summary>
    /// <seealso cref="Rock.Cms.StructuredContent.StructuredContentBlockChangeHandler{TData}" />
    [StructuredContentBlock( "image" )]
    public class ImageChangeHandler : StructuredContentBlockChangeHandler<ImageData>
    {
        /// <inheritdoc/>
        protected override void DetectBlockChanges( ImageData newData, ImageData oldData, StructuredContentChanges changes )
        {
            var imageChanges = changes.GetData<ImageChangeData>();

            if ( imageChanges == null )
            {
                imageChanges = new ImageChangeData();
                changes.AddOrReplaceData( imageChanges );
            }

            // If the new data has a file identifier then store it for later review.
            if ( newData?.File?.FileId != null )
            {
                imageChanges.NewFileIds.Add( newData.File.FileId.Value );
            }

            // If the old data has a file identifier then store it for alter review.
            if ( oldData?.File?.FileId != null )
            {
                imageChanges.OldFileIds.Add( oldData.File.FileId.Value );
            }
        }

        /// <inheritdoc/>
        public override bool ApplyDatabaseChanges( StructuredContentHelper helper, StructuredContentChanges changes, RockContext rockContext )
        {
            var imageChanges = changes.GetData<ImageChangeData>();

            if ( imageChanges == null )
            {
                return false;
            }

            var addedBinaryFileIds = imageChanges?.NewFileIds?.Except( imageChanges.OldFileIds )?.ToList();
            var removedBinaryFileIds = imageChanges?.OldFileIds?.Except( imageChanges.NewFileIds )?.ToList();

            bool needSave = false;
            var binaryFileService = new BinaryFileService( rockContext );

            // If there are any newly added binary files then mark them as
            // permanent so they will persist in the database.
            if ( addedBinaryFileIds != null && addedBinaryFileIds.Count > 0 )
            {
                var filesToAdd = binaryFileService.Queryable()
                    .Where( b => addedBinaryFileIds.Contains( b.Id ) )
                    .ToList();

                filesToAdd.ForEach( b => b.IsTemporary = false );
                needSave = true;
            }

            // If there are any removed binary files then mark them as temporary
            // so they will be later removed by the cleanup job.
            if ( removedBinaryFileIds != null && removedBinaryFileIds.Count > 0 )
            {
                var filesToRemove = binaryFileService.Queryable()
                    .Where( b => removedBinaryFileIds.Contains( b.Id ) )
                    .ToList();

                filesToRemove.ForEach( b => b.IsTemporary = true );
                needSave = true;
            }

            return needSave;
        }

        /// <summary>
        /// The changes related to the image blocks.
        /// </summary>
        private class ImageChangeData
        {
            /// <summary>
            /// Gets the binary file identifiers that exist in the new data
            /// about to be saved.
            /// </summary>
            /// <value>
            /// The binary file identifiers that exist in the new data about
            /// to be saved.
            /// </value>
            public List<int> NewFileIds { get; } = new List<int>();

            /// <summary>
            /// Gets the binary file identifiers that existed in the old
            /// content before the save.
            /// </summary>
            /// <value>
            /// The binary file identifiers that existed in the old content
            /// before the save.
            /// </value>
            public List<int> OldFileIds { get; } = new List<int>();
        }
    }
}
