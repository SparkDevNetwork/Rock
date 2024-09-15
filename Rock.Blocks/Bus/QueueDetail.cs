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
using System.ComponentModel;
using Rock.Attribute;
using Rock.Bus.Queue;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Bus.QueueDetail;

namespace Rock.Blocks.Bus
{
    /// <summary>
    /// Displays the details of a particular achievement type.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Queue Detail" )]
    [Category( "Bus" )]
    [Description( "Displays the details of a queue detail." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "6c61ffc3-b37c-4d90-8b99-cc3c53150ee3" )]
    [Rock.SystemGuid.BlockTypeGuid( "db19d24e-b0c8-4686-8582-7b84dae33ee8" )]
    public class QueueDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string QueueKey = "QueueKey";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<QueueBag, QueueDetailOptionsBag>();

            SetBoxInitialQueueState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the queue.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the queue is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private QueueDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new QueueDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Rock Queue for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="rockQueue">The Rock Queue to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AchievementType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateQueue( IRockQueue rockQueue, out string errorMessage )
        {
            errorMessage = null;

            if ( !rockQueue.TimeToLiveSeconds.HasValue || rockQueue.TimeToLiveSeconds.Value == 0 )
            {
                errorMessage = "Time to live is invalid";
            }

            return true;
        }

        /// <summary>
        /// Sets the initial queue state of the box. Populates the Queue or
        /// ErrorMessage properties depending on the queue and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialQueueState( DetailBlockBox<QueueBag, QueueDetailOptionsBag> box )
        {
            var rockQueue = GetQueue();

            if ( rockQueue == null )
            {
                box.ErrorMessage = $"The Queue was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            // Existing entity was found, prepare for view mode by default.
            if ( isViewable )
            {
                box.Entity = GetQueueBagForView( rockQueue );
                box.SecurityGrantToken = GetSecurityGrantToken( rockQueue );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AchievementType.FriendlyTypeName );
            }
        }

        /// <summary>
        /// Gets the queue bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="queue">The queue to be represented as a bag.</param>
        /// <returns>A <see cref="QueueBag"/> that represents the queue.</returns>
        private QueueBag GetCommonQueueBag( IRockQueue rockQueue )
        {
            if ( rockQueue == null )
            {
                return null;
            }

            return new QueueBag
            {
                Name = rockQueue.Name,
                TimeToLiveSeconds = rockQueue.TimeToLiveSeconds
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified queue.
        /// </summary>
        /// <param name="queue">The queue to be represented for view purposes.</param>
        /// <returns>A <see cref="QueueBag"/> that represents the queue.</returns>
        private QueueBag GetQueueBagForView( IRockQueue rockQueue )
        {
            if ( rockQueue == null )
            {
                return null;
            }

            var bag = GetCommonQueueBag( rockQueue );
            if ( rockQueue.StatLog != null )
            {
                bag.MessagesConsumedLastMinute = rockQueue.StatLog.MessagesConsumedLastMinute;
                bag.MessagesConsumedLastHour = rockQueue.StatLog.MessagesConsumedLastHour;
                bag.MessagesConsumedLastDay = rockQueue.StatLog.MessagesConsumedLastDay;
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified queue.
        /// </summary>
        /// <param name="queue">The queue to be represented for edit purposes.</param>
        /// <returns>A <see cref="QueueBag"/> that represents the queue.</returns>
        private QueueBag GetQueueBagForEdit( IRockQueue rockQueue )
        {
            if ( rockQueue == null )
            {
                return null;
            }

            var bag = GetCommonQueueBag( rockQueue );

            return bag;
        }

        /// <summary>
        /// Updates the queue from the data in the save box.
        /// </summary>
        /// <param name="rockQueue">The queue to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <returns><c>true</c> if the box was valid and the queue was updated, <c>false</c> otherwise.</returns>
        private bool UpdateQueueFromBox( IRockQueue rockQueue, DetailBlockBox<QueueBag, QueueDetailOptionsBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.TimeToLiveSeconds ),
                () => rockQueue.TimeToLiveSeconds = box.Entity.TimeToLiveSeconds );

            return true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var rockQueue = GetQueue();

                return GetSecurityGrantToken( rockQueue );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( IRockQueue rockQueue )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an queue to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockQueue">Contains the queue that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the queue was loaded and passed security checks.</returns>
        private bool TryGetQueueForEditAction( string idKey, out IRockQueue rockQueue, out BlockActionResult error )
        {
            error = null;
            rockQueue = null;
            // Determine if we are editing an existing queue or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing queue then load it and make sure it
                // was found and can still be edited.

                rockQueue = GetQueue();
            }

            if ( rockQueue == null )
            {
                error = ActionBadRequest( $"Queue not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit Rock Queue." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the queue
        /// </summary>
        /// <returns></returns>
        private IRockQueue GetQueue()
        {
            var queueKey = PageParameter( PageParameterKey.QueueKey );

            if ( queueKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return Rock.Bus.Queue.RockQueue.Get( queueKey );
        }


        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the queue to be edited.</param>
        /// <returns>A box that contains the queue and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetQueueForEditAction( key, out var rockQueue, out var actionError ) )
            {
                return actionError;
            }

            var box = new DetailBlockBox<QueueBag, QueueDetailOptionsBag>
            {
                Entity = GetQueueBagForEdit( rockQueue )
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the queue contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new queue bag to be used when returning to view mode, or the URL to redirect to after creating a new queue.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<QueueBag, QueueDetailOptionsBag> box )
        {
            if ( !TryGetQueueForEditAction( box.Entity.Name, out var rockQueue, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateQueueFromBox( rockQueue, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateQueue( rockQueue, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            rockQueue = GetQueue();

            return ActionOk( GetQueueBagForView( rockQueue ) );
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<QueueBag, QueueDetailOptionsBag> box )
        {
            return ActionBadRequest( "Attributes are not supported by this block." );
        }

        #endregion
    }
}
