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
namespace Rock.Lava
{
    /// <summary>
    /// Specifies that this object can supply an ILavaDataDictionary, either as a representation of itself or as a related object.
    /// </summary>
    public interface ILavaDataDictionarySource
    {
        /// <summary>
        /// Gets an ILavaDataDictionary that represents the data contained in the implementing object or a related object.
        /// </summary>
        /// <returns></returns>
        ILavaDataDictionary GetLavaDataDictionary();
    }
}
