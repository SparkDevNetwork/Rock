using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;
using Xunit;

namespace Rock.Tests.Rock.Utility.NcoaApi
{
    /// <summary>
    /// Test that NCOA report result columns match implemented standard
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class TrueNcoaApiFixture : IDisposable
    {
        /// <summary>
        /// Gets or sets the TrueNOCA response columns.
        /// </summary>
        /// <value>
        /// The TrueNOCA response columns.
        /// </value>
        public List<string> ResponseColumns { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrueNcoaApiFixture"/> class and download an export from TrueNCOA.
        /// </summary>
        public TrueNcoaApiFixture()
        {
            ResponseColumns = new List<string>();

            string NCOA_SERVER = "https://app.testing.truencoa.com";
            string exportfileid = "4c58fba3-8ee9-4644-893a-e3903ad0f91b";

            var _client = new RestClient( NCOA_SERVER );
            _client.AddDefaultHeader( "user_name", "gerhard@sparkdevnetwork.org" );
            _client.AddDefaultHeader( "password", "TrueNCOA_password" );
            _client.AddDefaultHeader( "Content-Type", "application/x-www-form-urlencoded" );

            var request = new RestRequest( $"api/files/{exportfileid}/records", Method.GET );
            request.AddParameter( "application/x-www-form-urlencoded", "status=submit", ParameterType.RequestBody );
            IRestResponse response = _client.Execute( request );
            if ( response.StatusCode != HttpStatusCode.OK )
            {
                return;
            }

            dynamic obj = null;
            try
            {
                obj = JObject.Parse( response.Content );
                foreach ( var o in obj.Records[0] )
                {
                    ResponseColumns.Add( o.Name );
                }

                return;
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

    }

    /// <summary>
    /// Test if TrueNCOA columns changed.
    /// </summary>
    /// <seealso cref="Xunit.IClassFixture{Rock.Tests.Rock.Utility.NcoaApi.TrueNcoaApiFixture}" />
    public class TrueNcoaApiTests : IClassFixture<TrueNcoaApiFixture>
    {
        TrueNcoaApiFixture _trueNcoaApiFixture;

        public TrueNcoaApiTests( TrueNcoaApiFixture trueNcoaApiFixture )
        {
            this._trueNcoaApiFixture = trueNcoaApiFixture;
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_individual_id()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "individual_id" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_individual_first_name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "individual_first_name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_individual_last_name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "individual_last_name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_address_line_1()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "address_line_1" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_address_line_2()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "address_line_2" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_address_city_name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "address_city_name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_address_state_code()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "address_state_code" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_address_postal_code()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "address_postal_code" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_address_country_code()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "address_country_code" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Household_Position()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Household Position" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Name_ID()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Name ID" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Street_Suffix()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Street Suffix" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Unit_Type()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Unit Type" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Unit_Number()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Unit Number" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Box_Number()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Box Number" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_City_Name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "City Name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_State_Code()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "State Code" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Postal_Code()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Postal Code" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Postal_Code_Extension()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Postal Code Extension" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Carrier_Route()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Carrier Route" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Address_Status()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Address Status" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Individual_Record_ID()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Individual Record ID" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Error_Number()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Error Number" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Address_Type()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Address Type" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Delivery_Point()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Delivery Point" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Check_Digit()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Check Digit" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Delivery_Point_Verification()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Delivery Point Verification" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Delivery_Point_Verification_Notes()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Delivery Point Verification Notes" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Vacant()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Vacant" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Congressional_District_Code()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Congressional District Code" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Area_Code()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Area Code" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Latitude()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Latitude" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_First_Name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "First Name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Longitude()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Longitude" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Time_Zone()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Time Zone" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_County_Name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "County Name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_County_FIPS()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "County FIPS" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_State_FIPS()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "State FIPS" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Barcode()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Barcode" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Locatable_Address_Conversion_System()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Locatable Address Conversion System" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Line_of_Travel()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Line of Travel" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Ascending_Descending()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Ascending/Descending" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Move_Applied()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Move Applied" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Last_Name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Last Name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Move_Type()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Move Type" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Move_Date()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Move Date" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Move_Distance()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Move Distance" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Match_Flag()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Match Flag" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_NXI()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "NXI" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_ANK()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "ANK" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Residential_Delivery_Indicator()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Residential Delivery Indicator" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Record_Type()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Record Type" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Country_Code()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Country Code" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Company_Name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Company Name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Address_Line_1()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Address Line 1" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Address_Line_2()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Address Line 2" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Address_Id()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Address Id" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Household_Id()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Household Id" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Individual_space_Id()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Individual Id" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Street_Number()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Street Number" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Street_Pre_Direction()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Street Pre Direction" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Street_Name()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Street Name" );
            Assert.True( output );
        }

        [Fact( Skip = "Needs seeded data in TrueNcoa" )]
        public void Records_Contains_Street_Post_Direction()
        {
            bool output = _trueNcoaApiFixture.ResponseColumns.Contains( "Street Post Direction" );
            Assert.True( output );
        }
    }
}
