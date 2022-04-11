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

namespace Rock.Model
{
    public static partial class SignatureDocumentTemplateExtensionMethods
    {
        /// <summary>
        /// Determines whether the template is a legacy template provider.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns><c>true</c> if the template is a legacy tempalte provider; otherwise, <c>false</c>.</returns>
        public static bool IsLegacyProvider( this SignatureDocumentTemplate template )
        {
            return template?.ProviderEntityTypeId != null;
        }
    }
}
