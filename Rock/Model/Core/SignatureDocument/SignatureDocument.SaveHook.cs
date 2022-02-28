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
using Rock.Data;
using Rock.ElectronicSignature;

namespace Rock.Model
{
    /// <summary>
    /// Class SignatureDocument.
    /// </summary>
    public partial class SignatureDocument
    {
        /// <summary>
        /// Class SaveHook.
        /// </summary>
        internal class SaveHook : EntitySaveHook<SignatureDocument>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Modified )
                {
                    var originalSignatureVerificationHash = OriginalValues[nameof( SignatureDocument.SignatureVerificationHash )] as string;
                    if ( originalSignatureVerificationHash.IsNotNullOrWhiteSpace() )
                    {
                        if ( originalSignatureVerificationHash != Entity.SignatureVerificationHash )
                        {
                            var signatureDocumentValidationException = new SignatureDocumentValidationException( $"{nameof( Entity.SignatureVerificationHash )} can not be modified once it is set." );
                            ExceptionLogService.LogException( signatureDocumentValidationException );
                            throw signatureDocumentValidationException;
                        }
                    }
                }

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed.
            /// </summary>
            /// <remarks>This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.</remarks>
            protected override void PostSave()
            {
                base.PostSave();
            }
        }
    }
}
