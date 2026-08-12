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

using Rock.ViewModels.Core.Grid;

namespace Rock.Obsidian.UI.GridField
{
    /// <summary>
    /// Abstract base for describing how a DataSelect's column renders in an
    /// Obsidian Grid. Rock ships seven root-tier sealed subclasses that each map
    /// to a Vue column type (Text, Html, Boolean, Number, Currency, Date, DateTime)
    /// plus four value-shaping subclasses that build on the roots (Label, Phone,
    /// List, Lava). Plugins extend by subclassing one of the shipped subclasses
    /// and overriding <see cref="TransformValue"/>.
    /// </summary>
    public abstract class ObsidianGridField
    {
        /// <summary>
        /// Internal so only Rock.dll can add new column-type roots. Plugins extend
        /// the hierarchy by subclassing one of the sealed built-in leaves
        /// (typically <see cref="TextObsidianGridField"/> or
        /// <see cref="HtmlObsidianGridField"/>) or one of the shipped value-shaping
        /// subclasses.
        /// </summary>
        internal ObsidianGridField()
        {
        }

        /// <summary>
        /// The Obsidian grid column-type string this field maps to. Each root-tier
        /// subclass seals this so its subclasses inherit the same Vue column type.
        /// </summary>
        public abstract string ColumnType { get; }

        /// <summary>
        /// When <c>true</c>, this field's <see cref="TransformValue"/> depends on
        /// peer columns' transformed values and must run after all non-late fields
        /// have been transformed for the current row. The output helper populates
        /// <see cref="ObsidianGridFieldContext.RowValues"/> with peer outputs and
        /// invokes late-binding fields in column order, updating
        /// <see cref="ObsidianGridFieldContext.RowValues"/> after each so a
        /// subsequent late-binding field sees the prior one's output.
        /// </summary>
        /// <remarks>
        /// Default is <c>false</c>. The only shipped subclass that opts in is
        /// <see cref="LavaObsidianGridField"/>, which needs peer values to
        /// resolve template merge fields. Every other subclass computes its
        /// output from its own raw value alone and skips the late-binding pass.
        /// </remarks>
        public virtual bool ReadsPeerValues => false;

        /// <summary>
        /// Projects the raw value produced by the DataSelect's GetExpression into
        /// whatever the Vue column expects. Default is identity. Subclasses
        /// override when the raw value shape does not match what the Vue column
        /// expects, or when peer column values are needed (via
        /// <paramref name="context"/>.RowValues, which requires opting into the
        /// late-binding pass via <see cref="ReadsPeerValues"/>).
        /// </summary>
        /// <param name="rawValue">Raw value for this column on the current row.</param>
        /// <param name="context">
        /// Per-row context, supplied at call time so the field instance itself
        /// remains stateless and safely reusable across requests. Carries the
        /// request-scoped <see cref="ObsidianGridFieldContext.RockContext"/>,
        /// the raw dynamic-typed <see cref="ObsidianGridFieldContext.RowObject"/>,
        /// per-column <see cref="ObsidianGridFieldContext.Columns"/> metadata,
        /// and (for late-binding fields only) the
        /// <see cref="ObsidianGridFieldContext.RowValues"/> peer-outputs dictionary.
        /// </param>
        /// <returns>The value to ship as the display cell.</returns>
        public virtual object TransformValue( object rawValue, ObsidianGridFieldContext context )
        {
            return rawValue;
        }

        /// <summary>
        /// Produces the value used for Excel / CSV export of this cell. Returns
        /// null by default, which the output helper and Vue side both interpret
        /// as "no distinct export value; use the display value." Subclasses
        /// override when the exported shape should differ from what renders on
        /// screen (e.g. stripping HTML markup, sending "Yes"/"No" instead of raw
        /// booleans, extracting inner text from a single-anchor cell).
        /// </summary>
        /// <param name="rawValue">Raw value for this column on the current row.</param>
        /// <param name="context">Per-row context.</param>
        /// <returns>
        /// The value to ship as the paired export field, or null to fall back to
        /// the display value.
        /// </returns>
        /// <remarks>
        /// The output helper projects the return into a paired {name}__export field
        /// on each row. Row-serialization SHOULD omit fields whose value is null so
        /// identity-export cells cost zero wire bytes; where the serializer does
        /// not naturally support this, the Vue-side fallback still yields correct
        /// behavior. The Vue side configures every column's exportValue prop to
        /// prefer the paired field when it is present and non-null, and fall back
        /// to the display field otherwise.
        /// </remarks>
        public virtual object GetExportValue( object rawValue, ObsidianGridFieldContext context )
        {
            return null;
        }

        /// <summary>
        /// Populates the per-column-type props flowed to Vue via
        /// <see cref="DynamicFieldDefinitionBag.FieldProperties"/>. Empty by
        /// default; subclasses that expose typed props override.
        /// </summary>
        /// <returns>The per-column-type props dictionary.</returns>
        protected virtual Dictionary<string, object> GetFieldProperties()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Produces the wire bag consumed by the Obsidian Grid. Non-virtual by
        /// design; subclasses influence the output via <see cref="ColumnType"/>
        /// and <see cref="GetFieldProperties"/>. The output helper additionally
        /// populates SortFields (from
        /// <see cref="Rock.Reporting.DataSelectComponent.SortProperties"/>) on the
        /// returned bag after this method runs.
        /// </summary>
        /// <returns>The wire bag.</returns>
        public DynamicFieldDefinitionBag GetDefinitionBag()
        {
            return new DynamicFieldDefinitionBag
            {
                ColumnType = ColumnType,
                FieldProperties = GetFieldProperties()
            };
        }
    }
}
