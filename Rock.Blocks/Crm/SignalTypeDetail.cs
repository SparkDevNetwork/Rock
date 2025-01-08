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
using Rock.Tasks;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.SignalTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays the details of a particular signal type.
    /// </summary>
    [DisplayName( "Person Signal Type Detail" )]
    [Category( "CRM" )]
    [Description( "Shows the details of a particular person signal type." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "e7b94691-bb91-4995-b2a0-3c3724224250" )]
    [Rock.SystemGuid.BlockTypeGuid( "8b65ee51-4075-4fc0-b1a9-f56c7153aa77" )]
    public class SignalTypeDetail : RockEntityDetailBlockType<SignalType, SignalTypeBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SignalTypeId = "SignalTypeId";
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
            var box = new DetailBlockBox<SignalTypeBag, SignalTypeDetailOptionsBag>();

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
        private SignalTypeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new SignalTypeDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the SignalType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="signalType">The SignalType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the SignalType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSignalType( SignalType signalType, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<SignalTypeBag, SignalTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {SignalType.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) || BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                box.Entity = GetEntityBagForView( entity );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( SignalType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="SignalTypeBag"/> that represents the entity.</returns>
        private SignalTypeBag GetCommonEntityBag( SignalType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new SignalTypeBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                Name = entity.Name,
                Order = entity.Order,
                SignalColor = entity.SignalColor,
                SignalIconCssClass = entity.SignalIconCssClass,
                CanAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
            };
        }

        /// <inheritdoc/>
        protected override SignalTypeBag GetEntityBagForView( SignalType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override SignalTypeBag GetEntityBagForEdit( SignalType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( SignalType entity, ValidPropertiesBox<SignalTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Order ),
                () => entity.Order = box.Bag.Order );

            box.IfValidProperty( nameof( box.Bag.SignalColor ),
                () => entity.SignalColor = box.Bag.SignalColor );

            box.IfValidProperty( nameof( box.Bag.SignalIconCssClass ),
                () => entity.SignalIconCssClass = box.Bag.SignalIconCssClass );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override SignalType GetInitialEntity()
        {
            return GetInitialEntity<SignalType, SignalTypeService>( RockContext, PageParameterKey.SignalTypeId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out SignalType entity, out BlockActionResult error )
        {
            var entityService = new SignalTypeService( RockContext );
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
                entity = new SignalType();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{SignalType.FriendlyTypeName} not found." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates the top-most signal and updates the person properties for Persons with this SignalType.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="signalTypeId">The signal type identifier.</param>
        private static void RecalculateSignals( RockContext rockContext, int signalTypeId )
        {
            var people = new PersonSignalService( rockContext ).Queryable()
                .Where( s => s.SignalTypeId == signalTypeId )
                .Select( s => s.PersonId )
                .Distinct()
                .ToList();

            //
            // If less than 250 people with this signal type then just update them all now,
            // otherwise put something in the rock queue to take care of it.
            //
            if ( people.Count < 250 )
            {
                new PersonService( rockContext ).Queryable()
                    .Where( p => people.Contains( p.Id ) )
                    .ToList()
                    .ForEach( p => p.CalculateSignals() );

                rockContext.SaveChanges();
            }
            else
            {
                var updatePersonSignalTypesMsg = new UpdatePersonSignalTypes.Message()
                {
                    PersonIds = people
                };

                updatePersonSignalTypesMsg.Send();
            }
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

            return ActionOk( new ValidPropertiesBox<SignalTypeBag>
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
        public BlockActionResult Save( ValidPropertiesBox<SignalTypeBag> box )
        {
            var entityService = new SignalTypeService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden();
            }

            // Update the entity instance from the information in the bag.  
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateSignalType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            RecalculateSignals( RockContext, entity.Id );

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
            var entityService = new SignalTypeService( RockContext );

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

            RecalculateSignals( RockContext, entity.Id );

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
