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
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v15 to replace all TransactionEntry blocks with the UtilityPaymentEntry block
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v15.0 - Replace TransactionEntry Blocks with UtilityPaymentEntry block." )]
    [Description( "This job will replace all existing instances of the TransactionEntryBlock with a new instance of the UtilityPayment block whiles preserving block settings." )]

    [IntegerField(
    "Command Timeout",
    AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV15DataMigrationsReplaceTransactionEntryBlocksWithUtilityPaymentBlock : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// The set up SQL format, sets up the variables to be used in swapping the block instances an copying over existing attribute values.
        /// </summary>
        private const string setUpSqlFormat = @"
DECLARE @PageId INT = {0}
DECLARE @EntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
DECLARE @BlockToBeReplacedBlockId INT = {5}
DECLARE @BlockToBeReplacedBlockTypeId INT = {1}
DECLARE @NewBlockBlockTypeId INT = (SELECT Id FROM [BlockType] WHERE [Guid] = '{2}')

/* Add New Block instance to page */
IF NOT EXISTS (select * from [Block] where [Guid] = '{3}')
BEGIN			
	INSERT INTO [Block] (
	    [IsSystem],[PageId],[BlockTypeId],[Zone],
	    [Order],[Name],[PreHtml],[PostHtml],[OutputCacheDuration],
	    [Guid])
	VALUES(
	    1,@PageId,@NewBlockBlockTypeId,'Main',
	    0,'{4}','','',0,
	    '{3}')

END

DECLARE @NewBlockBlockId AS INT = (SELECT Id FROM Block WHERE Guid = '{3}')
DECLARE @NewBlockBlockAttributeId AS INT
DECLARE @NewBlockBlockAttributeValueId AS INT
DECLARE @BlockToBeReplacedBlockAttributeId AS INT
DECLARE @BlockToBeReplacedBlockAttributeValueId AS INT
DECLARE @BlockToBeReplacedBlockAttributeValue AS NVARCHAR(MAX)
";

        /// <summary>
        /// The copy attribute value SQL format, finds existing attribute values for the existing block so they can be copied and saved as values for the replacement/new block.
        /// </summary>
        private const string copyAttributeValueSqlFormat = @"

SET @BlockToBeReplacedBlockAttributeId = (SELECT Id FROM [Attribute]
	WHERE EntityTypeId = @EntityTypeId
	AND EntityTypeQualifierValue = @BlockToBeReplacedBlockTypeId
	AND EntityTypeQualifierColumn = 'BlockTypeId' and [Key] = '{1}')

SET @BlockToBeReplacedBlockAttributeValueId = (SELECT Id FROM AttributeValue
	WHERE EntityId = @BlockToBeReplacedBlockId
	AND AttributeId = @BlockToBeReplacedBlockAttributeId)

IF (@BlockToBeReplacedBlockAttributeValueId IS NOT NULL)
BEGIN

    SET @NewBlockBlockAttributeId = (SELECT Id FROM [Attribute]
	    WHERE EntityTypeId = @EntityTypeId
	    AND EntityTypeQualifierValue = @NewBlockBlockTypeId
	    AND EntityTypeQualifierColumn = 'BlockTypeId' and [Key] = '{0}')

    SET @NewBlockBlockAttributeValueId = (SELECT Id FROM [AttributeValue]
        WHERE [AttributeId] = @NewBlockBlockAttributeId
        AND [EntityId] = @NewBlockBlockId)

    SET @BlockToBeReplacedBlockAttributeValue = (SELECT Value FROM AttributeValue
    	WHERE Id = @BlockToBeReplacedBlockAttributeValueId)

    IF (@NewBlockBlockAttributeId IS NOT NULL)
    BEGIN
        IF (@NewBlockBlockAttributeValueId IS NULL)
        
        BEGIN
        	INSERT INTO [AttributeValue] (
        		[IsSystem],[AttributeId],[EntityId],
        		[Value],
        		[Guid])
        	VALUES(
        		1,@NewBlockBlockAttributeId,@NewBlockBlockId,
        		@BlockToBeReplacedBlockAttributeValue,
        		NEWID())
        END
        ELSE
        BEGIN
        	UPDATE [AttributeValue]
        	SET [Value] = @BlockToBeReplacedBlockAttributeValue
        	WHERE Id = @NewBlockBlockAttributeValueId
        END
    END
END
";

        /// <summary>
        /// The create or update attribute value SQL format, adds new attribute values to the replacement/new block that do not exist for the existing block instance
        /// </summary>
        private const string createOrUpdateAttributeValueSqlFormat = @"
SET @NewBlockBlockAttributeId = (SELECT Id FROM [Attribute]
	WHERE EntityTypeId = @EntityTypeId
	AND EntityTypeQualifierValue = @NewBlockBlockTypeId
	AND EntityTypeQualifierColumn = 'BlockTypeId' and [Key] = '{0}')

SET @NewBlockBlockAttributeValueId = (SELECT Id FROM [AttributeValue]
    WHERE [AttributeId] = @NewBlockBlockAttributeId
    AND [EntityId] = @NewBlockBlockId)

IF @NewBlockBlockAttributeValueId IS NULL
BEGIN
	INSERT INTO [AttributeValue] (
		[IsSystem],[AttributeId],[EntityId],
		[Value],
		[Guid])
	VALUES(
		1,@NewBlockBlockAttributeId,@NewBlockBlockId,
		'{1}',
		NEWID())
END
ELSE
BEGIN
	UPDATE [AttributeValue]
	SET [Value] = '{1}'
	WHERE Id = @NewBlockBlockAttributeValueId
END
";

        /// <summary>
        /// The delete old block SQL, run after new block instance is added to page, removed the existing block instance.
        /// </summary>
        private const string deleteOldBlockSql = @"
DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @BlockToBeReplacedBlockId
DELETE [Block] WHERE [Id] = @BlockToBeReplacedBlockId
";

        /// <summary>
        /// The default success header
        /// </summary>
        private const string defaultSuccessHeader = @"
<p>
    Thank you for your generous contribution.  Your support is helping {{ ''Global'' | Attribute:''OrganizationName'' }} actively
    achieve our mission.  We are so grateful for your commitment.
</p>
";

        /// <summary>
        /// The default success title
        /// </summary>
        private const string defaultSuccessTitle = "Gift Information";

        /// <summary>
        /// The finish lava template
        /// </summary>
        private const string finishLavaTemplate = @"
<div class=""well"">

    <legend>##SUCCESS_TITLE##</legend>
  
    ##SUCCESS_HEADER##

    {% if Transaction.ScheduledTransactionDetails %}
        {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
    {% else %}
        {% assign transactionDetails = Transaction.TransactionDetails %}
    {% endif %}
    
    <dl class=""dl-horizontal"">
        <dt>Confirmation Code</dt>
        <dd>{{ Transaction.TransactionCode }}</dd>
        <dd></dd>
    </dl>
    
    <dl class=""dl-horizontal gift-success"">
        <dt>Name</dt>
        <dd>{{ Person.FullName }}</dd>
        <dd></dd>
        
        <dt>Email</dt>
        <dd>{{ Person.Email }}</dd>
        <dd></dd>
        
        <dt>Address</dt>
        <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
        <dd></dd>
    </dl>
    
    <dl class=""dl-horizontal"">
        {% for transactionDetail in transactionDetails %}
            <dt>{{ transactionDetail.Account.PublicName }}</dt>
            <dd>{{ transactionDetail.Amount | Minus: transactionDetail.FeeCoverageAmount | FormatAsCurrency }}</dd>
        {% endfor %}
            <dt>Total</dt>
            <dd>{{ transactionDetails | Select: ''Amount'' | Sum | FormatAsCurrency }}</dd>
        {% if Transaction.TotalFeeCoverageAmount %}
            <dt>Fee Coverage</dt>
            <dd>{{ Transaction.TotalFeeCoverageAmount | FormatAsCurrency }}</dd>
        {% endif %}
        <dd></dd>
    </dl>
    
    <dl class=""dl-horizontal"">
            <dt>Payment Method</dt>
        <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>
    
        {% if PaymentDetail.AccountNumberMasked  != '''' %}
            <dt>Account Number</dt>
            <dd>{{ PaymentDetail.AccountNumberMasked }}</dd>
        {% endif %}
    
        <dt>When<dt>
        <dd>
    
        {% if Transaction.TransactionFrequencyValue %}
            {{ Transaction.TransactionFrequencyValue.Value }} starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}
        {% else %}
            Today
        {% endif %}
        </dd>
    </dl>
</div>
";

        /// <summary>
        /// The attribute key mappings, attribute key mappings for attributes with different keys but same functionality
        /// </summary>
        private readonly List<AttributeKeyMapping> attributeKeyMappings = new List<AttributeKeyMapping>()
        {
            // Attributes with different keys but same functionalities
            new AttributeKeyMapping( "CCGateway", "FinancialGateway"  ),
            new AttributeKeyMapping( "PaymentComment", "PaymentCommentTemplate"  ),
            new AttributeKeyMapping( "AllowAccountsInURL", "AllowAccountOptionsInURL"  ),
            new AttributeKeyMapping( "Accounts", "AccountsToDisplay"  ),
        };

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                // BlockType to be replaced
                var transactionEntryBlockTypeGuid = "74EE3481-3E5A-4971-A02E-D463ABB45591".AsGuid();
                var blockService = new BlockService( rockContext );
                var attributeService = new AttributeService( rockContext );
                var attributeValueService = new AttributeValueService( rockContext );
                List<BlockSwapMigrationHelperClass> blockSwapHelpers = new List<BlockSwapMigrationHelperClass>();

                // Get all current transaction entry block instances except the one on the Fundraising Transaction Entry Page.
                foreach ( var block in blockService.Queryable().Where( p => p.BlockType.Guid == transactionEntryBlockTypeGuid ).ToList() )
                {
                    // Get all the block attributes of the old/existing block except the ones with different keys (those will be mapped manually)
                    var differentAttributeKeys = attributeKeyMappings.Select( m => m.OldBlockKey );
                    var blockAttributes = attributeService.Queryable()
                        .Where( a =>
                            a.EntityType.Name == "Rock.Model.Block"
                            && a.EntityTypeQualifierColumn == "BlockTypeId"
                            && a.EntityTypeQualifierValue == block.BlockTypeId.ToString()
                            && !differentAttributeKeys.Contains( a.Key ) )
                        .ToList();
                    Dictionary<string, string> attributesToAdd = new Dictionary<string, string>();

                    // Add to the attributeKeyMappings, this represents the list of attributes whose key match the replacement block's attribute keys
                    // if those attributes have any values they will be copied over to the replacement block instance.
                    attributeKeyMappings.AddRange( blockAttributes.Select( a => new AttributeKeyMapping() { NewBlockKey = a.Key, OldBlockKey = a.Key } ) );

                    // List of attribute values which serve the same functionality but have different keys or field types. These need custom logic to carry over their
                    // functionality to the new block instance.
                    var manuallyMappedAttributeValues = attributeValueService.Queryable()
                        .Where( a =>
                            ( a.Attribute.Key == "SuccessHeader"
                            || a.Attribute.Key == "SuccessTitle"
                            || a.Attribute.Key == "ACHGateway"
                            || a.Attribute.Key == "CCGateway" )
                            && a.EntityId == block.Id )
                        .ToList();

                    var blockFinishLavaTemplate = finishLavaTemplate;
                    foreach ( var attributeValue in manuallyMappedAttributeValues )
                    {
                        switch ( attributeValue.AttributeKey )
                        {
                            case "SuccessHeader":
                                // Escape any single quote so it doesn't break the sql script.
                                var successHeader = attributeValue.Value.Replace( "'", "''" );
                                // Success header from the TransactionEntry block uses 'FinancialTransaction' as its lava variable name for the Transaction entity,
                                // the FinishLavaTemplate however uses 'Transaction' so we replace 'FinancialTransaction' variable names with 'Transaction'.
                                successHeader = successHeader.Replace( "{{ FinancialTransaction.", "{{ Transaction." );
                                blockFinishLavaTemplate = blockFinishLavaTemplate.Replace( "##SUCCESS_HEADER##", successHeader );
                                break;
                            case "SuccessTitle":
                                // Escape any single quotes so it doesn't break the sql script.
                                var successTitle = attributeValue.Value.Replace( "'", "''" );
                                blockFinishLavaTemplate = blockFinishLavaTemplate.Replace( "##SUCCESS_TITLE##", successTitle );
                                break;
                            case "ACHGateway":
                                attributesToAdd.AddOrReplace( "EnableACH", "True" );
                                break;
                            case "CCGateway":
                                attributesToAdd.AddOrReplace( "EnableCreditCard", "True" );
                                break;
                        }
                    }

                    // If the above loop did not replace the SUCCESS_HEADER or SUCCESS_TITLE do that here.
                    blockFinishLavaTemplate = blockFinishLavaTemplate.Replace( "##SUCCESS_HEADER##", defaultSuccessHeader );
                    blockFinishLavaTemplate = blockFinishLavaTemplate.Replace( "##SUCCESS_TITLE##", defaultSuccessTitle );

                    attributesToAdd.Add( "FinishLavaTemplate", blockFinishLavaTemplate );

                    // Create helper class for each page with the Transaction Entry Block instance the help with the swap
                    // from the Transaction Entry Block to the UtilityPaymentEntry block.
                    var helperClass = new BlockSwapMigrationHelperClass()
                    {
                        AttributesToCopy = attributeKeyMappings,
                        AttributesToAdd = attributesToAdd,
                        NewBlockBlockTypeGuid = "4CCC45A5-4AB9-4A36-BF8D-A6E316790004",
                        NewBlockGuid = Guid.NewGuid().ToString(),
                        NewBlockName = block.Name,
                        OldBlockId = block.Id,
                        OldBlockTypeId = block.BlockTypeId,
                        PageId = block.PageId.GetValueOrDefault(),
                    };

                    blockSwapHelpers.Add( helperClass );
                }

                // Iterate over the helper classes to set the block details into the SQL script.
                foreach ( var blockSwapHelper in blockSwapHelpers )
                {
                    var setUpSql = string.Format( setUpSqlFormat,
                        blockSwapHelper.PageId,// 0
                        blockSwapHelper.OldBlockTypeId,// 1 
                        blockSwapHelper.NewBlockBlockTypeGuid,// 2
                        blockSwapHelper.NewBlockGuid,// 3
                        blockSwapHelper.NewBlockName,// 4
                        blockSwapHelper.OldBlockId // 5
                        );
                    var stringBuilder = new StringBuilder( setUpSql );

                    if ( blockSwapHelper.AttributesToCopy != null )
                    {
                        foreach ( var attributeKey in blockSwapHelper.AttributesToCopy )
                        {
                            stringBuilder.AppendFormat( copyAttributeValueSqlFormat, attributeKey.NewBlockKey, attributeKey.OldBlockKey );
                        }
                    }

                    if ( blockSwapHelper.AttributesToAdd != null )
                    {
                        foreach ( var newAttributeValue in blockSwapHelper.AttributesToAdd )
                        {
                            stringBuilder.AppendFormat( createOrUpdateAttributeValueSqlFormat, newAttributeValue.Key, newAttributeValue.Value );
                        }
                    }

                    stringBuilder.AppendLine( deleteOldBlockSql );

                    rockContext.Database.ExecuteSqlCommand( stringBuilder.ToString() );
                }
            }

            DeleteJob( GetJobId() );
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public static void DeleteJob( int jobId )
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Helper class to help swap an existing instance of a block for a new one.
        /// </summary>
        private sealed class BlockSwapMigrationHelperClass
        {
            /// <summary>
            /// Gets or sets the page identifier of the page with the block instance.
            /// </summary>
            /// <value>
            /// The page identifier.
            /// </value>
            public int PageId { get; set; }
            /// <summary>
            /// Gets or sets the name of the new block instance.
            /// </summary>
            /// <value>
            /// The new name of the block.
            /// </value>
            public string NewBlockName { get; set; }
            /// <summary>
            /// Gets or sets the unique identifier of the new block instance
            /// </summary>
            /// <value>
            /// The new block unique identifier.
            /// </value>
            public string NewBlockGuid { get; set; }
            /// <summary>
            /// Gets or seys the unique identifier of the BlockType of the new block instance
            /// </summary>
            /// <value>
            /// The new block BlockType unique identifier.
            /// </value>
            public string NewBlockBlockTypeGuid { get; set; }
            /// <summary>
            /// Gets or sets the old block type identifier.
            /// </summary>
            /// <value>
            /// The old block type identifier.
            /// </value>
            public int OldBlockTypeId { get; set; }
            /// <summary>
            /// Gets or sets the old block identifier.
            /// </summary>
            /// <value>
            /// The old block identifier.
            /// </value>
            public int OldBlockId { get; set; }

            /// <summary>
            /// Gets or sets the attributes to copy over from the existing block instance to the replacement block instance.
            /// </summary>
            /// <value>
            /// The attributes to copy.
            /// </value>
            public List<AttributeKeyMapping> AttributesToCopy { get; set; }
            /// <summary>
            /// Gets or sets the attributes values to add to the replacement instance.
            /// </summary>
            /// <value>
            /// The attributes to add.
            /// </value>
            public Dictionary<string, string> AttributesToAdd { get; set; }
        }

        /// <summary>
        /// Helper class used primarily to map attribute keys between the 2 block instances about to be swapped
        /// </summary>
        private sealed class AttributeKeyMapping
        {
            public AttributeKeyMapping()
            {
            }

            public AttributeKeyMapping( string key ) : this( key, key )
            {
            }

            public AttributeKeyMapping( string oldBlockKey, string newBlockKey )
            {
                OldBlockKey = oldBlockKey;
                NewBlockKey = newBlockKey;
            }

            public string OldBlockKey { get; set; }
            public string NewBlockKey { get; set; }
        }
    }
}
