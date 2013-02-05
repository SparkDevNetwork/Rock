//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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


        // change selection when picked
        $('.smart-search .dropdown-menu a').click(function () {{
            var text = $(this).html();
            $('.smart-search .dropdown a.dropdown-toggle span').html(text);
            $('#{1}_hSearchFilter').val($(this).parent().attr('key'));
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
            appendTo: 'div.smart-search',
            messages: {{
              noResults: function () {{ }},
              results: function () {{ }}
            }}
        }});

        $('input#{0}').keyup(function(event){{
            if(event.keyCode == 13){{
                var keyValue = $('#{1}_hSearchFilter:first').val();
                var $li = $('.dropdown ul li[key=""' + keyValue + '""]:first');
                var target = $li.attr('target');
                window.location.href = rock.baseUrl + target.replace('{{0}}',encodeURIComponent($(this).val()));
            }}
        }});


    }});
", this.ClientID, this.ID );

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), "search-field-" + this.ID.ToString(), script, true );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
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

            writer.AddAttribute( "class", "smart-search" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            base.RenderControl( writer );

            writer.AddAttribute( "class", "nav pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Ul );

            writer.AddAttribute( "class", "dropdown" );
            writer.RenderBeginTag( HtmlTextWriterTag.Li );
            
            writer.AddAttribute( "class", "dropdown-toggle" );
            writer.AddAttribute( "data-toggle", "dropdown" );
            writer.RenderBeginTag( HtmlTextWriterTag.A);

            // wrap item in span for css hook
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.Write( searchExtensions[hfFilter.Value].Item1 );
            writer.RenderEndTag();

            // add carat
            writer.AddAttribute( "class", "caret" );
            writer.RenderBeginTag( HtmlTextWriterTag.B );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute( "class", "dropdown-menu" );
            writer.RenderBeginTag( HtmlTextWriterTag.Ul );

            foreach ( var searchExtension in searchExtensions )
            {
                writer.AddAttribute( "key", searchExtension.Key );
                writer.AddAttribute( "target", searchExtension.Value.Item2 );
                writer.RenderBeginTag( HtmlTextWriterTag.Li );

                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.Write( searchExtension.Value.Item1 );
                writer.RenderEndTag();
                
                writer.RenderEndTag();
            }

            writer.RenderEndTag(); //ul
            writer.RenderEndTag(); //li
            writer.RenderEndTag(); //ul

            hfFilter.RenderControl(writer);

            writer.RenderEndTag(); //div
        }
    }
}