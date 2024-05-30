using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.ViewModels.Blocks.Crm.PhotoVerify;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    [DisplayName( "Verify Photo" )]
    [Category( "CRM" )]
    [Description( "Allows uploaded photos to be verified." )]

    [IntegerField(
        "Photo Size",
        Key = AttributeKey.PhotoSize,
        Description = "The size of the preview photo. Default is 65.",
        IsRequired = false,
        DefaultIntegerValue = 65,
        Order = 0 )]

    public class PhotoVerify : RockEntityListBlockType<Model.GroupMember>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PhotoSize = "PhotoSize";
        }

        #endregion

        #region Fields

        // used for private variables
        int size = 65;

        /// <summary>
        /// Group that manages the people using the Photo Request system.
        /// </summary>
        private Rock.Model.Group _photoRequestGroup = null;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PhotoVerifyBag>();
            var builder = GetGridBuilder();

            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            //box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PhotoVerifyBag GetBoxOptions()
        {
            size = GetAttributeValue( AttributeKey.PhotoSize ).AsInteger();
            if ( size <= 0 )
            {
                size = 65;
            }

            var photoVerifyBag = new PhotoVerifyBag
            {
                PhotoSize = size,
            };
            return photoVerifyBag;
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupMember> GetListQueryable( RockContext rockContext )
        {
            var guid = Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid();

            return new GroupMemberService( rockContext ).Queryable().Where( m => m.Group.Guid == guid );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Model.GroupMember> GetGridBuilder()
        {
            return new GridBuilder<Model.GroupMember>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "personIdKey", a => a.Person.IdKey )
                .AddField( "photo", a => a.Person.PhotoUrl )
                .AddDateTimeField( "created", a => a.CreatedDateTime )
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
            var groupMember = groupMemberService.Get(key);

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
                GroupMember groupMember = null;

                foreach ( string currentGroupMemberIdKey in selectedGroupMembers )
                {
                    groupMember = groupMemberService.Get(currentGroupMemberIdKey);
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
                //nbMessage.NotificationBoxType = NotificationBoxType.Success;
                //nbMessage.Text = string.Format( "Verified {0} photo{1}.", count, count > 1 ? "s" : "" );
            }
            else
            {
                ActionBadRequest("No photos selected.");
            }

            return ActionOk();
        }

        #endregion

    }
}
