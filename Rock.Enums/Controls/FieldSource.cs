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
namespace Rock.Enums.Controls
{
    /// <summary>
    /// The source of the data for the field. The two types are properties on the item model and an attribute expression.
    /// This is copied to Rock/Mobile/JsonFields/FieldSource.cs. If any changes are made here,
    /// they may need to be copied there as well.
    /// </summary>
    public enum FieldSource
    {
        /// <summary>
        /// The source comes from a model property.
        /// </summary>
        Property = 0,

        /// <summary>
        /// The source comes from an attribute defined on the model.
        /// </summary>
        Attribute = 1,

        /// <summary>
        /// The source comes from a custom lava expression.
        /// </summary>
        LavaExpression = 2
    }

}
