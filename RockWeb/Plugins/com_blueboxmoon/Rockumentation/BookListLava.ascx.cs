using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.blueboxmoon.Rockumentation;
using com.blueboxmoon.Rockumentation.Cache;
using com.blueboxmoon.Rockumentation.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.Rockumentation
{
    /// <summary>
    /// Displays a list of Book items.
    /// </summary>
    [DisplayName( "Book List Lava" )]
    [Category( "Blue Box Moon > Rockumentation" )]
    [Description( "Displays a list of Documentation Books formatted in Lava." )]

    #region Block Attributes

    [LinkedPage( "View Book Page",
        IsRequired = true,
        Order = 0 )]

    [CodeEditorField( "Lava Template",
        Description = "The Lava template to use when rendering the collection of books.",
        IsRequired = true,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        DefaultValue = "{% include '~/Plugins/com_blueboxmoon/Rockumentation/Assets/BookShelf.lava' %}",
        Order = 1 )]

    #endregion

    public partial class BookListLava : RockBlock
    {
        #region Attribute Properties

        protected Guid? ViewBookPage
        {
            get
            {
                return GetAttributeValue( "ViewBookPage" ).AsGuidOrNull();
            }
        }

        protected string LavaTemplate
        {
            get
            {
                return GetAttributeValue( "LavaTemplate" );
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Plugins/com_blueboxmoon/Rockumentation/Styles/rockumentation.css" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowBooks();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the books.
        /// </summary>
        protected void ShowBooks()
        {
            using ( var rockContext = new RockContext() )
            {
                var books = new DocumentationBookService( rockContext ).Queryable()
                    .OrderBy( a => a.Title )
                    .ToList()
                    .Where( a => a.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    .Select( a =>
                    {
                        var cachedBook = new CachedBook();
                        cachedBook.SetFromEntity( a );

                        return cachedBook;
                    } )
                    .Where( a => a.LatestPublishedVersion != null )
                    .Select( a =>
                    {
                        var viewBookPageCache = PageCache.Get( ViewBookPage ?? Guid.Empty );
                        if ( viewBookPageCache != null )
                        {
                            a.Url = Utility.BuildArticleRoute( viewBookPageCache, a.LatestPublishedVersion.MainArticle, "/" );
                        }
                        else
                        {
                            a.Url = "#";
                        }

                        return a;
                    } )
                    .ToList();

                var mergeFields = LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                mergeFields.Add( "Books", books );

                ltContent.Text = LavaTemplate.ResolveMergeFields( mergeFields, CurrentPerson );
            }
        }

        #endregion

        #region Events Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowBooks();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Helper class for Lava so we provide a valid URL for them to use.
        /// </summary>
        /// <seealso cref="com.blueboxmoon.Rockumentation.Cache.DocumentationBookCache" />
        private class CachedBook : DocumentationBookCache
        {
            public string Url { get; set; }
        }

        #endregion
    }
}
