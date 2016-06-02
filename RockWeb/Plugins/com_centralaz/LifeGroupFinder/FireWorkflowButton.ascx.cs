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
using System.Linq;
using System.Text;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.LifeGroupFinder
{
    [DisplayName( "Fire Workflow Button" )]
    [Category( "com_centralaz > Groups" )]
    [Description( "Allows a user to fire off a workflow" )]

    [WorkflowTypeField( "Workflow Actions", "The workflows to make available as actions.", false, false, "", "", 1 )]
    [LinkedPage( "Workflow Entry Page", "", true, "", "", 0 )]
    [TextField( "Button Text", "The text the button will have." )]
    public partial class FireWorkflowButton : RockBlock
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

            if ( !Page.IsPostBack )
            {
                List<LeaderToolboxWorkflow> workflowList = new List<LeaderToolboxWorkflow>();
                var workflowActions = GetAttributeValue( "WorkflowActions" );
                var groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
                if ( groupId != null )
                {
                    if ( !string.IsNullOrWhiteSpace( workflowActions ) )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var group = new GroupService( rockContext ).Get( groupId.Value );
                            if ( group != null )
                            {
                                var workflowTypeService = new WorkflowTypeService( rockContext );
                                foreach ( string guidValue in workflowActions.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                                {
                                    Guid? guid = guidValue.AsGuidOrNull();
                                    if ( guid.HasValue )
                                    {
                                        var workflowType = workflowTypeService.Get( guid.Value );
                                        if ( workflowType != null && workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                                        {
                                            var queryParams = new Dictionary<string, string>();
                                            queryParams.Add( "WorkflowTypeId", workflowType.Id.ToString() );
                                            queryParams.Add( "GroupId", groupId.ToString() );
                                            string url = LinkedPageUrl( "WorkflowEntryPage", queryParams );
                                            workflowList.Add( new LeaderToolboxWorkflow
                                            {
                                                Name = !String.IsNullOrWhiteSpace( GetAttributeValue( "ButtonText" ) ) ? GetAttributeValue( "ButtonText" ) : workflowType.Name,
                                                Url = ResolveRockUrl( url )
                                            } );
                                        }
                                    }
                                }

                                if ( workflowList.Any() && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                                {
                                    rptLeaderWorkflows.Visible = true;
                                    rptLeaderWorkflows.DataSource = workflowList.ToList();
                                    rptLeaderWorkflows.DataBind();
                                }
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class to store the event item occurrences dates
        /// </summary>
        public class LeaderToolboxWorkflow
        {
            /// <summary>
            /// Gets or sets the event item occurrence.
            /// </summary>
            /// <value>
            /// The event item occurrence.
            /// </value>
            public String Name { get; set; }

            /// <summary>
            /// Gets or sets the dates.
            /// </summary>
            /// <value>
            /// The dates.
            /// </value>
            public String Url { get; set; }
        }

        #endregion
    }
}