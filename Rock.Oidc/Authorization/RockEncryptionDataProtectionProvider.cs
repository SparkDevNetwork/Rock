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
using Microsoft.AspNetCore.DataProtection;

namespace Rock.Oidc.Authorization
{
    /// <summary>
    /// Implements the IDataProtectionProvider interface using the MachineKey class to
    /// create IDataProtector instances that can be used to protect and unprotect data.
    /// </summary>
    public class RockEncryptionDataProtectionProvider : IDataProtectionProvider
    {
        /// <summary>
        /// The legacy data protector provider to use for backward compatibility
        /// when unprotecting data that was protected with a previous version
        /// of the protector.
        /// </summary>
        /// <remarks>
        /// This should be considered safe to remove by Rock v22.
        /// </remarks>
        private readonly IDataProtectionProvider _legacyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockEncryptionDataProtectionProvider"/> class.
        /// </summary>
        /// <param name="legacyProvider">The legacy data protection provider to use for backward compatibility.</param>
        public RockEncryptionDataProtectionProvider( IDataProtectionProvider legacyProvider )
        {
            _legacyProvider = legacyProvider;
        }

        /// <inheritdoc/>
        public IDataProtector CreateProtector( string purpose )
        {
            return new RockEncryptionDataProtector( new[] { purpose }, _legacyProvider.CreateProtector( purpose ) );
        }
    }
}
