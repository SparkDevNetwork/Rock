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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using RestSharp;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.VersionInfo;
using System.Runtime.Caching;

namespace RockWeb.Plugins.com_mineCartStudio.Cms
{
    /// <summary>
    /// Block that syncs selected people to an exchange server.
    /// </summary>
    [DisplayName( "Rock Book List" )]
    [Category( "Mine Cart Studio > CMS" )]
    [Description( "Block that lists the Rock Documentation using a Lava Template." )]

    [CustomCheckboxListField( "Categories", "Documentation categories to include.", "136^Getting Started,152^User Guides,153^Administration", true, order: 0 )]
    [TextField("Rock Version", "Rock version to show. Leave blank to have the block use the current running version of Rock.", false, order: 1)]
    [CodeEditorField("Lava Template", "Lava template to use for the layout", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, true, @"<div class='panel panel-block'>
    <div class='panel-heading'>
        <h1 class='panel-title'>
            <i class='fa fa-book'></i> Rock Documentation
        </h1>
    </div>

    <div class='panel-body'>
        {% assign currentCategoryId = -1 %}
        
        {% for book in Books %}
            {% if currentCategoryId != book.CategoryId %}
                
                {% if currentCategoryId != -1 %}
                    </div>
                {% endif %}
                
                <h2>{{ book.Category }}</h2>
                {% assign currentCategoryId = book.CategoryId %}
                
                <div class='row'>
            {% endif %}
            
            <div class='col-md-3 margin-b-lg'>
                <a href='http://www.rockrms.com/Rock/BookContent/{{ book.Id }}/{{ book.VersionId }}'>
                    <img src='http://www.rockrms.com/GetImage.ashx?id={{ book.CoverImageBinaryFileId }}&width=253&height=327' class='img-thumbnail' style='width: 100%' />
                </a>
            </div>
        {%  endfor %}
    </div>
</div>", order: 2)]
    [TextField( "Excluded Book Ids", "A comma delimited list of book ids to exclude.", false, order: 3 )]
    [BooleanField( "Enable Debug", "Show the lava merge fields.", order: 4 )]
    public partial class RockBookList : Rock.Web.UI.RockBlock
    {

        #region Fields

        private readonly string _rockServer = "http://www.rockrms.com";
        private readonly string BOOKS_CACHE_KEY = "RockBooks";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            lMessages.Text = string.Empty;

            if ( !Page.IsPostBack )
            {
                LoadBooks();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            FlushCacheItem( BOOKS_CACHE_KEY );
            LoadBooks();
        }
        #endregion


        #region Methods

        /// <summary>
        /// Loads the books.
        /// </summary>
        private void LoadBooks()
        {
            // look for books in cache
            var books = GetCacheItem( BOOKS_CACHE_KEY ) as List<BookResult>;

            if ( books != null )
            {
                DisplayBooks( books );
            }
            else
            {
                var client = new RestClient( _rockServer );
                client.Timeout = 12000;

                string version = GetAttributeValue( "RockVersion" );

                if ( string.IsNullOrWhiteSpace( version ) )
                {
                    version = VersionInfo.GetRockSemanticVersionNumber();

                    // get major release
                    version = version.Remove( version.Length - 1, 1 ) + "0";
                }

                string categories = GetAttributeValue( "Categories" );

                if ( !string.IsNullOrWhiteSpace( categories ) )
                {
                    string requestUrl = string.Format( "/api/Books/GetBooksByVersion/{0}/{1}", categories, version );

                    var request = new RestRequest( requestUrl, Method.GET );

                    var response = client.Execute<List<BookResult>>( request );

                    if ( response.ResponseStatus == ResponseStatus.Completed )
                    {
                        // cache the result
                        var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddDays( 1 ) };
                        AddCacheItem( BOOKS_CACHE_KEY, response.Data, cacheItemPolicy );

                        DisplayBooks( response.Data );
                    }
                    else
                    {
                        lMessages.Text = string.Format( "<div class='alert alert-warning'>Error in connecting to the Rock server. {0}", response.ErrorMessage );
                    }
                }
                else
                {
                    lMessages.Text = string.Format( "<div class='alert alert-warning'>No categories selected for display." );
                }
            }
        }

        /// <summary>
        /// Displays the books.
        /// </summary>
        /// <param name="books">The books.</param>
        private void DisplayBooks( List<BookResult> books )
        {
            // removed excluded books
            List<int> excludedBookIds = new List<int>();
            string excludedBookList = GetAttributeValue( "ExcludedBookIds" );

            if ( excludedBookList != null )
            {
                int mos = 0;
                excludedBookIds = excludedBookList.Split( ',' )
                    .Select( m => { int.TryParse( m, out mos ); return mos; } )
                    .Where( m => mos != 0 )
                    .ToList();
            }

            var bookList = books.Where( b => !excludedBookIds.Contains( b.Id ) );

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Books", bookList );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lContent.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
            else
            {
                lDebug.Visible = false;
                lDebug.Text = string.Empty;
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [DotLiquid.LiquidType( "Id", "VersionId", "Name", "Category", "CategoryId", "Subtitle", "CoverImageBinaryFileId", "Order", "IsActive" )]
    public class BookResult
    {

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the version identifier.
        /// </summary>
        /// <value>
        /// The version identifier.
        /// </value>
        public int VersionId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the subtitle.
        /// </summary>
        /// <value>
        /// The subtitle.
        /// </value>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the cover image binary file identifier.
        /// </summary>
        /// <value>
        /// The cover image binary file identifier.
        /// </value>
        public int? CoverImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }
    }
}