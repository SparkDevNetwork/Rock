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
    public partial class Rollup_20211116 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixPageRouteCapitalization();
            RockMobileConnectionBlocksUp();
            DataIntegrityMenuUpdate();
            StarkExternalSiteLayoutName();
            LinkScheduledTransactionToSavedAccount();
            UpdatePersonalizedCommunicationHistoryBlock();
            UpdateAttendingToAttend();
            AddDataAutomationIgnoredPersonAttributesDefinedTypeUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMobileConnectionBlocksDown();
            AddDataAutomationIgnoredPersonAttributesDefinedTypeDown();
        }

        /// <summary>
        /// GJ: Fix Page Route Capitalization
        /// </summary>
        private void FixPageRouteCapitalization()
        {
            Sql( @"
                UPDATE [PageRoute]
                SET [Route]=N'people/steps/program/{ProgramId}'
                WHERE ([Guid]='F38326C5-17AB-6FCB-29B0-B9A9C3B71248')" );
        }

        /// <summary>
        /// DH: Mobile Connection Blocks
        /// </summary>
        private void RockMobileConnectionBlocksUp()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Mobile > Connection > Connection Type List.
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Connection.ConnectionTypeList",
                "Connection Type List",
                "Rock.Blocks.Types.Mobile.Connection.ConnectionTypeList, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Connection Type List",
                "Displays the list of connection types.",
                "Rock.Blocks.Types.Mobile.Connection.ConnectionTypeList",
                "Mobile > Connection",
                Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Connection > Connection Type List",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_TYPE_LIST );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "E0D00422-7895-4081-9C06-16DE9BF48E1A",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_TYPE_LIST,
                "Default",
                @"<StackLayout Spacing=""0"">
    {% for type in ConnectionTypes %}    
        <Frame StyleClass=""connection-type""
            HasShadow=""false"">
            <Grid ColumnDefinitions=""50,*,Auto""
                RowDefinitions=""Auto,Auto""
                RowSpacing=""0"">
                {% if type.IconCssClass != null and type.IconCssClass != '' %}
                    <Rock:Icon StyleClass=""connection-type-icon""
                        IconClass=""{{ type.IconCssClass | Replace:'fa fa-','' }}""
                        HorizontalOptions=""Center""
                        VerticalOptions=""Center""
                        Grid.RowSpan=""2"" />
                {% endif %}
                
                <Label StyleClass=""connection-type-name""
                    Text=""{{ type.Name | Escape }}""
                    Grid.Column=""1"" />

                <Label StyleClass=""connection-type-description""
                    Text=""{{ type.Description | Escape }}""
                    MaxLines=""2""
                    LineBreakMode=""TailTruncation""
                    Grid.Column=""1""
                    Grid.Row=""1""
                    Grid.ColumnSpan=""2"" />

                <Rock:Tag StyleClass=""connection-type-count""
                    Text=""{{ ConnectionRequestCounts[type.Id].AssignedToYouCount }}""
                    Type=""info""
                    Grid.Column=""2"" />
            </Grid>

            {% if DetailPage != null %}            
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionTypeGuid={{ type.Guid }}"" />
                </Frame.GestureRecognizers>
            {% endif %}
        </Frame>
    {% endfor %}
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            // Mobile > Connection > Connection Opportunity List.
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityList",
                "Connection Opportunity List",
                "Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityList, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Connection Opportunity List",
                "Displays the list of connection opportunities for a single connection type.",
                "Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityList",
                "Mobile > Connection",
                Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Connection > Connection Opportunity List",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "1FB8E236-DF34-4BA2-B5C6-CA8B542ABC7A",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST,
                "Default",
                @"<StackLayout Spacing=""0"">
    {% for opportunity in ConnectionOpportunities %}    
        <Frame StyleClass=""connection-opportunity""
            HasShadow=""false"">
            <Grid ColumnDefinitions=""50,*,Auto""
                RowDefinitions=""Auto,Auto""
                RowSpacing=""0"">
                {% if opportunity.IconCssClass != null and opportunity.IconCssClass != '' %}
                    <Rock:Icon StyleClass=""connection-opportunity-icon""
                        IconClass=""{{ opportunity.IconCssClass | Replace:'fa fa-','' }}""
                        HorizontalOptions=""Center""
                        VerticalOptions=""Center""
                        Grid.RowSpan=""2"" />
                {% endif %}
                
                <Label StyleClass=""connection-opportunity-name""
                    Text=""{{ opportunity.Name | Escape }}""
                    Grid.Column=""1"" />

                <Label StyleClass=""connection-opportunity-description""
                    Text=""{{ opportunity.Summary | StripHtml | Escape }}""
                    MaxLines=""2""
                    LineBreakMode=""TailTruncation""
                    Grid.Column=""1""
                    Grid.Row=""1""
                    Grid.ColumnSpan=""2"" />

                <Rock:Tag StyleClass=""connection-opportunity-count""
                    Text=""{{ ConnectionRequestCounts[opportunity.Id].AssignedToYouCount }}""
                    Type=""info""
                    Grid.Column=""2"" />
            </Grid>

            {% if DetailPage != null %}            
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionOpportunityGuid={{ opportunity.Guid }}"" />
                </Frame.GestureRecognizers>
            {% endif %}
        </Frame>
    {% endfor %}
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            // Mobile > Connection > Connection Request List.
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestList",
                "Connection Request List",
                "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestList, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Connection Request List",
                "Displays the list of connection requests for a single opportunity.",
                "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestList",
                "Mobile > Connection",
                Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Connection > Connection Request List",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_LIST );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "787BFAA8-FF61-49BA-80DD-67074DC362C2",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_LIST,
                "Default",
                @"<StackLayout Spacing=""0"">
    {% for request in ConnectionRequests %}
        {% assign person = request.PersonAlias.Person %}
        <Frame StyleClass=""connection-request""
            HasShadow=""false"">
            <Grid ColumnDefinitions=""50,*,Auto""
                RowDefinitions=""Auto,Auto""
                RowSpacing=""0"">
                <Rock:Image StyleClass=""connection-request-image""
                    Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if person.PhotoId != null %}{{ person.PhotoUrl | Append:'&width=200' | Escape }}{% else %}{{ person.PhotoUrl | Escape }}{% endif %}""
                    HorizontalOptions=""Center""
                    VerticalOptions=""Center""
                    Grid.RowSpan=""2"">
                    <Rock:CircleTransformation />
                </Rock:Image>
                
                <Label StyleClass=""connection-request-name""
                    Text=""{{ person.FullName | Escape }}""
                    Grid.Column=""1"" />

                <Label Text=""{{ request.Comments | Default:'' | Escape }}""
                    MaxLines=""2""
                    LineBreakMode=""TailTruncation""
                    StyleClass=""connection-request-description""
                    Grid.Column=""1""
                    Grid.Row=""1""
                    Grid.ColumnSpan=""2"" />

                <Label StyleClass=""connection-request-date""
                    Text=""{{ request.CreatedDateTime | Date:'sd' }}""
                    Grid.Column=""2"" />
            </Grid>

            {% if DetailPage != null %}
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionRequestGuid={{ request.Guid }}"" />
                </Frame.GestureRecognizers>
            {% endif %}
        </Frame>
    {% endfor %}
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            // Mobile > Connection > Connection Request Detail.
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetail",
                "Connection Request Detail",
                "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetail, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Connection Request Detail",
                "Displays the details of the given connection request for editing state, status, etc.",
                "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetail",
                "Mobile > Connection",
                Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Connection > Connection Request Detail",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "D19A6D1A-BB4F-45FB-92DE-17EB97479F40",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL,
                "Default",
                @"<StackLayout StyleClass=""request-activities"" Spacing=""0"">
    <Rock:Divider />
    
    <Label StyleClass=""title"" Text=""Activity"" />
    
    {% for activity in Activities %}
        {% assign connector = activity.ConnectorPersonAlias.Person %}
        <Frame StyleClass=""request-activity{% if activity.ConnectionRequestId != ConnectionRequest.Id %},related-activity{% endif %}""
            HasShadow=""false"">
            <StackLayout Orientation=""Horizontal"">
                <Rock:Image StyleClass=""activity-image""
                    Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if connector.PhotoId != null %}{{ connector.PhotoUrl | Append:'&width=200' | Escape }}{% else %}{{ connector.PhotoUrl | Escape }}{% endif %}"">
                    <Rock:CircleTransformation />
                </Rock:Image>
                
                <StackLayout Spacing=""0"" HorizontalOptions=""FillAndExpand"">
                    <StackLayout Orientation=""Horizontal"">
                        <Label StyleClass=""activity-connector-name""
                            Text=""{{ connector.FullName }}""
                            HorizontalOptions=""FillAndExpand"" />

                        {% if activity.ConnectionRequestId != ConnectionRequest.Id %}
                            <Rock:Tag StyleClass=""related-activity-name""
                                Type=""Warning""
                                Size=""Small""
                                HorizontalOptions=""End""
                                Text=""{{ activity.ConnectionRequest.ConnectionOpportunity.Name | Escape }}"" />
                        {% endif %}

                        {% if activity.CreatedDateTime != null %}
                            <Label StyleClass=""activity-date""
                                Text=""{{ activity.CreatedDateTime | Date:'sd' }}""
                                HorizontalOptions=""End"" />
                        {% endif %}
                    </StackLayout>
                    
                    {% if activity.Note != null and activity.Note != '' %}
                        <Label StyleClass=""activity-note"">{{ activity.ConnectionActivityType.Name | Append:': ' | Append:activity.Note | XamlWrap }}</Label>
                    {% else %}
                        <Label StyleClass=""activity-note"" Text=""{{ activity.ConnectionActivityType.Name | Escape }}"" />
                    {% endif %}
                </StackLayout>
            </StackLayout>
        </Frame>
    {% endfor %}
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

        }

        /// <summary>
        /// DH: Mobile Connection Blocks
        /// </summary>
        private void RockMobileConnectionBlocksDown()
        {
            // Mobile > Connection > Connection Request Detail
            RockMigrationHelper.DeleteTemplateBlockTemplate( "D19A6D1A-BB4F-45FB-92DE-17EB97479F40" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL_BLOCK_TYPE );

            // Mobile > Connection > Connection Request List
            RockMigrationHelper.DeleteTemplateBlockTemplate( "787BFAA8-FF61-49BA-80DD-67074DC362C2" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_LIST );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST_BLOCK_TYPE );

            // Mobile > Connection > Connection Opportunity List
            RockMigrationHelper.DeleteTemplateBlockTemplate( "1FB8E236-DF34-4BA2-B5C6-CA8B542ABC7A" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE );

            // Mobile > Connection > Connection Type List
            RockMigrationHelper.DeleteTemplateBlockTemplate( "E0D00422-7895-4081-9C06-16DE9BF48E1A" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_TYPE_LIST );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST_BLOCK_TYPE );
        }

        /// <summary>
        /// GJ: DataIntegrity Menu Update
        /// </summary>
        private void DataIntegrityMenuUpdate()
        {
            // Add Block Attribute Value              
            //   Block: Page Menu              
            //   BlockType: Page Menu              
            //   Category: CMS              
            //   Block Location: Page=Data Integrity, Site=Rock RMS              
            //   Attribute: Template              /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsBlocks-DataIntegrity.lava' %} */              
            RockMigrationHelper.AddBlockAttributeValue("5A428636-5522-4AFD-8FDE-228F711E51C1","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageListAsBlocks-DataIntegrity.lava' %}");
        }

        /// <summary>
        /// GJ: Stark / External Site Layout Name Migration
        /// </summary>
        private void StarkExternalSiteLayoutName()
        {
            Sql( @"
                UPDATE [Layout] SET [Name]=N'Full Width' WHERE ([Guid]='5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD')
                UPDATE [Layout] SET [Name]=N'Left Sidebar' WHERE ([Guid]='325B7BFD-8B80-44FD-A951-4E4763DA6C0D')" );
        }

        /// <summary>
        /// MP: LinkScheduledTransactionToSavedAccount
        /// </summary>
        private void LinkScheduledTransactionToSavedAccount()
        {
            Sql( @"/*
                Update ScheduledTransactions to be associated with the SavedAccount that was used
                if it hasn't been linked already.

                Do this by matching [TransactionCode] -or [ReferenceNumber], [PersonAliasId] and [FinancialGatewayId].
                Stripe populates ReferenceNumber with the TransactionCode, so use that to join if TransactionCode isn't set.
                */

                UPDATE fpd
                SET FinancialPersonSavedAccountId = fpsa.[Id]
                FROM [FinancialScheduledTransaction] fst
                INNER JOIN [FinancialPaymentDetail] fpd
                    ON fpd.[Id] = fst.[FinancialPaymentDetailId]
                INNER JOIN [FinancialPersonSavedAccount] fpsa
                    ON isnull(fpsa.[TransactionCode], fpsa.[ReferenceNumber]) = fst.[TransactionCode]
                        AND fst.[AuthorizedPersonAliasId] = fpsa.[PersonAliasId]
                        AND fst.[FinancialGatewayId] = fpsa.[FinancialGatewayId]
                WHERE fpd.[FinancialPersonSavedAccountId] IS NULL
                    AND isnull(fpsa.[TransactionCode], fpsa.[ReferenceNumber]) IS NOT NULL
                    AND fst.[TransactionCode] IS NOT NULL" );
        }

        /// <summary>
        /// DL: Replace Communication History block on Person History tab with Personalized Communication History block.
        /// </summary>
        private void UpdatePersonalizedCommunicationHistoryBlock()
        {
            const string rock_SystemGuid_Attribute_COMMUNICATION_HISTORY_PERSONALIZED_COMMUNICATION_LIST_DETAIL_PAGE = "530DC2A7-DC47-447B-92CE-CF46612488A4";
            const string rock_SystemGuid_Attribute_COMMUNICATION_HISTORY_PERSONALIZED_COMMUNICATION_SEGMENT_DETAIL_PAGE = "4523F7E5-1356-42DB-9F18-F3FB33F15ADD";
            const string rock_SystemGuid_Attribute_COMMUNICATION_HISTORY_PERSONALIZED_COMMUNICATION_TEMPLATE_DETAIL_PAGE = "FD364362-2BC0-41AA-8E76-FEE888A9560E";
            const string rock_SystemGuid_Attribute_COMMUNICATION_HISTORY_PERSONALIZED_COMMUNICATION_DETAIL_PAGE = "1210C9FB-A6EF-4C10-868B-1D8F054B2356";
            const string rock_SystemGuid_Block_PERSON_COMMUNICATION_HISTORY_PERSONALIZED = "3F294916-A02D-48D5-8FE4-E8D7B98F61F7";
            const string rock_SystemGuid_Page_COMMUNICATION_TEMPLATE_DETAIL = "924036BE-6B89-4C60-96ED-0A9AF1201CC4";
            const string rock_SystemGuid_Page_COMMUNICATION_LIST_DETAIL = "60216406-5BD6-4253-B891-262717C07A00";

            // Add Block Personalized Communication History to Page: Person/History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true,
                Rock.SystemGuid.Page.HISTORY.AsGuid(),
                null,
                Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(),
                rock_SystemGuid_Block_PERSON_COMMUNICATION_HISTORY_PERSONALIZED.AsGuid(),
                "Personalized Communication History",
                "SectionC1",
                @"",
                @"",
                0,
                rock_SystemGuid_Block_PERSON_COMMUNICATION_HISTORY_PERSONALIZED );

            // Add Block Attribute Values
            RockMigrationHelper.AddBlockAttributeValue( rock_SystemGuid_Block_PERSON_COMMUNICATION_HISTORY_PERSONALIZED,
                rock_SystemGuid_Attribute_COMMUNICATION_HISTORY_PERSONALIZED_COMMUNICATION_LIST_DETAIL_PAGE,
                rock_SystemGuid_Page_COMMUNICATION_LIST_DETAIL );

            RockMigrationHelper.AddBlockAttributeValue( rock_SystemGuid_Block_PERSON_COMMUNICATION_HISTORY_PERSONALIZED,
                rock_SystemGuid_Attribute_COMMUNICATION_HISTORY_PERSONALIZED_COMMUNICATION_SEGMENT_DETAIL_PAGE,
                Rock.SystemGuid.Page.DATA_VIEWS );

            RockMigrationHelper.AddBlockAttributeValue( rock_SystemGuid_Block_PERSON_COMMUNICATION_HISTORY_PERSONALIZED,
                rock_SystemGuid_Attribute_COMMUNICATION_HISTORY_PERSONALIZED_COMMUNICATION_TEMPLATE_DETAIL_PAGE,
                rock_SystemGuid_Page_COMMUNICATION_TEMPLATE_DETAIL );

            RockMigrationHelper.AddBlockAttributeValue( rock_SystemGuid_Block_PERSON_COMMUNICATION_HISTORY_PERSONALIZED,
                rock_SystemGuid_Attribute_COMMUNICATION_HISTORY_PERSONALIZED_COMMUNICATION_DETAIL_PAGE,
                Rock.SystemGuid.Page.NEW_COMMUNICATION );

            // Remove Block Communication History from Page: Person/History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "27F84ADB-AA13-439E-A130-FBF73698B172" );
        }

        /// <summary>
        /// CR: Change Group Schedule Email Wording
        /// Changes the wording of the system communications to replace 'attending' with 'attend.
        /// </summary>
        public void UpdateAttendingToAttend()
        {
            Sql( @"
                UPDATE SystemCommunication
                SET Body = REPLACE(body, 'be attending as soon as possible.', 'attend as soon as possible.')
                WHERE [Guid] = '" + Rock.SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION + "' AND [Body] LIKE '%be attending as soon as possible.%'" );

            Sql( @"
                UPDATE SystemEmail
                SET Body = REPLACE(body, 'be attending as soon as possible.', 'attend as soon as possible.')
                WHERE [Guid] = '" + Rock.SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION + "' AND [Body] LIKE '%be attending as soon as possible.%'" );
        }

        /// <summary>
        /// SK: Add Data Automation Ignored Person Attributes Defined Type up.
        /// </summary>
        private void AddDataAutomationIgnoredPersonAttributesDefinedTypeUp()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Data Automation Ignored Person Attributes", "The person attribute keys listed here will be ignored when performing Data Automation on people records.", "886CDB4E-ED8B-48DD-A4CC-D615E032E622", @"" );
            RockMigrationHelper.UpdateDefinedValue( "886CDB4E-ED8B-48DD-A4CC-D615E032E622", "CurrentJourneyGivingStage", "", "98600C4B-8510-43E6-A852-9B6A111C16AD", false );
            RockMigrationHelper.UpdateDefinedValue( "886CDB4E-ED8B-48DD-A4CC-D615E032E622", "JourneyGivingStageChangeDate", "", "473F6FDE-D79A-4ADC-9A58-B487AAD1A286", false );
            RockMigrationHelper.UpdateDefinedValue( "886CDB4E-ED8B-48DD-A4CC-D615E032E622", "PreviousJourneyGivingStage", "", "0D26EA3A-0013-4F67-9C7B-1C817D4339AF", false );
        }

        /// <summary>
        /// SK: Add Data Automation Ignored Person Attributes Defined Type down.
        /// </summary>
        private void AddDataAutomationIgnoredPersonAttributesDefinedTypeDown()
        {
            RockMigrationHelper.DeleteDefinedValue( "0D26EA3A-0013-4F67-9C7B-1C817D4339AF" ); // PreviousJourneyGivingStage
            RockMigrationHelper.DeleteDefinedValue( "473F6FDE-D79A-4ADC-9A58-B487AAD1A286" ); // JourneyGivingStageChangeDate
            RockMigrationHelper.DeleteDefinedValue( "98600C4B-8510-43E6-A852-9B6A111C16AD" ); // CurrentJourneyGivingStage
            RockMigrationHelper.DeleteDefinedType( "886CDB4E-ED8B-48DD-A4CC-D615E032E622" ); // Data Automation Ignored Person Attributes
        }
    }
}
