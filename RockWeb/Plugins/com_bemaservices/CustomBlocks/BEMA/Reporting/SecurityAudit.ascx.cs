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
using System.Data;
using System.Text.RegularExpressions;
using Rock.Security;

namespace RockWeb.Plugins.com_bemaservices.CustomBlocks.Bema.Reporting
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Security Audit" )]
    [Category( "BEMA Services > Reporting" )]
    [Description( "Report block to give an update on a site's security" )]

    // Rock Admin Count Settings
    [IntegerField( "Recommended Maximum Number of Rock Admins", "The recommended maximum number of Rock Admins to use for the audit.", true, 10, "Rock Admin Count Settings", 0, AttributeKey.RecommendedMaximumAdminCount )]

    // Unencrypted Traffic Settings
    [IntegerField( "Recommended Maximum Number of Pages Allowing Unencrypted Traffic", "The recommended maximum number of pages that can allow unencrypted traffic.", true, 0, "Unencrypted Traffic Settings", 1, AttributeKey.RecommendedUnencryptedPageCount )]

    // Non Staff Members Settings
    [IntegerField( "Recommended Maximum Number of Non-Staff Members of Security Roles", "The recommended maximum number of non-staff members of security roles.", true, 5, "Non Staff Members Settings", 2, AttributeKey.RecommendedNonStaffMembers )]
    [CustomCheckboxListField( "Excluded Security Roles", "Security roles to exclude from the non-staff audit", "Select Id as Value, Name as Text from [Group] where IsSecurityRole = 1", false, "", "Non Staff Members Settings", 3, AttributeKey.ExcludedSecurityRoles )]

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

    public partial class SecurityAudit : RockBlock, ICustomGridColumns
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

        private void GenerateHeader( string headerText, string descriptionText, int auditValue, int auditGoal, bool showWarning, HtmlGenericControl targetHeader, Literal targetLiteral )
        {
            var headerCss = "";
            TotalChecks++;
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

            targetHeader.InnerHtml = String.Format( "<h5 class='mb-0'>{0}<span class=\"pull-right\"><i class=\"fa fa-chevron-down\"></i></span></h5>", headerText );
            targetLiteral.Text = descriptionText;
            targetHeader.AddCssClass( headerCss );
        }

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
            var descriptionText = String.Format( "A key part of improving your site's security is to require SSL Encryption. BEMA recommends allowing no more than {0} pages to be accessed on an unencrypted connection.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderSslEnabled, lDescriptionSslEnabled );
        }

        private void gSslEnabled_GridRebind( object sender, EventArgs e )
        {
            BindSslEnabledGrid();
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

        #region Non Staff Methods

        private void BuildNonStaffHeader( RockContext rockContext )
        {
            var organizationUnitGuid = Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT.AsGuid();
            var excludedRoleIds = GetAttributeValue( AttributeKey.ExcludedSecurityRoles ).SplitDelimitedValues().AsIntegerList();

            var auditValue = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                .Where( gm =>
                    gm.Person.Members.Where( gm1 => gm1.Group.GroupType.Guid == organizationUnitGuid && gm1.GroupMemberStatus == GroupMemberStatus.Active ).Count() == 0 &&
                    gm.Group.IsSecurityRole &&
                    !excludedRoleIds.Contains( gm.GroupId ) &&
                    gm.Group.IsActive
                )
                .Count();
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedNonStaffMembers ).AsInteger();

            var headerText = String.Format( "Non-Staff Security Role Members: {0}", auditValue );
            var descriptionText = String.Format( "BEMA recommends limiting the number of people in security roles that are not in your organization. We recommend no more than {0} people be in a security role that are not part of your organization in some way.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderNonStaff, lDescriptionNonStaff );
        }

        private void gNonStaff_GridRebind( object sender, EventArgs e )
        {
            BindNonStaffGrid();
        }

        private void BindNonStaffGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

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

            gNonStaff.DataSource = qry.ToList();
            gNonStaff.DataBind();
        }

        #endregion

        #region PageParameterSql Methods

        private void BuildPageParameterSqlHeader( RockContext rockContext )
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

            var auditValue = dataSet.Tables[0].Rows.Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedSqlPageParameterBlocks ).AsInteger();

            var headerText = String.Format( "Blocks Using Page Parameters in SQL Queries: {0}", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit the capability for Sql injection attacks. BEMA recommends allowing no more than {0} blocks to use page parameters in their sql queries.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderPageParameterSql, lDescriptionPageParameterSql );
        }

        private void gPageParameterSql_GridRebind( object sender, EventArgs e )
        {
            BindPageParameterSqlGrid();
        }

        private void BindPageParameterSqlGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

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
            gPageParameterSql.DataSource = dataSet.Tables[0];
            gPageParameterSql.DataBind();
        }

        #endregion

        #region SqlLavaCommand Methods

        private void BuildSqlLavaCommandHeader( RockContext rockContext )
        {
            var siteList = GetAttributeValue( AttributeKey.SqlLavaCommandSites );
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

            var auditValue = dataSet.Tables[0].Rows.Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedSqlLavaCommandBlocks ).AsInteger();


            var headerText = String.Format( "Blocks Using SQL Lava Commands: {0}", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit the capability for Sql injection attacks. BEMA recommends allowing no more than {0} blocks to use sql commands in their lava.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderSqlLavaCommand, lDescriptionSqlLavaCommand );
        }

        private void gSqlLavaCommand_GridRebind( object sender, EventArgs e )
        {
            BindSqlLavaCommandGrid();
        }

        private void BindSqlLavaCommandGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

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
            gSqlLavaCommand.DataSource = dataSet.Tables[0];
            gSqlLavaCommand.DataBind();
        }

        #endregion


        #region Person Auth Methods

        private void BuildPersonAuthHeader( RockContext rockContext )
        {
            var auditValue = new AuthService( rockContext ).Queryable().AsNoTracking().Where( a => a.PersonAliasId != null ).Count();
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumPersonAuthCount ).AsInteger();

            var headerText = String.Format( "Number of Person-Specific Auth Rules: {0}", auditValue );
            var descriptionText = String.Format( "Limiting the number of person-specific auth rules and relying on group-based auth rules is crucial to improving your organization's security. BEMA recommends having no more than {0} person-specific auth rules set on your database.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderPersonAuth, lDescriptionPersonAuth );
        }

        private void gPersonAuth_GridRebind( object sender, EventArgs e )
        {
            BindPersonAuthGrid();
        }

        private void BindPersonAuthGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }


            AuthService authService = new AuthService( rockContext );

            // sample query to display a few people
            var qry = authService.Queryable().AsNoTracking().Where( a => a.PersonAliasId != null )
                        .ToList();

            gPersonAuth.DataSource = qry.ToList();
            gPersonAuth.DataBind();
        }

        #endregion

        #region Unencrypted Sensitive Data Methods

        private void BuildUnencryptedSensitiveDataHeader( RockContext rockContext )
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
            var auditValue = dataSet.Tables[0].Rows.Count;

            var auditGoal = 0;

            var headerText = String.Format( "Unencrypted Sensitive Attributes: {0}", auditValue );
            var descriptionText = String.Format( "Below is a list of all attributes containing sensitive data that are not currently encrypted. Currently this only includes Social Security Numbers. BEMA recommends using the encrypted text field type for all attributes containing sensitive data." );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderUnencryptedSensitiveData, lDescriptionUnencryptedSensitiveData );
        }

        private void gUnencryptedSensitiveData_GridRebind( object sender, EventArgs e )
        {
            BindUnencryptedSensitiveDataGrid();
        }

        private void BindUnencryptedSensitiveDataGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var query = @"Select a.Id, a.Name, et.Name as EntityType, a.EntityTypeQualifierColumn, a.EntityTypeQualifierValue, count(0) as SensitiveRecords
                            From AttributeValue av
                            Join Attribute a on a.Id = av.AttributeId
                            Join EntityType et on a.EntityTypeId = et.Id
                            Where av.Value like '[0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9][0-9][0-9]'
                            or av.Value like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                            or av.Value like '[0-9][0-9][0-9] [0-9][0-9] [0-9][0-9][0-9][0-9]'
                            Group By a.Id, a.Name, et.Name, a.EntityTypeQualifierColumn, a.EntityTypeQualifierValue";
            DataSet dataSet = DbService.GetDataSet( query, CommandType.Text, null, GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180 );

            gUnencryptedSensitiveData.DataSource = dataSet.Tables[0];
            gUnencryptedSensitiveData.DataBind();
        }

        #endregion


        #region Finance Data View Methods

        private void BuildFinanceDataViewsHeader( RockContext rockContext )
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

            var auditValue = unsecuredDataViews.Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumUnsecuredFinanceDataViewCount ).AsInteger();

            var headerText = String.Format( "Unsecured Finance Data Views: {0}", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit access to financial information. BEMA recommends allowing no more than {0} unsecured financial data views.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderFinanceDataViews, lDescriptionFinanceDataViews );
        }

        private void gFinanceDataViews_GridRebind( object sender, EventArgs e )
        {
            BindFinanceDataViewsGrid();
        }

        private void BindFinanceDataViewsGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }
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
            gFinanceDataViews.DataSource = unsecuredDataViews;
            gFinanceDataViews.DataBind();
        }

        #endregion


        #region FileTypeSecurity Methods

        private void BuildFileTypeSecurityHeader( RockContext rockContext )
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

            var auditValue = atRiskFileTypes.Count;
            var auditGoal = 0;

            var headerText = String.Format( "Unsecured Sensitive File Types: {0}", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to secure file types dealing with sensitive data. BEMA recommends locking down all sensitive file types.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderFileTypeSecurity, lDescriptionFileTypeSecurity );
        }

        private void gFileTypeSecurity_GridRebind( object sender, EventArgs e )
        {
            BindFileTypeSecurityGrid();
        }

        private void BindFileTypeSecurityGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }


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
            gFileTypeSecurity.DataSource = atRiskFileTypes;
            gFileTypeSecurity.DataBind();
        }

        #endregion

        #region GlobalLavaCommands Methods

        private void BuildGlobalLavaCommandsHeader( RockContext rockContext )
        {
            var defaultLavaCommands = GlobalAttributesCache.Get()
                        .GetValue( "DefaultEnabledLavaCommands" )
                        .SplitDelimitedValues()
                        .Where( lc => lc == "RockEntity" || lc == "Sql" || lc == "Execute" || lc == "All" )
                        .ToList();

            var auditValue = defaultLavaCommands.Count;
            var auditGoal = 0;


            var headerText = String.Format( "Vulnerable Default Lava Commands: {0}", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is limit where lava commands can be used. BEMA recommends disabling the Execute, RockEntity, and Sql commands from the Global Default Enabled Lava Commands.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, false, divHeaderGlobalLavaCommands, lDescriptionGlobalLavaCommands );
        }

        private void gGlobalLavaCommands_GridRebind( object sender, EventArgs e )
        {
            BindGlobalLavaCommandsGrid();
        }

        private void BindGlobalLavaCommandsGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var defaultLavaCommands = GlobalAttributesCache.Get()
                        .GetValue( "DefaultEnabledLavaCommands" )
                        .SplitDelimitedValues()
                        .Where( lc => lc == "RockEntity" || lc == "Sql" || lc == "Execute" || lc == "All" )
                        .ToList();
            gGlobalLavaCommands.DataSource = defaultLavaCommands;
            gGlobalLavaCommands.DataBind();
        }

        #endregion

        #region Finance Page Methods

        private void BuildFinancePagesHeader( RockContext rockContext )
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

            var auditValue = unsecuredPages.Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumUnsecuredFinancePageCount ).AsInteger();

            var headerText = String.Format( "Unsecured Finance Pages: {0}", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit access to financial information. BEMA recommends allowing no more than {0} unsecured financial pages.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderFinancePages, lDescriptionFinancePages );
        }

        private void gFinancePages_GridRebind( object sender, EventArgs e )
        {
            BindFinancePagesGrid();
        }

        private void BindFinancePagesGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

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
            gFinancePages.DataSource = unsecuredPages;
            gFinancePages.DataBind();
        }

        #endregion

        #region Admin Page Methods

        private void BuildAdminPagesHeader( RockContext rockContext )
        {
            var pageService = new PageService( rockContext );

            List<int> unsecuredPageIds = new List<int>();
            List<int> authorizedGroupIds = new List<int>();

            var groupService = new GroupService( rockContext );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( "1918E74F-C00D-4DDD-94C4-2E7209CE12C3".AsGuid() ).Id );

            var rootPage = pageService.Get( Rock.SystemGuid.Page.ADMIN_TOOLS.AsGuid() );
            if ( rootPage != null )
            {
                var pages = pageService.GetAllDescendents( rootPage.Id );
                foreach ( var page in pages )
                {
                    CheckItemSecurity( rockContext, unsecuredPageIds, authorizedGroupIds, page );
                }
            }
            var unsecuredPages = pageService.GetByIds( unsecuredPageIds ).ToList();

            var auditValue = unsecuredPages.Count;
            var auditGoal = GetAttributeValue( AttributeKey.RecommendedMaximumUnsecuredAdminPageCount ).AsInteger();

            var headerText = String.Format( "Unsecured Admin Pages: {0}", auditValue );
            var descriptionText = String.Format( "A key part of improving your site's security is to limit access to admin configuration pages. BEMA recommends allowing no more than {0} unsecured admin pages.", auditGoal );
            GenerateHeader( headerText, descriptionText, auditValue, auditGoal, true, divHeaderAdminPages, lDescriptionAdminPages );
        }

        private void gAdminPages_GridRebind( object sender, EventArgs e )
        {
            BindAdminPagesGrid();
        }

        private void BindAdminPagesGrid( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var pageService = new PageService( rockContext );

            List<int> unsecuredPageIds = new List<int>();
            List<int> authorizedGroupIds = new List<int>();

            var groupService = new GroupService( rockContext );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() ).Id );
            authorizedGroupIds.Add( groupService.Get( "1918E74F-C00D-4DDD-94C4-2E7209CE12C3".AsGuid() ).Id );

            var rootPage = pageService.Get( Rock.SystemGuid.Page.ADMIN_TOOLS.AsGuid() );
            if ( rootPage != null )
            {
                var pages = pageService.GetAllDescendents( rootPage.Id );
                foreach ( var page in pages )
                {
                    CheckItemSecurity( rockContext, unsecuredPageIds, authorizedGroupIds, page );
                }
            }
            var unsecuredPages = pageService.GetByIds( unsecuredPageIds ).ToList();

            gAdminPages.DataSource = unsecuredPages;
            gAdminPages.DataBind();
        }

        #endregion
    }
}