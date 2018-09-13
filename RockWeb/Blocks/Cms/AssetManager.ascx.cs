using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                        return string.Format( "{{ \"AssetStorageSystemId\": \"{0}\", \"Key\": \"{1}\" }}", lbAssetStorageId.Text, keyControl.Text );
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
        public event EventHandler SelectItem;

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
    data.formData = {{ StorageId: storageId, Key: assetKey, IsAssetStorageSystemAsset: true }};
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
this.ResolveUrl( "~/api/AssetStorageSystems/GetChildren?assetFolderId=" ), // {0}
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

        /// <summary>
        /// Lists the files for the selected folder.
        /// </summary>
        protected void ListFiles()
        {
            AssetStorageSystem assetStorageSystem = GetAssetStorageSystem();
            if ( assetStorageSystem == null )
            {
                return;
            }

            var component = assetStorageSystem.GetAssetStorageComponent();
            if ( component == null )
            {
                return;
            }

            var files = component.ListFilesInFolder( assetStorageSystem, new Asset { Key = lbSelectFolder.Text, Type = AssetType.Folder } );
            rptFiles.DataSource = files;
            rptFiles.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the lbDownload control.
        /// Downloads the file and propts user to save or open.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDownload_Click( object sender, EventArgs e )
        {
            AssetStorageSystem assetStorageSystem = GetAssetStorageSystem();
            var component = assetStorageSystem.GetAssetStorageComponent();

            foreach ( RepeaterItem file in rptFiles.Items )
            {
                var cbEvent = file.FindControl( "cbSelected" ) as RockCheckBox;
                if ( cbEvent.Checked == true )
                {
                    var keyControl = file.FindControl( "lbKey" ) as Label;
                    string key = keyControl.Text;
                    Asset asset = component.GetObject( assetStorageSystem, new Asset { Key = key, Type = AssetType.File } );

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
            AssetStorageSystem assetStorageSystem = GetAssetStorageSystem();
            var component = assetStorageSystem.GetAssetStorageComponent();

            foreach ( RepeaterItem file in rptFiles.Items )
            {
                var cbEvent = file.FindControl( "cbSelected" ) as RockCheckBox;
                if ( cbEvent.Checked == true )
                {
                    var keyControl = file.FindControl( "lbKey" ) as Label;
                    string key = keyControl.Text;
                    component.DeleteAsset( assetStorageSystem, new Asset { Key = key, Type = AssetType.File } );
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
            AssetStorageSystem assetStorageSystem = GetAssetStorageSystem();
            var component = assetStorageSystem.GetAssetStorageComponent();
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

            component.CreateFolder( assetStorageSystem, asset );
            upnlFolders.Update();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteFolder_Click( object sender, EventArgs e )
        {
            AssetStorageSystem assetStorageSystem = GetAssetStorageSystem();
            var component = assetStorageSystem.GetAssetStorageComponent();
            component.DeleteAsset( assetStorageSystem, new Asset { Key = lbSelectFolder.Text, Type = AssetType.Folder } );

            lbSelectFolder.Text = string.Empty;
            upnlFolders.Update();
            // TODO: select the parent of the folder just deleted and list the files
        }

        /// <summary>
        /// Gets the asset storage system using the ID stored in the hidden field, otherwise returns a new AssetStorageSystem.
        /// </summary>
        /// <returns></returns>
        private AssetStorageSystem GetAssetStorageSystem()
        {
            AssetStorageSystem assetStorageSystem = new AssetStorageSystem();
            string assetStorageId = lbAssetStorageId.Text;

            if ( assetStorageId.IsNotNullOrWhiteSpace() )
            {
                var assetStorageService = new AssetStorageSystemService( new RockContext() );
                assetStorageSystem = assetStorageService.Get( assetStorageId.AsInteger() );
                assetStorageSystem.LoadAttributes();
            }

            return assetStorageSystem;
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

        /// <summary>
        /// Handles the Click event of the lbRenameFileAccept control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRenameFileAccept_Click( object sender, EventArgs e )
        {
            AssetStorageSystem assetStorageSystem = GetAssetStorageSystem();
            var component = assetStorageSystem.GetAssetStorageComponent();

            foreach ( RepeaterItem repeaterItem in rptFiles.Items )
            {
                var cbEvent = repeaterItem.FindControl( "cbSelected" ) as RockCheckBox;
                if ( cbEvent.Checked == true )
                {
                    var keyControl = repeaterItem.FindControl( "lbKey" ) as Label;
                    string key = keyControl.Text;
                    component.RenameAsset( assetStorageSystem, new Asset { Key = key, Type = AssetType.File }, tbRenameFile.Text );
                }
            }

            ListFiles();
        }
    }
}