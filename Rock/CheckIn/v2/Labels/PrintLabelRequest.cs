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

using System.Collections.Generic;

using Rock.Data;
using Rock.Lava;
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// Encapsulates all the data about a request to print a single label.
    /// </summary>
    internal class PrintLabelRequest
    {
        /// <summary>
        /// The cached merged fields for the label. This be set dynamically when
        /// first requested and then never modified.
        /// </summary>
        private Dictionary<string, object> _mergeFields;

        /// <summary>
        /// The capabilities of the printer that will receive the rendered
        /// print data.
        /// </summary>
        public PrinterCapabilities Capabilities { get; set; }

        /// <summary>
        /// The data that describes the designed label being printed.
        /// </summary>
        public DesignedLabelBag Label { get; set; }

        /// <summary>
        /// The data sources available to the renderer when generating the
        /// printer data.
        /// </summary>
        public IReadOnlyDictionary<string, FieldDataSource> DataSources { get; set; }

        /// <summary>
        /// The <see cref="RockContext"/> object that should be used if access
        /// to the database is required. This should only be used for read
        /// operations. Any write operation should create a new context.
        /// </summary>
        public RockContext RockContext { get; set; }

        /// <summary>
        /// The data object used by data sources and any custom Lava merge fields.
        /// </summary>
        public object LabelData { get; set; }

        /// <summary>
        /// Gets the merge fields associated with <see cref="LabelData"/>. This
        /// takes each property from <see cref="LabelData"/> and adds the value
        /// to the merge fields with the property name.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetMergeFields()
        {
            if ( _mergeFields == null )
            {
                var mergeFields = new Dictionary<string, object>();

                if ( LabelData != null )
                {
                    foreach ( var prop in LabelData.GetType().GetProperties() )
                    {
                        var value = prop.GetValue( LabelData );

                        mergeFields.Add( prop.Name, LavaDataWrapper.GetWrappedObject( value ) );
                    }
                }

                _mergeFields = mergeFields;
            }

            return _mergeFields;
        }
    }
}
