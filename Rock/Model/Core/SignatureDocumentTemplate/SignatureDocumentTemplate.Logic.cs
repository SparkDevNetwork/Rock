﻿// <copyright>
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

using System.ComponentModel.DataAnnotations.Schema;

namespace Rock.Model
{
    public partial class SignatureDocumentTemplate
    {
        /// <summary>
        /// The default <see cref="LavaTemplate"/>
        /// </summary>
        public const string DefaultLavaTemplate = @"
<p>## Insert your legal disclaimer here ##</p>
<br>
";
    }

    /// <summary>
    /// Entity Set Item POCO Entity.
    /// </summary>
    public partial class SignatureDocumentTemplate
    {
        /// <summary>
        /// Determines whether this instance is legacy.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is legacy; otherwise, <c>false</c>.
        /// </returns>
        [NotMapped]
        public bool IsLegacy
        {
            get { return ProviderEntityTypeId.HasValue; }
        }
    }
}
