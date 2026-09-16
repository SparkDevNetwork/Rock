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

using System.Linq;
using System.Text;

using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Builds merge field identifiers that reference an entity type, optionally
    /// constrained by entity type qualifiers, in the format
    /// "EntityTypeName~QualifierColumn+QualifierValue~..." (for example,
    /// "GroupMemberAssignment~GroupId+15~ScheduleId+22"). A communication merge
    /// field with such a key carries an entity identifier as its value, and
    /// <see cref="Rock.Model.CommunicationRecipient"/> resolves it to the entity
    /// as a Lava merge object at send time. This helper exists outside of
    /// <see cref="Rock.Web.UI.Controls.MergeFieldPicker"/> (which delegates to it)
    /// so that Obsidian blocks and shared code can build these identifiers without
    /// referencing a WebForms control.
    /// </summary>
    internal static class EntityMergeFieldIdHelper
    {
        /// <summary>
        /// Gets the merge field identifier for the specified entity type and qualifiers.
        /// </summary>
        /// <typeparam name="T">The entity type the merge field references.</typeparam>
        /// <param name="entityTypeQualifiers">The entity type qualifiers, if any.</param>
        /// <returns>The merge field identifier.</returns>
        internal static string GetMergeFieldId<T>( EntityMergeFieldQualifier[] entityTypeQualifiers )
        {
            var mergeFieldIdBuilder = new StringBuilder( EntityTypeCache.Get<T>().Name );

            if ( entityTypeQualifiers?.Any() == true )
            {
                foreach ( var qualifier in entityTypeQualifiers )
                {
                    mergeFieldIdBuilder.Append( $"~{qualifier.Column}+{qualifier.Value}" );
                }
            }

            return mergeFieldIdBuilder.ToString();
        }
    }

    /// <summary>
    /// An entity type qualifier (column and value) used when building an entity
    /// merge field identifier with <see cref="EntityMergeFieldIdHelper"/>.
    /// </summary>
    internal class EntityMergeFieldQualifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMergeFieldQualifier"/> class.
        /// </summary>
        /// <param name="column">The qualifier column.</param>
        /// <param name="value">The qualifier value.</param>
        public EntityMergeFieldQualifier( string column, string value )
        {
            Column = column;
            Value = value;
        }

        /// <summary>
        /// Gets the qualifier column.
        /// </summary>
        public string Column { get; }

        /// <summary>
        /// Gets the qualifier value.
        /// </summary>
        public string Value { get; }
    }
}
