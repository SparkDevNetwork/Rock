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

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20231127 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateCheckScannerDownloadLinkUp();
            ImprovedHomepageMetricDisplay();
            UpdateAppleDevicesDefinedType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateCheckScannerDownloadLinkDown();
        }

        /// <summary>
        /// JPH: Update the check scanner download link up.
        /// </summary>
        private void UpdateCheckScannerDownloadLinkUp()
        {
            Sql( @"
DECLARE @AttributeId [int] = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9');
DECLARE @DefinedValueId [int] = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'BD5C5A1F-D295-4946-84A4-771834ED0598');
UPDATE [AttributeValue]
SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.16.1/checkscanner.msi'
WHERE [AttributeId] = @AttributeId and [EntityId] = @DefinedValueId" );
        }

        /// <summary>
        /// JPH: Update the check scanner download link down.
        /// </summary>
        private void UpdateCheckScannerDownloadLinkDown()
        {
            Sql( @"
DECLARE @AttributeId [int] = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9');
DECLARE @DefinedValueId [int] = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'BD5C5A1F-D295-4946-84A4-771834ED0598');
UPDATE [AttributeValue]
SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.14.1/checkscanner.msi'
WHERE [AttributeId] = @AttributeId and [EntityId] = @DefinedValueId" );
        }

        /// <summary>
        /// GJ: Improve Homepage Metric Display
        /// </summary>
        private void ImprovedHomepageMetricDisplay()
        {
            Sql( @"
UPDATE [AttributeValue] SET [Value] = N'{% stylesheet id:''home-feature'' %}

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
    
    {% endstylesheet %}
    
    <div class=""communicationview"">
        {% assign featureLink = Item | Attribute:''FeatureLink'',''RawValue'' %}
    
        <div class=""feature"">
    
            <div class=""feature-image"" style="" background-image: url(''/GetImage.ashx?Guid={{ Item | Attribute:''FeatureImage'',''RawValue'' }}&w=2400&h=2400'');""></div>
            <h1 class=""feature-title"">{{ Item | Attribute:''FeatureTitle'' }}</h1>
            <p>
                {{ Item | Attribute:''FeatureText'' }}
            </p>
    
            {% if featureLink != empty -%}
                <a class=""btn btn-xs btn-link p-0"" href=""{{ featureLink | Remove:''https://'' | Remove:''http://'' | Prepend:''https://'' }}"">More Info</a>
            {% endif -%}
        </div>
    
        <hr class=""margin-v-lg"" />
    
        <div class=""margin-b-lg"">
            {{ Item | Attribute:''Articles'' }}
        </div>
    
        {% assign metricCount = Metrics | Size -%}
        {% if metricCount > 0 -%}
            <h1>Metrics</h1>
            {[kpis columncount:''3'' size:''lg'' ]}
                {% for metric in Metrics %}
                    [[ kpi icon:''{{ metric.IconCssClass  }}'' value:''{{ metric.LastValue | AsInteger | Format:''N0'' }}'' label:''{{ metric.Title }}'' secondarylabel:''{{ metric.LastRunDateTime | Date:''sd'' }}'' color:''#c0c0c1'']][[ endkpi ]]
                {% endfor %}
            {[endkpis]}
        {% endif %}
    
    </div>' WHERE [Guid] = '1E2F7914-FD32-4B86-AE08-987408F09DCD' AND [ModifiedDateTime] IS NULL
" );
        }

        /// <summary>
        /// PA: Update Apple Devices
        /// </summary>
        private void UpdateAppleDevicesDefinedType()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,1", "Apple Watch Series 9 41mm case (GPS)", "47E5CA6B-F484-45EA-A497-D6F98DFB0E20", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,2", "Apple Watch Series 9 45mm case (GPS)", "12C39DF0-4182-44B8-BC21-01095DC44C8F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,3", "Apple Watch Series 9 41mm case (GPS+Cellular)", "8AFBE44F-88BB-4A2F-B218-479D8CBEC968", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,4", "Apple Watch Series 9 45mm case (GPS+Cellular)", "EA3DA24A-C8BA-449D-9F77-B50DB0219E5E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,5", "Apple Watch Ultra 2", "913DA648-80A0-4E9F-AB67-7978CF4EE912", true );
        }
    }
}
