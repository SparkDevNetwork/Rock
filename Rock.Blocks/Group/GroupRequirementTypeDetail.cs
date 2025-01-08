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
using Rock.ViewModels.Blocks.Group.GroupRequirementTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group requirement type.
    /// </summary>

    [DisplayName( "Group Requirement Type Detail" )]
    [Category( "Group" )]
    [Description( "Displays the details of the given group requirement type for editing." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "8a95bcf0-63cb-4cd6-99c9-e812d9afae99" )]
    [Rock.SystemGuid.BlockTypeGuid( "c17b6d03-fdf3-4dd7-b9a9-3d6159a838f5" )]
    public class GroupRequirementTypeDetail : RockEntityDetailBlockType<GroupRequirementType, GroupRequirementTypeBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string GroupRequirementTypeId = "GroupRequirementTypeId";
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
            var box = new DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private GroupRequirementTypeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new GroupRequirementTypeDetailOptionsBag();

            var see = typeof( DueDateType ).ToEnumListItemBag();
            options.DueDateOptions = ToEnumListItemBag( typeof( DueDateType ) );
            options.RequirementTypeOptions = new List<ListItemBag>()
            {
                new ListItemBag() { Text = "SQL", Value = RequirementCheckType.Sql.ToString() },
                new ListItemBag() { Text = "Data View", Value = RequirementCheckType.Dataview.ToString() },
                new ListItemBag() { Text = "Manual", Value = RequirementCheckType.Manual.ToString() },
            };

            return options;
        }

        /// <summary>
        /// Converts to the enum to a ListItemBag.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns></returns>
        private static List<ListItemBag> ToEnumListItemBag( Type enumType )
        {
            var listItemBag = new List<ListItemBag>();
            foreach ( Enum enumValue in Enum.GetValues( enumType ) )
            {
                var text = enumValue.GetDescription() ?? enumValue.ToString().SplitCase();
                listItemBag.Add( new ListItemBag { Text = text, Value = enumValue.ToString() } );
            }

            return listItemBag.ToList();
        }

        /// <summary>
        /// Validates the GroupRequirementType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="groupRequirementType">The GroupRequirementType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the GroupRequirementType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateGroupRequirementType( GroupRequirementType groupRequirementType, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {GroupRequirementType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.Entity.CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( GroupRequirementType.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( GroupRequirementType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="GroupRequirementTypeBag"/> that represents the entity.</returns>
        private GroupRequirementTypeBag GetCommonEntityBag( GroupRequirementType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new GroupRequirementTypeBag
            {
                IdKey = entity.IdKey,
                CanExpire = entity.CanExpire,
                Category = entity.Category.ToListItemBag(),
                CheckboxLabel = entity.CheckboxLabel,
                DataView = entity.DataView.ToListItemBag(),
                Description = entity.Description,
                DoesNotMeetWorkflowLinkText = entity.DoesNotMeetWorkflowLinkText,
                DoesNotMeetWorkflowType = entity.DoesNotMeetWorkflowType.ToListItemBag(),
                DueDateOffsetInDays = entity.DueDateOffsetInDays,
                DueDateType = entity.DueDateType.ToString(),
                ExpireInDays = entity.ExpireInDays,
                IconCssClass = entity.IconCssClass,
                Name = entity.Name,
                NegativeLabel = entity.NegativeLabel,
                PositiveLabel = entity.PositiveLabel,
                RequirementCheckType = entity.Id == 0 ? RequirementCheckType.Manual.ToString() : entity.RequirementCheckType.ToString(),
                ShouldAutoInitiateDoesNotMeetWorkflow = entity.ShouldAutoInitiateDoesNotMeetWorkflow,
                ShouldAutoInitiateWarningWorkflow = entity.ShouldAutoInitiateWarningWorkflow,
                SqlExpression = entity.SqlExpression,
                Summary = entity.Summary,
                WarningDataView = entity.WarningDataView.ToListItemBag(),
                WarningLabel = entity.WarningLabel,
                WarningSqlExpression = entity.WarningSqlExpression,
                WarningWorkflowLinkText = entity.WarningWorkflowLinkText,
                WarningWorkflowType = entity.WarningWorkflowType.ToListItemBag()
            };

            bag.LoadAttributesAndValuesForPublicView( entity, GetCurrentPerson() );

            return bag;
        }

        /// <inheritdoc/>
        protected override GroupRequirementTypeBag GetEntityBagForView( GroupRequirementType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override GroupRequirementTypeBag GetEntityBagForEdit( GroupRequirementType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.SqlHelpHTML = @"A SQL expression that returns a list of Person Ids that meet the criteria. Example:
<pre>
SELECT [Id] FROM [Person]
WHERE [LastName] = 'Decker'</pre>
</pre>
The SQL can include Lava merge fields:

<ul>
   <li>Group</i>
   <li>GroupRequirementType</i>
</ul>

TIP: When calculating for a specific Person, a <strong>Person</strong> merge field will also be included. This can improve performance in cases when the system is checking requirements for a specific person. Example:

<pre>
    SELECT [Id] FROM [Person]
        WHERE [LastName] = 'Decker'
    {% if Person != empty %}
        AND [Id] = {{ Person.Id }}
    {% endif %}
</pre>
";

            bag.SqlHelpHTML += entity.GetMergeObjects( new Rock.Model.Group(), this.GetCurrentPerson() ).lavaDebugInfo();

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( GroupRequirementType entity, ValidPropertiesBox<GroupRequirementTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.CanExpire ),
                () => entity.CanExpire = box.Bag.CanExpire );

            box.IfValidProperty( nameof( box.Bag.Category ),
                () => entity.CategoryId = box.Bag.Category.GetEntityId<Category>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CheckboxLabel ),
                () => entity.CheckboxLabel = box.Bag.CheckboxLabel );

            box.IfValidProperty( nameof( box.Bag.DataView ),
                () => entity.DataViewId = box.Bag.RequirementCheckType == RequirementCheckType.Dataview.ToString() ? box.Bag.DataView.GetEntityId<DataView>( RockContext ) : null );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.DoesNotMeetWorkflowLinkText ),
                () => entity.DoesNotMeetWorkflowLinkText = box.Bag.DoesNotMeetWorkflowLinkText );

            box.IfValidProperty( nameof( box.Bag.DoesNotMeetWorkflowType ),
                () => entity.DoesNotMeetWorkflowTypeId = box.Bag.DoesNotMeetWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.DueDateType ),
                () => entity.DueDateType = box.Bag.DueDateType.ConvertToEnum<DueDateType>() );

            box.IfValidProperty( nameof( box.Bag.DueDateOffsetInDays ),
                () => entity.DueDateOffsetInDays = box.Bag.DueDateType == nameof( DueDateType.DaysAfterJoining ) || box.Bag.DueDateType == nameof( DueDateType.GroupAttribute )
                ? box.Bag.DueDateOffsetInDays
                : null );

            box.IfValidProperty( nameof( box.Bag.ExpireInDays ),
                () => entity.ExpireInDays = entity.CanExpire ? box.Bag.ExpireInDays : null );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.NegativeLabel ),
                () => entity.NegativeLabel = box.Bag.NegativeLabel );

            box.IfValidProperty( nameof( box.Bag.PositiveLabel ),
                () => entity.PositiveLabel = box.Bag.PositiveLabel );

            box.IfValidProperty( nameof( box.Bag.RequirementCheckType ),
                () => entity.RequirementCheckType = box.Bag.RequirementCheckType.ConvertToEnum<RequirementCheckType>() );

            box.IfValidProperty( nameof( box.Bag.ShouldAutoInitiateDoesNotMeetWorkflow ),
                () => entity.ShouldAutoInitiateDoesNotMeetWorkflow = box.Bag.ShouldAutoInitiateDoesNotMeetWorkflow );

            box.IfValidProperty( nameof( box.Bag.ShouldAutoInitiateWarningWorkflow ),
                () => entity.ShouldAutoInitiateWarningWorkflow = box.Bag.ShouldAutoInitiateWarningWorkflow );

            box.IfValidProperty( nameof( box.Bag.SqlExpression ),
                () => entity.SqlExpression = box.Bag.RequirementCheckType == RequirementCheckType.Sql.ToString() ? box.Bag.SqlExpression : null );

            box.IfValidProperty( nameof( box.Bag.Summary ),
                () => entity.Summary = box.Bag.Summary );

            box.IfValidProperty( nameof( box.Bag.WarningDataView ),
                () => entity.WarningDataViewId = box.Bag.RequirementCheckType == RequirementCheckType.Dataview.ToString() ? box.Bag.WarningDataView.GetEntityId<DataView>( RockContext ) : null );

            box.IfValidProperty( nameof( box.Bag.WarningLabel ),
                () => entity.WarningLabel = box.Bag.WarningLabel );

            box.IfValidProperty( nameof( box.Bag.WarningSqlExpression ),
                () => entity.WarningSqlExpression = box.Bag.RequirementCheckType == RequirementCheckType.Sql.ToString() ? box.Bag.WarningSqlExpression : null );

            box.IfValidProperty( nameof( box.Bag.WarningWorkflowLinkText ),
                () => entity.WarningWorkflowLinkText = box.Bag.WarningWorkflowLinkText );

            box.IfValidProperty( nameof( box.Bag.WarningWorkflowType ),
                () => entity.WarningWorkflowTypeId = box.Bag.WarningWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override GroupRequirementType GetInitialEntity()
        {
            return GetInitialEntity<GroupRequirementType, GroupRequirementTypeService>( RockContext, PageParameterKey.GroupRequirementTypeId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out GroupRequirementType entity, out BlockActionResult error )
        {
            var entityService = new GroupRequirementTypeService( RockContext );
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
                entity = new GroupRequirementType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{GroupRequirementType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${GroupRequirementType.FriendlyTypeName}." );
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

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<GroupRequirementTypeBag>()
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
        public BlockActionResult Save( ValidPropertiesBox<GroupRequirementTypeBag> box )
        {
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
            if ( !ValidateGroupRequirementType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.SaveChanges();

            var bag = GetEntityBagForView( entity );

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new GroupRequirementTypeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
