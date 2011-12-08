using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Controls
{
    /// <summary>
    /// Displays a standard warning or error message box
    /// </summary>
    [DefaultProperty( "Text" )]
    [ToolboxData( "<{0}:NotificationBox runat=server></{0}:NotificationBox>" )]
    public class NotificationBox : Literal
    {
        private string _Title;
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        private NotificationBoxType _NotificationBoxType;
        /// <summary>
        /// Gets or sets the type of the notification box.
        /// </summary>
        /// <value>
        /// The type of the notification box.
        /// </value>
        public NotificationBoxType NotificationBoxType
        {
            get { return _NotificationBoxType; }
            set { _NotificationBoxType = value; }
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            writer.Write( "<div class=\"notification-box " + _NotificationBoxType.ToString().ToLower() + "\">" + Environment.NewLine );
            writer.Write( "    <div class=\"text\">" + Environment.NewLine );

            if ( _Title != null && _Title != string.Empty )
                writer.Write( "         <strong>" + _Title + "</strong>" );

            writer.Write( this.Text );

            writer.Write( "     </div>" + Environment.NewLine );
            writer.Write( "</div>" + Environment.NewLine );
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
        Information,

        /// <summary>
        /// Display a warning box
        /// </summary>
        Warning,

        /// <summary>
        /// Display an error box
        /// </summary>
        Error,

        /// <summary>
        /// Display a tip box
        /// </summary>
        Tip,

        /// <summary>
        /// Display a success box
        /// </summary>
        Success
    };


}