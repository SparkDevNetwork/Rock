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
    public partial class AddressVerificationService : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteBlockAttributeValue( "60DED2D5-0675-452A-B82B-781B044BB856", "259AF14D-0214-4BE4-A7BF-40423EA07C99" );
            DeleteBlock( "60DED2D5-0675-452A-B82B-781B044BB856" );

            DeletePage( "7112FB95-1F52-448F-9BBC-2FF8B6A3F0A6" );

            Sql( @"
    UPDATE [AttributeValue] SET 
        [Value] = 'Rock.Address.VerificationContainer, Rock' 
    WHERE [Guid] = '364EF9F3-B5A2-4A1F-983D-A3480AC438AD'

    UPDATE [Page] SET
        [InternalName] = 'Location Services'
        , [PageTitle] = 'Location Services'
        , [BrowserTitle] = 'Location Services'
        , [Description] = 'Configuration settings for location services (i.e. address standardization and geocoding).'
    WHERE [Guid] = '1FD5698F-7279-463F-9637-9A80DB86BB86'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
