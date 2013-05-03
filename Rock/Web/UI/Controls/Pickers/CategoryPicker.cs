//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class CategoryPicker : ItemPicker
    {
        private Label label;

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText
        {
            get { return label.Text; }
            set 
            { 
                label.Text = value;
                base.FieldName = label.Text;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryPicker" /> class.
        /// </summary>
        public CategoryPicker()
            : base()
        {
            label = new Label();
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="category">The category.</param>
        public void SetValue( Rock.Model.Category category )
        {
            if ( category != null )
            {
                ItemId = category.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = category.ParentCategory;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new char[] { ',' } );
                ItemName = category.Name;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="categories">The categories.</param>
        public void SetValues( IEnumerable<Category> categories )
        {
            var theCategories = categories.ToList();

            if ( theCategories.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var category in theCategories )
                {
                    if ( category != null )
                    {
                        ids.Add( category.Id.ToString() );
                        names.Add( category.Name );
                        var parentCategory = category.ParentCategory;

                        while ( parentCategory != null )
                        {
                            parentCategoryIds += parentCategory.Id.ToString() + ",";
                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var item = new CategoryService().Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var items = new CategoryService().Queryable().Where( i => ItemIds.Contains( i.Id.ToString() ) );
            this.SetValues( items );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/categories/getchildren/"; }
        }

        /// <summary>
        /// Sets the type of the category entity.
        /// </summary>
        /// <value>
        /// The type of the category entity.
        /// </value>
        public string CategoryEntityTypeName
        {
            set
            {
                ItemRestUrlExtraParams = "/" + value + "/false";
            }
        }
        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Add( label );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( string.IsNullOrEmpty( LabelText ) )
            {
                base.RenderControl( writer );
            }
            else
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                label.AddCssClass( "control-label" );

                label.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                base.Render( writer );

                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }
}