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
using Rock.Data;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.minecartstudio.PCOSync.Model;
using System.Collections.Generic;
using Rock.Model;
using System.Data.Entity;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_mineCartStudio.PCOSync
{
    /// <summary>
    /// Lists all the PCO accounts and allows for managing them.
    /// </summary>
    [DisplayName( "Account List" )]
    [Category( "Mine Cart Studio > PCO Sync" )]
    [Description( "Lists all the PCO accounts and allows for managing them." )]

    [LinkedPage( "Detail Page" )]
    public partial class AccountList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAccount.DataKeyNames = new string[] { "Id" };
            gAccount.Actions.ShowAdd = true;
            gAccount.Actions.AddClick += gAccount_Add;
            gAccount.GridRebind += gAccount_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gAccount.Actions.ShowAdd = canAddEditDelete;
            gAccount.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                gAccount_Bind();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAccount_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "accountId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAccount_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "accountId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAccount_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var accountPersonService = new PCOAccountPersonService( rockContext );
            var accountService = new PCOAccountService( rockContext );

            PCOAccount type = accountService.Get( e.RowKeyId );

            if ( type != null )
            {
                string errorMessage;
                if ( !accountService.CanDelete( type, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                accountService.Delete( type );

                rockContext.SaveChanges();
            }

            gAccount_Bind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAccount_GridRebind( object sender, EventArgs e )
        {
            gAccount_Bind();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid for defined types.
        /// </summary>
        private void gAccount_Bind()
        {
            using ( var rockContext = new RockContext() )
            {
                var queryable = new PCOAccountService( rockContext ).Queryable();

                SortProperty sortProperty = gAccount.SortProperty;
                if ( sortProperty != null )
                {
                    queryable = queryable.Sort( sortProperty );
                }
                else
                {
                    queryable = queryable.OrderBy( a => a.Name );
                }

                var accounts = new List<AccountHelper>();
                foreach ( var account in queryable )
                {
                    accounts.Add( new AccountHelper( account, rockContext ) );
                }

                gAccount.DataSource = accounts;
                gAccount.DataBind();
            }
        }

        #endregion

        #region HelperClass

        public class AccountHelper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int GroupCount { get; set; }
            public int MemberCount { get; set; }
            public string WelcomeEmailTemplate { get; set; }

            public AccountHelper( PCOAccount account, RockContext rockContext )
            {
                if ( account != null )
                {
                    Id = account.Id;
                    Name = account.Name;
                    WelcomeEmailTemplate = account.WelcomeEmailTemplate;

                    var pcoAccountFieldType = FieldTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.FieldType.PCO_ACCOUNT.AsGuid(), rockContext );
                    var groupEntityType = EntityTypeCache.Read<Rock.Model.Group>( false, rockContext );

                    if ( pcoAccountFieldType != null && groupEntityType != null )
                    {
                        var groupIds = new AttributeValueService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( v =>
                                v.Attribute.EntityTypeId == groupEntityType.Id &&
                                v.Attribute.FieldTypeId == pcoAccountFieldType.Id &&
                                v.EntityId.HasValue &&
                                v.Value == account.Guid.ToString() )
                            .Select( v => v.EntityId.Value )
                            .ToList();

                        GroupCount = new GroupService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m =>
                                groupIds.Contains( m.Id ) &&
                                m.IsActive )
                            .Count();

                        MemberCount = new GroupMemberService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m =>
                                groupIds.Contains( m.Group.Id ) &&
                                m.Group.IsActive &&
                                m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Count();
                    }
                }
            }

        }

        #endregion
    }
}