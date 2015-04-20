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
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    internal class RockBlockWrapper : CompositeControl
    {
        #region Private Fields

        private RockBlock _rockBlock = null;
        private List<Control> _adminControls = new List<Control>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RockBlockWrapper"/> class.
        /// </summary>
        /// <param name="rockBlock">The rock block.</param>
        internal RockBlockWrapper ( RockBlock rockBlock )
        {
            _rockBlock = rockBlock;
            _adminControls = rockBlock.GetAdministrateControls( _rockBlock.UserCanAdministrate, _rockBlock.UserCanEdit );
        }

        /// <summary>
        /// Ensures the block controls.
        /// </summary>
        public void EnsureBlockControls()
        {
            base.EnsureChildControls();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            foreach ( Control configControl in _adminControls )
            {
                configControl.ClientIDMode = ClientIDMode.AutoID;
                Controls.Add( configControl );
            }

            Controls.Add( _rockBlock );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            var blockCache = _rockBlock.BlockCache;

            string preHtml = string.Empty;
            string postHtml = string.Empty;
            string appRoot = _rockBlock.ResolveRockUrl( "~/" );
            string themeRoot = _rockBlock.ResolveRockUrl( "~~/" );

            if ( _rockBlock.Visible )
            {
                if ( !string.IsNullOrWhiteSpace( blockCache.PreHtml ) )
                {
                    preHtml = blockCache.PreHtml.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );
                }

                if ( !string.IsNullOrWhiteSpace( blockCache.PostHtml ) )
                {
                    postHtml = blockCache.PostHtml.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );
                }

                if ( preHtml.HasMergeFields() || postHtml.HasMergeFields() )
                {
                    var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( _rockBlock.CurrentPerson );
                    mergeFields.Add( "CurrentPerson", _rockBlock.CurrentPerson );
                    mergeFields.Add( "Campuses", CampusCache.All() );
                    mergeFields.Add( "PageParameter", _rockBlock.PageParameters() );

                    var contextObjects = new Dictionary<string, object>();
                    foreach ( var contextEntityType in _rockBlock.RockPage.GetContextEntityTypes() )
                    {
                        var contextEntity = _rockBlock.RockPage.GetCurrentContext( contextEntityType );
                        if ( contextEntity != null && contextEntity is DotLiquid.ILiquidizable )
                        {
                            var type = Type.GetType( contextEntityType.AssemblyName ?? contextEntityType.Name );
                            if ( type != null )
                            {
                                contextObjects.Add( type.Name, contextEntity );
                            }
                        }
                    }

                    if ( contextObjects.Any() )
                    {
                        mergeFields.Add( "Context", contextObjects );
                    }

                    preHtml = preHtml.ResolveMergeFields( mergeFields );
                    postHtml = postHtml.ResolveMergeFields( mergeFields );
                }
            }

            StringBuilder sbOutput = null;
            StringWriter swOutput = null;
            HtmlTextWriter twOutput = null;

            if ( _rockBlock.BlockCache.OutputCacheDuration > 0 )
            {
                sbOutput = new StringBuilder();
                swOutput = new StringWriter( sbOutput );
                twOutput = new HtmlTextWriter( swOutput );
            }

            // Create block wrapper
            string blockTypeCss = blockCache.BlockType != null ? blockCache.BlockType.Name : "";
            var parts = blockTypeCss.Split( new char[] { '>' } );
            if ( parts.Length > 1 )
            {
                blockTypeCss = parts[parts.Length - 1].Trim();
            }
            blockTypeCss = blockTypeCss.Replace( ' ', '-' ).ToLower();
            string blockInstanceCss = "block-instance " +
                blockTypeCss +
                ( string.IsNullOrWhiteSpace( blockCache.CssClass ) ? "" : " " + blockCache.CssClass.Trim() ) +
                ( _rockBlock.UserCanEdit || _rockBlock.UserCanAdministrate ? " can-configure " : "" );

            writer.Write( preHtml );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, string.Format( "bid_{0}", blockCache.Id ) );
            writer.AddAttribute( "data-zone-location", blockCache.BlockLocation.ToString() );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, blockInstanceCss );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "block-content" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( blockCache.OutputCacheDuration > 0 )
            {
                twOutput.Write( preHtml );
                twOutput.AddAttribute( HtmlTextWriterAttribute.Id, string.Format( "bid_{0}", blockCache.Id ) );
                twOutput.AddAttribute( "data-zone-location", blockCache.BlockLocation.ToString() );
                twOutput.AddAttribute( HtmlTextWriterAttribute.Class, blockInstanceCss );
                twOutput.RenderBeginTag( HtmlTextWriterTag.Div );

                twOutput.AddAttribute( HtmlTextWriterAttribute.Class, "block-content" );
                twOutput.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            if ( _rockBlock.PageCache.IncludeAdminFooter && ( _rockBlock.UserCanAdministrate || _rockBlock.UserCanEdit ) )
            {
                // Add the config buttons
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "block-configuration config-bar" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-arrow-circle-right" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "block-configuration-bar" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( string.IsNullOrWhiteSpace( blockCache.Name ) ? blockCache.BlockType.Name : blockCache.Name );
                writer.RenderEndTag();

                foreach ( Control configControl in _adminControls )
                {
                    configControl.RenderControl( writer );
                }

                writer.RenderEndTag();  // block-configuration-bar
                writer.RenderEndTag();  // config-bar
            }

            _rockBlock.RenderControl( writer );

            writer.RenderEndTag();  // block-content
            writer.RenderEndTag();  // block-instance
            writer.Write( postHtml );

            if ( blockCache.OutputCacheDuration > 0 )
            {
                base.Render( twOutput );

                twOutput.RenderEndTag();  // block-content
                twOutput.RenderEndTag();  // block-instance
                twOutput.Write( postHtml );

                CacheItemPolicy cacheDuration = new CacheItemPolicy();
                cacheDuration.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( blockCache.OutputCacheDuration );

                ObjectCache cache = RockMemoryCache.Default;
                string _blockCacheKey = string.Format( "Rock:BlockOutput:{0}", blockCache.Id );
                cache.Set( _blockCacheKey, sbOutput.ToString(), cacheDuration );
            }
        }
    }

}
