//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class TimePicker : DataTextBox
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string kendoFunction = "kendoTimePicker";

            string script = string.Format(
                @"$(document).ready(function() {{
                    $('#{0}').{1}();
                }});",
                this.ClientID,
                kendoFunction );

            ScriptManager.RegisterClientScriptBlock( this.Page, typeof( Page ), "KendoTimePickerScript_" + this.ID, script, true );
        }

        /// <summary>
        /// Gets or sets the selected time.
        /// </summary>
        /// <value>
        /// The selected time.
        /// </value>
        public TimeSpan? SelectedTime
        {
            get
            {
                if ( !string.IsNullOrWhiteSpace( this.Text ) )
                {
                    DateTime result;
                    if ( DateTime.TryParse( this.Text, out result ) )
                    {
                        return result.TimeOfDay;
                    }
                    else
                    {
                        ShowErrorMessage( Rock.Constants.WarningMessage.DateTimeFormatInvalid( this.PropertyName ) );
                    }
                }

                return null;
            }

            set
            {
                if ( value != null )
                {
                    this.Text = value.Value.ToTimeString();
                }
                else
                {
                    this.Text = string.Empty;
                }
            }
        }
    }
}