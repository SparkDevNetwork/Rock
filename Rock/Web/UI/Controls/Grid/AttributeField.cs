//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Rock.Attribute;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AttributeField : BoundField
    {
        /// <summary>
        /// Retrieves the value of the field bound to the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="controlContainer">The container for the field value.</param>
        /// <returns>
        /// The value of the field bound to the <see cref="T:System.Web.UI.WebControls.BoundField" />.
        /// </returns>
        protected override object GetValue( System.Web.UI.Control controlContainer )
        {
            var row = controlContainer as GridViewRow;
            if ( row != null )
            {
                var dataItem = ( row.DataItem as IHasAttributes );
                if ( dataItem != null )
                {

                    if ( dataItem.Attributes == null )
                    {
                        dataItem.LoadAttributes();
                    }

                    bool exists = dataItem.Attributes.ContainsKey( this.DataField );
                    if ( exists )
                    {
                        var attrib = dataItem.Attributes[this.DataField];
                        string rawValue = dataItem.GetAttributeValue( this.DataField );
                        string resultHtml = attrib.FieldType.Field.FormatValue( controlContainer, rawValue, attrib.QualifierValues, true );
                        return new HtmlString( resultHtml );
                    }
                }
            }

            return string.Empty;
        }

        protected override string FormatDataValue( object dataValue, bool encode )
        {
            return base.FormatDataValue( dataValue, false );
        }
    }
}