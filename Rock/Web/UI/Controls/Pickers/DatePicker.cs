//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class DatePicker : DataTextBox
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.CssClass = "input-width-md";
            this.AppendText = "<i class='icon-calendar'></i>";

            if ( string.IsNullOrWhiteSpace( this.SourceTypeName ) )
            {
                this.LabelTextFromPropertyName = false;
                this.SourceTypeName = "Rock.Web.UI.Controls.DatePicker, Rock";
                this.PropertyName = "SelectedDate";
            }

            var script = string.Format( @"Rock.controls.datePicker.initialize({{ id: '{0}' }});", this.ClientID );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "date_picker-" + this.ClientID, script, true );
        }

        /// <summary>
        /// Gets the selected date.
        /// </summary>
        /// <value>
        /// The selected date.
        /// </value>
        public DateTime? SelectedDate
        {
            get
            {
                if ( !string.IsNullOrWhiteSpace( this.Text ) )
                {
                    DateTime result;
                    if ( DateTime.TryParse( this.Text, out result ) )
                    {
                        return result.Date;
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
                    this.Text = value.Value.ToShortDateString();
                    this.Attributes["value"] = this.Text;
                }
                else
                {
                    this.Text = string.Empty;
                }
            }
        }

    }
}