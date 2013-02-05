//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
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
        /// Gets or sets the heading (heading is on it's own line at the top)
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
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                string alertType = NotificationBoxType.ToString().ToLower();
                if ( alertType == "warning" )
                {
                    alertType = "block";
                }

                bool showMessage = !string.IsNullOrWhiteSpace( Heading ) || !string.IsNullOrWhiteSpace( Title ) || !string.IsNullOrWhiteSpace( this.Text );

                if ( showMessage )
                {
                    writer.AddAttribute( "class", "alert alert-" + alertType );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    if ( !string.IsNullOrWhiteSpace( Heading ) )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.H4 );
                        writer.Write( Heading );
                        writer.RenderEndTag();
                    }

                    if ( !string.IsNullOrWhiteSpace( Title ) )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Strong );
                        writer.Write( Title + " ");
                        writer.RenderEndTag();
                    }

                    base.RenderControl( writer );

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
        Error,

        /// <summary>
        /// Display a success box
        /// </summary>
        Success
    };
}