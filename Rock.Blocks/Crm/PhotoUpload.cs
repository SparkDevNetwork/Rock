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
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks.Crm.PhotoUpload;

namespace Rock.Blocks.Crm
{
    [DisplayName( "Photo Upload" )]
    [Category( "CRM" )]
    [Description( "Allows a photo to be uploaded for the given person (logged in person) and optionally their family members." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Include Family Members",
        Key = AttributeKey.IncludeFamilyMembers,
        Description = "If checked, other family members will also be displayed allowing their photos to be uploaded.",
        DefaultBooleanValue = true,
        Order = 0 )]

    [BooleanField(
        "Allow Staff",
        Key = AttributeKey.AllowStaff,
        Description = "If checked, staff members will also be allowed to upload new photos for themselves.",
        DefaultBooleanValue = false,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "E9B8A70B-BB59-4044-900F-44150DA73300" )]
    [Rock.SystemGuid.BlockTypeGuid( "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" )]
    public class PhotoUpload : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string IncludeFamilyMembers = "IncludeFamilyMembers";
            public const string AllowStaff = "AllowStaff";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetPhotoUploadInitializationBox();
        }

        /// <summary>
        /// Sets the initial state of the Photo Upload Bag.
        /// </summary>
        private PhotoUploadInitializationBox GetPhotoUploadInitializationBox()
        {
            var photoUploadInitializationBox = new PhotoUploadInitializationBox
            {
                PersonPhotoList = new List<PersonPhotoBag>()
            };

            foreach ( var candidate in GetPhotoUploadCandidates() )
            {
                photoUploadInitializationBox.PersonPhotoList.Add( new PersonPhotoBag
                {
                    IdKey = candidate.Person.IdKey,
                    FullName = candidate.Person.FullName,
                    ProfilePhoto = candidate.Person.Photo.ToListItemBag(),
                    NoPhotoUrl = Rock.Model.Person.GetPersonNoPictureUrl( candidate.Person ),
                    IsStaffMemberDisabled = candidate.IsStaffMemberDisabled,
                } );
            }

            return photoUploadInitializationBox;
        }

        /// <summary>
        /// Gets the people this block offers to the current person: themselves, plus their family members when Include
        /// Family Members is enabled. A candidate flagged as a staff member is listed but may not be updated, so the
        /// displayed list and the update action both read eligibility from here and cannot disagree.
        /// </summary>
        /// <returns>The candidate people, or an empty list when there is no authenticated person.</returns>
        private List<(Rock.Model.Person Person, bool IsStaffMemberDisabled)> GetPhotoUploadCandidates()
        {
            var candidates = new List<(Rock.Model.Person Person, bool IsStaffMemberDisabled)>();
            var currentPerson = RequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return candidates;
            }

            var people = new List<Rock.Model.Person> { currentPerson };

            if ( GetAttributeValue( AttributeKey.IncludeFamilyMembers ).AsBoolean() )
            {
                people.AddRange( currentPerson.GetFamilyMembers( includeSelf: false, rockContext: RockContext )
                    .Select( m => m.Person )
                    .ToList() );
            }

            Rock.Model.Group staffGroup = null;

            if ( !GetAttributeValue( AttributeKey.AllowStaff ).AsBoolean() )
            {
                staffGroup = new GroupService( RockContext ).GetByGuid( Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS.AsGuid() );
            }

            foreach ( var person in people )
            {
                candidates.Add( (person, staffGroup != null && staffGroup.Members.Any( m => m.PersonId == person.Id )) );
            }

            return candidates;
        }

        /// <summary>
        /// Add the person (if not already existing) to the Photo Request group and set status to Pending.
        /// </summary>
        /// <param name="person">The person.</param>
        private void AddOrUpdatePersonInPhotoRequestGroup( Rock.Model.Person person )
        {
            GroupService service = new GroupService( RockContext );
            var photoRequestGroup = service.GetByGuid( Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid() );

            var groupMember = photoRequestGroup.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();

            if ( groupMember == null )
            {
                groupMember = new GroupMember
                {
                    GroupId = photoRequestGroup.Id,
                    PersonId = person.Id,
                    GroupRoleId = photoRequestGroup.GroupType.DefaultGroupRoleId ?? -1
                };
                photoRequestGroup.Members.Add( groupMember );
            }
            groupMember.GroupMemberStatus = GroupMemberStatus.Pending;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Updates the Profile Photo associated with a given Person.
        /// </summary>
        /// <param name="personIdKey">The identifier of the Person.</param>
        /// <param name="photoGuid">The identifier of the photo.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult UpdatePersonProfilePhoto( string personIdKey, Guid photoGuid )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized();
            }

            PersonService personService = new PersonService( RockContext );
            var person = personService.Get( personIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            var isPersonEditable = person != null
                && GetPhotoUploadCandidates().Any( c => c.Person.Id == person.Id && !c.IsStaffMemberDisabled );

            if ( !isPersonEditable )
            {
                // An ineligible person gets the same response as a missing one so a caller cannot learn which identifiers exist.
                return ActionBadRequest( "Person not found." );
            }

            person.PhotoId = new BinaryFileService( RockContext ).GetId( photoGuid );

            if ( person.PhotoId == null )
            {
                return ActionBadRequest( "Profile photo not found." );
            }

            AddOrUpdatePersonInPhotoRequestGroup( person );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
