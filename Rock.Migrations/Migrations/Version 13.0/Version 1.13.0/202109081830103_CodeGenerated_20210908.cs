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
    public partial class CodeGenerated_20210908 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Attribute for BlockType: Attribute Values:Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF08B18C-8AA4-4EE1-B930-B4EE757F0CA5", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Category", "Category", "Category", @"The Attribute Categories to display attributes from", 0, @"", "479E9091-71FC-41E5-B8B7-AA7F3F990F88" );

            // Attribute for BlockType: Attribute Values:Attribute Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF08B18C-8AA4-4EE1-B930-B4EE757F0CA5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Order", "AttributeOrder", "Attribute Order", @"The order to use for displaying attributes.  Note: this value is set through the block's UI and does not need to be set here.", 1, @"", "0DB16ADD-A256-4333-8DBC-DC1FC02996E1" );

            // Attribute for BlockType: Attribute Values:Use Abbreviated Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF08B18C-8AA4-4EE1-B930-B4EE757F0CA5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Abbreviated Name", "UseAbbreviatedName", "Use Abbreviated Name", @"Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.", 2, @"False", "82B0F212-F133-4C11-A6F2-96C910EA0F79" );

            // Attribute for BlockType: Attribute Values:Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF08B18C-8AA4-4EE1-B930-B4EE757F0CA5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title", "BlockTitle", "Block Title", @"The text to display as the heading.", 3, @"", "2E58B2D7-8E53-4671-A56B-7E8064BC40A7" );

            // Attribute for BlockType: Attribute Values:Block Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF08B18C-8AA4-4EE1-B930-B4EE757F0CA5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Icon", "BlockIcon", "Block Icon", @"The css class name to use for the heading icon.", 4, @"", "470EFF21-B851-4E5A-9F8E-C9D3BBFF67BA" );

            // Attribute for BlockType: Attribute Values:Show Category Names as Separators
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF08B18C-8AA4-4EE1-B930-B4EE757F0CA5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category Names as Separators", "ShowCategoryNamesasSeparators", "Show Category Names as Separators", @"If enabled, attributes will be grouped by category and will include the category name as a heading separator.", 5, @"False", "CB16AAA6-83B8-43F1-BE85-7044AFAF9640" );

            // Attribute for BlockType: Registration Entry:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "702647D6-487F-41A0-9D54-2DE8AF59464B" );

            // Attribute for BlockType: Registration Entry:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 1, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "54FCEC51-2355-4BD3-BF3A-E2B8A8721A42" );

            // Attribute for BlockType: Registration Entry:Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source", "Source", "Source", @"The Financial Source Type to use when creating transactions", 2, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "FA60857D-74E1-483D-B381-A0CB20036500" );

            // Attribute for BlockType: Registration Entry:Batch Name Prefix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "Batch Name Prefix", @"The batch prefix name to use when creating a new batch", 3, @"Event Registration", "C60997F5-DA4C-4C9A-8D05-4F4BFC1BFEE4" );

            // Attribute for BlockType: Registration Entry:Display Progress Bar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Progress Bar", "DisplayProgressBar", "Display Progress Bar", @"Display a progress bar for the registration.", 4, @"True", "9ED859B6-F6C4-4E4F-8CDC-F0CC2CDD9E2E" );

            // Attribute for BlockType: Registration Entry:Allow InLine Digital Signature Documents
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow InLine Digital Signature Documents", "SignInline", "Allow InLine Digital Signature Documents", @"Should inline digital documents be allowed? This requires that the registration template is configured to display the document inline", 6, @"True", "DC263886-B346-4B0D-92B9-A5D282D0303A" );

            // Attribute for BlockType: Registration Entry:Family Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Term", "FamilyTerm", "Family Term", @"The term to use for specifying which household or family a person is a member of.", 8, @"immediate family", "C22B4472-ADB7-4F2F-AE2B-197C59BA9E46" );

            // Attribute for BlockType: Registration Entry:Force Email Update
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Force Email Update", "ForceEmailUpdate", "Force Email Update", @"Force the email to be updated on the person's record.", 9, @"False", "700E2AEA-B9FA-4EDC-B7E8-DA4B7EF6989F" );

            // Attribute for BlockType: Registration Entry:Show Field Descriptions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Field Descriptions", "ShowFieldDescriptions", "Show Field Descriptions", @"Show the field description as help text", 10, @"True", "776CFF07-1C00-4213-A2B4-C830F74072CB" );

            // Attribute for BlockType: Registration Entry:Enabled Saved Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BDB939BC-AAE2-45F5-ADF7-6400223E041F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enabled Saved Account", "EnableSavedAccount", "Enabled Saved Account", @"Set this to false to disable the using Saved Account as a payment option, and to also disable the option to create saved account for future use.", 11, @"True", "9659B79A-374B-4B47-9193-62ED33DB1AC1" );

            // Attribute for BlockType: Transaction Entry:Financial Gateway
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "7B34F9D8-6BBA-423E-B50E-525ABB3A1013", "Financial Gateway", "FinancialGateway", "Financial Gateway", @"The payment gateway to use for Credit Card and ACH transactions.", 0, @"", "80FF9B9B-9B81-4A75-8E00-9EECDED2D3A4" );

            // Attribute for BlockType: Transaction Entry:Enable ACH
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable ACH", "EnableACH", "Enable ACH", @"", 1, @"False", "766A4182-DEC0-4E21-921D-DBF10AB597DC" );

            // Attribute for BlockType: Transaction Entry:Enable Credit Card
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Credit Card", "EnableCreditCard", "Enable Credit Card", @"", 2, @"True", "7412AFFC-7870-4727-8ECD-3FA7399C9208" );

            // Attribute for BlockType: Transaction Entry:Batch Name Prefix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "Batch Name Prefix", @"The batch prefix name to use when creating a new batch.", 3, @"Online Giving", "BD36EF80-FF09-45C7-9BCB-B94DAB425EA2" );

            // Attribute for BlockType: Transaction Entry:Financial Source Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Financial Source Type", "FinancialSourceType", "Financial Source Type", @"The Financial Source Type to use when creating transactions", 19, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "E5854089-EE19-4ECF-8526-17BD3BEF7406" );

            // Attribute for BlockType: Transaction Entry:Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "AccountsToDisplay", "Accounts", @"The accounts to display. If the account has a child account for the selected campus, the child account for that campus will be used.", 5, @"", "AE860DA4-75AA-4857-A2BC-FF4A650ED4C7" );

            // Attribute for BlockType: Transaction Entry:Ask for Campus if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ask for Campus if Known", "AskForCampusIfKnown", "Ask for Campus if Known", @"If the campus for the person is already known, should the campus still be prompted for?", 10, @"True", "E489E02C-20DB-4AC4-88E2-513A77A0149D" );

            // Attribute for BlockType: Transaction Entry:Include Inactive Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "Include Inactive Campuses", @"Set this to true to include inactive campuses", 10, @"False", "89FB998F-FB49-4091-91F9-64F6351031F8" );

            // Attribute for BlockType: Transaction Entry:Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "IncludedCampusTypes", "Campus Types", @"Set this to limit campuses by campus type.", 11, @"", "22D9851E-B50E-4570-826A-148B9287C85C" );

            // Attribute for BlockType: Transaction Entry:Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "IncludedCampusStatuses", "Campus Statuses", @"Set this to limit campuses by campus status.", 12, @"", "A4D1EFD2-22E3-4DCD-8DB7-A7EC0D9388B0" );

            // Attribute for BlockType: Transaction Entry:Enable Multi-Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Multi-Account", "EnableMultiAccount", "Enable Multi-Account", @"Should the person be able specify amounts for more than one account?", 13, @"True", "349C73A6-E84C-441D-9F7D-84FBF19ACE1F" );

            // Attribute for BlockType: Transaction Entry:Enable Business Giving
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Business Giving", "EnableBusinessGiving", "Enable Business Giving", @"Should the option to give as a business be displayed.", 999, @"True", "573B6958-013B-4C77-9771-2B067FE1E90C" );

            // Attribute for BlockType: Transaction Entry:Enable Anonymous Giving
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Anonymous Giving", "EnableAnonymousGiving", "Enable Anonymous Giving", @"Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fund-raising contributors.", 24, @"False", "582ADD35-C1E4-43A6-BC6B-7B3EBC8E4741" );

            // Attribute for BlockType: Transaction Entry:Anonymous Giving Tool-tip
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Anonymous Giving Tool-tip", "AnonymousGivingTooltip", "Anonymous Giving Tool-tip", @"The tool-tip for the 'Give Anonymously' check box.", 25, @"", "0A42E404-E6A1-44D3-91B9-70F2A570BB76" );

            // Attribute for BlockType: Transaction Entry:Scheduled Transactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduled Transactions", "AllowScheduledTransactions", "Scheduled Transactions", @"If the selected gateway(s) allow scheduled transactions, should that option be provided to user.", 1, @"True", "4AB1A22C-B2D5-4FDD-A797-F9083325878A" );

            // Attribute for BlockType: Transaction Entry:Show Scheduled Gifts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Scheduled Gifts", "ShowScheduledTransactions", "Show Scheduled Gifts", @"If the person has any scheduled gifts, show a summary of their scheduled gifts.", 2, @"True", "92FEB4DD-2319-4D69-930A-49530607D6EA" );

            // Attribute for BlockType: Transaction Entry:Scheduled Gifts Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Scheduled Gifts Template", "ScheduledTransactionsTemplate", "Scheduled Gifts Template", @"The Lava Template to use to display Scheduled Gifts.", 3, @"
