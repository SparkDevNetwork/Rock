using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotLiquid;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks.Core.PersonSignalList;
using Rock.ViewModels.Blocks.Crm.PhotoUpload;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Allows a person to opt-out of future photo requests.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Photo Upload" )]
    [Category( "CRM" )]
    [Description( "Allows a photo to be uploaded for the given person (logged in person) and optionally their family members." )]

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

    // [SupportedSiteTypes( Model.SiteType.Web )]


    public class PhotoUpload : RockBlockType
    {

        private static class AttributeKey
        {
            public const string IncludeFamilyMembers = "IncludeFamilyMembers";
            public const string AllowStaff = "AllowStaff";
        }

        /// <summary>
        /// This is special "staff" group but it's only loaded when the AllowStaff block attribute is set to false.
        /// </summary>
        private Rock.Model.Group _staffGroup = null;

        /// <summary>
        /// Group that manages the people using the Photo Request system.
        /// </summary>
        private Rock.Model.Group _photoRequestGroup = null;

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            if ( ! GetAttributeValue( AttributeKey.AllowStaff ).AsBoolean() )
                {
                    GroupService service = new GroupService( new RockContext() );
                    _staffGroup = service.GetByGuid( new Guid( Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS ) );
                }

            return GetPhotoUploadBag();
        }

        private PhotoUploadBag GetPhotoUploadBag()
        {
            var photoUploadBag = new PhotoUploadBag
            {
                PersonPhotoList = new List<PersonPhotoBag>()
            };

            var loggedInPerson = RequestContext.CurrentPerson;

            if ( loggedInPerson != null )
            {
                var isStaffMember = false;
                if ( _staffGroup != null && _staffGroup.Members.Where( m => m.PersonId == loggedInPerson.Id ).Count() > 0 )
                {
                    isStaffMember = true;
                }
                PersonPhotoBag currentPersonPhotoBag = new PersonPhotoBag
                {
                    Id = loggedInPerson.Id,
                    FullName = loggedInPerson.FullName,
                    ProfilePhoto = loggedInPerson.Photo.ToListItemBag(),
                    NoPhotoUrl = Rock.Model.Person.GetPersonNoPictureUrl( loggedInPerson ),
                    IsStaffMember = isStaffMember,
                };
                photoUploadBag.PersonPhotoList.Add( currentPersonPhotoBag );

                if ( GetAttributeValue( AttributeKey.IncludeFamilyMembers ).AsBoolean() )
                {
                    foreach ( var member in loggedInPerson.GetFamilyMembers( includeSelf: false ).ToList() )
                    {
                        if ( _staffGroup != null && _staffGroup.Members.Where( m => m.PersonId == member.Id ).Count() > 0 )
                        {
                            isStaffMember = true;
                        }
                        var familyMemberPhotoBag = new PersonPhotoBag
                        {
                            Id = member.Person.Id,
                            FullName = member.Person.FullName,
                            ProfilePhoto = member.Person.Photo.ToListItemBag(),
                            NoPhotoUrl = Rock.Model.Person.GetPersonNoPictureUrl(member.Person)
                        };
                        photoUploadBag.PersonPhotoList.Add( familyMemberPhotoBag );
                    }
                }
            }

            return photoUploadBag;
        }

        /// <summary>
        /// Add the person (if not already existing) to the Photo Request group and set status to Pending.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddOrUpdatePersonInPhotoRequestGroup( Rock.Model.Person person, RockContext rockContext )
        {
            if ( _photoRequestGroup == null )
            {
                GroupService service = new GroupService( rockContext );
                _photoRequestGroup = service.GetByGuid( Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid() );
            }

            var groupMember = _photoRequestGroup.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();
            if ( groupMember == null )
            {
                groupMember = new GroupMember();
                groupMember.GroupId = _photoRequestGroup.Id;
                groupMember.PersonId = person.Id;
                groupMember.GroupRoleId = _photoRequestGroup.GroupType.DefaultGroupRoleId ?? -1;
                _photoRequestGroup.Members.Add( groupMember );
            }

            groupMember.GroupMemberStatus = GroupMemberStatus.Pending;
        }

        #region Block Actions

        [BlockAction]
        public BlockActionResult UpdatePersonProfilePhoto( int personId, string photoGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                PersonService personService = new PersonService( rockContext );
                var person = personService.Get( personId );
                person.PhotoId = new BinaryFileService( rockContext ).GetId( photoGuid.AsGuid() );

                AddOrUpdatePersonInPhotoRequestGroup( person, rockContext );

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}
