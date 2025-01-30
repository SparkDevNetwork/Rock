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
    public class ObsidianDataViewComponentWrapper : CompositeControl, INamingContainer
    {
        public string ComponentUrl
        {
            get => ViewState[nameof( ComponentUrl )] as string;
            set => ViewState[nameof( ComponentUrl )] = value;
        }

        public Dictionary<string, string> ComponentData
        {
            get
            {
                EnsureChildControls();
                return ( ( HiddenField ) Controls[0] ).Value.FromJsonOrNull<Dictionary<string, string>>();
            }
            set
            {
                EnsureChildControls();
                ( ( HiddenField ) Controls[0] ).Value = value.ToJson();
            }
        }

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            var hfId = new HiddenField
            {
                ID = "hfData"
            };

            Controls.Add( hfId );
        }

        protected override void OnPreRender( EventArgs e )
        {
            var script = $@"
Obsidian.onReady(() => {{
    System.import(""@Obsidian/Templates/rockPage.js"").then(module => {{
        module.initializeDataViewComponentWrapper(""{ComponentUrl}"", ""{ClientID}"", ""{Controls[0].ClientID}"");
    }});
}});";
            ScriptManager.RegisterStartupScript( this, GetType(), "obsidian-data-component-wrapper-init", script, true );

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
