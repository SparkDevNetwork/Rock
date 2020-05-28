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
using Rock.Web.UI;
using System.Web.UI.HtmlControls;
using System.Data.Entity;

namespace RockWeb.Plugins.com_bemaservices.Support
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Security Audit" )]
    [Category( "BEMA Services > Support" )]
    [Description( "Report block to give an update on a site's security" )]
    [IntegerField( "Recommended Maximum Number of Rock Admins", "The recommended maximum number of Rock Admins to use for the audit.", true, 10, "", 0, AttributeKey.RecommendedMaximumAdminCount )]
    [IntegerField( "Recommended Maximum Number of Pages Allowing Unencrypted Traffic", "The recommended maximum number of pages that can allow unencrypted traffic.", true, 0, "", 1, AttributeKey.RecommendedUnencryptedPageCount )]
    public partial class SecurityAudit : RockBlock, ICustomGridColumns
    {
        #region Fields

        private decimal _percentComplete = 0;
        private int _passedChecks = 0;
        private int _totalChecks = 10;

        /* BEMA.Start */
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string RecommendedMaximumAdminCount = "RecommendedMaximumAdminCount";
            public const string RecommendedUnencryptedPageCount = "RecommendedUnencryptedPageCount";
        }

        #endregion
        /* BEMA.End */
        #endregion

        #region Properties

        public decimal PercentComplete
        {
            get
            {
                return _percentComplete;
            }

            set
            {
                _percentComplete = value;
            }
        }

        public int PassedChecks
        {
            get
            {
                return _passedChecks;
            }

            set
            {
                _passedChecks = value;
            }
        }

        public int TotalChecks
        {
            get
            {
                return _totalChecks;
            }

            set
            {
                _totalChecks = value;
            }
        }

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gRockAdmins.GridRebind += gRockAdmins_GridRebind;
            gSslEnabled.GridRebind += gSslEnabled_GridRebind;

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
                ShowDetail();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();

        }

        #endregion

        #region Generic Methods

        private void ShowDetail()
        {
            var rockContext = new RockContext();

            BuildRockAdminHeader( rockContext );
            BindRockAdminGrid( rockContext );

            BuildSslEnabledHeader( rockContext );
            BindSslEnabledGrid( rockContext );

            PercentComplete = ( PassedChecks.ToString().AsDecimal() / TotalChecks.ToString().AsDecimal() ) * 100.0m;

            lChecksPassed.Text = String.Format( "{0}/{1} Checks Passed", PassedChecks.ToString(), TotalChecks.ToString() );
            divProgressBar.InnerHtml = String.Format( @"
            <div  class='progress-bar' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' style='width: {0}%;'>
                <span id='sProgressBar' runat='server' class='sr-only'>{0}% Complete</span>
            </div>", PercentComplete );
        }

        private void GenerateHeader( string headerText, string descriptionText, int auditValue, int auditGoal, bool showWarning, HtmlGenericControl targetHeader, Literal targetLiteral )
        {
            var headerCss = "";

            if ( auditValue <= auditGoal )
            {
                PassedChecks++;
                if ( auditValue == auditGoal && showWarning )
                {
                    headerCss = "header-warning";

                }
                else
                {
                    headerCss = "header-success";
                }
            }
            else
            {
                headerCss = "header-danger";
            }
            targetHeader.InnerHtml = String.Format( "<h5 class='mb-0'>{0}</h5>", headerText );
            targetLiteral.Text = descriptionText;
            targetHeader.AddCssClass( headerCss );
        }

        #endregion

        #region Rock Admin Methods

        private void BuildRockAdminHeader( RockContext rockContext )
        {
            var auditValue = new GroupService( rockContext ).Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Members.Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumAdminCount ).AsInteger();

            var headerText = String.Format( "Number of Rock Admins: {0}", auditValue );
            var descriptionText = String.Format( "Limiting the number of people who have access to modify all of Rock is crucial to improving your organization's security. BEMA recommends having no more than {0} members of the Rock Administration Group.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderRockAdmin, lDescriptionRockAdmin );
        }

        private void gRockAdmins_GridRebind( object sender, EventArgs e )
        {
            BindRockAdminGrid();
        }

        private void BindRockAdminGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            var administratorGroupGuid = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();
            // sample query to display a few people
            var qry = groupMemberService.Queryable()
                        .Where( gm => gm.Group.Guid == administratorGroupGuid &&
                                gm.GroupMemberStatus == GroupMemberStatus.Active )
                        .ToList();

            gRockAdmins.DataSource = qry.ToList();
            gRockAdmins.DataBind();
        }

        #endregion

        #region SSL Enabled Methods

        private void BuildSslEnabledHeader( RockContext rockContext )
        {
            var auditValue = new PageService( rockContext ).Queryable().AsNoTracking().Where( p => !p.RequiresEncryption && !p.Layout.Site.RequiresEncryption ).Count();
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedUnencryptedPageCount ).AsInteger();

            var headerText = String.Format( "Pages Allowing Unencrypted Traffic: {0}", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to require SSL Encryption. BEMA recommends allowing no more than {0} pages to be accessed on an unencrypted connection.", auditValue );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderSslEnabled, lDescriptionSslEnabled );
        }

        private void gSslEnabled_GridRebind( object sender, EventArgs e )
        {
            BindRockAdminGrid();
        }

        private void BindSslEnabledGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var pageService = new PageService( rockContext );

            var administratorGroupGuid = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();
            // sample query to display a few people
            var qry = pageService.Queryable()
                        .AsNoTracking().Where( p => !p.RequiresEncryption && !p.Layout.Site.RequiresEncryption )
                        .ToList();

            gSslEnabled.DataSource = qry.ToList();
            gSslEnabled.DataBind();
        }

        #endregion
    }
}