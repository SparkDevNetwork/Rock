//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:NewFamilyAttributes runat=server></{0}:NewFamilyAttributes>" )]
    public class NewFamilyAttributes : CompositeControl, INamingContainer
    {
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

        public List<NewFamilyPersonAttributesRow> FamilyMemberRows
        {
            get
            {
                var rows = new List<NewFamilyPersonAttributesRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is NewFamilyPersonAttributesRow )
                    {
                        var newFamilyAttributesRow = control as NewFamilyPersonAttributesRow;
                        if ( newFamilyAttributesRow != null )
                        {
                            rows.Add( newFamilyAttributesRow );
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
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "table" );
            writer.RenderBeginTag( HtmlTextWriterTag.Table );

            writer.RenderBeginTag( HtmlTextWriterTag.Thead );
            writer.RenderBeginTag( HtmlTextWriterTag.Tr );

            foreach ( var attribute in AttributeList)
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
                if ( control is NewFamilyPersonAttributesRow )
                {
                    control.RenderControl( writer );
                }
            }

            writer.RenderEndTag();  // tbody

            writer.RenderEndTag();  // table
        }
       
    }
}