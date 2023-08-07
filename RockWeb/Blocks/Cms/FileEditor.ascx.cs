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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "File Manager" )]
    [Category( "CMS" )]
    [Description( "Block that can be used to browse and manage files on the web server" )]

    #region Block Attributes

    [TextField(
        "Relative File Path",
        Description = "The relative path to file",
        IsRequired = false,
        Key = AttributeKey.RelativeFilePath )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387" )]
    public partial class FileEditor : RockBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Just in case something passes in specific configuration in the URL
        /// </summary>
        private static class PageParameterKey
        {
            public const string RelativeFilePath = "RelativeFilePath";
        }

        #endregion Page Parameter Keys

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RelativeFilePath = "RelativeFilePath";
        }

        #endregion Attribute Keys

        #region Fields

        string _fileRelativePath = string.Empty;

        #endregion

        #region Properties

        private Dictionary<string, CodeEditorMode> EditorMode
        {
            get
            {
                return new Dictionary<string, CodeEditorMode>()
                {
                    { ".ascx", CodeEditorMode.Html},
                    { ".cs",CodeEditorMode.CSharp},
                    { ".html",CodeEditorMode.Html},
                    {".less",CodeEditorMode.Css },
                    {".css",CodeEditorMode.Css },
                    {".lava",CodeEditorMode.Lava }
                };
            }
        }

        private List<string> RestrictedFileExtension
        {
            get
            {
                return new List<string>()
                {
                    ".bin",
                    ".png",
                    ".jpg",
                    ".ico",
                    ".jpeg",
                    ".config",
                    ".eot",
                    ".woff",
                    ".woff2"
                };
            }
        }

        #endregion

        #region Control Methods

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _fileRelativePath = GetRelativePath();

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var fileRelativePath = hfRelativePath.Value;
            if ( string.IsNullOrWhiteSpace( fileRelativePath ) )
            {
                nbWarningMessage.Text = "Error while saving the content back to file";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                return;
            }

            File.WriteAllText( Server.MapPath( _fileRelativePath ), ceFilerEditor.Text );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> queryString = new Dictionary<string, string>();
            var relativePath = PageParameter( PageParameterKey.RelativeFilePath );
            if ( relativePath.IsNotNullOrWhiteSpace() )
            {
                queryString.Add( PageParameterKey.RelativeFilePath, relativePath );
            }
            NavigateToParentPage( queryString );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            nbWarningMessage.Visible = false;
            pnlEditDetails.Visible = false;
            string fileUrl = Server.MapPath( _fileRelativePath );
            if ( string.IsNullOrWhiteSpace( _fileRelativePath )  || !File.Exists( fileUrl ) )
            {
                nbWarningMessage.Text = "Invalid file relative path";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            string ext = Path.GetExtension( _fileRelativePath );
            if ( RestrictedFileExtension.Any( a => ext.Equals( a, StringComparison.OrdinalIgnoreCase ) ) )
            {
                nbWarningMessage.Text = "viewing/editing the file is not allowed.";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            hfRelativePath.Value = _fileRelativePath;
            pnlEditDetails.Visible = true;
            if ( EditorMode.ContainsKey( ext.ToLower() ) )
            {
                ceFilerEditor.EditorMode = EditorMode[ext];
            }
            else
            {
                ceFilerEditor.EditorMode = CodeEditorMode.Text;
            }

            string fileName = Path.GetFileName( _fileRelativePath );
            
            string content = File.ReadAllText( fileUrl );

            if ( !string.IsNullOrEmpty( content ) )
            {
                ceFilerEditor.Text = content;
            }
        }

        /// <summary>
        /// Determines which item to edit based on either the configuration or the relativeFilePath that was passed in.
        /// </summary>
        private string GetRelativePath()
        {
            string relativePath = GetAttributeValue( AttributeKey.RelativeFilePath );

            // A configured defined type takes precedence over any definedTypeId param value that is passed in.
            if ( string.IsNullOrWhiteSpace( relativePath ) )
            {
                relativePath = PageParameter( PageParameterKey.RelativeFilePath );
            }

            return relativePath;
        }

        #endregion
    }
}