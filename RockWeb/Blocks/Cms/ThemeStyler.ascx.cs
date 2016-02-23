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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Humanizer;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;
using dotless.Core;
using Rock.Utility;
using System.Web.UI.HtmlControls;

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

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
                // load themes
                DirectoryInfo dInfo = new DirectoryInfo( Request.PhysicalApplicationPath + "Themes" );
                DirectoryInfo[] subdirs = dInfo.GetDirectories();

                ddlTheme.DataSource = subdirs;
                ddlTheme.DataTextField = "Name";
                ddlTheme.DataValueField = "Name";
                ddlTheme.DataBind();

                ddlTheme.Items.Insert( 0, "" );

                if ( !string.IsNullOrWhiteSpace( Request["EditTheme"] )){
                    ddlTheme.SelectedValue = Request["EditTheme"];
                    btnSave.Visible = true;
                    ceOverrides.Visible = true;

                    // load the CSS overrides                
                    LoadCssOverrides();
                }
            }

            BuildControls();
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }


        protected void ddlTheme_SelectedIndexChanged( object sender, EventArgs e )
        {
            // load the theme
            if ( !string.IsNullOrWhiteSpace( ddlTheme.SelectedValue )){

                btnSave.Visible = true;
                ceOverrides.Visible = true;

                // load the CSS overrides                
                LoadCssOverrides();

            } else
            {
                btnSave.Visible = false;
                ceOverrides.Visible = false;
            }
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            string variableFile = string.Format( @"{0}Themes/{1}/Styles/_variables.less", Request.PhysicalApplicationPath, ddlTheme.SelectedValue );
            string variableOverrideFile = string.Format( @"{0}Themes/{1}/Styles/_variable-overrides.less", Request.PhysicalApplicationPath, ddlTheme.SelectedValue );
            string cssOverrideFile = string.Format( @"{0}Themes/{1}/Styles/_css-overrides.less", Request.PhysicalApplicationPath, ddlTheme.SelectedValue );


            if ( File.Exists( cssOverrideFile ) )
            {
                File.WriteAllText( cssOverrideFile, ceOverrides.Text );
            }

            // get list of original values
            Dictionary<string, string> originalValues = GetVariables( variableFile );

            StringBuilder overrideFile = new StringBuilder();

            foreach (var control in phThemeControls.Controls )
            {
                if ( control is TextBox )
                {
                    var textBoxControl = (TextBox)control;
                    string variableName = textBoxControl.ID.Replace(" ", "-").ToLower();

                    // find original value
                    if ( originalValues.ContainsKey( variableName )){

                        string originalValue = originalValues[variableName];

                        // color picker will convert #fff to #ffffff so take that into account
                        string secondaryValue = string.Empty;
                        if ( originalValue.Length == 4 && originalValue[0] == '#' )
                        {
                            secondaryValue = originalValue + originalValue.Substring( 1, 3 );
                        }

                        if ( originalValue != textBoxControl.Text && secondaryValue != textBoxControl.Text )
                        {
                            overrideFile.Append( string.Format( "@{0}: {1};{2}", variableName, textBoxControl.Text, Environment.NewLine ) );
                        }
                    }
                }
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter( variableOverrideFile );
            file.WriteLine( overrideFile );
            file.Dispose();

            // compile theme
            string messages = string.Empty;
            RockLess.CompileTheme( ddlTheme.SelectedValue, out messages );

            // redirect to the page to reload the pages styles
            Response.Redirect( Request.Url.LocalPath + "?EditTheme=" + ddlTheme.SelectedValue );
        }
        #endregion

        #region Methods

        private void BuildControls()
        {
            bool inPanel = false;

            if ( !string.IsNullOrWhiteSpace( ddlTheme.SelectedValue ) )
            {
                string variableFile = string.Format( @"{0}Themes/{1}/Styles/_variables.less", Request.PhysicalApplicationPath, ddlTheme.SelectedValue );
                string variableOverrideFile = string.Format( @"{0}Themes/{1}/Styles/_variable-overrides.less", Request.PhysicalApplicationPath, ddlTheme.SelectedValue );

                if ( !File.Exists( variableFile ) || !File.Exists( variableOverrideFile ) )
                {
                    nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                    nbMessages.Text = "This theme does not have a variables file(s) to allow overriding.";
                    return;
                }

                // get list of current overrides
                Dictionary<string, string> overrides = GetVariables( variableOverrideFile );

                foreach ( string line in File.ReadLines( variableFile ) )
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

                        if (lineParts.Length > 0 )
                        {
                            headerText = lineParts[0].Replace(@"/", "");
                        }

                        if (lineParts.Length > 1 )
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
                            content.Append( "</div></div>" );
                            inPanel = false;
                        }

                        bool showInEditor = false;
                        if ( title.Contains( "*show in editor*" ) )
                        {
                            showInEditor = true;
                            title = title.Replace( "*show in editor*", "" );
                        }

                        if ( showInEditor )
                        {
                            inPanel = true;

                            content.Append( "<div class='panel panel-widget'>" );
                            content.Append( "<div class='panel-heading'>" );
                            content.Append( string.Format( "<h1 class='panel-title'>{0} <div class='pull-right'><a class='btn btn-link btn-xs js-panel-toggle'><i class='fa fa-chevron-up'></i></a></div></h1>", title ) );
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
                            List<string> lessColorFunctions = new List<string>(){ "lighten", "darken", "saturate", "desaturate", "fadein", "fadeout", "fade", "spin", "mix" };
                            if ( variableParts[2].Contains( "#color" ) && !variableParts[1].StartsWith("@") && !lessColorFunctions.Any(x => variableParts[1].StartsWith(x)) ) // todo check for less color functions
                            {
                                variableType = VariableType.Color;
                            }

                            // get help
                            helpText = variableParts[2].Replace( "#color", "" ).Replace( "//", "" ).Trim();
                        }

                        // get variable name
                        string variableName = variableParts[0].Replace( "@", "" ).Replace( "-", " " ).Titleize();
                        string variableKey = variableParts[0].Replace( "@", "" );

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
                                        //colorPicker.CssClass = "input-width-lg";

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
                                        textbox.CssClass = "input-width-xxl";

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

                if ( inPanel || 0==0)
                {
                    Literal header = new Literal();
                    header.Text = "</div></div>";
                    AddControl( header, true );
                }
            }
        }

        private void AddControl(Control control, bool inPanel)
        {
            if ( inPanel )
            {
                phThemeControls.Controls.Add( control );
            }
        }

        private Dictionary<string,string> GetVariables(string filePath )
        {
            Dictionary<string, string> overrides = new Dictionary<string, string>();

            if ( File.Exists( filePath ) )
            {
                foreach ( string line in File.ReadLines( filePath ) )
                {
                    if ( line.Replace(" ", "").Contains( "/*Custom CSS*/" ) )
                    {
                        break;
                    }

                    char[] delimiters = new char[] { ':', ';' };
                    string[] variableParts = line.Split( delimiters );

                    if (variableParts.Length > 2 )
                    {
                        overrides.Add( variableParts[0].Replace( "@", "" ).ToLower(), variableParts[1].Trim() );
                    }
                }
            }

            return overrides;
        }

        private void LoadCssOverrides()
        {
            // load the CSS overrides                
            string overrideFile = string.Format( @"{0}Themes/{1}/Styles/_css-overrides.less", Request.PhysicalApplicationPath, ddlTheme.SelectedValue );
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

        enum VariableType
        {
            Text,
            Color,
            Number
        }
    }
}