// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Data;
using System.Text.RegularExpressions;
using Rock.Security;

namespace RockWeb.Plugins.com_bemaservices.CustomBlocks.Bema.Reporting
{
    [DisplayName( "Security Audit" )]
    [Category( "BEMA Services > Reporting" )]
    [Description( "Report block to give an update on a site's security" )]

    // Rock Admin Count Settings
    [IntegerField( "Recommended Maximum Number of Rock Admins", "The recommended maximum number of Rock Admins to use for the audit.", true, 10, "Rock Admin Count Settings", 0, AttributeKey.RecommendedMaximumAdminCount )]

    // Unencrypted Traffic Settings
    [IntegerField( "Recommended Maximum Number of Pages Allowing Unencrypted Traffic", "The recommended maximum number of pages that can allow unencrypted traffic.", true, 0, "Unencrypted Traffic Settings", 1, AttributeKey.RecommendedUnencryptedPageCount )]

    // Non Staff Members Settings
    [IntegerField( "Recommended Maximum Number of Non-Staff Members of Security Roles", "The recommended maximum number of non-staff members of security roles.", true, 5, "Non Staff Members Settings", 2, AttributeKey.RecommendedNonStaffMembers )]
    [CustomCheckboxListField( "Excluded Security Roles", "Security roles to exclude from the non-staff audit", "Select Id as Value, Name as Text from [Group] where IsSecurityRole = 1 Order By Name", false, "", "Non Staff Members Settings", 3, AttributeKey.ExcludedSecurityRoles )]

    // Sql Page Parameter Settings
    [IntegerField( "Recommended Maximum Number of blocks using page parameters in their sql", "The recommended maximum number of blocks using page parameters in their sql.", true, 5, "Sql Page Parameter Settings", 4, AttributeKey.RecommendedSqlPageParameterBlocks )]
    [CustomCheckboxListField( "Sites", "Sites to include in the Sql Page Parameter Audit.", "Select Id as Value, Name as Text from [Site]", false, "3", "Sql Page Parameter Settings", 5, AttributeKey.SqlPageParameterSites )]

    // Sql Lava Command Settings
    [IntegerField( "Recommended Maximum Number of blocks using sql lava commands", "The recommended maximum number of blocks using sql lava commands.", true, 5, " Sql Lava Command Settings", 6, AttributeKey.RecommendedSqlLavaCommandBlocks )]
    [CustomCheckboxListField( "Sites", "Sites to include in the Sql Lava Command Audit.", "Select Id as Value, Name as Text from [Site]", false, "3", " Sql Lava Command Settings", 7, AttributeKey.SqlLavaCommandSites )]

    // Person Auth Settings
    [IntegerField( "Recommended Maximum Number of Person Authorizations", "The recommended maximum number of person-specific auth rules to use for the audit.", true, 4, "Person Auth Settings", 8, AttributeKey.RecommendedMaximumPersonAuthCount )]

    // Finance Data Views Settings
    [IntegerField( "Recommended Maximum Number of Unsecured Finance Data Views", "The recommended maximum number of unsecured finance data views to use for the audit.", true, 4, "Finance Data View Settings", 9, AttributeKey.RecommendedMaximumUnsecuredFinanceDataViewCount )]

    // Finance Pages Settings
    [IntegerField( "Recommended Maximum Number of Unsecured Finance Pages", "The recommended maximum number of unsecured finance pages to use for the audit.", true, 0, "Finance Page Settings", 10, AttributeKey.RecommendedMaximumUnsecuredFinancePageCount )]

    // Admin Pages Settings
    [IntegerField( "Recommended Maximum Number of Unsecured Admin Pages", "The recommended maximum number of unsecured admin pages to use for the audit.", true, 6, "Admin Page Settings", 11, AttributeKey.RecommendedMaximumUnsecuredAdminPageCount )]

    // Sensitive File Type Settings
    [BinaryFileTypesField( "Sensitive File Types", "The file types that should not be accessible to all users.", true, "", "Sensitive File Type Settings", 12, AttributeKey.SensitiveFileTypes )]

    public partial class SecurityAudit : RockBlock
    {
        #region Fields

        private decimal _percentComplete = 0;
        private int _passedChecks = 0;
        private int _totalChecks = 0;

