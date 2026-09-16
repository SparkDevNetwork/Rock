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
namespace Rock.Obsidian.UI.GridField
{
    /// <summary>
    /// Metadata describing a single column in the grid being built. The output
    /// helper populates one descriptor per column and exposes the collection on
    /// <see cref="ObsidianGridFieldContext.Columns"/> so fields whose transform
    /// needs peer-column context (see
    /// <see cref="ObsidianGridField.ReadsPeerValues"/>) can iterate the columns
    /// and look up peer values.
    /// </summary>
    public class ObsidianGridColumnDescriptor
    {
        /// <summary>
        /// The friendly merge-key for this column (e.g. <c>"FirstName"</c>),
        /// derived by the output helper from ColumnHeaderText or a fallback.
        /// This is the key under which the column's value appears in
        /// <see cref="ObsidianGridFieldContext.RowValues"/>.
        /// </summary>
        public string MergeKey { get; }

        /// <summary>
        /// The runtime dynamic-type field name that carries this column's raw
        /// expression output on the row object (e.g. <c>"entity_NickName_1"</c>).
        /// Present for advanced consumers that want to read raw values directly
        /// from <see cref="ObsidianGridFieldContext.RowObject"/>.
        /// </summary>
        public string SourceFieldName { get; }

        /// <summary>
        /// The <see cref="ObsidianGridField"/> instance registered for this
        /// column. Late-binding fields inspect this to reason about the column
        /// (its <see cref="ObsidianGridField.ColumnType"/>, its class type,
        /// etc.) without a bag of type-specific flags.
        /// </summary>
        public ObsidianGridField Field { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ObsidianGridColumnDescriptor"/> class.
        /// </summary>
        /// <param name="mergeKey">The friendly merge-key.</param>
        /// <param name="sourceFieldName">The runtime dynamic-type field name.</param>
        /// <param name="field">The registered ObsidianGridField.</param>
        public ObsidianGridColumnDescriptor( string mergeKey, string sourceFieldName, ObsidianGridField field )
        {
            MergeKey = mergeKey;
            SourceFieldName = sourceFieldName;
            Field = field;
        }
    }
}
