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
using Rock.Security;

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
        /// Gets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; private set; }

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
        /// Provides ability to log into the site.
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

        private ConcurrentDictionary<string, string> _securityActions = null;

        /// <summary>
        /// Gets the security action attributes for the <seealso cref="SecurityActionAttribute">SecurityAction</seealso> attributes on this <seealso cref="System.Type"/> of this block
        /// </summary>
        /// <returns></returns>
        private ConcurrentDictionary<string, string> GetSecurityActionAttributes()
        {
            var blockType = this.GetCompiledType();

            if ( blockType == null )
            {
                return new ConcurrentDictionary<string, string>();
            }

            var securityActions = new ConcurrentDictionary<string, string>();
            object[] customAttributes = blockType.GetCustomAttributes( typeof( SecurityActionAttribute ), true );
            foreach ( var customAttribute in customAttributes )
            {
                var securityActionAttribute = customAttribute as SecurityActionAttribute;
                if ( securityActionAttribute != null )
                {
                    securityActions.TryAdd( securityActionAttribute.Action, securityActionAttribute.Description );
                }
            }

            return securityActions;
        }

        /// <summary>
        /// Gets the security actions.
        /// </summary>
        /// <value>
        /// The security actions.
        /// </value>
        [DataMember]
        public ConcurrentDictionary<string, string> SecurityActions
        {
            get
            {
                /* MDP 2020-03-17
                This was changed to Load On Demand instead of getting set by RockPage.
                This was done because nothing in core was using SecurityActions, and because loading them without needing them causes unneccessary overhead
                 */

                if ( _securityActions == null )
                {
                    _securityActions = GetSecurityActionAttributes();
                }

                return _securityActions;
            }
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType => EntityTypeId.HasValue ? EntityTypeCache.Get( EntityTypeId.Value ) : null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var blockType = entity as BlockType;
            if ( blockType == null )
            {
                return;
            }

            IsSystem = blockType.IsSystem;
            IsCommon = blockType.IsCommon;
            Path = blockType.Path;
            EntityTypeId = blockType.EntityTypeId;
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

            //
            // This method is only necessary if there is an associated file on disk.
            //
            if ( string.IsNullOrWhiteSpace( Path ) )
            {
                return;
            }

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
            if ( !fileinfo.Exists )
            {
                return;
            }

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
            UpdateCacheItem( this.Id.ToString(), this );
        }

        /// <summary>
        /// Gets the compiled type of this block type. If this is a legacy ASCX block then it
        /// is dynamically compiled (and cached), otherwise a lookup is done via Entity Type.
        /// </summary>
        /// <returns>A Type that represents the logic class of this block type.</returns>
        public Type GetCompiledType()
        {
            if ( !string.IsNullOrWhiteSpace( this.Path ) )
            {
                try
                {
                    return System.Web.Compilation.BuildManager.GetCompiledType( Path );
                }
                catch ( Exception ex )
                {
                    // Added some diagnostics to record where this code is being called from
                    // since our current exceptions are missing this detail.
                    var stackTrace = new System.Diagnostics.StackTrace( true );
                    Logging.RockLogger.Log.Debug( Logging.RockLogDomains.Other, $"Path: {Path}" + System.Environment.NewLine + stackTrace.ToString() );
                    ExceptionLogService.LogException( ex );
                    return null;
                }

            }
            else if ( EntityTypeId.HasValue )
            {
                try
                {
                    return EntityType.GetEntityType();
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    return null;
                }

            }

            return null;
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