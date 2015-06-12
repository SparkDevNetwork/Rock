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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Blocks.Involvement
{
    [DisplayName( "Connection Request Detail" )]
    [Category( "Involvement" )]
    [Description( "Displays the details of the given connection request for editing state, status, etc." )]
    public partial class ConnectionRequestDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            ClearErrorMessage();

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
            }
            else
            {
                var connectionRequest = new ConnectionRequest { ConnectionOpportunityId = hfConnectionOpportunityId.ValueAsInt() };
                if ( connectionRequest != null )
                {
                    connectionRequest.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( connectionRequest, phAttributes, false );
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? connectionRequestId = PageParameter( pageReference, "ConnectionRequestId" ).AsIntegerOrNull();
            if ( connectionRequestId != null )
            {
                ConnectionRequest connectionRequest = new ConnectionRequestService( new RockContext() ).Get( connectionRequestId.Value );
                if ( connectionRequest != null )
                {
                    breadCrumbs.Add( new BreadCrumb( connectionRequest.PersonAlias.Person.FullName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New ConnectionOpportunity Member", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                ConnectionRequestService connectionRequestService = new ConnectionRequestService( rockContext );
                GroupMemberRequirementService connectionRequestRequirementService = new GroupMemberRequirementService( rockContext );
                ConnectionRequest connectionRequest;

                int connectionRequestId = int.Parse( hfConnectionRequestId.Value );

                // if adding a new connectionOpportunity member 
                if ( connectionRequestId.Equals( 0 ) )
                {
                    connectionRequest = new ConnectionRequest { Id = 0 };
                    connectionRequest.ConnectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
                }
                else
                {
                    // load existing connectionOpportunity member
                    connectionRequest = connectionRequestService.Get( connectionRequestId );
                }

                connectionRequest.LoadAttributes();

                Rock.Attribute.Helper.GetEditValues( phAttributes, connectionRequest );

                if ( !Page.IsValid )
                {
                    return;
                }

                // if the connectionRequest IsValue is false, and the UI controls didn't report any errors, it is probably because the custom rules of ConnectionRequest didn't pass.
                // So, make sure a message is displayed in the validation summary
                cvConnectionRequest.IsValid = connectionRequest.IsValid;

                if ( !cvConnectionRequest.IsValid )
                {
                    cvConnectionRequest.ErrorMessage = connectionRequest.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return;
                }

                // using WrapTransaction because there are three Saves
                rockContext.WrapTransaction( () =>
                {
                    if ( connectionRequest.Id.Equals( 0 ) )
                    {
                        connectionRequestService.Add( connectionRequest );
                    }

                    rockContext.SaveChanges();
                    connectionRequest.SaveAttributeValues( rockContext );

                } );
            }

            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["ConnectionOpportunityId"] = hfConnectionOpportunityId.Value;
            NavigateToParentPage( qryString );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfConnectionRequestId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["ConnectionOpportunityId"] = hfConnectionOpportunityId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ConnectionRequestService connectionRequestService = new ConnectionRequestService( new RockContext() );
                ConnectionRequest connectionRequest = connectionRequestService.Get( int.Parse( hfConnectionRequestId.Value ) );

                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["ConnectionOpportunityId"] = connectionRequest.ConnectionOpportunityId.ToString();
                NavigateToParentPage( qryString );
            }
        }

        #endregion

        #region Control Events

        #region ReadPanel Events
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            pnlReadDetails.Visible = false;
            wpConnectionOpportunityActivities.Visible = false;
            wpConnectionRequestWorkflow.Visible = false;
            pnlEditDetails.Visible = true;
        }
        protected void lbConnect_Click( object sender, EventArgs e )
        {

        }
        protected void lbTransfer_Click( object sender, EventArgs e )
        {

        }
        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="connectionRequestId">The connectionOpportunity member identifier.</param>
        public void ShowDetail( int connectionRequestId )
        {
            ShowDetail( connectionRequestId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="connectionRequestId">The connectionOpportunity member identifier.</param>
        /// <param name="connectionOpportunityId">The connectionOpportunity id.</param>
        public void ShowDetail( int connectionRequestId, int? connectionOpportunityId )
        {
            // autoexpand the person picker if this is an add
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "StartupScript", @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-authorizedperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }

            });", true );

            var rockContext = new RockContext();
            ConnectionRequest connectionRequest = null;

            if ( !connectionRequestId.Equals( 0 ) )
            {
                connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId );
            }
            else
            {
                // only create a new one if parent was specified
                if ( connectionOpportunityId.HasValue )
                {
                    connectionRequest = new ConnectionRequest { Id = 0 };
                    connectionRequest.ConnectionOpportunityId = connectionOpportunityId.Value;
                    connectionRequest.ConnectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionRequest.ConnectionOpportunityId );
                }
            }

            if ( connectionRequest == null )
            {
                nbErrorMessage.Title = "Invalid Request";
                nbErrorMessage.Text = "An incorrect querystring parameter was used.  A valid ConnectionRequestId or ConnectionOpportunityId parameter is required.";
                pnlReadDetails.Visible = false;
                return;
            }

            pnlReadDetails.Visible = true;

            hfConnectionOpportunityId.Value = connectionRequest.ConnectionOpportunityId.ToString();
            hfConnectionRequestId.Value = connectionRequest.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            var connectionOpportunity = connectionRequest.ConnectionOpportunity;
            if ( !string.IsNullOrWhiteSpace( connectionOpportunity.IconCssClass ) )
            {
                lConnectionOpportunityIconHtml.Text = string.Format( "<i class='{0}' ></i>", connectionOpportunity.IconCssClass );
            }
            else
            {
                lConnectionOpportunityIconHtml.Text = "<i class='fa fa-user' ></i>";
            }

            lReadOnlyTitle.Text = connectionRequest.PersonAlias.Person.FullName.FormatAsHtmlTitle();

            // user has to have EDIT Auth to the Block OR the connectionOpportunity
            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) && !connectionOpportunity.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionOpportunity.FriendlyTypeName );
            }

            //if ( connectionRequest.IsSystem )
            //{
            //    readOnly = true;
            //    nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( ConnectionOpportunity.FriendlyTypeName );
            //}

            btnSave.Visible = !readOnly;

            LoadDropDowns();

            ppConnectionRequestPerson.SetValue( connectionRequest.PersonAlias.Person );
            ppConnectionRequestPerson.Enabled = !readOnly;

            if ( connectionRequest.Id != 0 )
            {
                // once a connectionOpportunity member record is saved, don't let them change the person
                ppConnectionRequestPerson.Enabled = false;
            }

            //ddlConnectionOpportunityRole.SetValue( connectionRequest.ConnectionOpportunityRoleId );
            //ddlConnectionOpportunityRole.Enabled = !readOnly;

            rblStatus.SetValue( (int)connectionRequest.ConnectionStatus.Id );
            rblStatus.Enabled = !readOnly;
            rblStatus.Label = "Status";

            connectionRequest.LoadAttributes();
            phAttributes.Controls.Clear();

            Rock.Attribute.Helper.AddEditControls( connectionRequest, phAttributes, true, "", true );
            if ( readOnly )
            {
                Rock.Attribute.Helper.AddDisplayControls( connectionRequest, phAttributesReadOnly );
                phAttributesReadOnly.Visible = true;
                phAttributes.Visible = false;
            }
            else
            {
                phAttributesReadOnly.Visible = false;
                phAttributes.Visible = true;
            }

            if ( connectionRequest.AssignedGroupId != null )
            {
                pnlRequirements.Visible = true;
                ShowConnectionOpportunityRequirementsStatuses();
            }
            else
            {
                pnlRequirements.Visible = false;
            }
        }

        /// <summary>
        /// Shows the connectionOpportunity requirements statuses.
        /// </summary>
        private void ShowConnectionOpportunityRequirementsStatuses()
        {
            var rockContext = new RockContext();
            int connectionRequestId = hfConnectionRequestId.Value.AsInteger();
            var connectionOpportunityId = hfConnectionOpportunityId.Value.AsInteger();
            ConnectionRequest connectionRequest = null;

            connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId );


            var groupMember = new GroupMember { Id = 0 };
            groupMember.GroupId = connectionRequest.AssignedGroupId.Value;
            groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
            groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            cblManualRequirements.Items.Clear();
            lRequirementsLabels.Text = string.Empty;

            IEnumerable<GroupRequirementStatus> requirementsResults;

            if ( groupMember.IsNewOrChangedGroupMember( rockContext ) )
            {
                requirementsResults = groupMember.Group.PersonMeetsGroupRequirements( connectionRequest.PersonAlias.PersonId, connectionRequest.ConnectionOpportunity.GroupMemberRoleId );
            }
            else
            {
                requirementsResults = groupMember.GetGroupRequirementsStatuses().ToList();
            }

            // only show the requirements that apply to the GroupRole (or all Roles)
            foreach ( var requirementResult in requirementsResults.Where( a => a.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable ) )
            {
                if ( requirementResult.GroupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                {
                    var checkboxItem = new ListItem( requirementResult.GroupRequirement.GroupRequirementType.CheckboxLabel, requirementResult.GroupRequirement.Id.ToString() );
                    if ( string.IsNullOrEmpty( checkboxItem.Text ) )
                    {
                        checkboxItem.Text = requirementResult.GroupRequirement.GroupRequirementType.Name;
                    }

                    checkboxItem.Selected = requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.Meets;
                    cblManualRequirements.Items.Add( checkboxItem );
                }
                else
                {
                    bool meets = requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.Meets;
                    string labelText;
                    if ( meets )
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.PositiveLabel;
                    }
                    else
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.NegativeLabel;
                    }

                    if ( string.IsNullOrEmpty( labelText ) )
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.Name;
                    }

                    lRequirementsLabels.Text += string.Format(
                        @"<span class='label label-{1}'>{0}</span>
                        ",
                        labelText,
                        meets ? "success" : "danger" );
                }
            }

            var requirementsWithErrors = requirementsResults.Where( a => a.MeetsGroupRequirement == MeetsGroupRequirement.Error ).ToList();
            if ( requirementsWithErrors.Any() )
            {
                nbRequirementsErrors.Visible = true;
                nbRequirementsErrors.Text = string.Format(
                    "An error occurred in one or more of the requirement calculations: <br /> {0}",
                    requirementsWithErrors.AsDelimited( "<br />" ) );
            }
            else
            {
                nbRequirementsErrors.Visible = false;
            }
        }

        /// <summary>
        /// Clears the error message title and text.
        /// </summary>
        private void ClearErrorMessage()
        {
            nbErrorMessage.Title = string.Empty;
            nbErrorMessage.Text = string.Empty;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            int connectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
            ConnectionOpportunity connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( connectionOpportunityId );
            if ( connectionOpportunity != null )
            {
                //ddlConnectionOpportunityRole.DataSource = connectionOpportunity.ConnectionOpportunityType.Roles.OrderBy( a => a.Order ).ToList();
                //ddlConnectionOpportunityRole.DataBind();
            }

            // rblStatus.BindToEnum<Connec>();
        }

        #endregion

    }
}