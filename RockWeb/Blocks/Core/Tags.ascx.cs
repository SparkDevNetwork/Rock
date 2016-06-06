// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Data;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Tags" )]
    [Category( "Core" )]
    [Description( "Add tags to current context object." )]

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
                var service = new TaggedItemService( new RockContext() );
                foreach ( dynamic item in service.Get(
                    contextEntity.TypeId, entityQualifierColumn, entityQualifierValue, CurrentPersonId, contextEntity.Guid )
                    .Select( i => new
                    {
                        OwnerId = ( i.Tag.OwnerPersonAlias != null ? i.Tag.OwnerPersonAlias.PersonId : (int?)null ),
                        Name = i.Tag.Name
                    } ) )
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
                    url: Rock.settings.get('baseUrl') + 'api/tags/availablenames/{0}/{1}/' + request.term + '/{2}{3}{4}',
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
            url: Rock.settings.get('baseUrl') + 'api/tags/{0}/{1}{3}{4}?name=' + encodeURIComponent(tagName),
            statusCode: {{
                404: function () {{
                        var r = confirm(""A tag called '"" + $('<div/>').text(tagName).html() + ""' does not exist. Do you want to create a new personal tag?"");
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
            url: Rock.settings.get('baseUrl') + 'api/taggeditems/{0}/{1}/{2}{3}{4}' + encodeURIComponent(tagName),
            error: function (xhr, status, error) {{
                alert('AddTag() status: ' + status + ' [' + error + ']: ' + xhr.responseText);
            }}
        }});
    }}

    function RemoveTag(tagName) {{
        $.ajax({{
            type: 'DELETE',
            url: Rock.settings.get('baseUrl') + 'api/taggeditems/{0}/{1}/{2}{3}{4}' + encodeURIComponent(tagName),
            error: function (xhr, status, error) {{
                alert('RemoveTag() status: ' + status + ' [' + error + ']: ' + xhr.responseText);
            }}
        }});
    }}

",
    contextEntity.TypeId, CurrentPersonId, contextEntity.Guid.ToString(),
    string.IsNullOrWhiteSpace( entityQualifierColumn ) ? "" : "/" + entityQualifierColumn,
    string.IsNullOrWhiteSpace( entityQualifierValue ) ? "" : "/" + entityQualifierValue );
                this.Page.ClientScript.RegisterStartupScript( this.GetType(), "tags-" + this.BlockId.ToString(), script, true );
            }
        }
    }
}