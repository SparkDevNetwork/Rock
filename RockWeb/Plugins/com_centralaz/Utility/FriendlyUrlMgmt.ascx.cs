// <copyright>
// Copyright by Central Christian Church
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
using System.Data;
using Microsoft.Web.Administration;


namespace RockWeb.Plugins.com_centralaz.Utility
{
    /// <summary>
    /// This block allows users to view and edit friendly urls stored in IIS. This block requires the Microsoft.Web.Administration dll.
    /// </summary>
    [DisplayName( "Friendly Url Management" )]
    [Category( "com_centralaz > Utility" )]
    [Description( "Block that allows users to view and edit friendly urls stored in IIS. This block requires the Microsoft.Web.Administration dll." )]
    [TextField( "Base Web URL", "This is the base URL for your public website", true, "" )]
    [TextField( "IIS Website Name", "This is the exact name of the IIS website you are accessing. (Default: 'Default Web Site')", false, "RockWeb" )]
    [TextField( "Physical Folder Location", @"This is the parent folder for all of the IIS Virtual Directory physical folders. (Default: 'C:\inetpub\_RedirectedSites\' ", false, @"C:\inetpub\_RedirectedSites\" )]
    public partial class FriendlyUrlMgmt : Rock.Web.UI.RockBlock
    {
        #region Fields

        bool _canEdit;

        #endregion

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

            gVirtualDirectory.GridRebind += gVirtualDirectory_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _canEdit = this.IsUserAuthorized( Rock.Security.Authorization.EDIT );

            if ( !Page.IsPostBack )
            {
                BindGrid();
                lblProcessReport.Text = string.Empty;
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

        }

        private void gVirtualDirectory_GridRebind( object sender, EventArgs e )
        {
            DataTable dtVirtualDirectories = GetVirtualDirectories();
            gVirtualDirectory.DataSource = dtVirtualDirectories;
            gVirtualDirectory.DataBind();
        }

        /// <summary>
        /// This was the event handler that performed the bulk update described below.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnChangeStatus_Click( object sender, EventArgs e )
        {
            ChangeEventIdVirtualsToTemporary();
        }

        protected void ddlFriendlyUrls_SelectedIndexChanged( object sender, EventArgs e )
        {
            lblProcessReport.Text = string.Empty;
            lblOutput.Text = string.Empty;

            DataTable dtVirtualDirectories = GetVirtualDirectories();

            if ( ddlFriendlyUrls.SelectedValue != "new" )
            {
                foreach ( DataRow dataRow in dtVirtualDirectories.Rows )
                {
                    if ( dataRow["FriendlyURL"].ToString() == ddlFriendlyUrls.SelectedValue )
                    {
                        FriendlyUrlName.Text = dataRow["FriendlyURL"].ToString();
                        RedirectDestination.Text = dataRow["Destination"].ToString();
                        ExactDestination.Checked = (bool)dataRow["ExactDestination"];
                        ChildOnly.Checked = (bool)dataRow["ChildOnly"];
                        ddlStatusCode.SelectedValue = dataRow["HttpResponseStatus"].ToString();
                        break;
                    }
                }
            }
            else
            {
                ClearForm();
            }
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ClearForm();
        }

        protected void btnDelete_Click( object sender, EventArgs e )
        {
            bool proceed = true;
            if ( string.IsNullOrEmpty( FriendlyUrlName.Text ) )
            {
                proceed = false;
            }

            if ( ( ddlFriendlyUrls.SelectedValue != "new" ) && ( proceed == true ) )
            {
                string basePath = GetAttributeValue( "PhysicalFolderLocation" );
                string vDirPath = "/" + FriendlyUrlName.Text;

                ServerManager serverManager = new ServerManager();
                Application app = serverManager.Sites[GetAttributeValue( "IISWebsiteName" )].Applications["/"];
                VirtualDirectory vDir = app.VirtualDirectories[vDirPath];
                app.VirtualDirectories.Remove( vDir );
                serverManager.CommitChanges();

                Directory.Delete( vDir.PhysicalPath, true );

                BindGrid();
            }
            else
            {
                lblProcessReport.Text = "No Friendly URL selected.";
                ClearForm();
            }
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            DataTable dtVirtualDirectories = GetVirtualDirectories();
            bool newVanityUrl = true;

            if ( !string.IsNullOrEmpty( FriendlyUrlName.Text ) && ( ddlFriendlyUrls.SelectedValue == "new" ) && !DirectoryExists( FriendlyUrlName.Text ) )
            {
                foreach ( DataRow dataRow in dtVirtualDirectories.Rows )
                {
                    if ( dataRow["FriendlyURL"].ToString() == FriendlyUrlName.Text )
                    {
                        lblOutput.Text = "Virtual Directory " + FriendlyUrlName.Text + " already exists!";
                        newVanityUrl = false;
                        break;
                    }
                }

                if ( newVanityUrl )
                {
                    string basePath = GetAttributeValue( "PhysicalFolderLocation" );
                    string fullPath = basePath + FriendlyUrlName.Text;
                    Directory.CreateDirectory( fullPath );

                    string vDirPath = "/" + FriendlyUrlName.Text;
                    ServerManager serverManager = new ServerManager();
                    Application app = serverManager.Sites[GetAttributeValue( "IISWebsiteName" )].Applications["/"];
                    app.VirtualDirectories.Add( vDirPath, fullPath );
                    serverManager.CommitChanges();

                    CreateHttpRedirectSettings( vDirPath );

                    BindGrid();
                }
            }
            else
            {
                if ( !string.IsNullOrEmpty( FriendlyUrlName.Text ) )
                {
                    lblOutput.Text = "Not new or " + FriendlyUrlName.Text + " exists.";
                    string vDirPath = "/" + FriendlyUrlName.Text;
                    if ( DirectoryExists( FriendlyUrlName.Text ) )
                    {
                        CreateHttpRedirectSettings( vDirPath );
                        BindGrid();
                    }
                    else
                    {
                        lblProcessReport.Text = "ERROR: Physical folder does not exist.";
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void LoadFriendlyUrlDdl( DataTable dtVirtualDirectories )
        {
            ddlFriendlyUrls.Items.Clear();
            ListItem i = new ListItem( "<add new>", "new" );
            ddlFriendlyUrls.Items.Add( i );
            ddlFriendlyUrls.DataSource = dtVirtualDirectories;
            ddlFriendlyUrls.DataTextField = dtVirtualDirectories.Columns["FriendlyURL"].ToString();
            ddlFriendlyUrls.DataBind();
            ddlFriendlyUrls.SelectedValue = "new";
        }

        private void ClearForm()
        {
            ddlFriendlyUrls.SelectedValue = "new";
            FriendlyUrlName.Text = string.Empty;
            RedirectDestination.Text = string.Empty;
            ExactDestination.Checked = true;
            ChildOnly.Checked = false;
            ddlStatusCode.SelectedValue = "Temporary";
        }

        private bool DirectoryExists( string directoryToFind )
        {
            bool directoryFound = false;
            string basePath = GetAttributeValue( "PhysicalFolderLocation" );
            string fullPath = basePath + directoryToFind;

            foreach ( string d in Directory.GetDirectories( basePath ) )
            {
                if ( d == fullPath )
                {
                    directoryFound = true;
                }
            }

            return directoryFound;
        }

        /// <summary>
        /// This special little functional method was used to bulk update all Friendly URLs
        /// that had a destination with "eventid" to have a status code of "Temporary" --
        /// because those sorts of destinations are essentially temporary (and definitely not
        /// permanent).
        /// </summary>
        private void ChangeEventIdVirtualsToTemporary()
        {
            DataTable dtVirtualDirectories = GetVirtualDirectories();
            foreach ( DataRow dataRow in dtVirtualDirectories.Rows )
            {
                // skip if not contains tolower eventid
                if ( !dataRow["Destination"].ToString().ToLower().Contains( "eventid" ) )
                {
                    continue;
                }

                string vDirPath = "/" + dataRow["FriendlyURL"];

                ServerManager serverManager = new ServerManager();

                Configuration config = serverManager.GetWebConfiguration( GetAttributeValue( "IISWebsiteName" ) + vDirPath );
                ConfigurationSection httpRedirectSection = config.GetSection( "system.webServer/httpRedirect" );

                httpRedirectSection["enabled"] = "true";
                httpRedirectSection["destination"] = dataRow["Destination"];
                httpRedirectSection["exactDestination"] = dataRow["ExactDestination"].ToString();
                httpRedirectSection["childOnly"] = dataRow["ChildOnly"].ToString();
                httpRedirectSection["httpResponseStatus"] = "Temporary";

                serverManager.CommitChanges();
                lblProcessReport.Text += string.Format( "<br>{0}", vDirPath );
            }
        }

        private DataTable GetVirtualDirectories()
        {
            DataTable dtFriendlyURLS;

            if ( Cache["dtFriendlyURLS"] == null )
            {
                ServerManager serverManager = new ServerManager();
                Application app = serverManager.Sites[GetAttributeValue( "IISWebsiteName" )].Applications["/"];

                dtFriendlyURLS = new DataTable();
                dtFriendlyURLS.Columns.Add( new DataColumn( "FriendlyURL", typeof( System.String ) ) );
                dtFriendlyURLS.Columns.Add( new DataColumn( "Destination", typeof( System.String ) ) );
                dtFriendlyURLS.Columns.Add( new DataColumn( "ExactDestination", typeof( System.Boolean ) ) );
                dtFriendlyURLS.Columns.Add( new DataColumn( "ChildOnly", typeof( System.Boolean ) ) );
                dtFriendlyURLS.Columns.Add( new DataColumn( "HttpResponseStatus", typeof( System.String ) ) );

                var vDs = ( from vDir in app.VirtualDirectories
                            where vDir.Path != "/"
                            orderby vDir.Path
                            select vDir );

                foreach ( VirtualDirectory vD in vDs )
                {
                    Configuration config = serverManager.GetWebConfiguration( GetAttributeValue( "IISWebsiteName" ) + vD.Path );
                    ConfigurationSection httpRedirectSection = config.GetSection( "system.webServer/httpRedirect" );

                    DataRow dRow = dtFriendlyURLS.NewRow();
                    char[] charsToTrim = { '/' };
                    dRow["FriendlyURL"] = vD.Path.ToString().Trim( charsToTrim );
                    dRow["Destination"] = httpRedirectSection["destination"].ToString();
                    dRow["ExactDestination"] = (bool)httpRedirectSection["exactDestination"];
                    dRow["ChildOnly"] = (bool)httpRedirectSection["childOnly"];
                    switch ( httpRedirectSection["httpResponseStatus"].ToString() )
                    {
                        case "301":
                            dRow["HttpResponseStatus"] = "Permanent";
                            break;
                        case "302":
                            dRow["HttpResponseStatus"] = "Found";
                            break;
                        case "307":
                            dRow["HttpResponseStatus"] = "Temporary";
                            break;
                    }

                    dtFriendlyURLS.Rows.Add( dRow );
                }

                Cache.Add( "dtFriendlyURLS", dtFriendlyURLS, null, DateTime.Now.AddMinutes( 3 ), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null );
            }
            else
            {
                dtFriendlyURLS = (DataTable)Cache["dtFriendlyURLS"];
            }

            return dtFriendlyURLS;
        }

        private void BindGrid()
        {
            Cache.Remove( "dtFriendlyURLS" );

            DataTable dtVirtualDirectories = GetVirtualDirectories();
            LoadFriendlyUrlDdl( dtVirtualDirectories );
            gVirtualDirectory.DataSource = dtVirtualDirectories;
            gVirtualDirectory.DataBind();
            ClearForm();

            lblProcessReport.Text = "Process Complete.";
        }

        private void CreateHttpRedirectSettings( string vDirPath )
        {
            ServerManager serverManager = new ServerManager();

            Configuration config = serverManager.GetWebConfiguration( GetAttributeValue( "IISWebsiteName" ) + vDirPath );
            ConfigurationSection httpRedirectSection = config.GetSection( "system.webServer/httpRedirect" );
            httpRedirectSection["enabled"] = "true";
            httpRedirectSection["destination"] = RedirectDestination.Text;
            httpRedirectSection["exactDestination"] = ExactDestination.Checked.ToString();
            httpRedirectSection["childOnly"] = ChildOnly.Checked.ToString();
            httpRedirectSection["httpResponseStatus"] = ddlStatusCode.SelectedValue;

            serverManager.CommitChanges();
        }

        #endregion
    }
}