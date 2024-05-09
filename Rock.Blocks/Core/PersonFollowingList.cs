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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.PersonFollowingList;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of followings.
    /// </summary>
    [DisplayName( "Person Following List" )]
    [Category( "Follow" )]
    [Description( "Block for displaying people that current person follows." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "030b944d-66b5-4edb-aa38-10081e2acfb6" )]
    [Rock.SystemGuid.BlockTypeGuid( "18fa879f-1466-413b-8623-834d728f677b" )]
    [CustomizedGrid]
    public class PersonFollowingList : RockEntityListBlockType<Person>
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonFollowingListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonFollowingListOptionsBag GetBoxOptions()
        {
            var options = new PersonFollowingListOptionsBag()
            {
                HomePhoneDefinedValueText = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Value,
                MobilePhoneDefinedValueText = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ).Value
            };
            return options;
        }

        /// <inheritdoc/>
        protected override GridDataBag GetGridDataBag( RockContext rockContext )
        {
            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( rockContext );
            qry = GetOrderedListQueryable( qry, rockContext );

            // Get the items from the queryable.
            var items = GetListItems( qry, rockContext );

            return GetGridBuilder().Build( items );
        }

        /// <inheritdoc/>
        protected override IQueryable<Person> GetListQueryable( RockContext rockContext )
        {
            var currentPerson = GetCurrentPerson();
            return new FollowingService( RockContext ).GetFollowedPersonItems( currentPerson.PrimaryAliasId.Value );
        }

        /// <inheritdoc/>
        protected override IQueryable<Person> GetOrderedListQueryable( IQueryable<Person> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( p => p.LastName ).ThenBy( p => p.NickName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Person> GetGridBuilder()
        {
            return new GridBuilder<Person>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "name", a => a )
                .AddDateTimeField( "birthDate", a => a.BirthDate )
                .AddTextField( "email", a => a.Email )
                .AddField( "guid", a => a.Guid )
                .AddTextField( "homePhone", a => GetHomePhone( a ) )
                .AddTextField( "cellPhone", a => GetCellPhone( a ) )
                .AddPersonField( "spouse", a => GetSpouse( a ) );
        }

        /// <summary>
        /// Gets the person's spouse.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private Person GetSpouse( Person person )
        {
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var marriedGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();

            return person.Members.Where( m =>
                    person.MaritalStatusValue.Guid.Equals( marriedGuid ) &&
                    m.GroupRole.Guid.Equals( adultGuid ) )
                .SelectMany( m => m.Group.Members )
                .Where( m =>
                    m.PersonId != person.Id &&
                    m.GroupRole.Guid.Equals( adultGuid ) &&
                    m.Person.MaritalStatusValue.Guid.Equals( marriedGuid ) )
                .Select( s => s.Person )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the person's cell phone.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private string GetCellPhone( Person person )
        {
            var cellPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
            return person.PhoneNumbers.Where( n => n.NumberTypeValue.Guid.Equals( cellPhoneGuid ) )
                .Select( n => n.NumberFormatted )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the person's home phone.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private string GetHomePhone( Person person )
        {
            var homePhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
            return person.PhoneNumbers.Where( n => n.NumberTypeValue.Guid.Equals( homePhoneGuid ) )
                .Select( n => n.NumberFormatted )
                .FirstOrDefault();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Unfollows the specified persons.
        /// </summary>
        /// <param name="selectedPersons">The person guids.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Unfollow( List<Guid> selectedPersons )
        {
            if ( selectedPersons.Any() )
            {
                var personAliasService = new PersonAliasService( RockContext );
                var followingService = new FollowingService( RockContext );
                var currentPerson = GetCurrentPerson();

                var paQry = personAliasService.Queryable()
                    .Where( p => selectedPersons.Contains( p.Person.Guid ) )
                    .Select( p => p.Id );

                int personAliasEntityTypeId = EntityTypeCache.Get( "Rock.Model.PersonAlias" ).Id;
                foreach ( var following in followingService.Queryable()
                    .Where( f =>
                        f.EntityTypeId == personAliasEntityTypeId &&
                        string.IsNullOrEmpty( f.PurposeKey ) &&
                        paQry.Contains( f.EntityId ) &&
                        f.PersonAliasId == currentPerson.PrimaryAliasId ) )
                {
                    followingService.Delete( following );
                }

                RockContext.SaveChanges();
            }

            return ActionOk();
        }

        #endregion
    }
}
