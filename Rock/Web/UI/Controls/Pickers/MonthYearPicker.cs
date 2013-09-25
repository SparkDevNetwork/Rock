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
    public class MonthYearPicker : DataTextBox
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( string.IsNullOrWhiteSpace( this.SourceTypeName ) )
            {
                this.LabelTextFromPropertyName = false;
                this.SourceTypeName = "Rock.Web.UI.Controls.MonthYearPicker, Rock";
                this.PropertyName = "SelectedDate";
            }

            // #TODO# Decide how we want to design this control.  Two DropDownLists?
            //var script = string.Format( @"Rock.controls.datePicker.initialize({{ id: '{0}', format: 'M yyyy', startView: 'year', minViewMode: 'year' }});", this.ClientID );
            //ScriptManager.RegisterStartupScript( this, this.GetType(), "date_picker-" + this.ClientID, script, true );
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
                    string[] dateParts = this.Text.Split( new char[] { '/', '-', ' ' } );
                    if ( dateParts.Length == 2 )
                    {
                        string shortDate = string.Format( "{0}/01/{1}", dateParts[0], dateParts[1] );
                        DateTime result;
                        if ( DateTime.TryParse( shortDate, out result ) )
                        {
                            return result.Date;
                        }
                    }
                    
                    ShowErrorMessage( Rock.Constants.WarningMessage.DateTimeFormatInvalid( "Month Year" ) );                   
                }

                return null;
            }

            set
            {
                if ( value != null )
                {
                    this.Text = value.Value.ToString( "MM/yy" );
                }
                else
                {
                    this.Text = string.Empty;
                }
            }
        }
    }
}