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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FinancialPledgeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular financial pledge.
    /// </summary>
    [DisplayName( "Pledge Detail" )]
    [Category( "Finance" )]
    [Description( "Allows the details of a given pledge to be edited." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes
    [GroupTypeField( "Select Group Type",
        Description = "Optional Group Type that if selected will display a list of groups that pledge can be associated to for selected user",
        IsRequired = false,
        Order = 1 )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "c7862196-7312-4370-b2d7-05b631429071" )]
    [Rock.SystemGuid.BlockTypeGuid( "2a5ae27f-f536-4acc-b5eb-9263c4b92ef5" )]
    public class FinancialPledgeDetail : RockDetailBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SelectGroupType = "SelectGroupType";
        }

        private static class PageParameterKey
        {
            public const string FinancialPledgeId = "PledgeId";
            public const string PersonActionIdentifier = "rckid";
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
                var box = new DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.Entity, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<FinancialPledge>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialPledgeDetailOptionsBag GetBoxOptions( FinancialPledgeBag bag, RockContext rockContext )
        {
            var selectedGroupTypeGuid = GetAttributeValue( AttributeKey.SelectGroupType ).AsGuidOrNull();
            var options = new FinancialPledgeDetailOptionsBag
            {
                SelectGroupTypeGuid = selectedGroupTypeGuid,
                Groups = LoadGroups( bag?.PersonAlias?.Value?.AsGuidOrNull(), rockContext ),
                GroupType = GroupTypeCache.Get( selectedGroupTypeGuid ?? Guid.Empty )?.Name
            };
            return options;
        }

        /// <summary>
        /// Validates the FinancialPledge for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="financialPledge">The FinancialPledge to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the FinancialPledge is valid, <c>false</c> otherwise.</returns>
        private bool ValidateFinancialPledge( FinancialPledge financialPledge, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( !financialPledge.IsValid )
            {
                errorMessage = string.Format( "Please correct the following:<ul><li>{0}</li></ul>", financialPledge.ValidationResults.AsDelimited( "</li><li>" ) ); ;
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
        private void SetBoxInitialEntityState( DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialPledge.FriendlyTypeName} was not found.";
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
                    box.Entity = GetEntityBagForView( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FinancialPledge.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialPledge.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="FinancialPledgeBag"/> that represents the entity.</returns>
        private FinancialPledgeBag GetCommonEntityBag( FinancialPledge entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new FinancialPledgeBag
            {
                IdKey = entity.IdKey,
                Account = entity.Account.ToListItemBag(),
                Group = entity.Group.ToListItemBag(),
                PersonAlias = entity.PersonAlias != null ? entity.PersonAlias.ToListItemBag() : GetPersonByPersonActionIdentifier( rockContext ),
                PledgeFrequencyValue = entity.PledgeFrequencyValue.ToListItemBag(),
                TotalAmount = entity.Id == 0 ? ( decimal? ) null : entity.TotalAmount
            };

            if ( entity.Id != 0 )
            {
                bag.StartDate = entity.StartDate == DateTime.MinValue.Date ? ( DateTime? ) null : entity.StartDate;
                bag.EndDate = entity.EndDate == DateTime.MaxValue.Date ? ( DateTime? ) null : entity.EndDate;
            }

            return bag;
        }

        /// <summary>
        /// Gets the person by person action identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private ListItemBag GetPersonByPersonActionIdentifier( RockContext rockContext )
        {
            var personActionId = PageParameter( PageParameterKey.PersonActionIdentifier );
            var person = new PersonService( rockContext ).GetByPersonActionIdentifier( personActionId, "pledge" );

            return person?.PrimaryAlias?.ToListItemBag();
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="FinancialPledgeBag"/> that represents the entity.</returns>
        private FinancialPledgeBag GetEntityBagForView( FinancialPledge entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="FinancialPledgeBag"/> that represents the entity.</returns>
        private FinancialPledgeBag GetEntityBagForEdit( FinancialPledge entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( FinancialPledge entity, DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Account ),
                () => entity.AccountId = box.Entity.Account.GetEntityId<FinancialAccount>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.EndDate ),
                () => entity.EndDate = box.Entity.EndDate ?? DateTime.MaxValue );

            box.IfValidProperty( nameof( box.Entity.Group ),
                () => entity.GroupId = box.Entity.Group.GetEntityId<Rock.Model.Group>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PersonAlias ),
                () => entity.PersonAliasId = box.Entity.PersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PledgeFrequencyValue ),
                () => entity.PledgeFrequencyValueId = box.Entity.PledgeFrequencyValue.GetEntityId<DefinedValue>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.StartDate ),
                () => entity.StartDate = box.Entity.StartDate ?? DateTime.MinValue );

            box.IfValidProperty( nameof( box.Entity.TotalAmount ),
                () => entity.TotalAmount = box.Entity.TotalAmount.GetValueOrDefault() );

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
        /// <returns>The <see cref="FinancialPledge"/> to be viewed or edited on the page.</returns>
        private FinancialPledge GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<FinancialPledge, FinancialPledgeService>( rockContext, PageParameterKey.FinancialPledgeId );
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
        private string GetSecurityGrantToken( FinancialPledge entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out FinancialPledge entity, out BlockActionResult error )
        {
            var entityService = new FinancialPledgeService( rockContext );
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
                entity = new FinancialPledge();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialPledge.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${FinancialPledge.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the groups.
        /// </summary>
        /// <param name="personAliasGuid">The person alias unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<ListItemBag> LoadGroups( Guid? personAliasGuid, RockContext rockContext )
        {
            Guid? groupTypeGuid = GetAttributeValue( AttributeKey.SelectGroupType ).AsGuidOrNull();
            if ( personAliasGuid.HasValue && groupTypeGuid.HasValue )
            {
                var personId = new PersonAliasService( rockContext ).GetSelect( personAliasGuid.Value, p => p.PersonId );

                var groups = new GroupMemberService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.Group.GroupType.Guid == groupTypeGuid.Value &&
                        m.PersonId == personId &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        m.Group.IsActive && !m.Group.IsArchived )
                    .Select( m => new
                    {
                        m.Group.Guid,
                        Name = m.Group.Name
                    } )
                    .AsEnumerable()
                    .Distinct()
                    .OrderBy( g => g.Name )
                    .ToList();

                return groups.ConvertAll( g => new ListItemBag()
                {
                    Text = g.Name,
                    Value = g.Guid.ToString(),
                } );
            }

            return new List<ListItemBag>();
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

                var box = new DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
        public BlockActionResult Save( DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag> box )
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
                if ( !ValidateFinancialPledge( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                return ActionContent( System.Net.HttpStatusCode.Created, this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.FinancialPledgeId] = entity.IdKey
                } ) );
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
                var entityService = new FinancialPledgeService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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

        /// <summary>
        /// Loads the groups.
        /// </summary>
        /// <param name="personAliasGuid">The person alias unique identifier.</param>
        /// <returns>The list of groups the person belongs to</returns>
        [BlockAction]
        public BlockActionResult LoadGroups( Guid? personAliasGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var groups = LoadGroups( personAliasGuid, rockContext );
                return ActionOk( groups );
            }
        }

        #endregion
    }
}
