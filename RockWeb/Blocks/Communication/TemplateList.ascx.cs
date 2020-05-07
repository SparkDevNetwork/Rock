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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "Template List" )]
    [Category( "Communication" )]
    [Description( "Lists the available communication templates that can used when creating new communications." )]

    #region Block Attributes
    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage )]
    [BooleanField(
        "Personal Templates View",
        Description = "Is the block being used to display personal templates (only templates that current user is allowed to edit)?",
        DefaultBooleanValue = false,
        Order = 1,
        Key = AttributeKey.PersonalTemplatesView )]
    #endregion Block Attributes
    public partial class TemplateList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string PersonalTemplatesView = "PersonalTemplatesView";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string TemplateId = "TemplateId";
        }

        #endregion

        #region fields

        private HashSet<int> _templatesWithCommunications;
        private bool _canFilterCreatedBy;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
            }

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            gCommunicationTemplates.DataKeyNames = new[] { "Id" };
            gCommunicationTemplates.Actions.ShowAdd = true;
            gCommunicationTemplates.Actions.AddClick += Actions_AddClick;
            gCommunicationTemplates.GridRebind += gCommunicationTemplates_GridRebind;

            // The created by column/filter should only be displayed if user is allowed to edit the block
            _canFilterCreatedBy = IsUserAuthorized( Authorization.EDIT );
            ppCreatedBy.Visible = _canFilterCreatedBy;
            var createdByField = gCommunicationTemplates.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "CreatedByPersonAlias.Person.FullName" );
            if ( createdByField != null )
            {
                createdByField.Visible = _canFilterCreatedBy;
            }

            var securityField = gCommunicationTemplates.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( CommunicationTemplate ) ).Id;
            }

            // make a custom delete confirmation dialog
            gCommunicationTemplates.ShowConfirmDeleteDialog = false;

            string deleteScript = @"
    $('table.js-grid-communicationtemplate-list a.grid-delete-button').on('click', function( e ){
        var $btn = $(this);
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this template?', function (result) {
            if (result) {
                if ( $btn.closest('tr').hasClass('js-has-communications') ) {
                    Rock.dialogs.confirm('Deleting this template will unlink it from communications that have used it. If you would like to keep the link consider inactivating the template instead. Are you sure you wish to delete this template?', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                } else {
                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gCommunicationTemplates, gCommunicationTemplates.GetType(), "deleteCommunicationTemplateScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
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
            if ( _canFilterCreatedBy )
            {
                rFilter.SaveUserPreference( "Created By", ppCreatedBy.PersonId.ToString() );
            }

            rFilter.SaveUserPreference( "Category", cpCategory.SelectedValue );
            rFilter.SaveUserPreference( "Supports", ddlSupports.SelectedValue );
            rFilter.SaveUserPreference( "Active", ddlActiveFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Category":
                    {
                        var categoryId = e.Value.AsIntegerOrNull();
                        if ( categoryId.HasValue && categoryId > 0 )
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
                        break;
                    }

                case "Communication Type":
                    {
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = ( (CommunicationType)Enum.Parse( typeof( CommunicationType ), e.Value ) ).ConvertToString();
                        }

                        break;
                    }

                case "Active":
                    {
                        break;
                    }

                case "Supports":
                    {
                        break;
                    }

                case "Created By":
                    {
                        var personId = e.Value.AsIntegerOrNull();
                        if ( personId.HasValue && personId != 0 )
                        {
                            var personService = new PersonService( new RockContext() );
                            var person = personService.Get( personId.Value );
                            if ( person != null )
                            {
                                e.Value = person.FullName;
                            }
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Actions_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.TemplateId, 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gCommunicationTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void gCommunicationTemplates_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.TemplateId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Copy event of the gCommunicationTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCommunicationTemplates_Copy( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new CommunicationTemplateService( rockContext );
            var template = service.Get( e.RowKeyId );
            if ( template != null )
            {
                var templateCopy = template.Clone( false );
                templateCopy.Id = 0;
                int copyNumber = 0;
                var copyName = "Copy of " + template.Name;
                while ( service.Queryable().Any( a => a.Name == copyName ) )
                {
                    copyNumber++;
                    copyName = string.Format( "Copy({0}) of {1}", copyNumber, template.Name );
                }

                templateCopy.Name = copyName.Truncate( 100 );
                templateCopy.IsSystem = false;
                templateCopy.Guid = Guid.NewGuid();
                templateCopy.CreatedByPersonAlias = null;
                templateCopy.CreatedByPersonAliasId = null;
                templateCopy.ModifiedByPersonAlias = null;
                templateCopy.ModifiedByPersonAliasId = null;
                templateCopy.LogoBinaryFileId = null;
                templateCopy.ImageFileId = null;
                templateCopy.CreatedDateTime = RockDateTime.Now;
                templateCopy.ModifiedDateTime = RockDateTime.Now;
                service.Add( templateCopy );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gCommunicationTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gCommunicationTemplates_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new CommunicationTemplateService( rockContext );
            var template = service.Get( e.RowKeyId );
            if ( template != null )
            {
                if ( !template.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    maGridWarning.Show( "You are not authorized to delete this template", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !service.CanDelete( template, out errorMessage ) )
                {
                    maGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( template );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gCommunicationTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void gCommunicationTemplates_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( !_canFilterCreatedBy )
            {
                rFilter.SaveUserPreference( "Created By", string.Empty );
            }

            if ( _canFilterCreatedBy )
            {
                var personId = rFilter.GetUserPreference( "Created By" ).AsIntegerOrNull();
                if ( personId.HasValue && personId != 0 )
                {
                    var personService = new PersonService( new RockContext() );
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        ppCreatedBy.SetValue( person );
                    }
                }
                else
                {
                    ppCreatedBy.SetValue( null );
                }
            }

            var categoryId = rFilter.GetUserPreference( "Category" ).AsIntegerOrNull();
            if ( categoryId > 0 )
            {
                cpCategory.SetValue( categoryId );
            }
            else
            {
                cpCategory.SetValue( null );
            }

            ddlActiveFilter.SetValue( rFilter.GetUserPreference( "Active" ) );
            ddlSupports.SetValue( rFilter.GetUserPreference( "Supports" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var communicationTemplateQry = new CommunicationTemplateService( rockContext ).Queryable( "CreatedByPersonAlias.Person" );

            if ( _canFilterCreatedBy )
            {
                var personId = rFilter.GetUserPreference( "Created By" ).AsIntegerOrNull();
                if ( personId.HasValue && personId != 0 )
                {
                    communicationTemplateQry = communicationTemplateQry
                        .Where( c =>
                            c.CreatedByPersonAlias != null &&
                            c.CreatedByPersonAlias.PersonId == personId.Value );
                }
            }

            var categoryId = rFilter.GetUserPreference( "Category" ).AsIntegerOrNull();
            if ( categoryId.HasValue && categoryId > 0 )
            {
                communicationTemplateQry = communicationTemplateQry.Where( a => a.CategoryId.HasValue && a.CategoryId.Value == categoryId.Value );
            }

            var activeFilter = rFilter.GetUserPreference( "Active" );
            switch ( activeFilter )
            {
                case "Active":
                    communicationTemplateQry = communicationTemplateQry.Where( a => a.IsActive );
                    break;
                case "Inactive":
                    communicationTemplateQry = communicationTemplateQry.Where( a => !a.IsActive );
                    break;
            }

            var sortProperty = gCommunicationTemplates.SortProperty;

            communicationTemplateQry = sortProperty != null ? communicationTemplateQry.Sort( sortProperty ) : communicationTemplateQry.OrderBy( c => c.Name );

            _templatesWithCommunications = new HashSet<int>( new CommunicationService( rockContext ).Queryable().Where( a => a.CommunicationTemplateId.HasValue ).Select( a => a.CommunicationTemplateId.Value ).Distinct().ToList() );

            var personalView = GetAttributeValue( AttributeKey.PersonalTemplatesView ).AsBoolean();
            var viewableCommunications = new List<CommunicationTemplate>();
            foreach ( var comm in communicationTemplateQry.ToList() )
            {
                var isViewable = comm.IsAuthorized( personalView ? Authorization.EDIT : Authorization.VIEW, CurrentPerson );
                if ( isViewable )
                {
                    viewableCommunications.Add( comm );
                }
            }

            var supports = rFilter.GetUserPreference( "Supports" );
            switch ( supports )
            {
                case "Email Wizard":
                    viewableCommunications = viewableCommunications.Where( a => a.SupportsEmailWizard() ).ToList();
                    break;
                case "Simple Email Template":
                    viewableCommunications = viewableCommunications.Where( a => !a.SupportsEmailWizard() ).ToList();
                    break;
            }

            gCommunicationTemplates.DataSource = viewableCommunications;
            gCommunicationTemplates.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gCommunicationTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gCommunicationTemplates_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var lSupports = e.Row.FindControl( "lSupports" ) as Literal;
            var communicationTemplate = e.Row.DataItem as CommunicationTemplate;
            if ( lSupports == null || communicationTemplate == null ) return;

            var html = new StringBuilder();
            if ( communicationTemplate.SupportsEmailWizard() )
            {
                html.AppendLine( "<span class='label label-success' title='This template contains an email template that supports the new communication wizard'>Email Wizard</span>" );
            }
            else if ( !string.IsNullOrEmpty( communicationTemplate.Message ) )
            {
                html.AppendLine( "<span class='label label-default' title='This template does not contain an email template that supports the new communication wizard'>Simple Email Template</span>" );
            }

            if ( communicationTemplate.Guid == Rock.SystemGuid.Communication.COMMUNICATION_TEMPLATE_BLANK.AsGuid() || communicationTemplate.HasSMSTemplate() )
            {
                html.AppendLine( "<span class='label label-success'>SMS</span>" );
            }

            lSupports.Text = html.ToString();

            if ( _templatesWithCommunications.Contains( communicationTemplate.Id ) )
            {
                e.Row.AddCssClass( "js-has-communications" );
            }
        }

        #endregion


    }
}