// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace RockWeb.Blocks.Crm.PhotoRequest
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Upload" )]
    [Category( "CRM > PhotoRequest" )]
    [Description( "Allows a photo to be uploaded for the given person (logged in person) and optionally their family members." )]

    [BooleanField( "Include Family Members", "If checked, other family members will also be displayed allowing their photos to be uploaded.", true )]
    [BooleanField( "Allow Staff", "If checked, staff members will also be allowed to upload new photos for themselves.", false )]
    public partial class Upload : Rock.Web.UI.RockBlock
    {
        #region Fields

        /// <summary>
        /// This is special "staff" group but it's only loaded when the AllowStaff block attribute is set to false.
        /// </summary>
        private Group _staffGroup = null;

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
            base.OnLoad( e );

            if ( ! Page.IsPostBack )
            {
                if ( ! GetAttributeValue( "AllowStaff" ).AsBoolean() )
                {
                    GroupService service = new GroupService( new RockContext() );
                    _staffGroup = service.GetByGuid( new Guid( Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS ) );
                }
                BindRepeater();
            }
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
            imageEditor.NoPictureUrl = Person.GetPhotoUrl( null, person.Age, person.Gender );
            if ( _staffGroup != null && _staffGroup.Members.Where( m => m.PersonId == person.Id ).Count() > 0 )
            {
                imageEditor.Label = string.Format( "{0} (staff member)", person.FullName );
                imageEditor.ButtonCssClass = "invisible";
            }
            else
            {
                imageEditor.Label = string.Format( "{0}", person.FullName );
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
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupContentFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fileUploader_FileUploaded( object sender, EventArgs e )
        {
            //var imageUploader = e.Item.FindControl( "imgupPhoto" ) as Rock.Web.UI.Controls.ImageUploader;
            var control = (Rock.Web.UI.Controls.ImageUploader)sender;
            var personId = control.Attributes["CommandArgument"];
            if ( personId != null && control.BinaryFileId != null )
            {
                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );
                var person = personService.Get( personId.AsInteger() );
                person.PhotoId = control.BinaryFileId.Value;
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
            var currentUser = CurrentUser;

            if ( currentUser != null )
            {
                people.Add( currentUser.Person );

                if ( GetAttributeValue( "IncludeFamilyMembers" ).AsBoolean() )
                {
                    foreach ( var member in currentUser.Person.GetFamilyMembers( includeSelf: false ).ToList() )
                    {
                        people.Add( member.Person );
                    }
                }
            }
            rptPhotos.DataSource = people;
            rptPhotos.DataBind();
        }

        #endregion
    }
}