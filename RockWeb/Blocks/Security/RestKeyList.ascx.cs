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

            gRestKeyList.DataKeyNames = new string[] { "id" };
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

        void gRestKeyList_AddClick( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "restUserId", "0" );
            NavigateToLinkedPage( "DetailPage", parms );            
        }

        void gRestKeyList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void gRestKeyList_RowDataBound( object sender, GridViewRowEventArgs e )
        {

        }

        protected void gRestKeyList_RowSelected( object sender, RowEventArgs e )
        {

        }

        protected void gRestKeyList_Delete( object sender, RowEventArgs e )
        {

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