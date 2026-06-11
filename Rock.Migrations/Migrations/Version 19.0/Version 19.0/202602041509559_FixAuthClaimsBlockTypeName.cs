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
    public partial class FixAuthClaimsBlockTypeName : Rock.Migrations.RockMigration
    {
        private const string AuthClaimsBlockTypeGuid = Rock.SystemGuid.BlockType.OIDC_CLAIMS;

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixAuthClaimsBlockTypeProperties();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }

        #region Private Methods

        /// <summary>
        /// Fixes the BlockType name/description for the OpenID Connect Claims ( AuthClaims.ascx.cs) WebForms block
        /// that was incorrectly set previously.
        /// </summary>
        private void FixAuthClaimsBlockTypeProperties()
        {
            Sql( $@"
UPDATE [BlockType]
SET [Name] = 'OpenID Connect Claims',
    [Description] = 'Block for displaying and editing available OpenID Connect claims.'
WHERE [Guid] = '{AuthClaimsBlockTypeGuid}';
" );
        }

        #endregion Private Methods
    }
}
