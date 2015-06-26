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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a standard warning or error message box
    /// </summary>
    [DefaultProperty( "Text" )]
    [ToolboxData( "<{0}:NotificationBox runat=server></{0}:NotificationBox>" )]
    public class NotificationBox : Literal
    {
        /// <summary>
        /// Gets or sets the title (title is inline with the message text but is bold).
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the heading (heading is on its own line at the top)
        /// </summary>
        /// <value>
        /// The heading.
        /// </value>
        public string Heading { get; set; }

        /// <summary>
        /// Gets or sets the type of the notification box.
        /// </summary>
        /// <value>
        /// The type of the notification box.
        /// </value>
        public NotificationBoxType NotificationBoxType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [dismissable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dismissable]; otherwise, <c>false</c>.
        /// </value>
        public bool Dismissable
        {
            get
            {
                return ViewState["Dismissable"] as bool? ?? false;
            }

            set
            {
                ViewState["Dismissable"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public string Details
        {
            get
            {
                return ViewState["Details"] as string;
            }

            set
            {
                ViewState["Details"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS Class.
        /// </value>
        public string CssClass
        {
            get
            {
                return ViewState["CssClass"] as string;
            }

            set
            {
                ViewState["CssClass"] = value;
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                string alertType = NotificationBoxType.ToString().ToLower();

                bool showMessage = !string.IsNullOrWhiteSpace( Heading ) || !string.IsNullOrWhiteSpace( Title ) || !string.IsNullOrWhiteSpace( this.Text );

                if ( showMessage )
                {
                    writer.AddAttribute( "class", string.Format("alert alert-{0} {1}", alertType, CssClass) );
                    writer.AddAttribute( "id", this.ClientID );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    if ( this.Dismissable )
                    {
                        writer.Write( @"<button type='button' class='close' data-dismiss='alert' aria-hidden='true'><i class='fa fa-times'></i></button>" );
                    }

                    if ( !string.IsNullOrWhiteSpace( this.Heading ) )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Strong );
                        writer.Write( Heading );
                        writer.RenderEndTag();
                    }

                    if ( !string.IsNullOrWhiteSpace( this.Title ) )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Strong );
                        writer.Write( Title + " " );
                        writer.RenderEndTag();
                    }

                    base.RenderControl( writer );

                    if ( !string.IsNullOrWhiteSpace( this.Details ) )
                    {
                        string detailsFormat = @"
<small>
    <a data-toggle='collapse' data-parent='#{1}' href='#error-details_{1}'>Show Details</a>
</small>
<div id='error-details_{1}' class='collapse'>
        <p class='margin-t-sm'>
        {0}
    </p>
</div>";
                        writer.Write( detailsFormat, this.Details, this.ClientID );
                    }

                    writer.RenderEndTag();
                }
            }
        }
    }

    /// <summary>
    /// The type of notification box to display
    /// </summary>
    public enum NotificationBoxType
    {
        /// <summary>
        /// Display an information box
        /// </summary>
        Info,

        /// <summary>
        /// Display a warning box
        /// </summary>
        Warning,

        /// <summary>
        /// Display an error box
        /// </summary>
        Danger,

        /// <summary>
        /// Display a success box
        /// </summary>
        Success
    }
}