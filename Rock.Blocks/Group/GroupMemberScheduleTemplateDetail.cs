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
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Utility;
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
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "611baab0-fef9-4e01-a0ea-688c7d4549ce" )]
    [Rock.SystemGuid.BlockTypeGuid( "07bcb48d-746e-4364-80f3-c5beb9075fc6" )]
    public class GroupMemberScheduleTemplateDetail : RockEntityDetailBlockType<GroupMemberScheduleTemplate, GroupMemberScheduleTemplateBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string AutoEdit = "autoEdit";
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
            var box = new DetailBlockBox<GroupMemberScheduleTemplateBag, GroupMemberScheduleTemplateDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Validates the GroupMemberScheduleTemplate for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="groupMemberScheduleTemplate">The GroupMemberScheduleTemplate to be validated.</param>
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
        private void SetBoxInitialEntityState( DetailBlockBox<GroupMemberScheduleTemplateBag, GroupMemberScheduleTemplateDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {GroupMemberScheduleTemplate.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            var isAutoEdit = PageParameter( PageParameterKey.AutoEdit ).AsBoolean();

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default, unless autoEdit flag was passed.
                if ( isAutoEdit && box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
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
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( GroupMemberScheduleTemplate.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
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

        /// <inheritdoc/>
        protected override GroupMemberScheduleTemplateBag GetEntityBagForView( GroupMemberScheduleTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        /// <inheritdoc/>
        protected override GroupMemberScheduleTemplateBag GetEntityBagForEdit( GroupMemberScheduleTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( GroupMemberScheduleTemplate entity, ValidPropertiesBox<GroupMemberScheduleTemplateBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            if ( entity.Schedule == null )
            {
                entity.Schedule = new Schedule();
            }

            box.IfValidProperty( nameof( box.Bag.CalendarContent ),
                () => entity.Schedule.iCalendarContent = box.Bag.CalendarContent );

            return true;
        }

        /// <inheritdoc/>
        protected override GroupMemberScheduleTemplate GetInitialEntity()
        {
            return GetInitialEntity<GroupMemberScheduleTemplate, GroupMemberScheduleTemplateService>( RockContext, PageParameterKey.GroupMemberScheduleTemplateId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out GroupMemberScheduleTemplate entity, out BlockActionResult error )
        {
            var entityService = new GroupMemberScheduleTemplateService( RockContext );
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<GroupMemberScheduleTemplateBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<GroupMemberScheduleTemplateBag> box )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest($"Not authorized to edit ${GroupMemberScheduleTemplate.FriendlyTypeName}.");
            }

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateGroupMemberScheduleTemplate( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
