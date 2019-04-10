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
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with numerical validation 
    /// </summary>
    [ToolboxData( "<{0}:PersonLink runat=server></{0}:PersonLink>" )]
    public class PersonLink : HtmlAnchor
    {
        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int PersonId
        {
            get { return ViewState["PersonId"] as int? ?? 0; }
            set { ViewState["PersonId"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName
        {
            get { return ViewState["PersonName"] as string; }
            set { ViewState["PersonName"] = value; }
        }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        /// <value>
        /// The role.
        /// </value>
        public string Role
        {
            get { return ViewState["Role"] as string; }
            set { ViewState["Role"] = value; }
        }

        /// <summary>
        /// Gets or sets the photo id.
        /// </summary>
        /// <value>
        /// The photo id.
        /// </value>
        public int? PhotoId
        {
            get { return ViewState["PhotoId"] as int?; }
            set { ViewState["PhotoId"] = value; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.HtmlControls.HtmlContainerControl" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the <see cref="T:System.Web.UI.HtmlControls.HtmlContainerControl" /> content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            this.AddCssClass( "popover-person" );

            HRef = Path.Combine( System.Web.VirtualPathUtility.ToAbsolute( "~" ), string.Format( "Person/{0}", PersonId ) );

            this.Attributes["personId"] = PersonId.ToString();
            base.RenderBeginTag( writer );

            if ( PhotoId.HasValue && PhotoId.Value != 0 )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-circle" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Strong );
            writer.Write( PersonName );
            writer.RenderEndTag();

            base.RenderEndTag( writer );

            if ( !string.IsNullOrWhiteSpace( Role ) )
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Small);
                writer.Write( " (" );
                writer.Write( Role );
                writer.Write( ")" );
                writer.RenderEndTag();
            }

            string script = @"
    $('.popover-person').popover({
        placement: 'right', 
        trigger: 'manual',
        delay: 500,
        html: true,
        content: function() {
            var dataUrl = Rock.settings.get('baseUrl') + 'api/People/PopupHtml/' +  $(this).attr('personid');

            var result = $.ajax({ 
                                type: 'GET', 
                                url: dataUrl, 
                                dataType: 'json', 
                                contentType: 'application/json; charset=utf-8',
                                async: false }).responseText;
            
            var resultObject = jQuery.parseJSON(result);

            return resultObject.PickerItemDetailsHtml;

        }
    }).on('mouseenter', function () {
        var _this = this;
        $(this).popover('show');
        $(this).siblings('.popover').on('mouseleave', function () {
            $(_this).popover('hide');
        });
    }).on('mouseleave', function () {
        var _this = this;
        setTimeout(function () {
            if (!$('.popover:hover').length) {
                $(_this).popover('hide')
            }
        }, 100);
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "person-link-popover", script, true );

        }
    }
}