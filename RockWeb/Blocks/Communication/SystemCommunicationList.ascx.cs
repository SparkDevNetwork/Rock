// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for managing the system emails
    /// </summary>
    [DisplayName( "System Communication List" )]
    [Category( "Communication" )]
    [Description( "Lists the system communications that can be configured for use by the system and other automated (non-user) tasks." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]
    public partial class SystemCommunicationList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
        }

        #endregion Page Parameter Keys

        #region User Preference Keys

        /// <summary>
        /// Keys to use for Filter Settings
        /// </summary>
        private static class FilterSettingName
        {
            public const string Category = "Category";
            public const string Active = "Active";
            public const string Supports = "Supports";
        }

        #endregion

        #region Filter Values

        /// <summary>
        /// Keys to use for Filter Values: Supports
        /// </summary>
        private static class NotificationTypeSupportedFilterValueSpecifier
        {
            public const string Email = "Email";
            public const string SMS = "SMS";
            public const string Push = "Push Notification";
        }

        /// <summary>
        /// Keys to use for Filter Values: Active
        /// </summary>
        private static class IsActiveFilterValueSpecifier
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                gEmailTemplates.DataKeyNames = new string[] { "Id" };
                gEmailTemplates.Actions.ShowAdd = true;
                gEmailTemplates.Actions.AddClick += gEmailTemplates_AddClick;
                gEmailTemplates.GridRebind += gEmailTemplates_GridRebind;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                if ( !Page.IsPostBack )
                {
                    LoadFilterSelectionLists();
                    BindFilter();
                    BindGrid();
                }
            }
            else
            {
                gEmailTemplates.Visible = false;
                nbMessage.Text = WarningMessage.NotAuthorizedToEdit( SystemCommunication.FriendlyTypeName );
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            int? categoryId = cpCategory.SelectedValueAsInt();
            rFilter.SaveUserPreference( FilterSettingName.Category, categoryId.HasValue ? categoryId.Value.ToString() : "");

            rFilter.SaveUserPreference( FilterSettingName.Supports, ddlSupports.SelectedValue );
            rFilter.SaveUserPreference( FilterSettingName.Active, ddlActiveFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == FilterSettingName.Category )
            {
                int? categoryId = e.Value.AsIntegerOrNull();
                if ( categoryId.HasValue )
                {
                    var category = CategoryCache.Get( categoryId.Value );
                    if ( category != null )
                    {
                        e.Value = category.Name;
                    }
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.CommunicationId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.CommunicationId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            SystemCommunicationService emailTemplateService = new SystemCommunicationService( rockContext );
            SystemCommunication emailTemplate = emailTemplateService.Get( e.RowKeyId );
            if ( emailTemplate != null )
            {
                emailTemplateService.Delete( emailTemplate );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gEmailTemplates_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var lSupports = e.Row.FindControl( "lSupports" ) as Literal;

            var systemCommunication = e.Row.DataItem as SystemCommunication;

            if ( lSupports == null || systemCommunication == null )
            {
                return;
            }

            var html = new StringBuilder();

            if ( !string.IsNullOrWhiteSpace( systemCommunication.SMSMessage ) )
            {
                html.AppendLine( "<span class='label label-info'>SMS</span>" );
            }
            if ( !string.IsNullOrWhiteSpace( systemCommunication.PushMessage ) )
            {
                html.AppendLine( "<span class='label label-info'>Push</span>" );
            }

            lSupports.Text = html.ToString();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Populate the selection lists for the filter.
        /// </summary>
        private void LoadFilterSelectionLists()
        {

            ddlSupports.Items.Clear();
            ddlSupports.Items.Add( new ListItem() );
            ddlSupports.Items.Add( new ListItem( NotificationTypeSupportedFilterValueSpecifier.SMS ) );
            ddlSupports.Items.Add( new ListItem( NotificationTypeSupportedFilterValueSpecifier.Push ) );

            ddlActiveFilter.Items.Clear();
            ddlActiveFilter.Items.Add( new ListItem() );
            ddlActiveFilter.Items.Add( new ListItem( IsActiveFilterValueSpecifier.Active ) );
            ddlActiveFilter.Items.Add( new ListItem( IsActiveFilterValueSpecifier.Inactive ) );
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            int? categoryId = rFilter.GetUserPreference( FilterSettingName.Category ).AsIntegerOrNull();
            cpCategory.SetValue( categoryId );

            ddlActiveFilter.SetValue( rFilter.GetUserPreference( FilterSettingName.Active ) );

            ddlSupports.SetValue( rFilter.GetUserPreference( FilterSettingName.Supports ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var SystemCommunicationService = new SystemCommunicationService( new RockContext() );
            SortProperty sortProperty = gEmailTemplates.SortProperty;

            var systemCommunications = SystemCommunicationService.Queryable( "Category" );

            // Filter By: Category
            int? categoryId = rFilter.GetUserPreference( FilterSettingName.Category ).AsIntegerOrNull();
            if ( categoryId.HasValue )
            {
                systemCommunications = systemCommunications.Where( a => a.CategoryId.HasValue && a.CategoryId.Value == categoryId.Value  );
            }

            // Filter By: Is Active
            var activeFilter = rFilter.GetUserPreference( FilterSettingName.Active );
            switch ( activeFilter )
            {
                case "Active":
                    systemCommunications = systemCommunications.Where( a => a.IsActive ?? false );
                    break;
                case "Inactive":
                    systemCommunications = systemCommunications.Where( a => !(a.IsActive ?? false) );
                    break;
            }

            // Filter By: Supports (Email|SMS)
            var supports = rFilter.GetUserPreference( FilterSettingName.Supports );
            switch ( supports )
            {
                case NotificationTypeSupportedFilterValueSpecifier.SMS:
                    systemCommunications = systemCommunications.Where( a => a.SMSMessage != null && a.SMSMessage.Trim() != "" );
                    break;
                case NotificationTypeSupportedFilterValueSpecifier.Push:
                    systemCommunications = systemCommunications.Where( a => a.PushMessage != null && a.PushMessage.Trim() != "" );
                    break;
            }

            // Apply grid sort order.
            if ( sortProperty != null )
            {
                gEmailTemplates.DataSource = systemCommunications.Sort( sortProperty ).ToList();
            }
            else
            {
                gEmailTemplates.DataSource = systemCommunications.OrderBy( a => a.Category.Name ).ThenBy( a => a.Title ).ToList();
            }

            gEmailTemplates.EntityTypeId = EntityTypeCache.Get<Rock.Model.SystemCommunication>().Id;
            gEmailTemplates.DataBind();
        }

        #endregion
    }
}