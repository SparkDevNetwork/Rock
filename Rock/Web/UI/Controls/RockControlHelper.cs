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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Helper class to intialize and render rock controls with Bootstrap html elements
    /// </summary>
    internal static class RockControlHelper
    {
        /// <summary>
        /// Inits the specified rock control.
        /// </summary>
        /// <param name="rockControl">The rock control.</param>
        public static void Init( IRockControl rockControl )
        {
            rockControl.RequiredFieldValidator = new RequiredFieldValidator();
            rockControl.HelpBlock = new HelpBlock();
        }

        /// <summary> 
        /// Creates the child controls and handles adding the required field validator control.
        /// </summary>
        /// <param name="rockControl">The rock control.</param>
        /// <param name="controls">The controls.</param>
        public static void CreateChildControls( IRockControl rockControl, ControlCollection controls )
        {
            if ( rockControl.RequiredFieldValidator != null )
            {
                rockControl.RequiredFieldValidator.ID = rockControl.ID + "_rfv";
                rockControl.RequiredFieldValidator.ControlToValidate = rockControl.ID;
                rockControl.RequiredFieldValidator.Display = ValidatorDisplay.Dynamic;
                rockControl.RequiredFieldValidator.CssClass = "validation-error help-inline";
                rockControl.RequiredFieldValidator.Enabled = rockControl.Required;
                controls.Add( rockControl.RequiredFieldValidator );
            }

            if ( rockControl.HelpBlock != null )
            {
                rockControl.HelpBlock.ID = rockControl.ID + "_hb";
                controls.Add( rockControl.HelpBlock );
            }
        }

        /// <summary>
        /// Renders the control which handles adding all the IRockControl common pieces (Label, Help, etc.).
        /// </summary>
        /// <param name="rockControl">The rock control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="additionalCssClass">The additional CSS class.</param>
        public static void RenderControl( IRockControl rockControl, HtmlTextWriter writer, string additionalCssClass = "" )
        {
            bool renderLabel = ( !string.IsNullOrEmpty( rockControl.Label ) );
            bool renderHelp = ( rockControl.HelpBlock != null && !string.IsNullOrWhiteSpace( rockControl.Help ) );

            if ( renderLabel )
            {
                var cssClass = new StringBuilder();
                cssClass.AppendFormat( "form-group {0}", rockControl.GetType().Name.SplitCase().Replace( ' ', '-' ).ToLower() );
                if ( ( (Control)rockControl ).Page.IsPostBack && !rockControl.IsValid )
                {
                    cssClass.Append( " has-error" );
                }
                if ( rockControl.Required )
                {
                    if ( ( rockControl is IDisplayRequiredIndicator ) && !( rockControl as IDisplayRequiredIndicator ).DisplayRequiredIndicator )
                    {
                        // if this is a rock control that implements IDisplayRequiredIndicator and DisplayRequiredIndicator is false, don't add the " required " cssclass
                    }
                    else
                    {
                        cssClass.Append( " required" );
                    }
                }
                if (!string.IsNullOrWhiteSpace(additionalCssClass))
                {
                    cssClass.Append( " " + additionalCssClass );
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, cssClass.ToString() );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( !( rockControl is RockLiteral ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                    writer.AddAttribute( HtmlTextWriterAttribute.For, rockControl.ClientID );
                }
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( rockControl.Label );

                if ( renderHelp )
                {
                    rockControl.HelpBlock.RenderControl( writer );
                }

                writer.RenderEndTag();
            }

            rockControl.RenderBaseControl( writer );

            if ( !renderLabel && renderHelp )
            {
                rockControl.HelpBlock.RenderControl( writer );
            }

            if ( rockControl.RequiredFieldValidator != null )
            {
                if ( rockControl.Required )
                {
                    rockControl.RequiredFieldValidator.Enabled = true;
                    if ( string.IsNullOrWhiteSpace( rockControl.RequiredFieldValidator.ErrorMessage ) )
                    {
                        rockControl.RequiredFieldValidator.ErrorMessage = rockControl.Label + " is Required.";
                    }
                    rockControl.RequiredFieldValidator.RenderControl( writer );
                }
                else
                {
                    rockControl.RequiredFieldValidator.Enabled = false;
                }
            }

            if ( renderLabel )
            {
                writer.RenderEndTag();
            }
        }

        public static void RenderControl( string label, Control control, HtmlTextWriter writer )
        {
            bool renderLabel = ( !string.IsNullOrEmpty( label ) );

            if ( renderLabel )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.AddAttribute( HtmlTextWriterAttribute.For, control.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( label );
                writer.RenderEndTag();  // label

                control.RenderControl( writer );

                writer.RenderEndTag();  // form-group
            }
            else
            {
                control.RenderControl( writer );
            }
        }

    }
}
