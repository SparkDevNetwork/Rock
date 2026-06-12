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
