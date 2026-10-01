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

using Microsoft.AspNetCore.DataProtection;

using Rock.Security;

namespace Rock.Oidc.Authorization
{
    /// <summary>
    /// Implements the IDataProtector interface using the MachineKey class to
    /// protect and unprotect data.
    /// </summary>
    public class RockEncryptionDataProtector : IDataProtector
    {
        /// <summary>
        /// The ordered purposes for which the data protector will be used.
        /// This is used to create a unique context for the encryption and
        /// decryption operations, ensuring that data protected for one purpose
        /// cannot be unprotected using a protector created for a different
        /// purpose.
        /// </summary>
        private readonly string[] _purposes;

        /// <summary>
        /// The legacy data protector to use for backward compatibility when
        /// unprotecting data that was protected with a previous version of
        /// the protector.
        /// </summary>
        /// <remarks>
        /// This should be considered safe to remove by Rock v22.
        /// </remarks>
        private readonly IDataProtector _legacy;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockEncryptionDataProtector"/> class.
        /// </summary>
        /// <param name="protectionKey">The key used to protect the data.</param>
        /// <param name="purposes">The purposes for which the data protector will be used.</param>
        /// <param name="legacy">The legacy data protector to use for backward compatibility.</param>
        public RockEncryptionDataProtector( string[] purposes, IDataProtector legacy )
        {
            _purposes = purposes;
            _legacy = legacy;
        }

        /// <inheritdoc/>
        public IDataProtector CreateProtector( string purpose )
        {
            return new RockEncryptionDataProtector( _purposes.Concat( new[] { purpose } ).ToArray(), _legacy.CreateProtector( purpose ) );
        }

        /// <inheritdoc/>
        public byte[] Protect( byte[] plaintext )
        {
            var contextInfo = Encryption.BuildPurposeContext( _purposes );
            var protectedData = Encryption.EncryptBytes( plaintext, contextInfo )
                ?? throw new System.InvalidOperationException( "Failed to encrypt data." );

            return protectedData;
        }

        /// <inheritdoc/>
        public byte[] Unprotect( byte[] protectedData )
        {
            var contextInfo = Encryption.BuildPurposeContext( _purposes );
            var plaintext = Encryption.DecryptBytes( protectedData, contextInfo );

            if ( plaintext != null )
            {
                return plaintext;
            }

            return _legacy.Unprotect( protectedData );
        }
    }
}
