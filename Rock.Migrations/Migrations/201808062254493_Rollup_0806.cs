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
    public partial class Rollup_0806 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Person Profile:Send SMS From
            RockMigrationHelper.UpdateBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Send SMS From", "SMSFrom", "", @"The phone number SMS messages should be sent from", 0, @"", "C6360203-CD88-45B3-A32B-309AE4C872E6" );

            NocaSparkDataFrom81();
            RollupsFrom81();
            RemoveAdultChildrenServiceJob();
            EnableAlternateIdentifier();
            MoveAdultChildrenDefaults();
            ERAFinancial();
            AddDisplayNotes();
            HomepageStylesheet();

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Person Profile:Send SMS From
            RockMigrationHelper.DeleteAttribute( "C6360203-CD88-45B3-A32B-309AE4C872E6" );

            RockMigrationHelper.DeletePage( "B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11" );
            RockMigrationHelper.DeleteBlock( "E7BA08B2-F8CC-2FA8-4677-EA3E776F4EEB" );
            RockMigrationHelper.DeleteBlockType( "6B6A429D-E42C-70B5-4A04-98E886C45E7A" );
            RockMigrationHelper.DeletePage( "0591e498-0ad6-45a5-b8ca-9bca5c771f03" );
            Sql( $@"DELETE FROM [dbo].[ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.GET_NCOA}'" );
            RockMigrationHelper.DeleteSystemEmail( SystemGuid.SystemEmail.SPARK_DATA_NOTIFICATION );

            // Attrib for BlockType: Group Attendance List:Display Notes
            RockMigrationHelper.DeleteAttribute( "5C78C7A4-51F5-4C5D-838C-CC2882D6D408" );

        }
                
        /// <summary>
        /// From HotFixMigration 51
        /// </summary>
        private void NocaSparkDataFrom81()
        {
            #region Job

            #region Add GetNcoa Job

            Sql( $@"IF NOT EXISTS(SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.GetNcoa')
                BEGIN
                    INSERT INTO [dbo].[ServiceJob] (
                         [IsSystem]
                        ,[IsActive]
                        ,[Name]
                        ,[Description]
                        ,[Class]
                        ,[CronExpression]
                        ,[NotificationStatus]
                        ,[Guid]
                    )
                    VALUES (
                         0 
                        ,0 
                        ,'Get National Change of Address (NCOA)'
                        ,'Job to get a National Change of Address (NCOA) report for all active people''s addresses.'
                        ,'Rock.Jobs.GetNcoa'
                        ,'0 0/25 * 1/1 * ? *'
                        ,1
                        ,'{Rock.SystemGuid.ServiceJob.GET_NCOA}');
                END" );

            #endregion

            // Delete ProcessNcoaResults job
            Sql( "DELETE FROM [dbo].[ServiceJob] WHERE Class = 'Rock.Jobs.ProcessNcoaResults'" );

            #endregion

            #region Page and block

            // Add the new page
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Spark Data Settings", "", "0591e498-0ad6-45a5-b8ca-9bca5c771f03", "fa fa-database", "74fb3214-8f11-4d40-a0e9-1aea377e9217" ); // Site:Spark Data
            RockMigrationHelper.UpdateBlockType( "Spark Data Settings", "Block used to set values specific to Spark Data (NCOA, Etc).", "~/Blocks/Administration/SparkDataSettings.ascx", "Administration", "6B6A429D-E42C-70B5-4A04-98E886C45E7A" );
            RockMigrationHelper.AddBlock( true, "0591e498-0ad6-45a5-b8ca-9bca5c771f03", "", "6B6A429D-E42C-70B5-4A04-98E886C45E7A", "Spark Data Settings", "Main", @"", @"", 0, "E7BA08B2-F8CC-2FA8-4677-EA3E776F4EEB" );

            // Remove Ncoa History Detail BlockType: ~/Blocks/Crm/NcoaHistoryDetail.ascx
            RockMigrationHelper.DeleteBlockType( "972b7955-ecf9-43b9-80b2-bff40675ffb8" );

            // Data Automation Settings: Remove 'NCOA' from description
            RockMigrationHelper.UpdateBlockType( "Data Automation Settings", "Block used to set values specific to data automation (Updating Person Status, Family Campus, Etc).", "~/Blocks/Administration/DataAutomationSettings.ascx", "Administration", "E34C45E9-97CA-4902-803B-1EFAC9174083" );

            // Add the new page
            RockMigrationHelper.AddPage( true, "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "NCOA Results", "", "B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11", "fa fa-people-carry", "a2d5f989-1e30-47b9-aafc-f7ec627aff21" ); // Site:NCOA Results
            RockMigrationHelper.AddBlock( true, "B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11", "", "3997fe75-e069-4879-b8ba-c8b19c367cd3", "NCOA Results", "Main", @"", @"", 0, "06F05271-BED0-BB9C-4231-CE00348F1035" );

            #endregion

            #region System e-mail
            RockMigrationHelper.UpdateSystemEmail( "System", "Spark Data Notification", "", "", "", "", "", "Spark Data: {{ SparkDataService }}", @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    The '{{ SparkDataService }}' job has {{ Status }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", SystemGuid.SystemEmail.SPARK_DATA_NOTIFICATION );

            #endregion
        }

        /// <summary>
        /// From HotFixMigration 52
        /// </summary>
        private void RollupsFrom81()
        {
            // MP - Fix for Inactive People showing up in Statement Generator Address when ExcludeInactive = True
            Sql( @"
                IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnCrm_GetFamilyTitleIncludeInactive]') AND type in (N'P', N'PC', N'TF'))
                DROP FUNCTION [dbo].[ufnCrm_GetFamilyTitleIncludeInactive]" );

            Sql( MigrationSQL._201808062254493_Rollup_0806_ufnCrm_GetFamilyTitleIncludeInactive );
            Sql( MigrationSQL._201808062254493_Rollup_0806_ufnCrm_GetFamilyTitle );
        }

        /// <summary>
        /// GP: Remove Process Adult Children job
        /// </summary>
        private void RemoveAdultChildrenServiceJob()
        {
            // Remove Rock.Jobs.DataAutomation.AdultChildren
            Sql( "DELETE FROM [dbo].[ServiceJob] WHERE [Guid]='18214883-0394-4C99-99E1-729D77B07FE4' " );
        }

        /// <summary>
        /// NA: Add New 'Enable Alternate Identifier' Attribute to AddGroup block
        /// </summary>
        private void EnableAlternateIdentifier()
        {
            // Attrib Value for BlockType: Add Group
            RockMigrationHelper.AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", SystemGuid.FieldType.BOOLEAN, "Enable Alternate Identifier", "EnableAlternateIdentifier", "", @"If enabled, an additional step will be shown for supplying a custom alternate identifier for each person.", 29, @"False", "94A8697C-6C62-47CC-8F45-CEAABF723097", false );
        }

        /// <summary>
        /// SK: Default Values IF move adult children is not enabled.
        /// </summary>
        private void MoveAdultChildrenDefaults()
        {
            Sql( @"
                    DECLARE @SiblingRelationshipId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid]='1D92F0E1-E161-4160-9C63-2D0A901D3C38')
                    DECLARE @ParentRelationshipId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '6F3FADC4-6320-4B54-9CF6-02EF9586A660') -- Person Pages
 
                    DECLARE @ReplaceValue Varchar(max) = '""ParentRelationshipId"":'+convert(varchar,@ParentRelationshipId)+',""SiblingRelationshipId"":'+ Convert(varchar,@SiblingRelationshipId) +',""UseSameHomeAddress""'
                    UPDATE
                        [Attribute]
                    SET
                        [DefaultValue] = REPLACE( DefaultValue, '""ParentRelationshipId"":null,""SiblingRelationshipId"":null,""UseSameHomeAddress""', @ReplaceValue ) WHERE[Key] = 'core_DataAutomationAdultChildren' AND
                    [DefaultValue] Like '{""IsEnabled"":false,%'" );
        }

        /// <summary>
        /// AF: ERA Financial
        /// Updated Family Analytics logic that determined who was giving. The new logic looks for transactions where the type is 'Contribution' instead of the previous logic that looked for transactions where the account was tax deductible. This allows for soft credits to be considered as contributions."
        /// </summary>
        private void ERAFinancial()
        {
            Sql( MigrationSQL._201808062254493_Rollup_0806_spCrm_FamilyAnalyticsGiving );
        }

        /// <summary>
        /// SK: Add 'Display Notes' to Group Attendance List
        /// </summary>
        private void AddDisplayNotes()
        {
            // Attrib for BlockType: Group Attendance List:Display Notes
            RockMigrationHelper.UpdateBlockTypeAttribute( "5C547728-38C2-420A-8602-3CDAAC369247", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Notes", "DisplayNotes", "", @"Should the Notes column be displayed?", 3, @"True", "5C78C7A4-51F5-4C5D-838C-CC2882D6D408" );

            // Attrib Value for Block:Group Attendance List, Attribute:Display Notes Page: Group Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "81D05028-E0C2-4375-B43D-7F1CD2C28E62", "5C78C7A4-51F5-4C5D-838C-CC2882D6D408", @"True" );
        }

        /// <summary>
        /// GJ: Fix Homepage Stylesheet bug
        /// </summary>
        private void HomepageStylesheet()
        {
            // Attrib Value for Block:Internal Communication View, Attribute:Body Template Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "E7A96FBE-C05F-4079-896E-E84115A96077", @"{% stylesheet id:'home-feature' %}

.feature-image {
    width: 100%;
    height: 450px;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
}


.communicationview h1 {
    font-size: 28px;
    margin-top: 12px;
}

.homepage-article .photo {
    width: 100%;
    height: 140px;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
}

.metric {
    border: 1px solid #ccc;
    padding: 12px;
    margin-bottom: 12px;
}

.metric h5 {
    font-size: 24px;
    margin-top: 0;
    margin-bottom: 0;
    width: 100%;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.metric .value {
    font-size: 48px;
    font-weight: 800;
    line-height: 1em;
}

.metric .value small{
    display: block;
    font-weight: 300;
    font-size: 14px;
    line-height: 1em;
}

.metric .icon {
    float: right;
    opacity: .3;
    font-size: 65px;
    border-radius: 0;
    width: 85px;
    height: 65px;
}

{% endstylesheet %}

<div class=""communicationview"">
    {% assign featureLink = Item | Attribute:'FeatureLink' -%}

    <div class=""feature"">

        <div class=""feature-image"" style="" background-image: url('/GetImage.ashx?Guid={{ Item | Attribute:'FeatureImage','RawValue' }}&w=2400&h=2400');""></div>
        <h1 class=""feature-title"">{{ Item | Attribute:'FeatureTitle' }}</h1>
        <p>
            {{ Item | Attribute:'FeatureText' }}
        </p>

        {% if featureLink != empty -%}
            <a class=""btn btn-xs btn-link"" href=""{{ featureLink }}"">More Info</a>
        {% endif -%}
    </div>

    <hr class=""margin-v-lg"" />

    <div class=""margin-b-lg"">
        {{ Item | Attribute:'Articles' }}
    </div>

    {% assign metricCount = Metrics | Size -%}

    {% if metricCount > 0 -%}
        <h1>Metrics</h1>

        <div class=""row"">
        {% for metric in Metrics -%}
            <div class=""col-lg-4"">
                <div class=""metric"">
                    <h5>{{ metric.Title }}</h5>
                    <span class=""date"">{{ metric.LastRunDateTime | Date:'sd' }}</span>
                    <i class=""icon {{ metric.IconCssClass  }}""></i>

                    <div class=""value"">
                        {{ metric.LastValue | AsInteger }}
                        <small>{{ metric.UnitsLabel }}</small>
                    </div>
                </div>
            </div>

            {% cycle '', '', '</div><div class=""row"">' %}
        {% endfor -%}
        </div>
    {% endif %}

</div>" );
        }
    }
}
