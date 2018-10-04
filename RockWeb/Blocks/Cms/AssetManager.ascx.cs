﻿// <copyright>
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
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Storage.AssetStorage;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Asset Manager" )]
    [Category( "Core" )]
    [Description( "Manage files stored on a remote server or 3rd party cloud storage" )]
    public partial class AssetManager : RockBlock, IPickerBlock
    {
        #region IPicker Implementation
        /// <summary>
        /// The selected value will be returned as a URL. For 3rd party cloud services a presigned URL must be created
        /// for the file to be publicly available.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                if ( lbAssetStorageId.Text.IsNullOrWhiteSpace() )
                {
                    return string.Empty;
                }

                foreach ( RepeaterItem repeaterItem in rptFiles.Items )
                {
                    var cbEvent = repeaterItem.FindControl( "cbSelected" ) as RockCheckBox;
                    if ( cbEvent.Checked == true )
                    {
                        var keyControl = repeaterItem.FindControl( "lbKey" ) as Label;
                        return string.Format( "{{ \"AssetStorageProviderId\": \"{0}\", \"Key\": \"{1}\" }}", lbAssetStorageId.Text, keyControl.Text );
                    }
                }

                return string.Empty;
            }

            set
            {
            }
        }

        /// <summary>
        /// Any Picker Settings that be configured. There are no settings for this block.
        /// </summary>
        /// <value>
        /// The picker settings.
        /// </value>
        public Dictionary<string, string> PickerSettings
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        /// </exception>
        event EventHandler IPickerBlock.SelectItem
        {
            add
            {
                // not implemented
            }

            remove
            {
                // not implemented
            }
        }

        /// <summary>
        /// Gets or sets the selected text.
        /// </summary>
        /// <value>
        /// The selected text.
        /// </value>
        public string SelectedText
        {
            get
            {
                foreach ( RepeaterItem repeaterItem in rptFiles.Items )
                {
                    var cbEvent = repeaterItem.FindControl( "cbSelected" ) as RockCheckBox;
                    if ( cbEvent.Checked == true )
                    {
                        var keyControl = repeaterItem.FindControl( "lbName" ) as Label;
                        return keyControl.Text;
                    }
                }

                return string.Empty;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets the text representing the selected item.
        /// Ignores the input and gets the value from the SelectedText property.
        /// </summary>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        /// <value>
        /// The selected text.
        /// </value>
        public string GetSelectedText( string selectedValue )
        {
            return SelectedText;
        }

        #endregion IPicker Implementation

        #region Control Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// Creating the js here because when this block is used as a field type then depending on the parent block the
        /// js needed at load time isn't run unless it goes through the ScriptManager.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( lbDownload );

            fupUpload.FileUploaded += fupUpload_FileUploaded;

            string submitScriptFormat = @"// include in the post to ~/FileUploader.ashx
    var assetKey = $('#{0}').text();
    var storageId = $('#{1}').text();
    data.formData = {{ StorageId: storageId, Key: assetKey, IsAssetStorageProviderAsset: true }};
";

            // setup javascript for when a file is submitted
            fupUpload.SubmitFunctionClientScript = string.Format( submitScriptFormat, lbSelectFolder.ClientID, lbAssetStorageId.ClientID );

            string doneScriptFormat = @"// reselect the node to refresh the list of files
    var selectedFolderPath =  $('#{1}').text() + ',' + $('#{0}').text();
    var foldersTree = $('.js-folder-treeview .treeview').data('rockTree');
    foldersTree.$el.trigger('rockTree:selected', selectedFolderPath);
";
            // setup javascript for when a file is done uploading
            fupUpload.DoneFunctionClientScript = string.Format( doneScriptFormat, lbSelectFolder.ClientID, lbAssetStorageId.ClientID );

            var folderTreeScript = string.Format( @"
Sys.Application.add_load(function () {{
    Rock.controls.assetManager.initialize({{
        restUrl: '{0}',
        controlId: '{1}',
        filesUpdatePanelId: '{2}'
    }});
}});
",
this.ResolveUrl( "~/api/AssetStorageProviders/GetChildren?assetFolderId=" ), // {0}
pnlAssetManager.ClientID, // {1}
upnlFiles.ClientID // {2}
);

            var scriptInitialized = this.Request.Params[hfScriptInitialized.UniqueID].AsBoolean();

            if ( !scriptInitialized )
            {
                ScriptManager.RegisterStartupScript( this, this.GetType(), string.Format( "AssetManager_js_init_{0}", this.ClientID ), folderTreeScript, true );
                hfScriptInitialized.Value = true.ToString();
                upnlFolders.Update();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            var hasAssetStorageId = lbAssetStorageId.Text.IsNotNullOrWhiteSpace();

            if ( !this.IsPostBack || !hasAssetStorageId )
            {
                lbAssetStorageId.Text = "-1";
                return;
            }

            fupUpload.Enabled = true;

            // handle custom postback events
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string previousAssetSelected = string.Empty;

                string[] args = postbackArgs.Split( new char[] { '?' } );
                foreach ( string arg in args )
                {
                    string[] nameValue = arg.Split( new char[] { ':' } );
                    string eventParam = nameValue[0];

                    switch ( eventParam )
                    {
                        case "folder-selected":
                            lbSelectFolder.Text = nameValue[1];
                            break;
                        case "storage-id":
                            lbAssetStorageId.Text = nameValue[1];
                            break;
                        case "expanded-folders":
                            lbExpandedFolders.Text = nameValue[1];
                            break;
                        default:
                            break;
                    }
                }

                ListFiles();
            }
        }

        #endregion Control Overrides

        #region control events
        /// <summary>
        /// Handles the Click event of the lbDownload control.
        /// Downloads the file and propts user to save or open.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDownload_Click( object sender, EventArgs e )
        {
            AssetStorageProvider assetStorageProvider = GetAssetStorageProvider();
            var component = assetStorageProvider.GetAssetStorageComponent();

            foreach ( RepeaterItem file in rptFiles.Items )
            {
                var cbEvent = file.FindControl( "cbSelected" ) as RockCheckBox;
                if ( cbEvent.Checked == true )
                {
                    var keyControl = file.FindControl( "lbKey" ) as Label;
                    string key = keyControl.Text;
                    Asset asset = component.GetObject( assetStorageProvider, new Asset { Key = key, Type = AssetType.File } );

                    byte[] bytes = asset.AssetStream.ReadBytesToEnd();

                    Response.ContentType = "application/octet-stream";
                    Response.AddHeader( "content-disposition", "attachment; filename=" + asset.Name );
                    Response.BufferOutput = true;
                    Response.BinaryWrite( bytes );
                    Response.End();
                }
            }

            ListFiles();
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// Deletes the checked files.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            AssetStorageProvider assetStorageProvider = GetAssetStorageProvider();
            var component = assetStorageProvider.GetAssetStorageComponent();

            foreach ( RepeaterItem file in rptFiles.Items )
            {
                var cbEvent = file.FindControl( "cbSelected" ) as RockCheckBox;
                if ( cbEvent.Checked == true )
                {
                    var keyControl = file.FindControl( "lbKey" ) as Label;
                    string key = keyControl.Text;
                    component.DeleteAsset( assetStorageProvider, new Asset { Key = key, Type = AssetType.File } );
                }
            }

            ListFiles();
        }

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// Refreshes the list of flles.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            ListFiles();
        }

        /// <summary>
        /// Handles the Click event of the lbCreateFolderAccept control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCreateFolderAccept_Click( object sender, EventArgs e )
        {
            if ( !IsValidName(tbCreateFolder.Text) || tbCreateFolder.Text.IsNullOrWhiteSpace() )
            {
                return;
            }

            AssetStorageProvider assetStorageProvider = GetAssetStorageProvider();
            var component = assetStorageProvider.GetAssetStorageComponent();
            var asset = new Asset { Type = AssetType.Folder };

            // Selecting the root does not put a value for the selected folder, so we have to make sure
            // if it does not have a value that we use name instead of key so the root folder is used
            // by the component.
            if ( lbSelectFolder.Text.IsNotNullOrWhiteSpace() )
            {
                asset.Key = lbSelectFolder.Text + tbCreateFolder.Text;
            }
            else
            {
                asset.Name = tbCreateFolder.Text;
            }

            component.CreateFolder( assetStorageProvider, asset );
            upnlFolders.Update();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteFolder_Click( object sender, EventArgs e )
        {
            AssetStorageProvider assetStorageProvider = GetAssetStorageProvider();
            var component = assetStorageProvider.GetAssetStorageComponent();
            component.DeleteAsset( assetStorageProvider, new Asset { Key = lbSelectFolder.Text, Type = AssetType.Folder } );

            lbSelectFolder.Text = string.Empty;
            upnlFolders.Update();
            // TODO: select the parent of the folder just deleted and list the files
        }

        /// <summary>
        /// Handles the Click event of the lbRenameFileAccept control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRenameFileAccept_Click( object sender, EventArgs e )
        {
            if ( !IsValidName( tbRenameFile.Text ) || tbRenameFile.Text.IsNullOrWhiteSpace() )
            {
                return;
            }

            AssetStorageProvider assetStorageProvider = GetAssetStorageProvider();
            var component = assetStorageProvider.GetAssetStorageComponent();

            foreach ( RepeaterItem repeaterItem in rptFiles.Items )
            {
                var cbEvent = repeaterItem.FindControl( "cbSelected" ) as RockCheckBox;
                if ( cbEvent.Checked == true )
                {
                    var keyControl = repeaterItem.FindControl( "lbKey" ) as Label;
                    string key = keyControl.Text;
                    component.RenameAsset( assetStorageProvider, new Asset { Key = key, Type = AssetType.File }, tbRenameFile.Text );
                }
            }

            ListFiles();
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fupUpload_FileUploaded( object sender, EventArgs e )
        {
            ListFiles();
        }

        #endregion control events

        #region private methods
        /// <summary>
        /// Lists the files for the selected folder.
        /// </summary>
        private void ListFiles()
        {
            AssetStorageProvider assetStorageProvider = GetAssetStorageProvider();
            if ( assetStorageProvider == null )
            {
                return;
            }

            var component = assetStorageProvider.GetAssetStorageComponent();
            if ( component == null )
            {
                return;
            }

            var files = component.ListFilesInFolder( assetStorageProvider, new Asset { Key = lbSelectFolder.Text, Type = AssetType.Folder } );
            rptFiles.DataSource = files;
            rptFiles.DataBind();
        }

        /// <summary>
        /// Gets the asset storage provider using the ID stored in the hidden field, otherwise returns a new AssetStorageProvider.
        /// </summary>
        /// <returns></returns>
        private AssetStorageProvider GetAssetStorageProvider()
        {
            AssetStorageProvider assetStorageProvider = new AssetStorageProvider();
            string assetStorageId = lbAssetStorageId.Text;

            if ( assetStorageId.IsNotNullOrWhiteSpace() )
            {
                var assetStorageService = new AssetStorageProviderService( new RockContext() );
                assetStorageProvider = assetStorageService.Get( assetStorageId.AsInteger() );
                assetStorageProvider.LoadAttributes();
            }

            return assetStorageProvider;
        }

        /// <summary>
        /// Determines whether [is valid folder name] [the specified rename folder name].
        /// </summary>
        /// <param name="renameFolderName">Name of the rename folder.</param>
        /// <returns></returns>
        private bool IsValidName( string renameFolderName )
        {
            Regex regularExpression = new Regex( "^([^*/><?\\|:,]).*$" );
            return regularExpression.IsMatch( renameFolderName );
        }

        #endregion private methods
    }
}