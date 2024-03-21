using System;

using Rock.IpAddress;

namespace Rock.Tests.Shared.Mocks
{
    public class IpRegistryMock : IpRegistry
    {
        /// <summary>
        /// The API Keys associated with a valid ipregistry.com account.
        /// For testing purposes, create a free account at http://ipregistery.co
        /// and use the default API Key associated with the account.
        /// </summary>
        public const string IpRegistryApiKeyHasZeroCredit = "6y3c071363eqou2y";
        public const string IpRegistryApiKeyHasPositiveCredit = "rg5z35nb9f8or00u";

        public string ApiKey { get; set; } = IpRegistryApiKeyHasPositiveCredit;

        public override string GetAttributeValue( string key )
        {
            if ( key == "APIKey" )
            {
                return ApiKey;
            }
            throw new Exception( "Invalid Test Attribute Key." );
        }
    }

}
