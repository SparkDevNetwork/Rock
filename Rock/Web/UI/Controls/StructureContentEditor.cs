using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for entering a Structure Content Editor.
    /// </summary>
    [ToolboxData( "<{0}:StructureContentEditor runat=server></{0}:StructureContentEditor>" )]
    public class StructureContentEditor : CompositeControl, IRockControl
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            EnsureChildControls();
        }

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

        /// <summary>
        /// The hf disable VRM
        /// </summary>
        public HiddenField _hfValueDisableVrm;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the html content.
        /// </summary>
        /// <value>
        /// The html content.
        /// </value>
        public string HtmlContent
        {
            get
            {
                string lava = "{% include '~/Assets/Lava/editorjs-lava-parser.lava' %}";
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "data", HttpUtility.UrlDecode( this.StructuredContent ) );
                return lava.ResolveMergeFields( mergeFields );
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
                return _hfValue.Value;
            }
            set
            {
                EnsureChildControls();
                _hfValue.Value = value;

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

            _hfValueDisableVrm = new HiddenField();
            _hfValueDisableVrm.ID = _hfValue.ID + "_dvrm";
            Controls.Add( _hfValueDisableVrm );
        }

        /// <summary>
        /// Called by the ASP.NET page framework after event processing has finished but
        /// just before rendering begins.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( this.Visible && !ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/header.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/image.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/delimiter.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/list.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/checklist.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/quote.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/code.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/embed.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/table.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/link.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/warning.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/marker.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/inline-code.js" );
                RockPage.AddScriptLink( Page, "~/Scripts/editor.js/editor.js" );
            }
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
            _hfValueDisableVrm.RenderControl( writer );
            writer.WriteLine();

            writer.AddStyleAttribute( HtmlTextWriterStyle.BackgroundColor, "#f7f7f7" );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.WriteLine();

            // add editor.js on demand only when there will be a structure content editor rendered
            if ( ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "header-include", ResolveUrl( "~/Scripts/editor.js/header.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "image-include", ResolveUrl( "~/Scripts/editor.js/image.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "delimiter-include", ResolveUrl( "~/Scripts/editor.js/delimiter.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "list-include", ResolveUrl( "~/Scripts/editor.js/list.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "checklist-include", ResolveUrl( "~/Scripts/editor.js/checklist.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "quote-include", ResolveUrl( "~/Scripts/editor.js/quote.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "code-include", ResolveUrl( "~/Scripts/editor.js/code.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "embed-include", ResolveUrl( "~/Scripts/editor.js/embed.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "table-include", ResolveUrl( "~/Scripts/editor.js/table.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "link-include", ResolveUrl( "~/Scripts/editor.js/link.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "warning-include", ResolveUrl( "~/Scripts/editor.js/warning.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "marker-include", ResolveUrl( "~/Scripts/editor.js/marker.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "inline-include", ResolveUrl( "~/Scripts/editor.js/inline-code.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "editor-include", ResolveUrl( "~/Scripts/editor.js/editor.js" ) );
            }

            RegisterJavascript();
        }

        /// <summary>
        /// Registers the javascript.
        /// </summary>
        private void RegisterJavascript()
        {
            var script = string.Format( @"
const fieldContent = $('#{1}').val();
 const output = document.getElementById('output');
/**
 * To initialize the Editor, create a new instance with configuration object
 * @see docs/installation.md for mode details
 */
var editor = new EditorJS({{
holderId: '{0}',
tools: {{
    header: {{
    class: Header,
    inlineToolbar: ['link'],
    config: {{
        placeholder: 'Header'
    }},
    shortcut: 'CMD+SHIFT+H'
    }},
    image: {{
    class: ImageTool,
    inlineToolbar: ['link'],
    }},
    list: {{
    class: List,
    inlineToolbar: true,
    shortcut: 'CMD+SHIFT+L'
    }},
    checklist: {{
    class: Checklist,
    inlineToolbar: true,
    }},
    quote: {{
    class: Quote,
    inlineToolbar: true,
    config: {{
        quotePlaceholder: 'Enter a quote',
        captionPlaceholder: 'Quote\'s author',
    }},
    shortcut: 'CMD+SHIFT+O'
    }},
    warning: Warning,
    marker: {{
    class:  Marker,
    shortcut: 'CMD+SHIFT+M'
    }},
    code: {{
    class:  CodeTool,
    shortcut: 'CMD+SHIFT+C'
    }},
    delimiter: Delimiter,
    inlineCode: {{
    class: InlineCode,
    shortcut: 'CMD+SHIFT+C'
    }},
    linkTool: LinkTool,
    embed: Embed,
    table: {{
    class: Table,
    inlineToolbar: true,
    shortcut: 'CMD+ALT+T'
    }}
}},
initialBlock: 'paragraph',
data: JSON.parse(decodeURIComponent(fieldContent)),
onChange: function() {{
    editor.save().then( function(savedData) {{
        $('#{1}').val(encodeURIComponent(JSON.stringify(savedData)));
    }}).catch((e) => {{
        console.log('Saving failed: ', e)
    }});
}}
}});
", this.ClientID, _hfValue.ClientID );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "editor-script" + this.ClientID, script, true );
        }
    }
}