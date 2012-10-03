//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Core;
using Rock.Field;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for editing the value(s) of a set of attributes for a given entity and category
    /// </summary>
    [Rock.Attribute.Property( 1, "Entity Qualifier Column", "Filter", "The entity column to evaluate when determining if this attribute applies to the entity", false, "" )]
    [Rock.Attribute.Property( 2, "Entity Qualifier Value", "Filter", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "" )]
    [Rock.Attribute.Property( 3, "Attribute Category", "Filter", "Attribute Category", true, "" )]
    public partial class AttributeCategoryView : Rock.Web.UI.ContextBlock
    {
        protected string _category = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            string entityQualifierColumn = AttributeValue( "EntityQualifierColumn" );
            if ( string.IsNullOrWhiteSpace( entityQualifierColumn ) )
                entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

            string entityQualifierValue = AttributeValue( "EntityQualifierValue" );
            if ( string.IsNullOrWhiteSpace( entityQualifierValue ) )
                entityQualifierValue = PageParameter( "EntityQualifierValue" );

            _category = AttributeValue( "AttributeCategory" );
            if ( string.IsNullOrWhiteSpace( _category ) )
                _category = PageParameter( "AttributeCategory" );

            ObjectCache cache = MemoryCache.Default;
            string cacheKey = string.Format( "Attributes:{0}:{1}:{2}", base.EntityType, entityQualifierColumn, entityQualifierValue );

            Dictionary<string, List<int>> cachedAttributes = cache[cacheKey] as Dictionary<string, List<int>>;
            if ( cachedAttributes == null )
            {
                cachedAttributes = new Dictionary<string, List<int>>();

                AttributeService attributeService = new AttributeService();
                foreach ( var item in attributeService.Queryable().
                    Where( a => a.Entity == EntityType &&
                        ( a.EntityQualifierColumn ?? string.Empty ) == entityQualifierColumn &&
                        ( a.EntityQualifierValue ?? string.Empty ) == entityQualifierValue ).
                    OrderBy( a => a.Category ).
                    ThenBy( a => a.Order ).
                    Select( a => new { a.Category, a.Id } ) )
                {
                    if ( !cachedAttributes.ContainsKey( item.Category ) )
                        cachedAttributes.Add( item.Category, new List<int>() );
                    cachedAttributes[item.Category].Add( item.Id );
                }

                CacheItemPolicy cacheItemPolicy = null;
                cache.Set( cacheKey, cachedAttributes, cacheItemPolicy );
            }

            Rock.Attribute.IHasAttributes model = base.Entity as Rock.Attribute.IHasAttributes;
            if ( model != null )
            {
                if ( cachedAttributes.ContainsKey( _category ) )
                    foreach ( var attributeId in cachedAttributes[_category] )
                    {
                        var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                        if ( attribute != null )
                        {
                            var li = new HtmlGenericControl("li");
                            ulAttributes.Controls.Add(li);
                            li.Controls.Add(new LiteralControl(attribute.Name));

                            li.Controls.Add( new LiteralControl( ": " ) );

                            var span = new HtmlGenericControl("span");
                            li.Controls.Add(span);
                            span.AddCssClass("value");

                            var values = model.AttributeValues[attribute.Key].Value;
                            if (values != null && values.Count > 0)
                                span.Controls.Add(new LiteralControl(attribute.FieldType.Field.FormatValue(span, values[0].Value, attribute.QualifierValues, false)));
                        }
                    }
            }
        }
    }
}
