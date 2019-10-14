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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 52, "1.8.0" )]
    public class MigrationRollupsForV8_1 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //InactivePeopleInStatementGenerator();
            //RemoveAdultChildrenServiceJob();
            //EnableAlternateIdentifier();
            //MoveAdultChildrenDefaults();
            //ERAFinancial();
            //AddDisplayNotes();
            //HomepageStylesheet();
            //HomepageMatrix();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }

        /// <summary>
        /// MP - Fix for Inactive People showing up in Statement Generator Address when ExcludeInactive = True
        /// </summary>
        private void InactivePeopleInStatementGenerator()
        {
            // MP - Fix for Inactive People showing up in Statement Generator Address when ExcludeInactive = True
//            Sql( @"
//IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnCrm_GetFamilyTitleIncludeInactive]') AND type in (N'P', N'PC', N'TF'))
//DROP PROCEDURE [dbo].[ufnCrm_GetFamilyTitleIncludeInactive]
//" );

//            Sql( HotFixMigrationResource._052_MigrationRollupsForV8_1_ufnCrm_GetFamilyTitleIncludeInactive );
//            Sql( HotFixMigrationResource._052_MigrationRollupsForV8_1_ufnCrm_GetFamilyTitle );
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
            Sql( HotFixMigrationResource._052_MigrationRollupsForV8_1_spCrm_FamilyAnalyticsGiving );
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

        <div class=""feature-image"" style="" background-image: url('{{ '' | ResolveRockUrl }}/GetImage.ashx?Guid={{ Item | Attribute:'FeatureImage','RawValue' }}&w=2400&h=2400');""></div>
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

        /// <summary>
        /// GJ: Default Values IF move adult children is not enabled.
        /// </summary>
        private void HomepageMatrix()
        {
            Sql( @"
            UPDATE [dbo].[AttributeMatrixTemplate] SET [FormattedLava] = N'{% if AttributeMatrixItems != empty %}
<div class=""row"">
{% for attributeMatrixItem in AttributeMatrixItems %}
    <div class=""homepage-article col-md-4"">
      <div class=""photo"" style=""background-image: url(''{{ '''' | ResolveRockUrl }}/GetImage.ashx?Guid={{ attributeMatrixItem | Attribute:''ArticleImage'',''RawValue'' }}'')"" alt=""{{ attributeMatrixItem | Attribute:''ArticleTitle'' }}""></div>
      <div class=""body"">
        <h5 class=""title"">{{ attributeMatrixItem | Attribute:''ArticleTitle'' }}</h5>
        <div class=""body"">{{ attributeMatrixItem | Attribute:''ArticleContent'' }}</div>
        {% assign articleLink = attributeMatrixItem | Attribute:''ArticleLink'',''RawValue'' -%}
        {% if articleLink != empty -%}
            <a class=""btn btn-xs btn-link"" href=""{{ articleLink }}"">More Info</a>
        {% endif -%}
     </div>
    </div>
    {% cycle '''', '''', ''</div><div class=""row"">'' %}
{% endfor %}
</div>
{% endif %}' WHERE [Guid] = '1d24694e-445c-4852-b5bc-64cdea6f7175'" );
        }

    }
}