<h4>Scheduled {{ GiftTerm | Pluralize }}</h4>

{% for scheduledTransaction in ScheduledTransactions %}
    <div class='scheduled-transaction js-scheduled-transaction' data-scheduled-transaction-id='{{ scheduledTransaction.Id }}' data-expanded='{{ ExpandedStates[scheduledTransaction.Id] }}'>
        <div class='panel panel-default'>
            <div class='panel-heading'>
                <span class='panel-title h1'>
                    <i class='fa fa-calendar'></i>
                    {{ scheduledTransaction.TransactionFrequencyValue.Value }}
                </span>

                <span class='js-scheduled-totalamount scheduled-totalamount margin-l-md'>
                    {{ scheduledTransaction.TotalAmount | FormatAsCurrency }}
                </span>

                <div class='panel-actions pull-right'>
                    <span class='js-toggle-scheduled-details toggle-scheduled-details clickable fa fa-chevron-down'></span>
                </div>
            </div>

            <div class='js-scheduled-details scheduled-details margin-l-lg'>
                <div class='panel-body'>
                    {% for scheduledTransactionDetail in scheduledTransaction.ScheduledTransactionDetails %}
                        <div class='account-details'>
                            <span class='scheduled-transaction-account control-label'>
                                {{ scheduledTransactionDetail.Account.PublicName }}
                            </span>
                            <br />
                            <span class='scheduled-transaction-amount'>
                                {{ scheduledTransactionDetail.Amount | FormatAsCurrency }}
                            </span>
                        </div>
                    {% endfor %}

                    <br />
                    <span class='scheduled-transaction-payment-detail'>
                        {% assign financialPaymentDetail = scheduledTransaction.FinancialPaymentDetail %}

                        {% if financialPaymentDetail.CurrencyTypeValue.Value != 'Credit Card' %}
                            {{ financialPaymentDetail.CurrencyTypeValue.Value }}
                        {% else %}
                            {{ financialPaymentDetail.CreditCardTypeValue.Value }} {{ financialPaymentDetail.AccountNumberMasked }}
                        {% endif %}
                    </span>
                    <br />

                    {% if scheduledTransaction.NextPaymentDate != null %}
                        Next Gift: {{ scheduledTransaction.NextPaymentDate | Date:'sd' }}.
                    {% endif %}


                    <div class='scheduled-details-actions margin-t-md'>
                        {% if LinkedPages.ScheduledTransactionEditPage != '' %}
                            <a href='{{ LinkedPages.ScheduledTransactionEditPage }}?ScheduledTransactionId={{ scheduledTransaction.Id }}'>Edit</a>
                        {% endif %}
                        <a class='margin-l-sm' onclick=""{{ scheduledTransaction.Id | Postback:'DeleteScheduledTransaction' }}"">Delete</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
{% endfor %}


