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

namespace Rock.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class DialogPage : RockPage
    {
        /// <summary>
        /// An optional subtitle
        /// </summary>
        /// <value>
        /// The sub title.
        /// </value>
        public virtual string SubTitle { get; set; }

        /// <summary>
        /// Gets or sets the close message.
        /// </summary>
        /// <value>
        /// The close message.
        /// </value>
        public virtual string CloseMessage { get; set; }

        /// <summary>
        /// Fires the save.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void FireSave( object sender, EventArgs e )
        {
            if ( OnSave != null )
            {
                OnSave( sender, e );
            }
        }

        /// <summary>
        /// Occurs when [on save].
        /// </summary>
        public event EventHandler<EventArgs> OnSave;
    }
}