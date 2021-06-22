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
    public partial class Rollup_0614 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            WindowsCheckinClientDownloadLinkUp();
            UniversalSearchEventItemResultTemplates();
            UpdateCheckinManagerBackButtons();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            WindowsCheckinClientDownloadLinkDown();
        }

        /// <summary>
        /// Updates the Rock Windows Check-in Client download link.
        /// </summary>
        private void WindowsCheckinClientDownloadLinkUp()
        {
            Sql( @"
                DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.12.4/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        /// <summary>
        /// Restores the old Rock Windows Check-in Client download link.
        /// </summary>
        private void WindowsCheckinClientDownloadLinkDown()
        {
            Sql( @"
                DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.11.1/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        /// <summary>
        /// JE: Universal Search EventItem Result Templates
        /// </summary>
        private void UniversalSearchEventItemResultTemplates()
        {
            Sql( @"
                UPDATE 
	                [EntityType]
                SET
	                [IndexResultTemplate] = '{% assign url = ""~/Event"" | ResolveRockUrl %}
{% if DisplayOptions.EventItem-Url and DisplayOptions.EventItem-Url != null and DisplayOptions.EventItem-Url != '''' %}
	{% assign url = DisplayOptions.EventItem-Url | ResolveRockUrl %} 
{% endif %}  
{% if IndexDocument.DetailsUrl != '''' %}
	<div class=""row model-cannavigate"" data-href=""{{ IndexDocument.DetailsUrl }}"">
{% else %}
	<div class=""row model-cannavigate"" data-href=""{{ url }}/{{ IndexDocument.Id }}"">
{% endif %}
	<div class=""col-sm-1 text-center"">
		<i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
	</div>
	<div class=""col-sm-11"">
		<strong>{{ IndexDocument.Name }}</strong>
		{% if IndexDocument.Description != null and IndexDocument.Description != '''' %}
			<br>
			{{ IndexDocument.Description }}
		{% endif %}
	</div>
</div>'
	            , [IndexDocumentUrl] = '{% assign url = ""~/Event"" | ResolveRockUrl %}  {% if DisplayOptions.EventItem-Url and DisplayOptions.EventItem-Url != null and DisplayOptions.EventItem-Url != '''' %}     {% assign url = DisplayOptions.EventItem-Url | ResolveRockUrl %} {% endif %}  {{ url }}/{{ IndexDocument.Id }}'
WHERE [Name] = 'Rock.Model.EventItem'" );
        }

        /// <summary>
        /// MP: Update CheckinManagerBackButtons
        /// </summary>
        private void UpdateCheckinManagerBackButtons()
        {
            // Update javascript so that it works on iOS CheckinClient app
            // Add/Update HtmlContent for Block: Back Button (Checkin Manager Roster)
            RockMigrationHelper.UpdateHtmlContentBlock( "B62CBF17-7FD1-42C8-9E98-00270A34400D",
@"<a href=""javascript:goBack();"" class=""btn btn-nav-zone""><i class=""fa fa-chevron-left""></i></a>
<script>
    function goBack() {   
        window.location = document.referrer;
    }
</script>", "26988382-5547-41E4-B737-99F0C079A788" );


            // Add/Update HtmlContent for Block: Back Button  (Checkin Manager Attendance Detail) 
            RockMigrationHelper.UpdateHtmlContentBlock( "A9A5FF01-2263-4CE3-82EB-326528BAAD98", @"<a href=""javascript:goBack();"" class=""btn btn-nav-zone""><i class=""fa fa-chevron-left""></i></a>
<script>
    function goBack() {   
        window.location = document.referrer;
    }
</script>", "89A52AED-0245-40DF-87D5-C692761B7E5E" );
        }
    }
}
