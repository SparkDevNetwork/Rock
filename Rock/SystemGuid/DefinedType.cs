//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// Static Guids used by the Rock ChMS application
    /// </summary>
    public class DefinedType
    {
        /// <summary>
        /// The types of communication supported (i.e. email, sms, twitter, app-push, etc)
        /// </summary>
        public const string COMMUNICATION_CHANNEL = "DC8A841C-E91D-4BD4-A6A7-0DE765308E8F";

        /// <summary>
        /// Guid for checkin search type
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE = "1EBCDB30-A89A-4C14-8580-8289EC2C7742";

        /// <summary>
        /// Guid for Device Type
        /// </summary>
        public const string DEVICE_TYPE = "0368B637-327A-4F5E-80C2-832079E482EE";

        /// <summary>
        /// Guid for Financial Currency Type
        /// </summary>
        public const string FINANCIAL_ACCOUNT_TYPE = "752DA126-471F-4221-8503-5297593C99FF";
		
		/// <summary>
        /// Guid for the types of Batches (ACH, Visa, MasterCard, Discover, Amex, and PayPal)
        /// </summary>
       // public const string FINANCIAL_BATCH_TYPE = "9e358fbe-2321-4c54-895f-c888e29298ae";
		
        /// <summary>
        /// Guid for Financial Currency Type
        /// </summary>
        public const string FINANCIAL_CURRENCY_TYPE =  "1D1304DE-E83A-44AF-B11D-0C66DD600B81"; 

        /// <summary>
        /// Guid for Financial Credit Card Type
        /// </summary>
        public const string FINANCIAL_CREDIT_CARD_TYPE =  "2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9";

        /// <summary>
        /// Guid for Financial Payment Type
        /// </summary>
        public const string FINANCIAL_PAYMENT_TYPE = "23E80D98-017E-47B9-BAF3-AC442A1EC3EE";

        /// <summary>
        /// Guid for the Financial Pledge Frequency 
        /// </summary>
        public const string FINANCIAL_PLEDGE_FREQUENCY = "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F";

        /// <summary>
        /// Guid for Financial Source Type
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE =  "4F02B41E-AB7D-4345-8A97-3904DDD89B01";

        /// <summary>
        /// Guid for Financial Frequency 
        /// </summary>
        public const string FINANCIAL_TRANSACTION_FREQUENCY = "1F645CFB-5BBD-4465-B9CA-0D2104A1479B";

        /// <summary>
        /// Guid for Financial Transaction Type
        /// </summary>
        public const string FINANCIAL_TRANSACTION_REFUND_REASON = "61FE3A58-9F4F-472F-A4E0-5116EB90A323";

        /// <summary>
        /// Guid for Financial transaction image type
        /// </summary>
        public const string FINANCIAL_TRANSACTION_IMAGE_TYPE = "0745D5DE-2D09-44B3-9017-40C1DA83CB39"; 

        /// <summary>
        /// Guid for Financial Transaction Type
        /// </summary>
        public const string FINANCIAL_TRANSACTION_TYPE = "FFF62A4B-5D88-4DEB-AF8F-8E6178E41FE5";
        
        /// <summary>
        /// Guid for the types of Locations (such as Home, Main Office, etc)
        /// </summary>
        public const string LOCATION_LOCATION_TYPE =  "2e68d37c-fb7b-4aa5-9e09-3785d52156cb";

        /// <summary>
        /// Guid for the types of States that can be tied to a Location's address.
        /// </summary>
        public const string LOCATION_ADDRESS_STATE = "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB";

        /// <summary>
        /// Guid for Marketing Campaign Audience Type
        /// </summary>
        public const string MARKETING_CAMPAIGN_AUDIENCE_TYPE =  "799301A3-2026-4977-994E-45DC68502559";

        /// <summary>
        /// Metric Collection Frequency
        /// </summary>
        public const string METRIC_COLLECTION_FREQUENCY =  "526CB333-2C64-4486-8469-7F7EA9366254";
        
        /// <summary>
        /// Guid for the types of Person Records (such as person, business, etc.)
        /// </summary>
        public const string PERSON_RECORD_TYPE =  "26be73a6-a9c5-4e94-ae00-3afdcf8c9275";

        /// <summary>
        /// Guid for the types of Person Record Statuses (such as active, inactive, pending, etc.)
        /// </summary>
        public const string PERSON_RECORD_STATUS =  "8522badd-2871-45a5-81dd-c76da07e2e7e";

        /// <summary>
        /// Guid for the types of Person Status Reasons (such as deceased, moved, etc.)
        /// </summary>
        public const string PERSON_RECORD_STATUS_REASON =  "e17d5988-0372-4792-82cf-9e37c79f7319";

        /// <summary>
        /// Guid for the person status (such as member, attendee, participant, etc.)
        /// </summary>
        public const string PERSON_STATUS =  "2e6540ea-63f0-40fe-be50-f2a84735e600";

        /// <summary>
        /// Guid for the types of Person Titles (such as Mr., Mrs., Dr., etc.)
        /// </summary>
        public const string PERSON_TITLE =  "4784cd23-518b-43ee-9b97-225bf6e07846";

        /// <summary>
        /// Guid for the types of Person Suffixes (such as Jr., Sr., etc.)
        /// </summary>
        public const string PERSON_SUFFIX =  "16f85b3c-b3e8-434c-9094-f3d41f87a740";

        /// <summary>
        /// Guid for the types of Person Marital Statuses (such as Married, Single, Divorced, Widowed, etc.)
        /// </summary>
        public const string PERSON_MARITAL_STATUS =  "b4b92c3f-a935-40e1-a00b-ba484ead613b";

        /// <summary>
        /// Guid for the types of Person phone numbers (such as Primary, Secondary, etc.)
        /// </summary>
        public const string PERSON_PHONE_TYPE =  "8345DD45-73C6-4F5E-BEBD-B77FC83F18FD";

        /// <summary>
        /// Guid for the types of Batches (ACH, Visa, MasterCard, Discover, Amex, and PayPal)
        /// </summary>
        public static Guid FINANCIAL_BATCH_TYPE { get { return new Guid( "9e358fbe-2321-4c54-895f-c888e29298ae" ); } }
    }
}