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
    public class GroupRequirementTypeDetail : RockDetailBlockType
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
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<GroupRequirementType>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private GroupRequirementTypeDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new GroupRequirementTypeDetailOptionsBag();

            options.DueDateOptions = ToEnumListItemBag( typeof( DueDateType ) );
            options.RequirementTypeOptions = new List<ListItemBag>()
            {
                new ListItemBag() { Text = "SQL", Value = RequirementCheckType.Sql.ToString() },
                new ListItemBag() { Text = "Data View", Value = RequirementCheckType.Dataview.ToString() },
                new ListItemBag() { Text = "Manual", Value = RequirementCheckType.Manual.ToString() },
            };

            return options;
        }

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
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the GroupRequirementType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateGroupRequirementType( GroupRequirementType groupRequirementType, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {GroupRequirementType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.Entity.CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
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
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( GroupRequirementType.FriendlyTypeName );
                }
            }
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

            return new GroupRequirementTypeBag
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
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="GroupRequirementTypeBag"/> that represents the entity.</returns>
        private GroupRequirementTypeBag GetEntityBagForView( GroupRequirementType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="GroupRequirementTypeBag"/> that represents the entity.</returns>
        private GroupRequirementTypeBag GetEntityBagForEdit( GroupRequirementType entity )
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

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( GroupRequirementType entity, DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.CanExpire ),
                () => entity.CanExpire = box.Entity.CanExpire );

            box.IfValidProperty( nameof( box.Entity.Category ),
                () => entity.CategoryId = box.Entity.Category.GetEntityId<Category>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.CheckboxLabel ),
                () => entity.CheckboxLabel = box.Entity.CheckboxLabel );

            box.IfValidProperty( nameof( box.Entity.DataView ),
                () => entity.DataViewId = box.Entity.RequirementCheckType == RequirementCheckType.Dataview.ToString() ? box.Entity.DataView.GetEntityId<DataView>( rockContext ) : null );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.DoesNotMeetWorkflowLinkText ),
                () => entity.DoesNotMeetWorkflowLinkText = box.Entity.DoesNotMeetWorkflowLinkText );

            box.IfValidProperty( nameof( box.Entity.DoesNotMeetWorkflowType ),
                () => entity.DoesNotMeetWorkflowTypeId = box.Entity.DoesNotMeetWorkflowType.GetEntityId<WorkflowType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.DueDateType ),
                () => entity.DueDateType = box.Entity.DueDateType.ConvertToEnum<DueDateType>() );

            box.IfValidProperty( nameof( box.Entity.DueDateOffsetInDays ),
                () => entity.DueDateOffsetInDays = box.Entity.DueDateType == DueDateType.DaysAfterJoining.ToString() ? box.Entity.DueDateOffsetInDays : null );

            box.IfValidProperty( nameof( box.Entity.ExpireInDays ),
                () => entity.ExpireInDays = entity.CanExpire ? box.Entity.ExpireInDays : null );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.NegativeLabel ),
                () => entity.NegativeLabel = box.Entity.NegativeLabel );

            box.IfValidProperty( nameof( box.Entity.PositiveLabel ),
                () => entity.PositiveLabel = box.Entity.PositiveLabel );

            box.IfValidProperty( nameof( box.Entity.RequirementCheckType ),
                () => entity.RequirementCheckType = box.Entity.RequirementCheckType.ConvertToEnum<RequirementCheckType>() );

            box.IfValidProperty( nameof( box.Entity.ShouldAutoInitiateDoesNotMeetWorkflow ),
                () => entity.ShouldAutoInitiateDoesNotMeetWorkflow = box.Entity.ShouldAutoInitiateDoesNotMeetWorkflow );

            box.IfValidProperty( nameof( box.Entity.ShouldAutoInitiateWarningWorkflow ),
                () => entity.ShouldAutoInitiateWarningWorkflow = box.Entity.ShouldAutoInitiateWarningWorkflow );

            box.IfValidProperty( nameof( box.Entity.SqlExpression ),
                () => entity.SqlExpression = box.Entity.RequirementCheckType == RequirementCheckType.Sql.ToString() ? box.Entity.SqlExpression : null );

            box.IfValidProperty( nameof( box.Entity.Summary ),
                () => entity.Summary = box.Entity.Summary );

            box.IfValidProperty( nameof( box.Entity.WarningDataView ),
                () => entity.WarningDataViewId = box.Entity.RequirementCheckType == RequirementCheckType.Dataview.ToString() ? box.Entity.WarningDataView.GetEntityId<DataView>( rockContext ) : null );

            box.IfValidProperty( nameof( box.Entity.WarningLabel ),
                () => entity.WarningLabel = box.Entity.WarningLabel );

            box.IfValidProperty( nameof( box.Entity.WarningSqlExpression ),
                () => entity.WarningSqlExpression = box.Entity.RequirementCheckType == RequirementCheckType.Sql.ToString() ? box.Entity.WarningSqlExpression : null );

            box.IfValidProperty( nameof( box.Entity.WarningWorkflowLinkText ),
                () => entity.WarningWorkflowLinkText = box.Entity.WarningWorkflowLinkText );

            box.IfValidProperty( nameof( box.Entity.WarningWorkflowType ),
                () => entity.WarningWorkflowTypeId = box.Entity.WarningWorkflowType.GetEntityId<WorkflowType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="GroupRequirementType"/> to be viewed or edited on the page.</returns>
        private GroupRequirementType GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<GroupRequirementType, GroupRequirementTypeService>( rockContext, PageParameterKey.GroupRequirementTypeId );
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

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( GroupRequirementType entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out GroupRequirementType entity, out BlockActionResult error )
        {
            var entityService = new GroupRequirementTypeService( rockContext );
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
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
        public BlockActionResult Save( DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateGroupRequirementType( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new GroupRequirementTypeService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
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
        public BlockActionResult RefreshAttributes( DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<GroupRequirementTypeBag, GroupRequirementTypeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
