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
using System.Text;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    [DisplayName( "REST Key Detail" )]
    [Category( "Security" )]
    [Description( "Displays the details of the given REST API Key." )]
    [Rock.SystemGuid.BlockTypeGuid( "A2C41730-BF79-4F8C-8368-2C4D5F76129D" )]
    public partial class RestKeyDetail : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "RestUserId" ).AsInteger() );
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var userLoginService = new UserLoginService( rockContext );

            var userLogin = userLoginService.Queryable().Where( a => a.ApiKey == tbKey.Text ).FirstOrDefault();
            if ( userLogin != null && userLogin.PersonId != int.Parse( hfRestUserId.Value ) )
            {
                // this key already exists in the database. Show the error and get out of here.
                nbWarningMessage.Text = "This API Key already exists. Please enter a different one, or generate one by clicking the 'Generate Key' button below. ";
                nbWarningMessage.Visible = true;
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );
                var restUser = new Person();
                if ( int.Parse( hfRestUserId.Value ) != 0 )
                {
                    restUser = personService.Get( int.Parse( hfRestUserId.Value ) );
                }
                else
                {
                    personService.Add( restUser );
                    rockContext.SaveChanges();
                }

                // the rest user name gets saved as the last name on a person
                restUser.LastName = tbName.Text;
                restUser.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
                if ( cbActive.Checked )
                {
                    restUser.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                }
                else
                {
                    restUser.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
                }

                if ( restUser.IsValid )
                {
                    rockContext.SaveChanges();
                }

                // the description gets saved as a system note for the person
                var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.PERSON_TIMELINE_NOTE.AsGuid() );
                if ( noteType != null )
                {
                    var noteService = new NoteService( rockContext );
                    var note = noteService.Get( noteType.Id, restUser.Id ).FirstOrDefault();
                    if ( note == null )
                    {
                        note = new Note();
                        noteService.Add( note );
                    }
                    note.NoteTypeId = noteType.Id;
                    note.EntityId = restUser.Id;
                    note.Text = tbDescription.Text;
                }
                rockContext.SaveChanges();

                // the key gets saved in the api key field of a user login (which you have to create if needed)
                var entityType = new EntityTypeService( rockContext )
                    .Get( "Rock.Security.Authentication.Database" );
                userLogin = userLoginService.GetByPersonId( restUser.Id ).FirstOrDefault();
                if ( userLogin == null )
                {
                    userLogin = new UserLogin();
                    userLoginService.Add( userLogin );
                }

                if ( string.IsNullOrWhiteSpace( userLogin.UserName ) )
                {
                    userLogin.UserName = Guid.NewGuid().ToString();
                }

                userLogin.IsConfirmed = true;
                userLogin.ApiKey = tbKey.Text;
                userLogin.PersonId = restUser.Id;
                userLogin.EntityTypeId = entityType.Id;
                rockContext.SaveChanges();
            } );
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbGenerate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbGenerate_Click( object sender, EventArgs e )
        {
            tbKey.Text = Rock.Utility.KeyHelper.GenerateKey( (RockContext rockContext, string key) => new UserLoginService( rockContext ).Queryable().Any( a => a.ApiKey == key ) );
        }

        #endregion

        #region Internal Methods
                
        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="restUserId">The rest user identifier.</param>
        public void ShowDetail( int restUserId )
        {
            var rockContext = new RockContext();

            Person restUser = null;

            if ( !restUserId.Equals( 0 ) )
            {
                restUser = new PersonService( rockContext ).Get( restUserId );
            }

            if ( restUser == null )
            {
                restUser = new Person { Id = 0 };
            }

            bool editAllowed = restUser.IsAuthorized( Authorization.EDIT, CurrentPerson );

            hfRestUserId.Value = restUser.Id.ToString();
            lTitle.Text = ActionTitle.Edit( "REST Key" ).FormatAsHtmlTitle();
            if ( restUser.Id > 0 )
            {
                tbName.Text = restUser.LastName;
                cbActive.Checked = false;
                var activeStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                if ( restUser.RecordStatusValueId == activeStatusId )
                {
                    cbActive.Checked = true;
                }

                var noteService = new NoteService( rockContext );
                var description = noteService.Queryable().Where( a => a.EntityId == restUser.Id ).FirstOrDefault();
                if ( description != null )
                {
                    tbDescription.Text = description.Text;
                }

                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.Queryable().Where( a => a.PersonId == restUser.Id ).FirstOrDefault();
                if ( userLogin != null )
                {
                    tbKey.Text = userLogin.ApiKey;
                }
            }
        }

        #endregion
    }
}