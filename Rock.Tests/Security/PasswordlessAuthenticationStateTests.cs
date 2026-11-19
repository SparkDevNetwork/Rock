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

using Rock.Security.Authentication;
using Rock.Security.Authentication.Passwordless;

namespace Rock.Tests.Security
{
    /// <summary>
    /// Unit tests for the registration code on <see cref="PasswordlessAuthenticationState"/>.
    /// </summary>
    [TestClass]
    public class PasswordlessAuthenticationStateTests
    {
        /// <summary>
        /// The registration code must be kept when the state is encrypted
        /// and decrypted, or Account Entry would refuse every verified state.
        /// </summary>
        [TestMethod]
        public void RegistrationCode_IsKeptWhenStateIsEncryptedAndDecrypted()
        {
            var state = CreateState();
            state.RegistrationCode = "BCDFGH";

            var encryptedState = PasswordlessAuthentication.GetEncryptedAuthenticationState( state );
            var decryptedState = PasswordlessAuthentication.GetDecryptedAuthenticationState( encryptedState );

            Assert.IsNotNull( decryptedState );
            Assert.AreEqual( "BCDFGH", decryptedState.RegistrationCode );
            Assert.AreEqual( state.Code, decryptedState.Code );
            Assert.AreEqual( state.UniqueIdentifier, decryptedState.UniqueIdentifier );
        }

        /// <summary>
        /// A state like the one returned when a code is sent has no
        /// registration code after it is decrypted.
        /// </summary>
        [TestMethod]
        public void RegistrationCode_IsMissingFromSendStepState()
        {
            var state = CreateState();

            var encryptedState = PasswordlessAuthentication.GetEncryptedAuthenticationState( state );
            var decryptedState = PasswordlessAuthentication.GetDecryptedAuthenticationState( encryptedState );

            Assert.IsNotNull( decryptedState );
            Assert.IsNull( decryptedState.RegistrationCode );
        }

        /// <summary>
        /// A registration code cannot be added to a state by changing the
        /// encrypted value, because a changed value does not decrypt.
        /// </summary>
        [TestMethod]
        public void ChangedEncryptedState_DoesNotDecrypt()
        {
            var encryptedState = PasswordlessAuthentication.GetEncryptedAuthenticationState( CreateState() );
            var characters = encryptedState.ToCharArray();
            var index = characters.Length / 2;
            characters[index] = characters[index] == 'A' ? 'B' : 'A';

            var decryptedState = PasswordlessAuthentication.GetDecryptedAuthenticationState( new string( characters ) );

            Assert.IsNull( decryptedState );
        }

        /// <summary>
        /// A state encrypted for another purpose cannot be used as a
        /// passwordless state.
        /// </summary>
        [TestMethod]
        public void StateEncryptedForAnotherPurpose_DoesNotDecrypt()
        {
            var state = CreateState();
            state.RegistrationCode = "BCDFGH";

            var encryptedState = Rock.Security.Encryption.EncryptString( state.ToJson() );

            var decryptedState = PasswordlessAuthentication.GetDecryptedAuthenticationState( encryptedState );

            Assert.IsNull( decryptedState );
        }

        private static PasswordlessAuthenticationState CreateState()
        {
            return new PasswordlessAuthenticationState
            {
                UniqueIdentifier = "ted.decker@example.com",
                Email = "ted.decker@example.com",
                Code = "ZXCVBN",
                CodeIssueDate = new DateTime( 2026, 10, 15, 9, 0, 0 ),
                CodeLifetime = TimeSpan.FromMinutes( 60 )
            };
        }
    }
}
