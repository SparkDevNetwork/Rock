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

namespace Rock.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class DialogMasterPage : RockMasterPage
    {
        /// <summary>
        /// An optional subtitle
        /// </summary>        
        public string SubTitle { get; set; }
        
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