        private static class AttributeKey
        {
            public const string RecommendedMaximumAdminCount = "RecommendedMaximumAdminCount";
            public const string RecommendedUnencryptedPageCount = "RecommendedUnencryptedPageCount";
            public const string RecommendedNonStaffMembers = "RecommendedNonStaffMembers";
            public const string ExcludedSecurityRoles = "ExcludedSecurityRoles";
            public const string RecommendedSqlPageParameterBlocks = "RecommendedSqlPageParameterBlocks";
            public const string SqlPageParameterSites = "SqlPageParameterSites";
            public const string RecommendedSqlLavaCommandBlocks = "RecommendedSqlLavaCommandBlocks";
            public const string SqlLavaCommandSites = "SqlLavaCommandSites";
            public const string RecommendedMaximumPersonAuthCount = "RecommendedMaximumPersonAuthCount";
            public const string RecommendedMaximumUnsecuredFinanceDataViewCount = "RecommendedMaximumUnsecuredFinanceDataViewCount";
            public const string RecommendedMaximumUnsecuredFinancePageCount = "RecommendedMaximumUnsecuredFinancePageCount";
            public const string RecommendedMaximumUnsecuredAdminPageCount = "RecommendedMaximumUnsecuredAdminPageCount";
            public const string SensitiveFileTypes = "SensitiveFileTypes";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the percent complete.
        /// </summary>
        /// <value>
        /// The percent complete.
        /// </value>
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

        /// <summary>
        /// Gets or sets the passed checks.
        /// </summary>
        /// <value>
        /// The passed checks.
        /// </value>
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

        /// <summary>
        /// Gets or sets the total checks.
        /// </summary>
        /// <value>
        /// The total checks.
        /// </value>
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
            gNonStaff.GridRebind += gNonStaff_GridRebind;
            gPageParameterSql.GridRebind += gPageParameterSql_GridRebind;
            gSqlLavaCommand.GridRebind += gSqlLavaCommand_GridRebind;
            gPersonAuth.GridRebind += gPersonAuth_GridRebind;
            gUnencryptedSensitiveData.GridRebind += gUnencryptedSensitiveData_GridRebind;
            gFinanceDataViews.GridRebind += gFinanceDataViews_GridRebind;
            gFileTypeSecurity.GridRebind += gFileTypeSecurity_GridRebind;
            gGlobalLavaCommands.GridRebind += gGlobalLavaCommands_GridRebind;
            gFinancePages.GridRebind += gFinancePages_GridRebind;
            gAdminPages.GridRebind += gAdminPages_GridRebind;

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

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            var rockContext = new RockContext();

            BuildRockAdminHeader( rockContext );
            BindRockAdminGrid( rockContext );

            BuildSslEnabledHeader( rockContext );
            BindSslEnabledGrid( rockContext );

            BuildNonStaffHeader( rockContext );
            BindNonStaffGrid( rockContext );

            BuildPageParameterSqlHeader( rockContext );
            BindPageParameterSqlGrid( rockContext );

            BuildSqlLavaCommandHeader( rockContext );
            BindSqlLavaCommandGrid( rockContext );

            BuildPersonAuthHeader( rockContext );
            BindPersonAuthGrid( rockContext );

            BuildUnencryptedSensitiveDataHeader( rockContext );
            BindUnencryptedSensitiveDataGrid( rockContext );

            BuildFinanceDataViewsHeader( rockContext );
            BindFinanceDataViewsGrid( rockContext );

            BuildFinancePagesHeader( rockContext );
            BindFinancePagesGrid( rockContext );

            BuildAdminPagesHeader( rockContext );
            BindAdminPagesGrid( rockContext );

            BuildFileTypeSecurityHeader( rockContext );
            BindFileTypeSecurityGrid( rockContext );

            BuildGlobalLavaCommandsHeader( rockContext );
            BindGlobalLavaCommandsGrid( rockContext );

            PercentComplete = ( PassedChecks.ToString().AsDecimal() / TotalChecks.ToString().AsDecimal() ) * 100.0m;

            lChecksPassed.Text = String.Format( "{0}/{1} Checks Passed", PassedChecks.ToString(), TotalChecks.ToString() );
            divProgressBar.InnerHtml = String.Format( @"
            <div  class='progress-bar' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' style='width: {0}%;'>
                <span id='sProgressBar' runat='server' class='sr-only'>{0}% Complete</span>
            </div>", PercentComplete );
        }

