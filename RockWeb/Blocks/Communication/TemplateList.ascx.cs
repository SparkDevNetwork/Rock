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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "Template List" )]
    [Category( "Communication" )]
    [Description( "Lists the available communication templates that can used when creating new communications." )]

    [LinkedPage( "Detail Page" )]
    public partial class TemplateList : Rock.Web.UI.RockBlock
    {
        #region fields

        private HashSet<int> _templatesWithCommunications = null;
        private bool _canEdit = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            gCommunicationTemplates.DataKeyNames = new string[] { "Id" };
            gCommunicationTemplates.Actions.ShowAdd = true;
            gCommunicationTemplates.Actions.AddClick += Actions_AddClick;
            gCommunicationTemplates.GridRebind += gCommunicationTemplates_GridRebind;

            // The created by column/filter should only be displayed if user is allowed to approve
            _canEdit = IsUserAuthorized( Authorization.EDIT );
            ppCreatedBy.Visible = _canEdit;
            RockBoundField createdByField = gCommunicationTemplates.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "CreatedByPersonAlias.Person.FullName" );
            createdByField.Visible = _canEdit;

            SecurityField securityField = gCommunicationTemplates.ColumnsOfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.CommunicationTemplate ) ).Id;

            // make a custom delete confirmation dialog
            gCommunicationTemplates.ShowConfirmDeleteDialog = false;

            string deleteScript = @"
    $('table.js-grid-communicationtemplate-list a.grid-delete-button').click(function( e ){
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
            if ( _canEdit )
            {
                rFilter.SaveUserPreference( "Created By", ppCreatedBy.PersonId.ToString() );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Communication Type":
                    {
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = ( ( CommunicationType ) System.Enum.Parse( typeof( CommunicationType ), e.Value ) ).ConvertToString();
                        }

                        break;
                    }

                case "Created By":
                    {
                        int? personId = e.Value.AsIntegerOrNull();
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
            NavigateToLinkedPage( "DetailPage", "TemplateId", 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gCommunicationTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void gCommunicationTemplates_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "TemplateId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gCommunicationTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gCommunicationTemplates_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new CommunicationTemplateService( rockContext );
            var template = service.Get( e.RowKeyId );
            if ( template != null )
            {
                if ( !template.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
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
            if ( !Page.IsPostBack )
            {
                if ( !_canEdit )
                {
                    rFilter.SaveUserPreference( "Created By", string.Empty );
                }

                int? personId = rFilter.GetUserPreference( "Created By" ).AsIntegerOrNull();
                if ( personId.HasValue && personId != 0 )
                {
                    var personService = new PersonService( new RockContext() );
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        ppCreatedBy.SetValue( person );
                    }
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var communicationTemplateQry = new CommunicationTemplateService( rockContext ).Queryable( "CreatedByPersonAlias.Person" );

            if ( _canEdit )
            {
                int? personId = rFilter.GetUserPreference( "Created By" ).AsIntegerOrNull();
                if ( personId.HasValue && personId != 0 )
                {
                    communicationTemplateQry = communicationTemplateQry
                        .Where( c =>
                            c.CreatedByPersonAlias != null &&
                            c.CreatedByPersonAlias.PersonId == personId.Value );
                }
            }

            var sortProperty = gCommunicationTemplates.SortProperty;

            if ( sortProperty != null )
            {
                communicationTemplateQry = communicationTemplateQry.Sort( sortProperty );
            }
            else
            {
                communicationTemplateQry = communicationTemplateQry.OrderBy( c => c.Name );
            }

            _templatesWithCommunications = new HashSet<int>( new CommunicationService( rockContext ).Queryable().Where( a => a.CommunicationTemplateId.HasValue ).Select( a => a.CommunicationTemplateId.Value ).Distinct().ToList() );

            var viewableCommunications = new List<CommunicationTemplate>();
            if ( _canEdit )
            {
                viewableCommunications = communicationTemplateQry.ToList();
            }
            else
            {
                foreach ( var comm in communicationTemplateQry.ToList() )
                {
                    if ( comm.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        viewableCommunications.Add( comm );
                    }
                }
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
            Literal lSupports = e.Row.FindControl( "lSupports" ) as Literal;
            CommunicationTemplate communicationTemplate = e.Row.DataItem as CommunicationTemplate;
            if ( lSupports != null && communicationTemplate != null )
            {
                StringBuilder html = new StringBuilder();
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
        }

        #endregion
    }
}