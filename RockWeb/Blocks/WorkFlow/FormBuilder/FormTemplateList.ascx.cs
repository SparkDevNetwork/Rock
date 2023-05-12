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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace RockWeb.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// List block that shows a list of form templates.
    /// </summary>
    [DisplayName( "Form Template List" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows a list form templates." )]

    #region Rock Attributes

    [LinkedPage(
        "Detail Page",
        Description = "Page to display details about a form template.",
        Order = 0,
        Key = AttributeKeys.DetailPage )]

    #endregion Rock Attributes

    [Rock.SystemGuid.BlockTypeGuid( "1DEFF313-39CF-400F-895A-82ADB9F192BD" )]
    public partial class FormTemplateList : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKeys
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys for page parameters extracted from the page route
        /// </summary>
        private static class PageParameterKeys
        {
            public const string FormTemplateId = "FormTemplateId";
        }

        #endregion Page Parameter Keys

        #region User Preference Keys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKeys
        {
            public const string Active = "Active";
        }

        #endregion User Preferance Keys

        #region Properties

        /// <summary>
        /// Gets or sets the ModalAlert control that shows page-level notifications to the user.
        /// </summary>
        public ModalAlert ModalAlertControl { get; set; }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gFormTemplates.DataKeyNames = new string[] { "Id" };
            gFormTemplates.GridRebind += gFormTemplates_GridRebind;
            gFormTemplates.Actions.ShowAdd = true;
            gFormTemplates.Actions.AddClick += Actions_AddClick;

            var securityField = gFormTemplates.ColumnsOfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = EntityTypeCache.Get<WorkflowFormBuilderTemplate>().Id;

            gfFormTemplates.ApplyFilterClick += gfFormTemplates_ApplyFilterClick;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            ModalAlertControl = mdAlert;

            BindGrid();
        }

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ( !Page.IsPostBack )
            {
                BindFilters();
                BindGrid();
            }
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the AddClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void Actions_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.DetailPage, PageParameterKeys.FormTemplateId, 0 );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFormTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gFormTemplates_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilter event of the gfFormTemplates control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param></param>
        private void gfFormTemplates_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFormTemplates.SetFilterPreference( UserPreferenceKeys.Active, ddlIsActive.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gFormTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gFormTemplates_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.DetailPage, PageParameterKeys.FormTemplateId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gFormTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFormTemplates_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                int templateId = ( int ) e.RowKeyValue;
                var workflowService = new WorkflowService( rockContext );
                var hasTemplates = workflowService.Queryable().Any( wf => wf.WorkflowType.FormBuilderTemplateId == templateId );
                if ( hasTemplates )
                {
                    ShowAlert( "This template has workflows assigned to it", ModalAlertType.Warning );
                }
                else
                {
                    var formTemplateService = new WorkflowFormBuilderTemplateService( rockContext );
                    var template = formTemplateService.Get( templateId );
                    formTemplateService.Delete( template );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var builderTemplateService = new WorkflowFormBuilderTemplateService( new RockContext() );
            var qry = builderTemplateService.Queryable().AsNoTracking();

            var isActive = gfFormTemplates.GetFilterPreference( UserPreferenceKeys.Active ).AsBooleanOrNull();

            // A null is active defaults to showing only active
            if ( !isActive.HasValue )
            {
                isActive = true;
            }

            if ( isActive.HasValue )
            {
                qry = qry.Where( w => w.IsActive == isActive.Value );
            }

            qry = qry.OrderBy( w => w.Name );

            gFormTemplates.SetLinqDataSource( qry );
            gFormTemplates.DataBind();
        }

        /// <summary>
        /// Sets the selected filter values on page load
        /// </summary>
        private void BindFilters()
        {
            var isActive = gfFormTemplates.GetFilterPreference( UserPreferenceKeys.Active ).AsBooleanOrNull();
            if ( isActive.HasValue )
            {
                var value = isActive.Value ? "Yes" : "No";
                ddlIsActive.SetValue( value );
            }
        }

        /// <summary>
        /// Show an alert message that requires user acknowledgement to continue.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="alertType"></param>
        private void ShowAlert( string message, ModalAlertType alertType )
        {
            ModalAlertControl.Show( message, alertType );
        }

        #endregion Methods
    }
}