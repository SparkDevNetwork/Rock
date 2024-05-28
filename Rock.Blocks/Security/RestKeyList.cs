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
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.RestKeyList;
using Rock.Web.Cache;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays a list of people.
    /// </summary>
    [DisplayName( "Rest Key List" )]
    [Category( "Security" )]
    [Description( "Lists all the REST API Keys" )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the person details.",
        Key = AttributeKey.DetailPage )]

    [SystemGuid.EntityTypeGuid( "55e010d1-152a-4745-8e1d-2db2195f2b36" )]
    [SystemGuid.BlockTypeGuid( "40b6af94-5ffc-4ee3-add9-c76818992274" )]
    [CustomizedGrid]
    public class RestKeyList : RockEntityListBlockType<Person>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RestKeyListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RestKeyListOptionsBag GetBoxOptions()
        {
            var options = new RestKeyListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "RestUserId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Person> GetListQueryable( RockContext rockContext )
        {
            var restUserRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            return new PersonService( RockContext ).Queryable()
                .Where( q => q.RecordTypeValueId == restUserRecordTypeId && q.Users.Any() );
        }

        protected override IQueryable<Person> GetOrderedListQueryable( IQueryable<Person> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( r => r.LastName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Person> GetGridBuilder()
        {
            var activeRecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            return new GridBuilder<Person>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "lastName", a => a.LastName )
                .AddTextField( "description", a => GetDescription( a ) )
                .AddTextField( "key", a => GetKey( a ) )
                .AddTextField( "status", a => a.RecordStatusValue.Value )
                .AddField( "isActive", a => a.RecordStatusValueId == activeRecordStatusValueId );
        }

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <param name="restUser">The rest user</param>
        /// <returns></returns>
        private string GetKey( Person restUser )
        {
            var userLoginService = new UserLoginService( RockContext );
            var userLogin = userLoginService.Queryable().Where( a => a.PersonId == restUser.Id ).FirstOrDefault();
            return userLogin?.ApiKey;
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <param name="restUser">The rest user</param>
        /// <returns></returns>
        private string GetDescription( Person restUser )
        {
            var noteService = new NoteService( RockContext );
            var description = noteService.Queryable().Where( a => a.EntityId == restUser.Id ).FirstOrDefault();
            return description?.Text;
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
            var entityService = new PersonService( RockContext );
            var userLoginService = new UserLoginService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Person.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Person.FriendlyTypeName}." );
            }

            entity.RecordStatusValueId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;

            // remove all user logins for key
            foreach ( var login in entity.Users.ToList() )
            {
                userLoginService.Delete( login );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
