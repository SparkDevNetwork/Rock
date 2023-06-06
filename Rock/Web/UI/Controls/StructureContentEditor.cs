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
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Cms.StructuredContent;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for entering a Structure Content Editor.
    /// </summary>
    [ToolboxData( "<{0}:StructureContentEditor runat=server></{0}:StructureContentEditor>" )]
    public class StructureContentEditor : CompositeControl, IRockControl
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
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
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
        public string ValidationGroup { get; set; }

        #endregion

        #region Controls

        /// <summary>
        /// The _HF value
        /// </summary>
        public HiddenField _hfValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the html content.
        /// </summary>
        /// <value>
        /// The html content.
        /// </value>
        [Obsolete( "Use Rock.Cms.StructuredContent.StructuredContentHelper instead." )]
        [RockObsolete( "1.13" )]
        public string HtmlContent
        {
            get
            {
                var helper = new StructuredContentHelper( HttpUtility.UrlDecode( this.StructuredContent ) );

                return helper.Render();
            }
        }

        /// <summary>
        /// Gets or sets the structured content.
        /// </summary>
        /// <value>
        /// The structured content.
        /// </value>
        public string StructuredContent
        {
            get
            {
                EnsureChildControls();
                return HttpUtility.UrlDecode(  _hfValue.Value );
            }
            set
            {
                EnsureChildControls();
                if ( value.IsNotNullOrWhiteSpace() )
                {
                    /*
                        1/15/2021 - NAm

                        An earlier note said we should not use HttpUtility.UrlEncode because it
                        would convert spaces into "+" instead of %20.  However we've now learned
                        that Uri.EscapeUriString has a limit between 32766-65520 characters
                        which is less than the length of some "values".  Therefore we'll jump through
                        this hoop and perform the conversion using a custom Escape method.

                        Reason: Image data inside of Structured Content Editor.
                     */

                    _hfValue.Value = EscapeLongDataString( value ); // HttpUtility.UrlEncode makes spaces + instead of %20
                }
            }
        }

        /// <summary>
        /// Gets or sets the Structure Content Tool Id.
        /// </summary>
        /// <value>
        /// The structure content tool value identifier.
        /// </value>
        public int? StructuredContentToolValueId
        {
            get
            {
                return ViewState["StructuredContentToolValueId"] as int?;
            }

            set
            {
                ViewState["StructuredContentToolValueId"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureContentEditor"/> class.
        /// </summary>
        public StructureContentEditor() : base()
        {
            this.HelpBlock = new HelpBlock();
            this.WarningBlock = new WarningBlock();
            RequiredFieldValidator = new HiddenFieldValidator();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfValue = new HiddenField();
            _hfValue.ID = this.ID + "_hfValue";
            Controls.Add( _hfValue );

            this.RequiredFieldValidator.InitialValue = "{}";
            this.RequiredFieldValidator.ControlToValidate = _hfValue.ID;
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
                RockPage.AddScriptLink( Page, "~/Scripts/Rock/UI/structuredcontenteditor/editor.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/Rock/UI/structuredcontenteditor/editor-tools.js" );
            }

            EnsureChildControls();
        }


        /// <summary>
        /// Called by the ASP.NET page framework after event processing has finished but
        /// just before rendering begins.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
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
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( _hfValue.Value.IsNullOrWhiteSpace() )
            {
                _hfValue.Value = "{}";
            }
            _hfValue.RenderControl( writer );
            writer.WriteLine();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "structured-content-container" );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();
            writer.WriteLine();

            // add script on demand only when there will be an htmleditor rendered
            if ( ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                var editorUrl = Page.ResolveUrl( "~/Scripts/Rock/UI/structuredcontenteditor/editor.js" );
                var toolsUrl = Page.ResolveUrl( "~/Scripts/Rock/UI/structuredcontenteditor/editor-tools.js" );

                ScriptManager.RegisterClientScriptInclude( Page, Page.GetType(), "rock-editorjs", editorUrl );
                ScriptManager.RegisterClientScriptInclude( Page, Page.GetType(), "rock-editorjs-tools", toolsUrl );
            }

            RegisterJavascript();
        }

        /// <summary>
        /// Registers the javascript.
        /// </summary>
        private void RegisterJavascript()
        {
            var structuredContentToolConfiguration = string.Empty;
            if ( StructuredContentToolValueId.HasValue )
            {
                var structuredContentToolValue = DefinedValueCache.Get( StructuredContentToolValueId.Value );
                if ( structuredContentToolValue != null )
                {
                    structuredContentToolConfiguration = structuredContentToolValue.Description;
                }
            }

            if ( structuredContentToolConfiguration.IsNullOrWhiteSpace() )
            {
                var structuredContentToolValue = DefinedValueCache.Get( SystemGuid.DefinedValue.STRUCTURE_CONTENT_EDITOR_DEFAULT );
                if ( structuredContentToolValue != null )
                {
                    structuredContentToolConfiguration = structuredContentToolValue.Description;
                }
            }

            var script = string.Format( @"
;(function() {{
var fieldContent = $('#{1}').val();
var editor = new Rock.UI.StructuredContentEditor.EditorJS({{
    holder: '{0}',
    tools: {2},
    defaultBlock: 'paragraph',
    data: JSON.parse(decodeURIComponent(fieldContent)),
    onReady: function() {{
        new Rock.UI.StructuredContentEditor.EditorDragDrop(editor);
    }},
    onChange: function() {{
        setTimeout(() => {{
            editor.save().then( function(savedData) {{
                $('#{1}').val(encodeURIComponent(JSON.stringify(savedData)));
            }}).catch((e) => {{
                console.log('Saving failed: ', e)
            }});
        }}, 0);
    }}
}});
}})();
", this.ClientID, _hfValue.ClientID, structuredContentToolConfiguration );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "structure-content-script" + this.ClientID, script, true );
        }

        /// <summary>
        /// Escapes the data string in such a way as to deal with really long values commonly found
        /// in pasted image data (data:image/png;base64,....) which would otherwise throw a
        /// UriFormatException ("Invalid URI: The Uri string is too long.").
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        private static string EscapeLongDataString( string str )
        {
            var maxLength = 32766;  // per https://docs.microsoft.com/en-us/dotnet/api/system.uri.escapedatastring?view=net-5.0
            var iterations = str.Length / maxLength;

            var sb = new StringBuilder();
            for ( int i = 0; i <= iterations; i++ )
            {
                sb.Append( Uri.EscapeDataString( i < iterations
                    ? str.Substring( maxLength * i, maxLength )
                    : str.Substring( maxLength * i ) ) );
            }

            return sb.ToString();
        }
    }
}