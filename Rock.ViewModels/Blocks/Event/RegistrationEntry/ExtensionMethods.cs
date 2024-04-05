using System.Collections.Generic;
using System.Linq;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    public static class ExtensionMethods
    {
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

        public static List<RegistrantInfo> AsRegistrantInfoListOrNull( this IEnumerable<RegistrantBag> registrants )
        {
            return registrants
                ?.Select( AsRegistrantInfoOrNull )
                // Remove null entries.
                .Where( info => info != null )
                .ToList();
        }

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

        public static List<RegistrantBag> AsRegistrantBagListOrNull( this IEnumerable<RegistrantInfo> registrants )
        {
            return registrants
                ?.Select( AsRegistrantBagOrNull )
                // Remove null entries.
                .Where( info => info != null )
                .ToList();
        }

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
