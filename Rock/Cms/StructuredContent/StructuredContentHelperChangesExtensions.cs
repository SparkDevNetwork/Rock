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
using System.Reflection;

using Rock.Data;
using Rock.Model;

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// Extension methods for <see cref="StructuredContentHelper"/> that provides
    /// additional methods related to changes (and require database access).
    /// </summary>
    public static class StructuredContentHelperChangesExtensions
    {
        /// <summary>
        /// The change handlers to be used by this class.
        /// </summary>
        private static IReadOnlyDictionary<string, IReadOnlyList<IStructuredContentBlockChangeHandler>> _changeHandlers;

        /// <summary>
        /// Gets the block change handlers used for change detection.
        /// </summary>
        /// <returns>A dictionary of <see cref="IStructuredContentBlockChangeHandler"/> instances.</returns>
        private static IReadOnlyDictionary<string, IReadOnlyList<IStructuredContentBlockChangeHandler>> GetBlockChangeHandlers()
        {
            if ( _changeHandlers == null )
            {
                var blockChangesTypes = Reflection.FindTypes( typeof( IStructuredContentBlockChangeHandler ) )
                    .Where( a => a.Value.GetCustomAttribute<StructuredContentBlockAttribute>() != null )
                    .OrderBy( a => a.Value.Assembly == typeof( BlockTypes.ParagraphRenderer ).Assembly )
                    .Select( a => a.Value );

                var changeHandlers = new Dictionary<string, IReadOnlyList<IStructuredContentBlockChangeHandler>>();

                foreach ( var type in blockChangesTypes )
                {
                    try
                    {
                        var blockType = type.GetCustomAttribute<StructuredContentBlockAttribute>().BlockType;
                        var list = ( List<IStructuredContentBlockChangeHandler> ) changeHandlers.GetValueOrNull( blockType );

                        if ( list == null )
                        {
                            list = new List<IStructuredContentBlockChangeHandler>();
                            changeHandlers.Add( blockType, list );
                        }

                        var changeHandler = ( IStructuredContentBlockChangeHandler ) Activator.CreateInstance( type );

                        list.Add( changeHandler );
                    }
                    catch
                    {
                        /* Intentionally left blank. */
                    }
                }

                _changeHandlers = changeHandlers;
            }

            return _changeHandlers;
        }

        /// <summary>
        /// Detects the changes that need to be applied to the database by
        /// looking at the old content and the current content.
        /// </summary>
        /// <param name="helper">The content helper.</param>
        /// <param name="oldContent">The old structured content before the save.</param>
        /// <returns>
        /// The changes that were detected.
        /// </returns>
        public static StructuredContentChanges DetectChanges( this StructuredContentHelper helper, string oldContent = "" )
        {
            return DetectChanges( helper, oldContent, GetBlockChangeHandlers() );
        }

        /// <summary>
        /// Detects the changes that need to be applied to the database by
        /// looking at the old content and the current content.
        /// </summary>
        /// <param name="helper">The content helper.</param>
        /// <param name="oldContent">The old structured content before the save.</param>
        /// <param name="changeHandlers">The change handlers to use for detection.</param>
        /// <returns>
        /// The changes that were detected.
        /// </returns>
        /// <remarks>This method is internal so that it can be used for unit testing.</remarks>
        internal static StructuredContentChanges DetectChanges( this StructuredContentHelper helper, string oldContent, IReadOnlyDictionary<string, IReadOnlyList<IStructuredContentBlockChangeHandler>> changeHandlers )
        {
            var changes = new StructuredContentChanges();
            var newData = helper.Content?.FromJsonOrNull<StructuredContentData>() ?? new StructuredContentData();
            var oldData = oldContent?.FromJsonOrNull<StructuredContentData>() ?? new StructuredContentData();

            // Walk all the blocks that already existed and still exist or
            // are new in the data.
            foreach ( var newBlock in newData.Blocks )
            {
                if ( changeHandlers.TryGetValue( newBlock.Type, out var handlers ) )
                {
                    var oldBlock = oldData.Blocks.FirstOrDefault( b => b.Id == newBlock.Id );

                    foreach ( var handler in handlers )
                    {
                        handler.DetectChanges( newBlock.Data, oldBlock?.Data, changes );
                    }
                }
            }

            // Walk all the old blocks that no longer exist.
            var newBlockIds = newData.Blocks.Select( b => b.Id ).ToList();
            foreach ( var removedBlock in oldData.Blocks.Where( b => !newBlockIds.Contains( b.Id ) ) )
            {
                if ( changeHandlers.TryGetValue( removedBlock.Type, out var handlers ) )
                {
                    foreach ( var handler in handlers )
                    {
                        handler.DetectChanges( null, removedBlock.Data, changes );
                    }
                }
            }

            return changes;
        }

        /// <summary>
        /// Applies any changes detected by a previous call to
        /// <see cref="DetectChanges(StructuredContentHelper, string)"/> to the
        /// database. This method does not persist any changes to the database,
        /// you must still call <see cref="DbContext.SaveChanges()"/>.
        /// </summary>
        /// <param name="helper">The content helper.</param>
        /// <param name="changes">The changes that were returned by a previous call to DetectChanges()." />.</param>
        /// <param name="rockContext">The rock database context to use when saving changes.</param>
        /// <returns><c>true</c> if any changes were made that require you to call <see cref="DbContext.SaveChanges()"/>; otherwise <c>false</c>.</returns>
        public static bool ApplyDatabaseChanges( this StructuredContentHelper helper, StructuredContentChanges changes, RockContext rockContext )
        {
            if ( changes == null )
            {
                throw new ArgumentNullException( nameof( changes ) );
            }

            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            return ApplyDatabaseChanges( helper, changes, rockContext, GetBlockChangeHandlers() );
        }

        /// <summary>
        /// Applies any changes detected by a previous call to
        /// <see cref="DetectChanges(StructuredContentHelper, string)"/> to the
        /// database. This method does not persist any changes to the database,
        /// you must still call <see cref="DbContext.SaveChanges()"/>.
        /// </summary>
        /// <param name="helper">The content helper.</param>
        /// <param name="changes">The changes that were returned by a previous call to DetectChanges()." />.</param>
        /// <param name="rockContext">The rock database context to use when saving changes.</param>
        /// <param name="changeHandlers">The change handlers to use for applying changes.</param>
        /// <returns><c>true</c> if any changes were made that require you to call <see cref="DbContext.SaveChanges()"/>; otherwise <c>false</c>.</returns>
        /// <remarks>This method is internal so that it can be used for unit testing.</remarks>
        internal static bool ApplyDatabaseChanges( this StructuredContentHelper helper, StructuredContentChanges changes, RockContext rockContext, IReadOnlyDictionary<string, IReadOnlyList<IStructuredContentBlockChangeHandler>> changeHandlers )
        {
            bool needSave = false;

            // Notify each change handler about the change.
            foreach ( var handler in changeHandlers.SelectMany( a => a.Value ) )
            {
                if ( handler.ApplyDatabaseChanges( helper, changes, rockContext ) )
                {
                    needSave = true;
                }
            }

            return needSave;
        }
    }
}
