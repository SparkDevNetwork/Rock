using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Helper class to intialize and render rock controls with Bootstrap html elements
    /// </summary>
    internal static class RockControl
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
        /// Creates the child controls.
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
        /// Renders the control.
        /// </summary>
        /// <param name="rockControl">The rock control.</param>
        /// <param name="writer">The writer.</param>
        public static void RenderControl( IRockControl rockControl, HtmlTextWriter writer )
        {
            bool renderLabel = ( !string.IsNullOrEmpty( rockControl.Label ) );
            bool renderHelp = ( rockControl.HelpBlock != null && !string.IsNullOrWhiteSpace( rockControl.Help ) );

            if ( renderLabel )
            {
                writer.AddAttribute( "class", "form-group" + ( rockControl.IsValid ? "" : " error" ) + ( rockControl.Required ? " required" : "" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( "for", rockControl.ClientID );
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
                rockControl.RequiredFieldValidator.ValidationGroup = rockControl.ValidationGroup;
                if ( string.IsNullOrWhiteSpace( rockControl.RequiredFieldValidator.ErrorMessage ) )
                {
                    rockControl.RequiredFieldValidator.ErrorMessage = rockControl.Label + " is Required.";
                }
                rockControl.RequiredFieldValidator.RenderControl( writer );
            }

            if ( renderLabel )
            {
                writer.RenderEndTag();
            }
        }
    }
}
