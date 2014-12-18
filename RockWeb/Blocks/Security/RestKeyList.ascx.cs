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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    [DisplayName( "Rest Key List" )]
    [Category( "Security" )]
    [Description( "Lists all the REST API Keys" )]
    [LinkedPage( "Detail Page" )]
    public partial class RestKeyList : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            gRestKeyList.DataKeyNames = new string[] { "Id" };
            gRestKeyList.Actions.ShowAdd = canEdit;
            gRestKeyList.Actions.AddClick += gRestKeyList_AddClick;
            gRestKeyList.GridRebind += gRestKeyList_GridRebind;
            gRestKeyList.IsDeleteEnabled = canEdit;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the AddClick event of the gRestKeyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gRestKeyList_AddClick( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "restUserId", "0" );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the GridRebind event of the gRestKeyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gRestKeyList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRestKeyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRestKeyList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var rockContext = new RockContext();
                var restUser = e.Row.DataItem as Person;
                var noteService = new NoteService( rockContext );
                var description = noteService.Queryable().Where( a => a.EntityId == restUser.Id ).FirstOrDefault();
                Label lblDescription = e.Row.FindControl( "lblDescription" ) as Label;
                if ( description != null )
                {
                    lblDescription.Text = description.Text;
                }

                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.Queryable().Where( a => a.PersonId == restUser.Id ).FirstOrDefault();
                Label lblKey = e.Row.FindControl( "lblKey" ) as Label;
                if ( userLogin != null )
                {
                    lblKey.Text = userLogin.ApiKey;
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gRestKeyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRestKeyList_RowSelected( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            var restUserId = e.RowKeyId;
            parms.Add( "restUserId", restUserId.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Delete event of the gRestKeyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRestKeyList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var userLoginService = new UserLoginService( rockContext );
            var restUser = personService.Get( e.RowKeyId );
            if ( restUser != null )
            {
                restUser.RecordStatusValueId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;

                // remove all user logins for key
                foreach ( var login in restUser.Users.ToList() )
                {
                    userLoginService.Delete( login );
                }
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var restUserRecordTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            var activeRecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            var queryable = new PersonService( rockContext ).Queryable()
                .Where( q => q.RecordTypeValueId == restUserRecordTypeId && q.RecordStatusValueId == activeRecordStatusValueId );

            SortProperty sortProperty = gRestKeyList.SortProperty;
            if ( sortProperty != null )
            {
                gRestKeyList.DataSource = queryable.Sort( sortProperty ).ToList();
            }
            else
            {
                gRestKeyList.DataSource = queryable.OrderBy( q => q.LastName ).ToList();
            }

            gRestKeyList.DataBind();
        }

        #endregion Internal Methods
    }
}