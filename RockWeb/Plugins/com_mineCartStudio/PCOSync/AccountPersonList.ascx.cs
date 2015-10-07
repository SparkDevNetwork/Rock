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

            _account = new AccountService( new RockContext() )
                .Queryable( "AdministratorGroup.Members,EditorGroup.Members,SchedulerGroup.Members,ViewerGroup.Members,ScheduledViewerGroup.Members" )
                .AsNoTracking()
                .FirstOrDefault( a => a.Id == accountId );

            if ( _account != null )
            {
                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

                gAccountPersons.DataKeyNames = new string[] { "Id" };
                gAccountPersons.Actions.ShowAdd = false;
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

        protected void btnRefresh_Click( object sender, EventArgs arg )
        {
            if ( _account != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Get the group ids that this account is associated with
                    var groupIdList = new List<int>();
                    if ( _account.AdministratorGroupId.HasValue )
                    {
                        groupIdList.Add( _account.AdministratorGroupId.Value );
                    }
                    if ( _account.EditorGroupId.HasValue )
                    {
                        groupIdList.Add( _account.EditorGroupId.Value );
                    }
                    if ( _account.SchedulerGroupId.HasValue )
                    {
                        groupIdList.Add( _account.SchedulerGroupId.Value );
                    }
                    if ( _account.ViewerGroupId.HasValue )
                    {
                        groupIdList.Add( _account.ViewerGroupId.Value );
                    }
                    if ( _account.ScheduledViewerGroupId.HasValue )
                    {
                        groupIdList.Add( _account.ScheduledViewerGroupId.Value );
                    }

                    // Get all the existing entries for this account
                    var accountPersonService = new AccountPersonService( rockContext );
                    var existingEntries = accountPersonService
                        .Queryable()
                        .Where( a =>
                            a.AccountId == _account.Id &&
                            a.PersonAlias != null )
                        .ToList();

                    // Group all the entries by person id & PCO id
                    var lastEntries = existingEntries
                        .GroupBy( g => new { g.PersonAlias.PersonId, g.PCOId } )
                        .Select( t => new
                        {
                            Id = t.Max( g => g.Id ),
                            PersonId = t.Key.PersonId,
                            PCOId = t.Key.PCOId
                        } )
                        .ToList();

                    // get the list of unique person ids
                    var existingPersonIds = lastEntries
                        .Select( e => e.PersonId )
                        .Distinct()
                        .ToList();

                    // If there were any duplicate entries, remove those with the older ids
                    var primaryIds = lastEntries.Select( e => e.Id ).ToList();
                    var dupEntries = existingEntries
                        .Where( e => !primaryIds.Contains( e.Id ) )
                        .ToList();
                    if ( dupEntries.Any() )
                    {
                        foreach ( var dupEntry in dupEntries )
                        {
                            accountPersonService.Delete( dupEntry );
                        }
                        rockContext.SaveChanges();
                    }

                    // Get all the active group members that belong to any of the groups that this account is associated with
                    var groupMemberPersonIds = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            groupIdList.Contains( m.GroupId ) &&
                            m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( m => m.PersonId );

                    // Find any people that are in the group that have not been added to this account
                    var newPersonIds = groupMemberPersonIds
                        .Where( i => !existingPersonIds.Contains( i ) )
                        .ToList();

                    // If there are any new people, add them to the account
                    if ( newPersonIds.Any() )
                    {
                        foreach ( var person in new PersonService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( p => newPersonIds.Contains( p.Id ) ) )
                        {
                            int? personAliasId = person.PrimaryAliasId;
                            if ( personAliasId.HasValue )
                            {
                                var accountPerson = new AccountPerson();
                                accountPerson.AccountId = _account.Id;
                                accountPerson.PersonAliasId = personAliasId.Value;
                                accountPersonService.Add( accountPerson );
                            }
                        }

                        rockContext.SaveChanges();
                    }
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
            cblRockPermission.BindToEnum<Permission>();
            cblPCOPermission.BindToEnum<Permission>();

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
                    // Get the group ids that this account is associated with
                    var groupIdList = new List<int>();
                    if ( _account.AdministratorGroupId.HasValue )
                    {
                        groupIdList.Add( _account.AdministratorGroupId.Value );
                    }
                    if ( _account.EditorGroupId.HasValue )
                    {
                        groupIdList.Add( _account.EditorGroupId.Value );
                    }
                    if ( _account.SchedulerGroupId.HasValue )
                    {
                        groupIdList.Add( _account.SchedulerGroupId.Value );
                    }
                    if ( _account.ViewerGroupId.HasValue )
                    {
                        groupIdList.Add( _account.ViewerGroupId.Value );
                    }
                    if ( _account.ScheduledViewerGroupId.HasValue )
                    {
                        groupIdList.Add( _account.ScheduledViewerGroupId.Value );
                    }
                    var groupMemberPersonIds = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            groupIdList.Contains( m.GroupId ) &&
                            m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( m => m.PersonId );
                    
                    var qry = new AccountPersonService( rockContext )
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
                        qry = qry.Where( a => groupMemberPersonIds.Contains( a.PersonAlias.PersonId ) );
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
                            groupMemberPersonIds.Contains( accountPerson.PersonAlias.PersonId ) ) );
                    }

                    string rockPermissions = rFilter.GetUserPreference( "Rock Permissions" );
                    if ( !string.IsNullOrWhiteSpace( rockPermissions ) )
                    {
                        var permissions = new List<Permission>();
                        foreach( var strValue in rockPermissions.Split( new char[] {';'}, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            permissions.Add( strValue.ConvertToEnum<Permission>() );
                        }
                        if ( permissions.Any() )
                        {
                            people = people.Where( p => permissions.Contains( p.RockPermissionEnum ) ).ToList();
                        }
                    }

                    string pcoPermissions = rFilter.GetUserPreference( "Rock Permissions" );
                    if ( !string.IsNullOrWhiteSpace( pcoPermissions ) )
                    {
                        var permissions = new List<Permission>();
                        foreach ( var strValue in pcoPermissions.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            permissions.Add( strValue.ConvertToEnum<Permission>() );
                        }
                        if ( permissions.Any() )
                        {
                            people = people.Where( p => permissions.Contains( p.PCOPermissionEnum ) ).ToList();
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
            public Person person { get; set; }
            public string LastName { get; set; }
            public string NickName { get; set; }
            public int? PCOId { get; set; }
            public bool Current { get; set; }
            public Permission RockPermissionEnum { get; set; }
            public Permission PCOPermissionEnum { get; set; }
            public string RockPermission { get; set; }
            public string PCOPermission { get; set; }
            public string RockPermissionLabel { get; set; }
            public string PCOPermissionLabel { get; set; }

            public AccountPersonHelper( AccountPerson accountPerson, bool current )
            {
                if ( accountPerson != null )
                {
                    Id = accountPerson.Id;

                    if ( accountPerson.PersonAlias != null )
                    {
                        person = accountPerson.PersonAlias.Person;
                        if ( person != null )
                        {
                            LastName = person.LastName;
                            NickName = person.NickName;
                        }
                    }

                    PCOId = accountPerson.PCOId;
                    Current = current;

                    RockPermissionEnum = GetPermission( accountPerson.RockSyncState );
                    PCOPermissionEnum = GetPermission( accountPerson.PCOSyncState );
                    RockPermission = RockPermissionEnum.ConvertToString();
                    PCOPermission = PCOPermissionEnum.ConvertToString();
                    RockPermissionLabel = GetSecurityLabel( RockPermissionEnum );
                    PCOPermissionLabel = GetSecurityLabel( PCOPermissionEnum );
                }
            }

            private Permission GetPermission( string state )
            {
                if ( !string.IsNullOrWhiteSpace( state ) )
                {
                    var pcoPerson = JsonConvert.DeserializeObject<PCOPerson>( state );
                    return People.PermissionLevel( pcoPerson.permissions );
                }

                return Permission.None;
            }

            private string GetSecurityLabel( Permission permission )
            {
                string labelType = "default";
                switch ( permission )
                {
                    case Permission.Administrator: labelType = "success"; break;
                    case Permission.Editor: labelType = "info"; break;
                    case Permission.Scheduler: labelType = "info"; break;
                    case Permission.ScheduledViewer: labelType = "warning"; break;
                    case Permission.Viewer: labelType = "warning"; break;
                    default: labelType = "default"; break;
                }

                return string.Format( "<span class='label label-{0}'>{1}</span>", labelType, permission.ConvertToString() );
            }
        }

        #endregion
    }
}