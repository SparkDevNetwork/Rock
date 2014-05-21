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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    [DisplayName( "Rest User List" )]
    [Category( "Security" )]
    [Description( "Lists all the REST API Users" )]
    [LinkedPage( "Detail Page" )]
    public partial class RestUserList : Rock.Web.UI.RockBlock
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

            gRestUserList.DataKeyNames = new string[] { "id" };
            gRestUserList.Actions.ShowAdd = canEdit;
            gRestUserList.Actions.AddClick += gRestUserList_AddClick;
            gRestUserList.GridRebind += gRestUserList_GridRebind;
            gRestUserList.IsDeleteEnabled = canEdit;
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

        void gRestUserList_AddClick( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "restUserId", "0" );
            NavigateToLinkedPage( "DetailPage", parms );            
        }

        void gRestUserList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void gRestUserList_RowDataBound( object sender, GridViewRowEventArgs e )
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

        protected void gRestUserList_RowSelected( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            var restUserId = (int)e.RowKeyValue;
            parms.Add( "restUserId", restUserId.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        protected void gRestUserList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var restUser = personService.Get( e.RowKeyId );
            if ( restUser != null )
            {
                restUser.RecordStatusValueId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
                rockContext.SaveChanges();
            }

            BindGrid();            
        }

        #endregion Events

        #region Internal Methods

        private void BindGrid()
        {
            // The "Name" is the Person LastName
            // The "Description" is a SystemNote
            // The "Key" is the UserLogin ApiKey
            // var attributes = Person.Attributes.Where( p => p.Value.Categories.Select( c => c.Guid ).Contains( socialCategoryGuid ) );
            // var result = attributes.Join( Person.AttributeValues, a => a.Key, v => v.Key, ( a, v ) => new { Attribute = a.Value, Values = v.Value } );

            // get people that have a record type of "rest user"
            var rockContext = new RockContext();
            var restUserRecordTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            var activeRecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            var queryable = new PersonService( rockContext ).Queryable()
                .Where( q => q.RecordTypeValueId == restUserRecordTypeId && q.RecordStatusValueId == activeRecordStatusValueId );

            SortProperty sortProperty = gRestUserList.SortProperty;
            if ( sortProperty != null )
            {
                gRestUserList.DataSource = queryable.Sort( sortProperty ).ToList();
            }
            else
            {
                gRestUserList.DataSource = queryable.OrderBy( q => q.LastName ).ToList();
            }

            gRestUserList.DataBind();
        }

        #endregion Internal Methods
    }
}