//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:TagList runat=server></{0}:TagList>" )]
    public class TagList : CompositeControl
    {
        public int EntityTypeId
        {
            get { return ViewState["EntityTypeId"] as int? ?? 0; }
            set { ViewState["EntityTypeId"] = value; }
        }

        public string EntityQualifierColumn
        {
            get { return ViewState["EntityQualifierColumn"] as string ?? string.Empty; }
            set { ViewState["EntityQualifierColumn"] = value; }
        }

        public string EntityQualifierValue
        {
            get { return ViewState["EntityQualifierValue"] as string ?? string.Empty; }
            set { ViewState["EntityQualifierValue"] = value; }
        }

        public int EntityId
        {
            get { return ViewState["EntityId"] as int? ?? 0; }
            set { ViewState["EntityId"] = value; }
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }
        public override void RenderControl( HtmlTextWriter writer )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                var sb = new StringBuilder();

                var service = new TaggedItemService();
                foreach ( dynamic item in service.Get(
                    EntityTypeId, EntityQualifierColumn, EntityQualifierValue, rockPage.CurrentPersonId, EntityId )
                    .Select( i => new
                    {
                        OwnerId = i.Tag.OwnerId,
                        Name = i.Tag.Name
                    } ) )
                {
                    if ( sb.Length > 0 )
                        sb.Append( ',' );
                    sb.Append( item.Name );
                    if ( rockPage.CurrentPersonId.HasValue && item.OwnerId == rockPage.CurrentPersonId.Value )
                        sb.Append( "^personal" );
                }

                var input = new HtmlGenericControl( "input" );
                input.ID = this.ID;
                input.Attributes.Add("value", sb.ToString());
                input.RenderControl(writer);

                string script = string.Format( @"

    $('ul.ui-autocomplete').css('width', '300px');

    $('#{5}').tagsInput({{
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
        'onAddTag': {5}_verifyTag,
        'onRemoveTag': {5}_RemoveTag,
        'enableDelete': true
    }});

    function {5}_verifyTag(tagName) {{
        $.ajax({{
            type: 'GET',
            url: rock.baseUrl + 'api/tags/{0}/{1}/' + tagName + '{3}{4}',
            statusCode: {{
                404: function () {{
                        var r = confirm(""A tag called '"" + tagName + ""' does not exist. Do you want to create a new personal tag?"");
                        if (r == true) {{
                            {5}_AddTag(tagName);
                        }}
                        else {{
                            // remove tag
                            $('#{5}').removeTag(tagName);
                        }}
                    }},
                200: function (data, status, xhr) {{
                        {5}_AddTag(tagName);
                    }}
            }},
        }});
    }}

    function {5}_AddTag(tagName) {{
        $.ajax({{
            type: 'POST',
            url: rock.baseUrl + 'api/taggeditems/{0}/{1}/{2}/' + tagName + '{3}{4}',
            error: function (xhr, status, error) {{
                alert('AddTag() status: ' + status + ' [' + error + ']: ' + xhr.responseText);
            }}
        }});
    }}

    function {5}_RemoveTag(tagName) {{
        $.ajax({{
            type: 'DELETE',
            url: rock.baseUrl + 'api/taggeditems/{0}/{1}/{2}/' + tagName + '{3}{4}',
            error: function (xhr, status, error) {{
                alert('RemoveTag() status: ' + status + ' [' + error + ']: ' + xhr.responseText);
            }}
        }});
    }}

",
    EntityTypeId, rockPage.CurrentPersonId, EntityId,
    string.IsNullOrWhiteSpace( EntityQualifierColumn ) ? "" : "/" + EntityQualifierColumn,
    string.IsNullOrWhiteSpace( EntityQualifierValue ) ? "" : "/" + EntityQualifierValue,
    this.ID);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "tag_picker_" + this.ID, script, true);
            }
        }
    }
}