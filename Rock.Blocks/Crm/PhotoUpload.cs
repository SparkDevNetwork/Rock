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
using Rock.ViewModels.Blocks.Crm.PhotoUpload;

namespace Rock.Blocks.Crm
{
    [DisplayName( "Photo Upload" )]
    [Category( "CRM" )]
    [Description( "Allows a photo to be uploaded for the given person (logged in person) and optionally their family members." )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

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

        #region Properties

        /// <summary>
        /// This is special "staff" group but it's only loaded when the AllowStaff block attribute is set to false.
        /// </summary>
        private Rock.Model.Group _staffGroup = null;

        /// <summary>
        /// Group that manages the people using the Photo Request system.
        /// </summary>
        private Rock.Model.Group _photoRequestGroup = null;

        #endregion

        #region Methods

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

        /// <summary>
        /// Sets the initial state of the Photo Upload Bag.
        /// </summary>
        private PhotoUploadBag GetPhotoUploadBag()
        {
            var photoUploadBag = new PhotoUploadBag
            {
                PersonPhotoList = new List<PersonPhotoBag>()
            };
            var loggedInPerson = RequestContext.CurrentPerson;

            if ( loggedInPerson != null )
            {
                var isStaffMemberDisabled = false;

                if ( _staffGroup != null && _staffGroup.Members.Where( m => m.PersonId == loggedInPerson.Id ).Count() > 0 )
                {
                    isStaffMemberDisabled = true;
                }

                photoUploadBag.PersonPhotoList.Add( new PersonPhotoBag
                {
                    IdKey = loggedInPerson.IdKey,
                    FullName = loggedInPerson.FullName,
                    ProfilePhoto = loggedInPerson.Photo.ToListItemBag(),
                    NoPhotoUrl = Rock.Model.Person.GetPersonNoPictureUrl( loggedInPerson ),
                    IsStaffMemberDisabled = isStaffMemberDisabled,
                } );

                if ( GetAttributeValue( AttributeKey.IncludeFamilyMembers ).AsBoolean() )
                {
                    foreach ( var member in loggedInPerson.GetFamilyMembers( includeSelf: false ).ToList() )
                    {
                        isStaffMemberDisabled = false;

                        if ( _staffGroup != null && _staffGroup.Members.Where( m => m.PersonId == member.Id ).Count() > 0 )
                        {
                            isStaffMemberDisabled = true;
                        }

                        photoUploadBag.PersonPhotoList.Add( new PersonPhotoBag
                        {
                            IdKey = member.Person.IdKey,
                            FullName = member.Person.FullName,
                            ProfilePhoto = member.Person.Photo.ToListItemBag(),
                            NoPhotoUrl = Rock.Model.Person.GetPersonNoPictureUrl( member.Person ),
                            IsStaffMemberDisabled = isStaffMemberDisabled,
                        } );
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

        #endregion

        #region Block Actions

        /// <summary>
        /// Updates the Profile Photo associated with a given Person.
        /// </summary>
        /// <param name="personIdKey">The identifier of the Person.</param>
        /// <param name="photoGuid">The identifier of the photo.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult UpdatePersonProfilePhoto( string personIdKey, string photoGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                PersonService personService = new PersonService( rockContext );
                var person = personService.Get( personIdKey );

                if ( person == null )
                {
                    return ActionBadRequest( "Person not found." );
                }

                person.PhotoId = new BinaryFileService( rockContext ).GetId( photoGuid.AsGuid() );
                AddOrUpdatePersonInPhotoRequestGroup( person, rockContext );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}
