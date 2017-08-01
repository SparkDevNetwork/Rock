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
using System.Web.UI.WebControls.Adapters;

using Rock;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Control adapter for checkbox list
    /// </summary>
    public class CheckBoxListAdapter : WebControlAdapter
    {
        /// <summary>
        /// Creates the beginning tag for the Web control in the markup that is transmitted to the target browser.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderBeginTag( System.Web.UI.HtmlTextWriter writer )
        {
            // Preserve any classes that a developer put on the control (such as a "well") by wrapping it in a <div>.
            CheckBoxList cbl = Control as CheckBoxList;
            if ( cbl != null && ! string.IsNullOrEmpty( cbl.CssClass ) )
            {
                writer.AddAttribute( "class", cbl.CssClass );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }
        }

        /// <summary>
        /// Creates the ending tag for the Web control in the markup that is transmitted to the target browser.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderEndTag( System.Web.UI.HtmlTextWriter writer )
        {
            // Close the <div> tag we may have started in the BeginTag above.
            CheckBoxList cbl = Control as CheckBoxList;
            if ( cbl != null && !string.IsNullOrEmpty( cbl.CssClass ) )
            {
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Generates the target-specific inner markup for the Web control to which the control adapter is attached.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderContents( System.Web.UI.HtmlTextWriter writer )
        {
            CheckBoxList cbl = Control as CheckBoxList;
            if ( cbl != null )
            {
                PostBackOptions postBackOption = null;

                if ( cbl.AutoPostBack )
                {
                    postBackOption = new PostBackOptions(cbl, string.Empty);
                    if (cbl.CausesValidation && this.Page.GetValidators(cbl.ValidationGroup).Count > 0)
                    {
                        postBackOption.PerformValidation = true;
                        postBackOption.ValidationGroup = cbl.ValidationGroup;
                    }
                    if (this.Page.Form != null)
                    {
                        postBackOption.AutoPostBack = true;
                    }
                }

                int i = 0;
                foreach (ListItem li in cbl.Items)
                {
                    writer.WriteLine();

                    if ( cbl.RepeatDirection == RepeatDirection.Vertical )
                    {
                        string cssClass = "checkbox";
                        if ( cbl.RepeatColumns > 1 )
                        {
                            switch( cbl.RepeatColumns )
                            {
                                case 2: cssClass += " col-md-6"; break;
                                case 3: cssClass += " col-md-4"; break;
                                case 4: cssClass += " col-md-3"; break;
                                case 6: cssClass += " col-md-2"; break;
                            }
                        }
                        writer.AddAttribute( "class", cssClass );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );

                        // use the muted text class on the label if the CBL is disabled
                        if ( !cbl.Enabled )
                        {
                            writer.AddAttribute( "class", "text-muted" );
                        }
                    }
                    else
                    {
                        // use the muted text class on the label if the CBL is disabled along with the checkbox-inline class
                        writer.AddAttribute( "class", ( !cbl.Enabled ) ? "text-muted checkbox-inline" : "checkbox-inline" );
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Label);

                    string itemId = string.Format("{0}_{1}", cbl.ClientID, i);
                    writer.AddAttribute("id", itemId);
                    writer.AddAttribute("type", "checkbox");
                    var checkboxInputName = string.Format( "{0}${1}", cbl.UniqueID, i++ );
                    writer.AddAttribute("name", checkboxInputName);
                    writer.AddAttribute("value", li.Value);
                    if (li.Selected)
                    {
                        writer.AddAttribute("checked", "checked");
                    }

                    if ( !cbl.Enabled )
                    {
                        writer.AddAttribute( "disabled", "" );
                    }

                    foreach (var attributeKey in li.Attributes.Keys)
                    {
                        var key = attributeKey as string;
                        writer.AddAttribute(key, li.Attributes[key]);
                    }

                    if (postBackOption != null)
                    {
                        var postBackReference = Page.ClientScript.GetPostBackEventReference( postBackOption, true );
                        postBackReference = postBackReference.Replace( cbl.UniqueID, checkboxInputName );
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick, postBackReference );
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Input);
                    writer.RenderEndTag();

                    writer.Write(li.Text);

                    writer.RenderEndTag();      // Label

                    if ( cbl.RepeatDirection == RepeatDirection.Vertical )
                    {
                        writer.RenderEndTag();   // div
                    }

                    if ( Page != null && Page.ClientScript != null )
                    {
                        Page.ClientScript.RegisterForEventValidation(cbl.UniqueID, li.Value);
                    }
                }
                
                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( cbl.UniqueID );
                }
            }
        }
    }
}