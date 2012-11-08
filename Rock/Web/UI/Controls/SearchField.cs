using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:SearchField runat=server></{0}:SearchField>" )]
    public class SearchField : TextBox
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = string.Format( @"
    $(document).ready(function() {{

        $('.dropdown dt').click(function () {{
            $('.dropdown dd ul').fadeToggle('fast');
        }});

        // change selection when picked
        $('.dropdown dd ul li').click(function () {{
            var text = $(this).html();
            $('.dropdown dt').html(text);
            $('.dropdown dd ul').hide();
            $('#{1}_hSearchFilter').val($(this).attr('key'));
        }});

        $('input#{0}').autocomplete({{
            source: function( request, response ) {{
                $.ajax({{
                    url: rock.baseUrl + 'api/search?type=' +  $('#{1}_hSearchFilter:first').val() + '&term=' + request.term,
                    dataType: 'json',
                    success: function(data, status, xhr){{ 
                        response($.map(data, function (item) {{
                            return {{
                                value: item
                            }}
                        }}))
                    }},
                    error: function(xhr, status, error) {{
                        alert(status + ' [' + error + ']: ' + xhr.reponseText);
                    }}
                }});
            }},
            minLength: 2,
            appendTo: 'div.filter-search'
        }});

        $('input#{0}').keyup(function(event){{
            if(event.keyCode == 13){{
                var keyValue = $('#{1}_hSearchFilter:first').val();
                var $li = $('.dropdown dd ul li[key=""' + keyValue + '""]:first');
                var target = $li.attr('target');
                window.location.href = rock.baseUrl + target.replace('{{0}}',encodeURIComponent($(this).val()));
            }}
        }});


    }});
", this.ClientID, this.ID );

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), "search-field-" + this.ID.ToString(), script, true );
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            HiddenField hfFilter = new HiddenField();
            hfFilter.ID = this.ID + "_hSearchFilter";

            var searchExtensions = new Dictionary<string,Tuple<string, string>>();
            foreach ( KeyValuePair<int, Lazy<Rock.Search.SearchComponent, Rock.Extension.IComponentData>> service in Rock.Search.SearchContainer.Instance.Components )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"][0].Value ) )
                {
                    searchExtensions.Add( service.Key.ToString(), Tuple.Create<string, string>( service.Value.Value.SearchLabel, service.Value.Value.ResultUrl ) );
                    if ( string.IsNullOrWhiteSpace( hfFilter.Value ) )
                        hfFilter.Value = service.Key.ToString();
                }

            writer.AddAttribute( "class", "filter-search" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            base.Render( writer );

            writer.AddAttribute( "class", "filter" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "dropdown" );
            writer.RenderBeginTag( HtmlTextWriterTag.Dl );

            writer.RenderBeginTag( HtmlTextWriterTag.Dt );
            writer.Write( searchExtensions[hfFilter.Value].Item1 );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Dd );
            writer.RenderBeginTag( HtmlTextWriterTag.Ul );

            foreach ( var searchExtension in searchExtensions )
            {
                writer.AddAttribute( "key", searchExtension.Key );
                writer.AddAttribute( "target", searchExtension.Value.Item2 );
                writer.RenderBeginTag( HtmlTextWriterTag.Li );
                writer.Write( searchExtension.Value.Item1 );
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();

            hfFilter.RenderControl(writer);

            writer.RenderEndTag();
            writer.RenderEndTag();
        }
    }
}