        /// <summary>
        /// Generates the header.
        /// </summary>
        /// <param name="headerText">The header text.</param>
        /// <param name="descriptionText">The description text.</param>
        /// <param name="auditValue">The audit value.</param>
        /// <param name="auditGoal">The audit goal.</param>
        /// <param name="showWarning">if set to <c>true</c> [show warning].</param>
        /// <param name="targetHeader">The target header.</param>
        /// <param name="targetLiteral">The target literal.</param>
        private void GenerateHeader( string headerText, string descriptionText, int auditValue, int auditGoal, bool showWarning, HtmlGenericControl targetHeader, Literal targetLiteral )
        {
            var headerCss = "";
            TotalChecks++;
            if ( auditValue <= auditGoal )
            {
                PassedChecks++;
                headerCss = "header-success";
            }
            else
            {
                if ( showWarning )
                {
                    headerCss = "header-warning";
                }
                else
                {
                    headerCss = "header-danger";
                }
            }

            targetHeader.InnerHtml = String.Format( "<h5 class='mb-0'>{0}<span class=\"pull-right\"><i class=\"fa fa-chevron-down\"></i></span></h5>", headerText );
            targetLiteral.Text = descriptionText;
            targetHeader.AddCssClass( headerCss );
        }

        /// <summary>
        /// Adds the parent rules.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        /// <param name="itemRules">The item rules.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="action">The action.</param>
        /// <param name="recurse">if set to <c>true</c> [recurse].</param>
        private void AddParentRules( AuthService authService, List<AuthRule> itemRules, ISecured parent, string action, bool recurse )
        {
            if ( parent != null )
            {
                var entityType = EntityTypeCache.Get( parent.TypeId );
                foreach ( var auth in authService.GetAuths( parent.TypeId, parent.Id, action ) )
                {
                    var rule = new AuthRule( auth );

                    if ( !itemRules.Exists( r =>
                            r.SpecialRole == rule.SpecialRole &&
                            r.PersonId == rule.PersonId &&
                            r.GroupId == rule.GroupId ) )
                    {
                        itemRules.Add( rule );
                    }
                }

                if ( recurse )
                {
                    AddParentRules( authService, itemRules, parent.ParentAuthority, action, true );
                }
            }
        }

        /// <summary>
        /// Checks the item security.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="isecuredIdList">The isecured identifier list.</param>
        /// <param name="authorizedGroupIds">The authorized group ids.</param>
        /// <param name="iSecuredEntity">The i secured entity.</param>
        private void CheckItemSecurity( RockContext rockContext, List<int> isecuredIdList, List<int> authorizedGroupIds, ISecured iSecuredEntity )
        {
            var authService = new AuthService( rockContext );

            var itemRules = new List<AuthRule>();
            foreach ( var auth in authService.GetAuths( iSecuredEntity.TypeId, iSecuredEntity.Id, Authorization.VIEW ) )
            {
                itemRules.Add( new AuthRule( auth ) );
            }

            AddParentRules( authService, itemRules, iSecuredEntity.ParentAuthorityPre, Authorization.VIEW, false );
            AddParentRules( authService, itemRules, iSecuredEntity.ParentAuthority, Authorization.VIEW, true );

            var authRules = Authorization.AuthRules( iSecuredEntity.TypeId, iSecuredEntity.Id, Authorization.VIEW );
            if ( !itemRules.Where( a => a.AllowOrDeny == 'D' && a.SpecialRole == SpecialRole.AllUsers ).Any() )
            {
                isecuredIdList.Add( iSecuredEntity.Id );
            }
            else
            {
                var denyRule = itemRules.Where( a => a.AllowOrDeny == 'D' && a.SpecialRole == SpecialRole.AllUsers ).FirstOrDefault();
                foreach ( var authRule in itemRules.Where( a => a.AllowOrDeny == 'A' && ( !a.GroupId.HasValue || !authorizedGroupIds.Contains( a.GroupId.Value ) ) ).ToList() )
                {
                    if ( itemRules.IndexOf( denyRule ) > itemRules.IndexOf( authRule ) )
                    {
                        isecuredIdList.Add( iSecuredEntity.Id );
                    }
                }
            }
        }

        #endregion

        #region Rock Admin Methods

        /// <summary>
        /// Builds the rock admin header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildRockAdminHeader( RockContext rockContext )
        {
            var auditValue = GetRockAdminData( rockContext ).Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumAdminCount ).AsInteger();

