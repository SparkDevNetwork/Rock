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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a block that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class BlockCache : ModelCache<BlockCache, Block>
    {
        #region Fields

        private List<EntityTypeCache> _contextTypesRequired;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Page"/> that this Block is implemented on. This property will only be populated
        /// if the Block is implemented on a <see cref="Rock.Model.Page"/>.
        /// Blocks that have a specific PageId will only be shown in the specified Page
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Page"/> that this Block is implemented on.  This value will be null if this Block is implemented 
        /// as part of a <see cref="Rock.Model.Layout"/> or <see cref="Rock.Model.Site"/>.
        /// </value>
        [DataMember]
        public int? PageId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Layout"/> that this Block is implemented on. This property will only be populated
        /// if the Block is implemented on a <see cref="Rock.Model.Layout"/>.
        /// Blocks that have a specific LayoutId will be shown on all pages on a site that have the specified LayoutId
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Layout"/> that this Block is implemented on.  This value will be null if this Block is implemented 
        /// as part of a <see cref="Rock.Model.Page"/> or <see cref="Rock.Model.Site"/>.
        /// </value>
        [DataMember]
        public int? LayoutId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Site"/> that this Block is implemented on. This property will only be populated
        /// if the Block is implemented on a <see cref="Rock.Model.Site"/>.
        /// Blocks that have a specific SiteId will be shown on all pages on a site
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Site"/> that this Block is implemented on.  This value will be null if this Block is implemented 
        /// as part of a <see cref="Rock.Model.Page"/> or <see cref="Rock.Model.Layout"/> .
        /// </value>
        [DataMember]
        public int? SiteId { get; private set; }

        /// <summary>
        /// Gets or sets the block type id.
        /// </summary>
        /// <value>
        /// The block type id.
        /// </value>
        [DataMember]
        public int BlockTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        [DataMember]
        public string Zone { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        [DataMember]
        public string CssClass { get; private set; }

        /// <summary>
        /// Gets the HTML to be rendered before the block
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        [DataMember]
        public string PreHtml { get; private set; }

        /// <summary>
        /// Gets the HTML to be rendered after the block
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        [DataMember]
        public string PostHtml { get; private set; }

        /// <summary>
        /// Gets or sets the duration of the output cache.
        /// </summary>
        /// <value>
        /// The duration of the output cache.
        /// </value>
        [DataMember]
        public int OutputCacheDuration { get; private set; }

        /// <summary>
        /// Gets the additional settings.
        /// </summary>
        /// <value>
        /// The additional settings.
        /// </value>
        public string AdditionalSettings { get; private set; }

        /// <summary>
        /// Gets or sets the Page that this Block is implemented on. This 
        /// property will be null if this Block is being implemented on as part of a Layout or Site
        /// </summary>
        /// <value>
        /// The Page that this Block is being implemented on. This value will 
        /// be null if the Block is implemented as part of a Layout or Site
        /// </value>
        public PageCache Page => PageId.HasValue ? PageCache.Get( PageId.Value ) : null;

        /// <summary>
        /// Gets or sets the Layout that this Block is implemented on. This 
        /// property will be null if this Block is being implemented on as part of a Page or Site
        /// </summary>
        /// <value>
        /// The Layout that this Block is being implemented on. This value will 
        /// be null if the Block is implemented as part of a Page or Site
        /// </value>
        public LayoutCache Layout
        {
            get
            {
                if ( LayoutId != null && LayoutId.Value != 0 )
                {
                    return LayoutCache.Get( LayoutId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the Site that this Block is implemented on. This 
        /// property will be null if this Block is being implemented on as part of a Page or Layout
        /// </summary>
        /// <value>
        /// The Site that this Block is being implemented on. This value will 
        /// be null if the Block is implemented as part of a Page or Layout
        /// </value>
        public SiteCache Site
        {
            get
            {
                if ( SiteId != null && SiteId.Value != 0 )
                {
                    return SiteCache.Get( SiteId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the block type
        /// </summary>
        public BlockTypeCache BlockType => BlockTypeCache.Get( BlockTypeId );

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                switch ( BlockLocation )
                {
                    case BlockLocation.Page:
                        return Page ?? base.ParentAuthority;
                    case BlockLocation.Layout:
                        return Layout ?? base.ParentAuthority;
                    case BlockLocation.Site:
                        return Site ?? base.ParentAuthority;
                    default:
                        return base.ParentAuthority;
                }
            }
        }

        /// <summary>
        /// Gets the block location.
        /// </summary>
        /// <value>
        /// The block location.
        /// </value>
        public virtual BlockLocation BlockLocation
        {
            get
            {
                if ( PageId.HasValue )
                {
                    return BlockLocation.Page;
                }

                if ( LayoutId.HasValue )
                {
                    return BlockLocation.Layout;
                }

                if ( SiteId.HasValue )
                {
                    return BlockLocation.Site;
                }

                return BlockLocation.None;
            }
        }

        /// <summary>
        /// Gets a list of any context entities that the block requires.
        /// </summary>
        public List<EntityTypeCache> ContextTypesRequired
        {
            get
            {
                if ( _contextTypesRequired == null )
                {
                    _contextTypesRequired = new List<EntityTypeCache>();

                    int properties = 0;
                    foreach ( var attribute in this.BlockType.GetCompiledType().GetCustomAttributes( typeof( ContextAwareAttribute ), true ) )
                    {
                        var contextAttribute = ( ContextAwareAttribute ) attribute;

                        if ( !contextAttribute.Contexts.Any() )
                        {
                            // If the entity type was not specified in the attribute, look for a property that defines it
                            string propertyKeyName = string.Format( "ContextEntityType{0}", properties > 0 ? properties.ToString() : string.Empty );
                            properties++;

                            Guid guid = Guid.Empty;
                            if ( Guid.TryParse( this.GetAttributeValue( propertyKeyName ), out guid ) )
                            {
                                _contextTypesRequired.Add( EntityTypeCache.Get( guid ) );
                            }
                        }
                        else
                        {
                            foreach ( var context in contextAttribute.Contexts )
                            {
                                var entityType = context.EntityType;

                                if ( entityType != null && !_contextTypesRequired.Any( e => e.Guid.Equals( entityType.Guid ) ) )
                                {
                                    _contextTypesRequired.Add( entityType );
                                }
                            }
                        }
                    }
                }

                return _contextTypesRequired;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var block = entity as Block;
            if ( block == null ) return;

            IsSystem = block.IsSystem;
            PageId = block.PageId;
            LayoutId = block.LayoutId;
            SiteId = block.SiteId;
            BlockTypeId = block.BlockTypeId;
            Zone = block.Zone;
            Order = block.Order;
            Name = block.Name;
            CssClass = block.CssClass;
            PreHtml = block.PreHtml;
            PostHtml = block.PostHtml;
            OutputCacheDuration = block.OutputCacheDuration;
            AdditionalSettings = block.AdditionalSettings;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}