// <copyright>
// Copyright by Triumph Tech
//
// NOTICE: All information contained herein is, and remains
// the property of Triumph Tech LLC. The intellectual and technical concepts contained
// herein are proprietary to Triumph Tech LLC  and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
//
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from Triumph Tech LLC.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

using tech.triumph.WistiaIntegration.Model;

namespace RockWeb.Plugins.tech_triumph.WistiaIntegration
{
    /// <summary>
    /// Displays the details of the given Project for a wistia account.
    /// </summary>
    [DisplayName( "Project Detail" )]
    [Category( "Triumph Tech > Wistia Integration" )]
    [Description( "Displays the details of the given Project for a wistia account." )]
    public partial class ProjectDetail : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var projectId = PageParameter( "projectId" ).AsInteger();

            if ( !Page.IsPostBack )
            {
                if ( projectId.Equals( 0 ) )
                {
                    this.Visible = false;
                    return;
                }

                ShowDetail( projectId );
            }

        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? projectId = PageParameter( pageReference, "ProjectId" ).AsIntegerOrNull();
            if ( projectId != null )
            {
                string projectName = new WistiaProjectService( new RockContext() )
                    .Queryable().AsNoTracking()
                    .Where( p => p.Id == projectId.Value )
                    .Select( p => p.Name )
                    .FirstOrDefault();

                if ( !string.IsNullOrWhiteSpace( projectName ) )
                {
                    breadCrumbs.Add( new BreadCrumb( projectName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Project", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            WistiaProject project = null;
            WistiaProjectService projectService = new WistiaProjectService( rockContext );

            int projectId = hfProjectId.ValueAsInt();
            if ( projectId > 0 )
            {
                project = projectService.Get( projectId );
            }

            project.ShowInIntegration = cbShowInIntegration.Checked;

            if ( !project.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            ShowReadonlyDetails( project );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            WistiaProjectService projectService = new WistiaProjectService( new RockContext() );
            WistiaProject project = projectService.Get( hfProjectId.ValueAsInt() );
            ShowEditDetails( project );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            // Cancelling on Edit.  Return to Details
            WistiaProjectService projectService = new WistiaProjectService( new RockContext() );
            WistiaProject project = projectService.Get( hfProjectId.ValueAsInt() );
            ShowReadonlyDetails( project );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="project">Wistia Project.</param>
        private void ShowReadonlyDetails( WistiaProject project )
        {
            SetEditMode( false );

            hfProjectId.SetValue( project.Id );
            lTitle.Text = project.Name.FormatAsHtmlTitle();

            DescriptionList detailDescription = new DescriptionList();
            detailDescription.Add( "Name", project.Name );
            detailDescription.Add( "Account", project.WistiaAccount.Name );
            if ( !string.IsNullOrEmpty( project.Description ) )
            {
                detailDescription.Add( "Description", project.Description );
            }
            detailDescription.Add( "Is Public", project.IsPublic.ToYesNo() );
            detailDescription.Add( "Show In Integration", project.ShowInIntegration.ToYesNo() );

            lblMainDetails.Text = detailDescription.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }


        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="project">Wistia Project.</param>
        private void ShowEditDetails( WistiaProject project )
        {
            lTitle.Text = project.Name.FormatAsHtmlTitle();
            SetEditMode( true );

            cbShowInIntegration.Checked = project.ShowInIntegration;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        public void ShowDetail( int projectId )
        {
            pnlDetails.Visible = true;
            WistiaProject project = null;

            if ( !projectId.Equals( 0 ) )
            {
                project = new WistiaProjectService( new RockContext() ).Get( projectId );
            }

            hfProjectId.SetValue( project.Id );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !UserCanEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( WistiaProject.FriendlyTypeName );
            }

            ShowReadonlyDetails( project );
            if ( readOnly )
            {
                btnEdit.Visible = false;
            }
            else
            {
                btnEdit.Visible = true;
            }

            lMediaCount.Text = project.WistiaMediaItems.Count().ToString();

            if ( project.PlayCount.HasValue )
            {
                lPlayCount.Text = project.PlayCount.Value.ToString( "N0" );
            }

            if ( project.LoadCount.HasValue )
            {
                lLoadCount.Text = project.LoadCount.Value.ToString( "N0" );
            }

            if ( project.HoursWatched.HasValue )
            {
                lHoursWatched.Text = project.HoursWatched.Value.ToString( "#,##0.0" );
            }
        }
    }


        #endregion
}