<script type='text/javascript'>

    // Scheduled Transaction JavaScripts
    function setScheduledDetailsVisibility($container, animate) {
        var $scheduledDetails = $container.find('.js-scheduled-details');
        var expanded = $container.attr('data-expanded');
        var $totalAmount = $container.find('.js-scheduled-totalamount');
        var $toggle = $container.find('.js-toggle-scheduled-details');

        if (expanded == 1) {
            if (animate) {
                $scheduledDetails.slideDown();
                $totalAmount.fadeOut();
            } else {
                $scheduledDetails.show();
                $totalAmount.hide();
            }

            $toggle.removeClass('fa-chevron-down').addClass('fa-chevron-up');
        } else {
            if (animate) {
                $scheduledDetails.slideUp();
                $totalAmount.fadeIn();
            } else {
                $scheduledDetails.hide();
                $totalAmount.show();
            }

            $toggle.removeClass('fa-chevron-up').addClass('fa-chevron-down');
        }
    };

    Sys.Application.add_load(function () {
        var $scheduleDetailsContainers = $('.js-scheduled-transaction');

        $scheduleDetailsContainers.each(function (index) {
            setScheduledDetailsVisibility($($scheduleDetailsContainers[index]), false);
        });

        var $toggleScheduledDetails = $('.js-toggle-scheduled-details');
        $toggleScheduledDetails.on('click', function () {
            var $scheduledDetailsContainer = $(this).closest('.js-scheduled-transaction');
            if ($scheduledDetailsContainer.attr('data-expanded') == 1) {
                $scheduledDetailsContainer.attr('data-expanded', 0);
            } else {
                $scheduledDetailsContainer.attr('data-expanded', 1);
            }

            setScheduledDetailsVisibility($scheduledDetailsContainer, true);
        });
    });
