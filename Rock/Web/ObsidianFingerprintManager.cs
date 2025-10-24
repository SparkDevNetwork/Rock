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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Configuration;

using Rock;
using Rock.Configuration;

namespace Rock.Web
{
    /// <summary>
    /// Manages automatic fingerprinting of Obsidian files so we can update
    /// the URL query string to bust browser caches when files change. This
    /// is a sledghammer approach that simply uses the last write time of
    /// any obsidian file as the fingerprint.
    /// </summary>
    internal class ObsidianFingerprintManager
    {
        #region Fields

        /// <summary>
        /// The fingerprint to use with obsidian files.
        /// </summary>
        private long _fingerprint = 0;

        /// <summary>
        /// The obsidian file watchers.
        /// </summary>
        private readonly List<FileSystemWatcher> _fileWatchers = new List<FileSystemWatcher>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObsidianFingerprintManager"/> class.
        /// Sets up the initial fingerprint and file system watchers if in debug mode.
        /// </summary>
        public ObsidianFingerprintManager()
        {
            InitializeFingerprint();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current fingerprint value as a string.
        /// </summary>
        /// <returns>A string representation of the current fingerprint.</returns>
        public virtual string GetFingerprint()
        {
            return _fingerprint.ToString();
        }

        /// <summary>
        /// Initializes the obsidian file fingerprint. This sets the initial
        /// fingerprint value and then if we are in Debug mode it monitors for
        /// any file system changes related to Obsidian and updates the
        /// fingerprint used when loading files to bust cache.
        /// </summary>
        private void InitializeFingerprint()
        {
            // Do everything in a try/catch because this is called from the
            // static initializer, meaning if something goes wrong Rock will
            // fail to start.
            try
            {
                var obsidianPath = RockApp.Current.MapPath( "~/Obsidian" );
                var pluginsPath = RockApp.Current.MapPath( "~/Plugins" );
                var now = RockDateTime.Now;

                // Find the last date any obsidian file was modified.
                var lastWriteTime = Directory.EnumerateFiles( obsidianPath, "*.js", SearchOption.AllDirectories )
                    .Union( Directory.EnumerateFiles( pluginsPath, "*.js", SearchOption.AllDirectories ) )
                    .Select( f =>
                    {
                        try
                        {
                            return ( DateTime? ) new FileInfo( f ).LastWriteTime;
                        }
                        catch
                        {
                            return null;
                        }
                    } )
                    .Where( d => d.HasValue )
                    .Select( d => ( DateTime? ) RockDateTime.ConvertLocalDateTimeToRockDateTime( d.Value ) )
                    // This is an attempt to fix random issues where people have the
                    // JS file cached in the browser. A theory is that some JS file
                    // has a future date time, so even after an upgrade the same
                    // fingerprint value is used. Ignore any dates in the future.
                    .Where( d => d < now )
                    .OrderByDescending( d => d )
                    .FirstOrDefault();

                _fingerprint = ( lastWriteTime ?? now ).Ticks;

#if WEBFORMS
                // Check if we are in debug mode and if so enable the watchers.
                var cfg = ( CompilationSection ) System.Configuration.ConfigurationManager.GetSection( "system.web/compilation" );
                if ( cfg != null && cfg.Debug )
#endif
                {
                    AddFileSystemWatcher( obsidianPath, "*.js" );
                    AddFileSystemWatcher( pluginsPath, "*.js" );
                }
            }
            catch ( Exception ex )
            {
                _fingerprint = RockDateTime.Now.Ticks;
                Debug.WriteLine( ex.Message );
            }
        }

        /// <summary>
        /// Add a new file system watcher for the specified <paramref name="directory"/>.
        /// It will update the fingerprint whenever a file matching the
        /// <paramref name="filter"/> changes.
        /// </summary>
        /// <param name="directory">The directory, and any sub-directories, to watch.</param>
        /// <param name="filter">The filename filter to use when watching for changes.</param>
        private void AddFileSystemWatcher( string directory, string filter )
        {
            // Setup a watcher to notify us of any changes to the directory.
            var watcher = new FileSystemWatcher
            {
                Path = directory,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = filter
            };

            // Add event handlers.
            watcher.Changed += FileSystemWatcher_OnChanged;
            watcher.Created += FileSystemWatcher_OnChanged;
            watcher.Renamed += FileSystemWatcher_OnRenamed;

            _fileWatchers.Add( watcher );

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Handles the OnRenamed event of the Obsidian FileSystemWatcher.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="renamedEventArgs">The <see cref="RenamedEventArgs"/> instance containing the event data.</param>
        private void FileSystemWatcher_OnRenamed( object sender, RenamedEventArgs renamedEventArgs )
        {
            try
            {
                var dateTime = new FileInfo( renamedEventArgs.FullPath ).LastWriteTime;

                dateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( dateTime );

                _fingerprint = Math.Max( _fingerprint, dateTime.Ticks );
            }
            catch
            {
                _fingerprint = RockDateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// Handles the OnChanged event of the Obsidian FileSystemWatcher.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="fileSystemEventArgs">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void FileSystemWatcher_OnChanged( object sender, FileSystemEventArgs fileSystemEventArgs )
        {
            try
            {
                var dateTime = new FileInfo( fileSystemEventArgs.FullPath ).LastWriteTime;

                dateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( dateTime );

                _fingerprint = Math.Max( _fingerprint, dateTime.Ticks );
            }
            catch
            {
                _fingerprint = RockDateTime.Now.Ticks;
            }
        }

        #endregion
    }
}
