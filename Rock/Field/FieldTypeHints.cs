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

using Rock.ViewModels.Utility;

namespace Rock.Field
{
    /// <summary>
    /// The hints that a Field Type can provide to help a consumer understand
    /// how to work with the raw values.
    /// </summary>
    public class FieldTypeHints
    {
        /// <summary>
        /// The potential values that this field type could have. This should
        /// only be filled in if the field type has a known set of values and
        /// that set is reasonably small. For large sets that have a known
        /// small set of common values, those common values can be provided
        /// here.
        /// </summary>
        public List<ListItemBag> Values { get; set; }

        /// <summary>
        /// Determines if the items in <see cref="Values"/> represents the
        /// complete list of possible values. If false, then the list is
        /// just a subset of common values.
        /// </summary>
        public bool IsCompleteList { get; set; }

        /// <summary>
        /// <para>
        /// A string that describes the raw format of the field type value.
        /// This should take into account any configuration values that may
        /// affect the range of values, such as a minimum or maximum. This text
        /// should be human readable, though it may be used in AI related
        /// tasks.
        /// </para>
        /// <para>
        /// Be sure to both the format of the value, any constraints on the
        /// value, and when appropriate where to get the value. For example,
        /// do not say 'A guid value', instead say 'A guid that represents
        /// a single entity from the Campus table'.
        /// </para>
        /// </summary>
        public string ValueFormat { get; set; }

        /// <summary>
        /// <para>
        /// What a consumer must do to obtain the valid values, when they cannot
        /// be read from <see cref="Values"/> alone. Typically because the list
        /// is too large to enumerate, or because it lives on another record
        /// that has to be looked up.
        /// </para>
        /// <para>
        /// This is written for an automated consumer deciding its next call, not
        /// for display to a person. Name the record that holds the values and the
        /// identifier needed to reach it, for example 'To find the correct values
        /// look them up using the Defined Type IdKey of aBc123XyZ.'
        /// </para>
        /// <para>
        /// Never name a specific tool, API, or endpoint. A field type has no
        /// knowledge of what is calling it, and the same hint is read by consumers
        /// with entirely different tooling. Describe the data, and let the caller
        /// choose how to fetch it.
        /// </para>
        /// <para>
        /// This is distinct from <see cref="ValueFormat"/>, which describes what a
        /// stored value looks like. This describes how to discover which values are
        /// allowed.
        /// </para>
        /// </summary>
        public string Instructions { get; set; }
    }
}
