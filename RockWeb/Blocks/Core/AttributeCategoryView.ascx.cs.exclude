//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Xml.Linq;
using System.Xml.Xsl;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for editing the value(s) of a set of attributes for a given entity and category
    /// </summary>
    [ContextAware]
    [TextField( "Entity Qualifier Column", "The entity column to evaluate when determining if this attribute applies to the entity", false, "", "Filter", 0 )]
    [TextField( "Entity Qualifier Value", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "", "Filter", 1 )]
    [TextField( "Attribute Categories", "Delimited List of Attribute Category Names", true, "", "Filter", 2 )]
    [TextField( "Xslt File", "XSLT File to use.", false, "AttributeValues.xslt", "Behavior" )]
    public partial class AttributeCategoryView : RockBlock
    {
        private XDocument xDocument = null;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            string entityQualifierColumn = GetAttributeValue( "EntityQualifierColumn" );
            if ( string.IsNullOrWhiteSpace( entityQualifierColumn ) )
                entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

            string entityQualifierValue = GetAttributeValue( "EntityQualifierValue" );
            if ( string.IsNullOrWhiteSpace( entityQualifierValue ) )
                entityQualifierValue = PageParameter( "EntityQualifierValue" );

            // Get the context entity
            Rock.Data.IEntity contextEntity = this.ContextEntity();

            if ( contextEntity != null )
            {
                ObjectCache cache = MemoryCache.Default;
                string cacheKey = string.Format( "Attributes:{0}:{1}:{2}", contextEntity.TypeId, entityQualifierColumn, entityQualifierValue );

                Dictionary<string, List<int>> cachedAttributes = cache[cacheKey] as Dictionary<string, List<int>>;
                if ( cachedAttributes == null )
                {
                    cachedAttributes = new Dictionary<string, List<int>>();

                    AttributeService attributeService = new AttributeService();
                    var attributes = new List<Rock.Web.Cache.AttributeCache>();
                    foreach(var attribute in attributeService
                        .Get(contextEntity.TypeId, entityQualifierColumn, entityQualifierValue)
                        .OrderBy( a => a.Order ))
                    {
                        attributes.Add(Rock.Web.Cache.AttributeCache.Read(attribute));
                    }

                    foreach(var attributeCategory in Rock.Attribute.Helper.GetAttributeCategories(attributes))
                    {
                        if ( !cachedAttributes.ContainsKey( attributeCategory.CategoryName ) )
                        {
                            cachedAttributes.Add( attributeCategory.CategoryName, new List<int>() );
                        }

                        foreach ( int attributeId in attributeCategory.Attributes.Select( a => a.Id ) )
                        {
                            cachedAttributes[attributeCategory.CategoryName].Add( attributeId );
                        }
                    }

                    CacheItemPolicy cacheItemPolicy = null;
                    cache.Set( cacheKey, cachedAttributes, cacheItemPolicy );
                }

                Rock.Attribute.IHasAttributes model = contextEntity as Rock.Attribute.IHasAttributes;
                if ( model != null )
                {
                    var rootElement = new XElement( "root" );

                    foreach ( string category in GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues( false ) )
                    {
                        if ( cachedAttributes.ContainsKey( category ) )
                        {
                            var attributesElement = new XElement( "attributes",
                                new XAttribute( "category-name", category )
                                );
                            rootElement.Add( attributesElement );

                            foreach ( var attributeId in cachedAttributes[category] )
                            {
                                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                                if ( attribute != null )
                                {
                                    var values = model.AttributeValues[attribute.Key];
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
                    }

                    xDocument = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );

                    xmlContent.DocumentContent = xDocument.ToString();
                    xmlContent.TransformSource = Server.MapPath( "~/Themes/" + CurrentPage.Site.Theme + "/Assets/Xslt/" + GetAttributeValue( "XsltFile" ) );
                }
            }
        }
    }
}
