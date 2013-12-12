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

using Rock;

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
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ValidationGroup = value;
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height of the control.
        /// </value>
        [
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The height in pixels of the control" )
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
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The language of the code to be edited" )
        ]
        public CodeEditorMode EditorMode
        {
            get
            {
                if ( ViewState["EditorMode"] != null )
                {
                    return ViewState["EditorMode"].ToString().ConvertToEnum<CodeEditorMode>( CodeEditorMode.Text );
                }
                else
                {
                    // Default value
                    return CodeEditorMode.Text;
                }
            }

            set
            {
                ViewState["EditorMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the editor theme.
        /// </summary>
        /// <value>
        /// The theme of the editor.
        /// </value>
        [
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The theme of the editor" )
        ]
        public CodeEditorTheme EditorTheme
        {
            get
            {
                if ( ViewState["EditorTheme"] != null )
                {
                    return ViewState["EditorTheme"].ToString().ConvertToEnum<CodeEditorTheme>( CodeEditorTheme.Rock );
                }
                else
                {
                    // Default value
                    return CodeEditorTheme.Rock;
                }
            }

            set
            {
                ViewState["EditorTheme"] = value;
            }
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
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/ace/ace.js" ) );
            // Note: AddScriptLink will not add a script during partial postback (i.e. inside an update panel)  Because of this, this script 
            // is also added in the AttributeEditor's OnInit since in that scenario this controls OnInit may not get run on the initial page load

            this.TextMode = TextBoxMode.MultiLine;
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

            // add editor div
            string height = string.IsNullOrWhiteSpace( EditorHeight ) ? "200" : EditorHeight;
            string customDiv = @"<div class='code-editor-container' style='position:relative; height: {0}px'><pre id='codeeditor-div-{1}'>{2}</pre></div>";
            writer.Write( string.Format( customDiv, height, this.ClientID, HttpUtility.HtmlEncode( this.Text ) ) );

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
            string cssCode = string.Format( customStyle, this.ClientID );
            writer.Write( cssCode );

            // make textbox hidden
            ( (WebControl)this ).Style.Add( HtmlTextWriterStyle.Display, "none" );

            string scriptFormat = @"
                var ce_{0} = ace.edit('codeeditor-div-{0}');
                ce_{0}.setTheme('ace/theme/{1}');
                ce_{0}.getSession().setMode('ace/mode/{2}');
                ce_{0}.setShowPrintMargin(false);

                ce_{0}.getSession().on('change', function(e) {{
                    document.getElementById('{0}').value = ce_{0}.getValue();
                }});
";
            string script = string.Format( scriptFormat, this.ClientID, EditorThemeAsString( this.EditorTheme ), EditorModeAsString( this.EditorMode ) );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "codeeditor_" + this.ID, script, true );

            base.RenderControl( writer );

        }

        /// <summary>
        /// Gets the mode of the editor as text based on property
        /// </summary>
        /// <returns>The text value of the mode.</returns>
        private string EditorModeAsString( CodeEditorMode mode )
        {
            string[] modeValues = new string[] { "text", "css", "html", "liquid", "javascript", "less", "powershell", "sql", "typescript", "csharp", "markdown" };

            return modeValues[(int)mode];
        }

        /// <summary>
        /// Gets the theme of the editor as text based on property
        /// </summary>
        /// <returns>The text value of the mode.</returns>
        private string EditorThemeAsString( CodeEditorTheme theme )
        {
            string[] themeValues = new string[] { "github", "chrome", "crimson_editor", "dawn", "dreamweaver", "eclipse", "solarized_light", "textmate", 
                "tomorrow", "xcode", "github", "ambiance", "chaos", "clouds_midnight", "cobalt", "idle_fingers", "kr_theme", 
                "merbivore", "merbivore_soft", "mono_industrial", "monokai", "pastel_on_dark", "solarized_dark", "terminal", "tomorrow_night", "tomorrow_night_blue",
                "tomorrow_night_bright", "tomorrow_night_eighties", "twilight", "vibrant_ink"};

            return themeValues[(int)theme];
        }

    }

    /// <summary>
    /// The CodeEditor Mode
    /// </summary>
    public enum CodeEditorMode
    {
        /// <summary>
        /// text
        /// </summary>
        Text = 0,
        /// <summary>
        /// CSS
        /// </summary>
        Css = 1,
        /// <summary>
        /// HTML
        /// </summary>
        Html = 2,
        /// <summary>
        /// liquid
        /// </summary>
        Liquid = 3,
        /// <summary>
        /// java script
        /// </summary>
        JavaScript = 4,
        /// <summary>
        /// less
        /// </summary>
        Less = 5,
        /// <summary>
        /// powershell
        /// </summary>
        Powershell = 6,
        /// <summary>
        /// SQL
        /// </summary>
        Sql = 7,
        /// <summary>
        /// type script
        /// </summary>
        TypeScript = 8,
        /// <summary>
        /// c sharp
        /// </summary>
        CSharp = 9,
        /// <summary>
        /// markdown
        /// </summary>
        Markdown = 10
    }

    /// <summary>
    /// The CodeEditor Theme
    /// </summary>
    public enum CodeEditorTheme
    {
        /// <summary>
        /// rock
        /// </summary>
        Rock = 0,
        /// <summary>
        /// chrome
        /// </summary>
        Chrome = 1,
        /// <summary>
        /// crimson editor
        /// </summary>
        CrimsonEditor = 2,
        /// <summary>
        /// dawn
        /// </summary>
        Dawn = 3,
        /// <summary>
        /// dreamweaver
        /// </summary>
        Dreamweaver = 4,
        /// <summary>
        /// eclipse
        /// </summary>
        Eclipse = 5,
        /// <summary>
        /// solarized light
        /// </summary>
        SolarizedLight = 6,
        /// <summary>
        /// textmate
        /// </summary>
        Textmate = 7,
        /// <summary>
        /// tomorrow
        /// </summary>
        Tomorrow = 8,
        /// <summary>
        /// xcode
        /// </summary>
        Xcode = 9,
        /// <summary>
        /// github
        /// </summary>
        Github = 10,
        /// <summary>
        /// ambiance dark
        /// </summary>
        AmbianceDark = 11,
        /// <summary>
        /// chaos dark
        /// </summary>
        ChaosDark = 12,
        /// <summary>
        /// clouds midnight dark
        /// </summary>
        CloudsMidnightDark = 13,
        /// <summary>
        /// cobalt dark
        /// </summary>
        CobaltDark = 14,
        /// <summary>
        /// idle fingers dark
        /// </summary>
        IdleFingersDark = 15,
        /// <summary>
        /// kr theme dark
        /// </summary>
        krThemeDark = 16,
        /// <summary>
        /// merbivore dark
        /// </summary>
        MerbivoreDark = 17,
        /// <summary>
        /// merbivore soft dark
        /// </summary>
        MerbivoreSoftDark = 18,
        /// <summary>
        /// mono industrial dark
        /// </summary>
        MonoIndustrialDark = 19,
        /// <summary>
        /// monokai dark
        /// </summary>
        MonokaiDark = 20,
        /// <summary>
        /// pastel on dark
        /// </summary>
        PastelOnDark = 21,
        /// <summary>
        /// solarized dark
        /// </summary>
        SolarizedDark = 22,
        /// <summary>
        /// terminal dark
        /// </summary>
        TerminalDark = 23,
        /// <summary>
        /// tomorrow night dark
        /// </summary>
        TomorrowNightDark = 24,
        /// <summary>
        /// tomorrow night blue dark
        /// </summary>
        TomorrowNightBlueDark = 25,
        /// <summary>
        /// tomorrow night bright dark
        /// </summary>
        TomorrowNightBrightDark = 26,
        /// <summary>
        /// tomorrow night eighties dark
        /// </summary>
        TomorrowNightEightiesDark = 27,
        /// <summary>
        /// twilight dark
        /// </summary>
        TwilightDark = 28,
        /// <summary>
        /// vibrant ink dark
        /// </summary>
        VibrantInkDark = 29,
    }
}