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

namespace Rock.Model
{
    /// <summary>
    /// DataViewFilterOverrides with a Dictionary of Filter Overrides where the Key is the DataViewFilter.Guid
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "{DebuggerDisplay}" )]
    public class DataViewFilterOverrides : Dictionary<Guid, DataViewFilterOverride>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataViewFilterOverrides"/> class.
        /// </summary>
        public DataViewFilterOverrides() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataViewFilterOverrides"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public DataViewFilterOverrides( List<DataViewFilterOverride> list ) :
            base( list.ToDictionary( k => k.DataFilterGuid, v => v ) )
        { }

        /// <summary>
        /// List of DataViewIds that should not use Persisted Values
        /// </summary>
        /// <value>
        /// The ignore data view persisted values.
        /// </value>
        public HashSet<int> IgnoreDataViewPersistedValues { get; set; } = new HashSet<int>();

        /// <summary>
        /// Gets the override.
        /// </summary>
        /// <param name="dataViewFilterGuid">The data view filter unique identifier.</param>
        /// <returns></returns>
        public DataViewFilterOverride GetOverride( Guid dataViewFilterGuid )
        {
            if ( this.ContainsKey( dataViewFilterGuid ) )
            {
                return this[dataViewFilterGuid];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the debugger display.
        /// </summary>
        /// <value>
        /// The debugger display.
        /// </value>
        private string DebuggerDisplay
        {
            get
            {
                return $@"IgnoreDataViewPersistedValues for DataViewIds: {IgnoreDataViewPersistedValues.ToList().AsDelimited( "," )},DataViewFilterOverrides.Count:{this.Count}";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether stats for this dataview filter should be logged].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [should log statics]; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldUpdateStatics { get; set; } = true;
    }
}
