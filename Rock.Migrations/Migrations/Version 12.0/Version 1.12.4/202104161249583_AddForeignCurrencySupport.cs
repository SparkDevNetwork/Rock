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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddForeignCurrencySupport : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransaction", "ForeignCurrencyCodeValueId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "PreferredForeignCurrencyCodeValueId", c => c.Int());
            AddColumn("dbo.FinancialScheduledTransaction", "ForeignCurrencyCodeValueId", c => c.Int());
            AddColumn("dbo.FinancialTransactionDetail", "ForeignCurrencyAmount", c => c.Decimal(precision: 18, scale: 2));

            RockMigrationHelper.AddDefinedType( "Financial", "Currency Code", "The ISO 4217 three-letter Currency Code is based on ISO 3166, which lists the codes for country names. The first two letters of the ISO 4217 three-letter code are the same as the code for the country name, and, where possible, the third letter corresponds to the first letter of the currency name.", Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, @"" );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "AED", "United Arab Emirates Dirham ", "E8311BFD-DFD6-4942-A353-C9D1048E8358", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "AFN", "Afghanistan Afghani ", "3F473DE6-435E-408D-A938-A38E48970B71", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "ALL", "Albania Lek", "8C1E2929-B814-49BD-8CCD-864A451726BA", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "AMD", "Armenia Dram ", "8BEB88F3-4615-4D66-B687-12CD49C133C0", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "ANG", "Netherlands Antilled Guilder ", "492EE19A-8E63-4CFE-B181-500966703FFC", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "AOA", "Angola Kwanza ", "57C9DCB6-181B-47B7-A8A0-393A3F282DBD", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "ARS", "Argentina Peso ", "11E670BC-5F1C-43FD-BB36-710BE2E7EA9C", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "AUD", "Australia Dollar ", "D385F680-74F5-4FDF-9F05-F36611B811C2", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "AWG", "Aruba Guilder ", "EF064E62-0F3F-48FE-805E-F75B0F6CC208", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "AZN", "Azerbaijan Manat", "68C599F4-41E5-4F0C-902A-A51C3A23EA80", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BAM", "Bosnia and Herzegovina Convertible Mark ", "99527366-8BE1-4DC6-A19B-D661925C22F6", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BBD", "Barbados Dollar ", "2BC04B92-52C7-4297-BE1E-EF2D2F4D872E", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BDT", "Bangladesh Taka ", "4BD2F195-1EAE-47DD-9200-A5CB42BA026D", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BGN", "Bulgaria Lev", "7CFB179B-CD1F-4007-A73E-3B3AEB70BA80", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BIF", "Bahrain Dinar", "8AE9766C-226F-46F5-9511-132018937540", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BMD", "Burundi Franc ", "107F3CE0-15EF-4B5B-A04C-F16B02897514", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BND", "Bermuda Dollar", "39061690-23A9-4AC0-972A-70C8348C2F13", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BOB", "Bolivia Boliviano ", "E9A0DD4A-7A60-4E18-BC9C-1E5CDF08C56F", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BRL", "Brazil Real ", "CA3B180E-3A4B-4DBF-8C09-5A36FFCEBC8B", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BSD", "Bahamas Dollar", "097D05AF-C8B7-4B7B-9B83-E87EB66F2557", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BWP", "Botswana Pula ", "40DB2AE9-2A54-4E73-9A07-67D3D7323D38", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "BZD", "Belize Dollar ", "9B369C03-F310-45FB-B9BD-CFAC380CF0A1", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "CAD", "Canada Dollar ", "C1F19493-15D6-4247-8BCD-C5062D08AA3A", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "CDF", "Congo/Kinshasa Franc", "DBAAC8AB-3CFA-4EA5-A468-9FCB3F58D32A", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "CHF", "Switzerland Franc", "1169A8EC-76F2-4470-AC6C-B669AE1CE335", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "CLP", "Chile Peso", "CF801143-791B-4057-84DD-F68E3E6A0096", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "CNY", "China Yuan Renminbi", "DC070450-95E0-474A-979C-958EB45D5B6D", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "COP", "Colombia Peso", "B2FE77B6-34BA-46BE-A21B-B844E4BBD9E7", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "CRC", "Costa Rica Colon ", "6A2CA4D2-65AA-427F-AE66-87D41C2C9DF1", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "CVE", "Cape Verde Escudo", "8E9EE059-69A5-4A24-A603-65DC8B923B42", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "CZK", "Czech Republic Koruna", "91F5F7BC-A0B7-4B84-BC5C-4E65D6CE138B", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "DJF", "Djibouti Franc", "C0301DF3-07A3-4FC0-97E2-EB49756C0719", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "DKK", "Denmark Krone ", "B42FEC19-9FF1-4B97-853D-46416F894A3E", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "DOP", "Dominican Republic Peso", "B438A909-7D48-4EAE-AD61-FD2AF6856AE1", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "DZD", "Algeria Dinar", "DA7291E8-3267-4195-A234-5A4A0FC76BDC", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "EGP", "Egypt Pound", "9B8A06CE-975A-4F8F-94B8-2B82CA736A8B", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "ETB", "Ethiopia Birr", "B8B922AF-6410-41A1-8C7C-0D1BD2A233EF", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "EUR", "Euro Member Countries ", "4BAF371B-D6EC-4485-BC95-F1ACE8E74A77", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "FJD", "Fiji Dollar ", "2F2410AF-F716-4987-9273-6F8C2137F196", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "FKP", "Falkland Islands (Malvinas) Pound", "A78E15BD-5219-4F5C-A96D-F0BA59E09B1E", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "GBP", "United Kingdom Pound", "2D90B494-6575-45F0-9A85-AF44CAD352EE", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "GEL", "Georgia Lari", "99B6448F-663C-4338-8A00-A993BB2F9D42", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "GIP", "Gibraltar Pound", "F61FAAC0-BD13-4DDD-8C62-B4B7CFAFB881", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "GMD", "Gambia Dalasi", "61B9F4BE-5E7D-44B6-8D7C-C9A049B693EE", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "GNF", "Guinea Franc", "6D66A95D-5AA7-48E4-802C-0BA9C5DB92D6", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "GTQ", "Guatemala Quetzal ", "407301C4-529B-4356-BC1A-ACD7D2106708", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "GYD", "Guyana Dollar", "17F3E292-D93F-4E02-AC6A-7288610C86E7", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "HKD", "Hong Kong Dollar", "F3A39E0D-FE32-44EC-A59B-2687DAA3DFB3", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "HNL", "Honduras Lempira", "2EAED942-07F6-4EA0-BFD0-5DBCBB35569F", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "HRK", "Croatia Dollar", "EA1144CF-9742-427B-A13C-2EFA83EB697E", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "HTG", "Haiti Gourde", "31E405A6-55CE-4EEB-8688-0C4D7B4B9B95", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "HUF", "Hungary Forint", "CA51CD64-68CC-470B-B1B2-7FD4C496D0B0", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "IDR", "Indonesia Rupiah", "9A2FD6B8-2D64-4ED3-933E-C8A8E437C9DE", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "ILS", "Israel Shekel", "33624EEB-BECB-4297-92A3-C0121B5B0806", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "INR", "India Rupee", "12F8FEF2-F5E4-4BA0-BF0F-A7DC57D095E9", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "ISK", "Iceland Krona ", "9AA6B574-77E5-4CD0-A109-5D31EF51C90B", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "JMD", "Jamaica Dollar", "0228B0DF-EC2E-41D5-87E3-56C53E4F36B9", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "JPY", "Japan Yen", "AF28AF43-8461-41AC-A2C5-85122712BE96", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "KES", "Kenya Shilling ", "BB3C284F-256C-4A6C-BD60-5FF36A034FC9", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "KGS", "Kyrgyzstan Som ", "6F9F4591-0995-40D9-9973-5FA47645C922", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "KHR", "Cambodia Riel", "BA1D927B-F010-4EBB-922B-A1815C65DEAB", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "KMF", "Comorian Franc", "DE144212-76A7-43D9-8B91-54E0DA5BBA46", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "KRW", "Korea (South) Won", "D64BA50A-BDF1-4F3D-8C86-F897A1F6C4E3", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "KYD", "Cayman Islands Dollar", "6E73E247-21AF-4953-A2EA-427F7F07C930", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "KZT", "Kazakhstan Tenge", "6414A05F-1F59-47D5-AF28-C47036C45581", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "LAK", "Laos Kip", "1F6E4C5E-21BC-4AB4-A8CF-4EE18DA3406E", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "LBP", "Lebanon Pound", "52FD7D31-FB97-47CB-A815-17652A99C651", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "LKR", "Sri Lanka Rupee", "2640B45E-F0DF-41F8-8CBB-806C00869034", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "LRD", "Liberia Dollar", "BDBA6849-F172-4A9A-9750-53448115544E", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "LSL", "Lesotho Loti", "964891DE-852C-48D5-A3ED-B56B4BE502AE", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MAD", "Morocco Dirham", "4E165A63-D75C-4DCF-866C-1A63E70ED6DA", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MDL", "Moldova Leu", "D4B9589B-092C-4EF5-A424-FAA2240B12A5", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MGA", "Madagascar Ariary ", "9EFE3400-BEC2-4FD7-9BED-11BCDB71D0C8", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MKD", "Macedonia Denar", "C12E7C5B-160A-45D3-9F08-A4EBED6BA41C", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MMK", "Myanmar (Burma) Kyat", "70C9DA9F-C74B-42BA-8194-861435C5F874", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MNT", "Mongolia Tughrik", "39C1528E-28D9-409B-A323-660910BF6F9B", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MOP", "Macau Pataca", "6B48892D-C1E7-435B-AE67-07C56E803B3C", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MRU", "Mauritania Ouguiya", "80333F0D-0A1A-40A3-846D-A4138DF42BED", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MUR", "Mauritius Rupee", "172E6CDD-95B5-4FC0-81B0-E63809EBB629", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MVR", "Maldives (Maldive Islands) Rufiyaa", "95D1883F-5519-4B3B-9717-E47AB2033A0A", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MWK", "Malawi Kwacha", "71475757-7BB4-4E90-BB46-B82F866CCB98", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MXN", "Mexico Kwacha", "C1AE0E8E-8C5F-45FB-8A88-000794C46CDB", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MYR", "Malaysia Ringgit", "FA188643-6407-46E3-9D4F-7455117C1377", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "MZN", "Mozambiqui Metical", "D5605D91-F9C4-494A-B4DA-3D4CCF89315D", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "NAD", "Namibia Dollar", "EEF22CB2-643C-439D-965B-10E93F9054FF", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "NGN", "Nigeria Naira", "BA71D131-4BDB-40CA-A40D-2AE01C4CDE32", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "NIO", "Nicaragua Cordoba", "4B6E4323-A26D-407F-A73C-FD33672B96BF", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "NOK", "Norway Krone", "35636979-6E39-4AAB-A2C2-F7718BC6574B", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "NPR", "Nepal Rupee", "A82D1B0D-2D77-44B0-9319-DF89D3C2C102", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "NZD", "New Zealand Dollar", "AE369700-7B98-4360-B477-E3BFEB0BE70A", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "PAB", "Panama Balboa", "F86BFA5D-196E-4B95-8575-AED565A01F40", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "PEN", "Peru Sol", "7FDAD2C4-53BB-448D-8C9C-611AA5A721AF", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "PGK", "Paupa New Guinea Kina", "B1814620-4F8A-4291-B6A0-99051EC5F13B", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "PHP", "Philippines Peso", "9806539A-D44B-4D75-8F15-FEFBDA20DA3F", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "PKR", "Pakistan Rupee", "F646D37D-6790-4F1F-BE4F-CC2E63711895", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "PLN", "Poland Zloty ", "CBE89C67-1E1D-4B10-A5B9-2180F690FD9A", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "PYG", "Paraguay Guarani", "08D73EDD-CF8A-42CF-B057-128F13A23A55", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "QAR", "Qatar Riyal", "66B75081-F3CC-47CD-B032-CA7964F78026", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "RON", "Romania Leu", "3E424D69-D4D7-4C2B-8A58-72424FC4A4E6", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "RSD", "Serbia Dinar", "2E2D91C4-1967-4F06-AAFA-B1A8D095F716", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "RUB", "Russia Ruble", "76D46F48-6F2E-4B58-AE76-50FDC638984F", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "RWF", "Rwanda Franc", "9CA1DE62-2A58-4FA1-A5C5-D34D8A6F7742", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SAR", "Saudi Arabia Riyal", "7DF4B48B-734E-44C0-991C-9154A8F7D704", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SBD", "Solomon Islands Dollar", "8B1F131D-625D-43B5-B0FB-BC3AE11EDE1A", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SCR", "Seychelles Rupee", "DE5B866A-D217-43F2-854C-F88A58A9CD74", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SEK", "Sweden Krona", "E34F8B89-A400-4839-9C9F-63DE6D91EE97", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SGD", "Singapore Dollar", "28EC8C33-7445-4EA3-9D62-264CD44A2045", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SHP", "Sain Helena Pound", "E52D57D1-2A04-4142-8244-1A23F63063D1", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SLL", "Sierra Leone Leone", "8D30F49F-3C71-40BD-B71E-16B6620652CE", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SOS", "Somalia Shilling", "ED9E9E39-00C5-4F5C-A9A5-D4F7A734EFB5", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SRD", "Suriname Dollar", "C216DA39-AF82-4497-9B90-83BA0CFC314E", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "STN", "Sao Tome and Principe Dobra", "2836C4C2-28A4-493E-B399-71114E26D776", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "SZL", "eSwatini Lilageni", "1F05BD1B-253F-4A6C-BF1D-884FEB1D0FB5", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "THB", "Thailand Baht", "667C5D07-99A2-4181-829D-F5C9ABBFC9D1", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "TJS", "Tajikistan Somoni", "1DA03888-48DB-43E6-BE2D-9A0776BF7F18", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "TOP", "Tonga Pa'anga ", "AAFBDB35-4885-4E80-BADF-B4D73DABD2A7", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "TRY", "Turkey Lira", "EA963A7E-FDC5-49E3-962D-70AC7ABFCECD", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "TTD", "Trinidad and Tobago Dollar", "056A1321-EA55-4D01-86FB-7007CAF7BEF4", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "TWD", "Taiwan New Dollar", "5C5CF899-1291-490B-93DE-7D0FFFA250DF", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "TZS", "Tanzania Shilling ", "416089FE-E4B6-4BF3-BF51-8374B6940B69", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "UAH", "Ukraine Hryvnia", "DDA2F2C4-1F5A-49E3-85A1-BDC4F0D121BE", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "UGX", "Uganda Shilling ", "DD51556E-E327-4305-9CA3-57F2E30FF654", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "USD", "United States Dollar", "0BD42A7E-9C7E-417C-AFFE-51102B1E4B43", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "UYU", "Uruguay Peso", "8E09755B-E47D-4472-9C0A-A16D2032F0D8", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "UZS", "Uzbekistan Som", "D926DEE2-2628-4DDF-AF80-75E2EAB3BC2B", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "VND", "Viet Nam Dong", "37544FBE-1317-4CD0-8DCB-A12A693DB056", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "VUV", "Vanuatu Vatu", "8F686039-89A6-47A7-AA7F-3F8F0BD1E434", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "WST", "Samoa Tala", "AC8716BF-A907-4657-80BD-BBB3FDE77802", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "XAF", "Communaute Financiere Africaine (BEAC) CFA Franc", "84CA3152-374D-4E07-BAF5-554F4CA450C4", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "XCD", "East Caribbean Dollar", "1582E971-543D-4C3B-B6D2-F6C5BE88461E", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "XOF", "Communaute Financiere Africaine (BCEAO) Franc", "85DA3507-E266-4494-BF7F-E9365F6645BD", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "XPF", "Comptoirs Francais du Pacifique (CFP) Franc", "81AE524B-1A16-49F8-99E1-0911897C26AD", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "YER", "Yemen Rial ", "5F96D37D-878B-4A5B-801B-D8C11F5C4DD8", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "ZAR", "South Africa Rand", "148F0F96-A551-45C7-B17C-4AFCB2B6ADDD", true );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, "ZMW", "Zambia Kwacha", "E7FE5E77-A202-4B93-8DE8-EA1DB8D674CC", true );

            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, SystemGuid.FieldType.TEXT, "Symbol", "Symbol", "The symbol the currency code should use. For example $.", 1, "", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E8311BFD-DFD6-4942-A353-C9D1048E8358", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "د.إ" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F473DE6-435E-408D-A938-A38E48970B71", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "؋" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8C1E2929-B814-49BD-8CCD-864A451726BA", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Lek" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8BEB88F3-4615-4D66-B687-12CD49C133C0", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "֏" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "492EE19A-8E63-4CFE-B181-500966703FFC", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "ƒ" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "57C9DCB6-181B-47B7-A8A0-393A3F282DBD", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Kz" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "11E670BC-5F1C-43FD-BB36-710BE2E7EA9C", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D385F680-74F5-4FDF-9F05-F36611B811C2", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EF064E62-0F3F-48FE-805E-F75B0F6CC208", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "ƒ" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "68C599F4-41E5-4F0C-902A-A51C3A23EA80", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₼" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99527366-8BE1-4DC6-A19B-D661925C22F6", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "KM" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2BC04B92-52C7-4297-BE1E-EF2D2F4D872E", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4BD2F195-1EAE-47DD-9200-A5CB42BA026D", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Tk" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7CFB179B-CD1F-4007-A73E-3B3AEB70BA80", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "лв" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8AE9766C-226F-46F5-9511-132018937540", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Franc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "107F3CE0-15EF-4B5B-A04C-F16B02897514", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "39061690-23A9-4AC0-972A-70C8348C2F13", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E9A0DD4A-7A60-4E18-BC9C-1E5CDF08C56F", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$b" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CA3B180E-3A4B-4DBF-8C09-5A36FFCEBC8B", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "R$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "097D05AF-C8B7-4B7B-9B83-E87EB66F2557", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "40DB2AE9-2A54-4E73-9A07-67D3D7323D38", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "P" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B369C03-F310-45FB-B9BD-CFAC380CF0A1", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "BZ$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C1F19493-15D6-4247-8BCD-C5062D08AA3A", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DBAAC8AB-3CFA-4EA5-A468-9FCB3F58D32A", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Franc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1169A8EC-76F2-4470-AC6C-B669AE1CE335", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "CHF" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CF801143-791B-4057-84DD-F68E3E6A0096", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DC070450-95E0-474A-979C-958EB45D5B6D", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "¥" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B2FE77B6-34BA-46BE-A21B-B844E4BBD9E7", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2CA4D2-65AA-427F-AE66-87D41C2C9DF1", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₡" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8E9EE059-69A5-4A24-A603-65DC8B923B42", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "91F5F7BC-A0B7-4B84-BC5C-4E65D6CE138B", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Kč" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C0301DF3-07A3-4FC0-97E2-EB49756C0719", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Franc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B42FEC19-9FF1-4B97-853D-46416F894A3E", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "kr." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B438A909-7D48-4EAE-AD61-FD2AF6856AE1", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "RD$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DA7291E8-3267-4195-A234-5A4A0FC76BDC", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "DA" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B8A06CE-975A-4F8F-94B8-2B82CA736A8B", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "£" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B8B922AF-6410-41A1-8C7C-0D1BD2A233EF", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Br" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4BAF371B-D6EC-4485-BC95-F1ACE8E74A77", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "€" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2F2410AF-F716-4987-9273-6F8C2137F196", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A78E15BD-5219-4F5C-A96D-F0BA59E09B1E", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "£" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2D90B494-6575-45F0-9A85-AF44CAD352EE", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "£" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99B6448F-663C-4338-8A00-A993BB2F9D42", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Lari" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F61FAAC0-BD13-4DDD-8C62-B4B7CFAFB881", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "£" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "61B9F4BE-5E7D-44B6-8D7C-C9A049B693EE", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Dalasi" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6D66A95D-5AA7-48E4-802C-0BA9C5DB92D6", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Franc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "407301C4-529B-4356-BC1A-ACD7D2106708", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Q" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "17F3E292-D93F-4E02-AC6A-7288610C86E7", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F3A39E0D-FE32-44EC-A59B-2687DAA3DFB3", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2EAED942-07F6-4EA0-BFD0-5DBCBB35569F", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "L" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EA1144CF-9742-427B-A13C-2EFA83EB697E", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "kn" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "31E405A6-55CE-4EEB-8688-0C4D7B4B9B95", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "G" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CA51CD64-68CC-470B-B1B2-7FD4C496D0B0", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Ft" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9A2FD6B8-2D64-4ED3-933E-C8A8E437C9DE", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Rp" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "33624EEB-BECB-4297-92A3-C0121B5B0806", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₪" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "12F8FEF2-F5E4-4BA0-BF0F-A7DC57D095E9", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₹" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9AA6B574-77E5-4CD0-A109-5D31EF51C90B", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "kr" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0228B0DF-EC2E-41D5-87E3-56C53E4F36B9", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "J$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AF28AF43-8461-41AC-A2C5-85122712BE96", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "¥" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BB3C284F-256C-4A6C-BD60-5FF36A034FC9", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "KSh" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6F9F4591-0995-40D9-9973-5FA47645C922", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "лв" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BA1D927B-F010-4EBB-922B-A1815C65DEAB", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "៛" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DE144212-76A7-43D9-8B91-54E0DA5BBA46", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Franc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D64BA50A-BDF1-4F3D-8C86-F897A1F6C4E3", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₩" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6E73E247-21AF-4953-A2EA-427F7F07C930", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6414A05F-1F59-47D5-AF28-C47036C45581", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₸" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1F6E4C5E-21BC-4AB4-A8CF-4EE18DA3406E", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₭" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "52FD7D31-FB97-47CB-A815-17652A99C651", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "ل.ل" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2640B45E-F0DF-41F8-8CBB-806C00869034", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₨" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BDBA6849-F172-4A9A-9750-53448115544E", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "964891DE-852C-48D5-A3ED-B56B4BE502AE", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Loti" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4E165A63-D75C-4DCF-866C-1A63E70ED6DA", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Dirham" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D4B9589B-092C-4EF5-A424-FAA2240B12A5", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Leu" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9EFE3400-BEC2-4FD7-9BED-11BCDB71D0C8", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Ar" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C12E7C5B-160A-45D3-9F08-A4EBED6BA41C", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "ден" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "70C9DA9F-C74B-42BA-8194-861435C5F874", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "K" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "39C1528E-28D9-409B-A323-660910BF6F9B", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₮" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6B48892D-C1E7-435B-AE67-07C56E803B3C", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "MOP$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "80333F0D-0A1A-40A3-846D-A4138DF42BED", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "UM" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "172E6CDD-95B5-4FC0-81B0-E63809EBB629", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₨" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "95D1883F-5519-4B3B-9717-E47AB2033A0A", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Rufiyaa" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "71475757-7BB4-4E90-BB46-B82F866CCB98", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "MK" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C1AE0E8E-8C5F-45FB-8A88-000794C46CDB", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA188643-6407-46E3-9D4F-7455117C1377", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "RM" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D5605D91-F9C4-494A-B4DA-3D4CCF89315D", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "MT" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EEF22CB2-643C-439D-965B-10E93F9054FF", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BA71D131-4BDB-40CA-A40D-2AE01C4CDE32", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₦" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4B6E4323-A26D-407F-A73C-FD33672B96BF", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "C$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "35636979-6E39-4AAB-A2C2-F7718BC6574B", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "kr" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A82D1B0D-2D77-44B0-9319-DF89D3C2C102", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₨" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AE369700-7B98-4360-B477-E3BFEB0BE70A", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F86BFA5D-196E-4B95-8575-AED565A01F40", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "B/." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7FDAD2C4-53BB-448D-8C9C-611AA5A721AF", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "S/." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B1814620-4F8A-4291-B6A0-99051EC5F13B", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "K" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9806539A-D44B-4D75-8F15-FEFBDA20DA3F", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₱" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F646D37D-6790-4F1F-BE4F-CC2E63711895", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₨" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CBE89C67-1E1D-4B10-A5B9-2180F690FD9A", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "zł" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "08D73EDD-CF8A-42CF-B057-128F13A23A55", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Gs" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "66B75081-F3CC-47CD-B032-CA7964F78026", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "﷼" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3E424D69-D4D7-4C2B-8A58-72424FC4A4E6", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "lei" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2E2D91C4-1967-4F06-AAFA-B1A8D095F716", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "РСД" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "76D46F48-6F2E-4B58-AE76-50FDC638984F", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₽" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9CA1DE62-2A58-4FA1-A5C5-D34D8A6F7742", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Franc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7DF4B48B-734E-44C0-991C-9154A8F7D704", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "﷼" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B1F131D-625D-43B5-B0FB-BC3AE11EDE1A", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DE5B866A-D217-43F2-854C-F88A58A9CD74", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₨" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E34F8B89-A400-4839-9C9F-63DE6D91EE97", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "kr" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "28EC8C33-7445-4EA3-9D62-264CD44A2045", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E52D57D1-2A04-4142-8244-1A23F63063D1", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "£" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8D30F49F-3C71-40BD-B71E-16B6620652CE", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Le" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "ED9E9E39-00C5-4F5C-A9A5-D4F7A734EFB5", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "S" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C216DA39-AF82-4497-9B90-83BA0CFC314E", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2836C4C2-28A4-493E-B399-71114E26D776", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Db" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1F05BD1B-253F-4A6C-BF1D-884FEB1D0FB5", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Lilangeni" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "667C5D07-99A2-4181-829D-F5C9ABBFC9D1", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "฿" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1DA03888-48DB-43E6-BE2D-9A0776BF7F18", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Somoni" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AAFBDB35-4885-4E80-BADF-B4D73DABD2A7", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "T$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EA963A7E-FDC5-49E3-962D-70AC7ABFCECD", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "TL" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "056A1321-EA55-4D01-86FB-7007CAF7BEF4", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "TT$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5C5CF899-1291-490B-93DE-7D0FFFA250DF", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "NT$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "416089FE-E4B6-4BF3-BF51-8374B6940B69", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Shilling" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DDA2F2C4-1F5A-49E3-85A1-BDC4F0D121BE", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₴" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DD51556E-E327-4305-9CA3-57F2E30FF654", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "UGX" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0BD42A7E-9C7E-417C-AFFE-51102B1E4B43", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8E09755B-E47D-4472-9C0A-A16D2032F0D8", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$U" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D926DEE2-2628-4DDF-AF80-75E2EAB3BC2B", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "лв" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "37544FBE-1317-4CD0-8DCB-A12A693DB056", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "₫" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8F686039-89A6-47A7-AA7F-3F8F0BD1E434", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "VT" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC8716BF-A907-4657-80BD-BBB3FDE77802", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84CA3152-374D-4E07-BAF5-554F4CA450C4", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "FCFA" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1582E971-543D-4C3B-B6D2-F6C5BE88461E", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "$" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85DA3507-E266-4494-BF7F-E9365F6645BD", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Franc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "81AE524B-1A16-49F8-99E1-0911897C26AD", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "Franc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5F96D37D-878B-4A5B-801B-D8C11F5C4DD8", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "﷼" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "148F0F96-A551-45C7-B17C-4AFCB2B6ADDD", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "R" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E7FE5E77-A202-4B93-8DE8-EA1DB8D674CC", SystemGuid.Attribute.CURRENCY_CODE_SYMBOL, "ZK" );

            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, SystemGuid.FieldType.TEXT, "Position", "Position", "The position relative to the number. 'Left' for the left side, 'Right' for the right side of the number.", 2, "", SystemGuid.Attribute.CURRENCY_CODE_POSITION );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E8311BFD-DFD6-4942-A353-C9D1048E8358", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F473DE6-435E-408D-A938-A38E48970B71", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8C1E2929-B814-49BD-8CCD-864A451726BA", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8BEB88F3-4615-4D66-B687-12CD49C133C0", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "492EE19A-8E63-4CFE-B181-500966703FFC", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "57C9DCB6-181B-47B7-A8A0-393A3F282DBD", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "11E670BC-5F1C-43FD-BB36-710BE2E7EA9C", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D385F680-74F5-4FDF-9F05-F36611B811C2", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EF064E62-0F3F-48FE-805E-F75B0F6CC208", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "68C599F4-41E5-4F0C-902A-A51C3A23EA80", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99527366-8BE1-4DC6-A19B-D661925C22F6", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2BC04B92-52C7-4297-BE1E-EF2D2F4D872E", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left " );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4BD2F195-1EAE-47DD-9200-A5CB42BA026D", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7CFB179B-CD1F-4007-A73E-3B3AEB70BA80", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8AE9766C-226F-46F5-9511-132018937540", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "107F3CE0-15EF-4B5B-A04C-F16B02897514", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "39061690-23A9-4AC0-972A-70C8348C2F13", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E9A0DD4A-7A60-4E18-BC9C-1E5CDF08C56F", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CA3B180E-3A4B-4DBF-8C09-5A36FFCEBC8B", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "097D05AF-C8B7-4B7B-9B83-E87EB66F2557", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "40DB2AE9-2A54-4E73-9A07-67D3D7323D38", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B369C03-F310-45FB-B9BD-CFAC380CF0A1", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C1F19493-15D6-4247-8BCD-C5062D08AA3A", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DBAAC8AB-3CFA-4EA5-A468-9FCB3F58D32A", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1169A8EC-76F2-4470-AC6C-B669AE1CE335", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CF801143-791B-4057-84DD-F68E3E6A0096", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DC070450-95E0-474A-979C-958EB45D5B6D", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B2FE77B6-34BA-46BE-A21B-B844E4BBD9E7", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2CA4D2-65AA-427F-AE66-87D41C2C9DF1", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8E9EE059-69A5-4A24-A603-65DC8B923B42", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "91F5F7BC-A0B7-4B84-BC5C-4E65D6CE138B", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C0301DF3-07A3-4FC0-97E2-EB49756C0719", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B42FEC19-9FF1-4B97-853D-46416F894A3E", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B438A909-7D48-4EAE-AD61-FD2AF6856AE1", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DA7291E8-3267-4195-A234-5A4A0FC76BDC", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B8A06CE-975A-4F8F-94B8-2B82CA736A8B", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B8B922AF-6410-41A1-8C7C-0D1BD2A233EF", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4BAF371B-D6EC-4485-BC95-F1ACE8E74A77", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2F2410AF-F716-4987-9273-6F8C2137F196", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A78E15BD-5219-4F5C-A96D-F0BA59E09B1E", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2D90B494-6575-45F0-9A85-AF44CAD352EE", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99B6448F-663C-4338-8A00-A993BB2F9D42", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F61FAAC0-BD13-4DDD-8C62-B4B7CFAFB881", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "61B9F4BE-5E7D-44B6-8D7C-C9A049B693EE", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6D66A95D-5AA7-48E4-802C-0BA9C5DB92D6", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "407301C4-529B-4356-BC1A-ACD7D2106708", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "17F3E292-D93F-4E02-AC6A-7288610C86E7", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F3A39E0D-FE32-44EC-A59B-2687DAA3DFB3", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2EAED942-07F6-4EA0-BFD0-5DBCBB35569F", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EA1144CF-9742-427B-A13C-2EFA83EB697E", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "31E405A6-55CE-4EEB-8688-0C4D7B4B9B95", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CA51CD64-68CC-470B-B1B2-7FD4C496D0B0", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9A2FD6B8-2D64-4ED3-933E-C8A8E437C9DE", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "33624EEB-BECB-4297-92A3-C0121B5B0806", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "12F8FEF2-F5E4-4BA0-BF0F-A7DC57D095E9", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9AA6B574-77E5-4CD0-A109-5D31EF51C90B", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0228B0DF-EC2E-41D5-87E3-56C53E4F36B9", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AF28AF43-8461-41AC-A2C5-85122712BE96", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BB3C284F-256C-4A6C-BD60-5FF36A034FC9", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6F9F4591-0995-40D9-9973-5FA47645C922", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BA1D927B-F010-4EBB-922B-A1815C65DEAB", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DE144212-76A7-43D9-8B91-54E0DA5BBA46", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D64BA50A-BDF1-4F3D-8C86-F897A1F6C4E3", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6E73E247-21AF-4953-A2EA-427F7F07C930", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6414A05F-1F59-47D5-AF28-C47036C45581", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1F6E4C5E-21BC-4AB4-A8CF-4EE18DA3406E", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "52FD7D31-FB97-47CB-A815-17652A99C651", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2640B45E-F0DF-41F8-8CBB-806C00869034", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BDBA6849-F172-4A9A-9750-53448115544E", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "964891DE-852C-48D5-A3ED-B56B4BE502AE", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4E165A63-D75C-4DCF-866C-1A63E70ED6DA", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D4B9589B-092C-4EF5-A424-FAA2240B12A5", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9EFE3400-BEC2-4FD7-9BED-11BCDB71D0C8", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C12E7C5B-160A-45D3-9F08-A4EBED6BA41C", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "70C9DA9F-C74B-42BA-8194-861435C5F874", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "39C1528E-28D9-409B-A323-660910BF6F9B", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6B48892D-C1E7-435B-AE67-07C56E803B3C", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "80333F0D-0A1A-40A3-846D-A4138DF42BED", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "172E6CDD-95B5-4FC0-81B0-E63809EBB629", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "95D1883F-5519-4B3B-9717-E47AB2033A0A", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "71475757-7BB4-4E90-BB46-B82F866CCB98", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C1AE0E8E-8C5F-45FB-8A88-000794C46CDB", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA188643-6407-46E3-9D4F-7455117C1377", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D5605D91-F9C4-494A-B4DA-3D4CCF89315D", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EEF22CB2-643C-439D-965B-10E93F9054FF", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BA71D131-4BDB-40CA-A40D-2AE01C4CDE32", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4B6E4323-A26D-407F-A73C-FD33672B96BF", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "35636979-6E39-4AAB-A2C2-F7718BC6574B", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A82D1B0D-2D77-44B0-9319-DF89D3C2C102", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AE369700-7B98-4360-B477-E3BFEB0BE70A", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F86BFA5D-196E-4B95-8575-AED565A01F40", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7FDAD2C4-53BB-448D-8C9C-611AA5A721AF", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B1814620-4F8A-4291-B6A0-99051EC5F13B", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9806539A-D44B-4D75-8F15-FEFBDA20DA3F", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F646D37D-6790-4F1F-BE4F-CC2E63711895", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CBE89C67-1E1D-4B10-A5B9-2180F690FD9A", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "08D73EDD-CF8A-42CF-B057-128F13A23A55", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "66B75081-F3CC-47CD-B032-CA7964F78026", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3E424D69-D4D7-4C2B-8A58-72424FC4A4E6", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2E2D91C4-1967-4F06-AAFA-B1A8D095F716", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "76D46F48-6F2E-4B58-AE76-50FDC638984F", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9CA1DE62-2A58-4FA1-A5C5-D34D8A6F7742", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7DF4B48B-734E-44C0-991C-9154A8F7D704", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B1F131D-625D-43B5-B0FB-BC3AE11EDE1A", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DE5B866A-D217-43F2-854C-F88A58A9CD74", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E34F8B89-A400-4839-9C9F-63DE6D91EE97", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "28EC8C33-7445-4EA3-9D62-264CD44A2045", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E52D57D1-2A04-4142-8244-1A23F63063D1", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8D30F49F-3C71-40BD-B71E-16B6620652CE", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "ED9E9E39-00C5-4F5C-A9A5-D4F7A734EFB5", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C216DA39-AF82-4497-9B90-83BA0CFC314E", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2836C4C2-28A4-493E-B399-71114E26D776", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1F05BD1B-253F-4A6C-BF1D-884FEB1D0FB5", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "667C5D07-99A2-4181-829D-F5C9ABBFC9D1", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1DA03888-48DB-43E6-BE2D-9A0776BF7F18", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AAFBDB35-4885-4E80-BADF-B4D73DABD2A7", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EA963A7E-FDC5-49E3-962D-70AC7ABFCECD", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "056A1321-EA55-4D01-86FB-7007CAF7BEF4", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5C5CF899-1291-490B-93DE-7D0FFFA250DF", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "416089FE-E4B6-4BF3-BF51-8374B6940B69", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DDA2F2C4-1F5A-49E3-85A1-BDC4F0D121BE", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DD51556E-E327-4305-9CA3-57F2E30FF654", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0BD42A7E-9C7E-417C-AFFE-51102B1E4B43", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8E09755B-E47D-4472-9C0A-A16D2032F0D8", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D926DEE2-2628-4DDF-AF80-75E2EAB3BC2B", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "37544FBE-1317-4CD0-8DCB-A12A693DB056", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8F686039-89A6-47A7-AA7F-3F8F0BD1E434", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC8716BF-A907-4657-80BD-BBB3FDE77802", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84CA3152-374D-4E07-BAF5-554F4CA450C4", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1582E971-543D-4C3B-B6D2-F6C5BE88461E", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85DA3507-E266-4494-BF7F-E9365F6645BD", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "81AE524B-1A16-49F8-99E1-0911897C26AD", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "let" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5F96D37D-878B-4A5B-801B-D8C11F5C4DD8", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "right" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "148F0F96-A551-45C7-B17C-4AFCB2B6ADDD", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E7FE5E77-A202-4B93-8DE8-EA1DB8D674CC", SystemGuid.Attribute.CURRENCY_CODE_POSITION, "left" );


            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE, SystemGuid.FieldType.INTEGER, "Decimal Places", "DecimalPlaces", "The the number of decimal places the currency should have. For example USD has two decimals.", 3, "2", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E8311BFD-DFD6-4942-A353-C9D1048E8358", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F473DE6-435E-408D-A938-A38E48970B71", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8C1E2929-B814-49BD-8CCD-864A451726BA", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8BEB88F3-4615-4D66-B687-12CD49C133C0", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "492EE19A-8E63-4CFE-B181-500966703FFC", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "57C9DCB6-181B-47B7-A8A0-393A3F282DBD", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "11E670BC-5F1C-43FD-BB36-710BE2E7EA9C", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D385F680-74F5-4FDF-9F05-F36611B811C2", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EF064E62-0F3F-48FE-805E-F75B0F6CC208", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "68C599F4-41E5-4F0C-902A-A51C3A23EA80", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99527366-8BE1-4DC6-A19B-D661925C22F6", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2BC04B92-52C7-4297-BE1E-EF2D2F4D872E", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4BD2F195-1EAE-47DD-9200-A5CB42BA026D", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7CFB179B-CD1F-4007-A73E-3B3AEB70BA80", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8AE9766C-226F-46F5-9511-132018937540", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "107F3CE0-15EF-4B5B-A04C-F16B02897514", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "39061690-23A9-4AC0-972A-70C8348C2F13", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E9A0DD4A-7A60-4E18-BC9C-1E5CDF08C56F", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CA3B180E-3A4B-4DBF-8C09-5A36FFCEBC8B", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "097D05AF-C8B7-4B7B-9B83-E87EB66F2557", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "40DB2AE9-2A54-4E73-9A07-67D3D7323D38", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B369C03-F310-45FB-B9BD-CFAC380CF0A1", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C1F19493-15D6-4247-8BCD-C5062D08AA3A", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DBAAC8AB-3CFA-4EA5-A468-9FCB3F58D32A", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1169A8EC-76F2-4470-AC6C-B669AE1CE335", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CF801143-791B-4057-84DD-F68E3E6A0096", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DC070450-95E0-474A-979C-958EB45D5B6D", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B2FE77B6-34BA-46BE-A21B-B844E4BBD9E7", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2CA4D2-65AA-427F-AE66-87D41C2C9DF1", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8E9EE059-69A5-4A24-A603-65DC8B923B42", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "91F5F7BC-A0B7-4B84-BC5C-4E65D6CE138B", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C0301DF3-07A3-4FC0-97E2-EB49756C0719", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B42FEC19-9FF1-4B97-853D-46416F894A3E", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B438A909-7D48-4EAE-AD61-FD2AF6856AE1", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DA7291E8-3267-4195-A234-5A4A0FC76BDC", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B8A06CE-975A-4F8F-94B8-2B82CA736A8B", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B8B922AF-6410-41A1-8C7C-0D1BD2A233EF", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4BAF371B-D6EC-4485-BC95-F1ACE8E74A77", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2F2410AF-F716-4987-9273-6F8C2137F196", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A78E15BD-5219-4F5C-A96D-F0BA59E09B1E", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2D90B494-6575-45F0-9A85-AF44CAD352EE", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99B6448F-663C-4338-8A00-A993BB2F9D42", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F61FAAC0-BD13-4DDD-8C62-B4B7CFAFB881", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "61B9F4BE-5E7D-44B6-8D7C-C9A049B693EE", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6D66A95D-5AA7-48E4-802C-0BA9C5DB92D6", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "407301C4-529B-4356-BC1A-ACD7D2106708", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "17F3E292-D93F-4E02-AC6A-7288610C86E7", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F3A39E0D-FE32-44EC-A59B-2687DAA3DFB3", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2EAED942-07F6-4EA0-BFD0-5DBCBB35569F", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EA1144CF-9742-427B-A13C-2EFA83EB697E", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "31E405A6-55CE-4EEB-8688-0C4D7B4B9B95", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CA51CD64-68CC-470B-B1B2-7FD4C496D0B0", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9A2FD6B8-2D64-4ED3-933E-C8A8E437C9DE", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "33624EEB-BECB-4297-92A3-C0121B5B0806", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "12F8FEF2-F5E4-4BA0-BF0F-A7DC57D095E9", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9AA6B574-77E5-4CD0-A109-5D31EF51C90B", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0228B0DF-EC2E-41D5-87E3-56C53E4F36B9", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AF28AF43-8461-41AC-A2C5-85122712BE96", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BB3C284F-256C-4A6C-BD60-5FF36A034FC9", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6F9F4591-0995-40D9-9973-5FA47645C922", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BA1D927B-F010-4EBB-922B-A1815C65DEAB", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DE144212-76A7-43D9-8B91-54E0DA5BBA46", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D64BA50A-BDF1-4F3D-8C86-F897A1F6C4E3", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6E73E247-21AF-4953-A2EA-427F7F07C930", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6414A05F-1F59-47D5-AF28-C47036C45581", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1F6E4C5E-21BC-4AB4-A8CF-4EE18DA3406E", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "52FD7D31-FB97-47CB-A815-17652A99C651", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2640B45E-F0DF-41F8-8CBB-806C00869034", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BDBA6849-F172-4A9A-9750-53448115544E", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "964891DE-852C-48D5-A3ED-B56B4BE502AE", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4E165A63-D75C-4DCF-866C-1A63E70ED6DA", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D4B9589B-092C-4EF5-A424-FAA2240B12A5", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9EFE3400-BEC2-4FD7-9BED-11BCDB71D0C8", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C12E7C5B-160A-45D3-9F08-A4EBED6BA41C", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "70C9DA9F-C74B-42BA-8194-861435C5F874", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "39C1528E-28D9-409B-A323-660910BF6F9B", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6B48892D-C1E7-435B-AE67-07C56E803B3C", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "80333F0D-0A1A-40A3-846D-A4138DF42BED", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "172E6CDD-95B5-4FC0-81B0-E63809EBB629", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "95D1883F-5519-4B3B-9717-E47AB2033A0A", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "71475757-7BB4-4E90-BB46-B82F866CCB98", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C1AE0E8E-8C5F-45FB-8A88-000794C46CDB", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA188643-6407-46E3-9D4F-7455117C1377", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D5605D91-F9C4-494A-B4DA-3D4CCF89315D", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EEF22CB2-643C-439D-965B-10E93F9054FF", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BA71D131-4BDB-40CA-A40D-2AE01C4CDE32", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4B6E4323-A26D-407F-A73C-FD33672B96BF", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "35636979-6E39-4AAB-A2C2-F7718BC6574B", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A82D1B0D-2D77-44B0-9319-DF89D3C2C102", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AE369700-7B98-4360-B477-E3BFEB0BE70A", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F86BFA5D-196E-4B95-8575-AED565A01F40", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7FDAD2C4-53BB-448D-8C9C-611AA5A721AF", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B1814620-4F8A-4291-B6A0-99051EC5F13B", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9806539A-D44B-4D75-8F15-FEFBDA20DA3F", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F646D37D-6790-4F1F-BE4F-CC2E63711895", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "CBE89C67-1E1D-4B10-A5B9-2180F690FD9A", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "08D73EDD-CF8A-42CF-B057-128F13A23A55", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "66B75081-F3CC-47CD-B032-CA7964F78026", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3E424D69-D4D7-4C2B-8A58-72424FC4A4E6", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2E2D91C4-1967-4F06-AAFA-B1A8D095F716", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "76D46F48-6F2E-4B58-AE76-50FDC638984F", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9CA1DE62-2A58-4FA1-A5C5-D34D8A6F7742", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7DF4B48B-734E-44C0-991C-9154A8F7D704", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B1F131D-625D-43B5-B0FB-BC3AE11EDE1A", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DE5B866A-D217-43F2-854C-F88A58A9CD74", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E34F8B89-A400-4839-9C9F-63DE6D91EE97", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "28EC8C33-7445-4EA3-9D62-264CD44A2045", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E52D57D1-2A04-4142-8244-1A23F63063D1", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8D30F49F-3C71-40BD-B71E-16B6620652CE", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "ED9E9E39-00C5-4F5C-A9A5-D4F7A734EFB5", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C216DA39-AF82-4497-9B90-83BA0CFC314E", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2836C4C2-28A4-493E-B399-71114E26D776", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1F05BD1B-253F-4A6C-BF1D-884FEB1D0FB5", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "667C5D07-99A2-4181-829D-F5C9ABBFC9D1", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1DA03888-48DB-43E6-BE2D-9A0776BF7F18", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AAFBDB35-4885-4E80-BADF-B4D73DABD2A7", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EA963A7E-FDC5-49E3-962D-70AC7ABFCECD", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "056A1321-EA55-4D01-86FB-7007CAF7BEF4", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5C5CF899-1291-490B-93DE-7D0FFFA250DF", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "416089FE-E4B6-4BF3-BF51-8374B6940B69", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DDA2F2C4-1F5A-49E3-85A1-BDC4F0D121BE", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DD51556E-E327-4305-9CA3-57F2E30FF654", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0BD42A7E-9C7E-417C-AFFE-51102B1E4B43", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8E09755B-E47D-4472-9C0A-A16D2032F0D8", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D926DEE2-2628-4DDF-AF80-75E2EAB3BC2B", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "37544FBE-1317-4CD0-8DCB-A12A693DB056", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8F686039-89A6-47A7-AA7F-3F8F0BD1E434", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC8716BF-A907-4657-80BD-BBB3FDE77802", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84CA3152-374D-4E07-BAF5-554F4CA450C4", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1582E971-543D-4C3B-B6D2-F6C5BE88461E", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85DA3507-E266-4494-BF7F-E9365F6645BD", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "81AE524B-1A16-49F8-99E1-0911897C26AD", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5F96D37D-878B-4A5B-801B-D8C11F5C4DD8", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "148F0F96-A551-45C7-B17C-4AFCB2B6ADDD", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E7FE5E77-A202-4B93-8DE8-EA1DB8D674CC", SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES, "2" );

            RockMigrationHelper.AddGlobalAttribute(
                Rock.SystemGuid.FieldType.DEFINED_VALUE,
                string.Empty,
                string.Empty,
                "Organization Currency Code",
                "The organization's currency code.",
                48,
                "0BD42A7E-9C7E-417C-AFFE-51102B1E4B43",
                SystemGuid.Attribute.ORGANIZATION_CURRENCY_CODE,
                SystemKey.SystemSetting.ORGANIZATION_CURRENCY_CODE,
                true );

            RockMigrationHelper.AddDefinedTypeAttributeQualifier(
                SystemGuid.Attribute.ORGANIZATION_CURRENCY_CODE,
                Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE,
                "52E53CD5-ABF4-4966-A0E7-3EC0447CFA42"
            );

            RemoveCurrencySymbolFromAttributeValue();
            RemoveCurrencySymbolFromRegistrationTemplate();
            RemoveCurrencySymbolFromSystemEmail();
            RemoveCurrencySymbolFromSystemCommunication();
            RemoveCurrencySymbolFromAttribute();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FinancialTransactionDetail", "ForeignCurrencyAmount");
            DropColumn("dbo.FinancialScheduledTransaction", "ForeignCurrencyCodeValueId");
            DropColumn("dbo.FinancialPersonSavedAccount", "PreferredForeignCurrencyCodeValueId");
            DropColumn("dbo.FinancialTransaction", "ForeignCurrencyCodeValueId");
        }

        private void RemoveCurrencySymbolFromAttributeValue()
        {
            Sql( @"UPDATE  AttributeValue
                SET [Value] = REPLACE([Value], '{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}', '')
                FROM AttributeValue av
                INNER JOIN Attribute a ON a.Id = av.AttributeId
                WHERE a.[Guid] = 'BA34F8F8-D828-47E0-8401-5940E0A9BFBA'

                UPDATE AttributeValue
                SET [Value] = REPLACE([Value], '{{ currencySymbol }}{{ TotalContributionAmount }}', '{{ TotalContributionAmount | FormatAsCurrency }}')
                FROM AttributeValue av
                INNER JOIN Attribute a ON a.Id = av.AttributeId
                WHERE a.[Guid] = 'BA34F8F8-D828-47E0-8401-5940E0A9BFBA'

                UPDATE AttributeValue
                SET [Value] = REPLACE([Value], '{{ currencySymbol }}{{ transactionDetail.Amount }}', '{{ transactionDetail.Amount | FormatAsCurrency }}')
                FROM AttributeValue av
                INNER JOIN Attribute a ON a.Id = av.AttributeId
                WHERE a.[Guid] = 'BA34F8F8-D828-47E0-8401-5940E0A9BFBA'

                UPDATE AttributeValue
                SET [Value] = REPLACE([Value], '{{ currencySymbol }}{{ transactionDetailNonCash.Amount }}', '{{ transactionDetailNonCash.Amount | FormatAsCurrency }}')
                FROM AttributeValue av
                INNER JOIN Attribute a ON a.Id = av.AttributeId
                WHERE a.[Guid] = 'BA34F8F8-D828-47E0-8401-5940E0A9BFBA'

                UPDATE AttributeValue
                SET [Value] = REPLACE([Value], '{{ currencySymbol }}{{ accountsummary.Total }}', '{{ accountsummary.Total | FormatAsCurrency }}')
                FROM AttributeValue av
                INNER JOIN Attribute a ON a.Id = av.AttributeId
                WHERE a.[Guid] = 'BA34F8F8-D828-47E0-8401-5940E0A9BFBA'

                UPDATE AttributeValue
                SET [Value] = REPLACE([Value], '{{ currencySymbol }}{{ pledge.AmountPledged }}', '{{ pledge.AmountPledged | FormatAsCurrency }}')
                FROM AttributeValue av
                INNER JOIN Attribute a ON a.Id = av.AttributeId
                WHERE a.[Guid] = 'BA34F8F8-D828-47E0-8401-5940E0A9BFBA'

                UPDATE AttributeValue
                SET [Value] = REPLACE([Value], '{{ currencySymbol }}{{ pledge.AmountGiven }}', '{{ pledge.AmountGiven | FormatAsCurrency }}')
                FROM AttributeValue av
                INNER JOIN Attribute a ON a.Id = av.AttributeId
                WHERE a.[Guid] = 'BA34F8F8-D828-47E0-8401-5940E0A9BFBA'

                UPDATE AttributeValue
                SET [Value] = REPLACE([Value], '{{ currencySymbol }}{{ pledge.AmountRemaining }}', '{{ pledge.AmountRemaining | FormatAsCurrency }}')
                FROM AttributeValue av
                INNER JOIN Attribute a ON a.Id = av.AttributeId
                WHERE a.[Guid] = 'BA34F8F8-D828-47E0-8401-5940E0A9BFBA'" );
        }

        private void RemoveCurrencySymbolFromRegistrationTemplate()
        {
            Sql( @"UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}', '')
                WHERE [ConfirmationEmailTemplate] LIKE '%{[%] assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' [%]}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}', '')
                WHERE [ConfirmationEmailTemplate] LIKE '%{[%] capture currencySymbol [%]}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{[%] endcapture [%]}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ registrant.Cost | Format:''#,##0.00'' }}', '{{ registrant.Cost | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ registrant.Cost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ registrant.TotalCost | Format:''#,##0.00'' }}', '{{ registrant.TotalCost | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ registrant.TotalCost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ Registration.TotalCost | Format:''#,##0.00'' }}', '{{ Registration.TotalCost | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ Registration.TotalCost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ Registration.DiscountedCost | Format:''#,##0.00'' }}', '{{ Registration.DiscountedCost | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ Registration.DiscountedCost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ payment.Amount | Format:''#,##0.00'' }}', '{{ payment.Amount | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ payment.Amount | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ Registration.TotalPaid | Format:''#,##0.00'' }}', '{{ Registration.TotalPaid | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ Registration.TotalPaid | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}', '{{ Registration.BalanceDue | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ fee.Cost | Format:''#,##0.00'' }}', '{{ fee.Cost | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ fee.Cost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{{ currencySymbol }}{{ fee.TotalCost | Format:''#,##0.00'' }}', '{{ fee.TotalCost | FormatAsCurrency }}')
                WHERE [ConfirmationEmailTemplate] LIKE '%{{ currencySymbol }}{{ fee.TotalCost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [ReminderEmailTemplate] = REPLACE([ReminderEmailTemplate], '{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}', '')
                WHERE [ReminderEmailTemplate] LIKE '%{[%] assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' [%]}%'

                UPDATE  RegistrationTemplate
                SET [ReminderEmailTemplate] = REPLACE([ReminderEmailTemplate], '{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}', '')
                WHERE [ReminderEmailTemplate] LIKE '%{[%] capture currencySymbol [%]}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{[%] endcapture [%]}%'

                UPDATE  RegistrationTemplate
                SET [ReminderEmailTemplate] = REPLACE([ReminderEmailTemplate], '{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}', '{{ Registration.BalanceDue | FormatAsCurrency }}')
                WHERE [ReminderEmailTemplate] LIKE '%{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}', '')
                WHERE [SuccessText] LIKE '%{[%] assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' [%]}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}', '')
                WHERE [SuccessText] LIKE '%{[%] capture currencySymbol [%]}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{[%] endcapture [%]}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ registrant.Cost | Format:''#,##0.00'' }}', '{{ registrant.Cost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ registrant.Cost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ registrant.Cost | Format''#,##0.00'' }}', '{{ registrant.Cost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ registrant.Cost | Format''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ fee.Cost | Format''#,##0.00'' }}', '{{ fee.Cost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ fee.Cost | Format''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ fee.Cost | Format:''#,##0.00'' }}', '{{ fee.Cost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ fee.Cost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ fee.TotalCost | Format:''#,##0.00'' }}', '{{ fee.TotalCost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ fee.TotalCost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ fee.TotalCost | Format''#,##0.00'' }}', '{{ fee.TotalCost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ fee.TotalCost | Format''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ Registration.TotalCost | Format''''#,##0.00'''' }}', '{{ Registration.TotalCost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ Registration.TotalCost | Format''''#,##0.00'''' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ Registration.TotalCost | Format:''#,##0.00'' }}', '{{ Registration.TotalCost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ Registration.TotalCost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ Registration.DiscountedCost | Format:''#,##0.00'' }}', '{{ Registration.DiscountedCost | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ Registration.DiscountedCost | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ payment.Amount | Format''''#,##0.00'''' }}', '{{ payment.Amount | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ payment.Amount | Format''''#,##0.00'''' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ payment.Amount | Format:''#,##0.00'' }}', '{{ payment.Amount | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ payment.Amount | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ Registration.TotalPaid | Format''''#,##0.00'''' }}', '{{ Registration.TotalPaid | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ Registration.TotalPaid | Format''''#,##0.00'''' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ Registration.TotalPaid | Format:''#,##0.00'' }}', '{{ Registration.TotalPaid | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ Registration.TotalPaid | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ Registration.BalanceDue | Format''''#,##0.00'''' }}', '{{ Registration.BalanceDue | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ Registration.BalanceDue | Format''''#,##0.00'''' }}%'

                UPDATE  RegistrationTemplate
                SET [SuccessText] = REPLACE([SuccessText], '{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}', '{{ Registration.BalanceDue | FormatAsCurrency }}')
                WHERE [SuccessText] LIKE '%{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [PaymentReminderEmailTemplate] = REPLACE([PaymentReminderEmailTemplate], '{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}', '')
                WHERE [PaymentReminderEmailTemplate] LIKE '%{[%] assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' [%]}%'

                UPDATE  RegistrationTemplate
                SET [PaymentReminderEmailTemplate] = REPLACE([PaymentReminderEmailTemplate], '{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}', '')
                WHERE [PaymentReminderEmailTemplate] LIKE '%{[%] capture currencySymbol [%]}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{[%] endcapture [%]}%'

                UPDATE  RegistrationTemplate
                SET [PaymentReminderEmailTemplate] = REPLACE([PaymentReminderEmailTemplate], '{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}', '{{ Registration.BalanceDue | FormatAsCurrency }}')
                WHERE [PaymentReminderEmailTemplate] LIKE '%{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}%'

                UPDATE  RegistrationTemplate
                SET [WaitListTransitionEmailTemplate] = REPLACE([WaitListTransitionEmailTemplate], '{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}', '')
                WHERE [WaitListTransitionEmailTemplate] LIKE '%{[%] assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' [%]}%'

                UPDATE  RegistrationTemplate
                SET [WaitListTransitionEmailTemplate] = REPLACE([WaitListTransitionEmailTemplate], '{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}', '')
                WHERE [WaitListTransitionEmailTemplate] LIKE '%{[%] capture currencySymbol [%]}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{[%] endcapture [%]}%'

                UPDATE  RegistrationTemplate
                SET [WaitListTransitionEmailTemplate] = REPLACE([WaitListTransitionEmailTemplate], '{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}', '{{ Registration.BalanceDue | FormatAsCurrency }}')
                WHERE [WaitListTransitionEmailTemplate] LIKE '%{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}%'" );
        }

        private void RemoveCurrencySymbolFromSystemEmail()
        {
            Sql( @"UPDATE  SystemEmail
                SET [Body] = REPLACE([Body], '{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ amount.Amount | Format:''#,##0.00'' }}', '{{ amount.Amount | FormatAsCurrency }}')
                WHERE [Body] LIKE '%{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ amount.Amount | Format:''#,##0.00'' }}%'

                UPDATE  SystemEmail
                SET [Body] = REPLACE([Body], '{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ TotalAmount | Format:''#,##0.00'' }}', '{{ TotalAmount | FormatAsCurrency }}')
                WHERE [Body] LIKE '%{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ TotalAmount | Format:''#,##0.00'' }}%'" );
        }

        private void RemoveCurrencySymbolFromSystemCommunication()
        {
            Sql( @"UPDATE  SystemCommunication
                SET [Body] = REPLACE([Body], '{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ amount.Amount | Format:''#,##0.00'' }}', '{{ amount.Amount | FormatAsCurrency }}')
                WHERE [Body] LIKE '%{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ amount.Amount | Format:''#,##0.00'' }}%'

                UPDATE  SystemCommunication
                SET [Body] = REPLACE([Body], '{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ TotalAmount | Format:''#,##0.00'' }}', '{{ TotalAmount | FormatAsCurrency }}')
                WHERE [Body] LIKE '%{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ TotalAmount | Format:''#,##0.00'' }}%'" );
        }

        private void RemoveCurrencySymbolFromAttribute()
        {
            Sql( @"UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}', '')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')

                UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{{ currencySymbol }}{{ registrant.Cost | Format:''#,##0.00'' }}', '{{ registrant.Cost | FormatAsCurrency }}')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')

                UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{{ currencySymbol }}{{ fee.Cost | Format:''#,##0.00'' }}', '{{ fee.Cost | FormatAsCurrency }}')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')

                UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{{ currencySymbol }}{{ fee.TotalCost | Format:''#,##0.00'' }}', '{{ fee.TotalCost | FormatAsCurrency }}')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')

                UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{{ currencySymbol }}{{ Registration.TotalCost | Format:''#,##0.00'' }}', '{{ Registration.TotalCost | FormatAsCurrency }}')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')

                UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{{ currencySymbol }}{{ Registration.DiscountedCost | Format:''#,##0.00'' }}', '{{ Registration.DiscountedCost | FormatAsCurrency }}')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')

                UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{{ currencySymbol }}{{ payment.Amount | Format:''#,##0.00'' }}', '{{ payment.Amount | FormatAsCurrency }}')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')

                UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{{ currencySymbol }}{{ Registration.TotalPaid | Format:''#,##0.00'' }}', '{{ Registration.TotalPaid | FormatAsCurrency }}')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')

                UPDATE  Attribute
                SET [DefaultValue] = REPLACE([DefaultValue], '{{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}', '{{ Registration.BalanceDue | FormatAsCurrency }}')
                WHERE [Guid] IN ('B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD','EBD8EB51-5514-43B5-8AA6-E0A509D865E5','10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5','C8AB59C0-3074-418E-8493-2BCED16D5034','E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099')" );
        }
    }
}
