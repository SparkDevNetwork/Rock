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

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Oidc.Configuration;
using Rock.SystemKey;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Security
{
    [TestClass]
    public class RockSigningCredentialsTests : DatabaseTestsBase
    {
        [TestMethod]
        public void RockSigningCredentials_CreatingANewObjectShouldAlwaysHaveASigningKey()
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE Attribute WHERE [Key] = '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}'" );

            RockCache.ClearAllCachedItems( false );

            var rockSigningCredentials = ReflectionHelper.InstantiateInternalObject<IRockOidcSigningCredentials>( "Rock.Oidc.Configuration.RockOidcSigningCredentials", RockOidcSettings.GetDefaultSettings() );
            Assert.That.IsNotNull( rockSigningCredentials );
            Assert.That.IsTrue( rockSigningCredentials.SigningKeys.Count > 0 );

            var systemSettings = SystemSettings.GetValue( SystemSetting.OPEN_ID_CONNECT_RSA_KEYS );
            Assert.That.IsNotEmpty( systemSettings );
        }

        [TestMethod]
        public void RockSigningCredentials_ShouldReturnTheActiveSigningKey()
        {
            var keyCreatedDate = DateTime.UtcNow.AddDays( -15 );
            var SigningKeys = $@"[{{""KeyId"":""c5c66078-c88f-4a57-9294-61b04491803a"",""KeyCreatedDate"":""{keyCreatedDate.ToString( "O" )}"",""Parameters"":{{""Exponent"":""AQAB"",""Modulus"":""y99YHW6o5ym/Oeia3cGwFNYUxSFyQTrSpuX7j/2uV6HcVxqFV3Hlo/UC/st5bnUfIaIDX3egC8VeoDyRacsNogAvBDXYGUMQjGd6lwwfm5XFaZXcETi1JioU2Ddl3R21fWxooXBVuUQE7g8+6p5/yMnahA/T1JcVCDeeHU2RWZovtQOh+Y9n3q6eL6mbjtF6TghQazeiAyL3LtYkos6+WwY8Y9gaAWgB3CeZU6j1Ae/E+qGPuQq0v4mwibuDL8oRUk0NhCWb7CM2jIbMMXe3SBjbK+qy7hndAMNgwJ/qvSJWWuonkzqeCdStZnpiNQc34JKedGqBKLk+it8Di3dSMQ=="",""P"":""/GyNNQIDxQdiTkQ2JhQwvsjrtMxgKKlQW6ZpHUdCZLDbpyEBa8fHgotO2j6ug8MX5dWv8/71KraUoNmbCC0hT8h+c0BMgiKrxUtCyrOEK3xOTsO2crbsQxnGRE0hNLQccLFzcxM+eJ6wQG8j8bMJNRbmlOy/Re+kNbvco6IyQYs="",""Q"":""zsK2x2F1Zm/wj5Cc4Q1c08be6C6dO6R1w9ZEWCoiCBh6F6I2+LyqXETsb+JD8aaFFel6nvsXDIXJlTYRLBjPjHD+Oh9CWIaSa6Uw016FowOt5R+VZipwzVK9tN7u09bcxOBoDcw4DlKmFN3u7APtjm349XT1NoLl3zFt8/muOrM="",""DP"":""ZTt3ifmr31mtwCu096KDRhA4D0MjkUsN0iOz5i0M4GrZPHaNJldxmNYbooUe4fLc46zGKvlmA2JDyxpaBXZr9J44sCnqRQp4juA3AinqaLIqiYYN5oWbzPFKRVwVZBiTi1JvNYhTNnaVwtGPvcAKgkMT6EfmEbsgxPf5bZp/wy8="",""DQ"":""ZIH+u/lCSqOLux4/RJ9sSn5YCWHviPivTp2v53PDy0+quiZetpdv8R6IGPNSt/uMolQ3CWVhlPLMRT5dJqSA7/JVgweDBumT78QDchx2tgGp3MF3rIxg/U6FbZEZY90EwKedrWdisIO4vFgONqjKJ/yJkzhhozgKam7q1ji/W0U="",""InverseQ"":""QzsjDBVYCpvHcjvBWTkS1gD1M/R5v5h03rKuCQ8u5p8U2PpzltLjglthjQ/Phn7yvUL7dfxfBasbFTSTYpPB/fXX42cAcCu7cmD5Sppy4v/h61W6Dhq1zH7N5DrGrp8z56EvZKph16xZCPwt1L96R2LaN01wlE9de7ehd3XRGyY="",""D"":""hAG66VvUy9ExO8rMNBiM7gDsc/RDKc9vxJeXutV0xNNOe6v1ePiLzA0Cgn63wvjdToa8Dl3D6LtEmRZ+xXSwAByEVQKUSU8ucOsz6of0E2b363UYiKIiUXLgClxcfb8V2/+NBDNbnllXo9mFUJ+OeDGTZv1kmPu2p5pzq9+k5NGTDTNZgYQmWnqDY44bj2BvtrP28w0TFdLm6zoCrkl8RYx028Nt3Vr2uGqroVef6L56BCH4Z6cbGArYBB7TZwG4qreJ+UcQHkNyzWtx6vHcz24WNyR8ZdD92mPvqrc7rGULEISHFlE67yYn31My6/m1jga3UaJADMMe/CS1eu483Q==""}}}}]";
            var rockContext = new RockContext();

            rockContext.Database.ExecuteSqlCommand( $"DELETE Attribute WHERE [Key] = '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}'" );
            rockContext.Database.ExecuteSqlCommand( $@"INSERT INTO Attribute([FieldTypeId], [EntityTypeQualifierColumn], [Key], [Name], [DefaultValue], [Guid], [IsSystem], [Order], [IsGridColumn], [IsMultiValue], [IsRequired])
                                                        VALUES (1, 'SystemSetting', '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}', '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}',
                                                        '{SigningKeys}', NEWID(), 1, 0, 0, 0, 0)" );

            RockCache.ClearAllCachedItems( false );

            var systemSettings = SystemSettings.GetValue( SystemSetting.OPEN_ID_CONNECT_RSA_KEYS );
            Assert.That.IsNotEmpty( systemSettings );

            var rockSigningCredentials = ReflectionHelper.InstantiateInternalObject<IRockOidcSigningCredentials>( "Rock.Oidc.Configuration.RockOidcSigningCredentials", RockOidcSettings.GetDefaultSettings() );
            Assert.That.IsNotNull( rockSigningCredentials );
            Assert.That.AreEqual( 1, rockSigningCredentials.SigningKeys.Count );

            var actualParameters = rockSigningCredentials.SigningKeys[0].ExportParameters( true );
            var actualModulus = Convert.ToBase64String( actualParameters.Modulus, 0, actualParameters.Modulus.Length );

            Assert.That.AreNotEqual( "y99YHW6o5ym/Oeia3cGwFNYUxSFyQTrSpuX7j/2uV6HcVxqFV3Hlo/UC/st5bnUfIaIDX3egC8VeoDyRacsNogAvBDXYGUMQjGd6lwwfm5XFaZXcETi1JioU2Ddl3R21fWxooXBVuUQE7g8+6p5/yMnahA/T1JcVCDeeHU2RWZovtQOh+Y9n3q6eL6mbjtF6TghQazeiAyL3LtYkos6+WwY8Y9gaAWgB3CeZU6j1Ae/E+qGPuQq0v4mwibuDL8oRUk0NhCWb7CM2jIbMMXe3SBjbK+qy7hndAMNgwJ/qvSJWWuonkzqeCdStZnpiNQc34JKedGqBKLk+it8Di3dSMQ==", actualModulus );
        }

        [TestMethod]
        public void RockSigningCredentials_AddANewKeyWhenTheOldKeyHasExpired()
        {
            var keyCreatedDate = DateTime.UtcNow.AddHours( -26 );
            var SigningKeys = $@"[{{""KeyId"":""c5c66078-c88f-4a57-9294-61b04491803a"",""KeyCreatedDate"":""{keyCreatedDate.ToString("O")}"",""Parameters"":{{""Exponent"":""AQAB"",""Modulus"":""y99YHW6o5ym/Oeia3cGwFNYUxSFyQTrSpuX7j/2uV6HcVxqFV3Hlo/UC/st5bnUfIaIDX3egC8VeoDyRacsNogAvBDXYGUMQjGd6lwwfm5XFaZXcETi1JioU2Ddl3R21fWxooXBVuUQE7g8+6p5/yMnahA/T1JcVCDeeHU2RWZovtQOh+Y9n3q6eL6mbjtF6TghQazeiAyL3LtYkos6+WwY8Y9gaAWgB3CeZU6j1Ae/E+qGPuQq0v4mwibuDL8oRUk0NhCWb7CM2jIbMMXe3SBjbK+qy7hndAMNgwJ/qvSJWWuonkzqeCdStZnpiNQc34JKedGqBKLk+it8Di3dSMQ=="",""P"":""/GyNNQIDxQdiTkQ2JhQwvsjrtMxgKKlQW6ZpHUdCZLDbpyEBa8fHgotO2j6ug8MX5dWv8/71KraUoNmbCC0hT8h+c0BMgiKrxUtCyrOEK3xOTsO2crbsQxnGRE0hNLQccLFzcxM+eJ6wQG8j8bMJNRbmlOy/Re+kNbvco6IyQYs="",""Q"":""zsK2x2F1Zm/wj5Cc4Q1c08be6C6dO6R1w9ZEWCoiCBh6F6I2+LyqXETsb+JD8aaFFel6nvsXDIXJlTYRLBjPjHD+Oh9CWIaSa6Uw016FowOt5R+VZipwzVK9tN7u09bcxOBoDcw4DlKmFN3u7APtjm349XT1NoLl3zFt8/muOrM="",""DP"":""ZTt3ifmr31mtwCu096KDRhA4D0MjkUsN0iOz5i0M4GrZPHaNJldxmNYbooUe4fLc46zGKvlmA2JDyxpaBXZr9J44sCnqRQp4juA3AinqaLIqiYYN5oWbzPFKRVwVZBiTi1JvNYhTNnaVwtGPvcAKgkMT6EfmEbsgxPf5bZp/wy8="",""DQ"":""ZIH+u/lCSqOLux4/RJ9sSn5YCWHviPivTp2v53PDy0+quiZetpdv8R6IGPNSt/uMolQ3CWVhlPLMRT5dJqSA7/JVgweDBumT78QDchx2tgGp3MF3rIxg/U6FbZEZY90EwKedrWdisIO4vFgONqjKJ/yJkzhhozgKam7q1ji/W0U="",""InverseQ"":""QzsjDBVYCpvHcjvBWTkS1gD1M/R5v5h03rKuCQ8u5p8U2PpzltLjglthjQ/Phn7yvUL7dfxfBasbFTSTYpPB/fXX42cAcCu7cmD5Sppy4v/h61W6Dhq1zH7N5DrGrp8z56EvZKph16xZCPwt1L96R2LaN01wlE9de7ehd3XRGyY="",""D"":""hAG66VvUy9ExO8rMNBiM7gDsc/RDKc9vxJeXutV0xNNOe6v1ePiLzA0Cgn63wvjdToa8Dl3D6LtEmRZ+xXSwAByEVQKUSU8ucOsz6of0E2b363UYiKIiUXLgClxcfb8V2/+NBDNbnllXo9mFUJ+OeDGTZv1kmPu2p5pzq9+k5NGTDTNZgYQmWnqDY44bj2BvtrP28w0TFdLm6zoCrkl8RYx028Nt3Vr2uGqroVef6L56BCH4Z6cbGArYBB7TZwG4qreJ+UcQHkNyzWtx6vHcz24WNyR8ZdD92mPvqrc7rGULEISHFlE67yYn31My6/m1jga3UaJADMMe/CS1eu483Q==""}}}}]";
            var rockContext = new RockContext();

            rockContext.Database.ExecuteSqlCommand( $"DELETE Attribute WHERE [Key] = '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}'" );
            rockContext.Database.ExecuteSqlCommand( $@"INSERT INTO Attribute([FieldTypeId], [EntityTypeQualifierColumn], [Key], [Name], [DefaultValue], [Guid], [IsSystem], [Order], [IsGridColumn], [IsMultiValue], [IsRequired])
                                                        VALUES (1, 'SystemSetting', '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}', '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}',
                                                        '{SigningKeys}', NEWID(), 1, 0, 0, 0, 0)" );

            RockCache.ClearAllCachedItems( false );

            var systemSettings = SystemSettings.GetValue( SystemSetting.OPEN_ID_CONNECT_RSA_KEYS );
            Assert.That.IsNotEmpty( systemSettings );

            var oidcSettings = RockOidcSettings.GetDefaultSettings();
            oidcSettings.AccessTokenLifetime = 3600;
            oidcSettings.SigningKeyLifetime = 86400;

            var rockSigningCredentials = ReflectionHelper.InstantiateInternalObject<IRockOidcSigningCredentials>( "Rock.Oidc.Configuration.RockOidcSigningCredentials", oidcSettings );
            Assert.That.IsNotNull( rockSigningCredentials );
            Assert.That.AreEqual( 2, rockSigningCredentials.SigningKeys.Count );

            var actualParameters = rockSigningCredentials.SigningKeys[1].ExportParameters( true );
            var actualModulus = Convert.ToBase64String( actualParameters.Modulus, 0, actualParameters.Modulus.Length );

            Assert.That.AreEqual( "y99YHW6o5ym/Oeia3cGwFNYUxSFyQTrSpuX7j/2uV6HcVxqFV3Hlo/UC/st5bnUfIaIDX3egC8VeoDyRacsNogAvBDXYGUMQjGd6lwwfm5XFaZXcETi1JioU2Ddl3R21fWxooXBVuUQE7g8+6p5/yMnahA/T1JcVCDeeHU2RWZovtQOh+Y9n3q6eL6mbjtF6TghQazeiAyL3LtYkos6+WwY8Y9gaAWgB3CeZU6j1Ae/E+qGPuQq0v4mwibuDL8oRUk0NhCWb7CM2jIbMMXe3SBjbK+qy7hndAMNgwJ/qvSJWWuonkzqeCdStZnpiNQc34JKedGqBKLk+it8Di3dSMQ==", actualModulus );
        }

        [TestMethod]
        public void RockSigningCredentials_RemoveKeyWhenTheOldKeyHasExpiredAndPassedMaxLifetime()
        {
            var rockOidcSettings = RockOidcSettings.GetDefaultSettings();

            var key1CreatedDate = DateTime.UtcNow.AddSeconds( -1 * ( rockOidcSettings.RefreshTokenLifetime + rockOidcSettings.SigningKeyLifetime ) ).ToString( "yyyy-MM-ddThh:mm:ss" );
            var key2CreatedDate = DateTime.UtcNow.AddSeconds( 12 * 60 * 60 ).ToString( "yyyy-MM-ddThh:mm:ss" );
            var expectedModulus = "y99YHW6o5ym/Oeia3cGwFNYUxSFyQTrSpuX7j/2uV6HcVxqFV3Hlo/UC/st5bnUfIaIDX3egC8VeoDyRacsNogAvBDXYGUMQjGd6lwwfm5XFaZXcETi1JioU2Ddl3R21fWxooXBVuUQE7g8+6p5/yMnahA/T1JcVCDeeHU2RWZovtQOh+Y9n3q6eL6mbjtF6TghQazeiAyL3LtYkos6+WwY8Y9gaAWgB3CeZU6j1Ae/E+qGPuQq0v4mwibuDL8oRUk0NhCWb7CM2jIbMMXe3SBjbK+qy7hndAMNgwJ/qvSJWWuonkzqeCdStZnpiNQc34JKedGqBKLk+it8Di3dSMQ==";

            var SigningKeys = @"[
	            {
		            ""KeyId"": ""c45bc5ac-f77e-418f-9f8c-76011e2c877e"",
		            ""KeyCreatedDate"": """ + key1CreatedDate + @""",
		            ""Parameters"": {
			            ""Exponent"": ""AQAB"",
			            ""Modulus"": ""xo/OrhJBZOkGEyNVzbMwb2xQMkGGVjqx/+Hv9Gk97SQA9IMsDWQGi2v6Dtkr+WnckDtK0f8r0P9SSBOoS2psRMFOQDl8VmGK+/1ku5x0FdAM8dW8S2+ugRGEg6u8T9RRAD5ube8+8IlhOuq1+FmPaKkPGQ02wyQzHETHZyo8P/1/xK4vVvIrDvu2+79Nw07q/JLCppmUp9Vblshe93IELr/WIpM7z9GVjqA3LaZ46JTmaqwMmVCjFosYOWpFqDrwwIl3fLpzZ5gPNVjFI485QZFKC5cnpvXbfDg2h0o4ysfVqZoC7QzIThQNOeglFhqeoB14v1/fCsxkQGq6ulodZQ=="",
			            ""P"": ""yXwVvY4ogHD47L3dL3ZeTaWDSonmpW/Xa46EdhEvJZMFdC7G8HnMc8CDRnwYxtsAaHWUJlrg+oKtrO2gkpbgOf0w6+SxCo5M999JsevJLeGz7nzmfGgnRWg8gCs17DrBjVmy7WiSF92fxKMfVBW0N44KH+B3Pexl6bA3KnW4J98="",
			            ""Q"": ""/ElDNR0UGdF6lwb5sul7i87AuutkT+nMKmKxp71CNvt5YjnqHAM65LtRLRUPchrD9EBXc75DDzEtc6+7YJs3jFO91RO7c1vDekPcRCZc4+8WRiOMDKgCs+PHr7gQp8NNjYNTd4tjrJgZ2hdiUZSR/U2nwLP9Jp/h3xalj2v4szs="",
			            ""DP"": ""JDmqEv2nLIijSLVOOkVW6TDz7QfkLyRvn8fs6ulmB6RqW5w5am0LpFgdgiO3tLEVXrKdI1Q9lOy/2xKSRyjXQbXTAOaKKjKxhfNgZZvV/OjSl/Ne4Uk8nk8CaazbMhDSUd6pu+OMOLxBCHKnpE+OqlpgWaDa74g8PorPMFQf+xM="",
			            ""DQ"": ""fvQTCOBIJDHgwa017A1IKXNyUt1vTjN1lQKzKZi4gFiNnZtNLqmFcmK8l7YYXrAPWZMjLtBYiIWTvLp6zUNucYQWa+oRExzjQlxxtC2l4uGGUOdEa34EVifSbZ9vaiyCkyq5ztdq4ghsQe4wgKMFz/TK7NLmGhCspgmMaJ7JuM8="",
			            ""InverseQ"": ""geiy/z+a9tAlhuw/VqQYrhYwvABT2aEsdiuZ9EhR+v2hk4UupVS4xLdohvKjH9VSwnAK3ncrag6JNTxqMEuVYDaUEMW6AMSWglfaj1PfGLSG74A/dKT5iJRLEKNi1BlhOLoQrJ+YXgeBX9VrGkxvKG1RdwryCVIhn6zUle4Fny0="",
			            ""D"": ""gdWkunPbaMVqIHd5cpCZujHj2oi384hbvcqZ4YIzaO2i5j8jPfpKwT2we6cLtwG+pFzw1pF4sCdTQSgBYpbLBsYziZFNORp9C7qr9HUf/udUn4k3n0f+ngy7TnGsJ7LX6EXqZV4MFLJv7lts82B0gmYRJjtatIzmErTyggW+8cOC4m90xX5qmSLyAhRFq/nvpeZ+lVAooLOw3kvc6ZexA/uce4pnkUYGW1T7qkdvPzGW6abw99ixzzLqwJMo3nCuHd3XPHnRbOyRC0ZH2cWdcYPsAGwoesgENaq/MN3lE6wzEPPxVt/ytrSjDhESfoqdQj9sd5wUM8g39WqiKQm7/Q==""
		            }
	            },
	            {
		            ""KeyId"": ""c5c66078-c88f-4a57-9294-61b04491803a"",
		            ""KeyCreatedDate"": """ + key2CreatedDate + @""",
		            ""Parameters"": {
			            ""Exponent"": ""AQAB"",
			            ""Modulus"": """ + expectedModulus + @""",
			            ""P"": ""/GyNNQIDxQdiTkQ2JhQwvsjrtMxgKKlQW6ZpHUdCZLDbpyEBa8fHgotO2j6ug8MX5dWv8/71KraUoNmbCC0hT8h+c0BMgiKrxUtCyrOEK3xOTsO2crbsQxnGRE0hNLQccLFzcxM+eJ6wQG8j8bMJNRbmlOy/Re+kNbvco6IyQYs="",
			            ""Q"": ""zsK2x2F1Zm/wj5Cc4Q1c08be6C6dO6R1w9ZEWCoiCBh6F6I2+LyqXETsb+JD8aaFFel6nvsXDIXJlTYRLBjPjHD+Oh9CWIaSa6Uw016FowOt5R+VZipwzVK9tN7u09bcxOBoDcw4DlKmFN3u7APtjm349XT1NoLl3zFt8/muOrM="",
			            ""DP"": ""ZTt3ifmr31mtwCu096KDRhA4D0MjkUsN0iOz5i0M4GrZPHaNJldxmNYbooUe4fLc46zGKvlmA2JDyxpaBXZr9J44sCnqRQp4juA3AinqaLIqiYYN5oWbzPFKRVwVZBiTi1JvNYhTNnaVwtGPvcAKgkMT6EfmEbsgxPf5bZp/wy8="",
			            ""DQ"": ""ZIH+u/lCSqOLux4/RJ9sSn5YCWHviPivTp2v53PDy0+quiZetpdv8R6IGPNSt/uMolQ3CWVhlPLMRT5dJqSA7/JVgweDBumT78QDchx2tgGp3MF3rIxg/U6FbZEZY90EwKedrWdisIO4vFgONqjKJ/yJkzhhozgKam7q1ji/W0U="",
			            ""InverseQ"": ""QzsjDBVYCpvHcjvBWTkS1gD1M/R5v5h03rKuCQ8u5p8U2PpzltLjglthjQ/Phn7yvUL7dfxfBasbFTSTYpPB/fXX42cAcCu7cmD5Sppy4v/h61W6Dhq1zH7N5DrGrp8z56EvZKph16xZCPwt1L96R2LaN01wlE9de7ehd3XRGyY="",
			            ""D"": ""hAG66VvUy9ExO8rMNBiM7gDsc/RDKc9vxJeXutV0xNNOe6v1ePiLzA0Cgn63wvjdToa8Dl3D6LtEmRZ+xXSwAByEVQKUSU8ucOsz6of0E2b363UYiKIiUXLgClxcfb8V2/+NBDNbnllXo9mFUJ+OeDGTZv1kmPu2p5pzq9+k5NGTDTNZgYQmWnqDY44bj2BvtrP28w0TFdLm6zoCrkl8RYx028Nt3Vr2uGqroVef6L56BCH4Z6cbGArYBB7TZwG4qreJ+UcQHkNyzWtx6vHcz24WNyR8ZdD92mPvqrc7rGULEISHFlE67yYn31My6/m1jga3UaJADMMe/CS1eu483Q==""
		            }
	            }
            ]";
            var rockContext = new RockContext();

            rockContext.Database.ExecuteSqlCommand( $"DELETE Attribute WHERE [Key] = '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}'" );
            rockContext.Database.ExecuteSqlCommand( $@"INSERT INTO Attribute([FieldTypeId], [EntityTypeQualifierColumn], [Key], [Name], [DefaultValue], [Guid], [IsSystem], [Order], [IsGridColumn], [IsMultiValue], [IsRequired])
                                                        VALUES (1, 'SystemSetting', '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}', '{SystemSetting.OPEN_ID_CONNECT_RSA_KEYS}',
                                                        '{SigningKeys}', NEWID(), 1, 0, 0, 0, 0)" );

            RockCache.ClearAllCachedItems( false );

            var systemSettings = SystemSettings.GetValue( SystemSetting.OPEN_ID_CONNECT_RSA_KEYS );
            Assert.That.IsNotEmpty( systemSettings );

            var rockSigningCredentials = ReflectionHelper.InstantiateInternalObject<IRockOidcSigningCredentials>( "Rock.Oidc.Configuration.RockOidcSigningCredentials", rockOidcSettings );
            Assert.That.IsNotNull( rockSigningCredentials );
            Assert.That.AreEqual( 1, rockSigningCredentials.SigningKeys.Count );

            var actualParameters = rockSigningCredentials.SigningKeys[0].ExportParameters( true );
            var actualModulus = Convert.ToBase64String( actualParameters.Modulus, 0, actualParameters.Modulus.Length );

            Assert.That.AreEqual( expectedModulus, actualModulus );
        }
    }
}