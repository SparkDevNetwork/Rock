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
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data;
    using System.Data.Entity.Migrations;
    using System.Text.RegularExpressions;
    using System.Data.SqlClient;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20240718 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefaultSecurityForCheckInV2RESTControllerUp();
            AddRunOnceJobPopulateEntityIntentsFromAdditionalSettingsJson();
            FixLavaShortcodeTypoUp();
            UpdateYouTubeShortcodeOptions();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddDefaultSecurityForCheckInV2RESTControllerDown();
            FixLavaShortcodeTypoDown();
        }

        #region DH: Add default security for CheckIn v2 REST controller

        private void AddDefaultSecurityForCheckInV2RESTControllerUp()
        {
            // REST Controller was previously created in CreateCheckInLabelModel migration.

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                0,
                Security.Authorization.VIEW,
                true,
                "51e02a99-b7cb-4e64-b7c8-065076aabc05",
                Model.SpecialRole.None,
                "6cdd12fe-c00a-4520-b692-afcd3e20f672" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                0,
                Security.Authorization.EDIT,
                true,
                "51e02a99-b7cb-4e64-b7c8-065076aabc05",
                Model.SpecialRole.None,
                "699b60f6-740e-4eeb-ac1f-f4702ce08b6d" );
        }

        private void AddDefaultSecurityForCheckInV2RESTControllerDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "699b60f6-740e-4eeb-ac1f-f4702ce08b6d" );
            RockMigrationHelper.DeleteSecurityAuth( "6cdd12fe-c00a-4520-b692-afcd3e20f672" );
        }

        #endregion

        #region JPH: Populate EntityIntents from AddtionalSettingsJson

        /// <summary>
        /// JPH: Migration to add post v16.7 run once job to populate EntityIntents from AdditionalSettingsJson.
        /// This goes along with commit https://github.com/SparkDevNetwork/Rock/commit/0ed00a66858345a58e345a2c869c9e5f5b5ef012.
        /// </summary>
        private void AddRunOnceJobPopulateEntityIntentsFromAdditionalSettingsJson()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v16.7 - Populate EntityIntents from AdditionalSettingsJson",
                description: "This job will migrate Interaction intents from Page and ContentChannelItem AdditionalSettingsJson fields to EntityIntents records.",
                jobType: "Rock.Jobs.PostV167PopulateEntityIntentsFromAdditionalSettingsJson",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_167_POPULATE_ENTITY_INTENTS_FROM_ADDITIONAL_SETTINGS_JSON );
        }

        #endregion

        #region KH: Fix Lava Shortcode Typo

        /// <summary>
        /// Fixes the typo in the lava shortcode
        /// </summary>
        private void FixLavaShortcodeTypoUp()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Documentation] = REPLACE( [Documentation], 'barHorizontal', 'horizontalBar' )
WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }

        private void FixLavaShortcodeTypoDown()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Documentation] = REPLACE( [Documentation], 'horizontalBar', 'barHorizontal' )
WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }

        #endregion

        #region JDR: Update YouTube shortcode to add a muted option

        /// <summary>
        /// JDR: Data Migration to Update the YouTube shortcode options to include a boolean 'muted' option
        /// </summary>
        private void UpdateYouTubeShortcodeOptions()
        {
            // Update the existing YouTube shortcode using its Guid
            Sql( @"
            UPDATE [LavaShortcode]
            SET
                [Description] = 'Creates a responsive YouTube embed from just a simple video id.',
                [Documentation] = '<p>Embedding a YouTube video is easy, right? Well what if you want it to be responsive (adjust with the size of the window)? Or what about control of what is shown in the player? The YouTube shortcode helps to shorten (see what we did there) the time it takes to get a video up on your Rock site. Here’s how:<br>  </p><p>Basic Usage:</p><pre>{[ youtube id:''8kpHK4YIwY4'' autoplay:''true'' muted:''true'' ]}</pre><p>This example code will put the video with the provided id onto your page in a responsive container, it will automatically start your video, and it will mute the video so that autoplay will work properly in the browser. The id can be found in the address of the YouTube video. There are also a couple of options for you to add:</p><ul><li><b>id</b> (required) – The YouTube id of the video.</li><li><b>width</b> (100%) – The width you would like the video to be. By default it will be 100% but you can provide any width in percentages, pixels, or any other valid CSS unit of measure.</li><li><b>muted&nbsp;</b>(false) – This determines if the video should be muted or not. This option is required for autoplay to work.&nbsp;</li><li><b>controls</b> (true) – This determines if the standard YouTube controls should be shown.</li><li><b>autoplay</b> (false) – This option will play the specified video upon page load. For autoplay to work, the <b>muted </b>option must be declared and set to true.</li></ul>',
                [Markup] = N'
{% assign wrapperId = uniqueid %}

{% assign parts = id | Split:''/'' %}
{% assign id = parts | Last %}
{% assign parts = id | Split:''='' %}
{% assign id = parts | Last | Trim %}

{% assign url = ''https://www.youtube.com/embed/'' | Append:id | Append:''?rel=0'' %}

{% assign controls = controls | AsBoolean %}
{% assign autoplay = autoplay | AsBoolean %}
{% assign muted = muted | AsBoolean %}

{% if controls %}
    {% assign url = url | Append:''&controls=1'' %}
{% else %}
    {% assign url = url | Append:''&controls=0'' %}
{% endif %}

{% if autoplay %}
    {% assign url = url | Append:''&autoplay=1'' %}
{% else %}
    {% assign url = url | Append:''&autoplay=0'' %}
{% endif %}

{% if muted %}
    {% assign url = url | Append:''&mute=1'' %}
{% else %}
    {% assign url = url | Append:''&mute=0'' %}
{% endif %}

<style>
#{{ wrapperId }} {
    width: {{ width }};
}

.embed-container { 
    position: relative; 
    padding-bottom: 56.25%; 
    height: 0; 
    overflow: hidden; 
    max-width: 100%; 
} 
.embed-container iframe, 
.embed-container object, 
.embed-container embed { 
    position: absolute; 
    top: 0; 
    left: 0; 
    width: 100%; 
    height: 100%; 
}
</style>

<div id=''{{ wrapperId }}''>
    <div class=''embed-container''><iframe src=''{{ url }}'' frameborder=''0'' allowfullscreen></iframe></div>
</div>',
                [Parameters] = 'id^|showinfo^false|controls^true|autoplay^false|width^100%|muted^false'
            WHERE
                [Guid] = '2FA4D446-3F63-4DFD-8C6A-55DBA76AEB83';
        " );
        }

        #endregion
    }
}
