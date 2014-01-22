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
            this.CssClass = "input-width-md";
            this.AppendText = "<span class='add-on'><i class='fa fa-clock-o'></i></span>";

            if ( string.IsNullOrWhiteSpace( this.SourceTypeName ) )
            {
                this.LabelTextFromPropertyName = false;
                this.SourceTypeName = "Rock.Web.UI.Controls.TimePicker, Rock";
                this.PropertyName = "SelectedTime";
            }

            var script = string.Format( @"Rock.controls.timePicker.initialize({{ id: '{0}' }});", this.ClientID );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "time_picker-" + this.ClientID, script, true );
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