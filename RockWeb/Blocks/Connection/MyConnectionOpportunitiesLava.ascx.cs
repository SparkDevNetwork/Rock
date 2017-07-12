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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block to display connection opportunities that are assigned to the current user. The display format is controlled by a lava template.
    /// </summary>
    [DisplayName( "My Connection Opportunities Lava" )]
    [Category( "Connection" )]
    [Description( "Block to display connection opportunities that are assigned to the current user. The display format is controlled by a lava template." )]

    [LinkedPage( "Detail Page", "Page used to view details of a request.", false, Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL, "", 1 )]
    [ConnectionTypesField( "Connection Types", "Optional list of connection types to limit the display to (All will be displayed by default).", false, order: 2 )]
    [CodeEditorField( "Contents", @"The Lava template to use for displaying connection opportunities assigned to current user.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, @"{% include '~~/Assets/Lava/MyConnectionOpportunitiesSortable.lava' %}", "", 3 )]
    public partial class MyConnectionOpportunitiesLava : Rock.Web.UI.RockBlock
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
                BindData();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion

        #region Methods

        private void BindData()
        {
            string contents = GetAttributeValue( "Contents" );

            string appRoot = ResolveRockUrl( "~/" );
            string themeRoot = ResolveRockUrl( "~~/" );
            contents = contents.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            var connectionTypeGuids = GetAttributeValue( "ConnectionTypes" ).SplitDelimitedValues().AsGuidList();

            DateTime midnightToday = RockDateTime.Today.AddDays( 1 );

            var rockContext = new RockContext();
            var connectionRequests = new ConnectionRequestService( rockContext ).Queryable()
                .Where( a => a.ConnectorPersonAlias != null && a.ConnectorPersonAlias.PersonId == CurrentPersonId )
                .Where( r => r.ConnectionState == ConnectionState.Active ||
                                    ( r.ConnectionState == ConnectionState.FutureFollowUp && r.FollowupDate.HasValue && r.FollowupDate.Value < midnightToday ) );

            if ( connectionTypeGuids.Any() )
            {
                connectionRequests = connectionRequests.Where( a => connectionTypeGuids.Contains( a.ConnectionOpportunity.ConnectionType.Guid ) );
            }

            connectionRequests = connectionRequests.OrderBy( r => r.PersonAlias.Person.LastName ).ThenBy( r => r.PersonAlias.Person.NickName );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "ConnectionRequests", connectionRequests.ToList() );

            var lastActivityNotes = connectionRequests.Select( r => new
            {
                ConnectionRequestId = r.Id,
                LastActivity = r.ConnectionRequestActivities.OrderByDescending(
                            a => a.CreatedDateTime ).FirstOrDefault()
            } ).ToDictionary( k => k.ConnectionRequestId, v => v.LastActivity );

            mergeFields.Add( "LastActivityLookup", lastActivityNotes );

            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "DetailPage", LinkedPageRoute( "DetailPage" ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            lContents.Text = contents.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}