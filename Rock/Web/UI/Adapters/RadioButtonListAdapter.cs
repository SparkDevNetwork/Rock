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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.UI.Controls;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Control adapter for checkbox list
    /// </summary>
    public class RadioButtonListAdapter : ListControlAdaptor
    {
        /// <summary>
        /// Gets the type of the input tag.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        public override string GetInputTagType( ListControl listControl )
        {
            return "radio";
        }

        /// <summary>
        /// Gets the repeat columns.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        public override int GetRepeatColumns( ListControl listControl )
        {
            return ( listControl as RadioButtonList )?.RepeatColumns ?? 0;
        }

        /// <summary>
        /// Gets the repeat direction.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        protected override RepeatDirection GetRepeatDirection( ListControl listControl )
        {
            return ( listControl as RadioButtonList )?.RepeatDirection ?? RepeatDirection.Vertical;
        }

        /// <summary>
        /// Gets the name of the input.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="itemIndex">Index of the item.</param>
        /// <returns></returns>
        public override string GetInputName( ListControl listControl, int itemIndex )
        {
            // only one input can be selected at a time, so don't append itemIndex
            return $"{listControl.UniqueID}";
        }

        /// <summary>
        /// Gets the label class.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="listItem">The list item.</param>
        /// <returns></returns>
        public override string GetLabelClass( ListControl listControl, ListItem listItem )
        {
            var buttonGroup = ( listControl as ButtonGroup );
            if ( buttonGroup != null )
            {
                string labelClass = string.Empty;
                if ( listItem.Selected )
                {
                    labelClass += buttonGroup.SelectedItemClass + " " + buttonGroup.ItemHookClass;
                }
                else
                {
                    labelClass += buttonGroup.UnselectedItemClass + " " + buttonGroup.ItemHookClass;
                }

                if ( !listControl.Enabled )
                {
                    labelClass += " aspNetDisabled";
                }

                return labelClass;
            }
            else
            {
                return base.GetLabelClass( listControl, listItem );
            }

        }

        /// <summary>
        /// Generates the target-specific inner markup for the Web control to which the control adapter is attached.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderContents( HtmlTextWriter writer )
        {
            // make sure any special base attributes from the RadioButton get written
            WebControl control = new WebControl( HtmlTextWriterTag.Span );
            control.ID = this.Control.ClientID;
            control.CopyBaseAttributes( this.Control );
            control.RenderBeginTag( writer );

            // call ListControlAdaptor RenderContents
            base.RenderContents( writer );

            control.RenderEndTag( writer );
        }
    }
}