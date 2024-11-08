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
using System.Collections.Generic;
using System.Linq;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts the RegistrationEntryArgsBag to RegistrationEntryBlockArgs or null (if null).
        /// </summary>
        public static RegistrationEntryBlockArgs AsArgsOrNull( this RegistrationEntryArgsBag bag )
        {
            return bag == null
                ? null
                : new RegistrationEntryBlockArgs
                {
                    AmountToPayNow = bag.AmountToPayNow,
                    DiscountCode = bag.DiscountCode,
                    FieldValues = bag.FieldValues,
                    GatewayToken = bag.GatewayToken,
                    Registrants = ( ( IRegistrationEntryBlockArgs ) bag ).Registrants,
                    Registrar = ( ( IRegistrationEntryBlockArgs ) bag ).Registrar,
                    RegistrationGuid = bag.RegistrationGuid,
                    RegistrationSessionGuid = bag.RegistrationSessionGuid,
                    SavedAccountGuid = bag.SavedAccountGuid,
                };
        }

        /// <summary>
        /// Converts the RegistrantBag to a RegistrantInfo or null (if null).
        /// </summary>
        public static RegistrantInfo AsRegistrantInfoOrNull( this RegistrantBag registrant )
        {
            return registrant == null
                ? null
                : new RegistrantInfo
                {
                    Cost = registrant.Cost,
                    ExistingSignatureDocumentGuid = registrant.ExistingSignatureDocumentGuid,
                    FamilyGuid = registrant.FamilyGuid,
                    FeeItemQuantities = registrant.FeeItemQuantities,
                    FieldValues = registrant.FieldValues,
                    Guid = registrant.Guid,
                    IsOnWaitList = registrant.IsOnWaitList,
                    PersonGuid = registrant.PersonGuid,
                    SignatureData = registrant.SignatureData,
                };
        }

        /// <summary>
        /// Converts the IEnumerable of RegistrantBag to a List of RegistrantInfo and excludes null entries.
        /// </summary>
        public static List<RegistrantInfo> AsRegistrantInfoListOrNull( this IEnumerable<RegistrantBag> registrants )
        {
            return registrants
                ?.Select( AsRegistrantInfoOrNull )
                // Remove null entries.
                .Where( info => info != null )
                .ToList();
        }

        /// <summary>
        /// Converts the RegistrantInfo to a RegistrantBag or null (if null).
        /// </summary>
        public static RegistrantBag AsRegistrantBagOrNull( this RegistrantInfo registrant )
        {
            return registrant == null
                ? null
                : new RegistrantBag
                {
                    Cost = registrant.Cost,
                    ExistingSignatureDocumentGuid = registrant.ExistingSignatureDocumentGuid,
                    FamilyGuid = registrant.FamilyGuid,
                    FeeItemQuantities = registrant.FeeItemQuantities,
                    FieldValues = registrant.FieldValues,
                    Guid = registrant.Guid,
                    IsOnWaitList = registrant.IsOnWaitList,
                    PersonGuid = registrant.PersonGuid,
                    SignatureData = registrant.SignatureData,
                };
        }

        /// <summary>
        /// Converts the IEnumerable RegistrantInfo to a List of RegistrantBag and excludes null entries.
        /// </summary>
        public static List<RegistrantBag> AsRegistrantBagListOrNull( this IEnumerable<RegistrantInfo> registrants )
        {
            return registrants
                ?.Select( AsRegistrantBagOrNull )
                // Remove null entries.
                .Where( info => info != null )
                .ToList();
        }

        /// <summary>
        /// Converts the RegistrarBag to a RegistrarInfo or null (if null).
        /// </summary>
        public static RegistrarInfo AsRegistrarInfoOrNull( this RegistrarBag registrar )
        {
            return registrar == null
                ? null
                : new RegistrarInfo
                {
                    Email = registrar.Email,
                    FamilyGuid = registrar.FamilyGuid,
                    LastName = registrar.LastName,
                    NickName = registrar.NickName,
                    UpdateEmail = registrar.UpdateEmail,
                };
        }

        /// <summary>
        /// Converts the RegistrarInfo to a RegistrarBag or null (if null).
        /// </summary>
        public static RegistrarBag AsRegistrarBagOrNull( this RegistrarInfo registrar )
        {
            return registrar == null
                ? null
                : new RegistrarBag
                {
                    Email = registrar.Email,
                    FamilyGuid = registrar.FamilyGuid,
                    LastName = registrar.LastName,
                    NickName = registrar.NickName,
                    UpdateEmail = registrar.UpdateEmail,
                };
        }
    }
}
