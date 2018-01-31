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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class LavaCommandsPicker : RockCheckBoxList
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An object that contains event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.RepeatDirection = RepeatDirection.Horizontal;

            this.Items.Add( "All" );

            foreach ( var command in Rock.Lava.LavaHelper.GetLavaCommands() )
            {
                this.Items.Add( new ListItem( command, command ) );
            }
        }

        /// <summary>
        /// Gets or sets the selected lava commands.
        /// </summary>
        /// <value>
        /// The selected lava commands.
        /// </value>
        public List<string> SelectedLavaCommands
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).ToList();
            }
            set
            {
                foreach ( ListItem item in this.Items )
                {
                    item.Selected = value.Exists( a => a.Equals( item.Value ) );
                }
            }
        }
    }
}