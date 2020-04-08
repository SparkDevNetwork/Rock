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
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Security;

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

            string script = string.Format( @"Rock.controls.searchField.initialize({{ controlId: '{0}' }});", this.ClientID );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "search-field-" + this.ID, script, true );

            this.CssClass = "searchinput";
            this.AccessKey = "q";
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

            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                foreach ( KeyValuePair<int, Lazy<Rock.Search.SearchComponent, Rock.Extension.IComponentData>> service in Rock.Search.SearchContainer.Instance.Components )
                {
                    var searchComponent = service.Value.Value;
                    if ( searchComponent.IsAuthorized( Authorization.VIEW, rockPage.CurrentPerson ) )
                    {
                        if ( !searchComponent.AttributeValues.ContainsKey( "Active" ) || bool.Parse( searchComponent.AttributeValues["Active"].Value ) )
                        {
                            searchExtensions.Add( service.Key.ToString(), Tuple.Create<string, string>( searchComponent.SearchLabel, searchComponent.ResultUrl ) );
                            if ( string.IsNullOrWhiteSpace( hfFilter.Value ) )
                                hfFilter.Value = service.Key.ToString();
                        }
                    }
                }
            }

            writer.AddAttribute( "class", "smartsearch " + this.CssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "fa fa-search" );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();

            

            writer.AddAttribute( "class", "nav pull-right smartsearch-type" );
            writer.RenderBeginTag( HtmlTextWriterTag.Ul );

            writer.AddAttribute( "class", "dropdown" );
            writer.RenderBeginTag( HtmlTextWriterTag.Li );

            writer.AddAttribute("class", "dropdown-toggle navbar-link");
            writer.AddAttribute( "data-toggle", "dropdown" );
            writer.RenderBeginTag( HtmlTextWriterTag.A);

            // wrap item in span for css hook
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            if ( searchExtensions.ContainsKey( hfFilter.Value ) )
            {
                writer.Write( searchExtensions[hfFilter.Value].Item1 );
            }
            writer.RenderEndTag();

            // add carat
            writer.AddAttribute( "class", "fa fa-caret-down" );
            writer.RenderBeginTag( HtmlTextWriterTag.B );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute( "class", "dropdown-menu" );
            writer.RenderBeginTag( HtmlTextWriterTag.Ul );

            foreach ( var searchExtension in searchExtensions )
            {
                writer.AddAttribute( "data-key", searchExtension.Key );
                writer.AddAttribute( "data-target", searchExtension.Value.Item2 );
                writer.RenderBeginTag( HtmlTextWriterTag.Li );

                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.Write( searchExtension.Value.Item1 );
                writer.RenderEndTag();
                
                writer.RenderEndTag();
            }

            writer.RenderEndTag(); //ul
            writer.RenderEndTag(); //li
            writer.RenderEndTag(); //ul

            base.RenderControl( writer );

            hfFilter.RenderControl(writer);

            writer.RenderEndTag(); //div
        }
    }
}