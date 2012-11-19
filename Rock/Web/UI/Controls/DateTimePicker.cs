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
    public class DateTimePicker : DataTextBox
    {
        /// <summary>
        /// Gets or sets the type of the date picker.
        /// </summary>
        /// <value>
        /// The type of the date picker.
        /// </value>
        public DateTimePickerType DatePickerType
        {
            get
            {
                var pickerType = ViewState["DatePickerType"];
                if ( pickerType != null )
                {
                    return (DateTimePickerType)pickerType;
                }
                else
                {
                    return DateTimePickerType.Date;
                }
            }
            
            set
            {
                ViewState["DatePickerType"] = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string kendoFunction;
            if (DatePickerType.Equals(DateTimePickerType.Date))
            {
                kendoFunction = "kendoDatePicker";
            }
            else
            {
                kendoFunction = "kendoDateTimePicker";
            }

            string script = string.Format( 
                @"$(document).ready(function() {{
                    $('#{0}').{1}();
                }});",
                this.ClientID,
                kendoFunction);

            ScriptManager.RegisterClientScriptBlock( this.Page, typeof( Page ), "KendoDatePickerScript_" + this.ID, script, true );
        }

        /// <summary>
        /// Gets the selected date.
        /// </summary>
        /// <value>
        /// The selected date.
        /// </value>
        public DateTime? SelectedDateTime
        {
            get
            {
                if ( !string.IsNullOrWhiteSpace( this.Text ) )
                {
                    DateTime result;
                    if ( DateTime.TryParse( this.Text, out result ) )
                    {
                        return result;
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
                }
                else
                {
                    this.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        /// <value>
        /// The selected date.
        /// </value>
        public DateTime? SelectedDate
        {
            get
            {
                if ( SelectedDateTime != null )
                {
                    return SelectedDateTime.Value.Date;
                }
                else
                {
                    return null;
                }
            }
            
            set
            {
                SelectedDateTime = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DateTimePickerType
    {
        /// <summary>
        /// 
        /// </summary>
        Date = 1,
        
        // we'll need to upgrade Kendo to get the DateTimePicker
        // DateTime = 2
    }
}