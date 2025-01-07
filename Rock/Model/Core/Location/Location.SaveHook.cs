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
using Rock.CheckIn.v2;
using Rock.Data;

namespace Rock.Model
{
    public partial class Location
    {
        /// <summary>
        /// Save hook implementation for <see cref="Location"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Location>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( Entity.ImageId.HasValue )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( RockContext );
                    var binaryFile = binaryFileService.Get( Entity.ImageId.Value );
                    if ( binaryFile != null && binaryFile.IsTemporary )
                    {
                        binaryFile.IsTemporary = false;
                    }
                }

                base.PreSave();
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                if ( Entity.Name.IsNotNullOrWhiteSpace() && PreSaveState == EntityContextState.Modified )
                {
                    // Changes to the hierarchy of a named location can cause
                    // an invalidation of kiosk configuration.
                    var oldParentLocationId = OriginalValues[nameof( Entity.ParentLocationId )] as int?;

                    if ( oldParentLocationId != Entity.ParentLocationId )
                    {
                        CheckInDirector.SendRefreshKioskConfiguration();
                    }
                }

                base.PostSave();
            }
        }
    }
}
