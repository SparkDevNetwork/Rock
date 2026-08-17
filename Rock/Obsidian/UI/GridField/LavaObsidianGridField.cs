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

using Rock.Lava;

namespace Rock.Obsidian.UI.GridField
{
    /// <summary>
    /// Value-shaping subclass that resolves a Lava template against the
    /// transformed peer-column values for the current row. Backs LiquidSelect,
    /// whose whole purpose is to let the Report author write a template that
    /// can reference peer columns by name.
    /// </summary>
    /// <remarks>
    /// This field opts into the late-binding pass by returning <c>true</c> from
    /// <see cref="ObsidianGridField.ReadsPeerValues"/>. The output helper runs
    /// all eager (non-late-binding) fields first, populates
    /// <see cref="ObsidianGridFieldContext.RowValues"/> with their transformed
    /// outputs (keyed by
    /// <see cref="ObsidianGridColumnDescriptor.MergeKey"/>), then invokes this
    /// field. The template therefore sees the DISPLAY values of peer columns
    /// (e.g. a DefinedValue attribute column rendered via
    /// <see cref="LabelObsidianGridField"/> appears as its badge markup rather
    /// than as a raw Guid). This differs subtly from the WebForms
    /// <c>LavaField</c> path, which gives raw expression values plus a
    /// DefinedValueField special-case; the new behavior is more consistent
    /// (every column shows its display form) and does not require special-casing
    /// specific field types.
    /// </remarks>
    public class LavaObsidianGridField : HtmlObsidianGridField
    {
        /// <summary>
        /// The Lava template supplied by the DataSelect at construction.
        /// </summary>
        public string LavaTemplate { get; set; }

        /// <inheritdoc/>
        public override bool ReadsPeerValues => true;

        /// <inheritdoc/>
        public override object TransformValue( object rawValue, ObsidianGridFieldContext context )
        {
            if ( string.IsNullOrWhiteSpace( LavaTemplate ) )
            {
                return string.Empty;
            }

            var mergeFields = new Dictionary<string, object>();
            if ( context?.RowValues != null )
            {
                foreach ( var kvp in context.RowValues )
                {
                    mergeFields[kvp.Key] = kvp.Value;
                }
            }

            return LavaTemplate.ResolveMergeFields( mergeFields );
        }
    }
}
