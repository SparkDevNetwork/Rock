﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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
            string script = string.Format( "bootbox.alert('<h4>{0}</h4>{1}');", alertType.ConvertToString(), message.EscapeQuotes() );
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