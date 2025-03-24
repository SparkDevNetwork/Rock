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
namespace Rock.Mobile.JsonFields
{
    /// <summary>
    /// The format to use for the attribute.
    /// This is copied to Rock.Enums/Controls/AttributeFormat.cs. If any changes are made here,
    /// they may need to be copied there as well.
    /// </summary>
    public enum AttributeFormat
    {
        /// <summary>
        /// The attribute's friendly value should be used.
        /// </summary>
        FriendlyValue = 0,

        /// <summary>
        /// The attribute's raw value should be used.
        /// </summary>
        RawValue = 1
    }

}
