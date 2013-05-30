//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// User control for viewing key attributes
    /// </summary>
    public partial class KeyAttributes : Rock.Web.UI.PersonBlock
    {
        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var attributes = new List<NameValue>();

            foreach ( string keyAttributeId in GetUserPreference( "Rock.KeyAttributes" ).SplitDelimitedValues() )
            {
                int attributeId = 0;
                if ( Int32.TryParse( keyAttributeId, out attributeId ) )
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                    if ( attribute != null && attribute.IsAuthorized( "View", CurrentPerson ) )
                    {
                        if ( Person != null && Person.AttributeValues != null )
                        {
                            var values = Person.AttributeValues[attribute.Key];
                            if ( values != null && values.Count > 0 )
                            {
                                attributes.Add( new NameValue( attribute.Name,
                                    attribute.FieldType.Field.FormatValue( null, values[0].Value, attribute.QualifierValues, false ) ?? string.Empty ) );
                            }
                        }
                    }
                }
            }

            rAttributes.DataSource = attributes;
            rAttributes.DataBind();
        }

        class NameValue
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public NameValue( string name, string value )
            {
                Name = name;
                Value = value;
            }
        }
    }
}