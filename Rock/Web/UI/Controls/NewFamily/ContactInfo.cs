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
    [ToolboxData( "<{0}:NewGroupContactInfo runat=server></{0}:NewGroupContactInfo>" )]
    public class NewGroupContactInfo : CompositeControl, INamingContainer
    {
        /// <summary>
        /// Gets or sets a value indicating whether [show cell phone first].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show cell phone first]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCellPhoneFirst
        {
            get
            {
                return ViewState["ShowCellPhoneFirst"] as bool? ?? false;
            }
            set
            {
                ViewState["ShowCellPhoneFirst"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is messaging visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is messaging visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsMessagingVisible
        {
            get { return ViewState["IsMessagingVisible"] as bool? ?? true; }
            set { ViewState["IsMessagingVisible"] = value; }
        }

        /// <summary>
        /// Gets the contact information rows.
        /// </summary>
        /// <value>
        /// The contact information rows.
        /// </value>
        public List<NewGroupContactInfoRow> ContactInfoRows
        {
            get
            {
                var rows = new List<NewGroupContactInfoRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is NewGroupContactInfoRow )
                    {
                        var newGroupMemberRow = control as NewGroupContactInfoRow;
                        if ( newGroupMemberRow != null )
                        {
                            rows.Add( newGroupMemberRow );
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

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "table table-groupcontactinfo" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:20%" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                if ( ShowCellPhoneFirst )
                {
                    RenderCellPhone( writer );
                    RenderHomePhone( writer );
                }
                else
                {
                    RenderHomePhone( writer );
                    RenderCellPhone( writer );
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:35%" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Email" );
                writer.RenderEndTag();

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // thead

                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( Control control in Controls )
                {
                    if ( control is NewGroupContactInfoRow )
                    {
                        control.RenderControl( writer );
                    }
                }

                writer.RenderEndTag();  // tbody

                writer.RenderEndTag();  // table
            }
        }

        private void RenderHomePhone( HtmlTextWriter writer )
        {
            var homePhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );

            writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:20%" );
            writer.RenderBeginTag( HtmlTextWriterTag.Th );
            writer.Write( homePhone != null ? homePhone.Value.EndsWith( "Phone" ) ? homePhone.Value : homePhone.Value + " Phone" : "Home Phone" );
            writer.RenderEndTag();
        }

        private void RenderCellPhone( HtmlTextWriter writer )
        {
            var cellPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

            writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:20%" );
            writer.RenderBeginTag( HtmlTextWriterTag.Th );
            writer.Write( cellPhone != null ? cellPhone.Value.EndsWith( "Phone" ) ? cellPhone.Value : cellPhone.Value + " Phone" : "Cell Phone" );
            writer.RenderEndTag();

            if ( IsMessagingVisible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:5%" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "SMS" );
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Clears the rows.
        /// </summary>
        public void ClearRows()
        {
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                if (Controls[i] is NewGroupContactInfoRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }

    }
}