</script>
", "972BDC8F-5AF8-4509-8EBD-38B8474D4106" );

            // Attribute for BlockType: Transaction Entry:Scheduled Transaction Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Edit Page", "ScheduledTransactionEditPage", "Scheduled Transaction Edit Page", @"The page to use for editing scheduled transactions.", 4, @"", "AD4F7077-A1AC-4261-B6FE-54A203BF25A5" );

            // Attribute for BlockType: Transaction Entry:Enable Comment Entry
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Comment Entry", "EnableCommentEntry", "Enable Comment Entry", @"Allows the guest to enter the value that's put into the comment field (will be appended to the 'Payment Comment' setting)", 1, @"False", "F89D8E87-ED5D-4EAF-9140-DD452B39EA1E" );

            // Attribute for BlockType: Transaction Entry:Comment Entry Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Entry Label", "CommentEntryLabel", "Comment Entry Label", @"The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).", 2, @"Comment", "51C745E4-3ED9-4326-A99B-324E3DB1C189" );

            // Attribute for BlockType: Transaction Entry:Payment Comment Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Payment Comment Template", "PaymentCommentTemplate", "Payment Comment Template", @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>", 3, @"", "3DBBD02C-D113-40D9-A88F-BCA0E987138C" );

            // Attribute for BlockType: Transaction Entry:Save Account Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Account Title", "SaveAccountTitle", "Save Account Title", @"The text to display as heading of section for saving payment information.", 1, @"Make Giving Even Easier", "165BE92F-321D-4E79-83A2-16B79715A248" );

            // Attribute for BlockType: Transaction Entry:Intro Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Intro Message", "IntroMessageTemplate", "Intro Message", @"The text to place at the top of the amount entry. <span class='tip tip-lava'></span>", 2, @"<h2>Your Generosity Changes Lives</h2>", "94B2B3F9-D5A5-4210-A59B-B015F27231BE" );

            // Attribute for BlockType: Transaction Entry:Gift Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Gift Term", "GiftTerm", "Gift Term", @"", 3, @"Gift", "5EB02BA7-CA0D-4392-BB5D-E22017544E4B" );

            // Attribute for BlockType: Transaction Entry:Give Button Text - Now 
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Give Button Text - Now ", "GiveButtonNowText", "Give Button Text - Now ", @"", 4, @"Give Now", "E30205F7-A4C9-46AA-9468-E1A6AAF7ADCD" );

            // Attribute for BlockType: Transaction Entry:Give Button Text - Scheduled
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Give Button Text - Scheduled", "GiveButtonScheduledText", "Give Button Text - Scheduled", @"", 5, @"Schedule Your Gift", "19533C6D-952A-4692-9B79-9DE793A2FB61" );

            // Attribute for BlockType: Transaction Entry:Amount Summary Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Amount Summary Template", "AmountSummaryTemplate", "Amount Summary Template", @"The text (HTML) to display on the amount summary page. <span class='tip tip-lava'></span>", 6, @"
{% assign sortedAccounts = Accounts | Sort:'Order,PublicName' %}

<span class='account-names'>{{ sortedAccounts | Map:'PublicName' | Join:', ' | ReplaceLast:',',' and' }}</span>
-
<span class='account-campus'>{{ Campus.Name }}</span>", "7481CC07-A846-4777-BD24-3F4890BBBB5C" );

            // Attribute for BlockType: Transaction Entry:Finish Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Finish Lava Template", "FinishLavaTemplate", "Finish Lava Template", @"The text (HTML) to display on the success page. <span class='tip tip-lava'></span>", 7, @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

<h1>Thank You!</h1>

<p>Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively achieve our
mission. We are so grateful for your commitment.</p>

