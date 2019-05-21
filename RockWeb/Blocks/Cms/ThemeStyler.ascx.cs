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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Humanizer;

using Rock;
using Rock.Model;
using Rock.Utility;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Theme Styler" )]
    [Category( "CMS" )]
    [Description( "Allows you to change the LESS variables of a theme." )]
    public partial class ThemeStyler : Rock.Web.UI.RockBlock
    {
        #region Fields

        private string _themeName = string.Empty;
        private string _variableFile = null;
        private string _variableOverrideFile = null;

        #endregion

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

            string script = @"
$('.js-panel-toggle').on('click', function (e) {
    e.stopImmediatePropagation();
    e.preventDefault();
    var $header = $(this).closest('.panel-heading');
    $header.siblings('.panel-body').slideToggle();
    $header.find('i').toggleClass('fa-chevron-up fa-chevron-down');
});
";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "DefinedValueChecklistScript", script, true );

            _themeName = PageParameter( "EditTheme" );

            _variableFile = string.Format( @"{0}Themes/{1}/Styles/_variables.less", Request.PhysicalApplicationPath, _themeName );
            _variableOverrideFile = string.Format( @"{0}Themes/{1}/Styles/_variable-overrides.less", Request.PhysicalApplicationPath, _themeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();

                ShowDetail( _themeName );
            }

            BuildControls();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // intentionally blank
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            string cssOverrideFile = string.Format( @"{0}Themes/{1}/Styles/_css-overrides.less", Request.PhysicalApplicationPath, _themeName );

            if ( File.Exists( cssOverrideFile ) )
            {
                File.WriteAllText( cssOverrideFile, ceOverrides.Text );
            }

            // get list of original values
            Dictionary<string, string> originalValues = GetVariables( _variableFile );

            StringBuilder overrideFile = new StringBuilder();

            if ( pnlFontAwesomeSettings.Visible )
            {
                overrideFile.AppendLine( FontAwesomeHelper.VariableOverridesTokens.StartRegion );

                var selectedPrimaryWeight = FontAwesomeHelper.FontAwesomeIconCssWeights.FirstOrDefault( a => a.WeightName == ddlFontAwesomeIconWeight.SelectedValue );

                overrideFile.AppendLine( string.Format( "{0} {1};", FontAwesomeHelper.VariableOverridesTokens.FontWeightValueLineStart, selectedPrimaryWeight.WeightValue ) );
                overrideFile.AppendLine( string.Format( "{0} '{1}';", FontAwesomeHelper.VariableOverridesTokens.FontWeightNameLineStart, selectedPrimaryWeight.WeightName ) );

                if ( FontAwesomeHelper.HasFontAwesomeProKey() )
                {
                    overrideFile.AppendLine( "@fa-edition: 'pro';" );
                }

                overrideFile.AppendLine();

                if ( !selectedPrimaryWeight.IncludedInFree )
                {
                    overrideFile.AppendLine( string.Format( "{0} {1};", FontAwesomeHelper.VariableOverridesTokens.FontEditionLineStart, FontAwesomeHelper.VariableOverridesTokens.FontEditionPro ) );
                }

                overrideFile.AppendLine( "@import \"../../../Styles/FontAwesome/_rock-fa-mixins.less\";" );

                foreach ( var alternateFontWeightName in cblFontAwesomeAlternateFonts.Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value ).ToList() )
                {
                    var alternateFont = FontAwesomeHelper.FontAwesomeIconCssWeights.Where( a => a.WeightName == alternateFontWeightName ).FirstOrDefault();
                    if ( alternateFont != null )
                    {
                        string suffixParam = string.Empty;
                        overrideFile.AppendLine(
                            string.Format( "{0} '{1}', 'pro' );",
                                FontAwesomeHelper.VariableOverridesTokens.FontFaceLineStart,
                                alternateFont.WeightName
                                ) );
                    }
                }

                overrideFile.AppendLine( FontAwesomeHelper.VariableOverridesTokens.EndRegion );
            }

            foreach ( var control in phThemeControls.Controls )
            {
                if ( control is TextBox )
                {
                    var textBoxControl = ( TextBox ) control;
                    string variableName = textBoxControl.ID.Replace( " ", "-" ).ToLower();

                    // find original value
                    if ( originalValues.ContainsKey( variableName ) )
                    {
                        string originalValue = originalValues[variableName];

                        // color picker will convert #fff to #ffffff so take that into account
                        string secondaryValue = string.Empty;
                        if ( originalValue.Length == 4 && originalValue[0] == '#' )
                        {
                            secondaryValue = originalValue + originalValue.Substring( 1, 3 );
                        }

                        if ( originalValue.ToLower() != textBoxControl.Text.ToLower() && secondaryValue.ToLower() != textBoxControl.Text.ToLower() )
                        {
                            overrideFile.Append( string.Format( "@{0}: {1};{2}", variableName, textBoxControl.Text, Environment.NewLine ) );
                        }
                    }
                }
            }

            File.WriteAllText( _variableOverrideFile, overrideFile.ToString() );

            // compile theme
            string messages = string.Empty;
            var theme = new RockTheme( _themeName );
            if ( !theme.Compile( out messages ) )
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Danger;
                nbMessages.Text = string.Format( "An error occurred while compiling the {0} theme.\nMessage: <pre>{1}</pre>", theme.Name, messages );;
                nbMessages.Visible = true;
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFontAwesomeIconWeight control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlFontAwesomeIconWeight_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedWeight = ddlFontAwesomeIconWeight.SelectedValue;
            LoadAlternateFontsCheckboxList( selectedWeight );
        }

        /// <summary>
        /// Loads the alternate fonts checkbox list.
        /// </summary>
        /// <param name="selectedWeight">The selected weight.</param>
        private void LoadAlternateFontsCheckboxList( string selectedWeight )
        {
            List<FontAwesomeHelper.FontAwesomeIconCssWeight> alternateFontWeights = null;
            if ( FontAwesomeHelper.HasFontAwesomeProKey() )
            {
                alternateFontWeights = FontAwesomeHelper.FontAwesomeIconCssWeights.Where( a => a.IsConfigurable && a.WeightName != selectedWeight ).ToList();
            }
            else
            {
                alternateFontWeights = FontAwesomeHelper.FontAwesomeIconCssWeights.Where( a => a.IsConfigurable && a.IncludedInFree && a.WeightName != selectedWeight ).ToList();
            }

            if ( alternateFontWeights.Count > 0 )
            {
                cblFontAwesomeAlternateFonts.Visible = true;
                var selectedAlternateFonts = cblFontAwesomeAlternateFonts.Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value ).ToList();
                cblFontAwesomeAlternateFonts.Items.Clear();
                foreach ( var fontWeight in alternateFontWeights )
                {
                    cblFontAwesomeAlternateFonts.Items.Add( new ListItem( fontWeight.DisplayName, fontWeight.WeightName ) { Selected = selectedAlternateFonts.Contains( fontWeight.WeightName ) } );
                }
            }
            else
            {
                cblFontAwesomeAlternateFonts.Visible = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail( string themeName )
        {
            lThemeName.Text = themeName + " Theme";

            // Font Awesome stuff
            // TODO: Read LESS files for current setup

            // Only show the FontAwesome settings if the _variableOverrideFile exists
            if ( File.Exists( _variableOverrideFile ) )
            {
                pnlFontAwesomeSettings.Visible = true;

                var fileLines = File.ReadAllLines( _variableOverrideFile ).ToList();

                int fontAwesomeRegionStartLine = fileLines.IndexOf( FontAwesomeHelper.VariableOverridesTokens.StartRegion );
                int fontAwesomeRegionEndLine = fileLines.IndexOf( FontAwesomeHelper.VariableOverridesTokens.EndRegion );
                List<string> fontAwesomeLines = new List<string>();
                if ( fontAwesomeRegionStartLine >= 0 && fontAwesomeRegionEndLine > fontAwesomeRegionStartLine )
                {
                    for ( int currentLine = fontAwesomeRegionStartLine; currentLine < fontAwesomeRegionEndLine; currentLine++ )
                    {
                        fontAwesomeLines.Add( fileLines[currentLine] );
                    }
                }

                string primaryWeight = "solid";
                List<string> alternateFonts = new List<string>();

                if ( fontAwesomeLines.Any() )
                {
                    primaryWeight = fontAwesomeLines.Where( a => a.StartsWith( FontAwesomeHelper.VariableOverridesTokens.FontWeightNameLineStart ) )
                        .Select( a => a.Split( new char[] { '\'' } ) ).Where( a => a.Length == 3 ).Select( a => a[1] ).FirstOrDefault();

                    alternateFonts = fontAwesomeLines.Where( a => a.StartsWith( FontAwesomeHelper.VariableOverridesTokens.FontFaceLineStart ) ).Select( a => a.Split( new char[] { '\'' } ) )
                        .Where( a => a.Length >= 3 ).Select( a => a[1] ).ToList();
                }

                ddlFontAwesomeIconWeight.SetValue( primaryWeight );
                LoadAlternateFontsCheckboxList( ddlFontAwesomeIconWeight.SelectedValue );
                foreach ( var cbAlternateFont in cblFontAwesomeAlternateFonts.Items.OfType<ListItem>() )
                {
                    cbAlternateFont.Selected = alternateFonts.Contains( cbAlternateFont.Value );
                }
            }
            else
            {
                pnlFontAwesomeSettings.Visible = false;
            }

            // Other theme stuff
            LoadCssOverrides();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlFontAwesomeIconWeight.Items.Clear();

            if ( FontAwesomeHelper.HasFontAwesomeProKey() )
            {
                // If they have pro, any of the weights can be used as the primary weight
                foreach ( var fontAwesomeIconCssWeight in FontAwesomeHelper.FontAwesomeIconCssWeights.Where( a => a.IsConfigurable ) )
                {
                    ddlFontAwesomeIconWeight.Items.Add( new ListItem( fontAwesomeIconCssWeight.DisplayName, fontAwesomeIconCssWeight.WeightName ) );
                }
            }
            else
            {
                // If they don't have pro, include list the weights that are included in the free version, and are allowed to be used a primary weight
                foreach ( var fontAwesomeIconCssWeight in FontAwesomeHelper.FontAwesomeIconCssWeights.Where( a => a.IsConfigurable && a.IncludedInFree && !a.RequiresProForPrimary ) )
                {
                    ddlFontAwesomeIconWeight.Items.Add( new ListItem( fontAwesomeIconCssWeight.DisplayName, fontAwesomeIconCssWeight.WeightName ) );
                }
            }
        }

        /// <summary>
        /// Builds the controls.
        /// </summary>
        private void BuildControls()
        {
            bool inPanel = false;
            List<string> lessColorFunctions = new List<string>() { "lighten", "darken", "saturate", "desaturate", "fadein", "fadeout", "fade", "spin", "mix" };

            /*Rock.Web.UI.Controls.ImageUploader fupTest = new Rock.Web.UI.Controls.ImageUploader();
            fupTest.ID = "test";
            fupTest.BinaryFileTypeGuid
            phThemeControls.Controls.Add( fupTest );*/

            if ( !string.IsNullOrWhiteSpace( _themeName ) )
            {
                if ( !File.Exists( _variableFile ) || !File.Exists( _variableOverrideFile ) )
                {
                    nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                    nbMessages.Text = "This theme does not have a variables file(s) to allow overriding.";
                    btnSave.Visible = false;
                    return;
                }

                // get list of current overrides
                Dictionary<string, string> overrides = GetVariables( _variableOverrideFile );

                foreach ( string line in File.ReadLines( _variableFile ) )
                {
                    if ( line.Left( 4 ) == @"//--" )
                    {
                        Literal spacing = new Literal();
                        spacing.Text = "<br/><br/>";
                        AddControl( spacing, inPanel );
                    }
                    else if ( line.Left( 4 ) == @"////" )
                    {
                        Literal header = new Literal();
                        string[] lineParts = line.Split( new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries );

                        string headerText = string.Empty;
                        string summaryText = string.Empty;

                        if ( lineParts.Length > 0 )
                        {
                            headerText = lineParts[0].Replace( @"/", string.Empty );
                        }

                        if ( lineParts.Length > 1 )
                        {
                            summaryText = lineParts[1];
                        }

                        header.Text = string.Format( "<h4>{0}</h4><p>{1}</p>", headerText, summaryText );
                        AddControl( header, inPanel );
                    }
                    else if ( line.Left( 2 ) == @"//" )
                    {
                        string title = line.Substring( 2 );

                        StringBuilder content = new StringBuilder();

                        // panel
                        if ( inPanel )
                        {
                            Literal panelClose = new Literal();
                            panelClose.Text = "</div></div>";
                            AddControl( panelClose, true );

                            inPanel = false;
                        }

                        bool showInEditor = false;
                        if ( title.Contains( "*show in editor*" ) )
                        {
                            showInEditor = true;
                            title = title.Replace( "*show in editor*", string.Empty );
                        }

                        if ( showInEditor )
                        {
                            inPanel = true;

                            content.Append( "<div class='panel panel-widget'>" );
                            content.Append( "<div class='panel-heading'>" );
                            content.Append( string.Format( "<h1 class='panel-title'>{0}</h1><div class='pull-right'><a class='btn btn-link btn-xs js-panel-toggle'><i class='fa fa-chevron-up'></i></a></div>", title ) );
                            content.Append( "</div>" );
                            content.Append( "<div class='panel-body'>" );

                            Literal header = new Literal();
                            header.Text = content.ToString();
                            AddControl( header, inPanel );
                        }
                    }
                    else if ( line.Left( 1 ) == "@" )
                    {
                        // variable
                        char[] delimiters = new char[] { ':', ';' };
                        string[] variableParts = Array.ConvertAll( line.Split( delimiters ), p => p.Trim() );

                        string helpText = string.Empty;

                        // determine type
                        VariableType variableType = VariableType.Text;
                        if ( variableParts.Length >= 3 )
                        {
                            // determine if we should use a color control. only do this if:
                            //     - if the comments tell us it's a color (#color)
                            //     - it's not a less variable (starts with a @)
                            //     - it's not a less color function

                            // todo check for less color functions
                            if ( variableParts[2].Contains( "#color" ) && !variableParts[1].StartsWith( "@" ) && !lessColorFunctions.Any( x => variableParts[1].StartsWith( x ) ) )
                            {
                                variableType = VariableType.Color;
                            }

                            // get help
                            helpText = variableParts[2].Replace( "#color", string.Empty ).Replace( "//", string.Empty ).Trim();
                        }

                        // get variable name
                        string variableName = variableParts[0].Replace( "@", string.Empty ).Replace( "-", " " ).Titleize();
                        string variableKey = variableParts[0].Replace( "@", string.Empty );

                        // get variable value
                        string variableValue = string.Empty;
                        if ( variableParts.Length > 1 )
                        {
                            variableValue = variableParts[1].Trim();
                        }

                        Literal overrideControl = new Literal();

                        switch ( variableType )
                        {
                            case VariableType.Color:
                                {
                                    if ( phThemeControls.FindControl( variableName ) == null )
                                    {
                                        ColorPicker colorPicker = new ColorPicker();
                                        colorPicker.ID = variableKey;
                                        colorPicker.Label = variableName;

                                        // check if override of the variable exists
                                        if ( overrides.ContainsKey( variableKey ) )
                                        {
                                            colorPicker.Text = overrides[variableKey];

                                            // add restore logic
                                            overrideControl.Text = string.Format( "<i class='fa fa-times margin-l-sm js-color-override variable-override pull-left' style='margin-top: 34px; cursor: pointer;' data-control='{0}' data-original-value='{1}'></i>", variableKey, variableValue );
                                        }
                                        else
                                        {
                                            colorPicker.Text = variableValue;
                                        }

                                        if ( !string.IsNullOrWhiteSpace( helpText ) )
                                        {
                                            colorPicker.Help = helpText;
                                        }

                                        colorPicker.RequiredFieldValidator = null;
                                        colorPicker.FormGroupCssClass = "pull-left";

                                        Literal beginWrapper = new Literal();
                                        beginWrapper.Text = "<div class='clearfix'>";

                                        AddControl( beginWrapper, inPanel );
                                        AddControl( colorPicker, inPanel );
                                        AddControl( overrideControl, inPanel );

                                        Literal endWrapper = new Literal();
                                        endWrapper.Text = "</div>";
                                        AddControl( endWrapper, inPanel );
                                    }

                                    break;
                                }

                            default:
                                {
                                    if ( phThemeControls.FindControl( variableName ) == null )
                                    {
                                        RockTextBox textbox = new RockTextBox();
                                        textbox.Label = variableName;
                                        textbox.ID = variableKey;
                                        textbox.CssClass = "input-width-xl";
                                        textbox.Help = helpText;

                                        // check if override of the variable exists
                                        if ( overrides.ContainsKey( variableKey ) )
                                        {
                                            textbox.Text = overrides[variableKey];

                                            // add restore logic
                                            overrideControl.Text = string.Format( "<i class='fa fa-times margin-l-sm js-text-override variable-override pull-left' style='margin-top: 34px; cursor: pointer;' data-control='{0}' data-original-value='{1}'></i>", variableKey, variableValue );
                                        }
                                        else
                                        {
                                            textbox.Text = variableValue;
                                        }

                                        textbox.FormGroupCssClass = "pull-left";
                                        textbox.RequiredFieldValidator = null;

                                        Literal beginWrapper = new Literal();
                                        beginWrapper.Text = "<div class='clearfix'>";

                                        AddControl( beginWrapper, inPanel );
                                        AddControl( textbox, inPanel );
                                        AddControl( overrideControl, inPanel );

                                        Literal endWrapper = new Literal();
                                        endWrapper.Text = "</div>";
                                        AddControl( endWrapper, inPanel );
                                    }

                                    break;
                                }
                        }
                    }
                }

                if ( inPanel )
                {
                    Literal panelClose = new Literal();
                    panelClose.Text = "</div></div>";
                    AddControl( panelClose, true );
                }

                if ( phThemeControls.Controls.Count == 0 && !pnlFontAwesomeSettings.Visible )
                {
                    btnSave.Visible = false;
                    nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                    nbMessages.Text = "This theme does not define any variables for editing.";
                }
            }
        }

        /// <summary>
        /// Adds the control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="inPanel">if set to <c>true</c> [in panel].</param>
        private void AddControl( Control control, bool inPanel )
        {
            if ( inPanel )
            {
                phThemeControls.Controls.Add( control );
            }
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private Dictionary<string, string> GetVariables( string filePath )
        {
            Dictionary<string, string> overrides = new Dictionary<string, string>();

            if ( File.Exists( filePath ) )
            {
                foreach ( string line in File.ReadLines( filePath ) )
                {
                    if ( line.Replace( " ", string.Empty ).Contains( "/*Custom CSS*/" ) )
                    {
                        break;
                    }

                    char[] delimiters = new char[] { ':', ';' };
                    string[] variableParts = line.Split( delimiters );

                    if ( variableParts.Length > 2 )
                    {
                        overrides.AddOrReplace( variableParts[0].Replace( "@", string.Empty ).ToLower(), variableParts[1].Trim() );
                    }
                }
            }

            return overrides;
        }

        /// <summary>
        /// Loads the CSS overrides.
        /// </summary>
        private void LoadCssOverrides()
        {
            // load the CSS overrides
            string overrideFile = string.Format( @"{0}Themes/{1}/Styles/_css-overrides.less", Request.PhysicalApplicationPath, _themeName );
            if ( File.Exists( overrideFile ) )
            {
                ceOverrides.Text += File.ReadAllText( overrideFile );
            }
            else
            {
                ceOverrides.Visible = false;
            }
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        public enum VariableType
        {
            /// <summary>
            /// The text
            /// </summary>
            Text,

            /// <summary>
            /// The color
            /// </summary>
            Color,

            /// <summary>
            /// The number
            /// </summary>
            Number
        }
    }
}