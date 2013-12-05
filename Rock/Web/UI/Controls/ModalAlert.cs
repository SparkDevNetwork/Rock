//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ModalAlert : Literal
    {
        /// <summary>
        /// Gets the script key.
        /// </summary>
        /// <value>
        /// The script key.
        /// </value>
        private string ScriptKey
        {
            get
            {
                return string.Format( "bootbox_info_{0}", this.ClientID );
            }
        }

        /// <summary>
        /// Shows the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="alertType">Type of the message.</param>
        public void Show( string message, ModalAlertType alertType )
        {
            //string script = "bootbox.alert('" + message + "');";
            string script = string.Format( "bootbox.alert('<h4>{0}</h4>{1}');", alertType.ConvertToString(), message );
            ScriptManager.RegisterStartupScript( this, this.GetType(), ScriptKey, script, true );
        }

        /// <summary>
        /// Hides this instance.
        /// </summary>
        public void Hide()
        {
            Visible = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ModalAlertType
    {
        /// <summary>
        /// 
        /// </summary>
        Alert,
        
        /// <summary>
        /// 
        /// </summary>
        Information,
        
        /// <summary>
        /// 
        /// </summary>
        Warning,
    }
}