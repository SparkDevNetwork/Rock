//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.UI;

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
    [TextField( 1, "Entity Qualifier Column", "Filter", "The entity column to evaluate when determining if this attribute applies to the entity", false, "" )]
    [TextField( 2, "Entity Qualifier Value", "Filter", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "" )]
    [TextField( 3, "Attribute Category", "Filter", "Attribute Category", true, "" )]
    public partial class ContextAttributeValues : RockBlock
    {
        protected string _category = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            string entityQualifierColumn = GetAttributeValue( "EntityQualifierColumn" );
            if ( string.IsNullOrWhiteSpace( entityQualifierColumn ) )
                entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

            string entityQualifierValue = GetAttributeValue( "EntityQualifierValue" );
            if ( string.IsNullOrWhiteSpace( entityQualifierValue ) )
                entityQualifierValue = PageParameter( "EntityQualifierValue" );

            _category = GetAttributeValue( "AttributeCategory" );
            if ( string.IsNullOrWhiteSpace( _category ) )
                _category = PageParameter( "AttributeCategory" );

            // Get the context entity
            int? contextEntityTypeId = null;
            Rock.Data.IEntity contextEntity = null;
            foreach ( KeyValuePair<string, Rock.Data.IEntity> entry in ContextEntities )
            {
                contextEntityTypeId = entry.Value.TypeId;
                contextEntity = entry.Value;
                // Should only be one.
                break;
            }

            if ( contextEntityTypeId.HasValue && contextEntity != null)
            {
                ObjectCache cache = MemoryCache.Default;
                string cacheKey = string.Format( "Attributes:{0}:{1}:{2}", contextEntityTypeId, entityQualifierColumn, entityQualifierValue );

                Dictionary<string, List<int>> cachedAttributes = cache[cacheKey] as Dictionary<string, List<int>>;
                if ( cachedAttributes == null )
                {
                    cachedAttributes = new Dictionary<string, List<int>>();

                    AttributeService attributeService = new AttributeService();
                    foreach ( var item in attributeService
                        .Get( contextEntityTypeId, entityQualifierColumn, entityQualifierValue )
                        .OrderBy( a => a.Category )
                        .ThenBy( a => a.Order )
                        .Select( a => new { a.Category, a.Id } ) )
                    {
                        if ( !cachedAttributes.ContainsKey( item.Category ) )
                            cachedAttributes.Add( item.Category, new List<int>() );
                        cachedAttributes[item.Category].Add( item.Id );
                    }

                    CacheItemPolicy cacheItemPolicy = null;
                    cache.Set( cacheKey, cachedAttributes, cacheItemPolicy );
                }

                Rock.Attribute.IHasAttributes model = contextEntity as Rock.Attribute.IHasAttributes;
                if ( model != null )
                {
                    if ( cachedAttributes.ContainsKey( _category ) )
                        foreach ( var attributeId in cachedAttributes[_category] )
                        {
                            var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                            if ( attribute != null )
                                phAttributes.Controls.Add( /*(AttributeInstanceValues)*/this.LoadControl( "~/Blocks/Core/AttributeInstanceValues.ascx", model, attribute, CurrentPersonId ) );
                        }
                }
            }

            string script = @"
    Sys.Application.add_load(function () {
        $('div.context-attribute-values .delete').click(function(){
            return confirm('Are you sure?');
        });
    });
";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmDelete", script, true );

        }
    }
}
