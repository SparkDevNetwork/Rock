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
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            base.RenderControl( writer );

            string kendoFunction;
            if (DatePickerType.Equals(DateTimePickerType.Date))
            {
                kendoFunction = "kendoDatePicker";
            }
            else
            {
                kendoFunction = "kendoDateTimePicker";
            }

            string script = string.Format( @"
                $(document).ready(function() {{
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