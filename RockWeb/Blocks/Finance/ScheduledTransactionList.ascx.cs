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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Collections.Generic;
using Rock.Security;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Lists scheduled transactions either for current person (Person Detail Page) or all scheduled transactions.
    /// </summary>
    [DisplayName( "Scheduled Transaction List" )]
    [Category( "Finance" )]
    [Description( "Lists scheduled transactions either for current person (Person Detail Page) or all scheduled transactions." )]

    [LinkedPage( "View Page" )]
    [LinkedPage( "Add Page" )]
    [ContextAware]
    public partial class ScheduledTransactionList : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <value>
        /// The target person.
        /// </value>
        protected Person TargetPerson { get; private set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            gList.DataKeyNames = new string[] { "Id" };
            gList.Actions.ShowAdd = canEdit && !string.IsNullOrWhiteSpace( GetAttributeValue( "AddPage" ) );
            gList.IsDeleteEnabled = canEdit;

            gList.Actions.AddClick += gList_Add;
            gList.GridRebind += gList_GridRebind;

            TargetPerson = ContextEntity<Person>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                cbIncludeInactive.Checked = !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Include Inactive" ) );

                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Include Inactive", cbIncludeInactive.Checked ? "Yes" : string.Empty );
            BindGrid();
        }

        /// <summary>
        /// Gfs the settings_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key != "Include Inactive" )
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ScheduledTransactionId", e.RowKeyId.ToString() );
            NavigateToLinkedPage( "ViewPage", parms );
        }

        /// <summary>
        /// Handles the Add event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "Person", TargetPerson.UrlEncodedKey );
            NavigateToLinkedPage( "AddPage", parms );
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            bool includeInactive = !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Include Inactive" ) );
            int? personId = null;
            int? givingGroupId = null;

            bool validRequest = false;

            if ( TargetPerson != null )
            {
                personId = TargetPerson.Id;
                givingGroupId = TargetPerson.GivingGroupId;
                validRequest = true;
            }
            else
            {
                int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                if ( !ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) )
                {
                    validRequest = true;
                }
            }

            if ( validRequest )
            {
                gList.DataSource = new FinancialScheduledTransactionService( new RockContext() )
                    .Get( personId, givingGroupId, includeInactive ).ToList();
                gList.DataBind();
            }
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ScheduledTransactionId", id.ToString() );
            parms.Add( "Person", TargetPerson.UrlEncodedKey );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        #endregion
    }
}
