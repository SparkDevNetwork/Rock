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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupMemberScheduleTemplateDetail;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group member schedule template.
    /// </summary>

    [DisplayName( "Group Member Schedule Template Detail" )]
    [Category( "Group Scheduling" )]
    [Description( "Displays the details of a group member schedule template." )]
    [IconCssClass( "fa fa-question" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "611baab0-fef9-4e01-a0ea-688c7d4549ce" )]
    [Rock.SystemGuid.BlockTypeGuid( "07bcb48d-746e-4364-80f3-c5beb9075fc6" )]
    public class GroupMemberScheduleTemplateDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string GroupMemberScheduleTemplateId = "GroupMemberScheduleTemplateId";
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
            using ( var rockContext = new RockContext() )
            {
                var box = new GroupMemberScheduleTemplateBox();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();

                return box;
            }
        }

        /// <summary>
        /// Validates the GroupMemberScheduleTemplate for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="groupMemberScheduleTemplate">The GroupMemberScheduleTemplate to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the GroupMemberScheduleTemplate is valid, <c>false</c> otherwise.</returns>
        private bool ValidateGroupMemberScheduleTemplate( GroupMemberScheduleTemplate groupMemberScheduleTemplate, out string errorMessage )
        {
            errorMessage = null;

            if ( !groupMemberScheduleTemplate.IsValid )
            {
                errorMessage = groupMemberScheduleTemplate.ValidationResults.ConvertAll( a => a.ErrorMessage ).AsDelimited( "<br />" );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( GroupMemberScheduleTemplateBox box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {GroupMemberScheduleTemplate.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetCommonEntityBag( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( GroupMemberScheduleTemplate.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetCommonEntityBag( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( GroupMemberScheduleTemplate.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="GroupMemberScheduleTemplateBag"/> that represents the entity.</returns>
        private GroupMemberScheduleTemplateBag GetCommonEntityBag( GroupMemberScheduleTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new GroupMemberScheduleTemplateBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name,
                CalendarContent = entity.Schedule?.iCalendarContent,
                Schedule = entity.Schedule?.FriendlyScheduleText
            };
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="GroupMemberScheduleTemplate"/> to be viewed or edited on the page.</returns>
        private GroupMemberScheduleTemplate GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<GroupMemberScheduleTemplate, GroupMemberScheduleTemplateService>( rockContext, PageParameterKey.GroupMemberScheduleTemplateId );
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
                var entity = GetInitialEntity( rockContext );

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( GroupMemberScheduleTemplate entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out GroupMemberScheduleTemplate entity, out BlockActionResult error )
        {
            var entityService = new GroupMemberScheduleTemplateService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new GroupMemberScheduleTemplate();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{GroupMemberScheduleTemplate.FriendlyTypeName} not found." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var box = new GroupMemberScheduleTemplateBox()
                {
                    Entity = GetCommonEntityBag( entity ),
                    IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( GroupMemberScheduleTemplateBox box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest($"Not authorized to edit ${GroupMemberScheduleTemplate.FriendlyTypeName}.");
                }

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( entity.Schedule == null )
                {
                    entity.Schedule = new Schedule();
                }

                entity.Name = box.Entity.Name;
                entity.Schedule.iCalendarContent = box.Entity.CalendarContent;

                // Ensure everything is valid before saving.
                if ( !ValidateGroupMemberScheduleTemplate( entity, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( GroupMemberScheduleTemplateBox box )
        {
            return ActionBadRequest( "Attributes are not supported by this block." );
        }

        #endregion
    }
}
