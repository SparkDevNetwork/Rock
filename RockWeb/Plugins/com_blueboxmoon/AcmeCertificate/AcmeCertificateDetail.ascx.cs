using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

using com.blueboxmoon.AcmeCertificate;
using System.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.AcmeCertificate
{
    [DisplayName( "Acme Certificate Detail" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Configures a certificate." )]
    public partial class AcmeCertificateDetail : RockBlock
    {
        #region Protected Properties

        /// <summary>
        /// Contains the information, via ViewState, for all the bindings that are configured
        /// for this certificate.
        /// </summary>
        protected List<BindingData> BindingsState { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Load the custom view state data.
        /// </summary>
        /// <param name="savedState">The object that contains our view state.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            BindingsState = ( List<BindingData> ) ViewState["BindingsState"];
        }

        /// <summary>
        /// Handles the OnInit event of the block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBindings.Actions.ShowAdd = true;
            gBindings.Actions.AddClick += gBindings_Add;
            lbDetailDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", "Certificate" );
        }

        /// <summary>
        /// Handles the OnLoad event of the block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                if ( PageParameter( "Id" ).AsInteger() != 0 )
                {
                    ShowDetail();
                }
                else
                {
                    ShowEdit();
                }
            }

            nbMessage.Text = string.Empty;
        }

        /// <summary>
        /// Save the custom view state data.
        /// </summary>
        /// <returns>The object that contains the view state.</returns>
        protected override object SaveViewState()
        {
            ViewState["BindingsState"] = BindingsState;

            return base.SaveViewState();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show the read-only panel and fill in all fields.
        /// </summary>
        protected void ShowDetail()
        {
            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( PageParameter( "Id" ).AsInteger() );

            group.LoadAttributes( rockContext );

            ltDetailTitle.Text = group.Name;
            ltRemoveOld.Text = group.GetAttributeValue( "RemoveOldCertificate" ).AsBoolean().ToString();
            ltDetailDomains.Text = string.Join( "<br />", group.GetAttributeValue( "Domains" ).SplitDelimitedValues() );
            ltDetailLastRenewed.Text = group.GetAttributeValue( "LastRenewed" );
            ltDetailExpires.Text = group.GetAttributeValue( "Expires" );

            var bindings = group.GetAttributeValue( "Bindings" )
                .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( b => new BindingData( b ) )
                .Select( b => string.Format( "{0} {1}:{2}:{3}", b.Site, string.IsNullOrWhiteSpace( b.IPAddress ) ? "*" : b.IPAddress, b.Port, b.Domain ) );

            ltDetailBindings.Text = string.Join( "<br />", bindings );

            CheckIISState();

            pnlEdit.Visible = false;
            pnlDetail.Visible = true;
        }

        /// <summary>
        /// Show the edit panel and fill in all fields.
        /// </summary>
        protected void ShowEdit()
        {
            int groupId = PageParameter( "Id" ).AsInteger();
            var group = new GroupService( new RockContext() ).Get( groupId );

            ltEditTitle.Text = groupId != 0 ? "Edit Certificate" : "Add Certificate";
            BindingsState = new List<BindingData>();

            if ( group != null )
            {
                group.LoadAttributes();

                tbFriendlyName.Text = group.Name;
                vlDomains.Value = group.GetAttributeValue( "Domains" );
                cbRemoveOldCertificate.Checked = group.GetAttributeValue( "RemoveOldCertificate" ).AsBoolean( false );

                var bindings = group.GetAttributeValue( "Bindings" ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                BindingsState = bindings.Select( b => new BindingData( b ) ).ToList();
            }

            GridBind();

            nbIISError.Visible = false;
            pnlIISRedirectModuleWarning.Visible = false;
            pnlIISRedirectSiteWarning.Visible = false;

            pnlDetail.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Performs various checks on IIS to ensure it is configured correctly for the certificates that
        /// have been setup for processing.
        /// </summary>
        protected void CheckIISState()
        {
            var rockContext = new RockContext();
            var targetUrl = GetRedirectUrl();
            var groupTypeId = GroupTypeCache.Get( com.blueboxmoon.AcmeCertificate.SystemGuid.GroupType.ACME_CERTIFICATES ).Id;
            var bindings = new List<BindingData>();

            var groups = new GroupService( rockContext ).Queryable()
                .Where( g => g.GroupTypeId == groupTypeId );

            var siteNames = new List<string>();

            //
            // Determine if we have any certificates that edit bindings of a site other than the Rock site.
            //
            foreach ( var group in groups )
            {
                var currentSiteName = System.Web.Hosting.HostingEnvironment.SiteName;

                group.LoadAttributes( rockContext );

                bindings.AddRange( group.GetAttributeValue( "Bindings" )
                    .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( b => new BindingData( b ) )
                    .Where( b => b.Site != currentSiteName ) );

                siteNames.AddRange( bindings.Where( b => !AcmeHelper.IsIISSiteRedirectEnabled( b.Site, targetUrl ) ).Select( b => b.Site ) );
            }

            //
            // If we have non-Rock sites to configure, ensure that the Http Redirect module has been
            // installed in IIS.
            //
            if ( bindings.Any() && !AcmeHelper.IsHttpRedirectModuleEnabled() )
            {
                pnlIISRedirectModuleWarning.Visible = true;
                pnlIISRedirectSiteWarning.Visible = false;
            }
            else
            {
                pnlIISRedirectModuleWarning.Visible = false;
            }

            //
            // If the redirect module has been installed but we have sites that need to be
            // configured, then present a notice about those sites.
            //
            if ( siteNames.Any() && !pnlIISRedirectModuleWarning.Visible )
            {
                siteNames = siteNames.Distinct().ToList();

                hfEnableSiteRedirects.Value = siteNames.AsDelimited( "," );
                ltEnableSiteRedirects.Text = "<li>" + siteNames.AsDelimited( "</li><li>" ) + "</li>";

                ltTargetRedirect.Text = targetUrl;
                pnlIISRedirectSiteWarning.Visible = true;
            }
            else
            {
                pnlIISRedirectSiteWarning.Visible = false;
            }
        }

        /// <summary>
        /// Gets a URL that should be used in configuring the Site Redirects to Rock.
        /// </summary>
        /// <returns>A string representing the URL to be used for site redirects.</returns>
        protected string GetRedirectUrl()
        {
            var url = GetAttributeValue( "RedirectOverride" );

            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = string.Format( "{0}.well-known/acme-challenge/", GlobalAttributesCache.Value( "PublicApplicationRoot" ) );
            }

            return url;
        }

        /// <summary>
        /// Bind the IIS Bindings grid to show the current list of configured bindings.
        /// </summary>
        protected void GridBind()
        {
            gBindings.DataSource = BindingsState;
            gBindings.DataBind();
        }

        /// <summary>
        /// Show the specified binding for editing.
        /// </summary>
        /// <param name="binding">The binding data to be edited or null to add a new binding.</param>
        protected void ShowBinding( BindingData binding )
        {
            ddlEditBindingSite.Items.Clear();
            ddlEditBindingSite.Items.Add( new ListItem() );
            try
            {
                AcmeHelper.GetSites().ToList().ForEach( s => ddlEditBindingSite.Items.Add( s ) );
            }
            catch { /* Intentionally left blank */ }

            ddlEditBindingIPAddress.Items.Clear();
            ddlEditBindingIPAddress.Items.Add( new ListItem() );
            try
            {
                AcmeHelper.GetIPv4Addresses().ToList().ForEach( a => ddlEditBindingIPAddress.Items.Add( a ) );
            }
            catch { /* Intentionally left blank */ }

            ddlEditBindingSite.SetValue( binding != null ? binding.Site : System.Web.Hosting.HostingEnvironment.SiteName );
            ddlEditBindingIPAddress.SetValue( binding != null ? binding.IPAddress : string.Empty );
            nbEditBindingPort.Text = binding != null ? binding.Port.ToString() : "443";
            tbEditBindingDomain.Text = binding != null ? binding.Domain : string.Empty;
        }

        #endregion

        #region Detail Event Methods

        /// <summary>
        /// Handles the Click event of the lbDetailEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailEdit_Click( object sender, EventArgs e )
        {
            ShowEdit();
        }

        /// <summary>
        /// Handles the Click event of the lbDetailRenew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailRenew_Click( object sender, EventArgs e )
        {
            pnlDetail.Visible = false;
            pnlRenew.Visible = true;
            pnlRenewOutput.Visible = true;
            pnlRenewInput.Visible = true;

            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( PageParameter( "Id" ).AsInteger() );
            var account = AcmeHelper.LoadAccountData();

            group.LoadAttributes( rockContext );

            var removeOldCertificate = group.GetAttributeValue( "RemoveOldCertificate" ).AsBoolean( false ).ToString().ToLower();
            var offlineMode = account.OfflineMode.ToString().ToLower();

            ltRenewTitle.Text = group.Name;
            tbRenewCSR.Text = string.Empty;

            var script = string.Format( @"
(function() {{
    var oldCertificateHash = '';
    var newCertificate = null;
    var $renewStatus = $('#{2}');
    var $status = null;
    var $error = null;
    var $progress = null;

    function GetExistingHash()
    {{
        UpdateStatus('Determining configuration...');

        $.get('/api/BBM_AcmeCertificate/Hash/{0}')
            .done(function(data) {{
                oldCertificateHash = data;
                RenewCertificate();
            }})
            .fail(function(data, status, xhr) {{
                ShowErrorObject(data);
            }});
    }}

    function RenewCertificate()
    {{
        UpdateStatus('Renewing certificate...');

        var url = '/api/BBM_AcmeCertificate/Renew/{0}';
        if ($('#{8}').val() != '')
        {{
            url += '?csr=' + encodeURIComponent($('#{8}').val());
        }}

        $.get(url)
            .done(function(data) {{
                newCertificate = data;
                if (newCertificate.PrivateKey && '{6}' === 'false')
                {{
                    InstallCertificate(true);
                }}
                else
                {{
                    ShowSuccess();
                }}
            }})
            .fail(function(data, status, xhr) {{
                ShowErrorObject(data);
            }});
    }}

    function InstallCertificate(retry)
    {{
        UpdateStatus('Installing Certificate...');

        $.post('/api/BBM_AcmeCertificate/Install', newCertificate)
            .done(function(data) {{
                VerifyCertificate();
            }})
            .fail(function(data, status, xhr) {{
                if (data.readyState == 4)
                {{
                    /* Sometimes the first attempt fails because we reconfigure bindings in IIS */
                    if (retry === true)
                    {{
                        InstallCertificate(false);
                    }}
                    else
                    {{
                        ShowErrorObject(data);
                    }}
                }}
                else
                {{
                    VerifyCertificate();
                }}
            }});
    }}

    function VerifyCertificate()
    {{
        UpdateStatus('Verifying Certificate Installed Correctly (this may take a while)...');

        $.get('/api/BBM_AcmeCertificate/Installed/{0}/' + newCertificate.Hash)
            .done(function(data) {{
                if (data == true)
                {{
                    DeleteOldCertificate();
                }}
                else
                {{
                    ShowError('The certificate did not install correctly and no error was returned. Check the exception log for more details.');
                }}
            }})
            .fail(function(data, status, xhr) {{
                ShowErrorObject(data);
            }});
    }}

    function DeleteOldCertificate()
    {{
        if ('{1}' != 'true')
        {{
            ShowSuccess();
            return;
        }}

        UpdateStatus('Removing old certificate...');

        $.ajax({{
            url: '/api/BBM_AcmeCertificate/Hash/' + oldCertificateHash,
            type: 'DELETE'
            }})
            .always(function() {{
                ShowSuccess();
            }});
    }}

    function ShowErrorObject(error)
    {{
        try
        {{
            var ex = JSON.parse(error.responseText);
            ShowError('Message: ' + ex.Message + '\nException Message: ' + ex.ExceptionMessage + '\nException Type: ' + ex.ExceptionType + '\nStack Trace: ' + ex.StackTrace);
        }}
        catch (e)
        {{
            if (typeof(error) == 'string')
            {{
                ShowError(error);
            }}
            else
            {{
                ShowError(JSON.stringify(error));
            }}
        }}
    }}

    function ShowError(error)
    {{
        if ($error == null)
        {{
            $error = $('<pre class=""margin-t-md alert alert-danger""></pre>');
            $error.appendTo($renewStatus);
        }}

        $error.text(error);

        $progress.hide();
        $('#{7}').show();
    }}

    function UpdateStatus(message)
    {{
        if ($status == null)
        {{
            $status = $('<pre class=""margin-t-md alert alert-info""></pre>');
            $status.appendTo($renewStatus);
        }}

        var oldText = $status.text();
        $status.text($status.text() + (oldText != '' ? '\n' : '') + message);
    }}

    function ShowExpander(title, content)
    {{
        var $panel = $('<div class=""panel panel-default margin-t-md""></div>');
        var $heading = $('<div class=""panel-heading""></div>').appendTo($panel);
        var $title = $('<h4 class=""panel-title""></h4>').appendTo($heading);
        var $body = $('<div class=""panel-collapse collapse""><div class=""panel-body""><pre>' + content + '</pre></div></div>').appendTo($panel);
        var $link = $('<a href=""#"">' + title + '</a>').on('click', function() {{ $body.collapse('toggle'); return false; }}).appendTo($title);

        $panel.appendTo($renewStatus);
    }}

    function ShowSuccess()
    {{
        var $success = $('<pre class=""margin-t-md alert alert-success""></pre>');
        $success.appendTo($renewStatus);

        var action = '{6}' != 'true' && newCertificate.PrivateKey ? 'installed' : 'renewed';

        $success.text('Certificate has been ' + action + '. You can see the certificate data by expanding the panels below.');

        if (newCertificate.PrivateKey)
        {{
            ShowExpander('Private Key', newCertificate.PrivateKey);
        }}

        for (var i = 0; i < newCertificate.Certificates.length; i++)
        {{
            ShowExpander(i == 0 ? 'Certificate' : 'Intermediate Certificate', newCertificate.Certificates[i]);
        }}

        $progress.hide();
        $('#{7}').show();
    }}

    $('#{5}').on('click', function () {{
        $('#{3}').hide();
        $('#{4}').show();

        $progress = $('<div class=""progress progress-striped active""><div class=""progress-bar"" role=""progressbar"" aria-valuenow=""100"" aria-valuemin=""0"" aria-valuemax=""100"" style=""width: 100%""></div></div>');
        $progress.appendTo($renewStatus);

        GetExistingHash();
    }});
}})();
",
                PageParameter( "Id" ),          // {0}
                removeOldCertificate,           // {1}
                divRenewStatus.ClientID,        // {2}
                pnlRenewInput.ClientID,         // {3}
                pnlRenewOutput.ClientID,        // {4}
                lbRequestCertificate.ClientID,  // {5}
                offlineMode,                    // {6}
                lbRenewDone.ClientID,           // {7}
                tbRenewCSR.ClientID             // {8}
            );

            ScriptManager.RegisterStartupScript( Page, GetType(), "initialize", script, true );
        }

        /// <summary>
        /// Handles the Click event of the lbDetailCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbDetailDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            var group = groupService.Get( PageParameter( "Id" ).AsInteger() );
            groupService.Delete( group );

            rockContext.SaveChanges();

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbEnableRedirectModule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEnableRedirectModule_Click( object sender, EventArgs e )
        {
            if ( !AcmeHelper.EnableIISHttpRedirectModule() )
            {
                nbIISError.Text = "Failed to enable the IIS Http Redirect module. Rock may not have enough permissions to perform this task. Please manually enable the IIS Http Redirect module.";
            }
            else
            {
                NavigateToCurrentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEnableSiteRedirects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEnableSiteRedirects_Click( object sender, EventArgs e )
        {
            var siteNames = hfEnableSiteRedirects.Value.Split( ',' );
            var targetUrl = GetRedirectUrl();

            var errors = new List<string>();

            //
            // For each site that was detected as not properly configured, try to configure it.
            //
            foreach ( var siteName in siteNames )
            {
                try
                {
                    AcmeHelper.EnableIISSiteRedirect( siteName, targetUrl );
                }
                catch ( Exception ex )
                {
                    errors.Add( ex.Message );
                }
            }

            if ( errors.Any() )
            {
                nbIISError.Text = string.Format( "Failed to enable the redirect on one or more sites. This may be due to insufficient permissions to make modifications to the other sites. <ul><li>{0}</li></ul>",
                    string.Join( "</li><li>", errors ) );
            }

            CheckIISState();
        }

        #endregion

        #region Edit Event Methods

        /// <summary>
        /// Handles the Click event of the lbEditSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditSave_Click( object sender, EventArgs e )
        {
            //
            // Verify we have at least one domain name entered.
            //
            if ( vlDomains.Value.SplitDelimitedValues().Length == 0 )
            {
                nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMessage.Text = "You must enter at least one domain to vaidate.";

                return;
            }

            //
            // Verify we have at least one binding configured.
            //
            if ( BindingsState.Count == 0 && !AcmeHelper.LoadAccountData().OfflineMode )
            {
                nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMessage.Text = "You must add at least one IIS binding.";

                return;
            }

            //
            // Load the existing data or create a new entry.
            //
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var group = groupService.Get( PageParameter( "Id" ).AsInteger() );
            if ( group == null )
            {
                group = new Group();
                group.GroupTypeId = GroupTypeCache.Get( com.blueboxmoon.AcmeCertificate.SystemGuid.GroupType.ACME_CERTIFICATES ).Id;

                groupService.Add( group );
            }

            group.LoadAttributes( rockContext );

            //
            // Store the data.
            //
            group.Name = tbFriendlyName.Text;
            group.SetAttributeValue( "RemoveOldCertificate", cbRemoveOldCertificate.Checked.ToString() );
            group.SetAttributeValue( "Domains", vlDomains.Value );
            group.SetAttributeValue( "Bindings", string.Join( "|", BindingsState ) );

            //
            // Save all the information.
            //
            rockContext.WrapTransaction( () =>
             {
                 rockContext.SaveChanges();

                 group.SaveAttributeValues( rockContext );
             } );

            NavigateToCurrentPage( new Dictionary<string, string> { { "Id", group.Id.ToString() } } );
        }

        /// <summary>
        /// Handles the Click event of the lbEditCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditCancel_Click( object sender, EventArgs e )
        {
            if ( PageParameter( "Id" ).AsInteger() == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowDetail();
            }
        }

        /// <summary>
        /// Handles the Add event of the gBindings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBindings_Add( object sender, EventArgs e )
        {
            mdEditBinding.SaveButtonText = "Add";
            mdEditBinding.Title = "Add Binding";

            hfEditBindingIndex.Value = string.Empty;
            ShowBinding( null );

            mdEditBinding.Show();
        }

        /// <summary>
        /// Handles the Delete event of the gBindings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBindings_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            BindingsState.RemoveAt( e.RowIndex );

            GridBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBindings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBindings_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            GridBind();
        }

        /// <summary>
        /// Handles the RowSelected event of the gBindings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBindings_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            mdEditBinding.SaveButtonText = "Save";
            mdEditBinding.Title = "Edit Binding";

            hfEditBindingIndex.Value = e.RowIndex.ToString();
            ShowBinding( BindingsState[e.RowIndex] );

            mdEditBinding.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEditBinding control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEditBinding_SaveClick( object sender, EventArgs e )
        {
            BindingData binding;

            //
            // Edit existing binding or create a new one.
            //
            if ( !string.IsNullOrWhiteSpace( hfEditBindingIndex.Value ) )
            {
                binding = BindingsState[hfEditBindingIndex.ValueAsInt()];
            }
            else
            {
                binding = new BindingData();
                BindingsState.Add( binding );
            }

            //
            // Set all the binding information.
            //
            binding.Site = ddlEditBindingSite.SelectedValue;
            binding.IPAddress = ddlEditBindingIPAddress.SelectedValue;
            binding.Port = nbEditBindingPort.Text.AsInteger();
            binding.Domain = tbEditBindingDomain.Text;

            mdEditBinding.Hide();
            GridBind();
        }

        /// <summary>
        /// Handles the Click event of the lbRecommendConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRecommendConfig_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var bindings = AcmeHelper.GetExistingBindings( null );
            string siteName = System.Web.Hosting.HostingEnvironment.SiteName;
            var siteBindings = bindings.Where( b => siteName.Equals( b.Site, StringComparison.CurrentCultureIgnoreCase ) ).ToList();

            //
            // Get all the configured domain names in Rock.
            // Skip any domains with "/" in case they configured poorly.
            //
            var domainNames = vlDomains.Value.SplitDelimitedValues().ToList();
            var newDomainNames = new SiteDomainService( rockContext ).Queryable()
                .Select( d => d.Domain )
                .Where( d => !d.Contains( "/" ) )
                .Where( d => !d.Equals( "localhost", StringComparison.CurrentCultureIgnoreCase ) )
                .ToList();
            domainNames.AddRange( newDomainNames.Where( d => !domainNames.Any( a => a.ToLower() == d.ToLower() ) ) );

            //
            // Check if we have a default binding (i.e. blank domain).
            //
            bool hasDefaultBinding = siteBindings.Any( b => string.IsNullOrWhiteSpace( b.Domain ) );

            //
            // Make a list of all the site bindings on port 80 without a 443 binding.
            //
            var needSecureBindings = siteBindings.Where( b => b.Port == 80 )
                .Where( b => !siteBindings.Any( a => a.Site == b.Site && a.IPAddress == b.IPAddress && a.Domain == b.Domain && a.Port == 443 ) )
                .ToList();

            //
            // Add new 443 bindings for any of those port 80-only bindings.
            //
            foreach ( var binding in needSecureBindings )
            {
                var newBinding = new BindingData
                {
                    Site = binding.Site,
                    IPAddress = binding.IPAddress,
                    Domain = binding.Domain,
                    Port = 443
                };

                siteBindings.Add( newBinding );
            }

            //
            // Add SSL bindings for any domains that we don't have bindings for.
            //
            if ( !hasDefaultBinding )
            {
                foreach ( var domain in domainNames )
                {
                    if ( siteBindings.Any( b => domain.Equals( b.Domain, StringComparison.CurrentCultureIgnoreCase ) ) )
                    {
                        continue;
                    }

                    var newBinding = new BindingData
                    {
                        Site = siteName,
                        Domain = domain,
                        Port = 443
                    };

                    var firstExistingBinding = siteBindings.FirstOrDefault();
                    newBinding.IPAddress = firstExistingBinding != null ? firstExistingBinding.IPAddress : string.Empty;

                    //
                    // Make sure we would not be generating a binding that already exists in another site.
                    //
                    if ( bindings.Any( b => b.IPAddress == newBinding.IPAddress && b.Port == newBinding.Port && newBinding.Domain.Equals( b.Domain, StringComparison.CurrentCultureIgnoreCase ) ) )
                    {
                        continue;
                    }

                    siteBindings.Add( newBinding );
                }
            }

            //
            // Filter down to just the 443 bindings, thats all we care about.
            //
            siteBindings = siteBindings.Where( b => b.Port == 443 ).ToList();

            //
            // Add any new domain names.
            //
            vlDomains.Value = string.Join( "|", domainNames );

            //
            // Add any new bindings.
            //
            var newBindings = siteBindings.Where( b => !BindingsState.Any( a => a.Site.ToLower() == b.Site.ToLower() && a.Domain.ToLower() == b.Domain.ToLower() && a.IPAddress == b.IPAddress && a.Port == b.Port ) );
            BindingsState.AddRange( newBindings );
            GridBind();
        }

        #endregion

        #region Renew Event Methods

        /// <summary>
        /// Handles the Click event of the lbRenewCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRenewCancel_Click( object sender, EventArgs e )
        {
            pnlRenew.Visible = false;
            pnlDetail.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbRenewDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRenewDone_Click( object sender, EventArgs e )
        {
            pnlRenew.Visible = false;
            NavigateToCurrentPage( new Dictionary<string, string> { { "Id", PageParameter( "Id" ) } } );
        }

        #endregion
    }
}
