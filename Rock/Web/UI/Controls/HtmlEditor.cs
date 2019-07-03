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
using System.Linq;
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
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
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
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

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

        private HiddenField _hfDisableVrm;
        private HiddenFieldWithClass _hfInCodeEditorMode;
        private CodeEditor _ceEditor;

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
        /// Gets or sets the custom javascript that will get executed when the editor 'onKeyUp' event occurs.
        /// Obsolete because it is misleading that this actually is triggered off of the onKeyUp event
        /// </summary>
        /// <value>
        /// The custom on change press script.
        /// </value>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use CallbackOnKeyupScript or CallbackOnChangeScript instead", true )]
        public string OnChangeScript
        {
            get
            {
                return CallbackOnKeyupScript;
            }

            set
            {
               CallbackOnKeyupScript= value;
            }
        }

        /// <summary>
        /// Gets or sets the custom javascript that will get executed when the editor 'onChange' event occurs
        /// </summary>
        /// <value>
        /// The custom on change press script.
        /// </value>
        public string CallbackOnChangeScript
        {
            get
            {
                return ViewState["CallbackOnChangeScript"] as string;
            }

            set
            {
                ViewState["CallbackOnChangeScript"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the custom javascript that will get executed when the editor 'onKeyUp' event occurs
        /// </summary>
        /// <value>
        /// The custom on change press script.
        /// </value>
        public string CallbackOnKeyupScript
        {
            get
            {
                return ViewState["CallbackOnKeyupScript"] as string;
            }

            set
            {
                ViewState["CallbackOnKeyupScript"] = value;
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
        /// Gets or sets a value indicating whether [start in code editor mode].
        /// </summary>
        /// <value>
        /// <c>true</c> if [start in code editor mode]; otherwise, <c>false</c>.
        /// </value>
        public bool StartInCodeEditorMode
        {
            get
            {
                bool startInCodeEditorMode = ViewState["StartInCodeEditorMode"] as bool? ?? false;

                if ( !startInCodeEditorMode )
                {
                    EnsureChildControls();
                    if ( _ceEditor.Text.HasLavaCommandFields() )
                    {
                        // if there are lava commands {% %} in the text, force code editor mode
                        startInCodeEditorMode = true;
                    }
                }

                ViewState["StartInCodeEditorMode"] = startInCodeEditorMode;

                return startInCodeEditorMode;
            }

            set
            {
                ViewState["StartInCodeEditorMode"] = value;
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
            WarningBlock = new WarningBlock();

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

            if ( this.Visible && !ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                RockPage.AddScriptLink( Page, "~/Scripts/summernote/summernote.min.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/Bundles/RockHtmlEditorPlugins", false );
            }

            EnsureChildControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( this.Page.IsPostBack )
            {
                if ( this.Page.Request.Params[_hfInCodeEditorMode.UniqueID].AsBoolean() )
                {
                    this.Text = _ceEditor.Text;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text content of the <see cref="T:System.Web.UI.WebControls.TextBox" /> control.
        /// </summary>
        public override string Text
        {
            get
            {
                EnsureChildControls();
                if ( _hfInCodeEditorMode.Value.AsBoolean() )
                {
                    return _ceEditor.Text;
                }
                else
                {
                    return base.Text;
                }
            }

            set
            {
                _ceEditor.Text = value;
                base.Text = value;
            }
        }


        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfDisableVrm = new HiddenField();
            _hfDisableVrm.ID = this.ID + "_dvrm";
            _hfDisableVrm.Value = "True";
            Controls.Add( _hfDisableVrm );

            _hfInCodeEditorMode = new HiddenFieldWithClass();
            _hfInCodeEditorMode.CssClass = "js-incodeeditormode";
            _hfInCodeEditorMode.ID = this.ID + "_hfInCodeEditorMode";
            Controls.Add( _hfInCodeEditorMode );

            _ceEditor = new CodeEditor();
            _ceEditor.ID = this.ID + "_codeEditor";
            _ceEditor.EditorMode = CodeEditorMode.Lava;
            if ( !string.IsNullOrEmpty(this.CallbackOnChangeScript) )
            {
                _ceEditor.OnChangeScript = this.CallbackOnChangeScript;
            }

            Controls.Add( _ceEditor );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                if ( this.StartInCodeEditorMode )
                {
                    if ( _ceEditor.Text != this.Text )
                    {
                        _ceEditor.Text = this.Text;
                    }

                    // in the case of when StartInCodeEditorMode = true, we can set base.Text to string.Empty to help prevent bad html and/or javascript from messing up things
                    // However, if StartInCodeEditorMode = false, we can't do this because the WYSIWIG editor needs to know the base.Text value
                    base.Text = string.Empty;
                }

                RockControlHelper.RenderControl( this, writer );
                _hfDisableVrm.RenderControl( writer );
                _hfInCodeEditorMode.RenderControl( writer );
                _ceEditor.RenderControl( writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            bool rockMergeFieldEnabled = MergeFields.Any();
            bool rockFileBrowserEnabled = false;
            bool rockAssetManagerEnabled = false;
            var currentPerson = this.RockBlock().CurrentPerson;
                
            // only show the File/Image plugin if they have Auth to the file browser page
            var fileBrowserPage = new Rock.Model.PageService( new RockContext() ).Get( Rock.SystemGuid.Page.HTMLEDITOR_ROCKFILEBROWSER_PLUGIN_FRAME.AsGuid() );
            if ( fileBrowserPage != null && currentPerson != null )
            {
                rockFileBrowserEnabled = fileBrowserPage.IsAuthorized( Authorization.VIEW, currentPerson );
            }

            var assetManagerPage = new Rock.Model.PageService( new RockContext() ).Get( Rock.SystemGuid.Page.HTMLEDITOR_ROCKASSETMANAGER_PLUGIN_FRAME.AsGuid() );
            if ( assetManagerPage != null && currentPerson != null )
            {
                rockAssetManagerEnabled = assetManagerPage.IsAuthorized( Authorization.VIEW, currentPerson );
            }

            //TODO: Look for a valid asset manager and disable the control if one is not found



            var globalAttributesCache = GlobalAttributesCache.Get();

            string imageFileTypeWhiteList = globalAttributesCache.GetValue( "ContentImageFiletypeWhitelist" );
            string fileTypeBlackList = globalAttributesCache.GetValue( "ContentFiletypeBlacklist" );
            string fileTypeWhiteList = globalAttributesCache.GetValue( "ContentFiletypeWhitelist" );

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

            string callbacksOption = string.Empty;
            if ( !string.IsNullOrEmpty( this.CallbackOnKeyupScript ) || !string.IsNullOrEmpty( this.CallbackOnChangeScript ) )
            {
                callbacksOption =
$@" 
onKeyup: function(e) {{  
    {this.CallbackOnKeyupScript}  
}},
onChange: function(contents, $editable) {{  
    {this.CallbackOnChangeScript}  
}}
";
             }


            string summernoteInitScript = $@"
function pageLoad() {{
  // remove any leftover popovers that summernote might have created and orphaned  
  $('.note-popover.popover').hide();
}}

$(document).ready( function() {{

    // workaround for https://github.com/summernote/summernote/issues/2017 and/or https://github.com/summernote/summernote/issues/1984
    if(!!document.createRange) {{
      document.getSelection().removeAllRanges();
    }}

    var summerNoteEditor_{this.ClientID} = $('#{this.ClientID}').summernote({{
        height: '{this.Height}', //set editable area's height
        toolbar: Rock.htmlEditor.toolbar_RockCustomConfig{this.Toolbar.ConvertToString()},

        popover: {{
          image: [
            ['custom1', ['rockimagelink']],
            ['imagesize', ['imageSize100', 'imageSize50', 'imageSize25']],
            ['custom2', ['rockimagebrowser', 'rockassetmanager']],
            ['float', ['floatLeft', 'floatRight', 'floatNone']],
            ['remove', ['removeMedia']]
          ],
          link: [
            ['link', ['linkDialogShow', 'unlink']]
          ],
          air: [
            ['color', ['color']],
            ['font', ['bold', 'underline', 'clear']],
            ['para', ['ul', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture']]
          ]
        }},

        callbacks: {{
           {callbacksOption} 
        }},

        buttons: {{
            rockfilebrowser: RockFileBrowser,
            rockimagebrowser: RockImageBrowser, 
            rockimagelink: RockImageLink,
            rockassetmanager: RockAssetManager,
            rockmergefield: RockMergeField,
            rockcodeeditor: RockCodeEditor,
            rockpastetext: RockPasteText,
            rockpastefromword: RockPasteFromWord
        }},

        rockFileBrowserOptions: {{ 
            enabled: {rockFileBrowserEnabled.ToTrueFalse().ToLower()},
            documentFolderRoot: '{Rock.Security.Encryption.EncryptString( documentFolderRoot )}', 
            imageFolderRoot: '{Rock.Security.Encryption.EncryptString( imageFolderRoot )}',
            imageFileTypeWhiteList: '{imageFileTypeWhiteList}',
            fileTypeBlackList: '{fileTypeBlackList}',
            fileTypeWhiteList: '{fileTypeWhiteList}'
        }},

        rockAssetManagerOptions: {{
            enabled: { rockAssetManagerEnabled.ToTrueFalse().ToLower() }
        }},

        rockMergeFieldOptions: {{ 
            enabled: {rockMergeFieldEnabled.ToTrueFalse().ToLower()},
            mergeFields: '{this.MergeFields.AsDelimited( "," )}' 
        }},
        rockTheme: '{( ( RockPage ) this.Page ).Site.Theme}',

        codeEditorOptions: {{
            controlId: '{_ceEditor.ClientID}',
            inCodeEditorModeHiddenFieldId: '{_hfInCodeEditorMode.ClientID}'
        }},
    }});

    if ({StartInCodeEditorMode.ToTrueFalse().ToLower()} && RockCodeEditor) {{
        RockCodeEditor(summerNoteEditor_{this.ClientID}.data('summernote'), true).click();
    }}

}});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "summernote_init_script_" + this.ClientID, summernoteInitScript, true );

            // add script on demand only when there will be an htmleditor rendered
            if ( ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "summernote-lib", ( (RockPage)this.Page ).ResolveRockUrl( "~/Scripts/summernote/summernote.min.js", true ) );
                var bundleUrl = System.Web.Optimization.BundleResolver.Current.GetBundleUrl( "~/Scripts/Bundles/RockHtmlEditorPlugins" );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "summernote-plugins", bundleUrl );
            }

            // set this textbox hidden until we can run the js to attach summernote to it
            this.Style[HtmlTextWriterStyle.Display] = "none";

            base.RenderControl( writer );
        }
    }
}