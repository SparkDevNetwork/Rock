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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Allows a photo to be uploaded for the given person (logged in person) and optionally their family members.
    /// </summary>
    [DisplayName( "Upload" )]
    [Category( "CRM > PhotoRequest" )]
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

    [Rock.SystemGuid.BlockTypeGuid( "7764E323-7460-4CB7-8024-056136C99603" )]
    public partial class PhotoUpload : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string IncludeFamilyMembers = "IncludeFamilyMembers";
            public const string AllowStaff = "AllowStaff";
        }
        #endregion Attribute Keys

        #region Fields

        /// <summary>
        /// This is special "staff" group but it's only loaded when the AllowStaff block attribute is set to false.
        /// </summary>
        private Group _staffGroup = null;

        /// <summary>
        /// Group that manages the people using the Photo Request system.
        /// </summary>
        private Group _photoRequestGroup = null;

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            nbWarning.Visible = false;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( ! Page.IsPostBack )
            {
                if ( ! GetAttributeValue( AttributeKey.AllowStaff ).AsBoolean() )
                {
                    GroupService service = new GroupService( new RockContext() );
                    _staffGroup = service.GetByGuid( new Guid( Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS ) );
                }
                BindRepeater();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles adjusting the image editor control for each person bound to the list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPhotos_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var person = e.Item.DataItem as Person;
            
            var imageEditor = e.Item.FindControl( "imgedPhoto" ) as Rock.Web.UI.Controls.ImageEditor;
            imageEditor.BinaryFileId = person.PhotoId;
            imageEditor.NoPictureUrl = Person.GetPersonNoPictureUrl( person );
            imageEditor.Label = string.Format( "{0}", person.FullName );

            if ( _staffGroup != null && _staffGroup.Members.Where( m => m.PersonId == person.Id ).Count() > 0 )
            {
                //imageEditor.ButtonCssClass = "invisible";
                imageEditor.ButtonCssClass = "btn btn-default btn-sm margin-t-sm aspNetDisabled";
                imageEditor.ButtonText = "<i class='fa fa-ban'></i> Staff Member";
            }
            
        }

        /// <summary>
        /// Handles connecting the saved image file to the person Id specified in the CommandArgument control
        /// attribute.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void imageEditor_FileSaved( object sender, EventArgs e )
        {
            var control = (Rock.Web.UI.Controls.ImageEditor)sender;
            var personId = control.Attributes["CommandArgument"];
            if ( personId != null && control.BinaryFileId != null )
            {
                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );
                var person = personService.Get( personId.AsInteger() );
                person.PhotoId = control.BinaryFileId.Value;

                AddOrUpdatePersonInPhotoRequestGroup( person, rockContext );

                rockContext.SaveChanges();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the person and their family (if the IncludeFamilyMembers block attribute
        /// is set to true) to the repeater which holds an image editor control for each
        /// person.
        /// </summary>
        private void BindRepeater()
        {
            var people = new List<Person>();

            if ( CurrentPerson != null )
            {
                people.Add( CurrentPerson );

                if ( GetAttributeValue( AttributeKey.IncludeFamilyMembers ).AsBoolean() )
                {
                    foreach ( var member in CurrentPerson.GetFamilyMembers( includeSelf: false ).ToList() )
                    {
                        people.Add( member.Person );
                    }
                }
            }
            rptPhotos.DataSource = people;
            rptPhotos.DataBind();
        }

        /// <summary>
        /// Add the person (if not already existing) to the Photo Request group and set status to Pending.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddOrUpdatePersonInPhotoRequestGroup( Person person, RockContext rockContext )
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
    }
}