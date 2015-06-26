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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

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
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
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
        /// Defaults to ~/Content
        /// </summary>
        /// <value>
        /// The document folder root.
        /// </value>
        public string DocumentFolderRoot
        {
            get
            {
                var result = ViewState["DocumentFolderRoot"] as string;
                if ( string.IsNullOrWhiteSpace( result ) )
                {
                    result = "~/Content";
                }

                return result;
            }

            set
            {
                ViewState["DocumentFolderRoot"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the image folder root.
        /// Defaults to ~/Content
        /// </summary>
        /// <value>
        /// The image folder root.
        /// </value>
        public string ImageFolderRoot
        {
            get
            {
                var result = ViewState["ImageFolderRoot"] as string;
                if ( string.IsNullOrWhiteSpace( result ) )
                {
                    result = "~/Content";
                }

                return result;
            }

            set
            {
                ViewState["ImageFolderRoot"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether root folder should be specific to user.  If true
        /// a folder name equal to the the current user's login will be added to the root path.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user specific root]; otherwise, <c>false</c>.
        /// </value>
        public bool UserSpecificRoot
        {
            get
            {
                return ViewState["UserSpecificRoot"] as bool? ?? false;
            }
            set
            {
                ViewState["UserSpecificRoot"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the merge fields to make available.  This should include either a list of
        /// entity type names (full name), or other non-object string values
        /// </summary>
        /// <remarks>
        /// Format should be one of the following formats
        ///     "FieldName"                     - Label will be a case delimited version of FieldName (i.e. "Field Name")
        ///     "FieldName|LabelName"
        ///     "FieldName^EntityType           - Will evaluate the entity type and add a navigable tree for the objects 
        ///                                       properties and attributes. Label will be a case delimited version of 
        ///                                       FieldName (i.e. "Field Name")
        ///     "FieldName^EntityType|LabelName - Will evaluate the entity type and add a navigable tree for the objects 
        ///                                       properties and attributes.    
        ///                                  
        /// Supports the following "special" field names
        ///     "GlobalAttribute"               - Provides navigable list of global attributes
        ///     "Campuses"                      - Will return an array of all campuses
        ///     "Date"                          - Will return lava syntax for displaying current date
        ///     "Time"                          - Will return lava syntax for displaying current time
        ///     "DayOfWeek"                     - Will return lava syntax for displaying the current day of the week
        ///     "PageParameter"                 - Will return lava synax and support for rendering any page parameter 
        ///                                       (query string and/or route parameter value)
        /// </remarks>
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

        /// <summary>
        /// Gets or sets any additional configuration settings for the CKEditor.  Should be in SettingName: SettingValue, ... format.  
        /// For example: autoParagrapth: false, enterMode: 3,
        /// </summary>
        /// <value>
        /// The additional configurations.
        /// </value>
        public string AdditionalConfigurations
        {
            get { return ViewState["AdditionalConfigurations"] as string ?? string.Empty; }
            set { ViewState["AdditionalConfigurations"] = value; }
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
// ensure that ckEditor.js link is added to page
if (!$('#ckeditorJsLib').length) {{
    // by default, jquery adds a cache-busting parameter on dynamically added script tags. set the ajaxSetup cache:true to prevent this
    $.ajaxSetup({{ cache: true }});
    $('head').prepend(""<script id='ckeditorJsLib' src='{12}' />"");
}}

// allow i tags to be empty (for font awesome)
CKEDITOR.dtd.$removeEmpty['i'] = false

// In IE, the CKEditor doesn't accept keyboard input when loading again within the same page instance.  Destroy fixes it, but destroy throws an exception in Chrome
if (CKEDITOR.instances.{0}) {{
    try
    {{
        CKEDITOR.instances.{0}.destroy();
    }}
    catch (ex)
    {{
        // ignore error
    }}
}}
  
CKEDITOR.replace('{0}', {{ 
    {11}
    allowedContent: true,
    toolbar: Rock.htmlEditor.toolbar_RockCustomConfig{1},
    removeButtons: '',
    baseFloatZIndex: 200000,  // set zindex to be 200000 so it will be on top of our modals (100000)
    entities: false, // stop CKEditor from using HTML entities in the editor output. Prevents single quote from getting escaped, etc
    htmlEncodeOutput: true,
    extraPlugins: '{5}',
    resize_maxWidth: '{3}',
    rockFileBrowserOptions: {{ 
    documentFolderRoot: '{6}', 
    imageFolderRoot: '{7}',
    imageFileTypeWhiteList: '{8}',
    fileTypeBlackList: '{9}'
    }},
    rockMergeFieldOptions: {{ mergeFields: '{10}' }},
    rockTheme: '{13}',
    on : {{
        change: function (e) {{
            // update the underlying TextElement on every little change (when in WYSIWIG mode) to ensure that Posting and Validation works consistently (doing it OnSubmit or OnBlur misses some cases)
            e.editor.updateElement();  
            {4}
        }},
        instanceReady: function (e) {{

            CKEDITOR.instances.{0}.updateElement();

            // update the underlying TextElement when there is a change event in SOURCE mode
            $('#cke_{0}').on( 'change paste', '.cke_source', function(e, data) {{
                CKEDITOR.instances.{0}.updateElement();
            }});

            // In IE, clicking the Source button does not cause the .cke_source to lose focus 
            // and fire the onchange event, so also updateElement when source button is clicked
            $('#cke_{0} .cke_button__source').click( function(e, data) {{
                CKEDITOR.instances.{0}.updateElement();
            }});

            // set the height
            if ('{2}' != '') {{
              var topHeight = $('#' + e.editor.id + '_top').height();
              var contentHeight = '{2}'.replace('px','') - topHeight - 40;
              $('#' + e.editor.id + '_contents').css('height', contentHeight);
            }}
        }}
    }}
}}
);
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

            // only show the File/Image plugin if they have Auth to the file browser page
            var fileBrowserPage = new Rock.Model.PageService( new RockContext() ).Get( Rock.SystemGuid.Page.CKEDITOR_ROCKFILEBROWSER_PLUGIN_FRAME.AsGuid() );
            if ( fileBrowserPage != null )
            {
                var currentPerson = this.RockBlock().CurrentPerson;
                if ( currentPerson != null )
                {
                    if ( fileBrowserPage.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        enabledPlugins.Add( "rockfilebrowser" );
                    }
                }
            }

            var globalAttributesCache = GlobalAttributesCache.Read();

            string imageFileTypeWhiteList = globalAttributesCache.GetValue( "ContentImageFiletypeWhitelist" );
            string fileTypeBlackList = globalAttributesCache.GetValue( "ContentFiletypeBlacklist" );

            string documentFolderRoot = this.DocumentFolderRoot;
            string imageFolderRoot = this.ImageFolderRoot;
            if ( this.UserSpecificRoot )
            {
                var currentUser = this.RockBlock().CurrentUser;
                if ( currentUser != null )
                {
                    documentFolderRoot = System.Web.VirtualPathUtility.Combine( documentFolderRoot.EnsureTrailingBackslash(), currentUser.UserName.ToString() );
                    imageFolderRoot = System.Web.VirtualPathUtility.Combine( imageFolderRoot.EnsureTrailingBackslash(), currentUser.UserName.ToString() );
                }
            }

            // Make sure that if additional configurations are defined, that the string ends in a comma.
            if ( !string.IsNullOrWhiteSpace( this.AdditionalConfigurations ) && !this.AdditionalConfigurations.Trim().EndsWith( "," ) )
            {
                this.AdditionalConfigurations = this.AdditionalConfigurations.Trim() + ",";
            }

            string ckEditorLib = ( (RockPage)this.Page ).ResolveRockUrl( "~/Scripts/ckeditor/ckeditor.js", true );

            string ckeditorInitScript = string.Format( ckeditorInitScriptFormat,
                this.ClientID,                                                  // {0}
                this.Toolbar.ConvertToString(),                                 // {1}
                this.Height,                                                    // {2}
                this.ResizeMaxWidth ?? 0,                                       // {3}
                customOnChangeScript,                                           // {4}
                enabledPlugins.AsDelimited( "," ),                              // {5}
                Rock.Security.Encryption.EncryptString( documentFolderRoot ),   // {6} encrypt the folders so the folder can only be configured on the server
                Rock.Security.Encryption.EncryptString( imageFolderRoot ),      // {7}
                imageFileTypeWhiteList,                                         // {8}
                fileTypeBlackList,                                              // {9}
                this.MergeFields.AsDelimited( "," ),                            // {10}
                this.AdditionalConfigurations,                                  // {11}
                ckEditorLib,                                                    // {12}
                ( (RockPage)this.Page ).Site.Theme                              // {13}
                );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "ckeditor_init_script_" + this.ClientID, ckeditorInitScript, true );

            base.RenderControl( writer );
        }

        /// <summary>
        /// Processes the postback data for the <see cref="T:System.Web.UI.WebControls.TextBox" /> control.
        /// </summary>
        /// <param name="postDataKey">The index within the posted collection that references the content to load.</param>
        /// <param name="postCollection">The collection posted to the server.</param>
        /// <returns>
        /// true if the posted content is different from the last posting; otherwise, false.
        /// </returns>
        protected override bool LoadPostData( string postDataKey, System.Collections.Specialized.NameValueCollection postCollection )
        {
            if ( base.LoadPostData( postDataKey, postCollection ) )
            {
                base.Text = HttpUtility.HtmlDecode( base.Text );
                return true;
            }
            return false;
        }
    }
}