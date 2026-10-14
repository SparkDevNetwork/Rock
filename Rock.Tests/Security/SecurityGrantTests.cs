using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Rock.Security;
using Rock.Security.SecurityGrantRules;
using Rock.Tests.Shared;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Security
{
    [TestClass]
    public class SecurityGrantTests
    {
        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            // Security grants read their lifetime from the system settings.
            // Provide empty settings so the defaults are used without a database.
            RockCache.AddOrUpdate( "Rock:SystemSettings", new SystemSettings() );
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Removing an item publishes a message that needs a RockApp.
            using ( TestHelper.CreateScopedRockApp() )
            {
                RockCache.Remove( "Rock:SystemSettings" );
            }
        }

        private static SecurityGrant CreateGrant()
        {
            return new SecurityGrant()
                .AddRule( new GroupPickerSecurityGrantRule() );
        }

        [TestMethod]
        public void FromTokenReturnsGrantCreatedByToToken()
        {
            var token = CreateGrant().ToToken();

            var grant = SecurityGrant.FromToken( token );

            Assert.IsNotNull( grant );
            Assert.IsTrue( grant.IsAccessGranted( GroupPickerSecurityGrantRule.AccessInstance, Authorization.VIEW ) );
        }

        [TestMethod]
        public void FromTokenReturnsGrantCreatedByRawToToken()
        {
            var token = CreateGrant().ToToken( true );

            var grant = SecurityGrant.FromToken( token );

            Assert.IsNotNull( grant );
            Assert.IsTrue( grant.IsAccessGranted( GroupPickerSecurityGrantRule.AccessInstance, Authorization.VIEW ) );
        }

        [TestMethod]
        public void FromTokenRejectsGrantEncryptedWithGeneralKey()
        {
            // Text encrypted by Encryption.EncryptString must never be
            // accepted as a security grant token, even if something that
            // looks like an authentication code is added to it.
            var json = JsonConvert.SerializeObject( CreateGrant() );
            var encrypted = Encryption.EncryptString( json );
            var fakeCode = System.Convert.ToBase64String( new byte[32] );

            Assert.IsNull( SecurityGrant.FromToken( encrypted ) );
            Assert.IsNull( SecurityGrant.FromToken( $"1;2099-01-01T00:00:00.0000000-07:00;{encrypted}" ) );
            Assert.IsNull( SecurityGrant.FromToken( $"{encrypted}.{fakeCode}" ) );
        }

        [TestMethod]
        public void FromTokenRejectsModifiedToken()
        {
            var token = CreateGrant().ToToken();
            var segments = token.Split( ';' );
            var payload = segments[2];
            var modifiedPayload = payload.Substring( 0, 5 ) + ( payload[5] == 'A' ? 'B' : 'A' ) + payload.Substring( 6 );

            Assert.IsNull( SecurityGrant.FromToken( $"{segments[0]};{segments[1]};{modifiedPayload}" ) );
        }

        [TestMethod]
        public void FromTokenReturnsNullForEmptyToken()
        {
            Assert.IsNull( SecurityGrant.FromToken( null ) );
            Assert.IsNull( SecurityGrant.FromToken( string.Empty ) );
        }

        #region File Upload Rule

        private static bool IsUploadAllowed( SecurityGrantRule rule, string rootFolder )
        {
            return new SecurityGrant()
                .AddRule( rule )
                .IsAccessGranted( new FileUploadSecurityGrantRule.FileUploadAccess( rootFolder ), Authorization.EDIT );
        }

        private static bool IsProviderUploadAllowed( SecurityGrantRule rule )
        {
            return new SecurityGrant()
                .AddRule( rule )
                .IsAccessGranted( FileUploadSecurityGrantRule.FileUploadAccess.AssetStorageProvider, Authorization.EDIT );
        }

        [TestMethod]
        [DataRow( "~/Content/Uploads" )]
        [DataRow( "~/Content/Uploads/" )]
        [DataRow( "~/content/uploads" )]
        [DataRow( "Content/Uploads" )]
        [DataRow( "~\\Content\\Uploads\\" )]
        public void FileUploadRuleAllowsSameRootFolder( string rootFolder )
        {
            Assert.IsTrue( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/Content/Uploads/" ), rootFolder ) );
        }

        [TestMethod]
        public void FileUploadRuleAllowsFolderInsideRootFolder()
        {
            Assert.IsTrue( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/Content/Uploads" ), "~/Content/Uploads/ted/" ) );
        }

        [TestMethod]
        [DataRow( "~/Content" )]
        [DataRow( "~/Content/UploadsOther" )]
        [DataRow( "~/Content/Uploads/../Other" )]
        [DataRow( "~/Content/Uploads/./" )]
        [DataRow( "~/App_Data" )]
        public void FileUploadRuleDeniesOtherFolders( string rootFolder )
        {
            Assert.IsFalse( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/Content/Uploads" ), rootFolder ) );
        }

        [TestMethod]
        public void FileUploadRuleTreatsEmptyRootFolderAsContentFolder()
        {
            Assert.IsTrue( IsUploadAllowed( new FileUploadSecurityGrantRule( null ), "~/Content/" ) );
            Assert.IsTrue( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/Content" ), null ) );
            Assert.IsFalse( IsUploadAllowed( new FileUploadSecurityGrantRule( null ), "~/App_Data" ) );
        }

        [TestMethod]
        public void FileUploadRuleForSiteRootAllowsAllFolders()
        {
            Assert.IsTrue( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/" ), "~/" ) );
            Assert.IsTrue( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/" ), "~/Themes/Custom" ) );
            Assert.IsTrue( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/" ), null ) );
            Assert.IsFalse( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/" ), "~/Content/../App_Data" ) );
            Assert.IsFalse( IsUploadAllowed( new FileUploadSecurityGrantRule( "~/Content" ), "~/" ) );
            Assert.IsFalse( IsUploadAllowed( new FileUploadSecurityGrantRule( null ), "~/" ) );
            Assert.IsFalse( IsProviderUploadAllowed( new FileUploadSecurityGrantRule( "~/" ) ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "~/Content" )]
        public void FileUploadRuleForRootFolderDeniesProviderUploads( string rootFolder )
        {
            Assert.IsFalse( IsProviderUploadAllowed( new FileUploadSecurityGrantRule( rootFolder ) ) );
        }

        [TestMethod]
        public void FileUploadRuleForProvidersAllowsOnlyProviderUploads()
        {
            var rule = FileUploadSecurityGrantRule.ForAssetStorageProviders();

            Assert.IsTrue( IsProviderUploadAllowed( rule ) );
            Assert.IsFalse( IsUploadAllowed( rule, null ) );
            Assert.IsFalse( IsUploadAllowed( rule, "~/Content" ) );
        }

        [TestMethod]
        public void FileUploadRuleSurvivesTokenRoundTrip()
        {
            var token = new SecurityGrant()
                .AddRule( new FileUploadSecurityGrantRule( "~/Content/Uploads" ) )
                .AddRule( FileUploadSecurityGrantRule.ForAssetStorageProviders() )
                .ToToken();

            var grant = SecurityGrant.FromToken( token );

            Assert.IsNotNull( grant );
            Assert.IsTrue( grant.IsAccessGranted( new FileUploadSecurityGrantRule.FileUploadAccess( "~/Content/Uploads" ), Authorization.EDIT ) );
            Assert.IsTrue( grant.IsAccessGranted( FileUploadSecurityGrantRule.FileUploadAccess.AssetStorageProvider, Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( new FileUploadSecurityGrantRule.FileUploadAccess( "~/Content/Other" ), Authorization.EDIT ) );
        }

        [TestMethod]
        public void AssetManagerRuleDoesNotAllowUploads()
        {
            var rule = new AssetAndFileManagerSecurityGrantRule( Authorization.EDIT );

            Assert.IsFalse( IsUploadAllowed( rule, "~/Content" ) );
            Assert.IsFalse( IsProviderUploadAllowed( rule ) );
        }

        [TestMethod]
        public void FileUploadRuleDoesNotAllowAssetManager()
        {
            var grant = new SecurityGrant()
                .AddRule( new FileUploadSecurityGrantRule( "~/Content" ) )
                .AddRule( FileUploadSecurityGrantRule.ForAssetStorageProviders() );

            Assert.IsFalse( grant.IsAccessGranted( AssetAndFileManagerSecurityGrantRule.AssetAndFileManagerAccess.Instance, Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( AssetAndFileManagerSecurityGrantRule.AssetAndFileManagerAccess.Instance, Authorization.VIEW ) );
        }

        #endregion

        #region Save Financial Account Rule

        private static readonly System.Guid GatewayGuid = new System.Guid( "6432d2d2-32ff-443d-b5b3-fb6c8414c3ad" );

        private static SaveFinancialAccountSecurityGrantRule.SaveFinancialAccountAccess CreateSaveAccess( System.Guid gatewayGuid, string transactionCode, string gatewayPersonIdentifier )
        {
            return new SaveFinancialAccountSecurityGrantRule.SaveFinancialAccountAccess( gatewayGuid, transactionCode, gatewayPersonIdentifier );
        }

        [TestMethod]
        public void SaveFinancialAccountRuleAllowsSameTransaction()
        {
            var grant = new SecurityGrant()
                .AddRule( new SaveFinancialAccountSecurityGrantRule( GatewayGuid, "T100", "P100" ) );

            Assert.IsTrue( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "T100", "P100" ), Authorization.EDIT ) );
        }

        [TestMethod]
        public void SaveFinancialAccountRuleDeniesOtherTransactions()
        {
            var grant = new SecurityGrant()
                .AddRule( new SaveFinancialAccountSecurityGrantRule( GatewayGuid, "T100", "P100" ) );

            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "T101", "P100" ), Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "t100", "P100" ), Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "T100", "P101" ), Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( System.Guid.NewGuid(), "T100", "P100" ), Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, null, null ), Authorization.EDIT ) );
        }

        [TestMethod]
        public void SaveFinancialAccountRuleWithMissingValuesDeniesAccess()
        {
            var grant = new SecurityGrant()
                .AddRule( new SaveFinancialAccountSecurityGrantRule( GatewayGuid, "", "" ) )
                .AddRule( new SaveFinancialAccountSecurityGrantRule( System.Guid.Empty, "T100", "P100" ) );

            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "", "" ), Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( System.Guid.Empty, "T100", "P100" ), Authorization.EDIT ) );
        }

        [TestMethod]
        public void SaveFinancialAccountRuleOnlyGrantsEdit()
        {
            var grant = new SecurityGrant()
                .AddRule( new SaveFinancialAccountSecurityGrantRule( GatewayGuid, "T100", "P100" ) );

            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "T100", "P100" ), Authorization.VIEW ) );
        }

        [TestMethod]
        public void SaveFinancialAccountRuleSurvivesTokenRoundTrip()
        {
            var token = new SecurityGrant()
                .AddRule( new SaveFinancialAccountSecurityGrantRule( GatewayGuid, "T100", "P100" ) )
                .ToToken();

            var grant = SecurityGrant.FromToken( token );

            Assert.IsNotNull( grant );
            Assert.IsTrue( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "T100", "P100" ), Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "T101", "P100" ), Authorization.EDIT ) );
        }

        [TestMethod]
        public void SaveFinancialAccountRuleDoesNotAllowOtherAccess()
        {
            var grant = new SecurityGrant()
                .AddRule( new SaveFinancialAccountSecurityGrantRule( GatewayGuid, "T100", "P100" ) );

            Assert.IsFalse( grant.IsAccessGranted( new FileUploadSecurityGrantRule.FileUploadAccess( "~/Content" ), Authorization.EDIT ) );
            Assert.IsFalse( grant.IsAccessGranted( AssetAndFileManagerSecurityGrantRule.AssetAndFileManagerAccess.Instance, Authorization.EDIT ) );
        }

        [TestMethod]
        public void OtherRulesDoNotAllowSavingFinancialAccounts()
        {
            var grant = new SecurityGrant()
                .AddRule( new FileUploadSecurityGrantRule( "~/" ) )
                .AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.EDIT ) )
                .AddRule( new GroupPickerSecurityGrantRule() );

            Assert.IsFalse( grant.IsAccessGranted( CreateSaveAccess( GatewayGuid, "T100", "P100" ), Authorization.EDIT ) );
        }

        #endregion
    }
}
