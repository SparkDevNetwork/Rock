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
using System.Collections.Concurrent;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a block type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheBlockType instead" )]
    public class BlockTypeCache : CachedModel<BlockType>
    {

        #region Constructors

        private BlockTypeCache()
        {
        }

        private BlockTypeCache( CacheBlockType cacheBlockType )
        {
            CopyFromNewCache( cacheBlockType );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this blocktype is commonly used
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is common; otherwise, <c>false</c>.
        /// </value>
        public bool IsCommon { get; private set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Rock.Attribute.TextFieldAttribute" /> attributes have been
        /// verified for the block type.  If not, Rock will create and/or update the attributes associated with the block.
        /// </summary>
        /// <value>
        /// <c>true</c> if attributes have already been verified; otherwise, <c>false</c>.
        /// </value>
        public bool IsInstancePropertiesVerified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [checked security actions].
        /// </summary>
        /// <value>
        /// <c>true</c> if [checked security actions]; otherwise, <c>false</c>.
        /// </value>
        public bool CheckedSecurityActions { get; set; }

        /// <summary>
        /// Gets or sets the security actions that were defined by a SecurityActionAttribute on the block type
        /// </summary>
        /// <value>
        /// The security actions.
        /// </value>
        public ConcurrentDictionary<string, string> SecurityActions { get; set; }


        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the security actions.
        /// </summary>
        /// <param name="blockControl">The block control.</param>
        public void SetSecurityActions( UI.RockBlock blockControl )
        {
            lock ( _obj )
            {
                if ( CheckedSecurityActions ) return;

                SecurityActions = new ConcurrentDictionary<string, string>();
                foreach ( var action in blockControl.GetSecurityActionAttributes() )
                {
                    SecurityActions.TryAdd( action.Key, action.Value );
                }
                CheckedSecurityActions = true;
            }
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is BlockType ) ) return;

            var blockType = (BlockType)model;
            IsSystem = blockType.IsSystem;
            IsCommon = blockType.IsCommon;
            Path = blockType.Path;
            Name = blockType.Name;
            Description = blockType.Description;
            IsInstancePropertiesVerified = false;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            var blockType = (CacheBlockType)cacheEntity;

            IsSystem = blockType.IsSystem;
            IsCommon = blockType.IsCommon;
            Path = blockType.Path;
            Name = blockType.Name;
            Description = blockType.Description;
            IsInstancePropertiesVerified = blockType.IsInstancePropertiesVerified;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns Block Type object from cache.  If block does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static BlockTypeCache Read( int id, RockContext rockContext = null )
        {
            return new BlockTypeCache( CacheBlockType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static BlockTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new BlockTypeCache( CacheBlockType.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified block type model.
        /// </summary>
        /// <param name="blockTypeModel">The block type model.</param>
        /// <returns></returns>
        public static BlockTypeCache Read( BlockType blockTypeModel )
        {
            return new BlockTypeCache( CacheBlockType.Get( blockTypeModel ) );
        }

        /// <summary>
        /// Removes block type from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheBlockType.Remove( id );
        }

        #endregion
    }
}