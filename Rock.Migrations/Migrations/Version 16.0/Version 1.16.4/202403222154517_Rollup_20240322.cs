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
    public partial class Rollup_20240322 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            PreventLavaErrorInContributionStatementUp();
            AddTermsAndPrivacyPageUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// CC: Prevent Lava error in contribution statement (GitHub Issue 5721).
        /// </summary>
        private void PreventLavaErrorInContributionStatementUp()
        {
            Sql( @"
                UPDATE [FinancialStatementTemplate]
                SET [ReportTemplate] = REPLACE([ReportTemplate],
                    '{% assign pledgeCount = Pledges | Size %}',
                    '{% assign pledgeCount = Pledges | Size %}
{% assign pledgeTotal = Pledges | Select:''AmountPledged'' | Sum %}')
                WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0'
                    AND [ReportTemplate] NOT LIKE '%assign pledgeTotal = Pledges | Select:''AmountPledged'' | Sum%';

                UPDATE [FinancialStatementTemplate]
                SET [ReportTemplate] = REPLACE([ReportTemplate],
                    '{% if pledgeCount > 0 %}',
                    '{% if pledgeCount > 0 and pledgeTotal > 0 %}')
                WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0'
                    AND [ReportTemplate] NOT LIKE '%if pledgeCount > 0 and pledgeTotal > 0%';
            " );
        }

        /// <summary>
        /// PA: Add Terms and Privacy Page Along with Page Routes 
        /// </summary>
        private void AddTermsAndPrivacyPageUp()
        {
            // Add Page Terms to Site:Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.SUPPORT_PAGES_EXTERNAL_SITE, SystemGuid.Layout.FULL_WIDTH, "Terms", "", "DF471E9C-EEFC-4493-B6C0-C8D94BC248EB" );

            // Add Block Terms of Use to Page: Terms, Site: External Site
            RockMigrationHelper.AddBlock( true, "DF471E9C-EEFC-4493-B6C0-C8D94BC248EB".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), Rock.SystemGuid.BlockType.HTML_CONTENT.AsGuid(), "Terms of Use", "Main", @"", @"", 0, "A29ACEF4-BB4B-498B-85EE-8543C8E0E7FC" );

            var termsOfUseHTML = @"<h1>Terms of Use</h1>
<div class=""container"">
    <div class=""alert alert-info"" role=""alert"">
    <strong>Note to Our Visitors:</strong> Welcome to the {{ 'Global' | Attribute:'OrganizationName' }} website! Please note, the content you're about to read in our Terms of Use is currently under construction. This means it's not the final version. We're working hard to refine these terms to ensure they're clear, fair, and tailored to serve our community best. We appreciate your understanding and patience. If you have any questions or feedback, don't hesitate to reach out to us.
    </div>

    <h2>Welcome to {{ 'Global' | Attribute:'OrganizationName' }}!</h2>
    <p>The following terms and conditions (the ""Terms of Use"") govern all use of the {{ 'Global' | Attribute:'OrganizationName' }} website and all content, services, and products available at or through the website (taken together, the Website). The Website is owned and operated by {{ 'Global' | Attribute:'OrganizationName' }} (""us"", ""we"", or ""our""). The Website is offered subject to your acceptance without modification of all of the terms and conditions contained herein and all other operating rules, policies (including, without limitation, {{ 'Global' | Attribute:'OrganizationName' }}'s Privacy Policy) and procedures that may be published from time to time on this Site by {{ 'Global' | Attribute:'OrganizationName' }} (collectively, the ""Agreement"").</p>

    <p>Please read this Agreement carefully before accessing or using the Website. By accessing or using any part of the web site, you agree to become bound by the terms and conditions of this agreement. If you do not agree to all the terms and conditions of this agreement, then you may not access the Website or use any services. If these terms and conditions are considered an offer by {{ 'Global' | Attribute:'OrganizationName' }}, acceptance is expressly limited to these terms.</p>

    <h3>Use of the Site</h3>
    <p>You may use our site solely for your personal, non-commercial use. Any use of the site not expressly permitted by these Terms of Use is a breach of this Agreement and may violate copyright, patent, trademark, and other laws.</p>

    <h3>Content</h3>
    <p>All content provided on the site is for informational purposes only. The church makes no representations as to the accuracy or completeness of any information on the site or found by following any link on the site.</p>

    <h3>Changes</h3>
    <p>{{ 'Global' | Attribute:'OrganizationName' }} reserves the right, at its sole discretion, to modify or replace any part of this Agreement at any time, by posting updates and changes to our website. It is your responsibility to check our website periodically for changes.</p>

    <h3>Disclaimer of Warranties</h3>
    <p>The Website is provided ""as is"". {{ 'Global' | Attribute:'OrganizationName' }} and its suppliers and licensors hereby disclaim all warranties of any kind, express or implied, including, without limitation, the warranties of merchantability, fitness for a particular purpose and non-infringement.</p>

    <h3>Limitation of Liability</h3>
    <p>In no event will {{ 'Global' | Attribute:'OrganizationName' }}, or its suppliers or licensors, be liable with respect to any subject matter of this agreement under any contract, negligence, strict liability or other legal or equitable theory for: (i) any special, incidental or consequential damages; (ii) the cost of procurement for substitute products or services; (iii) for interruption of use or loss or corruption of data.</p>

    <h3>Contact Us</h3>
    <p>If you have any questions about these Terms, please contact us at:<br>
    {{ 'Global' | Attribute:'OrganizationName' }}<br>
    {{ 'Global' | Attribute:'OrganizationAddress' }}<br>
    {{ 'Global' | Attribute:'OrganizationEmail' }}<br>
    {{ 'Global' | Attribute:'OrganizationPhone' }}<br>    
    </p>
</div>";

            // Add/Update HtmlContent for Block: Terms of Use
            RockMigrationHelper.UpdateHtmlContentBlock( "A29ACEF4-BB4B-498B-85EE-8543C8E0E7FC", termsOfUseHTML, "BBCF19A5-BEAD-4C49-80A9-A6F07AE9E87F" );

            // Add Page Route terms if not already present
            Sql( @"DECLARE @PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = 'DF471E9C-EEFC-4493-B6C0-C8D94BC248EB')
IF NOT EXISTS( SELECT * FROM [PageRoute] WHERE [Route] = 'terms' )
BEGIN
    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
    VALUES( 0, @PageId, 'terms', '5A43B517-D95B-48FD-AC9B-B7697926F780' )
END" );

            // Add Page Privacy Site:Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.SUPPORT_PAGES_EXTERNAL_SITE, SystemGuid.Layout.FULL_WIDTH, "Privacy", "", "964A4B4B-BF7F-4520-8620-05A6DEEAB5C8" );

            // Add Block Privacy to Page: Privacy, Site: External Site
            RockMigrationHelper.AddBlock( true, "964A4B4B-BF7F-4520-8620-05A6DEEAB5C8".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), Rock.SystemGuid.BlockType.HTML_CONTENT.AsGuid(), "Privacy", "Main", @"", @"", 0, "375B273F-A94A-4364-B10B-143FD2A7C92B" );

            var privacyHTML = @"<h1>Privacy Policy</h1>
<div class=""container"">
    <div class=""alert alert-info"" role=""alert"">
    <strong>Note to Our Visitors:</strong> Welcome to the {{ 'Global' | Attribute:'OrganizationName' }} website! Please note, the content you're about to read in our Privacy Policy is currently under construction. This means it's not the final version. We're working to refine this to ensure it is clear, fair, and tailored to serve our community. We appreciate your understanding and patience. If you have any questions or feedback, don't hesitate to reach out to us.
    </div>
<!-- <p>Effective date: [Insert Date]</p> -->

    <h2>Introduction</h2>
    <p>Thank you for visiting this website, offered by {{ 'Global' | Attribute:'OrganizationName' }} (""we"", ""us"" or ""our""), where we respect your privacy and are committed to protecting it through our compliance with this Privacy Policy. If you have any questions or concerns about this privacy policy, or our practices with regards to your personal information, please contact us at {{ 'Global' | Attribute:'OrganizationEmail' }}.</p>

    <p>When you visit our website {{ 'Global' | Attribute:'OrganizationWebsite' }}, and use our services, you trust us with your personal information. We take your privacy very seriously. In this privacy policy, we seek to explain to you in the clearest way possible what information we collect, how we use it, and what rights you have in relation to it. We hope you take some time to read through it carefully, as it is important. If there are any terms in this privacy policy that you do not agree with, please discontinue use of our Sites and our services.</p>

    <p>This privacy policy applies to all information collected through our website (such as {{ 'Global' | Attribute:'OrganizationWebsite' }}), and/or any related services, sales, marketing or events (we refer to them collectively in this privacy policy as the ""Services"").</p>

    <h2>What Information Do We Collect?</h2>

    <p>We collect personal information that you voluntarily provide to us when expressing an interest in obtaining information about us or our products and services, when participating in activities on the [Website/Service] or otherwise contacting us.</p>

    <p>The personal information that we collect depends on the context of your interactions with us and the [Website/Service], the choices you make, and the products and features you use.</p>

    <h2>How Do We Use Your Information?</h2>
    <p>If you elect to provide us with your information while using the Website or Services, for example, by registering on our [Website/Service] or e-mailing us, etc., we may provide you with product and service related announcements or contact you regarding your member service requests. For example, all registered users receive a welcome e-mail to confirm their registration. These types of communications are necessary to serve you, respond to your concerns, and provide the high level of care that we strive to offer our members.</p>

    <p>We may also use information that we collect about you or that you provide to us, including any Personal Information to: (i) provide you with the [Website/Service] and their contents, and any other information, materials or services that you request from us; (ii) fulfill any other purpose for which you provide it; (iii) give you notices about your account/subscription, including expiration and renewal notices; (iv) carry out our obligations and enforce our rights arising from any contracts entered into between you and us, including for billing and collection; and (v) notify you when updates are available, and of changes to any materials or services we offer or provide through the Website and Services.</p>

    <p>The usage information we collect helps us improve our Website and Services and deliver a better and more personalized experience by enabling us to estimate our audience size and usage patterns, store information about your preferences, customize our Website and Services according to your individual interests, speed up your searches, and recognize you when you use the Website or Services.</p>

    <p>We use information we collect about your location to determine the extent our products and services are used through-out the world.</p>

    <h2>Will Your Information Be Shared With Anyone?</h2>
    <p>We only share information with your consent, to comply with laws, to protect your rights, or to fulfill business obligations.</p>

    <h2>How Do We Handle Your Social Logins?</h2>
    <p>If you choose to register or log in to our services using a social media account, we may have access to certain information about you.</p>

    <h2>What Is Our Stance on Third-Party Websites?</h2>
    <p>Our [Website/Service] may contain advertisements from third parties that are not affiliated with us and which may link to other websites, online services, or mobile applications. We cannot guarantee the safety and privacy of data you provide to any third parties. Any data collected by third parties is not covered by this privacy policy. We are not responsible for the content or privacy and security practices and policies of any third parties, including other websites, services, or applications that may be linked to or from the [Website/Service].</p>

    <h2>Contact Us</h2>
    <p>If you have any questions about these Terms, please contact us at:<br>
    {{ 'Global' | Attribute:'OrganizationName' }}<br>
    {{ 'Global' | Attribute:'OrganizationAddress' }}<br>
    {{ 'Global' | Attribute:'OrganizationEmail' }}<br>
    {{ 'Global' | Attribute:'OrganizationPhone' }}<br>    
    </p>
</div>";

            // Add/Update HtmlContent for Block: Privacy
            RockMigrationHelper.UpdateHtmlContentBlock( "375B273F-A94A-4364-B10B-143FD2A7C92B", privacyHTML, "B81D242C-F063-4296-A5D2-47C446D783AD" );

            // Add Page Route privacy if not already present
            Sql( @"DECLARE @PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '964A4B4B-BF7F-4520-8620-05A6DEEAB5C8')
IF NOT EXISTS( SELECT * FROM [PageRoute] WHERE [Route] = 'privacy' )
BEGIN
    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
    VALUES( 0, @PageId, 'privacy', 'F5BE04CA-A30D-4798-BC4F-83A02004EA3E' )
END" );

            // Mark all the above pages and blocks as Non System so that they may be deleted by the churches as needed.
            Sql( @"
UPDATE [Page] SET [IsSystem] = 0 WHERE [Guid] = 'DF471E9C-EEFC-4493-B6C0-C8D94BC248EB'
UPDATE [Page] SET [IsSystem] = 0 WHERE [Guid] = '964A4B4B-BF7F-4520-8620-05A6DEEAB5C8'
UPDATE [Block] SET [IsSystem] = 0 WHERE [Guid] = 'A29ACEF4-BB4B-498B-85EE-8543C8E0E7FC'
UPDATE [Block] SET [IsSystem] = 0 WHERE [Guid] = '375B273F-A94A-4364-B10B-143FD2A7C92B'
" );
        }
    }
}
