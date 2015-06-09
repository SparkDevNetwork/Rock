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
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:NewFamilyContactInfo runat=server></{0}:NewFamilyContactInfo>" )]
    public class NewFamilyContactInfo : CompositeControl, INamingContainer
    {

        /// <summary>
        /// Gets the contact information rows.
        /// </summary>
        /// <value>
        /// The contact information rows.
        /// </value>
        public List<NewFamilyContactInfoRow> ContactInfoRows
        {
            get
            {
                var rows = new List<NewFamilyContactInfoRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is NewFamilyContactInfoRow )
                    {
                        var newFamilyMemberRow = control as NewFamilyContactInfoRow;
                        if ( newFamilyMemberRow != null )
                        {
                            rows.Add( newFamilyMemberRow );
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
                var homePhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                var cellPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "table table-familycontactinfo" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:20%" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:20%" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( homePhone != null ? homePhone.Value.EndsWith( "Phone" ) ? homePhone.Value : homePhone.Value + " Phone" : "Home Phone" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:20%" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( cellPhone != null ? cellPhone.Value.EndsWith( "Phone" ) ? cellPhone.Value : cellPhone.Value + " Phone" : "Cell Phone" );
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Style, "width:5%");
                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                writer.Write("SMS");
                writer.RenderEndTag();


                writer.AddAttribute( HtmlTextWriterAttribute.Style, "width:35%" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Email" );
                writer.RenderEndTag();

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // thead

                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( Control control in Controls )
                {
                    if ( control is NewFamilyContactInfoRow )
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
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                if (Controls[i] is NewFamilyContactInfoRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }

    }
}