            var headerText = String.Format( "Number of Rock Admins: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "Limiting the number of people who have access to modify all of Rock is crucial to improving your organization's security. BEMA recommends having no more than {0} members of the Rock Administration Group.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderRockAdmin, lDescriptionRockAdmin );
        }

        /// <summary>
        /// Handles the GridRebind event of the gRockAdmins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gRockAdmins_GridRebind( object sender, EventArgs e )
        {
            BindRockAdminGrid();
        }

        /// <summary>
        /// Binds the rock admin grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindRockAdminGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<GroupMember> qry = GetRockAdminData( rockContext );
            gRockAdmins.DataSource = qry.ToList();
            gRockAdmins.DataBind();
        }

        /// <summary>
        /// Gets the rock admin data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<GroupMember> GetRockAdminData( RockContext rockContext )
        {
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            var administratorGroupGuid = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();
            var qry = groupMemberService.Queryable()
                        .Where( gm => gm.Group.Guid == administratorGroupGuid &&
                                gm.GroupMemberStatus == GroupMemberStatus.Active )
                        .ToList();
            return qry;
        }

        #endregion

        #region SSL Enabled Methods

        /// <summary>
        /// Builds the SSL enabled header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildSslEnabledHeader( RockContext rockContext )
        {
            var auditValue = GetSslEnabledData( rockContext ).Count();
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedUnencryptedPageCount ).AsInteger();

            var headerText = String.Format( "Pages Allowing Unencrypted Traffic: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to require SSL Encryption. BEMA recommends allowing no more than {0} pages to be accessed on an unencrypted connection.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderSslEnabled, lDescriptionSslEnabled );
        }

        /// <summary>
        /// Handles the GridRebind event of the gSslEnabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gSslEnabled_GridRebind( object sender, EventArgs e )
        {
            BindSslEnabledGrid();
        }

        /// <summary>
        /// Binds the SSL enabled grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindSslEnabledGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<Rock.Model.Page> qry = GetSslEnabledData( rockContext );

            gSslEnabled.DataSource = qry.ToList();
            gSslEnabled.DataBind();
        }

        /// <summary>
        /// Gets the SSL enabled data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<Rock.Model.Page> GetSslEnabledData( RockContext rockContext )
        {
            var pageService = new PageService( rockContext );

            var administratorGroupGuid = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();
            var qry = pageService.Queryable()
                        .AsNoTracking().Where( p => !p.RequiresEncryption && !p.Layout.Site.RequiresEncryption )
                        .ToList();
            return qry;
        }

        #endregion

        #region Non Staff Methods

        /// <summary>
        /// Builds the non staff header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildNonStaffHeader( RockContext rockContext )
        {
            var auditValue = GetNonStaffData( rockContext ).Count();
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedNonStaffMembers ).AsInteger();

