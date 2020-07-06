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
    /// Control that can be used to select multiple lava commands
    /// <para>Commands are displayed as a series of checkboxes</para>
    /// </summary>
    public class LavaCommandsPicker : RockCheckBoxList
    {
        /*
            7/6/2020 - JH
            With the addition of the "InteractionContentChannelItemWrite" Lava Command within Rock Core, the RockCheckBoxList default
            value of 4 is no longer ideal for LavaCommandsPicker.RepeatColumns. I have changed this value to 3 for this implementation
            in order to help prevent wrapping of longer command names. Note that I have exposed this default value so other classes
            (such as the LavaCommandsFieldType) can make use of this value as well.
        */
        /// <summary>
        /// The default number of columns to display in the <see cref="RockCheckBoxList" /> control.
        /// </summary>
        public static readonly int DefaultRepeatColumns = 3;

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

        /// <summary>
        /// Gets or sets the number of columns to display in the <see cref="RockCheckBoxList" /> control.
        /// If RepeatDirection is Horizontal, this will default to 3 columns. There is no upper limit in the code so use wisely.
        /// </summary>
        public override int RepeatColumns
        {
            get
            {
                if ( this.RepeatDirection == RepeatDirection.Horizontal )
                {
                    // unless set specifically, default Horizontal to 3
                    return _repeatColumns ?? DefaultRepeatColumns;
                }
                else
                {
                    return _repeatColumns ?? 0;
                }
            }

            set
            {
                _repeatColumns = value;
            }
        }

        private int? _repeatColumns;
    }
}