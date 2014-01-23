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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for rendering an html editor
    /// </summary>
    [ToolboxData( "<{0}:HtmlEditor runat=server></{0}:HtmlEditor>" )]
    public class HtmlEditor : TextBox, IRockControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }

            set
            {
                base.ValidationGroup = value;
                RequiredFieldValidator.ValidationGroup = value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public enum ToolbarConfig
        {
            /// <summary>
            /// A lighter more airy view
            /// </summary>
            Light,

            /// <summary>
            /// The full monty
            /// </summary>
            Full
        }

        /// <summary>
        /// Gets or sets the toolbar.
        /// </summary>
        /// <value>
        /// The toolbar.
        /// </value>
        public ToolbarConfig Toolbar
        {
            get
            {
                object toolbarObj = ViewState["Toolbar"];
                if ( toolbarObj != null )
                {
                    return (ToolbarConfig)toolbarObj;
                }
                else
                {
                    return ToolbarConfig.Light;
                }
            }

            set
            {
                ViewState["Toolbar"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum width of the resize.
        /// </summary>
        /// <value>
        /// The maximum width of the resize.
        /// </value>
        public int? ResizeMaxWidth
        {
            get
            {
                return ViewState["ResizeMaxWidth"] as int?;
            }

            set
            {
                ViewState["ResizeMaxWidth"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the custom javascript that will get executed when the ckeditor 'on change' event occurs
        /// </summary>
        /// <value>
        /// The custom on change press script.
        /// </value>
        public string OnChangeScript
        {
            get
            {
                return ViewState["OnChangeScript"] as string;
            }

            set
            {
                ViewState["OnChangeScript"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the document folder root.
        /// </summary>
        /// <value>
        /// The document folder root.
        /// </value>
        public string DocumentFolderRoot
        {
            get
            {
                return ViewState["DocumentFolderRoot"] as string;
            }

            set
            {
                ViewState["DocumentFolderRoot"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the image folder root.
        /// </summary>
        /// <value>
        /// The image folder root.
        /// </value>
        public string ImageFolderRoot
        {
            get
            {
                return ViewState["ImageFolderRoot"] as string;
            }

            set
            {
                ViewState["ImageFolderRoot"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the image file filter.
        /// </summary>
        /// <value>
        /// The image file filter.
        /// </value>
        public string ImageFileFilter
        {
            get
            {
                string result = ViewState["ImageFileFilter"] as string;
                if (string.IsNullOrWhiteSpace(result))
                {
                    // default to common ones that are supported by most browsers
                    result = "*.png;*.jpg;*.gif;*.svg;*.bmp";
                }
                
                return result;
            }

            set
            {
                ViewState["ImageFileFilter"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the merge fields to make available.  This should include either a list of
        /// entity type names (full name), or other non-object string values
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        public List<string> MergeFields
        {
            get
            {
                var mergeFields = ViewState["MergeFields"] as List<string>;
                if ( mergeFields == null )
                {
                    mergeFields = new List<string>();
                    ViewState["MergeFields"] = mergeFields;
                }

                return mergeFields;
            }

            set
            {
                ViewState["MergeFields"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlEditor"/> class.
        /// </summary>
        public HtmlEditor()
            : base()
        {
            RequiredFieldValidator = new RequiredFieldValidator();
            RequiredFieldValidator.ValidationGroup = this.ValidationGroup;
            HelpBlock = new HelpBlock();

            TextMode = TextBoxMode.MultiLine;
            Rows = 10;
            Columns = 80;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            // NOTE: Some of the plugins in the Full (72 plugin) build of CKEditor are buggy, so we are just using the Standard edition. 
            // This is why some of the items don't appear in the RockCustomConfiguFull toolbar (like the Justify commands)
            string ckeditorInitScriptFormat = @"
var toolbar_RockCustomConfigLight =
	[
        ['Source'],
        ['Bold', 'Italic', 'Underline', 'Strike', 'NumberedList', 'BulletedList', 'Link', 'Image', 'PasteFromWord', '-', 'RemoveFormat'],
        ['Format'], 
        ['rockmergefield', '-', 'rockimagebrowser', 'rockdocumentbrowser']
	];

var toolbar_RockCustomConfigFull =
	[
        ['Source'],
        ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'],
        ['Find', 'Replace', '-', 'Scayt'],
        ['Link', 'Unlink', 'Anchor'],
        ['Styles', 'Format'],
        '/',
        ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'],
        ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-'], 
        ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
        ['-', 'Image', 'Table'],
        ['rockmergefield', '-', 'rockimagebrowser', 'rockdocumentbrowser']
	];	

CKEDITOR.replace('{0}', {{ 
  allowedContent: true,
  toolbar: toolbar_RockCustomConfig{1},
  removeButtons: '',
  height: '{2}',
  baseFloatZIndex: 200000,  // set zindex to be 200000 so it will be on top of our modals (100000)
  extraPlugins: '{5}',
  resize_maxWidth: '{3}',
  rockFileBrowserOptions: {{ 
    documentFolderRoot: '{6}', 
    imageFolderRoot: '{7}',
    imageFileFilter: '{8}'
  }},
  rockMergeFieldOptions: {{ mergeFields: '{9}' }},
  on : {{
       change: function (e) {{
         // update the underlying TextElement on every little change to ensure that Posting and Validation works consistently (doing it OnSubmit or OnBlur misses some cases)
         e.editor.updateElement();  
         {4}
       }}
  }}
}} );
            ";

            string customOnChangeScript = null;

            if ( !string.IsNullOrWhiteSpace( this.OnChangeScript ) )
            {
                customOnChangeScript = @"
                // custom on change script 
                " + this.OnChangeScript;
            }

            List<string> enabledPlugins = new List<string>();
            if ( MergeFields.Any() )
            {
                enabledPlugins.Add( "rockmergefield" );
            }

            enabledPlugins.Add( "rockfilebrowser" );

            string ckeditorInitScript = string.Format( ckeditorInitScriptFormat, this.ClientID, this.Toolbar.ConvertToString(),
                this.Height, this.ResizeMaxWidth ?? 0, customOnChangeScript, enabledPlugins.AsDelimited( "," ), 
                Rock.Security.Encryption.EncryptString(this.DocumentFolderRoot), // encrypt the folders so the folder can only be configured on the server
                Rock.Security.Encryption.EncryptString(this.ImageFolderRoot),
                this.ImageFileFilter,
                this.MergeFields.AsDelimited(",") );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "ckeditor_init_script_" + this.ClientID, ckeditorInitScript, true );

            base.RenderControl( writer );
        }
    }
}