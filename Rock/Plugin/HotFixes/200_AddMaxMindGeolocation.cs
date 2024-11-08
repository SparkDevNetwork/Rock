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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 200, "1.16.5" )]
    public class AddMaxMindGeolocation : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameObservabilityHttpModuleUp();
            RefactorPopulateInteractionSessionDataJobUp();
            RemoveIpAddressServiceLocationPageUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveIpAddressServiceLocationPageDown();
            RefactorPopulateInteractionSessionDataJobDown();
            RenameObservabilityHttpModuleDown();
        }

        /// <summary>
        /// Rename Observability HTTP module up.
        /// </summary>
        private void RenameObservabilityHttpModuleUp()
        {
            Sql( $@"
UPDATE [EntityType]
SET [Name] = 'Rock.Web.HttpModules.RockGateway'
    , [AssemblyName] = 'Rock' -- This will get overwritten with the correct assembly name on Rock startup.
    , [FriendlyName] = 'Rock Gateway'
WHERE [Guid] = '{SystemGuid.EntityType.HTTP_MODULE_ROCK_GATEWAY}';" );
        }

        /// <summary>
        /// Refactor PopulateInteractionSessionData job up.
        /// </summary>
        private void RefactorPopulateInteractionSessionDataJobUp()
        {
            // Attribute: Rock.Jobs.PopulateInteractionSessionData: IP Address Geocoding Component
            RockMigrationHelper.DeleteAttribute( "B58B9B93-779D-46DE-8308-E8BCAE7DC352" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Lookback Maximum (days)
            RockMigrationHelper.DeleteAttribute( "BF23E452-603F-41F9-A7BF-FB68E8296686" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Max Records To Process Per Run
            RockMigrationHelper.DeleteAttribute( "C1096F04-0ECF-4519-9ABF-0CBB58BFFEA8" );

            Sql( @"
UPDATE [ServiceJob]
SET [Description] = 'This job will update Interaction counts and Durations for InteractionSession records.'
WHERE [Guid] = 'C0A57A17-1FE9-4C52-96B9-2F6EA1433D00';" );
        }

        /// <summary>
        /// Remove "IP Address Service Location" page up.
        /// </summary>
        private void RemoveIpAddressServiceLocationPageUp()
        {
            // Remove Block
            // Name: Components, from Page: IP Address Location Service, Site: Rock RMS
            // from Page: IP Address Location Service, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "54805637-AB36-4F22-9F58-D49019D6335D" );

            try
            {
                // Delete Page
                // Internal Name: IP Address Location Service
                // Site: Rock RMS
                // Layout: Full Width
                RockMigrationHelper.DeletePage( "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49" );
            }
            catch
            {
                // Don't fail the whole migration if we're unable to delete this page.
            }
        }

        /// <summary>
        /// Remove "IP Address Service Location" page down.
        /// </summary>
        private void RemoveIpAddressServiceLocationPageDown()
        {
            // Add Page
            // Internal Name: IP Address Location Service
            // Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "IP Address Location Service", "", "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49", "fa fa-globe-americas" );

            // Add Page Route
            //  Page:IP Address Location Service
            // Route:admin/system/ip-location-services
            RockMigrationHelper.AddOrUpdatePageRoute( "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49", "admin/system/ip-location-services", "3D69707E-5F37-4F15-AFE8-903861DE8D90" );

            // Add Block
            // Block Name: Components
            // Page Name: IP Address Location Service
            // Layout: -
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "21F5F466-59BC-40B2-8D73-7314D936C3CB".AsGuid(), "Components", "Main", @"", @"", 0, "54805637-AB36-4F22-9F58-D49019D6335D" );

            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: Component Container
            // /*   Attribute Value: Rock.IpAddress.IpAddressLookupContainer, Rock */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.IpAddress.IpAddressLookupContainer, Rock" );

            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: Support Ordering
            // /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "A4889D7B-87AA-419D-846C-3E618E79D875", @"True" );

            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: Support Security
            // /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "A8F1D1B8-0709-497C-9DCB-44826F26AE7A", @"True" );

            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: core.EnableDefaultWorkflowLauncher
            // /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "C29E9E43-B246-4CBB-9A8A-274C8C377FDF", @"True" );

            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: core.CustomGridEnableStickyHeaders
            // /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "9912A0F4-83FB-4447-BFFE-BCE713E1B885", @"False" );
        }

        /// <summary>
        /// Refactor PopulateInteractionSessionData job down.
        /// </summary>
        private void RefactorPopulateInteractionSessionDataJobDown()
        {
            // Attribute: Rock.Jobs.PopulateInteractionSessionData: IP Address Geocoding Component
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Class", "Rock.Jobs.PopulateInteractionSessionData", "IP Address Geocoding Component", "IP Address Geocoding Component", @"The service that will perform the IP GeoCoding lookup for any new IPs that have not been GeoCoded. Not required to be set here because the job will use the first active component if one is not configured here.", 0, @"", "B58B9B93-779D-46DE-8308-E8BCAE7DC352", "IPAddressGeoCodingComponent" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Lookback Maximum (days)
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.PopulateInteractionSessionData", "Lookback Maximum (days)", "Lookback Maximum (days)", @"The number of days into the past the job should look for unmatched addresses in the InteractionSession table. (default 30 days)", 1, @"30", "BF23E452-603F-41F9-A7BF-FB68E8296686", "LookbackMaximumInDays" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Max Records To Process Per Run
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.PopulateInteractionSessionData", "Max Records To Process Per Run", "Max Records To Process Per Run", @"The number of unique IP addresses to process on each run of this job.", 2, @"50000", "C1096F04-0ECF-4519-9ABF-0CBB58BFFEA8", "HowManyRecords" );

            Sql( @"
UPDATE [ServiceJob]
SET [Description] = 'This job will create new InteractionSessionLocation records and / or link existing InteractionSession records to the corresponding InteractionSessionLocation record.'
WHERE [Guid] = 'C0A57A17-1FE9-4C52-96B9-2F6EA1433D00';" );
        }

        /// <summary>
        /// Rename Observability HTTP module down.
        /// </summary>
        private void RenameObservabilityHttpModuleDown()
        {
            Sql( $@"
UPDATE [EntityType]
SET [Name] = 'Rock.Web.HttpModules.Observability'
    , [AssemblyName] = 'Rock' -- This will get overwritten with the correct assembly name on Rock startup.
    , [FriendlyName] = 'Observability HTTP Module'
WHERE [Guid] = '{SystemGuid.EntityType.HTTP_MODULE_ROCK_GATEWAY}';" );
        }
    }
}
