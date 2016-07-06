// <copyright>
// Copyright by Central Christian Church
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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Connection Request Entry" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "A block for servant ministers to use to enter connection requests into Rock." )]
    [ConnectionTypesField( "Include Connection Types", "The connection types to include." )]
    public partial class ConnectionRequestEntry : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                nbSuccess.Text = string.Empty;
                nbSuccess.Visible = false;
                nbDanger.Visible = false;
                LoadOpportunities();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemDataBound event of the rptConnnectionTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptConnnectionTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var cblOpportunities = e.Item.FindControl( "cblOpportunities" ) as RockCheckBoxList;
            var lConnectionTypeName = e.Item.FindControl( "lConnectionTypeName" ) as Literal;
            var connectionType = e.Item.DataItem as ConnectionType;
            if ( cblOpportunities != null && lConnectionTypeName != null && connectionType != null )
            {
                lConnectionTypeName.Text = String.Format( "<h4 class='block-title'>{0}</h4>", connectionType.Name );

                cblOpportunities.DataSource = connectionType.ConnectionOpportunities.OrderBy( c => c.Name );
                cblOpportunities.DataBind();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSubmit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var opportunityService = new ConnectionOpportunityService( rockContext );
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var personService = new PersonService( rockContext );
                if ( ppPerson.PersonId.HasValue )
                {
                    Person person = personService.Get( ppPerson.PersonId.Value );
                    List<string> opportunityNames = new List<string>();

                    // If there is a valid person with a primary alias, continue
                    if ( person != null && person.PrimaryAliasId.HasValue )
                    {

                        foreach ( RepeaterItem typeItem in rptConnnectionTypes.Items )
                        {
                            var cblOpportunities = typeItem.FindControl( "cblOpportunities" ) as RockCheckBoxList;
                            foreach ( int connectionOpportunityId in cblOpportunities.SelectedValuesAsInt )
                            {

                                // Get the opportunity and default status
                                var opportunity = opportunityService
                                    .Queryable()
                                    .Where( o => o.Id == connectionOpportunityId )
                                    .FirstOrDefault();

                                int defaultStatusId = opportunity.ConnectionType.ConnectionStatuses
                                    .Where( s => s.IsDefault )
                                    .Select( s => s.Id )
                                    .FirstOrDefault();

                                // If opportunity is valid and has a default status
                                if ( opportunity != null && defaultStatusId > 0 )
                                {
                                    // Now we create the connection request
                                    var connectionRequest = new ConnectionRequest();
                                    connectionRequest.PersonAliasId = person.PrimaryAliasId.Value;
                                    connectionRequest.Comments = tbComments.Text.Trim();
                                    connectionRequest.ConnectionOpportunityId = opportunity.Id;
                                    connectionRequest.ConnectionState = ConnectionState.Active;
                                    connectionRequest.ConnectionStatusId = defaultStatusId;

                                    if ( person.GetCampus() != null )
                                    {
                                        var campusId = person.GetCampus().Id;
                                        connectionRequest.CampusId = campusId;
                                        connectionRequest.ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campusId );
                                        if (
                                            opportunity != null &&
                                            opportunity.ConnectionOpportunityCampuses != null )
                                        {
                                            var campus = opportunity.ConnectionOpportunityCampuses
                                                .Where( c => c.CampusId == campusId )
                                                .FirstOrDefault();
                                            if ( campus != null )
                                            {
                                                connectionRequest.ConnectorPersonAliasId = campus.DefaultConnectorPersonAliasId;
                                            }
                                        }
                                    }

                                    if ( !connectionRequest.IsValid )
                                    {
                                        // Controls will show warnings
                                        return;
                                    }

                                    opportunityNames.Add( opportunity.Name );

                                    connectionRequestService.Add( connectionRequest );
                                }
                            }
                        }
                    }

                    if ( opportunityNames.Count > 0 )
                    {
                        rockContext.SaveChanges();

                        // Reset everything for the next person
                        tbComments.Text = string.Empty;
                        LoadOpportunities();
                        ppPerson.SetValue( null );

                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine( String.Format( "{0}'s connection requests have been entered for the following opportunities:\n<ul>", person.FullName ) );
                        foreach ( var name in opportunityNames )
                        {
                            sb.AppendLine( String.Format( "<li> {0}</li>", name ) );
                        }
                        sb.AppendLine( "</ul>" );

                        nbSuccess.Text = sb.ToString();
                        nbSuccess.Visible = true;
                        nbDanger.Visible = false;
                    }
                    else
                    {
                        nbDanger.Visible = true;
                        nbDanger.Text = "Please select an opportunity.";
                        nbSuccess.Visible = false;
                    }
                }
                else
                {
                    nbDanger.Visible = true;
                    nbDanger.Text = "Please select a person.";
                    nbSuccess.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            hlCampus.Text = new PersonService( new RockContext() ).Get( ppPerson.PersonId.Value ).GetCampus().Name;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            nbSuccess.Text = string.Empty;
            nbSuccess.Visible = false;
            nbDanger.Visible = false;
            LoadOpportunities();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the opportunities.
        /// </summary>
        private void LoadOpportunities()
        {
            var typeFilter = GetAttributeValue( "IncludeConnectionTypes" ).SplitDelimitedValues().AsGuidList();
            rptConnnectionTypes.DataSource = new ConnectionTypeService( new RockContext() ).Queryable().Where( t => typeFilter.Contains( t.Guid ) ).ToList();
            rptConnnectionTypes.DataBind();
        }

        #endregion

    }
}