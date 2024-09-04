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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.LayoutDetail;
using Rock.ViewModels.Blocks.Core.PersonSignalList;
using Rock.ViewModels.Blocks.Crm.BadgeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of person signals.
    /// </summary>

    [DisplayName( "Person Signal List" )]
    [Category( "Core" )]
    [Description( "Displays a list of person signals." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "db2e3ce3-94bd-4d12-8add-598bf938e8e1" )]
    [Rock.SystemGuid.BlockTypeGuid( "653052a0-ca1c-41b8-8340-4b13149c6e66" )]
    [CustomizedGrid]
    public class PersonSignalList : RockEntityListBlockType<PersonSignal>
    {
        #region Keys

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new ListBlockBox<PersonSignalListOptionsBag>();
                var builder = GetGridBuilder();

                box.IsAddEnabled = GetIsAddEnabled();
                box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                box.ExpectedRowCount = null;
                box.Options = GetBoxOptions();
                box.GridDefinition = builder.BuildDefinition();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonSignalListOptionsBag GetBoxOptions()
        {
            var options = new PersonSignalListOptionsBag();
            options.SignalTypeOptions = SignalTypeCache.All()
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Id )
                .ToListItemBagList();

            return options;
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out PersonSignal entity, out BlockActionResult error )
        {
            var entityService = new PersonSignalService( rockContext );
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
                entity = new PersonSignal();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{PersonSignal.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( "Not authorized to make changes to this signal." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( PersonSignal entity, ValidPropertiesBox<PersonSignalBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var personAliasService = new PersonAliasService( rockContext );
            var person = this.RequestContext.GetContextEntity<Person>();

            if ( person == null )
            {
                return false;
            }
            entity.PersonId = person.Id;

            var isSignalTypeValid = box.IfValidProperty( nameof( box.Bag.SignalType ), () =>
            {
                var signalType = SignalTypeCache.Get( new Guid( box.Bag.SignalType.Value ) );
                if ( signalType == null )
                {
                    return false;
                }
                entity.SignalTypeId = signalType.Id;
                return true;
            }, true );
            if ( !isSignalTypeValid )
            {
                return false;
            }
            var isOwnerValid = box.IfValidProperty( nameof( box.Bag.Owner ), () =>
            {
                var owner = personAliasService.Get( box.Bag.Owner.Value.AsGuid() );
                if ( owner == null )
                {
                    return false;
                }
                entity.OwnerPersonAliasId = owner.Id;
                return true;
            }, true );
            if ( !isOwnerValid )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.ExpirationDate ),
                () => {
                    if ( box.Bag.ExpirationDate.HasValue )
                    {
                        entity.ExpirationDate = box.Bag.ExpirationDate.Value.DateTime.Date;
                    }
                    else
                    {
                        entity.ExpirationDate = null;
                    }
                } );
            box.IfValidProperty( nameof( box.Bag.Note ),
                () => entity.Note = box.Bag.Note );

            return true;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonSignal> GetListQueryable( RockContext rockContext )
        {
            var personInView = this.RequestContext.GetContextEntity<Person>();
            return base.GetListQueryable( rockContext ).Where( s => s.PersonId == personInView.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonSignal> GetGridBuilder()
        {
            return new GridBuilder<PersonSignal>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.SignalType.Name )
                .AddPersonField( "owner", a => a.OwnerPersonAlias.Person )
                .AddTextField( "note", a => a.Note )
                .AddDateTimeField( "expirationDate", a => a.ExpirationDate )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the signal representation for editing purposes.
        /// </summary>
        /// <param name="key">The unique identifier of the signal to be edited.</param>
        /// <returns>A response that includes the editable representation of the signal.</returns>
        [BlockAction]
        public BlockActionResult GetEditPersonSignal( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new PersonSignalService( RockContext );
                var entity = entityService.Get( key, false );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{PersonSignal.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( "Not authorized to make changes to this signal." );
                }

                var editBag = new PersonSignalBag
                {
                    Guid = entity.Guid,
                    IdKey = entity.IdKey,
                    SignalType = entity.SignalType.ToListItemBag(),
                    Owner = entity.OwnerPersonAlias.ToListItemBag(),
                    ExpirationDate = entity.ExpirationDate,
                    Note = entity.Note,

                };

                return ActionOk( editBag );
            }
        }

        [BlockAction]
        public BlockActionResult SavePersonSignal( ValidPropertiesBox<PersonSignalBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var personSignalService = new PersonSignalService( RockContext );

                if ( !TryGetEntityForEditAction( box.Bag.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionOk( GetGridBuilder() );
                }

                return ActionOk( GetGridBuilder() );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new PersonSignalService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{PersonSignal.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to make changes to this signal." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
