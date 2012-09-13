//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Text;
using System.Web.UI;

using Rock.Core;

namespace RockWeb.Blocks.Core
{
	[Rock.Attribute.Property( 1, "Entity Qualifier Column", "Filter", "The entity column to evaluate when determining if this attribute applies to the entity", false, "" )]
	[Rock.Attribute.Property( 2, "Entity Qualifier Value", "Filter", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "" )]
	public partial class Tags : Rock.Web.UI.ContextBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
			string entityQualifierColumn = AttributeValue( "EntityQualifierColumn" );
			if ( string.IsNullOrWhiteSpace( entityQualifierColumn ) )
				entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

			string entityQualifierValue = AttributeValue( "EntityQualifierValue" );
			if ( string.IsNullOrWhiteSpace( entityQualifierValue ) )
				entityQualifierValue = PageParameter( "EntityQualifierValue" );

			var sb = new StringBuilder();

			if ( base.Entity != null )
			{
				var service = new TaggedItemService();
				foreach ( dynamic item in service.GetByEntity(
					base.EntityType, entityQualifierColumn, entityQualifierValue, CurrentPersonId, base.Entity.Id )
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
					url: rock.baseUrl + 'api/tags/availablenames/{0}/{1}/{2}{3}{4}',
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
						alert(status + ' [' + error + ']: ' + xhr.reponseText);
					}}
				}});
			}},
            autoCompleteAppendTo: 'div.tag-wrap',
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
            url: rock.baseUrl + 'api/tags/{0}/{1}/' + tagName + '{3}{4}',
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
            url: rock.baseUrl + 'api/taggeditems/{0}/{1}/{2}/' + tagName + '{3}{4}',
            error: function (xhr, status, error) {{
                alert(status + ' [' + error + ']: ' + xhr.responseText);
            }}
        }});
    }}

    function RemoveTag(tagName) {{
		$.ajax({{
			type: 'DELETE',
            url: rock.baseUrl + 'api/taggeditems/{0}/{1}/{2}/' + tagName + '{3}{4}',
            error: function (xhr, status, error) {{
                alert(status + ' [' + error + ']: ' + xhr.responseText);
            }}
        }});
    }}

",
	base.EntityType, CurrentPersonId, base.Entity.Id,
	string.IsNullOrWhiteSpace( entityQualifierColumn ) ? "" : "/" + entityQualifierColumn,
	string.IsNullOrWhiteSpace( entityQualifierValue ) ? "" : "/" + entityQualifierValue );
				this.Page.ClientScript.RegisterStartupScript( this.GetType(), "tags-" + this.BlockInstance.Id.ToString(), script, true );
			}
		}
    }
}