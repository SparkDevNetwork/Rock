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
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.minecartstudio.PCOSync.Model;
using com.minecartstudio.PCOSync;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_mineCartStudio.PCOSync
{
    /// <summary>
    /// Block for viewing the Rock people who have ever been synced with a particular PCO Account.
    /// </summary>
    [DisplayName( "Account Person List" )]
    [Category( "Mine Cart Studio > PCO Sync" )]
    [Description( "Block for viewing the Rock people who have ever been synced with a particular PCO Account." )]
    public partial class AccountPersonList : RockBlock, ISecondaryBlock
    {
        #region Private Variables

        private Account _account = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int accountId = PageParameter( "accountId" ).AsInteger();

            _account = new AccountService( new RockContext() ).Get( accountId );

            if ( _account != null )
            {
                gAccountPersons.DataKeyNames = new string[] { "Id" };
                gAccountPersons.Actions.ShowAdd = false;
                gAccountPersons.RowDataBound += gAccountPersons_RowDataBound;
                gAccountPersons.GridRebind += gAccountPersons_GridRebind;
                gAccountPersons.IsDeleteEnabled = false;

                modalValue.SaveClick += btnSaveValue_Click;
                modalValue.OnCancelScript = string.Format( "$('#{0}').val('');", hfAccountPersonId.ClientID );
            }
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
                if ( _account != null )
                {
                    ShowDetail();
                }
            }
        }

        #endregion

        #region Events

        void gAccountPersons_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == System.Web.UI.WebControls.DataControlRowType.DataRow )
            {
                var accountPerson = e.Row.DataItem as AccountPerson;
                if ( accountPerson != null )
                {
                    if ( !string.IsNullOrWhiteSpace(accountPerson.RockSyncState))
                    {
                        var pcoPerson = JsonConvert.DeserializeObject<PCOPerson>( accountPerson.RockSyncState );
                        var lSecurity = e.Row.FindControl( "lRockSecurity" ) as System.Web.UI.WebControls.Literal;
                        SetSecurityLabel( lSecurity, pcoPerson );
                    }

                    if ( !string.IsNullOrWhiteSpace( accountPerson.PCOSyncState ) )
                    {
                        var pcoPerson = JsonConvert.DeserializeObject<PCOPerson>( accountPerson.PCOSyncState );
                        var lSecurity = e.Row.FindControl( "lPCOSecurity" ) as System.Web.UI.WebControls.Literal;
                        SetSecurityLabel( lSecurity, pcoPerson );
                    }
                }
            }
        }

        private void SetSecurityLabel( System.Web.UI.WebControls.Literal literal, PCOPerson person )
        {
            if ( literal != null && person != null )
            {
                var permission = People.PermissionLevel( person.permissions );
                string labelType = "default";
                switch( permission )
                {
                    case Permission.Administrator: labelType = "success"; break;
                    case Permission.Editor: labelType = "info"; break;
                    case Permission.Scheduler: labelType = "info"; break;
                    case Permission.ScheduledViewer: labelType = "warning"; break;
                    case Permission.Viewer: labelType = "warning"; break;
                    default: labelType = "default"; break;
                }

                literal.Text = string.Format( "<span class='label label-{0}'>{1}</span>", labelType, permission.ConvertToString() );
            }
        }

        protected void gAccountPersons_RowSelected( object sender, RowEventArgs e )
        {
            AccountPerson accountPerson = new AccountPersonService( new RockContext() ).Get( e.RowKeyId );
            if ( accountPerson != null &&
                accountPerson.PersonAlias != null &&
                accountPerson.PersonAlias.Person != null )
            {
                modalValue.Title = ActionTitle.Edit( accountPerson.PersonAlias.Person.FullName );

                hfAccountPersonId.SetValue( accountPerson.Id );
                tbPcoId.Text = accountPerson.PCOId.HasValue ? accountPerson.PCOId.ToString() : string.Empty;
                lRockValues.Text = accountPerson.RockSyncState;
                lPCOValues.Text = accountPerson.PCOSyncState;

                modalValue.Show();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAccountPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveValue_Click( object sender, EventArgs e )
        {
            AccountPerson accountPerson;
            var rockContext = new RockContext();
            AccountPersonService accountPersonService = new AccountPersonService( rockContext );

            int accountPersonId = hfAccountPersonId.ValueAsInt();
            accountPerson = accountPersonService.Get( accountPersonId );
            if ( accountPerson != null )
            {
                accountPerson.PCOId = tbPcoId.Text.AsIntegerOrNull();

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !accountPerson.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                rockContext.SaveChanges();

                BindAccountPersonsGrid();

                hfAccountPersonId.Value = string.Empty;
                modalValue.Hide();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail()
        {
            hfAccountId.SetValue( _account.Id );
            BindAccountPersonsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAccountPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAccountPersons_GridRebind( object sender, EventArgs e )
        {
            BindAccountPersonsGrid();
        }

        /// <summary>
        /// Binds the defined values grid.
        /// </summary>
        protected void BindAccountPersonsGrid()
        {
            if ( _account != null )
            {
                var queryable = new AccountPersonService( new RockContext() )
                    .Queryable()
                    .Where( a =>
                        a.AccountId == _account.Id &&
                        a.PersonAlias != null &&
                        a.PersonAlias.Person != null )
                    .OrderBy( a => a.PersonAlias.Person.LastName )
                    .ThenBy( a => a.PersonAlias.Person.NickName );

                gAccountPersons.DataSource = queryable.ToList();
                gAccountPersons.DataBind();
            }
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}