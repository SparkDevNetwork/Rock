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
using System.ComponentModel;
using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// from http://stackoverflow.com/a/8761161/1755417
    /// </summary>
    public class HiddenFieldWithClass : HiddenFieldWithValidationProperty
    {
        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        [CssClassProperty]
        [DefaultValue( "" )]
        public virtual string CssClass
        {
            get
            {
                string Value = this.ViewState["CssClass"] as string;
                if ( Value == null )
                    Value = "";
                return Value;
            }
            set
            {
                this.ViewState["CssClass"] = value;
            }
        }

        /// <summary>
        /// Renders the Web server control content to the client's browser using the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object used to render the server control content on the client's browser.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( this.CssClass != "" )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, this.CssClass );
            }
            base.Render( writer );
        }
    }
}
