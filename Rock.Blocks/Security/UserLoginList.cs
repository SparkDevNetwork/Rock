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
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.UserLoginList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Block for displaying logins.  By default displays all logins, but can be configured to use person context to display logins for a specific person.
    /// </summary>
    [DisplayName( "User Login List" )]
    [Category( "Security" )]
    [Description( "Block for displaying logins.  By default displays all logins, but can be configured to use person context to display logins for a specific person." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [ContextAware]

    [SystemGuid.EntityTypeGuid( "b74114da-830b-45cf-a04b-e96c5d27783f" )]
    [SystemGuid.BlockTypeGuid( "dbfa9e41-fa62-4869-8a44-d03b561433b2" )]
    [CustomizedGrid]
    public class UserLoginList : RockEntityListBlockType<UserLogin>
    {
        #region Keys

        private static class PreferenceKey
        {
            public const string FilterUsername = "filter-username";
            public const string FilterAuthenticationProvider = "filter-authentication-provider";
            public const string FilterDateCreatedUpperValue = "filter-date-created-upper-value";
            public const string FilterDateCreatedLowerValue = "filter-date-created-lower-value";
            public const string FilterLastLoginDateUpperValue = "filter-last-login-date-upper-value";
            public const string FilterLastLoginDateLowerValue = "filter-last-login-date-lower-value";
            public const string FilterIsConfirmed = "filter-is-confirmed";
            public const string FilterIsLockedOut = "filter-is-locked-out";
        }

        #endregion Keys

        #region Properties

        protected string FilterUsername => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterUsername );

        protected Guid? FilterAuthenticationProvider => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterAuthenticationProvider )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        protected DateTime? FilterDateCreatedUpperValue => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateCreatedUpperValue )
            .AsDateTime();

        protected DateTime? FilterDateCreatedLowerValue => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateCreatedLowerValue )
            .AsDateTime();

        protected DateTime? FilterLastLoginDateUpperValue => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterLastLoginDateUpperValue )
            .AsDateTime();

        protected DateTime? FilterLastLoginDateLowerValue => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterLastLoginDateLowerValue )
            .AsDateTime();

        protected bool? FilterIsConfirmed => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIsConfirmed )
            .AsBooleanOrNull();

        protected bool? FilterIsLockedOut => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIsLockedOut )
            .AsBooleanOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<UserLoginListOptionsBag>();
            var builder = GetGridBuilder();
            var isAddDeleteEnabled = GetIsAddDeleteEnabled();

            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private UserLoginListOptionsBag GetBoxOptions()
        {
            var options = new UserLoginListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<UserLogin> GetListQueryable( RockContext rockContext )
        {
            var personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            if ( BlockCache.ContextTypesRequired.Exists( e => e.Id == personEntityTypeId ) && RequestContext.GetContextEntity<Person>() == null )
            {
                return new List<UserLogin>().AsQueryable();
            }

            var personId = RequestContext.GetContextEntity<Person>()?.Id;
            var queryable = new UserLoginService( rockContext ).Queryable()
                .Where( l => !personId.HasValue || l.PersonId == personId.Value );

            // username filter
            if ( !string.IsNullOrWhiteSpace( FilterUsername ) )
            {
                queryable = queryable.Where( l => l.UserName.StartsWith( FilterUsername ) );
            }

            // provider filter
            if ( FilterAuthenticationProvider.HasValue )
            {
                queryable = queryable.Where( l => l.EntityType.Guid.Equals( FilterAuthenticationProvider.Value ) );
            }

            // created filter
            if ( FilterDateCreatedLowerValue.HasValue )
            {
                queryable = queryable.Where( l => l.CreatedDateTime.HasValue && DbFunctions.TruncateTime( l.CreatedDateTime ).Value >= DbFunctions.TruncateTime( FilterDateCreatedLowerValue.Value ) );
            }

            if ( FilterDateCreatedUpperValue.HasValue )
            {
                var upperDate = FilterDateCreatedUpperValue.Value.Date.AddDays( 1 );
                queryable = queryable.Where( l => l.CreatedDateTime.HasValue && DbFunctions.TruncateTime( l.CreatedDateTime ) < upperDate );
            }

            // last login filter
            if ( FilterLastLoginDateLowerValue.HasValue )
            {
                queryable = queryable.Where( l => DbFunctions.TruncateTime( l.LastLoginDateTime ) >= DbFunctions.TruncateTime( FilterLastLoginDateLowerValue.Value ) );
            }

            if ( FilterLastLoginDateUpperValue.HasValue )
            {
                var upperDate = FilterLastLoginDateUpperValue.Value.Date.AddDays( 1 );
                queryable = queryable.Where( l => DbFunctions.TruncateTime( l.LastLoginDateTime ) < upperDate );
            }

            // is Confirmed filter
            if ( FilterIsConfirmed.HasValue )
            {
                queryable = queryable.Where( l => l.IsConfirmed == FilterIsConfirmed.Value || ( !FilterIsConfirmed.Value && l.IsConfirmed == null ) );
            }

            // is locked out filter
            if ( FilterIsLockedOut.HasValue )
            {
                queryable = queryable.Where( l => l.IsLockedOut == FilterIsLockedOut.Value || ( !FilterIsLockedOut.Value && l.IsLockedOut == null ) );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<UserLogin> GetOrderedListQueryable( IQueryable<UserLogin> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( u => u.UserName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<UserLogin> GetGridBuilder()
        {
            return new GridBuilder<UserLogin>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "userName", a => a.UserName )
                .AddPersonField( "person", a => a.Person )
                .AddTextField( "provider", a => GetComponentName( GetProvider( a.EntityTypeId ) ) )
                .AddDateTimeField( "dateCreated", a => a.CreatedDateTime )
                .AddDateTimeField( "lastLogin", a => a.LastLoginDateTime )
                .AddField( "isConfirmed", a => a.IsConfirmed )
                .AddField( "isLockedOut", a => a.IsLockedOut )
                .AddField( "isPasswordChangeRequired", a => a.IsPasswordChangeRequired );
        }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        private EntityTypeCache GetProvider( int? entityTypeId )
        {
            return entityTypeId.HasValue ? EntityTypeCache.Get( entityTypeId.Value ) : null;
        }

        /// <summary>
        /// Gets the bag for editing the User Login.
        /// </summary>
        /// <param name="entity">The entity to be represented for editing purposes.</param>
        /// <returns>A <see cref="UserLoginBag"/> that represents the entity.</returns>
        private UserLoginBag GetEntityBagForEdit( UserLogin entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new UserLoginBag()
            {
                IdKey = entity.IdKey,
                UserName = entity.UserName,
                IsConfirmed = entity.IsConfirmed,
                IsLockedOut = entity.IsLockedOut,
                IsPasswordChangeRequired = entity.IsPasswordChangeRequired,
                PersonAlias = entity.Person.PrimaryAlias.ToListItemBag(),
            };

            var componentEntityType = GetProvider( entity.EntityTypeId );
            if ( componentEntityType != null )
            {
                bag.AuthenticationProvider = new ViewModels.Utility.ListItemBag()
                {
                    Value = componentEntityType.Guid.ToString(),
                    Text = GetComponentName( componentEntityType )
                };
            }

            return bag;
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="componentEntityType">Type of the component entity.</param>
        /// <returns></returns>
        private string GetComponentName( EntityTypeCache componentEntityType )
        {
            var componentName = Rock.Reflection.GetDisplayName( componentEntityType.GetEntityType() );

            // If it has a DisplayName use it as is, otherwise use the original logic
            if ( string.IsNullOrWhiteSpace( componentName ) )
            {
                componentName = AuthenticationContainer.GetComponentName( componentEntityType.Name );
                // If the component name already has a space then trust
                // that they are using the exact name formatting they want.
                if ( !componentName.Contains( ' ' ) )
                {
                    componentName = componentName.SplitCase();
                }
            }

            return componentName;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new UserLoginService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{UserLogin.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {UserLogin.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the specified entity for editing.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${UserLogin.FriendlyTypeName}." );
            }

            var entityService = new UserLoginService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                var person = RequestContext.GetContextEntity<Person>();

                entity = new UserLogin
                {
                    Person = person
                };
            }

            return ActionOk( GetEntityBagForEdit( entity ) );
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="bag">The bag that contains all the information required to save.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( UserLoginBag bag )
        {
            var entityService = new UserLoginService( RockContext );
            UserLogin entity = null;

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${DefinedValue.FriendlyTypeName}." );
            }

            if ( bag.IdKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            // Check to see if there is a change to the username, and if so check that the new username does not exist.
            var newUserName = bag.UserName;
            if ( ( entity == null || ( entity.UserName != newUserName ) ) && entityService.GetByUserName( newUserName ) != null )
            {
                // keep looking until we find the next available one 
                var numericSuffix = 1;
                var nextAvailableUserName = newUserName + numericSuffix.ToString();
                while ( entityService.GetByUserName( nextAvailableUserName ) != null )
                {
                    numericSuffix++;
                    nextAvailableUserName = newUserName + numericSuffix.ToString();
                }

                return ActionBadRequest( $"The User Name you selected already exists. Next available username: {nextAvailableUserName}" );
            }

            if ( entity == null )
            {
                var person = RequestContext.GetContextEntity<Person>();
                entity = new UserLogin();

                if ( person != null )
                {
                    entity.PersonId = person.Id;
                }
                else if ( bag.PersonAlias != null )
                {
                    var personAliasGuid = bag.PersonAlias.Value.AsGuid();
                    var personAlias = new PersonAliasService( RockContext ).Get( personAliasGuid );
                    entity.PersonId = personAlias?.PersonId;
                }

                if ( !entity.PersonId.HasValue )
                {
                    return ActionBadRequest("No person selected, or the person you are editing has no person Id.");
                }

                entityService.Add( entity );
            }

            entity.UserName = newUserName;
            entity.IsConfirmed = bag.IsConfirmed;
            entity.IsLockedOut = bag.IsLockedOut;
            entity.IsPasswordChangeRequired = bag.IsPasswordChangeRequired;

            var entityType = EntityTypeCache.Get( bag.AuthenticationProvider?.Value?.AsGuid() ?? Guid.Empty );
            if ( entityType != null )
            {
                entity.EntityTypeId = entityType.Id;

                if ( !string.IsNullOrWhiteSpace( bag.Password ) )
                {
                    var component = AuthenticationContainer.GetComponent( entityType.Name );
                    if ( component != null && component.ServiceType == AuthenticationServiceType.Internal )
                    {
                        if ( bag.Password == bag.ConfirmPassword )
                        {
                            if ( UserLoginService.IsPasswordValid( bag.Password ) )
                            {
                                entity.Password = component.EncodePassword( entity, bag.Password );
                                entity.LastPasswordChangedDateTime = RockDateTime.Now;
                            }
                            else
                            {
                                return ActionBadRequest( UserLoginService.FriendlyPasswordRules() );
                            }
                        }
                        else
                        {
                            return ActionBadRequest( "Password and Confirmation do not match." );
                        }
                    }
                }
            }

            if ( !entity.IsValid )
            {
                var errorMessage = entity.ValidationResults.ConvertAll( x => x.ErrorMessage );
                return ActionBadRequest( errorMessage.JoinStrings( "<br>" ) );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the specified component.
        /// </summary>
        /// <param name="entityTypeGuid">The identifier of the component entity type.</param>
        [BlockAction]
        public BlockActionResult GetComponent( Guid entityTypeGuid )
        {
            var componentEntityType = EntityTypeCache.Get( entityTypeGuid );
            var component = AuthenticationContainer.GetComponent( componentEntityType?.Name );

            if ( component == null )
            {
                return ActionBadRequest( "Unable to find component." );
            }

            return ActionOk( new AuthenticationComponentBag() { PromptForPassword = component.PromptForPassword, SupportsChangePassword = component.SupportsChangePassword } );
        }

        #endregion
    }
}
