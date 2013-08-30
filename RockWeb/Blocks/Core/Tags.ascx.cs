//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [ContextAware]
    [TextField( "Entity Qualifier Column", "The entity column to evaluate when determining if this attribute applies to the entity", false, "", "Filter", 0 )]
    [TextField( "Entity Qualifier Value", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "", "Filter", 1 )]
    public partial class Tags : RockBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            string entityQualifierColumn = GetAttributeValue( "EntityQualifierColumn" );
            if ( string.IsNullOrWhiteSpace( entityQualifierColumn ) )
                entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

            string entityQualifierValue = GetAttributeValue( "EntityQualifierValue" );
            if ( string.IsNullOrWhiteSpace( entityQualifierValue ) )
                entityQualifierValue = PageParameter( "EntityQualifierValue" );

            var sb = new StringBuilder();

            // Get the context entity
            Rock.Data.IEntity contextEntity = this.ContextEntity();

            if ( contextEntity != null )
            {
                var service = new TaggedItemService();
                foreach ( dynamic item in service.Get(
                    contextEntity.TypeId, entityQualifierColumn, entityQualifierValue, CurrentPersonId, contextEntity.Guid )
                    .Select( i => new {
                        OwnerId = i.Tag.OwnerId,
                        Name = i.Tag.Name
                    }))
                {
                    if ( sb.Length > 0 )
                        sb.Append( ',' );
                    sb.Append( item.Name );
                    if ( CurrentPersonId.HasValue && item.OwnerId == CurrentPersonId.Value )
                        sb.Append( "^personal" );
                }

                phTags.Controls.Add( new LiteralControl( string.Format(
                    "<input name=\"person-tags\" id=\"person-tags\" value=\"{0}\" />", sb.ToString() ) ) );

                string script = string.Format( @"
    $(document).ready(function () {{
        $('ul.ui-autocomplete').css('width', '300px');
        $('#person-tags').tagsInput({{
            'autocomplete_url': function( request, response ) {{
                $.ajax({{
                    url: Rock.settings.get('baseUrl') + 'api/tags/availablenames/{0}/{1}/{2}{3}{4}',
                    dataType: 'json',
                    success: function(data, status, xhr){{ 
                        response($.map(data, function (item) {{
                            return {{
                                value: item.Name,
                                class: item.OwnerId == null || item.OwnerId == '' ? 'system' : 'personal'
                            }}
                        }}))
                    }},
                    error: function(xhr, status, error) {{
                        alert('availablenames status: ' + status + ' [' + error + ']: ' + xhr.reponseText);
                    }}
                }});
            }},
            autoCompleteAppendTo: 'div.tag-wrap',
            autoCompleteMessages: {{
              noResults: function () {{ }},
              results: function () {{ }}
            }},
            'height': 'auto',
            'width': '100%',
            'interactive': true,
            'defaultText': 'add tag',
            'removeWithBackspace': false,
            'onAddTag': verifyTag,
            'onRemoveTag': RemoveTag,
            'enableDelete': true
        }});
    }});

    function verifyTag(tagName) {{
        $.ajax({{
            type: 'GET',
            url: Rock.settings.get('baseUrl') + 'api/tags/{0}/{1}/' + tagName + '{3}{4}',
            statusCode: {{
                404: function () {{
                        var r = confirm(""A tag called '"" + tagName + ""' does not exist. Do you want to create a new personal tag?"");
                        if (r == true) {{
                            AddTag(tagName);
                        }}
                        else {{
                            // remove tag
                            $('#person-tags').removeTag(tagName);
                        }}
                    }},
                200: function (data, status, xhr) {{
                        AddTag(tagName);
                    }}
            }},
        }});
    }}

    function AddTag(tagName) {{
        $.ajax({{
            type: 'POST',
            url: Rock.settings.get('baseUrl') + 'api/taggeditems/{0}/{1}/{2}/' + tagName + '{3}{4}',
            error: function (xhr, status, error) {{
                alert('AddTag() status: ' + status + ' [' + error + ']: ' + xhr.responseText);
            }}
        }});
    }}

    function RemoveTag(tagName) {{
        $.ajax({{
            type: 'DELETE',
            url: Rock.settings.get('baseUrl') + 'api/taggeditems/{0}/{1}/{2}/' + tagName + '{3}{4}',
            error: function (xhr, status, error) {{
                alert('RemoveTag() status: ' + status + ' [' + error + ']: ' + xhr.responseText);
            }}
        }});
    }}

",
    contextEntity.TypeId, CurrentPersonId, contextEntity.Guid.ToString(),
    string.IsNullOrWhiteSpace( entityQualifierColumn ) ? "" : "/" + entityQualifierColumn,
    string.IsNullOrWhiteSpace( entityQualifierValue ) ? "" : "/" + entityQualifierValue );
                this.Page.ClientScript.RegisterStartupScript( this.GetType(), "tags-" + this.CurrentBlock.Id.ToString(), script, true );
            }
        }
    }
}