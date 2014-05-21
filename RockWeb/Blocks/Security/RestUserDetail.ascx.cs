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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    [DisplayName( "Rest User Detail" )]
    [Category( "Security" )]
    [Description( "Displays the details of the given REST API User." )]
    public partial class RestUserDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                var rockContext = new RockContext();
                string itemId = PageParameter( "restUserId" );

                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "restUserId", int.Parse( itemId ) );
                }
                else
                {
                    //pnlDetails.Visible = false;
                }
            }
        }

        #endregion Control Methods

        #region Events

        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            Rock.Data.RockTransactionScope.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );
                var changes = new List<string>();
                var restUser = new Person();
                if ( int.Parse( hfRestUserId.Value ) != 0 )
                {
                    restUser = personService.Get( int.Parse( hfRestUserId.Value ) );
                }
                else
                {
                    personService.Add( restUser );
                }

                // the rest user name gets saved as the last name on a person
                History.EvaluateChange( changes, "Last Name", restUser.LastName, tbName.Text );
                restUser.LastName = tbName.Text;
                restUser.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
                if ( cbActive.Checked )
                {
                    restUser.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                }
                else
                {
                    restUser.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
                }

                if ( restUser.IsValid )
                {
                    if ( rockContext.SaveChanges() > 0 )
                    {
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                restUser.Id,
                                changes );
                        }
                    }
                }

                // the description gets saved as a system note for the person
                var entityTypeService = new EntityTypeService( rockContext );
                var entityType = entityTypeService.Get("Rock.Model.Person");
                var noteTypeService = new NoteTypeService( rockContext );
                var noteType = noteTypeService.Get(entityType.Id, "Timeline");
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
                rockContext.SaveChanges();

                // the key gets saved in the api key field of a user login (which you have to create if needed)
                entityType = entityTypeService.Get( "Rock.Security.Authentication.Database" );
                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.GetByPersonId( restUser.Id ).FirstOrDefault();
                if ( userLogin == null )
                {
                    userLogin = new UserLogin();
                    userLoginService.Add( userLogin );
                }

                if ( string.IsNullOrWhiteSpace( userLogin.UserName ) )
                {
                    userLogin.UserName = Guid.NewGuid().ToString();
                }

                userLogin.ApiKey = tbKey.Text;
                userLogin.PersonId = restUser.Id;
                userLogin.EntityTypeId = entityType.Id;
                rockContext.SaveChanges();
            } );
            NavigateToParentPage();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        protected void lbGenerate_Click( object sender, EventArgs e )
        {
            // Generate a unique random 12 digit api key
            var randomNumber = new Random();
        }

        #endregion Events

        #region Internal Methods

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            var rockContext = new RockContext();

            if ( !itemKey.Equals( "restUserId" ) )
            {
                return;
            }

            bool editAllowed = true;

            Person restUser = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                restUser = new PersonService( rockContext ).Get( itemKeyValue );
                editAllowed = restUser.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
            else
            {
                restUser = new Person { Id = 0 };
            }

            if ( restUser == null )
            {
                return;
            }

            hfRestUserId.Value = restUser.Id.ToString();
            lTitle.Text = ActionTitle.Edit( "Rest User" ).FormatAsHtmlTitle();
            if ( restUser.Id > 0 )
            {
                tbName.Text = restUser.LastName;
                cbActive.Checked = false;
                var activeStatusId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
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

        #endregion Internal Methods
    }
}