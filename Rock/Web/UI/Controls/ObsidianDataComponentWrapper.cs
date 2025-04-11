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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Wraps an Obsidian component for use inside a WebForms block. This provides
    /// a basic dictionary of strings for the data.
    /// </summary>
    public class ObsidianDataComponentWrapper : CompositeControl, INamingContainer, IHasValidationGroup
    {
        /// <summary>
        /// The custom validator is used to trigger validation of Obsidian
        /// controls embedded inside a WebForms control. There is logic in the
        /// initializeDataComponentWrapper() function that will wire up a
        /// proxy between the Obsidian components and the WebForms validation.
        /// </summary>
        private CustomValidator _validator;

        /// <summary>
        /// The URL to load the Obsidian component from.
        /// </summary>
        public string ComponentUrl { get; set; }

        /// <summary>
        /// The data that will be passed to the component, or during postback
        /// that was provided by the component back to the server.
        /// </summary>
        public Dictionary<string, string> ComponentData
        {
            get
            {
                EnsureChildControls();

                return ( ( HiddenField ) Controls[0] ).Value.UnescapeDataString().FromJsonOrNull<Dictionary<string, string>>()
                    ?? new Dictionary<string, string>();
            }
            set
            {
                EnsureChildControls();

                if ( value != null )
                {
                    ( ( HiddenField ) Controls[0] ).Value = value.ToJson().EscapeDataString();
                }
                else
                {
                    ( ( HiddenField ) Controls[0] ).Value = "{}".EscapeDataString();
                }
            }
        }

        /// <summary>
        /// Additional properties to pass to the component.
        /// </summary>
        /// <remarks>
        /// The keys are the property names and the values must be one of the
        /// following value types: <see cref="bool"/>, <see cref="long"/>,
        /// <see cref="string"/>. Other value types may work, but the CLR type
        /// may change during PostBack operations.
        /// </remarks>
        public Dictionary<string, object> ComponentProperties
        {
            get
            {
                EnsureChildControls();

                return ( ( HiddenField ) Controls[1] ).Value.UnescapeDataString().FromJsonOrNull<Dictionary<string, object>>()
                    ?? new Dictionary<string, object>();
            }
            set
            {
                EnsureChildControls();

                if ( value != null )
                {
                    ( ( HiddenField ) Controls[1] ).Value = value.ToJson().EscapeDataString();
                }
                else
                {
                    ( ( HiddenField ) Controls[1] ).Value = "{}".EscapeDataString();
                }
            }
        }

        /// <inheritdoc/>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return _validator.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _validator.ValidationGroup = value;
            }
        }

        /// <inheritdoc/>
        protected override object SaveViewState()
        {
            ViewState[nameof( ComponentUrl )] = ComponentUrl;

            return base.SaveViewState();
        }

        /// <inheritdoc/>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ComponentUrl = ( string ) ViewState[nameof( ComponentUrl )];
        }

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            Controls.Add( new HiddenField
            {
                ID = "hfData",
                Value = "{}",
                ValidateRequestMode = ValidateRequestMode.Disabled
            } );

            Controls.Add( new HiddenField
            {
                ID = "hfConfigurationProperties",
                Value = "{}"
            } );

            _validator = new CustomValidator
            {
                ID = "cvData",
                ErrorMessage = "One or more fields are invalid.",
                Display = ValidatorDisplay.None
            };
            Controls.Add( _validator );
        }

        /// <inheritdoc/>
        protected override void OnPreRender( EventArgs e )
        {
            // Use a unique validation function so we don't conflict with
            // other embedded Obsidian components.
            _validator.ClientValidationFunction = $"validator_{ClientID}";

            var script = $@"
Obsidian.onReady(() => {{
    System.import(""@Obsidian/Templates/rockPage.js"").then(module => {{
        module.initializeDataComponentWrapper(""{ComponentUrl}"", ""{ClientID}"", ""{Controls[0].ClientID}"", ""{Controls[1].ClientID}"");
    }});
}});";
            ScriptManager.RegisterStartupScript( this, GetType(), $"init-{ClientID}", script, true );

            base.OnPreRender( e );
        }

        /// <inheritdoc/>
        public override void RenderControl( HtmlTextWriter writer )
        {
            RenderChildren( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Id, ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();
        }
    }
}
