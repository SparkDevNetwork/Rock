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
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Opportunity Detail Lava" )]
    [Category( "Connection" )]
    [Description( "Displays the details of the given opportunity for the external website." )]

    #region Block Attributes

    [LinkedPage(
        "Signup Page",
        Description = "The page used to sign up for an opportunity",
        Key = AttributeKey.SignupPage )]
    [CodeEditorField(
        "Lava Template",
        Description = "Lava template to use to display the package details.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/OpportunityDetail.lava' %}",
        Order = 2,
        Key = AttributeKey.LavaTemplate )]
    [BooleanField(
        "Set Page Title",
        Description = "Determines if the block should set the page title with the package name.",
        DefaultBooleanValue = false,
        Key = AttributeKey.SetPageTitle )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC" )]
    public partial class ConnectionOpportunityDetailLava : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string SignupPage = "SignupPage";
            public const string LavaTemplate = "LavaTemplate";
            public const string SetPageTitle = "SetPageTitle";
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
            if ( !IsPostBack )
            {
                LoadContent();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? opportunityId  = PageParameter( pageReference, "OpportunityId" ).AsIntegerOrNull();
            if ( opportunityId != null )
            {
                var connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( opportunityId.Value );
                if ( connectionOpportunity != null )
                {
                    breadCrumbs.Add( new BreadCrumb( connectionOpportunity.Name, pageReference ) );
                }
            }

            return breadCrumbs;
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        #endregion

        #region Internal Methods

        public void LoadContent()
        {
            int? opportunityId = PageParameter( "OpportunityId" ).AsIntegerOrNull();

            if ( !opportunityId.HasValue )
            {
                return;
            }

            var connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( opportunityId.Value );

            if ( connectionOpportunity != null )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                // Add a merge field for the Signup Page Route
                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "SignupPage", LinkedPageRoute( AttributeKey.SignupPage ) );
                mergeFields.Add( "LinkedPages", linkedPages );

                // Add Campus Context
                mergeFields.Add( "CampusContext", RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus );

                // Resolve any lava in the summary/description fields before adding the opportunity
                connectionOpportunity.Summary = connectionOpportunity.Summary.ResolveMergeFields( mergeFields );
                connectionOpportunity.Description = connectionOpportunity.Description.ResolveMergeFields( mergeFields );
                mergeFields.Add( "Opportunity", connectionOpportunity );

                // Format the opportunity using lava template
                lOutput.Text = GetAttributeValue( AttributeKey.LavaTemplate ).ResolveMergeFields( mergeFields );

                // Set the page title from opportunity (if configured)
                if ( GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean() )
                {
                    string pageTitle = connectionOpportunity.PublicName;
                    RockPage.PageTitle = pageTitle;
                    RockPage.BrowserTitle = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                    RockPage.Header.Title = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                }
            }
        }

        #endregion

    }
}