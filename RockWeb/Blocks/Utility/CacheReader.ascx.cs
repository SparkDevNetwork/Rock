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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Utility
{
    [DisplayName( "Cache Reader" )]
    [Category( "Utility" )]
    [Description( "Shows information about what's being cached in Rock." )]

    #region Block Attributes

    [BooleanField(
        "Show Email Address",
        Key = AttributeKey.ShowEmailAddress,
        Description = "Should the email address be shown?",
        DefaultBooleanValue = true,
        Order = 1 )]

    [EmailField(
        "Email",
        Key = AttributeKey.Email,
        Description = "The Email address to show.",
        DefaultValue = "ted@rocksolidchurchdemo.com",
        Order = 2 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "B2859CA9-F796-4D83-A83B-62AA44FC6BC5" )]
    public partial class CacheReader : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowEmailAddress = "ShowEmailAddress";
            public const string Email = "Email";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string StarkId = "StarkId";
        }

        #endregion PageParameterKeys

        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
            if ( !Page.IsPostBack )
            {
                GetCacheData();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        private void GetCacheData()
        {

            var totalCacheSize = 0;

            // Determine the size of the object cache
            var objectCacheKeys = RockCache.ObjectConcurrentCacheKeyReferences;

            var objectCacheSize = 0;
            foreach( var objectCacheKeyItem in objectCacheKeys )
            {
                var objectCacheKey = objectCacheKeyItem.Value;
                var cacheItem = RockCache.Get( objectCacheKey.Key, objectCacheKey.Region );
                if ( cacheItem != null )
                {
                    objectCacheSize += cacheItem.ToJson().Length;
                }
            }

            totalCacheSize += objectCacheSize;

            // Output object cache size
            lOutput.Text = string.Format( "Object Cache: {0:n0} KB", objectCacheSize / 1024 );

            // Determine the size of the string cache
            var stringCacheKeys = RockCache.StringConcurrentCacheKeyReferences;

            var stringCacheSize = 0;

            foreach ( var stringCacheKeyItem in stringCacheKeys )
            {
                var stringCacheKey = stringCacheKeyItem.Value;

                // Cache tags are in an empty region. Calls without a region throw an exception
                if ( stringCacheKey.Region.IsNotNullOrWhiteSpace() )
                {
                    var value = RockCacheManager<List<string>>.Instance.Get( stringCacheKey.Key, stringCacheKey.Region );

                    if ( value != null )
                    {
                        stringCacheSize += value.ToJson().Length;
                    }
                }
                else
                {
                    var cacheItem = RockCache.Get( stringCacheKey.Key, stringCacheKey.Region );
                    if ( cacheItem != null )
                    {
                        stringCacheSize += cacheItem.ToJson().Length;
                    }
                }
            }

            totalCacheSize += stringCacheSize;

            // Output object cache size
            lOutput.Text += string.Format( "<br>String Cache: {0:n0} KB", stringCacheSize / 1024 );


            lOutput.Text += "<hr>";
            lOutput.Text += "<h4>Model Cache Sizes</h4>";

            // Get type for the generic model type cache
            var cacheModelType = typeof( ModelCache<,> );

            // Get a listing of all cached models
            var cachedModels = RockCache.GetAllModelCacheTypes().OrderBy( a => a.Name ).ToList();

            foreach( var cachedModel in cachedModels )
            {
                //Type rockCacheManagerType = typeof( RockCacheManager<> ).MakeGenericType( new Type[] { cachedModel } );

                if ( cachedModel.BaseType.Name == cacheModelType.Name )
                {
                    // this is an entity cache model
                    try
                    {
                        var methodInfo = cachedModel.BaseType.BaseType.GetMethods().Where( m => m.Name == "All" ).Where( m => m.GetParameters().Length == 0 ).FirstOrDefault();
                        if ( methodInfo != null )
                        {
                            var length = methodInfo.Invoke( null, null ).ToJson().Length;
                            lOutput.Text += string.Format( "<br>{0} - {1:n0} KB", cachedModel.Name, length / 1024 );

                            totalCacheSize += length;
                        }
                    }
                    catch ( Exception ) { };
                }
            }

            lOutput.Text += string.Format( "<p><p><strong>Total Cache Size: {0:n0} KB</strong>", totalCacheSize / 1024 );
        }

        #endregion


    }
}