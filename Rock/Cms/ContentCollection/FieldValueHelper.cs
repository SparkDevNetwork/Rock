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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;

namespace Rock.Cms.ContentCollection
{
    /// <summary>
    /// Provides a single place for all updates to field values to take place.
    /// This class will batch update requests and perform those changes in
    /// a single database hit.
    /// </summary>
    internal static class FieldValueHelper
    {
        private static readonly ConcurrentQueue<ValueItem> _collectionQueue = new ConcurrentQueue<ValueItem>();

        private static readonly object _processLock = new object();
        private static int _taskRunning = 0;

        /// <summary>
        /// Add a new attribute field value for the content collection. This will
        /// later be used by the view block to provide a list of items for
        /// the filters to display. This should be called anytime an attribute
        /// value is indexed.
        /// </summary>
        /// <param name="contentCollectionId">The identifier of the content collection this field value belongs to.</param>
        /// <param name="key">The key of the attribute whose value has been indexed.</param>
        /// <param name="value">The value of the attribute that was indexed.</param>
        /// <param name="formattedValue">The formatted value of the attribute that was indexed.</param>
        public static void AddFieldValue( int contentCollectionId, string key, string value, string formattedValue )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return;
            }

            _collectionQueue.Enqueue( new ValueItem
            {
                ContentCollectionId = contentCollectionId,
                Key = key,
                Value = value,
                FormattedValue = formattedValue
            } );

            // Replace the value in a thread safe way. Set it to 1 and then
            // the original value is returned. If it was 0 then that means
            // the queue task isn't running. This is a few ns faster than
            // a full lock.
            if ( Interlocked.Exchange( ref _taskRunning, 1 ) == 0 )
            {
                // No task running, start it up.
                Task.Run( ProcessQueueAsync );
            }
        }

        /// <summary>
        /// Add a new attribute field value for the content collection. This will
        /// later be used by the view block to provide a list of items for
        /// the filters to display. This should be called anytime an attribute
        /// value is indexed.
        /// </summary>
        /// <param name="contentCollectionId">The identifier of the content collection this field value belongs to.</param>
        /// <param name="key">The key of the attribute whose value has been indexed.</param>
        /// <param name="value">The value of the attribute that was indexed.</param>
        /// <param name="formattedValue">The formatted value of the attribute that was indexed.</param>
        public static void AddAttributeFieldValue( int contentCollectionId, string key, string value, string formattedValue )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return;
            }

            _collectionQueue.Enqueue( new ValueItem
            {
                ContentCollectionId = contentCollectionId,
                IsAttribute = true,
                Key = key,
                Value = value,
                FormattedValue = formattedValue
            } );

            // Replace the value in a thread safe way. Set it to 1 and then
            // the original value is returned. If it was 0 then that means
            // the queue task isn't running. This is a few ns faster than
            // a full lock.
            if ( Interlocked.Exchange( ref _taskRunning, 1 ) == 0 )
            {
                // No task running, start it up.
                Task.Run( ProcessQueueAsync );
            }
        }

        /// <summary>
        /// A background task that will process the queue after a period of
        /// time. This allows for batching up changes.
        /// </summary>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        private static async Task ProcessQueueAsync()
        {
            // Wait 250ms before we update to give things a chance to batch up
            // if we are making lots of edits.
            await Task.Delay( 250 );

            // Indicate that it is safe to start a new task.
            Interlocked.Exchange( ref _taskRunning, 0 );

            // Get a list of all the items to process.
            var itemsToProcess = new List<ValueItem>();
            while ( _collectionQueue.TryDequeue( out var item ) )
            {
                itemsToProcess.Add( item );
            }

            // At this point, another task may have started, so we will wrap
            // our actual processing inside a lock. Since this is something
            // happening on a background task and only once every 250ms, it
            // should be safe and performant.
            // This keeps two tasks from updating the same content collection
            // at the same time.
            lock ( _processLock )
            {
                ProcessQueueItems( itemsToProcess );
            }
        }

        /// <summary>
        /// Process the items that were dequeued.
        /// </summary>
        /// <param name="items">The items that were dequeued.</param>
        private static void ProcessQueueItems( List<ValueItem> items )
        {
            // Process each content collection as single update.
            var collectionItems = items.GroupBy( i => i.ContentCollectionId );

            using ( var rockContext = new RockContext() )
            {
                var contentCollectionService = new ContentCollectionService( rockContext );

                foreach ( var itemGroup in collectionItems )
                {
                    var contentCollection = contentCollectionService.Get( itemGroup.Key );

                    if ( contentCollection == null )
                    {
                        continue;
                    }

                    var filterSettings = contentCollection.FilterSettings.FromJsonOrNull<ContentCollectionFilterSettingsBag>() ?? new ContentCollectionFilterSettingsBag();
                    bool modified = false;

                    foreach ( var item in itemGroup )
                    {
                        List<ListItemBag> values;

                        // Ensure we have a valid dictionaries to work with.
                        if ( filterSettings.FieldValues == null )
                        {
                            filterSettings.FieldValues = new Dictionary<string, List<ListItemBag>>();
                        }
                        if ( filterSettings.AttributeValues == null )
                        {
                            filterSettings.AttributeValues = new Dictionary<string, List<ListItemBag>>();
                        }

                        // Try to get the existing values for the item, if
                        // we couldn't find it then start a new list.
                        if ( !item.IsAttribute )
                        {
                            if ( !filterSettings.FieldValues.TryGetValue( item.Key, out values ) )
                            {
                                values = new List<ListItemBag>();
                                filterSettings.FieldValues.Add( item.Key, values );
                            }
                        }
                        else
                        {
                            if ( !filterSettings.AttributeValues.TryGetValue( item.Key, out values ) )
                            {
                                values = new List<ListItemBag>();
                                filterSettings.AttributeValues.Add( item.Key, values );
                            }
                        }

                        // Try to find the existing value.
                        var itemValue = values.FirstOrDefault( v => v.Value == item.Value );

                        if ( itemValue == null )
                        {
                            // If not found, add it to the list.
                            values.Add( new ListItemBag
                            {
                                Value = item.Value,
                                Text = item.FormattedValue
                            } );
                            modified = true;
                        }
                        else if ( itemValue.Text != item.FormattedValue )
                        {
                            // If found and the formatted values differ, then
                            // update the formatted value.
                            itemValue.Text = item.FormattedValue;
                            modified = true;
                        }
                    }

                    // Only update the database if anything actually changed.
                    if ( modified )
                    {
                        contentCollection.FilterSettings = filterSettings.ToJson();
                    }
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Temporary storage of the attribute field values that have been updated.
        /// </summary>
        private class ValueItem
        {
            /// <summary>
            /// Gets or sets the content collection identifier.
            /// </summary>
            /// <value>The content collection identifier.</value>
            public int ContentCollectionId { get; set; }

            /// <summary>
            /// Gets or sets the attribute key.
            /// </summary>
            /// <value>The attribute key.</value>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this item is for an attribute.
            /// </summary>
            /// <value><c>true</c> if this item is for an attribute; otherwise, <c>false</c>.</value>
            public bool IsAttribute { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the formatted value.
            /// </summary>
            /// <value>The formatted value.</value>
            public string FormattedValue { get; set; }
        }
    }
}
