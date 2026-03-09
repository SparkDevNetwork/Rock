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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Defines the properties that will be used when translating the <see cref="Id"/>
    /// property to an <see cref="IdKey"/> value via extension method.
    /// </summary>
    public interface ITranslateIdKey
    {
        /// <summary>
        /// The integer identifier of the entity. This will be used to create
        /// the <see cref="IdKey"/> hash via extension method and then set to
        /// <c>null</c>.
        /// </summary>
        int? Id { get; set; }

        /// <summary>
        /// The identifier key of this entity.
        /// </summary>
        string IdKey { get; set; }
    }
}
