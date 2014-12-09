// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        /// Initializes a new instance of the <see cref="SecurityField" /> class.
        /// </summary>
        public SecurityField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "grid-columncommand";
            this.ItemStyle.CssClass = "grid-columncommand";
        }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the field that contains the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string TitleField { get; set; }

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
            SecurityFieldTemplate editFieldTemplate = new SecurityFieldTemplate(control.Page, EntityTypeId, TitleField);
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
        /// Gets or sets the title field
        /// </summary>
        /// <value>
        /// The title field
        /// </value>
        public string TitleField { get; set; }

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
        public SecurityFieldTemplate(System.Web.UI.Page page, int entityTypeId, string titleField )
        {
            this.page = page;
            this.EntityTypeId = entityTypeId;
            this.TitleField = titleField;
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
                HtmlGenericControl aSecure = new HtmlGenericControl( "a" );
                cell.Controls.Add( aSecure );
                aSecure.Attributes.Add("class", "btn btn-security btn-sm");

                // height attribute is used by the modal that pops up when the button is clicked
                aSecure.Attributes.Add( "height", "500px" );

                HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                buttonIcon.Attributes.Add( "class", "fa fa-lock" );
                aSecure.Controls.Add( buttonIcon );

                aSecure.DataBinding += new EventHandler( aSecure_DataBinding );
            }
        }

        void aSecure_DataBinding( object sender, EventArgs e )
        {
            HtmlGenericControl lnk = (HtmlGenericControl)sender;
            GridViewRow container = (GridViewRow)lnk.NamingContainer;

            // Get title
            string title = "Security";
            if ( !string.IsNullOrWhiteSpace( TitleField ) )
            {
                object titleValue = DataBinder.Eval( container.DataItem, TitleField );
                if ( titleValue != DBNull.Value )
                {
                    title = titleValue.ToString();
                }
            }

            // Get Id
            object dataValue = DataBinder.Eval( container.DataItem, "id" );
            if ( dataValue != DBNull.Value )
            {
                string url = page.ResolveUrl( string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done",
                    EntityTypeId, dataValue.ToString(), title ) );
                lnk.Attributes.Add( "href", "javascript: Rock.controls.modal.show($(this), '" + url.EscapeQuotes() + "')" );
            }
        }
    }
}