<dl>
    <dt>Confirmation Code</dt>
    <dd>{{ Transaction.TransactionCode }}</dd>
    <dd></dd>

    <dt>Name</dt>
    <dd>{{ Person.FullName }}</dd>
    <dd></dd>
    <dd>{{ Person.Email }}</dd>
    <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
</dl>

<dl class='dl-horizontal'>
    {% for transactionDetail in transactionDetails %}
        <dt>{{ transactionDetail.Account.PublicName }}</dt>
        <dd>{{ transactionDetail.Amount | FormatAsCurrency }}</dd>
    {% endfor %}
    <dd></dd>

    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked }}</dd>
    {% endif %}

    <dt>When<dt>
    <dd>

    {% if Transaction.TransactionFrequencyValue %}
        {{ Transaction.TransactionFrequencyValue.Value }} starting on {{ Transaction.NextPaymentDate | Date:'sd' }}
    {% else %}
        Today
    {% endif %}
    </dd>
</dl>
", "A713A1C6-E5F7-4EEF-81B3-AE6D4D901246" );

            // Attribute for BlockType: Transaction Entry:Confirm Account Email Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirm Account Email Template", "ConfirmAccountEmailTemplate", "Confirm Account Email Template", @"The Email Template to use when confirming a new account", 1, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "29EB295B-9D2B-40DE-8698-BD125B8DDE4F" );

            // Attribute for BlockType: Transaction Entry:Receipt Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Receipt Email", "ReceiptEmail", "Receipt Email", @"The system email to use to send the receipt.", 2, @"", "5319007C-475B-4269-BB34-D95EC37531DE" );

            // Attribute for BlockType: Transaction Entry:Prompt for Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Phone", "PromptForPhone", "Prompt for Phone", @"Should the user be prompted for their phone number?", 1, @"False", "54CB2276-5F67-40A8-85C9-BB4BC0366D0E" );

            // Attribute for BlockType: Transaction Entry:Prompt for Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Email", "PromptForEmail", "Prompt for Email", @"Should the user be prompted for their email address?", 2, @"True", "E61C9B70-43D0-4F22-B5F8-83AFFA205E66" );

            // Attribute for BlockType: Transaction Entry:Address Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Address Type", "PersonAddressType", "Address Type", @"The location type to use for the person's address", 3, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "4388B926-0F84-48B6-818C-D940AEE64B7C" );

            // Attribute for BlockType: Transaction Entry:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "PersonConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Web Prospect'.)", 4, @"368DD475-242C-49C4-A42C-7278BE690CC2", "E0C373C1-DB32-48A8-9D43-3CF86B5AC34F" );

            // Attribute for BlockType: Transaction Entry:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "PersonRecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 5, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "3A56F1DA-182A-4289-B54D-C09A7DB3DEFD" );

            // Attribute for BlockType: Transaction Entry:Transaction Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Type", "Transaction Type", "Transaction Type", @"", 1, @"2D607262-52D6-4724-910D-5C6E8FB89ACC", "31B731AA-F409-4A42-A1D0-A4FF3373FE84" );

            // Attribute for BlockType: Transaction Entry:Transaction Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Transaction Entity Type", "TransactionEntityType", "Transaction Entity Type", @"The Entity Type for the Transaction Detail Record (usually left blank)", 2, @"", "93A14901-CCE4-4D05-9560-94DA5ABD87C4" );

            // Attribute for BlockType: Transaction Entry:Entity Id Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Id Parameter", "EntityIdParam", "Entity Id Parameter", @"The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)", 3, @"", "91867D3F-2664-4175-B377-D69ED26B8913" );

            // Attribute for BlockType: Transaction Entry:Allowed Transaction Attributes From URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Allowed Transaction Attributes From URL", "AllowedTransactionAttributesFromURL", "Allowed Transaction Attributes From URL", @"Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world", 4, @"", "BE8DBB00-A23A-46DA-BB7A-003577AAAC94" );

            // Attribute for BlockType: Transaction Entry:Allow Account Options In URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Account Options In URL", "AllowAccountOptionsInURL", "Allow Account Options In URL", @"Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true", 5, @"False", "309B9D3C-769D-4854-ABA0-7BE343C4280C" );

            // Attribute for BlockType: Transaction Entry:Only Public Accounts In URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Only Public Accounts In URL", "OnlyPublicAccountsInURL", "Only Public Accounts In URL", @"Set to true if using the 'Allow Account Options In URL' option to prevent non-public accounts to be specified.", 6, @"True", "42536B06-956B-4569-B93F-08D359AAF733" );

            // Attribute for BlockType: Transaction Entry:Invalid Account Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid Account Message", "InvalidAccountInURLMessage", "Invalid Account Message", @"Display this text (HTML) as an error alert if an invalid 'account' or 'GL account' is passed through the URL. Leave blank to just ignore the invalid accounts and not show a message.", 7, @"", "3A17894A-5850-4E28-ACB1-1BFA0BEC92A0" );

            // Attribute for BlockType: Transaction Entry:Enable Initial Back button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Initial Back button", "EnableInitialBackButton", "Enable Initial Back button", @"Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry", 8, @"False", "EA73581D-83D1-4C4A-B8E8-D0288C4C9514" );

            // Attribute for BlockType: Transaction Entry:Impersonation
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "944E16D9-F697-4A15-BD53-896335E59ED8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Impersonation", "AllowImpersonation", "Impersonation", @"Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.", 9, @"False", "86B28765-A135-4732-B40B-35E92A906376" );

            // Attribute for BlockType: Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DEE4F9D9-A3C1-4BCD-B11F-904FE38EFD82", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "1C2F70ED-6AF8-4E25-8CC7-7BDF49ACFE0C" );

            // Attribute for BlockType: Login:Forgot Password URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DEE4F9D9-A3C1-4BCD-B11F-904FE38EFD82", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password URL", "ForgotPasswordUrl", "Forgot Password URL", @"The URL to link the user to when they have forgotten their password.", 1, @"", "C05DEA3C-CFD9-4361-831A-DFD31FCB53A7" );

            // Attribute for BlockType: Login:Locked Out Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DEE4F9D9-A3C1-4BCD-B11F-904FE38EFD82", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Locked Out Caption", "LockedOutCaption", "Locked Out Caption", @"The text (HTML) to display when a user's account has been locked.", 2, @"{%- assign phone = Global' | Attribute:'OrganizationPhone' | Trim -%}
{%- assign email = Global' | Attribute:'OrganizationEmail' | Trim -%}
Sorry, your account has been locked.  Please
{% if phone != '' %}
    contact our office at {{ phone }} or email
{% else %}
    email us at
{% endif %}
<a href='mailto:{{ email }}'>{{ email }}</a>
for help. Thank you.", "36C4DEAE-B0D0-4055-B21F-26415114838D" );

            // Attribute for BlockType: Login:Confirm Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DEE4F9D9-A3C1-4BCD-B11F-904FE38EFD82", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirm Caption", "ConfirmCaption", "Confirm Caption", @"The text (HTML) to display when a user's account needs to be confirmed.", 3, @"Thank you for logging in, however, we need to confirm the email associated with this account belongs to you. We’ve sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "C3048AD0-C86E-402B-B2C4-AF74713B8AFC" );

            // Attribute for BlockType: Login:Help Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DEE4F9D9-A3C1-4BCD-B11F-904FE38EFD82", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Help Page", "HelpPage", "Help Page", @"Page to navigate to when user selects 'Help' option (if blank will use 'ForgotUserName' page route)", 4, @"", "DF76295C-A9A6-4B98-86B7-D1246F4D3A59" );

            // Attribute for BlockType: Stark Detail:Show Email Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0CC256C9-E971-4C6C-B7A9-E4E8B8BBF559", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email Address", "ShowEmailAddress", "Show Email Address", @"Should the email address be shown?", 1, @"True", "76612759-ECCD-4419-8CFC-3D22CFE1D668" );

            // Attribute for BlockType: Stark Detail:Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0CC256C9-E971-4C6C-B7A9-E4E8B8BBF559", "3D045CAE-EA72-4A04-B7BE-7FD1D6214217", "Email", "Email", "Email", @"The Email address to show.", 2, @"ted@rocksolidchurchdemo.com", "B512319D-C9D6-4FB5-B9D2-08E71EC6990F" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Email Attribute for BlockType: Stark Detail
            RockMigrationHelper.DeleteAttribute("B512319D-C9D6-4FB5-B9D2-08E71EC6990F");

            // Show Email Address Attribute for BlockType: Stark Detail
            RockMigrationHelper.DeleteAttribute("76612759-ECCD-4419-8CFC-3D22CFE1D668");

            // Help Page Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("DF76295C-A9A6-4B98-86B7-D1246F4D3A59");

            // Confirm Caption Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("C3048AD0-C86E-402B-B2C4-AF74713B8AFC");

            // Locked Out Caption Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("36C4DEAE-B0D0-4055-B21F-26415114838D");

            // Forgot Password URL Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("C05DEA3C-CFD9-4361-831A-DFD31FCB53A7");

            // Registration Page Attribute for BlockType: Login
            RockMigrationHelper.DeleteAttribute("1C2F70ED-6AF8-4E25-8CC7-7BDF49ACFE0C");

            // Impersonation Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("86B28765-A135-4732-B40B-35E92A906376");

            // Enable Initial Back button Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("EA73581D-83D1-4C4A-B8E8-D0288C4C9514");

            // Invalid Account Message Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("3A17894A-5850-4E28-ACB1-1BFA0BEC92A0");

            // Only Public Accounts In URL Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("42536B06-956B-4569-B93F-08D359AAF733");

            // Allow Account Options In URL Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("309B9D3C-769D-4854-ABA0-7BE343C4280C");

            // Allowed Transaction Attributes From URL Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("BE8DBB00-A23A-46DA-BB7A-003577AAAC94");

            // Entity Id Parameter Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("91867D3F-2664-4175-B377-D69ED26B8913");

            // Transaction Entity Type Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("93A14901-CCE4-4D05-9560-94DA5ABD87C4");

            // Transaction Type Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("31B731AA-F409-4A42-A1D0-A4FF3373FE84");

            // Record Status Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("3A56F1DA-182A-4289-B54D-C09A7DB3DEFD");

            // Connection Status Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("E0C373C1-DB32-48A8-9D43-3CF86B5AC34F");

            // Address Type Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("4388B926-0F84-48B6-818C-D940AEE64B7C");

            // Prompt for Email Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("E61C9B70-43D0-4F22-B5F8-83AFFA205E66");

            // Prompt for Phone Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("54CB2276-5F67-40A8-85C9-BB4BC0366D0E");

            // Receipt Email Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("5319007C-475B-4269-BB34-D95EC37531DE");

            // Confirm Account Email Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("29EB295B-9D2B-40DE-8698-BD125B8DDE4F");

            // Finish Lava Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("A713A1C6-E5F7-4EEF-81B3-AE6D4D901246");

            // Amount Summary Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("7481CC07-A846-4777-BD24-3F4890BBBB5C");

            // Give Button Text - Scheduled Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("19533C6D-952A-4692-9B79-9DE793A2FB61");

            // Give Button Text - Now  Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("E30205F7-A4C9-46AA-9468-E1A6AAF7ADCD");

            // Gift Term Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("5EB02BA7-CA0D-4392-BB5D-E22017544E4B");

            // Intro Message Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("94B2B3F9-D5A5-4210-A59B-B015F27231BE");

            // Save Account Title Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("165BE92F-321D-4E79-83A2-16B79715A248");

            // Payment Comment Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("3DBBD02C-D113-40D9-A88F-BCA0E987138C");

            // Comment Entry Label Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("51C745E4-3ED9-4326-A99B-324E3DB1C189");

            // Enable Comment Entry Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("F89D8E87-ED5D-4EAF-9140-DD452B39EA1E");

            // Scheduled Transaction Edit Page Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("AD4F7077-A1AC-4261-B6FE-54A203BF25A5");

            // Scheduled Gifts Template Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("972BDC8F-5AF8-4509-8EBD-38B8474D4106");

            // Show Scheduled Gifts Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("92FEB4DD-2319-4D69-930A-49530607D6EA");

            // Scheduled Transactions Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("4AB1A22C-B2D5-4FDD-A797-F9083325878A");

            // Anonymous Giving Tool-tip Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("0A42E404-E6A1-44D3-91B9-70F2A570BB76");

            // Enable Anonymous Giving Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("582ADD35-C1E4-43A6-BC6B-7B3EBC8E4741");

            // Enable Business Giving Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("573B6958-013B-4C77-9771-2B067FE1E90C");

            // Enable Multi-Account Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("349C73A6-E84C-441D-9F7D-84FBF19ACE1F");

            // Campus Statuses Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("A4D1EFD2-22E3-4DCD-8DB7-A7EC0D9388B0");

            // Campus Types Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("22D9851E-B50E-4570-826A-148B9287C85C");

            // Include Inactive Campuses Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("89FB998F-FB49-4091-91F9-64F6351031F8");

            // Ask for Campus if Known Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("E489E02C-20DB-4AC4-88E2-513A77A0149D");

            // Accounts Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("AE860DA4-75AA-4857-A2BC-FF4A650ED4C7");

            // Financial Source Type Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("E5854089-EE19-4ECF-8526-17BD3BEF7406");

            // Batch Name Prefix Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("BD36EF80-FF09-45C7-9BCB-B94DAB425EA2");

            // Enable Credit Card Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("7412AFFC-7870-4727-8ECD-3FA7399C9208");

            // Enable ACH Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("766A4182-DEC0-4E21-921D-DBF10AB597DC");

            // Financial Gateway Attribute for BlockType: Transaction Entry
            RockMigrationHelper.DeleteAttribute("80FF9B9B-9B81-4A75-8E00-9EECDED2D3A4");

            // Enabled Saved Account Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("9659B79A-374B-4B47-9193-62ED33DB1AC1");

            // Show Field Descriptions Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("776CFF07-1C00-4213-A2B4-C830F74072CB");

            // Force Email Update Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("700E2AEA-B9FA-4EDC-B7E8-DA4B7EF6989F");

            // Family Term Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("C22B4472-ADB7-4F2F-AE2B-197C59BA9E46");

            // Allow InLine Digital Signature Documents Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("DC263886-B346-4B0D-92B9-A5D282D0303A");

            // Display Progress Bar Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("9ED859B6-F6C4-4E4F-8CDC-F0CC2CDD9E2E");

            // Batch Name Prefix Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("C60997F5-DA4C-4C9A-8D05-4F4BFC1BFEE4");

            // Source Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("FA60857D-74E1-483D-B381-A0CB20036500");

            // Record Status Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("54FCEC51-2355-4BD3-BF3A-E2B8A8721A42");

            // Connection Status Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("702647D6-487F-41A0-9D54-2DE8AF59464B");

            // Show Category Names as Separators Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("CB16AAA6-83B8-43F1-BE85-7044AFAF9640");

            // Block Icon Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("470EFF21-B851-4E5A-9F8E-C9D3BBFF67BA");

            // Block Title Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("2E58B2D7-8E53-4671-A56B-7E8064BC40A7");

            // Use Abbreviated Name Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("82B0F212-F133-4C11-A6F2-96C910EA0F79");

            // Attribute Order Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("0DB16ADD-A256-4333-8DBC-DC1FC02996E1");

            // Category Attribute for BlockType: Attribute Values
            RockMigrationHelper.DeleteAttribute("479E9091-71FC-41E5-B8B7-AA7F3F990F88");

            // Delete BlockType Stark Detail
            RockMigrationHelper.DeleteBlockType("0CC256C9-E971-4C6C-B7A9-E4E8B8BBF559"); // Stark Detail

            // Delete BlockType Login
            RockMigrationHelper.DeleteBlockType("DEE4F9D9-A3C1-4BCD-B11F-904FE38EFD82"); // Login

            // Delete BlockType Group Member List
            RockMigrationHelper.DeleteBlockType("1361D181-E383-4DDF-9E84-F7DE5496563D"); // Group Member List

            // Delete BlockType Transaction Entry
            RockMigrationHelper.DeleteBlockType("944E16D9-F697-4A15-BD53-896335E59ED8"); // Transaction Entry

            // Delete BlockType Person Secondary
            RockMigrationHelper.DeleteBlockType("8E2031CD-B833-42A6-B294-278FA7A32EAD"); // Person Secondary

            // Delete BlockType Person Detail
            RockMigrationHelper.DeleteBlockType("A272DF45-E97C-4DFD-B106-81F1F82DEA3C"); // Person Detail

            // Delete BlockType Large Dataset Grid
            RockMigrationHelper.DeleteBlockType("F04E508B-602F-4FFE-86A0-507672595EC8"); // Large Dataset Grid

            // Delete BlockType Field Type Gallery
            RockMigrationHelper.DeleteBlockType("EA2D0DDC-4EB9-45FE-A204-7F71E943737B"); // Field Type Gallery

            // Delete BlockType Control Gallery
            RockMigrationHelper.DeleteBlockType("050CD71E-1A98-4EC8-B453-C0540F02BEC6"); // Control Gallery

            // Delete BlockType Registration Entry
            RockMigrationHelper.DeleteBlockType("BDB939BC-AAE2-45F5-ADF7-6400223E041F"); // Registration Entry

            // Delete BlockType Attribute Values
            RockMigrationHelper.DeleteBlockType("AF08B18C-8AA4-4EE1-B930-B4EE757F0CA5"); // Attribute Values

            // Delete BlockType Widget List
            RockMigrationHelper.DeleteBlockType("13855051-B395-4DEA-AF3D-53325CC158EA"); // Widget List

            // Delete BlockType Context Group
            RockMigrationHelper.DeleteBlockType("0DF1D80D-A71B-471E-8ECC-C977C2C666A0"); // Context Group

            // Delete BlockType Context Entities
            RockMigrationHelper.DeleteBlockType("DA76850C-1801-463B-8078-B85C2B1C3965"); // Context Entities

        }
    }
}
