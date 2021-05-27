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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Update;
using Rock.Update.Enum;
using Rock.Update.Exceptions;
using Rock.Update.Helpers;
using Rock.Update.Models;
using Rock.Update.Services;
using Rock.VersionInfo;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Rock Update" )]
    [Category( "Core" )]
    [Description( "Handles checking for and performing upgrades to the Rock system." )]
    public partial class RockUpdate : Rock.Web.UI.RockBlock
    {
        #region Fields
        private bool _isEarlyAccessOrganization = false;
        private List<RockRelease> _releases = new List<RockRelease>();
        Version _installedVersion = new Version( "0.0.0" );
        private bool _isOkToProceed = false;
        private bool _hasSqlServer14OrHigher = false;
        #endregion

        #region Properties
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
                $('#btn-restart').on('click', function () {
                    var btn = $(this);
                    btn.button('loading');
                    location = location.href;
                });
            ";

            ScriptManager.RegisterStartupScript( pnlUpdateSuccess, pnlUpdateSuccess.GetType(), "restart-script", script, true );
        }

        /// <summary>
        /// Invoked on page load.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            var rockUpdateService = new RockUpdateService();

            // Set timeout for up to 15 minutes (just like installer)
            Server.ScriptTimeout = 900;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 900;

            _isEarlyAccessOrganization = rockUpdateService.IsEarlyAccessInstance();
            _installedVersion = new Version( VersionInfo.GetRockSemanticVersionNumber() );

            if ( rockUpdateService.GetRockReleaseProgram() != RockReleaseProgram.Production )
            {
                nbRepoWarning.Visible = true;
            }

            DisplayRockVersion();
            if ( !IsPostBack )
            {
                btnIssues.NavigateUrl = rockUpdateService.GetRockEarlyAccessRequestUrl();

                if ( _isEarlyAccessOrganization )
                {
                    hlblEarlyAccess.LabelType = Rock.Web.UI.Controls.LabelType.Success;
                    hlblEarlyAccess.Text = "Early Access: Enabled";

                    pnlEarlyAccessNotEnabled.Visible = false;
                    pnlEarlyAccessEnabled.Visible = true;
                }

                var result = VersionValidationHelper.CheckFrameworkVersion();

                _isOkToProceed = true;

                if ( result == DotNetVersionCheckResult.Fail )
                {

                    nbVersionIssue.Visible = true;
                    nbVersionIssue.Text += "<p>You will need to upgrade your hosting server in order to proceed with the v13 update.</p>";
                    nbBackupMessage.Visible = false;
                }
                else if ( result == DotNetVersionCheckResult.Unknown )
                {
                    nbVersionIssue.Visible = true;
                    nbVersionIssue.Text += "<p>You may need to upgrade your hosting server in order to proceed with the v13 update. We were <b>unable to determine which Framework version</b> your server is using.</p>";
                    nbVersionIssue.Details += "<div class='alert alert-warning'>We were unable to check your server to verify that the .Net 4.7.2 Framework is installed! <b>You MUST verify this manually before you proceed with the update</b> otherwise your Rock application will be broken until you update the server.</div>";
                    nbBackupMessage.Visible = false;
                }

                _hasSqlServer14OrHigher = VersionValidationHelper.CheckSqlServerVersionGreaterThenSqlServer2012();

                if ( !_hasSqlServer14OrHigher )
                {
                    nbSqlServerVersionIssue.Visible = true;
                }

                _releases = GetOrderedReleaseList( rockUpdateService, _installedVersion );

                if ( _releases.Count > 0 )
                {
                    if ( new Version( _releases.Last().SemanticVersion ) >= new Version( "1.13.0" ) )
                    {
                        nbVersionIssue.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    }
                    pnlUpdatesAvailable.Visible = true;
                    pnlUpdates.Visible = true;
                    pnlNoUpdates.Visible = false;
                    cbIncludeStats.Visible = true;
                    BindGrid();
                }

                FileManagementHelper.CleanUpDeletedFiles();
            }
        }

        private List<RockRelease> GetOrderedReleaseList( RockUpdateService rockUpdateService, Version installedVersion )
        {
            return rockUpdateService
                .GetReleasesList( installedVersion )
                .OrderByDescending( p => new Version( p.SemanticVersion ) )
                .ToList();
        }

        #endregion

        #region Events

        /// <summary>
        /// Bind the available packages to the repeater.
        /// </summary>
        private void BindGrid()
        {
            rptPackageVersions.DataSource = _releases;
            rptPackageVersions.DataBind();
        }

        /// <summary>
        /// Wraps the install or update process in some guarded code while putting the app in "offline"
        /// mode and then back "online" when it's complete.
        /// </summary>
        /// <param name="version">the semantic version number</param>
        private void Update( string version )
        {
            var targetVersion = new Version( version );

            try
            {
                pnlUpdatesAvailable.Visible = false;
                pnlUpdates.Visible = false;

                if ( !UpdateRockPackage( version ) )
                {
                    pnlError.Visible = true;
                    pnlUpdateSuccess.Visible = false;
                }
                else
                {
                    SendStatictics( version );
                }

                lRockVersion.Text = "";
            }
            catch ( Exception ex )
            {
                pnlError.Visible = true;
                pnlUpdateSuccess.Visible = false;
                nbErrors.Text = string.Format( "Something went wrong.  Although the errors were written to the error log, they are listed for your review:<br/>{0}", ex.Message );
                LogException( ex );
            }
        }

        /// <summary>
        /// Enables and sets the appropriate CSS class on the install buttons and each div panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPackageVersions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var package = e.Item.DataItem as RockRelease;
                if ( package != null )
                {
                    LinkButton lbInstall = e.Item.FindControl( "lbInstall" ) as LinkButton;
                    var divPanel = e.Item.FindControl( "divPanel" ) as HtmlGenericControl;

                    if ( ( package.RequiresVersion.IsNotNullOrWhiteSpace() && new Version( package.RequiresVersion ) <= _installedVersion )
                        || ( package.RequiresVersion.IsNullOrWhiteSpace() && new Version( package.SemanticVersion ) > _installedVersion ) )
                    {
                        var release = _releases.Where( r => r.Version == package.Version.ToString() ).FirstOrDefault();
                        if ( !_isEarlyAccessOrganization && release != null && release.RequiresEarlyAccess )
                        {
                            lbInstall.Enabled = false;
                            lbInstall.Text = "Available to Early<br/>Access Organizations";
                            lbInstall.AddCssClass( "btn-warning" );
                            divPanel.AddCssClass( "panel-block" );
                        }
                        else
                        {
                            lbInstall.Enabled = true;
                            lbInstall.AddCssClass( "btn-info" );
                            divPanel.AddCssClass( "panel-info" );
                        }
                    }
                    else
                    {
                        lbInstall.Enabled = false;
                        lbInstall.Text = "Pending";
                        lbInstall.AddCssClass( "btn-default" );
                        divPanel.AddCssClass( "panel-block" );
                    }

                    if ( !_isOkToProceed || !VersionValidationHelper.CanInstallVersion( new Version( package.SemanticVersion ) ) )
                    {
                        lbInstall.Enabled = false;
                        lbInstall.AddCssClass( "btn-danger" );
                        lbInstall.AddCssClass( "small" );
                        lbInstall.AddCssClass( "btn-xs" );
                        lbInstall.Text = "Requirements not met";
                    }
                    else if ( package.PackageUri.IsNullOrWhiteSpace() )
                    {
                        lbInstall.Enabled = false;
                        lbInstall.AddCssClass( "small" );
                        lbInstall.AddCssClass( "btn-xs" );
                        lbInstall.Text = "Package doesn't exists.";
                    }
                    // If any packages can be installed we need to show the backup message.
                    nbBackupMessage.Visible = nbBackupMessage.Visible || lbInstall.Enabled;
                }
            }
        }

        /// <summary>
        /// Handles the click event for the particular version button that was clicked.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptPackageVersions_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            string version = e.CommandArgument.ToString();

            hdnInstallVersion.Value = version;
            litConfirmationMessage.Text = string.Format( "Are you sure you want to upgrade to Rock {0}?", RockVersion( new Version( version ) ) );
            mdConfirmInstall.Show();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Updates an existing Rock package to the given version and returns true if successful.
        /// </summary>
        /// <returns>true if the update was successful; false if errors were encountered</returns>
        protected bool UpdateRockPackage( string version )
        {
            var errors = Enumerable.Empty<string>();
            var rockUpdateService = new RockUpdateService();
            try
            {
                var rockInstaller = new RockInstaller( rockUpdateService, new Version( version ), _installedVersion );

                var installedRelease = rockInstaller.InstallVersion();

                nbSuccess.Text = ConvertToHtmlLiWrappedUl( installedRelease.ReleaseNotes ).ConvertCrLfToHtmlBr();
                lSuccessVersion.Text = GetRockVersion( installedRelease.SemanticVersion );
            }
            catch ( OutOfMemoryException ex )
            {
                errors = errors.Concat( new[] { string.Format( "There is a problem installing v{0}. It looks like your website ran out of memory. Check out <a href='http://www.rockrms.com/Rock/UpdateIssues#outofmemory'>this page for some assistance</a>", version ) } );
                LogException( ex );
            }
            catch ( System.Xml.XmlException ex )
            {
                errors = errors.Concat( new[] { string.Format( "There is a problem installing v{0}. It looks one of the standard XML files ({1}) may have been customized which prevented us from updating it. Check out <a href='http://www.rockrms.com/Rock/UpdateIssues#customizedxml'>this page for some assistance</a>", version, ex.Message ) } );
                LogException( ex );
            }
            catch ( IOException ex )
            {
                errors = errors.Concat( new[] { string.Format( "There is a problem installing v{0}. We were not able to replace an important file ({1}) after the update. Check out <a href='http://www.rockrms.com/Rock/UpdateIssues#unabletoreplacefile'>this page for some assistance</a>", version, ex.Message ) } );
                LogException( ex );
            }
            catch ( VersionValidationException ex )
            {
                errors = errors.Concat( new[] { ex.Message } );
                LogException( ex );
            }
            catch ( Exception ex )
            {
                errors = errors.Concat( new[] { string.Format( "There is a problem installing v{0}: {1}", version, ex.Message ) } );
                LogException( ex );
            }

            if ( errors != null && errors.Count() > 0 )
            {
                pnlError.Visible = true;
                nbErrors.Text = errors.Aggregate( new StringBuilder( "<ul class='list-padded'>" ), ( sb, s ) => sb.AppendFormat( "<li>{0}</li>", s ) ).Append( "</ul>" ).ToString();
                return false;
            }
            else
            {
                pnlUpdateSuccess.Visible = true;
                nbMoreUpdatesAvailable.Visible = GetOrderedReleaseList( rockUpdateService, new Version( version ) ).Count > 0;
                rptPackageVersions.Visible = false;
                return true;
            }
        }

        /// <summary>
        /// Fetches and displays the official Rock product version.
        /// </summary>
        protected void DisplayRockVersion()
        {
            lRockVersion.Text = string.Format( "<b>Current Version: </b> {0}", VersionInfo.GetRockProductVersionFullName() );
            lNoUpdateVersion.Text = VersionInfo.GetRockProductVersionFullName();
        }

        protected string GetRockVersion( object version )
        {
            var semanticVersion = version as Version;
            if ( semanticVersion == null )
            {
                semanticVersion = new Version( version.ToString() );
            }

            if ( semanticVersion != null )
            {
                return "Rock " + RockVersion( semanticVersion );
            }
            else
            {
                return string.Empty;
            }
        }

        protected string RockVersion( Version version )
        {
            switch ( version.Major )
            {
                case 1:
                    return string.Format( "McKinley {0}.{1}", version.Minor, version.Build );
                default:
                    return string.Format( "{0}.{1}.{2}", version.Major, version.Minor, version.Build );
            }
        }

        /// <summary>
        /// Converts + and * to html line items (li) wrapped in unordered lists (ul).
        /// </summary>
        /// <param name="str">a string that contains lines that start with + or *</param>
        /// <returns>an html string of <code>li</code> wrapped in <code>ul</code></returns>
        public string ConvertToHtmlLiWrappedUl( string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            bool foundMatch = false;

            // Lines that start with  "+ *" or "+" or "*"
            var re = new System.Text.RegularExpressions.Regex( @"^\s*(\+ \* |[\+\*]+)(.*)" );
            var htmlBuilder = new StringBuilder();

            // split the string on newlines...
            string[] splits = str.Split( new[] { Environment.NewLine, "\x0A" }, StringSplitOptions.RemoveEmptyEntries );
            // look at each line to see if it starts with a + or * and then strip it and wrap it in <li></li>
            for ( int i = 0; i < splits.Length; i++ )
            {
                var match = re.Match( splits[i] );
                if ( match.Success )
                {
                    foundMatch = true;
                    htmlBuilder.AppendFormat( "<li>{0}</li>", match.Groups[2] );
                }
                else
                {
                    htmlBuilder.Append( splits[i] );
                }
            }

            // if we had a match then wrap it in <ul></ul> markup
            return foundMatch ? string.Format( "<ul class='list-padded'>{0}</ul>", htmlBuilder.ToString() ) : htmlBuilder.ToString();
        }

        /// <summary>
        /// Sends statistics to the SDN server but only if there are more than 100 person records
        /// or the sample data has not been loaded.
        ///
        /// The statistics are:
        ///     * Rock Instance Id
        ///     * Update Version
        ///     * IP Address - The IP address of your Rock server.
        ///
        /// ...and we only send these if they checked the "Include Impact Statistics":
        ///     * Organization Name and Address
        ///     * Public Web Address
        ///     * Number of Active Records
        ///
        /// As per http://www.rockrms.com/Rock/Impact
        /// </summary>
        /// <param name="version">the semantic version number</param>
        private void SendStatictics( string version )
        {
            try
            {
                var ipAddress = Request.ServerVariables["LOCAL_ADDR"];
                var environmentData = RockUpdateHelper.GetEnvDataAsJson( Request, ResolveRockUrl( "~/" ) );
                using ( var rockContext = new RockContext() )
                {
                    var instanceStatistics = new RockInstanceImpactStatistics( new RockImpactService(), rockContext );

                    instanceStatistics.SendImpactStatisticsToSpark( cbIncludeStats.Checked, version, ipAddress, environmentData );
                }
            }
            catch ( Exception ex )
            {
                // Just catch any exceptions, log it, and keep moving... We don't want to mess up the experience
                // over a few statistics/metrics.
                try
                {
                    LogException( ex );
                }
                catch { }
            }
        }

        #endregion

        protected void mdConfirmInstall_SaveClick( object sender, EventArgs e )
        {
            mdConfirmInstall.Hide();
            Update( hdnInstallVersion.Value );
        }
    }
}