//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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
    public class PagePicker : ItemPicker, ILabeledControl
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
        /// Initializes a new instance of the <see cref="AccountPicker" /> class.
        /// </summary>
        public PagePicker()
            : base()
        {
            label = new Label();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            ItemRestUrlExtraParams = string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="page">The page.</param>
        public void SetValue( Rock.Model.Page page )
        {
            if ( page != null )
            {
                ItemId = page.Id.ToString();

                string parentPageIds = string.Empty;
                var parentPage = page.ParentPage;
                while ( parentPage != null )
                {
                    parentPageIds = parentPage.Id + "," + parentPageIds;
                    parentPage = parentPage.ParentPage;
                }

                InitialItemParentIds = parentPageIds.TrimEnd( new char[] { ',' } );
                ItemName = page.Name;
            }
            else
            {
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="pages">The pages.</param>
        public void SetValues( IEnumerable<Rock.Model.Page> pages )
        {
            var thePages = pages.ToList();

            if ( thePages.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentPageIds = string.Empty;

                foreach ( var page in thePages )
                {
                    if ( page != null )
                    {
                        ids.Add( page.Id.ToString() );
                        names.Add( page.Name );
                        var parentPage = page.ParentPage;

                        while ( parentPage != null )
                        {
                            parentPageIds += parentPage.Id.ToString() + ",";
                            parentPage = parentPage.ParentPage;
                        }
                    }
                }

                InitialItemParentIds = parentPageIds.TrimEnd( new[] { ',' } );
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
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void SetValueOnSelect()
        {
            var page = new PageService().Get( int.Parse( ItemId ) );
            this.SetValue( page );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var pages = new PageService().Queryable().Where( p => ItemIds.Contains( p.Id.ToString() ) );
            this.SetValues( pages );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/pages/getchildren/"; }
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