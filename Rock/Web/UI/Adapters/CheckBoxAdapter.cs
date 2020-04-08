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

using Rock.Web.UI.Controls;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Control adapter for checkbox
    /// </summary>
    public class CheckBoxAdapter : WebControlAdapter
    {
        /// <summary>
        /// Creates the beginning tag for the Web control in the markup that is transmitted to the target browser.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderBeginTag( System.Web.UI.HtmlTextWriter writer )
        {
        }

        /// <summary>
        /// Creates the ending tag for the Web control in the markup that is transmitted to the target browser.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderEndTag( System.Web.UI.HtmlTextWriter writer )
        {
        }

        /// <summary>
        /// Generates the target-specific inner markup for the Web control to which the control adapter is attached.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderContents( System.Web.UI.HtmlTextWriter writer )
        {
            CheckBox cb = Control as CheckBox;
            if ( cb != null )
            {
                writer.WriteLine();

                // always render the label tag for the checkbox, even if the checkbox doesn't have text

                bool renderAsCheckbox = true;

                var cbSwitch = cb as Rock.Web.UI.Controls.Switch;
                if ( cbSwitch != null )
                {
                    renderAsCheckbox = false;
                }

                var textCssClass = "";
                var controlCssClass = "";
                if ( renderAsCheckbox )
                {
                    textCssClass = "label-text";
                    var containerCssClass = "checkbox";


                    if ( cb is RockCheckBox )
                    {
                        if ( ( cb as RockCheckBox ).DisplayInline )
                        {
                            containerCssClass = "checkbox-inline";
                        }
                        containerCssClass += " " + ( cb as RockCheckBox ).ContainerCssClass;
                        textCssClass += " " + ( cb as RockCheckBox ).TextCssClass;
                    }

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, containerCssClass );
                    writer.AddAttribute( HtmlTextWriterAttribute.Style, cb.Style.Value );

                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( "title", cb.ToolTip );
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );

                    controlCssClass = cb.CssClass;
                }
                else
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "custom-control custom-switch" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    controlCssClass = "custom-control-input " + cb.CssClass;
                }

                writer.AddAttribute( "id", cb.ClientID );
                writer.AddAttribute( "type", "checkbox" );
                writer.AddAttribute( "name", cb.UniqueID );
                if ( cb.Checked )
                {
                    writer.AddAttribute( "checked", "checked" );
                }

                if ( !string.IsNullOrWhiteSpace( controlCssClass ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, controlCssClass );
                }

                foreach ( var inputAttributeKey in cb.InputAttributes.Keys )
                {
                    var key = inputAttributeKey as string;
                    writer.AddAttribute( key, cb.InputAttributes[key] );
                }

                if ( cb.AutoPostBack )
                {
                    PostBackOptions postBackOption = new PostBackOptions( cb, string.Empty );
                    if ( cb.CausesValidation && this.Page.GetValidators( cb.ValidationGroup ).Count > 0 )
                    {
                        postBackOption.PerformValidation = true;
                        postBackOption.ValidationGroup = cb.ValidationGroup;
                    }

                    if ( this.Page.Form != null )
                    {
                        postBackOption.AutoPostBack = true;
                    }

                    writer.AddAttribute( HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackEventReference( postBackOption, true ) );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();

                

                if ( renderAsCheckbox )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, textCssClass );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );

                    if ( cb.Text.Length > 0 )
                    {
                        writer.Write( cb.Text );
                    }
                    else
                    {
                        writer.Write( "&nbsp;" );
                    }

                    writer.RenderEndTag();      // Span
                    writer.RenderEndTag();      // Label
                }
                else
                {
                    // If the switch does not have a lable make the text bold
                    if ( cbSwitch.BoldText )
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "custom-control-label custom-control-label-bold" );
                    }
                    else
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "custom-control-label" );
                    }
                    writer.AddAttribute( HtmlTextWriterAttribute.For, cb.ClientID );
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );

                    if ( cb.Text.Length > 0 )
                    {
                        writer.Write( cb.Text );
                    }
                    else
                    {
                        writer.Write( "&nbsp;" );
                    }
                    writer.RenderEndTag();
                }

                var rockCb = cb as Rock.Web.UI.Controls.RockCheckBox;
                if ( rockCb != null )
                {
                    bool renderRockLabel = !string.IsNullOrEmpty( rockCb.Label );
                    bool renderRockHelp = rockCb.HelpBlock != null && !string.IsNullOrWhiteSpace( rockCb.Help );
                    bool renderRockWarning = rockCb.WarningBlock != null && !string.IsNullOrWhiteSpace( rockCb.Warning );

                    if ( !renderRockLabel && renderRockHelp )
                    {
                        rockCb.HelpBlock.RenderControl( writer );
                    }

                    if ( !renderRockLabel && renderRockWarning )
                    {
                        rockCb.WarningBlock.RenderControl( writer );
                    }
                }

                writer.RenderEndTag();      // Div

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( cb.UniqueID );
                }
            }
        }
    }
}