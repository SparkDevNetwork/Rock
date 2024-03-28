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
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for editing the security of an item in a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:SecurityField BoundField=server></{0}:SecurityField>" )]
    public class SecurityField : RockTemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass
        {
            get
            {
                string iconCssClass = ViewState["IconCssClass"] as string;
                if ( string.IsNullOrWhiteSpace( iconCssClass ) )
                {
                    iconCssClass = "fa fa-lock";
                    ViewState["IconCssClass"] = iconCssClass;
                }
                return iconCssClass;
            }
            set
            {
                ViewState["IconCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the button CSS class.
        /// </summary>
        /// <value>
        /// The button CSS class.
        /// </value>
        public string ButtonCssClass
        {
            get
            {
                string buttonCssClass = ViewState["ButtonCssClass"] as string;
                if ( string.IsNullOrWhiteSpace( buttonCssClass ) )
                {
                    buttonCssClass = "btn btn-security btn-sm";
                    ViewState["ButtonCssClass"] = buttonCssClass;
                }
                return buttonCssClass;
            }
            set
            {
                ViewState["ButtonCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the tooltip.
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        public string ToolTip
        {
            get
            {
                // Uncomment this if we want to have a default value
                //string tooltip = ViewState["Tooltip"] as string;
                //if ( string.IsNullOrWhiteSpace( tooltip ) )
                //{
                //    tooltip = "Secure";
                //    ViewState["Tooltip"] = tooltip;
                //}
                //return tooltip;

                return ViewState["Tooltip"] as string;
            }
            set
            {
                ViewState["Tooltip"] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityField" /> class.
        /// </summary>
        public SecurityField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "grid-columncommand";
            this.ItemStyle.CssClass = "grid-columncommand";
            this.Title = "Secure";
        }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the field that is displayed in the title.
        /// </summary>
        /// <value>
        /// The title field name that is displayed.
        /// </value>
        public string TitleField { get; set; }

        /// <summary>
        /// Gets or sets the display title which is also a prefix to <seealso cref="TitleField"/> if one is specified. Defaults to the title "Secure".
        /// </summary>
        /// <value>
        /// The title name.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// When exporting a grid to Excel, this property controls whether a column is included
        /// in the export. See <seealso cref="ExcelExportBehavior" />.
        /// </summary>
        public override ExcelExportBehavior ExcelExportBehavior
        {
            get
            {
                return ExcelExportBehavior.NeverInclude;
            }
            set
            {
                base.ExcelExportBehavior = value;
            }
        }

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField"/>.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            SecurityFieldTemplate editFieldTemplate = new SecurityFieldTemplate( control.Page, EntityTypeId, TitleField, Title );
            this.ItemTemplate = editFieldTemplate;

            return base.Initialize( sortingEnabled, control );
        }
    }

    /// <summary>
    /// Template used by the <see cref="SecurityField"/> control
    /// </summary>
    public class SecurityFieldTemplate : ITemplate
    {
        private System.Web.UI.Page page;

        /// <summary>
        /// Gets or sets the field that is displayed in the title.
        /// </summary>
        /// <value>
        /// The title field name that is displayed.
        /// </value>
        public string TitleField { get; set; }

        /// <summary>
        /// Gets or sets the display title which is also a prefix to <seealso cref="TitleField"/> if one is specified. Defaults to the title "Secure".
        /// </summary>
        /// <value>
        /// The title name.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityFieldTemplate" /> class.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="titleField">The title field.</param>
        [Obsolete( "Use the SecurityFieldTemplate constructor with the [string:title] parameter.", false )]
        [RockObsolete( "1.14" )]
        public SecurityFieldTemplate( System.Web.UI.Page page, int entityTypeId, string titleField )
        {
            this.page = page;
            this.EntityTypeId = entityTypeId;
            this.TitleField = titleField;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityFieldTemplate" /> class.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="titleField">The title field.</param>
        /// <param name="title">The title name. Which will be used as a prefix to <seealso cref="TitleField"/> if one is specified.</param>
        public SecurityFieldTemplate( System.Web.UI.Page page, int entityTypeId, string titleField, string title )
        {
            this.page = page;
            this.EntityTypeId = entityTypeId;
            this.TitleField = titleField;
            this.Title = title;
        }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                SecurityField securityField = cell.ContainingField as SecurityField;
                HtmlGenericControl aSecure = new HtmlGenericControl( "a" );
                cell.Controls.Add( aSecure );
                aSecure.Attributes.Add( "class", securityField.ButtonCssClass );

                // height attribute is used by the modal that pops up when the button is clicked
                aSecure.Attributes.Add( "height", "500px" );
                aSecure.Attributes.Add( "title", securityField.ToolTip );

                HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                buttonIcon.Attributes.Add( "class", securityField.IconCssClass );
                aSecure.Controls.Add( buttonIcon );

                aSecure.DataBinding += new EventHandler( aSecure_DataBinding );
            }
        }

        void aSecure_DataBinding( object sender, EventArgs e )
        {
            HtmlGenericControl lnk = ( HtmlGenericControl ) sender;
            GridViewRow container = ( GridViewRow ) lnk.NamingContainer;

            var title = Title;

            if ( !TitleField.IsNullOrWhiteSpace() )
            {
                object titleFieldName = DataBinder.Eval( container.DataItem, TitleField );
                if ( titleFieldName != DBNull.Value )
                {
                    if ( !title.IsNullOrWhiteSpace() )
                    {
                        title = $"{title} {titleFieldName}";
                    }
                    else
                    {
                        title = titleFieldName.ToString();
                    }
                }
            }

            // Get Id
            var idValue = DataBinder.Eval( container.DataItem, "id" ) as int?;
            if ( idValue.HasValue && idValue > 0 )
            {
                /*
                 * 01/26/2024 - KA
                 * Any existing ampersand is 'encoded' before calling HttpUtility.UrlEncode because Asp.Net's
                 * HttpRequestBase decodes the url and interprets the encoded '&' as a query parameter delimiter
                 * which results in null values when parsing to the QueryString parameter on HttpRequestBase.
                 * However in this scenario the '&' has to be included as the value for the 't' key.
                 * See https://stackoverflow.com/questions/3667902/c-sharp-asp-net-httpwebrequest-automatically-decodes-ampersand-values-from-q.
                 *
                */
                title = title.Replace( "&", "%26" );
                title = HttpUtility.UrlEncode( title.EscapeQuotes() );
                string url = page.ResolveUrl( string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done",
                    EntityTypeId, idValue.ToString(), title ) );
                lnk.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + url.EscapeQuotes() + "')" );
                lnk.Visible = true;
            }
            else
            {
                lnk.Visible = false;
            }
        }
    }
}