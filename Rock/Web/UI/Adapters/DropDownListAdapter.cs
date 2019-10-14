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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Custom control adapter for rendering a dropdown list
    /// </summary>
    public class DropDownListAdapter : WebControlAdapter
    {
        /// <summary>
        /// Generates the target-specific inner markup for the Web control to which the control adapter is attached.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderContents( HtmlTextWriter writer )
        {
            DropDownList list = Control as DropDownList;
            string currentOptionGroup;
            var renderedOptionGroups = new List<string>();

            // group the items by optiongroup first, but honor whatever the original sorting was
            foreach ( IGrouping<string, ListItem> itemGrouping in list.Items.OfType<ListItem>().GroupBy(g => g.Attributes["OptionGroup"]) )
            {
                currentOptionGroup = itemGrouping.Key;
                foreach ( var item in itemGrouping )
                {
                    // Determine if the item is part of an option group
                    if ( currentOptionGroup != null )
                    {
                        // the option header was already written, just render the list item
                        if ( renderedOptionGroups.Contains( currentOptionGroup ) )
                        {
                            RenderListItem( item, writer );
                        }
                        else
                        {
                            // the header was not written- do that first
                            if ( renderedOptionGroups.Count > 0 )
                            {
                                // need to close previous group
                                RenderOptionGroupEndTag( writer );
                            }

                            RenderOptionGroupBeginTag( currentOptionGroup, writer );
                            renderedOptionGroups.Add( currentOptionGroup );

                            RenderListItem( item, writer );
                        }
                    }
                    else
                    {
                        // default behavior: render the list item as normal
                        RenderListItem( item, writer );
                    }

                    if ( Page != null && Page.ClientScript != null )
                    {
                        Page.ClientScript.RegisterForEventValidation( list.UniqueID, item.Value );
                    }
                }
            }

            if ( renderedOptionGroups.Count > 0 )
            {
                RenderOptionGroupEndTag( writer );
            }

            if ( Page != null && Page.ClientScript != null )
            {
                Page.ClientScript.RegisterForEventValidation( list.UniqueID );
            }
        }

        /// <summary>
        /// Renders the option group begin tag.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="writer">The writer.</param>
        private void RenderOptionGroupBeginTag( string name, HtmlTextWriter writer )
        {
            writer.WriteBeginTag( "optgroup" );
            writer.WriteAttribute( "label", name );
            writer.Write( HtmlTextWriter.TagRightChar );
            writer.WriteLine();
        }

        /// <summary>
        /// Renders the option group end tag.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void RenderOptionGroupEndTag( HtmlTextWriter writer )
        {
            writer.WriteEndTag( "optgroup" );
            writer.WriteLine();
        }

        /// <summary>
        /// Renders the list item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="writer">The writer.</param>
        private void RenderListItem( ListItem item, HtmlTextWriter writer )
        {
            writer.WriteBeginTag( "option" );
            writer.WriteAttribute( "value", item.Value, true );
            if ( item.Selected )
            {
                writer.WriteAttribute( "selected", "selected", false );
            }

            if ( !item.Enabled )
            {
                writer.WriteAttribute( "disabled", string.Empty );
            }

            foreach ( string key in item.Attributes.Keys )
            {
                if ( !key.Equals("optiongroup", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    writer.WriteAttribute( key, item.Attributes[key] );
                }
            }

            writer.Write( HtmlTextWriter.TagRightChar );

            HttpUtility.HtmlEncode( item.Text, writer );
            writer.WriteEndTag( "option" );
            writer.WriteLine();
        }
    }
}