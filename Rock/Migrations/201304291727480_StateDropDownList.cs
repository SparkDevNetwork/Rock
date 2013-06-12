//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public partial class StateDropDownList : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedType( "Location", "Address State", "Postal state of a location's address", "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "AL", "Alabama", "45FAE9D7-F6ED-4FAF-B35A-8309EF35405D" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "AK", "Alaska", "1A448D09-E08D-43C0-A8E0-55955939C205" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "AS", "American Samoa", "D2EB1B74-DF74-4644-A1BC-A0F3C0B48855" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "AZ", "Arizona", "8C5902AB-1021-4059-B10E-CAD7E9634921" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "AR", "Arkansas", "9EBF321A-D9CF-42C4-8A87-BEE42778B51D" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "CA", "California", "1558AA93-189E-43AB-8528-CF012E944D76" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "CO", "Colorado", "EFDC2027-E7C1-436E-9522-701A1D305768" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "CT", "Connecticut", "60ECE236-27B9-45FE-B0FC-80EA4902A787" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "DE", "Deleware", "C6D6865E-B78B-4565-9836-A63EB27C8EC6" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "DC", "District of Columbia", "88491B92-D9A4-4A29-A0B7-6BB23DA82176" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "FM", "Federated States of Micronesia", "0E97E2D3-B056-4F1B-9D0E-0AB496262BDA" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "FL", "Florida", "E34B9846-1CA5-4580-9F38-64AB41D5DE25" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "GA", "Georgia", "D63E0285-0C66-4247-AAAD-2C122C916459" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "GU", "Guam", "F38C12C8-BDFB-4084-99F9-6EB825748B63" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "HI", "Hawaii", "A8C16D6D-9F43-4EF6-B747-A639FC29580E" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "ID", "Idaho", "BA8C5035-381D-4439-A704-8940465A3DBF" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "IL", "Illinois", "4E6491DF-362E-4B7E-B447-9F534CCF2958" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "IN", "Indiana", "4B44EB06-993D-415D-8582-72E522006EE5" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "IA", "Iowa", "5F077A88-6F8C-4606-8904-486811A88414" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "KS", "Kansas", "6AFA5AFE-56E1-448B-839A-D29D99DF581E" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "KY", "Kentucky", "201188DA-A73A-45CE-BCB7-C0895F357FD2" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "LA", "Louisiana", "3B6E5B3A-4653-42F2-B54A-E454D2E25FF4" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "ME", "Maine", "81C31993-06AB-4210-BD4F-938A2BAFF293" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MH", "Marshall Islands", "E0119206-261F-4DE4-8115-5C288CCFE27E" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MD", "Maryland", "BCACEAAA-C3A5-4DE9-B5E6-9CCF9FDF0B60" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MA", "Massachusetts", "D3D1B088-B760-4163-B027-1887C2E4A140" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MI", "Michigan", "6033F70B-0F21-485C-BA5B-C150AC70533A" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MN", "Minnesota", "FFCE1258-0875-48F7-9080-BEE722294FB3" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MS", "Mississippi", "F6A0A845-A186-489B-9361-82A787E445A0" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MO", "Missouri", "7FBC063F-C729-4339-B974-C684E74D69DE" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MT", "Montana", "BC6B24EB-D1BE-4454-AF64-AA7032501B3D" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NE", "Nebraska", "FD0C1A69-5AAD-4263-8319-61A45078E46F" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NV", "Nevada", "BCB272AD-4A28-4B77-8CD9-580F990B7EB8" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NH", "New Hampshire", "610C2D7A-DDA9-40E9-8E89-A43404221995" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NJ", "New Jersey", "06549574-9330-42F2-A2C3-0F00C3C24181" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NM", "New Mexico", "7A328958-C04B-4B99-8A10-4CD4794CDDE5" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NY", "New York", "578C88A2-6231-4B82-9167-26F5C077C4C8" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NC", "North Carolina", "5E39C0E8-752A-45EC-A040-0421A106616C" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "ND", "North Dakota", "8E6B40BB-0706-42A3-ABCA-9F6E34196D79" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MP", "Northern Mariana Islands", "A788BCEA-0B66-4145-BA99-7C90BA5F6084" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "OH", "Ohio", "8FAA8F87-DB72-46A6-B2BF-E799011ED0C6" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "OK", "Oklahoma", "A8018B10-C1A6-4D0D-A6E4-ECF8D0167D77" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "OR", "Oregon", "C0E9361E-18F7-4408-95A9-E726EE3C5259" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "PW", "Palau", "FD35A352-5F20-4A40-82A6-8AA8C78C47D1" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "PA", "Pennsylvania", "2A59EC01-BE2F-4A44-8957-AC3DBD1385F5" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "PR", "Puerto Rico", "936D1A2E-4C42-4181-9AD3-D6CF5AF43097" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "RI", "Rhode Island", "CBCC2EBE-B1EC-489F-8F13-F2BE389538E5" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "SC", "South Carolina", "966BB348-F1E1-4BE3-8AE1-FDBEF0D92076" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "SD", "South Dakota", "06B0329A-BDC1-4FE0-9561-D6D7DD5E71D7" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "TN", "Tennessee", "E6B3E307-AC8F-49EF-B9BD-C6FB86580A35" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "TX", "Texas", "8C1EEB0D-D973-4495-A0E7-C231C3FFBBA7" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "UT", "Utah", "3C3FC00F-BC24-480C-9681-EA4CE980DA98" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "VT", "Vermont", "0C3FAE1F-46D2-470A-88AD-DEF38A48ABE0" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "VI", "Virgin Islands", "90402955-BF85-4FDD-B091-FC026A0AB29B" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "VA", "Virginia", "8183BB65-D255-4CFE-A21F-201D5EEC85B3" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "WA", "Washington", "563966AA-9E31-4647-A463-33929D1BFFD1" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "WV", "West Virginia", "B4C5BC76-34DB-47F4-BC3C-D7CED9E0DF4B" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "WI", "Wisconsin", "D0D48D02-0A8B-4416-9963-8D871BDFCF5E" );
            AddDefinedValue( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "WY", "Wyoming", "EE33C331-00FD-4225-8579-8020C4093774" );

            // Updating Public-Facing Giving block attributes to use new AccountsField attribute
            AddBlockTypeAttribute( "4A2AA794-A968-4CCD-973A-C90FD589996F", "17033CDD-EF97-4413-A483-7B85A787A87F", "Default Accounts to display", "DefaultAccounts", "Payments", "Which accounts should be displayed by default?", 3, "", "DFA69848-2AE0-4308-A350-7BB4CAF12BD1" );
            AddBlockTypeAttribute( "F679692F-133E-4F57-9072-D87C675C3283", "17033CDD-EF97-4413-A483-7B85A787A87F", "Default Accounts to display", "DefaultAccounts", "Payments", "Which accounts should be displayed by default?", 3, "", "C6910E1C-CF53-4BAD-9389-F6AC975AC5AD" );
            // Attrib Value for One Time Gift:Default Accounts to display
            AddBlockAttributeValue("3BFFEDFD-2198-4A13-827A-4FD1A774949E","DFA69848-2AE0-4308-A350-7BB4CAF12BD1","67c6181c-1d8c-44d7-b262-b81e746f06d8,bab250ee-cae6-4a41-9756-ad9327408be0");  
            // Attrib Value for Recurring Gift:Default Accounts to display
            AddBlockAttributeValue("0F17BF49-A6D5-47C3-935A-B050127EA939","C6910E1C-CF53-4BAD-9389-F6AC975AC5AD","67c6181c-1d8c-44d7-b262-b81e746f06d8,bab250ee-cae6-4a41-9756-ad9327408be0");  
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "DFA69848-2AE0-4308-A350-7BB4CAF12BD1" ); // Default Accounts to display
            DeleteAttribute( "C6910E1C-CF53-4BAD-9389-F6AC975AC5AD" ); // Default Accounts to display
            DeleteDefinedType( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB" ); // Location - Address State
        }
    }
}
