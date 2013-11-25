//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
                rockControl.RequiredFieldValidator.Enabled = false;
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
                    cssClass.Append( " required" );
                }
                if (!string.IsNullOrWhiteSpace(additionalCssClass))
                {
                    cssClass.Append( additionalCssClass );
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

            if ( rockControl.RequiredFieldValidator != null && rockControl.Required )
            {
                rockControl.RequiredFieldValidator.Enabled = true;
                if ( string.IsNullOrWhiteSpace( rockControl.RequiredFieldValidator.ErrorMessage ) )
                {
                    rockControl.RequiredFieldValidator.ErrorMessage = rockControl.Label + " is Required.";
                }
                rockControl.RequiredFieldValidator.ValidationGroup = rockControl.ValidationGroup;
                rockControl.RequiredFieldValidator.RenderControl( writer );
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