            var headerText = String.Format( "Non-Staff Security Role Members: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "BEMA recommends limiting the number of people in security roles that are not in your organization. We recommend no more than {0} people be in a security role that are not part of your organization in some way.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderNonStaff, lDescriptionNonStaff );
        }

        /// <summary>
        /// Handles the GridRebind event of the gNonStaff control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gNonStaff_GridRebind( object sender, EventArgs e )
        {
            BindNonStaffGrid();
        }

        /// <summary>
        /// Binds the non staff grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindNonStaffGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<GroupMember> qry = GetNonStaffData( rockContext );

            gNonStaff.DataSource = qry.ToList();
            gNonStaff.DataBind();
        }

        /// <summary>
        /// Gets the non staff data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<GroupMember> GetNonStaffData( RockContext rockContext )
        {
            var pageService = new PageService( rockContext );

            var organizationUnitGuid = Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT.AsGuid();
            var excludedRoleIds = GetAttributeValue( AttributeKey.ExcludedSecurityRoles ).SplitDelimitedValues().AsIntegerList();

            var qry = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                .Where( gm =>
                    gm.Person.Members.Where( gm1 => gm1.Group.GroupType.Guid == organizationUnitGuid && gm1.GroupMemberStatus == GroupMemberStatus.Active ).Count() == 0 &&
                    gm.Group.IsSecurityRole &&
                    !excludedRoleIds.Contains( gm.GroupId ) &&
                    gm.Group.IsActive
                )
                .OrderBy( gm => gm.Person.LastName ).ThenBy( gm => gm.Person.FirstName ).ThenBy( gm => gm.Group.Name )
                .ToList();
            return qry;
        }

        #endregion

        #region PageParameterSql Methods

        /// <summary>
        /// Builds the page parameter SQL header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildPageParameterSqlHeader( RockContext rockContext )
        {
            var auditValue = GetPageParameterSqlData().Rows.Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedSqlPageParameterBlocks ).AsInteger();

            var headerText = String.Format( "Blocks Using Page Parameters in SQL Queries: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit the capability for Sql injection attacks. BEMA recommends allowing no more than {0} blocks to use page parameters in their sql queries.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderPageParameterSql, lDescriptionPageParameterSql );
        }

        /// <summary>
        /// Handles the GridRebind event of the gPageParameterSql control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPageParameterSql_GridRebind( object sender, EventArgs e )
        {
            BindPageParameterSqlGrid();
        }

        /// <summary>
        /// Binds the page parameter SQL grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindPageParameterSqlGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            DataTable data = GetPageParameterSqlData();

            gPageParameterSql.DataSource = data;
            gPageParameterSql.DataBind();
        }

        /// <summary>
        /// Gets the page parameter SQL data.
        /// </summary>
        /// <returns></returns>
        private DataTable GetPageParameterSqlData()
        {
            var siteList = GetAttributeValue( AttributeKey.SqlPageParameterSites );
            var query = @"Select distinct p.Id as PageId, p.PageTitle, b.Id as BlockId, b.Name
                            From AttributeValue av
                            Join Attribute a on a.Id = av.AttributeId and a.Name Like '%SQL%' or a.Name like '%Query%' and a.EntityTypeQualifierColumn = 'BlockTypeId'
                            Join Block b on b.Id = av.EntityId
                            Join Page p on b.PageId = p.Id
                            Join Layout l on p.LayoutId = l.Id
                            Where av.Value like '%{%PageParameter%}%'
                            And l.SiteId in (" + siteList + ")";
            DataSet dataSet = DbService.GetDataSet( query, CommandType.Text, null, GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180 );
            var data = dataSet.Tables[0];
            return data;
        }

        #endregion

        #region SqlLavaCommand Methods

        /// <summary>
        /// Builds the SQL lava command header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildSqlLavaCommandHeader( RockContext rockContext )
        {
            var auditValue = GetSqlLavaCommandData().Rows.Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedSqlLavaCommandBlocks ).AsInteger();

            var headerText = String.Format( "Blocks Using SQL Lava Commands: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit the capability for Sql injection attacks. BEMA recommends allowing no more than {0} blocks to use sql commands in their lava.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderSqlLavaCommand, lDescriptionSqlLavaCommand );
        }

        /// <summary>
        /// Handles the GridRebind event of the gSqlLavaCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gSqlLavaCommand_GridRebind( object sender, EventArgs e )
        {
            BindSqlLavaCommandGrid();
        }

        /// <summary>
        /// Binds the SQL lava command grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindSqlLavaCommandGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var data = GetSqlLavaCommandData();

            gSqlLavaCommand.DataSource = data;
            gSqlLavaCommand.DataBind();
        }

        /// <summary>
        /// Gets the SQL lava command data.
        /// </summary>
        /// <returns></returns>
        private DataTable GetSqlLavaCommandData()
        {
            var siteList = GetAttributeValue( AttributeKey.SqlPageParameterSites );
            var query = @"Declare @BlockAttributes table(
	                            AttributeId int
	                            )
                            Insert into @BlockAttributes
                            Select Id From Attribute Where EntityTypeQualifierColumn = 'BlockTypeId'

                            Select distinct p.Id as PageId, p.PageTitle, b.Id as BlockId, b.Name
                            From Block b
                            Join Page p on b.PageId = p.Id
                            Join Layout l on p.LayoutId = l.Id
                            Left Join HtmlContent hc on hc.BlockId = b.Id and (hc.ExpireDateTime is null or hc.ExpireDateTime > GetDate())
                            Left Join AttributeValue av on av.EntityId = b.Id and av.AttributeId in (Select AttributeId from @BlockAttributes) and av.Value like '%{%sql%}%'
                            Where hc.Content like '%{%sql%}%' or b.PreHtml like '%{%sql%}%' or b.PostHtml like '%{%sql%}%' or av.Value like '%{%sql%}%'
                            And l.SiteId in (" + siteList + ")";
            DataSet dataSet = DbService.GetDataSet( query, CommandType.Text, null, GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180 );
            var data = dataSet.Tables[0];
            return data;
        }

        #endregion

        #region Person Auth Methods

        /// <summary>
        /// Builds the person authentication header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildPersonAuthHeader( RockContext rockContext )
        {
            var auditValue = GetPersonAuthData( rockContext ).Count();
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumPersonAuthCount ).AsInteger();

            var headerText = String.Format( "Number of Person-Specific Auth Rules: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "Limiting the number of person-specific auth rules and relying on group-based auth rules is crucial to improving your organization's security. BEMA recommends having no more than {0} person-specific auth rules set on your database.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderPersonAuth, lDescriptionPersonAuth );
        }

        /// <summary>
        /// Handles the GridRebind event of the gPersonAuth control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPersonAuth_GridRebind( object sender, EventArgs e )
        {
            BindPersonAuthGrid();
        }

        /// <summary>
        /// Binds the person authentication grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindPersonAuthGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<Auth> qry = GetPersonAuthData( rockContext );

            gPersonAuth.DataSource = qry.ToList();
            gPersonAuth.DataBind();
        }

        /// <summary>
        /// Gets the person authentication data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<Auth> GetPersonAuthData( RockContext rockContext )
        {
            AuthService authService = new AuthService( rockContext );
            var qry = authService.Queryable().AsNoTracking().Where( a => a.PersonAliasId != null )
                        .ToList();
            return qry;
        }

        #endregion

        #region Unencrypted Sensitive Data Methods

        /// <summary>
        /// Builds the unencrypted sensitive data header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildUnencryptedSensitiveDataHeader( RockContext rockContext )
        {
            var auditValue = GetUnencryptedSensitiveDataData().Rows.Count;
            var auditGoal = 0;

            var headerText = String.Format( "Unencrypted Sensitive Attributes: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "Below is a list of all attributes containing sensitive data that are not currently encrypted. Currently this only includes Social Security Numbers. BEMA recommends using the encrypted text field type for all attributes containing sensitive data." );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderUnencryptedSensitiveData, lDescriptionUnencryptedSensitiveData );
        }

        /// <summary>
        /// Handles the GridRebind event of the gUnencryptedSensitiveData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gUnencryptedSensitiveData_GridRebind( object sender, EventArgs e )
        {
            BindUnencryptedSensitiveDataGrid();
        }

        /// <summary>
        /// Binds the unencrypted sensitive data grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindUnencryptedSensitiveDataGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            DataTable data = GetUnencryptedSensitiveDataData();

            gUnencryptedSensitiveData.DataSource = data;
            gUnencryptedSensitiveData.DataBind();
        }

        /// <summary>
        /// Gets the unencrypted sensitive data data.
        /// </summary>
        /// <returns></returns>
        private DataTable GetUnencryptedSensitiveDataData()
        {
            var query = @"Select a.Id, a.Name, et.Name as EntityType, a.EntityTypeQualifierColumn, a.EntityTypeQualifierValue, count(0) as SensitiveRecords
                            From AttributeValue av
                            Join Attribute a on a.Id = av.AttributeId
                            Join EntityType et on a.EntityTypeId = et.Id
                            Where av.Value like '[0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9][0-9][0-9]'
                            or av.Value like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                            or av.Value like '[0-9][0-9][0-9] [0-9][0-9] [0-9][0-9][0-9][0-9]'
                            Group By a.Id, a.Name, et.Name, a.EntityTypeQualifierColumn, a.EntityTypeQualifierValue";
            DataSet dataSet = DbService.GetDataSet( query, CommandType.Text, null, GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180 );
            var data = dataSet.Tables[0];
            return data;
        }

        #endregion

        #region Finance Data View Methods

        /// <summary>
        /// Builds the finance data views header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildFinanceDataViewsHeader( RockContext rockContext )
        {
            var auditValue = GetFinanceDataViewsData( rockContext ).Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumUnsecuredFinanceDataViewCount ).AsInteger();

            var headerText = String.Format( "Unsecured Finance Data Views: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit access to financial information. BEMA recommends allowing no more than {0} unsecured financial data views.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderFinanceDataViews, lDescriptionFinanceDataViews );
        }

        /// <summary>
        /// Handles the GridRebind event of the gFinanceDataViews control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gFinanceDataViews_GridRebind( object sender, EventArgs e )
        {
            BindFinanceDataViewsGrid();
        }

        /// <summary>
        /// Binds the finance data views grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindFinanceDataViewsGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<Rock.Model.DataView> unsecuredDataViews = GetFinanceDataViewsData( rockContext );

            gFinanceDataViews.DataSource = unsecuredDataViews;
            gFinanceDataViews.DataBind();
        }

        /// <summary>
        /// Gets the finance data views data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<Rock.Model.DataView> GetFinanceDataViewsData( RockContext rockContext )
        {
            var dataViewService = new DataViewService( rockContext );

            List<int> unsecuredDataViewIds = new List<int>();
            List<int> authorizedGroupIds = new List<int>();

            var groupService = new GroupService( rockContext );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_FINANCE_USERS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_BENEVOLENCE.AsGuid() ).Id );

            var dataViews = dataViewService.Queryable().AsNoTracking().Where( dv => dv.EntityType.Name.Contains( "Financ" ) ).ToList();
            foreach ( var dataView in dataViews )
            {
                CheckItemSecurity( rockContext, unsecuredDataViewIds, authorizedGroupIds, dataView );
            }

            var unsecuredDataViews = dataViewService.GetByIds( unsecuredDataViewIds ).ToList();
            return unsecuredDataViews;
        }

        #endregion

        #region FileTypeSecurity Methods

        /// <summary>
        /// Builds the file type security header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildFileTypeSecurityHeader( RockContext rockContext )
        {
            var auditValue = GetFileTypeSecurityData( rockContext ).Count;
            var auditGoal = 0;

            var headerText = String.Format( "Unsecured Sensitive File Types: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to secure file types dealing with sensitive data. BEMA recommends locking down all sensitive file types.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderFileTypeSecurity, lDescriptionFileTypeSecurity );
        }

        /// <summary>
        /// Handles the GridRebind event of the gFileTypeSecurity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gFileTypeSecurity_GridRebind( object sender, EventArgs e )
        {
            BindFileTypeSecurityGrid();
        }

        /// <summary>
        /// Binds the file type security grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindFileTypeSecurityGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<BinaryFileType> atRiskFileTypes = GetFileTypeSecurityData( rockContext );

            gFileTypeSecurity.DataSource = atRiskFileTypes;
            gFileTypeSecurity.DataBind();
        }

        /// <summary>
        /// Gets the file type security data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<BinaryFileType> GetFileTypeSecurityData( RockContext rockContext )
        {
            List<BinaryFileType> atRiskFileTypes = new List<BinaryFileType>();
            var selectedFileTypes = new BinaryFileTypeService( rockContext ).GetByGuids( GetAttributeValue( AttributeKey.SensitiveFileTypes ).SplitDelimitedValues().AsGuidList() );
            foreach ( var fileType in selectedFileTypes )
            {
                var authRules = Authorization.AuthRules( fileType.TypeId, fileType.Id, Authorization.VIEW );
                if ( !authRules.Where( a => a.AllowOrDeny == 'D' && a.SpecialRole == SpecialRole.AllUsers ).Any() )
                {
                    atRiskFileTypes.Add( fileType );
                }
            }

            return atRiskFileTypes;
        }

        #endregion

        #region GlobalLavaCommands Methods

        /// <summary>
        /// Builds the global lava commands header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildGlobalLavaCommandsHeader( RockContext rockContext )
        {
            var auditValue = GetGlobalLavaCommandsData().Count;
            var auditGoal = 0;

            var headerText = String.Format( "Vulnerable Default Lava Commands: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is limit where lava commands can be used. BEMA recommends disabling the Execute, RockEntity, and Sql commands from the Global Default Enabled Lava Commands.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderGlobalLavaCommands, lDescriptionGlobalLavaCommands );
        }

        /// <summary>
        /// Handles the GridRebind event of the gGlobalLavaCommands control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gGlobalLavaCommands_GridRebind( object sender, EventArgs e )
        {
            BindGlobalLavaCommandsGrid();
        }

        /// <summary>
        /// Binds the global lava commands grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindGlobalLavaCommandsGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<string> defaultLavaCommands = GetGlobalLavaCommandsData();

            gGlobalLavaCommands.DataSource = defaultLavaCommands;
            gGlobalLavaCommands.DataBind();
        }

        /// <summary>
        /// Gets the global lava commands data.
        /// </summary>
        /// <returns></returns>
        private static List<string> GetGlobalLavaCommandsData()
        {
            return GlobalAttributesCache.Get()
                                    .GetValue( "DefaultEnabledLavaCommands" )
                                    .SplitDelimitedValues()
                                    .Where( lc => lc == "RockEntity" || lc == "Sql" || lc == "Execute" || lc == "All" )
                                    .ToList();
        }

        #endregion

        #region Finance Page Methods

        /// <summary>
        /// Builds the finance pages header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildFinancePagesHeader( RockContext rockContext )
        {
            var auditValue = GetFinancePagesData( rockContext ).Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumUnsecuredFinancePageCount ).AsInteger();

            var headerText = String.Format( "Unsecured Finance Pages: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit access to financial information. BEMA recommends allowing no more than {0} unsecured financial pages.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderFinancePages, lDescriptionFinancePages );
        }

        /// <summary>
        /// Handles the GridRebind event of the gFinancePages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gFinancePages_GridRebind( object sender, EventArgs e )
        {
            BindFinancePagesGrid();
        }

        /// <summary>
        /// Binds the finance pages grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindFinancePagesGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<Rock.Model.Page> unsecuredPages = GetFinancePagesData( rockContext );

            gFinancePages.DataSource = unsecuredPages;
            gFinancePages.DataBind();
        }

        /// <summary>
        /// Gets the finance pages data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<Rock.Model.Page> GetFinancePagesData( RockContext rockContext )
        {
            var pageService = new PageService( rockContext );

            List<int> unsecuredPageIds = new List<int>();
            List<int> authorizedGroupIds = new List<int>();

            var groupService = new GroupService( rockContext );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_FINANCE_USERS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_BENEVOLENCE.AsGuid() ).Id );

            var rootPage = pageService.Get( Rock.SystemGuid.Page.FINANCE.AsGuid() );
            if ( rootPage != null )
            {
                var pages = pageService.GetAllDescendents( rootPage.Id );
                foreach ( var page in pages )
                {
                    CheckItemSecurity( rockContext, unsecuredPageIds, authorizedGroupIds, page );
                }
            }
            var unsecuredPages = pageService.GetByIds( unsecuredPageIds ).ToList();
            return unsecuredPages;
        }

        #endregion

        #region Admin Page Methods

        /// <summary>
        /// Builds the admin pages header.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BuildAdminPagesHeader( RockContext rockContext )
        {
            var auditValue = GetAdminPagesData( rockContext ).Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumUnsecuredAdminPageCount ).AsInteger();

            var headerText = String.Format( "Unsecured Admin Pages: <span class='badge'>{0}</span>", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit access to admin configuration pages. BEMA recommends allowing no more than {0} unsecured admin pages.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderAdminPages, lDescriptionAdminPages );
        }

        /// <summary>
        /// Handles the GridRebind event of the gAdminPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAdminPages_GridRebind( object sender, EventArgs e )
        {
            BindAdminPagesGrid();
        }

        /// <summary>
        /// Binds the admin pages grid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindAdminPagesGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            List<Rock.Model.Page> unsecuredPages = GetAdminPagesData( rockContext );

            gAdminPages.DataSource = unsecuredPages;
            gAdminPages.DataBind();
        }

        /// <summary>
        /// Gets the admin pages data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<Rock.Model.Page> GetAdminPagesData( RockContext rockContext )
        {
            var pageService = new PageService( rockContext );

            List<int> unsecuredPageIds = new List<int>();
            List<int> authorizedGroupIds = new List<int>();

            var groupService = new GroupService( rockContext );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( "1918E74F-C00D-4DDD-94C4-2E7209CE12C3".AsGuid() ).Id );

            var rootPage = pageService.Get( Rock.SystemGuid.Page.ROCK_SETTINGS.AsGuid() );
            if ( rootPage != null )
            {
                var pages = pageService.GetAllDescendents( rootPage.Id );
                foreach ( var page in pages )
                {
                    CheckItemSecurity( rockContext, unsecuredPageIds, authorizedGroupIds, page );
                }
            }
            var unsecuredPages = pageService.GetByIds( unsecuredPageIds ).ToList();
            return unsecuredPages;
        }

        #endregion
    }
}