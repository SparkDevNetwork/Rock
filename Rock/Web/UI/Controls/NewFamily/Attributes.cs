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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:NewGroupAttributes runat=server></{0}:NewGroupAttributes>" )]
    public class NewGroupAttributes : CompositeControl, INamingContainer
    {
        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        public int CategoryId
        {
            get { return ViewState["CategoryId"] as int? ?? 0; }
            set { ViewState["CategoryId"] = value; }
        }

        /// <summary>
        /// Gets or sets the attribute ids.
        /// </summary>
        /// <value>
        /// The attribute ids.
        /// </value>
        public List<AttributeCache> AttributeList
        {
            get 
            {
                if ( _attributeList == null )
                {
                    _attributeList = ViewState["AttributeList"] as List<AttributeCache>;
                    if ( _attributeList == null )
                    {
                        _attributeList = new List<AttributeCache>();
                    }
                }
                return _attributeList;
            }
            set 
            {
                _attributeList = value;
                ViewState["AttributeList"] = _attributeList;
            }
        }
        private List<AttributeCache> _attributeList = null;

        /// <summary>
        /// Gets the attributes rows.
        /// </summary>
        /// <value>
        /// The attributes rows.
        /// </value>
        public List<NewGroupAttributesRow> AttributesRows
        {
            get
            {
                var rows = new List<NewGroupAttributesRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is NewGroupAttributesRow )
                    {
                        var newGroupAttributesRow = control as NewGroupAttributesRow;
                        if ( newGroupAttributesRow != null )
                        {
                            rows.Add( newGroupAttributesRow );
                        }
                    }
                }

                return rows;
            }
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "table" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                // name
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                foreach ( var attribute in AttributeList )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Th );
                    writer.Write( attribute.Name );
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // thead

                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( Control control in Controls )
                {
                    if ( control is NewGroupAttributesRow )
                    {
                        control.RenderControl( writer );
                    }
                }

                writer.RenderEndTag();  // tbody

                writer.RenderEndTag();  // table
            }
        }

        /// <summary>
        /// Clears the rows.
        /// </summary>
        public void ClearRows()
        {
            for ( int i = Controls.Count - 1; i >= 0; i-- )
            {
                if ( Controls[i] is NewGroupAttributesRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }
    }
}