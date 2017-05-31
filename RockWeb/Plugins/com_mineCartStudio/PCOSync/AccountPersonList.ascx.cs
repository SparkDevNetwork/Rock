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

using com.minecartstudio.PCOSync.Model;
using com.minecartstudio.PCOSync;

using Newtonsoft.Json;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.com_mineCartStudio.PCOSync
{
    /// <summary>
    /// Block for viewing the Rock people who have ever been synced with a particular PCO Account.
    /// </summary>
    [DisplayName( "Account Person List" )]
    [Category( "Mine Cart Studio > PCO Sync" )]
    [Description( "Block for viewing the Rock people who have ever been synced with a particular PCO Account." )]

    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 2, "PersonProfilePage" )]
    public partial class AccountPersonList : RockBlock, ISecondaryBlock
    {
        #region Private Variables

        private PCOAccount _account = null;
        protected LinkButton _lbRefresh = new LinkButton();
        protected LinkButton _lbDeleteSelected = new LinkButton();

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

                gAccountPersons.ColumnsOfType<HyperLinkField>().First().DataNavigateUrlFormatString = LinkedPageUrl( "PersonProfilePage", new Dictionary<string, string> { { "PersonId", "###" } } ).Replace( "###", "{0}" );


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
            rFilter.SaveUserPreference( "Rock Permissions", cblRockPermission.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( "PCO Permissions", cblPCOPermission.SelectedValues.AsDelimited( ";" ) );

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
                case "Rock Permissions":
                    {
                        e.Value = ResolveValues( e.Value, cblRockPermission );
                        break;
                    }
                case "PCO Permissions":
                    {
                        e.Value = ResolveValues( e.Value, cblRockPermission );
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
                AccountPersonHelper person = e.Row.DataItem as AccountPersonHelper;
                if ( person != null && !person.Current )
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
                lRockValues.Text = PCOApi.PersonDictionary( accountPerson.RockState ).Select( d => string.Format( "{0}: {1}", d.Key, d.Value ) ).ToList().AsDelimited( "<br/>" );
                lPCOValues.Text = PCOApi.PersonDictionary( accountPerson.PCOState ).Select( d => string.Format( "{0}: {1}", d.Key, d.Value ) ).ToList().AsDelimited( "<br/>" );

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
                var api = new PCOApi( _account );
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
                var api = new PCOApi( _account );
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
            var permissionsType = DefinedTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid() );
            cblRockPermission.BindToDefinedType( permissionsType );
            cblPCOPermission.BindToDefinedType( permissionsType );

            tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
            cbCurrentOnly.Checked = rFilter.GetUserPreference( "Current People Only" ).AsBoolean();
            cbBlankPCOId.Checked = rFilter.GetUserPreference( "Blank PCO Ids" ).AsBoolean();
            int? pcoId = rFilter.GetUserPreference( "Specific PCO Id" ).AsIntegerOrNull();
            nbPCOId.Text = pcoId.HasValue ? pcoId.Value.ToString() : string.Empty;

            string rockPermissions = rFilter.GetUserPreference( "Rock Permissions" );
            if ( !string.IsNullOrWhiteSpace( rockPermissions ) )
            {
                cblRockPermission.SetValues( rockPermissions.Split( ';' ).ToList() );
            }

            string pcoPermissions = rFilter.GetUserPreference( "PCO Permissions" );
            if ( !string.IsNullOrWhiteSpace( pcoPermissions ) )
            {
                cblPCOPermission.SetValues( pcoPermissions.Split( ';' ).ToList() );
            }

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
                    // Get all the active group members that belong to any of the groups that this account is associated with
                    var accountMembers = _account.GetAccountMembers( rockContext );

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

                    if ( rFilter.GetUserPreference( "Current People Only" ).AsBoolean() )
                    {
                        qry = qry.Where( a => accountMembers.Keys.Contains( a.PersonAlias.PersonId ) );
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

                    var people = new List<AccountPersonHelper>();
                    foreach ( var accountPerson in qry
                        .OrderBy( a => a.PersonAlias.Person.LastName )
                        .ThenBy( a => a.PersonAlias.Person.NickName )
                        .ToList() )
                    {
                        people.Add( new AccountPersonHelper(
                            accountPerson,
                            accountMembers
                                .Where( a => a.Key == accountPerson.PersonAlias.PersonId )
                                .Select( a => a.Value )
                                .FirstOrDefault() ) );
                    }

                    string rockPermissions = rFilter.GetUserPreference( "Rock Permissions" );
                    if ( !string.IsNullOrWhiteSpace( rockPermissions ) )
                    {
                        var selectedPermissionIds = rockPermissions.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToList().AsIntegerList();
                        if ( selectedPermissionIds.Any() )
                        {
                            people = people
                                .Where( p => 
                                    p.RockPermission != null &&
                                    selectedPermissionIds.Contains( p.RockPermission.Id ) )
                                .ToList();
                        }
                    }

                    string pcoPermissions = rFilter.GetUserPreference( "PCO Permissions" );
                    if ( !string.IsNullOrWhiteSpace( pcoPermissions ) )
                    {
                        var selectedPermissionIds = pcoPermissions.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToList().AsIntegerList();
                        if ( selectedPermissionIds.Any() )
                        {
                            people = people
                                .Where( p =>  
                                    p.PCOPermission != null &&
                                    selectedPermissionIds.Contains( p.PCOPermission.Id ) )
                                .ToList();
                        }
                    }

                    var peopleQry = people.AsQueryable();

                    SortProperty sortProperty = gAccountPersons.SortProperty;
                    if ( sortProperty != null )
                    {
                        peopleQry = peopleQry.Sort( sortProperty );
                    }
                    else
                    {
                        peopleQry = peopleQry.OrderBy( p => p.LastName ).ThenBy( p => p.NickName );
                    }

                    gAccountPersons.DataSource = peopleQry.ToList();
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
            if ( accountPerson != null && accountPerson.PCOId.HasValue )
            {
                if ( api.DeletePerson( accountPerson.PCOId.Value ) )
                {
                    accountPersonService.Delete( accountPerson );
                    rockContext.SaveChanges();
                }
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

        #region HelperClass

        public class AccountPersonHelper
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public Person person { get; set; }
            public string LastName { get; set; }
            public string NickName { get; set; }
            public int? PCOId { get; set; }
            public bool Current { get; set; }
            public DefinedValueCache RockPermission { get; set; }
            public DefinedValueCache PCOPermission { get; set; }
            public string RockPermissionLabel { get; set; }
            public string PCOPermissionLabel { get; set; }

            public AccountPersonHelper( PCOAccountPerson accountPerson, DefinedValueCache permission )
            {
                if ( accountPerson != null )
                {
                    Id = accountPerson.Id;

                    if ( accountPerson.PersonAlias != null )
                    {
                        person = accountPerson.PersonAlias.Person;
                        if ( person != null )
                        {
                            PersonId = person.Id;
                            LastName = person.LastName;
                            NickName = person.NickName;
                        }
                    }

                    PCOId = accountPerson.PCOId;
                    Current = permission != null;

                    RockPermission = permission;
                    PCOPermission = GetPermission( accountPerson.PCOSyncState );
                    RockPermissionLabel = GetSecurityLabel( RockPermission );
                    PCOPermissionLabel = GetSecurityLabel( PCOPermission );
                }
            }

            private DefinedValueCache GetPermission( string state )
            {
                var permissionsType = DefinedTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid() );
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

            private string GetSecurityLabel( DefinedValueCache permission )
            {
                if ( permission != null )
                {
                    string labelType = permission.GetAttributeValue( "LabelType" );
                    return string.Format( "<span class='label label-{0}'>{1}</span>",
                        !string.IsNullOrWhiteSpace( labelType ) ? labelType : "default", permission.Value );
                }
                return string.Empty;
            }
        }

        #endregion

    }
}