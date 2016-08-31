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
    public partial class AddIdleTimeoutsBack : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Utility/IdleRedirect.ascx'" );
            RockMigrationHelper.AddBlockType( "Idle Redirect", "Redirects user to a new url after a specific number of idle seconds.", "~/Blocks/Utility/IdleRedirect.ascx", "Utility", "49fc4b38-741e-4b0b-b395-7c1929340d88" );
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.AddBlockTypeAttribute( "49FC4B38-741E-4B0B-B395-7C1929340D88", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Idle Seconds", "IdleSeconds", "", "How many seconds of idle time to wait before redirecting user", 0, @"20", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.AddBlockTypeAttribute( "49FC4B38-741E-4B0B-B395-7C1929340D88", "9C204CD0-1233-41C5-818A-C5DA439445AA", "New Location", "NewLocation", "", "The new location URL to send user to after idle time", 0, @"", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4" );


            RockMigrationHelper.AddBlock( "d47858c0-0e6e-46dc-ae99-8ec84ba5f45f", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "c864b0ad-7242-4bf2-a23d-bc915ddf3e6e" );
            RockMigrationHelper.AddBlockAttributeValue( "c864b0ad-7242-4bf2-a23d-bc915ddf3e6e", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "c864b0ad-7242-4bf2-a23d-bc915ddf3e6e", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            
            RockMigrationHelper.AddBlock( "10c97379-f719-4acb-b8c6-651957b660a4", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "7791585a-75d0-4d41-9cdc-e3346cd8c723" );
            RockMigrationHelper.AddBlockAttributeValue( "7791585a-75d0-4d41-9cdc-e3346cd8c723", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "7791585a-75d0-4d41-9cdc-e3346cd8c723", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );

            RockMigrationHelper.AddBlock( "bb8cf87f-680f-48f9-9147-f4951e033d17", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "dd1eb2e3-7532-4e2b-a924-f41ddcd98d8f" );
            RockMigrationHelper.AddBlockAttributeValue( "dd1eb2e3-7532-4e2b-a924-f41ddcd98d8f", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "dd1eb2e3-7532-4e2b-a924-f41ddcd98d8f", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            
            RockMigrationHelper.AddBlock( "60e3ea1f-fd6b-4f0e-9c72-a9960e13427c", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "922a6a94-ce74-48ef-8935-0e04321167d6" );
            RockMigrationHelper.AddBlockAttributeValue( "922a6a94-ce74-48ef-8935-0e04321167d6", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "922a6a94-ce74-48ef-8935-0e04321167d6", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            
            RockMigrationHelper.AddBlock( "043bb717-5799-446f-b8da-30e575110b0c", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "5d5f33a8-3299-4e1e-ad06-075fa2dcfcf6" );
            RockMigrationHelper.AddBlockAttributeValue( "5d5f33a8-3299-4e1e-ad06-075fa2dcfcf6", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "5d5f33a8-3299-4e1e-ad06-075fa2dcfcf6", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            
            RockMigrationHelper.AddBlock( "6f0cb22b-e05b-42f1-a329-9219e81f6c34", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "cb45fe1d-c99b-4cb9-9e0b-becfe233512b" );
            RockMigrationHelper.AddBlockAttributeValue( "cb45fe1d-c99b-4cb9-9e0b-becfe233512b", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "cb45fe1d-c99b-4cb9-9e0b-becfe233512b", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            
            RockMigrationHelper.AddBlock( "c0afa081-b64e-4006-bffc-a350a51ae4cc", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "d9ee24ab-df49-4897-bfc7-b09c3d7d5c40" );
            RockMigrationHelper.AddBlockAttributeValue( "d9ee24ab-df49-4897-bfc7-b09c3d7d5c40", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "d9ee24ab-df49-4897-bfc7-b09c3d7d5c40", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            
            RockMigrationHelper.AddBlock( "e08230b8-35a4-40d6-a0bb-521418314da9", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "2ff4cfd8-bbf3-4d27-a47e-76cc75e3969e" );
            RockMigrationHelper.AddBlockAttributeValue( "2ff4cfd8-bbf3-4d27-a47e-76cc75e3969e", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "2ff4cfd8-bbf3-4d27-a47e-76cc75e3969e", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            
            RockMigrationHelper.AddBlock( "a1cbdaa4-94dd-4156-8260-5a3781e39fd0", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "8acb3d17-be0a-4196-baa3-6f33cf4d058c" );
            RockMigrationHelper.AddBlockAttributeValue( "8acb3d17-be0a-4196-baa3-6f33cf4d058c", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "8acb3d17-be0a-4196-baa3-6f33cf4d058c", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );

            RockMigrationHelper.AddBlock( "af83d0b2-2995-4e46-b0df-1a4763637a68", null, "49fc4b38-741e-4b0b-b395-7c1929340d88", "Idle Timeout", "Main", "", "", 1, "14e5d210-b37c-4c3b-be9b-cc9238688944" );
            RockMigrationHelper.AddBlockAttributeValue( "14e5d210-b37c-4c3b-be9b-cc9238688944", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            RockMigrationHelper.AddBlockAttributeValue( "14e5d210-b37c-4c3b-be9b-cc9238688944", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Utility/IdleRedirect.ascx'" );

            RockMigrationHelper.DeleteBlock( "14e5d210-b37c-4c3b-be9b-cc9238688944" );
            RockMigrationHelper.DeleteBlockAttributeValue( "14e5d210-b37c-4c3b-be9b-cc9238688944", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "14e5d210-b37c-4c3b-be9b-cc9238688944", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "8acb3d17-be0a-4196-baa3-6f33cf4d058c" );
            RockMigrationHelper.DeleteBlockAttributeValue( "8acb3d17-be0a-4196-baa3-6f33cf4d058c", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "8acb3d17-be0a-4196-baa3-6f33cf4d058c", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "2ff4cfd8-bbf3-4d27-a47e-76cc75e3969e" );
            RockMigrationHelper.DeleteBlockAttributeValue( "2ff4cfd8-bbf3-4d27-a47e-76cc75e3969e", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "2ff4cfd8-bbf3-4d27-a47e-76cc75e3969e", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "d9ee24ab-df49-4897-bfc7-b09c3d7d5c40" );
            RockMigrationHelper.DeleteBlockAttributeValue( "d9ee24ab-df49-4897-bfc7-b09c3d7d5c40", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "d9ee24ab-df49-4897-bfc7-b09c3d7d5c40", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "cb45fe1d-c99b-4cb9-9e0b-becfe233512b" );
            RockMigrationHelper.DeleteBlockAttributeValue( "cb45fe1d-c99b-4cb9-9e0b-becfe233512b", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "cb45fe1d-c99b-4cb9-9e0b-becfe233512b", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "5d5f33a8-3299-4e1e-ad06-075fa2dcfcf6" );
            RockMigrationHelper.DeleteBlockAttributeValue( "5d5f33a8-3299-4e1e-ad06-075fa2dcfcf6", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "5d5f33a8-3299-4e1e-ad06-075fa2dcfcf6", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "922a6a94-ce74-48ef-8935-0e04321167d6" );
            RockMigrationHelper.DeleteBlockAttributeValue( "922a6a94-ce74-48ef-8935-0e04321167d6", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "922a6a94-ce74-48ef-8935-0e04321167d6", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "dd1eb2e3-7532-4e2b-a924-f41ddcd98d8f" );
            RockMigrationHelper.DeleteBlockAttributeValue( "dd1eb2e3-7532-4e2b-a924-f41ddcd98d8f", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "dd1eb2e3-7532-4e2b-a924-f41ddcd98d8f", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "7791585a-75d0-4d41-9cdc-e3346cd8c723" );
            RockMigrationHelper.DeleteBlockAttributeValue( "7791585a-75d0-4d41-9cdc-e3346cd8c723", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "7791585a-75d0-4d41-9cdc-e3346cd8c723", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            RockMigrationHelper.DeleteBlock( "c864b0ad-7242-4bf2-a23d-bc915ddf3e6e" );
            RockMigrationHelper.DeleteBlockAttributeValue( "c864b0ad-7242-4bf2-a23d-bc915ddf3e6e", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlockAttributeValue( "c864b0ad-7242-4bf2-a23d-bc915ddf3e6e", "897A1627-B7BB-42C6-8362-DA3F8AD0BD09" );

            
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.DeleteAttribute( "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.DeleteAttribute( "2254B67B-9CB1-47DE-A63D-D0B56051ECD4" );

            RockMigrationHelper.DeleteBlockType( "49fc4b38-741e-4b0b-b395-7c1929340d88" );
        }
    }
}
