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
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Event
{
    [DisplayName( "Registration Instance - Navigation" )]
    [Category( "Event" )]
    [Description( "Provides the navigation for the tabs navigation section of the Registration Instance Page/Layout" )]

    [LinkedPage( "Wait List Page",
        "The Page that shows the Wait List",
        Key = AttributeKey.WaitListPage,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE_WAIT_LIST,
        Order = 0 )]

    [LinkedPage( "Group Placement Tool Page",
        "The Page that shows Group Placements for the selected placement type",
        Key = AttributeKey.GroupPlacementToolPage,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE_PLACEMENT_GROUPS,
        Order = 1 )]

    [Rock.SystemGuid.BlockTypeGuid( "AF0740C9-BC60-434B-A360-EB70A7CEA108" )]
    public partial class RegistrationInstanceNavigation : RegistrationInstanceBlock, ISecondaryBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string WaitListPage = "WaitListPage";
            public const string GroupPlacementToolPage = "GroupPlacementToolPage";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            /// <summary>
            /// The Registration Instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";

            /// <summary>
            /// The Registration Template identifier.
            /// </summary>
            public const string RegistrationTemplateId = "RegistrationTemplateId";

            /// <summary>
            /// The registration template placement identifier
            /// </summary>
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";

            /// <summary>
            /// The registrant identifier
            /// </summary>
            public const string RegistrantId = "RegistrantId";
        }

        #endregion PageParameterKeys

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindPageList();
            }
            base.OnLoad( e );
        }

        /// <summary>
        /// Binds the page list.
        /// </summary>
        public void BindPageList()
        {
            if ( this.RegistrationInstance == null || this.RegistrationInstance.RegistrationTemplate == null )
            {
                return;
            }

            var waitListPageGuid = this.GetAttributeValue( AttributeKey.WaitListPage ).SplitDelimitedValues().FirstOrDefault().AsGuidOrNull();
            var groupPlacementToolPageGuid = this.GetAttributeValue( AttributeKey.GroupPlacementToolPage ).SplitDelimitedValues().FirstOrDefault().AsGuidOrNull();

            var showWaitListTab = this.RegistrationInstance.RegistrationTemplate.WaitListEnabled;

            var rockContext = new RockContext();
            var pageList = this.PageCache.ParentPage.GetPages( rockContext ).OrderBy( a => a.Order ).ToList();

            if ( !showWaitListTab && waitListPageGuid.HasValue )
            {
                pageList = pageList.Where( a => a.Guid != waitListPageGuid.Value ).ToList();
            }

            if ( groupPlacementToolPageGuid.HasValue )
            {
                pageList = pageList.Where( a => a.Guid != groupPlacementToolPageGuid.Value ).ToList();
            }

            var navigationPageInfoList = pageList
                .Where( a => a.DisplayInNavWhen != DisplayInNavWhen.Never )
                .Select( a => new NavigationPageInfo
                {
                    TabTitle = a.PageTitle,
                    PageReference = new PageReference( a.Id )
                } ).ToList();

            if ( groupPlacementToolPageGuid.HasValue )
            {
                var groupPlacementToolPageId = PageCache.GetId( groupPlacementToolPageGuid.Value );
                if ( groupPlacementToolPageId.HasValue )
                {
                    pageList = pageList.Where( a => a.Guid != groupPlacementToolPageGuid.Value ).ToList();
                    var registrationTemplatePlacements = this.RegistrationInstance.RegistrationTemplate.Placements.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
                    foreach ( var registrationTemplatePlacement in registrationTemplatePlacements )
                    {
                        var groupPlacementPageReference = new PageReference( groupPlacementToolPageId.Value );
                        groupPlacementPageReference.Parameters.Add( PageParameterKey.RegistrationTemplatePlacementId, registrationTemplatePlacement.Id.ToString() );
                        var navigationPageInfo = new NavigationPageInfo
                        {
                            PageReference = groupPlacementPageReference,
                            TabTitle = registrationTemplatePlacement.Name,
                            IsGroupPlacementPage = true,
                            RegistrationTemplatePlacementId = registrationTemplatePlacement.Id };

                        navigationPageInfoList.Add( navigationPageInfo );
                    }
                }
            }

            var currentPageParameters = this.PageParameters().Where( a =>
                a.Key != "PageId"
                && a.Key != PageParameterKey.RegistrationTemplatePlacementId
                && a.Key != PageParameterKey.RegistrantId ).ToList();

            foreach ( var navigationPageInfo in navigationPageInfoList )
            {
                foreach ( var pageParameter in currentPageParameters )
                {
                    navigationPageInfo.PageReference.Parameters.AddOrReplace( pageParameter.Key, pageParameter.Value.ToString() );
                }
            }

            rptPages.DataSource = navigationPageInfoList;
            rptPages.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPages_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var navigationPageInfo = e.Item.DataItem as NavigationPageInfo;
            if ( navigationPageInfo == null )
            {
                return;
            }

            var pageReference = navigationPageInfo.PageReference;

            var liNavigationTab = e.Item.FindControl( "liNavigationTab" ) as HtmlControl;
            if ( pageReference.PageId == this.PageCache.Id )
            {
                if ( navigationPageInfo.IsGroupPlacementPage )
                {
                    if ( navigationPageInfo.RegistrationTemplatePlacementId.HasValue && this.PageParameter( PageParameterKey.RegistrationTemplatePlacementId ).AsInteger() == navigationPageInfo.RegistrationTemplatePlacementId.Value )
                    {
                        liNavigationTab.AddCssClass( "active" );
                    }
                }
                else
                {
                    liNavigationTab.AddCssClass( "active" );
                }
            }

            var aPageLink = e.Item.FindControl( "aPageLink" ) as HtmlAnchor;

            aPageLink.HRef = pageReference.BuildUrl();
            aPageLink.InnerHtml = navigationPageInfo.TabTitle;

            var lPageName = e.Item.FindControl( "lPageName" ) as Literal;
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlView.Visible = visible;
        }

        /// <summary>
        /// 
        /// </summary>
        private class NavigationPageInfo
        {
            public string TabTitle { get; set; }

            public PageReference PageReference { get; set; }

            public bool IsGroupPlacementPage { get; set; }

            public int? RegistrationTemplatePlacementId { get; set; }
        }
    }
}