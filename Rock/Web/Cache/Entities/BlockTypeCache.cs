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
using System.IO;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a block type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class BlockTypeCache : ModelCache<BlockTypeCache, BlockType>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is common.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is common; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCommon { get; private set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        [DataMember]
        public string Path { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the user defined description of the BlockType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Description of the BlockType
        /// </value>
        /// <example>
        /// Provides ability to login to site.
        /// </example>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the category of the BlockType.  Blocks will be grouped by category when displayed to user
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the category of the BlockType.
        /// </value>
        /// <example>
        /// Security
        /// </example>
        [DataMember]
        public string Category { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the  attributes have been
        /// verified for the block type.  If not, Rock will create and/or update the attributes associated with the block.
        /// </summary>
        /// <value>
        /// <c>true</c> if attributes have already been verified; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsInstancePropertiesVerified { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [checked security actions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [checked security actions]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CheckedSecurityActions { get; private set; }

        /// <summary>
        /// Gets or sets the security actions.
        /// </summary>
        /// <value>
        /// The security actions.
        /// </value>
        [DataMember]
        public ConcurrentDictionary<string, string> SecurityActions { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the security actions.
        /// </summary>
        /// <param name="blockControl">The block control.</param>
        public void SetSecurityActions( Web.UI.RockBlock blockControl )
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
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var blockType = entity as BlockType;
            if ( blockType == null ) return;

            IsSystem = blockType.IsSystem;
            IsCommon = blockType.IsCommon;
            Path = blockType.Path;
            Name = blockType.Name;
            Description = blockType.Description;
            Category = blockType.Category;
            IsInstancePropertiesVerified = false;
        }

        /// <summary>
        /// Method that is called by the framework immediately after being added to cache
        /// </summary>
        public override void PostCached()
        {
            base.PostCached();

            string physicalPath;

            // This will add a file system watcher so that when the block on the file system changes, this 
            // object will be removed from cache. This is to force the cmsPage object to revalidate any
            // BlockPropery attributes that may have been added or modified.
            if ( System.Web.HttpContext.Current != null )
            {
                physicalPath = System.Web.HttpContext.Current.Request.MapPath( Path );
            }
            else
            {
                physicalPath = System.Web.Hosting.HostingEnvironment.MapPath( Path );
            }

            var fileinfo = new FileInfo( physicalPath );
            if ( !fileinfo.Exists ) return;

            // Create a new FileSystemWatcher and set its properties.
            var watcher = new FileSystemWatcher
            {
                Path = fileinfo.DirectoryName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = fileinfo.Name + ".*"
            };

            // Add event handlers.
            watcher.Changed += FileSystemWatcher_OnChanged;
            watcher.Deleted += FileSystemWatcher_OnChanged;
            watcher.Renamed += FileSystemWatcher_OnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Updates the Is Instance Properties Verified flag
        /// </summary>
        public void MarkInstancePropertiesVerified( bool verified )
        {
            IsInstancePropertiesVerified = verified;
            UpdateCacheItem( this.Id.ToString(), this, TimeSpan.MaxValue );
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

        #region File Watcher Events

        private void FileSystemWatcher_OnRenamed( object sender, RenamedEventArgs renamedEventArgs )
        {
            FlushItem( Id );
        }

        private void FileSystemWatcher_OnChanged( object sender, FileSystemEventArgs fileSystemEventArgs )
        {
            FlushItem( Id );
        }

        #endregion

    }
}