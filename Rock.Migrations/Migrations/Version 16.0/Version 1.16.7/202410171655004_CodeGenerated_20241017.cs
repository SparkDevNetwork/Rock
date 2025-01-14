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
    public partial class CodeGenerated_20241017 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Security
            //   Attribute: Login Confirmation Alert System Communication
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Login Confirmation Alert System Communication", "LoginConfirmationAlertSystemCommunication", "Login Confirmation Alert System Communication", @"The system communication to use for sending login confirmation alerts when a user successfully logs in using a new browser. Merge fields include: UserAgent, IPAddress, LoginDateTime, Location, etc.<span class='tip tip-lava'></span>", 24, @"90D986D4-F3B5-4B28-B731-5A3F35172BA9", "EC2F2A9F-481B-4B52-947E-29A4B28AA99C" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Security
            //   Attribute: Account Protection Profiles for Login Confirmation Alerts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Account Protection Profiles for Login Confirmation Alerts", "AccountProtectionProfilesForLoginConfirmationAlerts", "Account Protection Profiles for Login Confirmation Alerts", @"", 25, @"", "1700CF15-47E1-4BBE-B4F2-A5F6205521D2" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 14, @"", "F8B92BB5-6A32-4090-A4BB-BC6E4127C53E" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 15, @"", "47C232B1-45A0-435A-A522-6AD07AADE1FD" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "47C232B1-45A0-435A-A522-6AD07AADE1FD" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "F8B92BB5-6A32-4090-A4BB-BC6E4127C53E" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Security
            //   Attribute: Account Protection Profiles for Login Confirmation Alerts
            RockMigrationHelper.DeleteAttribute( "1700CF15-47E1-4BBE-B4F2-A5F6205521D2" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Security
            //   Attribute: Login Confirmation Alert System Communication
            RockMigrationHelper.DeleteAttribute( "EC2F2A9F-481B-4B52-947E-29A4B28AA99C" );
        }
    }
}
