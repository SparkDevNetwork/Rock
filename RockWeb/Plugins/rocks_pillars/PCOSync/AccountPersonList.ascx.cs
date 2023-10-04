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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using rocks.pillars.PCOSync.Model;
using rocks.pillars.PCOSync;

using Newtonsoft.Json;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.rocks_pillars.PCOSync
{
    /// <summary>
    /// Block for viewing the Rock people who have ever been synced with a particular PCO Account.
    /// </summary>
    [DisplayName( "Account Person List" )]
    [Category( "Pillars > PCO Sync" )]
    [Description( "Block for viewing the Rock people who have ever been synced with a particular PCO Account." )]

    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 2, "PersonProfilePage" )]
    public partial class AccountPersonList : RockBlock, ISecondaryBlock
    {
        #region Private Variables

        private PCOAccount _account = null;
        protected LinkButton _lbRefresh = new LinkButton();
        protected LinkButton _lbDeleteSelected = new LinkButton();
        protected Dictionary<int, DefinedValueCache> _personPermissions = new Dictionary<int, DefinedValueCache>();

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

            _account = new PCOAccountService( new RockContext() )
                .Queryable().AsNoTracking()
                .FirstOrDefault( a => a.Id == accountId );

            if ( _account != null )
            {
                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

                gAccountPersons.DataKeyNames = new string[] { "Id" };
                gAccountPersons.Actions.ShowAdd = false;
                gAccountPersons.GridRebind += gAccountPersons_GridRebind;
                gAccountPersons.RowDataBound += GAccountPersons_RowDataBound;
                gAccountPersons.IsDeleteEnabled = UserCanAdministrate;

                _lbRefresh.ID = "lbRefresh";
                _lbRefresh.CssClass = "btn btn-default btn-sm pull-left";
                _lbRefresh.Click += lbRefresh_Click;
                _lbRefresh.Text = "Refresh List From Groups";
                _lbRefresh.CausesValidation = false;
                gAccountPersons.Actions.AddCustomActionControl( _lbRefresh );

                if ( UserCanAdministrate )
                {
                    gAccountPersons.ColumnsOfType<SelectField>().First().Visible = true;

                    _lbDeleteSelected.ID = "lbDeleteSelected";
                    _lbDeleteSelected.CssClass = "btn btn-default btn-sm pull-left js-delete-people";
                    _lbDeleteSelected.Click += _lbDeleteSelected_Click;
                    _lbDeleteSelected.Text = "Delete Selected";
                    _lbDeleteSelected.CausesValidation = false;
                    gAccountPersons.Actions.AddCustomActionControl( _lbDeleteSelected );
                }

                modalValue.SaveClick += btnSaveValue_Click;
                modalValue.OnCancelScript = string.Format( "$('#{0}').val('');", hfAccountPersonId.ClientID );
            }

            string deleteScript = @"
    $('table.js-account-people a.grid-delete-button').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this sync record? This will delete the person from Planning Center Online also! Note: If this person is still active in a synced group, they will get resynced the next time the PCO sync job runs.', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });

    $('.js-delete-people').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete the selected records? This will delete these people from Planning Center Online also! Note: If any of these people are still active in a synced group, they will get resynced the next time the PCO sync job runs.', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gAccountPersons, gAccountPersons.GetType(), "deletePCOPersonScript", deleteScript, true );
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
                    SetFilter();
                    ShowDetail();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "First Name" , tbFirstName.Text );
            rFilter.SaveUserPreference( "Last Name" , tbLastName.Text );
            rFilter.SaveUserPreference( "Current People Only", cbCurrentOnly.Checked.ToString() );
            rFilter.SaveUserPreference( "Blank PCO Ids", cbBlankPCOId.Checked.ToString() );
            rFilter.SaveUserPreference( "Specific PCO Id", nbPCOId.Text );

            BindAccountPersonsGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "First Name":
                case "Last Name":
                case "Specific PCO Id":
                    {
                        break;
                    }
                case "Current People Only":
                case "Blank PCO Ids":
                    {
                        e.Value = e.Value.AsBoolean() ? "Yes" : string.Empty;
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the GAccountPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void GAccountPersons_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var accountPerson = e.Row.DataItem as PCOAccountPerson;
                if ( accountPerson != null && !accountPerson.PermissionLevelValueId.HasValue )
                {
                    e.Row.AddCssClass( "is-inactive" );
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gAccountPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountPersons_RowSelected( object sender, RowEventArgs e )
        {
            PCOAccountPerson accountPerson = new PCOAccountPersonService( new RockContext() ).Get( e.RowKeyId );
            if ( accountPerson != null &&
                accountPerson.PersonAlias != null &&
                accountPerson.PersonAlias.Person != null )
            {
                modalValue.Title = ActionTitle.Edit( accountPerson.PersonAlias.Person.FullName );

                hfAccountPersonId.SetValue( accountPerson.Id );
                tbPcoId.Text = accountPerson.PCOId.HasValue ? accountPerson.PCOId.ToString() : string.Empty;
                lLegacyId.Text = accountPerson.PCOLegacyId.HasValue ? accountPerson.PCOLegacyId.ToString() : string.Empty;
                lLastUpdate.Text = accountPerson.LastPCOUpdate.HasValue ? accountPerson.LastPCOUpdate.Value.ToShortDateTimeString() : string.Empty;
                lRockValues.Text = PCOApi.PersonDictionary( accountPerson.RockState, true ).Select( d => string.Format( "{0}: {1}", d.Key, d.Value ) ).ToList().AsDelimited( "<br/>" );
                lPCOValues.Text = PCOApi.PersonDictionary( accountPerson.PCOState, _account.MapFirstAndNickName ).Select( d => string.Format( "{0}: {1}", d.Key, d.Value ) ).ToList().AsDelimited( "<br/>" );

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
            PCOAccountPerson accountPerson;
            var rockContext = new RockContext();
            var accountPersonService = new PCOAccountPersonService( rockContext );

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

        protected void lbRefresh_Click( object sender, EventArgs arg )
        {
            if ( _account != null )
            {
                _account.RefreshAccountPeople();
            }

            BindAccountPersonsGrid();
        }


        protected void gAccountPerson_Delete( object sender, RowEventArgs e )
        {
            if ( _account != null )
            {
                var api = new PCOApi( _account, false );
                var rockContext = new RockContext();
                var accountPersonService = new PCOAccountPersonService( rockContext );
                DeleteRecord( e.RowKeyId, rockContext, accountPersonService, api );
            }

            BindAccountPersonsGrid();
        }

        private void _lbDeleteSelected_Click( object sender, EventArgs e )
        {
            var idsSelected = new List<int>();
            gAccountPersons.SelectedKeys.ToList().ForEach( b => idsSelected.Add( b.ToString().AsInteger() ) );

            if ( idsSelected.Any() )
            {
                var api = new PCOApi( _account, false );
                var rockContext = new RockContext();
                var accountPersonService = new PCOAccountPersonService( rockContext );

                foreach( int id in idsSelected )
                {
                    DeleteRecord( id, rockContext, accountPersonService, api );
                }
            }

            BindAccountPersonsGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
            cbCurrentOnly.Checked = rFilter.GetUserPreference( "Current People Only" ).AsBoolean();
            cbBlankPCOId.Checked = rFilter.GetUserPreference( "Blank PCO Ids" ).AsBoolean();
            int? pcoId = rFilter.GetUserPreference( "Specific PCO Id" ).AsIntegerOrNull();
            nbPCOId.Text = pcoId.HasValue ? pcoId.Value.ToString() : string.Empty;
        }

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
                using ( var rockContext = new RockContext() )
                {
                    var qry = new PCOAccountPersonService( rockContext )
                        .Queryable( "PersonAlias.Person" )
                        .Where( a =>
                            a.AccountId == _account.Id &&
                            a.PersonAlias != null &&
                            a.PersonAlias.Person != null );

                    string firstName = rFilter.GetUserPreference( "First Name" );
                    if ( !string.IsNullOrWhiteSpace( firstName ) )
                    {
                        qry = qry.Where( a => a.PersonAlias.Person.FirstName.StartsWith( firstName ) );
                    }

                    string lastName = rFilter.GetUserPreference( "Last Name" );
                    if ( !string.IsNullOrWhiteSpace( lastName ) )
                    {
                        qry = qry.Where( a => a.PersonAlias.Person.LastName.StartsWith( lastName ) );
                    }

                    if ( rFilter.GetUserPreference( "Blank PCO Ids" ).AsBoolean() )
                    {
                        qry = qry.Where( a => !a.PCOId.HasValue || a.PCOId.Value == 0 );
                    }

                    int? pcoId = rFilter.GetUserPreference( "Specific PCO Id" ).AsIntegerOrNull();
                    if ( pcoId.HasValue )
                    {
                        qry = qry.Where( a => a.PCOId.HasValue && a.PCOId.Value == pcoId.Value );
                    }

                    if ( rFilter.GetUserPreference( "Current People Only" ).AsBoolean() )
                    {
                        qry = qry.Where( a => a.PermissionLevelValueId.HasValue );
                    }

                    SortProperty sortProperty = gAccountPersons.SortProperty;
                    if ( sortProperty != null )
                    {
                        gAccountPersons.SetLinqDataSource( qry.Sort( sortProperty ) );
                    }
                    else
                    {
                        gAccountPersons.SetLinqDataSource( qry.OrderBy( p => p.PersonAlias.Person.LastName ).ThenBy( p => p.PersonAlias.Person.NickName ) );
                    }

                    gAccountPersons.DataBind();
                }
            }
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        private void DeleteRecord( int id, RockContext rockContext, PCOAccountPersonService accountPersonService, PCOApi api )
        {
            var accountPerson = accountPersonService.Get( id );
            if ( accountPerson != null )
            {
                if ( accountPerson.PCOId.HasValue && accountPerson.PCOId.Value > 0 )
                {
                    api.DeletePerson( accountPerson.PCOId.Value );
                }

                accountPersonService.Delete( accountPerson );
                rockContext.SaveChanges();
            }
        }

        protected string GetSecurityLabel( object obj )
        {
            if ( obj != null )
            {
                DefinedValueCache permission = null;

                var id = obj as int?;
                if ( id.HasValue )
                {
                    permission = DefinedValueCache.Get( id.Value );
                }
                else
                {
                    permission = GetPermission( obj.ToString() );
                }

                if ( permission != null )
                {
                    string labelType = permission.GetAttributeValue( "LabelType" );
                    return string.Format( "<span class='label label-{0}'>{1}</span>",
                        !string.IsNullOrWhiteSpace( labelType ) ? labelType : "default", permission.Value );
                }
            }
            return string.Empty;
        }

        private DefinedValueCache GetPermission( string state )
        {
            var permissionsType = DefinedTypeCache.Get( rocks.pillars.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid() );
            if ( permissionsType != null && !string.IsNullOrWhiteSpace( state ) )
            {
                var pcoPerson = JsonConvert.DeserializeObject<PCOPerson>( state );
                if ( pcoPerson != null )
                {
                    return permissionsType.DefinedValues.FirstOrDefault( v => v.Value == pcoPerson.permissions );
                }
            }

            return null;
        }

        protected string GetPersonLink( object obj )
        {
            int? personId = obj as int?;
            if ( personId.HasValue )
            {
                var qryParms = new Dictionary<string, string> { { "PersonId", personId.Value.ToString() } };
                var url = LinkedPageUrl( "PersonProfilePage", qryParms );
                return string.Format( "<a href='{0}' class='btn btn-default btn-sm' ><i class='fa fa-user'></i></a>", url );
            }

            return string.Empty;
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