// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Linq;
using System.Collections.Generic;
using Rock.Model;
using Rock.Web.UI;
using Rock.Attribute;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "CkEditor FileBrowser" )]
    [Category( "Utility" )]
    [Description( "Block to be used as part of the RockFileBrowser CKEditor Plugin" )]
    [TextField( "Root Folder", "The root folder of the Folder Browser")]
    public partial class CkEditorFileBrowser : RockBlock
    {
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

            if ( !this.Page.IsPostBack )
            {
                
                string startupScript = @" 
// init rockTree on folder (no options since we are generating static html)
$('.treeview').rockTree( {} );

// init scroll bars for folder and file list divs
$('.js-folder-treeview').tinyscrollbar({ size: 120, sizethumb: 20 });
$('.js-file-list').tinyscrollbar({ size: 120, sizethumb: 20 });

$('.treeview').on('rockTree:expand rockTree:collapse rockTree:dataBound rockTree:rendered', function (evt) {
  // update the folder treeview scroll bar
  $('.js-folder-treeview').tinyscrollbar_update('relative');
});

";
                ScriptManager.RegisterStartupScript( this, this.GetType(), "script_" + this.ID, startupScript, true );
            }

            var sb = new StringBuilder();

            sb.AppendLine( "<ul id=\"treeview\">" );

            

            // the rootFolder param is encrypted to help prevent the web user from specifying a folder
            string rootFolder = PageParameter( "rootFolder" );
            if (!string.IsNullOrWhiteSpace(rootFolder) ) {
                rootFolder = Rock.Security.Encryption.DecryptString( rootFolder );
            }

            // default to the "~/Content" if the rootFolder is not specified
            if ( string.IsNullOrWhiteSpace( rootFolder ) )
            {
                rootFolder = "~/Content";
            }

            // ensure that the folder is formatted to be relative to web root
            if (!rootFolder.StartsWith("~/"))
            {
                rootFolder = "~/" + rootFolder;
            }

            string physicalRootFolder = this.Request.MapPath( rootFolder );
            if ( Directory.Exists( physicalRootFolder ) )
            {
                List<string> directoryList = Directory.GetDirectories( physicalRootFolder ).OrderBy( a => a ).ToList();

                foreach ( string directoryPath in directoryList )
                {
                    sb.Append( DirectoryNode( directoryPath ) );
                }

                sb.AppendLine( "</ul>" );

                lblFolders.Text = sb.ToString();
            }
            else
            {
                nbWarning.Title = "Warning";
                nbWarning.Text = "Folder does not exist: " + physicalRootFolder;
                nbWarning.Visible = true;
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the PageLiquid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            
        }

        #endregion

        #region Methods

        protected string DirectoryNode( string directoryPath )
        {
            var sb = new StringBuilder();

            DirectoryInfo directoryInfo = new DirectoryInfo( directoryPath );

            sb.AppendFormat( "<li data-expanded='false' data-id='{0}'><span><i class=\"fa fa-folder\"></i> {1}</span> \n", directoryPath, directoryInfo.Name );

            List<string> subDirectoryList = Directory.GetDirectories( directoryPath ).OrderBy( a => a ).ToList();

            if ( subDirectoryList.Any() )
            {
                sb.AppendLine( "<ul>" );

                foreach ( var subDirectoryPath in subDirectoryList )
                {
                    sb.Append( DirectoryNode( subDirectoryPath ) );
                }

                sb.AppendLine( "</ul>" );
            }

            sb.AppendLine( "</li>" );

            return sb.ToString();
        }

        #endregion
    }
}