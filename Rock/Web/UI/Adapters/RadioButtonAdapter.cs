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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;

using Rock.Web.UI.Controls;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Control adapter for radio button
    /// </summary>
    public class RadioButtonAdapter : WebControlAdapter
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
            RadioButton rb = Control as RadioButton;
            if ( rb != null )
            {
                writer.WriteLine();

                // always render the label tag for the radiobutton, even if the radiobutton doesn't have text
                bool renderRadioButtonLabel = true;
                if ( renderRadioButtonLabel )
                {
                    if ( rb is RockRadioButton )
                    {
                        if ( ( rb as RockRadioButton ).DisplayInline )
                        {
                            writer.AddAttribute( HtmlTextWriterAttribute.Class, "radio-inline" );
                        }
                    }

                    writer.RenderBeginTag( HtmlTextWriterTag.Label );
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Id, rb.ClientID );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "radio" );
                
                string uniqueGroupName = rb.GetType().GetProperty( "UniqueGroupName", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( rb, null ) as string;
                writer.AddAttribute( HtmlTextWriterAttribute.Name, uniqueGroupName );

                writer.AddAttribute( HtmlTextWriterAttribute.Value, rb.ID );

                if ( !string.IsNullOrWhiteSpace( rb.CssClass ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, rb.CssClass );
                }
                
                if ( rb.Checked )
                {
                    writer.AddAttribute( "checked", "checked" );
                }

                foreach ( var attributeKey in rb.Attributes.Keys )
                {
                    var key = attributeKey as string;
                    writer.AddAttribute( key, rb.Attributes[key] );
                }

                foreach ( var inputAttributeKey in rb.InputAttributes.Keys )
                {
                    var key = inputAttributeKey as string;
                    writer.AddAttribute( key, rb.InputAttributes[key] );
                }

                if ( rb.AutoPostBack )
                {
                    PostBackOptions postBackOption = new PostBackOptions( rb, string.Empty );
                    if ( rb.CausesValidation && this.Page.GetValidators( rb.ValidationGroup ).Count > 0 )
                    {
                        postBackOption.PerformValidation = true;
                        postBackOption.ValidationGroup = rb.ValidationGroup;
                    }
                    if ( this.Page.Form != null )
                    {
                        postBackOption.AutoPostBack = true;
                    }
                    writer.AddAttribute( HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackEventReference( postBackOption, true ) );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();

                if ( renderRadioButtonLabel )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "label-text" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );
                    
                    if ( rb.Text.Length > 0 )
                    {
                        
                        writer.Write(  rb.Text );
                    }
                    else
                    {
                        writer.Write( "&nbsp;" );
                    }
                    writer.RenderEndTag();      // Span
                    writer.RenderEndTag();      // Label
                }

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( uniqueGroupName, rb.ID );
                }
            }
        }
    }
}