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
using System.Collections.Generic;

using Rock.Data;

namespace Rock.Obsidian.UI.GridField
{
    /// <summary>
    /// Per-row context supplied to <see cref="ObsidianGridField.TransformValue"/>.
    /// The output helper builds one instance per row and reuses the same caches
    /// backing store across every row of a single report render, so memoized
    /// lookups accumulate across the whole grid build.
    /// </summary>
    public class ObsidianGridFieldContext
    {
        /*
            2026-08-12 - DH

            ConcurrentDictionary chosen so GetCache<T> stays safe if the row-materialization
            loop is ever refactored to be multi-threaded; the per-lookup overhead is
            negligible next to the cache-hit work itself. Not exposed directly; access is
            via GetCache<T>.

            Reason: Future-proofing against parallel materialization at trivial perf cost.
        */
        private readonly ConcurrentDictionary<Type, object> _caches;

        /// <summary>
        /// Request-scoped <see cref="Rock.Data.RockContext"/> for lookups
        /// (DefinedValueCache, PersonAliasService, etc.).
        /// </summary>
        public RockContext RockContext { get; }

        /// <summary>
        /// The raw dynamic-typed row instance produced by the report query. All
        /// columns' raw expression outputs are accessible as public fields on
        /// this object; consumers typically use
        /// <see cref="ObsidianGridColumnDescriptor.SourceFieldName"/> to look
        /// them up via reflection when they need raw peer values.
        /// </summary>
        public object RowObject { get; }

        /// <summary>
        /// Per-column metadata for every column in the current grid, in the
        /// order the output helper registered them. Fields whose transform
        /// needs to reason about peer columns (see
        /// <see cref="ObsidianGridField.ReadsPeerValues"/>) iterate this
        /// collection to find them.
        /// </summary>
        public IReadOnlyList<ObsidianGridColumnDescriptor> Columns { get; }

        /// <summary>
        /// The <see cref="ObsidianGridField.TransformValue"/> outputs of prior
        /// columns for the current row, keyed by
        /// <see cref="ObsidianGridColumnDescriptor.MergeKey"/>. Populated by the
        /// output helper ONLY for fields whose
        /// <see cref="ObsidianGridField.ReadsPeerValues"/> is <c>true</c>; for
        /// eager fields this is <c>null</c>. When multiple late-binding fields
        /// exist, they run in column order and <c>RowValues</c> accumulates each
        /// one's output before the next runs.
        /// </summary>
        public IReadOnlyDictionary<string, object> RowValues { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ObsidianGridFieldContext"/> class.
        /// </summary>
        /// <param name="rockContext">The request-scoped RockContext.</param>
        /// <param name="rowObject">The raw dynamic-typed row instance.</param>
        /// <param name="columns">Per-column metadata for the current grid.</param>
        /// <param name="rowValues">
        /// The peer-column values dictionary; supply <c>null</c> for eager fields
        /// and a populated dict for late-binding fields.
        /// </param>
        /// <param name="sharedCaches">
        /// The shared cache backing store. The output helper allocates one dictionary
        /// per report render and passes the same reference into every per-row context.
        /// </param>
        public ObsidianGridFieldContext(
            RockContext rockContext,
            object rowObject,
            IReadOnlyList<ObsidianGridColumnDescriptor> columns,
            IReadOnlyDictionary<string, object> rowValues,
            ConcurrentDictionary<Type, object> sharedCaches )
        {
            RockContext = rockContext;
            RowObject = rowObject;
            Columns = columns;
            RowValues = rowValues;
            _caches = sharedCaches;
        }

        /// <summary>
        /// Returns a per-report-render cache of type <typeparamref name="T"/>,
        /// allocating it on first access. The same instance is returned to every
        /// field's <see cref="ObsidianGridField.TransformValue"/> call for the
        /// current report render, so lookups memoized on one row are available on
        /// all subsequent rows.
        /// </summary>
        /// <typeparam name="T">
        /// The caller-defined cache shape (typically a class with one or more typed
        /// <see cref="Dictionary{TKey,TValue}"/> properties). Type identity prevents
        /// collisions between different fields' caches; a field that keys by
        /// <c>int PersonId</c> for one purpose does not collide with another field
        /// that keys by <c>int</c> for a different purpose because each defines its
        /// own cache type.
        /// </typeparam>
        /// <returns>The shared cache instance for type <typeparamref name="T"/>.</returns>
        public T GetCache<T>() where T : class, new()
        {
            return ( T ) _caches.GetOrAdd( typeof( T ), _ => new T() );
        }
    }
}
