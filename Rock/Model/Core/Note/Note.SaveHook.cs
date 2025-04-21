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
using System.Threading.Tasks;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Note
    {
        /// <summary>
        /// Save hook implementation for <see cref="Note"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Note>
        {

            private bool _shouldRunAIApproval = false;

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                base.PreSave();

                CreateAutoWatchForAuthor();

                // If the NoteType requires approvals and the note text has changed (or is new), mark it as pending approval
                // and flag it for ai auto-approval (if configured).
                var noteType = NoteTypeCache.Get( this.Entity.NoteTypeId );
                if ( noteType.RequiresApprovals )
                {
                    if ( State == EntityContextState.Added ||
                        !Entity.Text.Equals( OriginalValues["Text"]?.ToStringSafe(), System.StringComparison.OrdinalIgnoreCase ) )
                    {
                        // If the individual is redirected to a block that shows notes - be sure the status is pending approval
                        // until the automation is complete.
                        Entity.ApprovalStatus = NoteApprovalStatus.PendingApproval;
                        Entity.ApprovedDateTime = null;
                        _shouldRunAIApproval = true;
                    }
                }
                else
                {
                    // If the NoteType doesn't require approvals - auto-approve.
                    // This mirrors the behavior before we re-added the ability to approve notes.
                    // It also makes queries easier to write by avoiding a check on NoteType.RequiresApprovals.
                    Entity.ApprovalStatus = NoteApprovalStatus.Approved;
                }
            }

            protected override void PostSave()
            {
                base.PostSave();

                // If the pre-save logic (where we can view previous values)
                // determined we should evaluate AI approvals.
                if ( _shouldRunAIApproval )
                {
                    Task.Run( async () => await RunAIApproval() );
                }
            }

            /// <summary>
            /// Creates a <see cref="NoteWatch"/> for the author of this <see cref="Note"/> when appropriate.
            /// </summary>
            private void CreateAutoWatchForAuthor()
            {
                if ( State != EntityContextState.Added )
                {
                    return; // only create new watches for new notes.
                }

                var noteType = NoteTypeCache.Get( this.Entity.NoteTypeId );
                if ( noteType == null )
                {
                    return; // Probably an error, but let's avoid creating another one.
                }

                if ( noteType.AutoWatchAuthors != true )
                {
                    return; // Don't auto watch.
                }

                if ( !this.Entity.CreatedByPersonAliasId.HasValue )
                {
                    return; // No author to add a watch for.
                }

                // add a NoteWatch so the author will get notified when there are any replies
                var noteWatch = new NoteWatch
                {
                    // we don't know the Note.Id yet, so just assign the NoteWatch.Note and EF will populate the NoteWatch.NoteId automatically
                    Note = this.Entity,
                    IsWatching = true,
                    WatcherPersonAliasId = this.Entity.CreatedByPersonAliasId.Value
                };

                new NoteWatchService( RockContext ).Add( noteWatch );
            }

            /// <summary>
            /// If the Note.NoteType is configured to use AI for approval, run the AI approval process.
            /// </summary>
            private async Task RunAIApproval()
            {
                using ( var rockContext = new RockContext() )
                {
                    var wasAutoApproved = await new NoteService( rockContext ).GetAIAutoApproval( Entity );

                    // If the request wasn't sent or the response wasn't returned in the proper format,
                    // don't make any changes to the approval status.
                    if ( wasAutoApproved == null )
                    {
                        return;
                    }

                    // Set the approval status based on the AI's decision.
                    if ( wasAutoApproved == true )
                    {
                        Entity.ApprovalStatus = NoteApprovalStatus.Approved;
                        if ( !Entity.ApprovedDateTime.HasValue )
                        {
                            Entity.ApprovedDateTime = RockDateTime.Now;
                        }
                    }
                    else
                    {
                        Entity.ApprovalStatus = NoteApprovalStatus.Denied;
                    }

                    // Ensure the Entity is marked as modified so that it gets saved with the approval status change.
                    rockContext.Entry( Entity ).State = System.Data.Entity.EntityState.Modified;

                    var disablePrePostProcessing = true;
                    rockContext.SaveChanges( disablePrePostProcessing );
                }
            }
        }
    }
}
