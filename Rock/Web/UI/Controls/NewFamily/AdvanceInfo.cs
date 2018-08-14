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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Advance Info control
    /// </summary>
    [ToolboxData( "<{0}:NewGroupAdvanceInfo runat=server></{0}:NewGroupAdvanceInfo>" )]
    public class NewGroupAdvanceInfo : CompositeControl, INamingContainer
    {
        /// <summary>
        /// Gets the advance information rows.
        /// </summary>
        /// <value>
        /// The advance information rows.
        /// </value>
        public List<NewGroupAdvanceInfoRow> AdvanceInfoRows
        {
            get
            {
                var rows = new List<NewGroupAdvanceInfoRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is NewGroupAdvanceInfoRow )
                    {
                        var newGroupAdvanceInfoRow = control as NewGroupAdvanceInfoRow;
                        if ( newGroupAdvanceInfoRow != null )
                        {
                            rows.Add( newGroupAdvanceInfoRow );
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

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "table table-groupaadvanceinfo" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:20%" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Alternate Id" );
                writer.RenderEndTag();

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // thead

                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( Control control in Controls )
                {
                    if ( control is NewGroupAdvanceInfoRow )
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
                if ( Controls[i] is NewGroupAdvanceInfoRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }

    }
}