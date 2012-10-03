//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Crm;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    public partial class KeyAttributes : Rock.Web.UI.PersonBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            foreach ( string keyAttributeId in GetUserValue( "Rock.KeyAttributes" ).SplitDelimitedValues() )
            {
                int attributeId = 0;
                if (Int32.TryParse(keyAttributeId, out attributeId))
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                    if ( attribute != null )
                    {
                        var li = new HtmlGenericControl( "li" );
                        ulAttributes.Controls.Add( li );
                        li.Controls.Add( new LiteralControl( attribute.Name ) );

                        li.Controls.Add( new LiteralControl( ": " ) );

                        var span = new HtmlGenericControl( "span" );
                        li.Controls.Add( span );
                        span.AddCssClass( "value" );

                        var values = Person.AttributeValues[attribute.Key].Value;
                        if ( values != null && values.Count > 0 )
                            span.Controls.Add( new LiteralControl( attribute.FieldType.Field.FormatValue( span, values[0].Value, attribute.QualifierValues, false ) ) );
                    }
                }
            }

        }
    }
}