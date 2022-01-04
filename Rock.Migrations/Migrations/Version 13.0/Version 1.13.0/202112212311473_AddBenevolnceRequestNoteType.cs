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
    public partial class AddBenevolnceRequestNoteType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.BenevolenceType", "Description", c => c.String());

            AddNoteType();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.BenevolenceType", "Description", c => c.String(nullable: false));

            DeleteNoteType();
        }

        #region SQL Methods
        private void AddNoteType()
        {
            var sql = @"IF NOT EXISTS (SELECT * FROM [NoteType] WHERE [Guid]='93D54D23-097B-4CC2-98C4-C21FD7F29DD1')
                               BEGIN 
                                 INSERT INTO [NoteType] 
                                 ([IsSystem],[EntityTypeId],[Name],[Guid],[CreatedDateTime],[ModifiedDateTime],[UserSelectable],[IconCssClass],[Order]) 
                                 VALUES 
                                 (1,268,'Benevolence Request Notes','93D54D23-097B-4CC2-98C4-C21FD7F29DD1',GetDate(),GetDate(),1,'fa fa-heart-o',1);
                               END";
            Sql( sql );
        }

        private void DeleteNoteType()
        {
            string sql = "DELETE FROM [NoteType] WHERE [Guid] = '93D54D23-097B-4CC2-98C4-C21FD7F29DD1'";
            Sql( sql );
        }
        #endregion SQL Methods
    }
}
