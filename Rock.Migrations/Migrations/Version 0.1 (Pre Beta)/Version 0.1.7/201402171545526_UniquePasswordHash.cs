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
    using System.Data.SqlClient;
    
    /// <summary>
    ///
    /// </summary>
    public partial class UniquePasswordHash : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            string qry = @"
    SELECT 
         L.[Id]
        ,L.[Password] 
        ,L.[Guid]
    FROM 
        [UserLogin] L
        INNER JOIN [EntityType] ET ON ET.[Id] = L.[entityTypeId] AND ET.[Guid] = '4E9B798F-BB68-4C0E-9707-0928D15AB020'
";
            var rdr = Rock.Data.DbService.GetDataReader( qry, System.Data.CommandType.Text, null );
            while ( rdr.Read() )
            {
                var hash = new System.Security.Cryptography.HMACSHA1();
                hash.Key = HexToByte( rdr["Guid"].ToString().Replace( "-", "" ) );

                string oldPassword = rdr["Password"].ToString();
                string newPassword = Convert.ToBase64String( hash.ComputeHash( Convert.FromBase64String( oldPassword ) ) );

                string updateQry = string.Format( @"
    UPDATE 
        [UserLogin]
    SET
        [Password] = '{0}'
    WHERE
        [Id] = {1}
", newPassword, rdr["Id"].ToString() );

                Rock.Data.DbService.ExecuteCommand( updateQry, System.Data.CommandType.Text );
            }

            rdr.Close();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private static byte[] HexToByte( string hexString )
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for ( int i = 0; i < returnBytes.Length; i++ )
                returnBytes[i] = Convert.ToByte( hexString.Substring( i * 2, 2 ), 16 );
            return returnBytes;
        }
    }
}
