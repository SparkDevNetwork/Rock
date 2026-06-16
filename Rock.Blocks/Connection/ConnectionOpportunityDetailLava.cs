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

using System.Collections.Generic;
using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks.Connection.ConnectionOpportunityDetailLava;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Connection
{
    /// <summary>
    /// Displays the details of a connection opportunity for an external website using a Lava template.
    /// </summary>
    [DisplayName( "Connection Opportunity Detail Lava" )]
    [Category( "Connection" )]
    [Description( "Displays the details of the given opportunity for the external website." )]

    #region Block Attributes

    [BooleanField( "Set Page Title",
        Description = "Determines if the block should set the page title with the package name.",
        DefaultBooleanValue = false,
        Order = 0,
        Key = AttributeKey.SetPageTitle )]

    [LinkedPage( "Signup Page",
        Description = "The page used to sign up for an opportunity.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.SignupPage )]

    [CodeEditorField( "Lava Template",
        Description = "Lava template to use to display the package details.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/OpportunityDetail.lava' %}",
        Order = 2,
        Key = AttributeKey.LavaTemplate )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3F641636-C648-4998-97B7-650C08541E29" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "6ECC9759-CB27-4559-B4C9-E4D00935A831" )]
    [Rock.SystemGuid.BlockTypeGuid( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC" )]
    public class ConnectionOpportunityDetailLava : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SignupPage = "SignupPage";
            public const string LavaTemplate = "LavaTemplate";
            public const string SetPageTitle = "SetPageTitle";
        }

        private static class PageParameterKey
        {
            public const string OpportunityId = "OpportunityId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ConnectionOpportunityDetailLavaOptionsBag();
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            return RenderContent();
        }

        /// <summary>
        /// Resolves the configured Lava template for the requested opportunity into HTML.
        /// </summary>
        /// <returns>The rendered HTML, or an empty string when no opportunity is requested or found.</returns>
        private string RenderContent()
        {
            var connectionOpportunity = GetConnectionOpportunity( PageParameter( PageParameterKey.OpportunityId ) );

            if ( connectionOpportunity == null )
            {
                return string.Empty;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            // Provide the signup page route so the template can build a "Connect" link.
            var linkedPages = new Dictionary<string, object>
            {
                ["SignupPage"] = this.GetLinkedPageUrl( AttributeKey.SignupPage )
            };
            mergeFields.Add( "LinkedPages", linkedPages );

            // Provide the current campus context for templates that vary by campus.
            mergeFields.Add( "CampusContext", RequestContext.GetContextEntity<Campus>() );

            // Resolve any Lava embedded in the summary and description before exposing the opportunity.
            connectionOpportunity.Summary = connectionOpportunity.Summary.ResolveMergeFields( mergeFields );
            connectionOpportunity.Description = connectionOpportunity.Description.ResolveMergeFields( mergeFields );
            mergeFields.Add( "Opportunity", connectionOpportunity );

            if ( GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean() )
            {
                SetOpportunityPageTitle( connectionOpportunity.PublicName );
            }

            return GetAttributeValue( AttributeKey.LavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Sets the page and browser title to the supplied opportunity name.
        /// </summary>
        /// <param name="title">The public name of the opportunity.</param>
        private void SetOpportunityPageTitle( string title )
        {
            if ( title.IsNullOrWhiteSpace() )
            {
                return;
            }

            var siteName = PageCache?.Layout?.Site?.Name;

            RequestContext.Response.SetPageTitle( title );
            RequestContext.Response.SetBrowserTitle( siteName.IsNotNullOrWhiteSpace() ? $"{title} | {siteName}" : title );
        }

        /// <summary>
        /// Gets the connection opportunity identified by the supplied key (Id, IdKey, or Guid).
        /// </summary>
        /// <param name="opportunityKey">The opportunity identifier from the page parameter.</param>
        /// <returns>The matching opportunity, or <c>null</c> when the key is empty or unmatched.</returns>
        private ConnectionOpportunity GetConnectionOpportunity( string opportunityKey )
        {
            if ( opportunityKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new ConnectionOpportunityService( RockContext )
                .Get( opportunityKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<IBreadCrumb>();

            var connectionOpportunity = GetConnectionOpportunity( pageReference.GetPageParameter( PageParameterKey.OpportunityId ) );

            if ( connectionOpportunity != null )
            {
                breadCrumbs.Add( new BreadCrumbLink( connectionOpportunity.Name, pageReference ) );
            }

            return new BreadCrumbResult
            {
                BreadCrumbs = breadCrumbs
            };
        }

        #endregion Methods
    }
}
