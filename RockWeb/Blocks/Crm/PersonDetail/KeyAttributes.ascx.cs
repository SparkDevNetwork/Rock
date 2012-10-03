//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock;
using Rock.Crm;

namespace RockWeb.Blocks.Crm.PersonDetail
{
	/// <summary>
	/// User control for viewing key attributes
	/// </summary>
	[Rock.Attribute.Property( 1, "Xslt File", "Behavior", "XSLT File to use.", false, "AttributeValues.xslt" )]
	public partial class KeyAttributes : Rock.Web.UI.PersonBlock
    {
		private XDocument xDocument = null;

		/// <summary>
		/// Raises the <see cref="E:Init" /> event.
		/// </summary>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		protected override void OnInit( EventArgs e )
		{
			base.OnInit( e );

			var rootElement = new XElement( "root" );
			var attributesElement = new XElement( "attributes",
				new XAttribute( "category-name", "Personal Key Attributes" )
				);
			rootElement.Add( attributesElement );

			foreach ( string keyAttributeId in GetUserValue( "Rock.KeyAttributes" ).SplitDelimitedValues() )
			{
				int attributeId = 0;
				if (Int32.TryParse(keyAttributeId, out attributeId))
				{
					var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
					if ( attribute != null )
					{
						var values = Person.AttributeValues[attribute.Key].Value;
						if ( values != null && values.Count > 0 )
						{
							attributesElement.Add( new XElement( "attribute",
								new XAttribute( "name", attribute.Name ),
								new XCData( attribute.FieldType.Field.FormatValue( null, values[0].Value, attribute.QualifierValues, false ) ?? string.Empty )
							) );
						}
					}
				}
			}

			xDocument = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
		}

		/// <summary>
		/// When a control renders it's content to the page, this method will also check to see if
		/// the block instance of this control has been configured for output caching, and if so,
		/// the contents will also be rendered to a string variable that will gets cached in the
		/// default MemoryCache for use next time by the Rock.Web.UI.Page.OnInit() method when rendering the
		/// control content.
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render( System.Web.UI.HtmlTextWriter writer )
		{
			try
			{
				if ( xDocument != null && !String.IsNullOrEmpty( AttributeValue( "XsltFile" ) ) )
				{
					string xsltFile = AttributeValue( "XsltFile" );
					if ( !String.IsNullOrEmpty( xsltFile ) )
					{
						string xsltPath = Server.MapPath( "~/Themes/" + CurrentPage.Site.Theme + "/Assets/Xslt/" + AttributeValue( "XsltFile" ) );
						var xslt = new XslCompiledTransform();
						xslt.Load( xsltPath );
						xslt.Transform( xDocument.CreateReader(), null, writer );
					}
				}
			}
			catch ( Exception ex )
			{
				writer.Write( "Error: " + ex.Message );
			}
		}

	}
}