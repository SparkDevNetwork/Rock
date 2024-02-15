using System.Data.Entity;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Communications
{
    /// <summary>
    /// Tests for System Communications
    /// </summary>
    [TestClass]
    public class SystemCommunicationTests : DatabaseTestsBase
    {
        [TestMethod]
        public void SystemCommunications_DoesNotContainPrayerRequestNotification()
        {
            var prayerRequestNotificationGuid = SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION.AsGuid();
            var prayerRequestNotification = new SystemCommunicationService( new RockContext() )
                .Queryable()
                .AsNoTracking()
                .FirstOrDefault( m => m.Guid == prayerRequestNotificationGuid );

            Assert.IsNull( prayerRequestNotification );
        }

        [TestMethod]
        public void SystemCommunications_ContainsPrayerRequestsNotificationAfterMigration()
        {
            var context = new RockContext();
            string sql = $@"
                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [SystemCommunication] WHERE [guid] = '{SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [SystemCommunication]
                    (
                      [IsSystem]
                      ,[Title]
                      ,[From]
                      ,[To]
                      ,[Cc]
                      ,[Bcc]
                      ,[Subject]
                      ,[Body]
                      ,[Guid]
                      ,[CreatedDateTime]
                      ,[ModifiedDateTime]
                      ,[CreatedByPersonAliasId]
                      ,[ModifiedByPersonAliasId]
                      ,[FromName]
                      ,[ForeignKey]
                      ,[CategoryId]
                      ,[ForeignGuid]
                      ,[ForeignId]
                    )
                    SELECT
                      [IsSystem]
                      ,[Title]
                      ,[From]
                      ,[To]
                      ,[Cc]
                      ,[Bcc]
                      ,[Subject]
                      ,[Body]
                      ,[Guid]
                      ,[CreatedDateTime]
                      ,[ModifiedDateTime]
                      ,[CreatedByPersonAliasId]
                      ,[ModifiedByPersonAliasId]
                      ,[FromName]
                      ,[ForeignKey]
                      ,[CategoryId]
                      ,[ForeignGuid]
                      ,[ForeignId]
                    FROM [SystemEmail]
                    WHERE [guid] = '{SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION}'
                    END
";
            var prayerRequestNotificationGuid = SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION.AsGuid();
            context.Database.ExecuteSqlCommand( sql );

            var prayerRequestNotification = new SystemCommunicationService( context )
                .Queryable()
                .AsNoTracking()
                .FirstOrDefault( m => m.Guid == prayerRequestNotificationGuid );

            Assert.IsNotNull( prayerRequestNotification );
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( $"DELETE FROM [SystemCommunication] WHERE [Guid] = '{SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION}'" );
            }
        }
    }
}
