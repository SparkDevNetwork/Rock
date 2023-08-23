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

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Description = "Page used to view details of a request.",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL,
        Order = 1,
        Key = AttributeKey.DetailPage )]

    [ConnectionTypesField(
        "Include Connection Types",
        Description = "Optional list of connection types to include in the display to (All will be displayed by default).",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.IncludedConnectionTypes )]

    [ConnectionTypesField(
        "Exclude Connection Types",
        Description = "Optional list of connection types to exclude from the display to (None will be excluded by default).",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.ExcludedConnectionTypes )]

    [CodeEditorField(
        "Contents",
        Description = @"The Lava template to use for displaying connection opportunities assigned to current user.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/MyConnectionOpportunitiesSortable.lava' %}",
        Key = AttributeKey.Contents,
        Order = 4 )]
    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "1B8E50A0-7AC4-475F-857C-50D0809A3F04" )]
    public partial class MyConnectionOpportunitiesLava : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string IncludedConnectionTypes = "ConnectionTypes";
            public const string DetailPage = "DetailPage";
            public const string Contents = "Contents";
            public const string ExcludedConnectionTypes = "ExcludedConnectionTypes";
        }
        #endregion Attribute Keys

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
            string contents = GetAttributeValue( AttributeKey.Contents );

            string appRoot = ResolveRockUrl( "~/" );
            string themeRoot = ResolveRockUrl( "~~/" );
            contents = contents.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            var includedConnectionTypeGuids = GetAttributeValue( AttributeKey.IncludedConnectionTypes ).SplitDelimitedValues().AsGuidList();
            var excludedConnectionTypeGuids = GetAttributeValue( AttributeKey.ExcludedConnectionTypes ).SplitDelimitedValues().AsGuidList();

            DateTime midnightToday = RockDateTime.Today.AddDays( 1 );

            var rockContext = new RockContext();
            var connectionRequests = new ConnectionRequestService( rockContext ).Queryable()
                .Where( a => a.ConnectorPersonAlias != null && a.ConnectorPersonAlias.PersonId == CurrentPersonId )
                .Where( r => r.ConnectionState == ConnectionState.Active ||
                                    ( r.ConnectionState == ConnectionState.FutureFollowUp && r.FollowupDate.HasValue && r.FollowupDate.Value < midnightToday ) );

            if ( includedConnectionTypeGuids.Any() )
            {
                connectionRequests = connectionRequests.Where( a => includedConnectionTypeGuids.Contains( a.ConnectionOpportunity.ConnectionType.Guid ) );
            }

            if ( excludedConnectionTypeGuids.Any() )
            {
                connectionRequests = connectionRequests.Where( a => !excludedConnectionTypeGuids.Contains( a.ConnectionOpportunity.ConnectionType.Guid ) );
            }

            connectionRequests = connectionRequests.OrderBy( r => r.PersonAlias.Person.LastName ).ThenBy( r => r.PersonAlias.Person.NickName );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );
            mergeFields.Add( "ConnectionRequests", connectionRequests.ToList() );

            var lastActivityNotes = connectionRequests.Select( r => new
            {
                ConnectionRequestId = r.Id,
                LastActivity = r.ConnectionRequestActivities.OrderByDescending(
                            a => a.CreatedDateTime ).FirstOrDefault()
            } ).ToDictionary( k => k.ConnectionRequestId, v => v.LastActivity );

            mergeFields.Add( "LastActivityLookup", lastActivityNotes );

            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "DetailPage", LinkedPageRoute( AttributeKey.DetailPage ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            lContents.Text = contents.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}