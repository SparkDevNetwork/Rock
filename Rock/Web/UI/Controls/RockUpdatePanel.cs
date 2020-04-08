// <copyright>
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
using System;
using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.UpdatePanel"/> that implements IPostBackEventHandler.
    /// </summary>
    [ToolboxData( "<{0}:RockUpdatePanel runat=server></{0}:RockUpdatePanel>" )]
    public class RockUpdatePanel : UpdatePanel, IPostBackEventHandler
    {
        /// <summary>
        /// Gets the post back event reference.
        /// </summary>
        /// <param name="eventArgument">The event argument.</param>
        /// <returns></returns>
        public string GetPostBackEventReference( string eventArgument )
        {
            return Page.ClientScript.GetPostBackEventReference( this, eventArgument );
        }

        /// <summary>
        /// Gets the post back hyperlink.
        /// </summary>
        /// <param name="eventArgument">The event argument.</param>
        /// <returns></returns>
        public string GetPostBackHyperlink( string eventArgument )
        {
            return Page.ClientScript.GetPostBackClientHyperlink( this, eventArgument );
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( OnPostBack != null )
            {
                var e = new PostBackEventArgs( eventArgument );
                OnPostBack( this, e );
            }
        }

        /// <summary>
        /// Occurs on post back.
        /// </summary>
        public event EventHandler<PostBackEventArgs> OnPostBack;

    }

    /// <summary>
    /// 
    /// </summary>
    public class PostBackEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the event argument.
        /// </summary>
        /// <value>
        /// The event argument.
        /// </value>
        public string EventArgument { get; private set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="PostBackEventArgs"/> class.
        /// </summary>
        /// <param name="eventArgument">The event argument.</param>
        public PostBackEventArgs(string eventArgument)
        {
            EventArgument = eventArgument;
        }
    }
}