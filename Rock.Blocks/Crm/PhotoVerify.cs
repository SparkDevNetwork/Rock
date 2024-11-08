using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PhotoVerifyList;

namespace Rock.Blocks.Crm
{
    [DisplayName( "Verify Photo" )]
    [Category( "CRM" )]
    [Description( "Allows uploaded photos to be verified." )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [IntegerField(
        "Photo Size",
        Key = AttributeKey.PhotoSize,
        Description = "The size of the preview photo. Default is 65.",
        IsRequired = false,
        DefaultIntegerValue = 150,
        Order = 0 )]

    [Rock.SystemGuid.EntityTypeGuid( "0582B38F-C622-4F5B-A4BB-FD04B07EEE1B" )]
    [Rock.SystemGuid.BlockTypeGuid( "1228F248-6AA1-4871-AF9E-195CF0FDA724" )]
    public class PhotoVerify : RockEntityListBlockType<Model.GroupMember>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PhotoSize = "PhotoSize";
        }

        private static class PreferenceKey
        {
            public const string FilterShowVerifiedPhotos = "filter-show-verified-photos";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the filter indicating whether verified photos should be included in the results.
        /// </summary>
        /// <value>
        /// The filter show verified photos.
        /// </value>
        protected string FilterShowVerifiedPhotos => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterShowVerifiedPhotos );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PhotoVerifyListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PhotoVerifyListOptionsBag GetBoxOptions()
        {
            var photoSize = GetAttributeValue( AttributeKey.PhotoSize ).AsInteger();

            if ( photoSize <= 0 )
            {
                photoSize = 150;
            }

            var PhotoVerifyListOptionsBag = new PhotoVerifyListOptionsBag
            {
                PhotoSize = photoSize,
            };

            return PhotoVerifyListOptionsBag;
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupMember> GetListQueryable( RockContext rockContext )
        {
            var service = new GroupMemberService( rockContext );

            var guid = Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid();

            var query = service.Queryable().Where( m => m.Group.Guid == guid );

            var showVerifiedPhotos = FilterShowVerifiedPhotos.AsBooleanOrNull() ?? false;

            if ( showVerifiedPhotos )
            {
                query = query.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active || m.GroupMemberStatus == GroupMemberStatus.Pending );
            }
            else
            {
                query = query.Where( m => m.GroupMemberStatus == GroupMemberStatus.Pending );
            }

            return query;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Model.GroupMember> GetGridBuilder()
        {
            return new GridBuilder<Model.GroupMember>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "personIdKey", a => a.Person.IdKey )
                .AddField( "photo", a => a.Person.PhotoUrl )
                .AddDateTimeField( "created", a => a.Person.Photo?.CreatedDateTime )
                .AddTextField( "name", a => a.Person.FullName )
                .AddTextField( "gender", a => a.Person.Gender.ConvertToString() )
                .AddTextField( "email", a => a.Person.Email )
                .AddField( "status", a => a.GroupMemberStatus );
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
            var groupMemberService = new GroupMemberService( RockContext );
            var groupMember = groupMemberService.Get( key );

            if ( groupMember == null )
            {
                return ActionBadRequest( $"Person not found." );
            }

            var binaryFileService = new BinaryFileService( RockContext );

            binaryFileService.Delete( groupMember.Person.Photo );

            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
            groupMember.Person.Photo = null;
            groupMember.Person.PhotoId = null;

            RockContext.SaveChanges();

            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult VerifySelectedMembers( List<string> selectedGroupMembers )
        {
            int count = 0;

            if ( selectedGroupMembers.Any() )
            {
                var groupMemberService = new GroupMemberService( RockContext );

                foreach ( string currentGroupMemberIdKey in selectedGroupMembers )
                {
                    GroupMember groupMember = groupMemberService.Get( currentGroupMemberIdKey );
                    if ( groupMember != null )
                    {
                        count++;
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    }
                }

                RockContext.SaveChanges();
            }

            if ( count > 0 )
            {
                return ActionOk();
            }
            else
            {
                return ActionBadRequest( "No photos selected." );
            }
        }

        #endregion

    }
}