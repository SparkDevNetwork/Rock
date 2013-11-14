//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:CodeEditor runat=server></{0}:CodeEditor>" )]
    public class CodeEditor : TextBox, IRockControl
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
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height of the control.
        /// </value>
        [
        Bindable(false),
        Category("Appearance"),
        DefaultValue(""),
        Description("The height in pixels of the control")
        ]
        public string EditorHeight
        {
            get { return ViewState["EditorHeight"] as string ?? "200"; }
            set { ViewState["EditorHeight"] = value; }
        }

        /// <summary>
        /// Gets or sets the editor mode (language).
        /// </summary>
        /// <value>
        /// The language of the editor.
        /// </value>
        [
        Bindable(false),
        Category("Appearance"),
        DefaultValue(""),
        Description("The language of the code to be edited")
        ]
        public CodeEditorMode EditorMode
        {
            get {
                if (ViewState["EditorMode"] != null)
                    return (CodeEditorMode)Enum.Parse(typeof(CodeEditorMode), ViewState["EditorMode"].ToString());
                return CodeEditorMode.Text; // Default value
            }
            set { ViewState["EditorMode"] = value; }
        }

        /// <summary>
        /// Gets or sets the editor theme.
        /// </summary>
        /// <value>
        /// The theme of the editor.
        /// </value>
        [
        Bindable(false),
        Category("Appearance"),
        DefaultValue(""),
        Description("The theme of the editor")
        ]
        public CodeEditorTheme EditorTheme
        {
            get
            {
                if (ViewState["EditorTheme"] != null)
                    return (CodeEditorTheme)Enum.Parse(typeof(CodeEditorTheme), ViewState["EditorTheme"].ToString());
                return CodeEditorTheme.Rock; // Default value
            }
            set { ViewState["EditorTheme"] = value; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RockTextBox" /> class.
        /// </summary>
        public CodeEditor()
            : base()
        {
            RequiredFieldValidator = new RequiredFieldValidator();
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            RockPage.AddScriptLink(Page, ResolveUrl("~/Scripts/ace/ace.js"));
            this.TextMode = TextBoxMode.MultiLine;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls(this, Controls);
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

            // add editor div
            if (!string.IsNullOrWhiteSpace(EditorHeight))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Style, "height:" + EditorHeight + "px;");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Style, "height: 200px;");
            }

            string customDiv = @"<div class='code-editor-container' style='position:relative; height: {0}px'><div id='codeeditor-div-{1}'>{2}</div></div>";

            writer.Write(string.Format(customDiv, this.EditorHeight, this.ClientID, HttpUtility.UrlDecode(this.Text)));

            // write custom css for the code editor
            string customStyle = @"
                <style type='text/css' media='screen'>
                    #codeeditor-div-{0} {{ 
                        position: absolute;
                        top: 0;
                        right: 0;
                        bottom: 0;
                        left: 0;
                    }}

                    
                </style>     
";
            string cssCode = string.Format(customStyle, this.ClientID);
            writer.Write(cssCode);

            // make textbox hidden
            ((WebControl)this).Style.Add(HtmlTextWriterStyle.Display, "none");
            

            string scriptFormat = @"
                var ce_{0} = ace.edit('codeeditor-div-{0}');
                ce_{0}.setTheme('ace/theme/{1}');
                ce_{0}.getSession().setMode('ace/mode/{2}');

                //var ce_{0}_dest = document.getElementById('{0}');

                ce_{0}.getSession().on('change', function(e) {{
                    document.getElementById('{0}').value = ce_{0}.getValue();
                    //ce_{0}_dest.value = ce_{0}.getValue();
                }});
                

";
            string script = string.Format(scriptFormat, this.ClientID, EditorThemeAsString(this.EditorTheme), EditorModeAsString(this.EditorMode));
            ScriptManager.RegisterStartupScript(this, this.GetType(), "codeeditor_" + this.ID, script, true);

            base.RenderControl( writer );

            RenderDataValidator( writer );

        }

        /// <summary>
        /// Renders any data validator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderDataValidator( HtmlTextWriter writer )
        {
        }

        /// <summary>
        /// Gets or sets the text content of the <see cref="T:System.Web.UI.WebControls.TextBox" /> control.
        /// </summary>
        /// <returns>The text displayed in the <see cref="T:System.Web.UI.WebControls.TextBox" /> control. The default is an empty string ("").</returns>
        public override string Text
        {
            get
            {
                if ( base.Text == null )
                {
                    return null;
                }
                else
                {
                    return base.Text.Trim();
                }
            }
            set
            {
                base.Text = value;
            }
        }

        /// <summary>
        /// Gets the mode of the editor as text based on property
        /// </summary>
        /// <returns>The text value of the mode.</returns>
        private string EditorModeAsString(CodeEditorMode mode)
        {
            string[] modeValues = new string[] { "text", "css", "html", "liquid", "javascript", "less", "powershell", "sql", "typescript", "csharp" };

            return modeValues[(int)mode];
        }

        /// <summary>
        /// Gets the theme of the editor as text based on property
        /// </summary>
        /// <returns>The text value of the mode.</returns>
        private string EditorThemeAsString(CodeEditorTheme theme)
        {
            string[] themeValues = new string[] { "github", "chrome", "crimson_editor", "dawn", "dreamweaver", "eclipse", "solarized_light", "textmate", 
                "tomorrow", "xcode", "github", "ambiance", "chaos", "clouds_midnight", "cobalt", "idle_fingers", "kr_theme", 
                "merbivore", "merbivore_soft", "mono_industrial", "monokai", "pastel_on_dark", "solarized_dark", "terminal", "tomorrow_night", "tomorrow_night_blue",
                "tomorrow_night_bright", "tomorrow_night_eighties", "twilight", "vibrant_ink"};

            return themeValues[(int)theme];
        }

    }

    public enum CodeEditorMode
    {
        Text = 0,
        Css = 1,
        Html = 2,
        Liquid = 3,
        JavaScript = 4,
        Less = 5,
        Powershell = 6,
        Sql = 7,
        TypeScript = 8,
        CSharp = 9,
    }

    public enum CodeEditorTheme
    {
        Rock = 0,
        Chrome = 1,
        CrimsonEditor = 2,
        Dawn = 3,
        Dreamweaver = 4,
        Eclipse = 5,
        SolarizedLight = 6,
        Textmate = 7,
        Tomorrow = 8,
        Xcode = 9,
        Github = 10,
        AmbianceDark = 11,
        ChaosDark = 12,
        CloudsMidnightDark = 13,
        CobaltDark = 14,
        IdleFingersDark = 15,
        krThemeDark = 16,
        MerbivoreDark = 17,
        MerbivoreSoftDark = 18,
        MonoIndustrialDark = 19,
        MonokaiDark = 20,
        PastelOnDark = 21,
        SolarizedDark = 22,
        TerminalDark = 23,
        TomorrowNightDark = 24,
        TomorrowNightBlueDark = 25,
        TomorrowNightBrightDark = 26,
        TomorrowNightEightiesDark = 27,
        TwilightDark = 28,
        VibrantInkDark = 29,
    }
}