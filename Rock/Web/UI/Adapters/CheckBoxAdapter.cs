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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;

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

                bool hasText = !string.IsNullOrWhiteSpace( cb.Text );
                if ( hasText )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "checkbox" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.RenderBeginTag( HtmlTextWriterTag.Label );
                }

                writer.AddAttribute( "id", cb.ClientID );
                writer.AddAttribute( "type", "checkbox" );
                writer.AddAttribute( "name", cb.UniqueID );
                if ( cb.Checked )
                {
                    writer.AddAttribute( "checked", "checked" );
                }

                if ( !string.IsNullOrWhiteSpace( cb.CssClass ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, cb.CssClass );
                }

                foreach ( var inputAttributeKey in cb.InputAttributes.Keys )
                {
                    var key = inputAttributeKey as string;
                    writer.AddAttribute( key, cb.InputAttributes[key] );
                }

                if (cb.AutoPostBack)
                {
                    PostBackOptions postBackOption = new PostBackOptions(cb, string.Empty);
                    if (cb.CausesValidation && this.Page.GetValidators(cb.ValidationGroup).Count > 0)
                    {
                        postBackOption.PerformValidation = true;
                        postBackOption.ValidationGroup = cb.ValidationGroup;
                    }
                    if (this.Page.Form != null)
                    {
                        postBackOption.AutoPostBack = true;
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackEventReference(postBackOption, true));
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();

                if ( hasText )
                {
                    writer.Write( cb.Text );

                    writer.RenderEndTag();      // Label
                }

                var rockCb = cb as Rock.Web.UI.Controls.RockCheckBox;
                if (rockCb != null)
                {
                    bool renderLabel = ( !string.IsNullOrEmpty( rockCb.Label ) );
                    bool renderHelp = ( rockCb.HelpBlock != null && !string.IsNullOrWhiteSpace( rockCb.Help ) );

                    if (!renderLabel && renderHelp)
                    {
                        rockCb.HelpBlock.RenderControl( writer );
                    }
                }

                if ( hasText )
                {
                    writer.RenderEndTag();      // Div
                }

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( cb.UniqueID );
                }
            }
        }
    }
}