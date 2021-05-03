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
    public partial class FixFailedPaymentCommunicationTemplate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixFailedPaymentTemplate();
        }

        private void FixFailedPaymentTemplate()
        {
            // Fixes issue where Template Editor would corrupt the HTML on template that had Lava logic in them.
            // This migration fixes any corruption that might have happened on the 'Failed Communication' Template which is the only core template that had that problem
            // NOTE the REPLACE(asdf , char(13) + char(10), char(10)) on stuff to account for inconsistant line endings
            Sql( @"
update SystemCommunication set Body = Replace('{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Transaction.AuthorizedPersonAlias.Person.NickName }}, 
</p>
<p>
{% if Transaction.ScheduledTransaction %}
    We just wanted to make you aware that your gift to {{ ''Global'' | Attribute:''OrganizationName'' }} that was scheduled for {{ Transaction.TransactionDateTime | Date:''M/d/yyyy'' }} in the amount of 
    {{ Transaction.ScheduledTransaction.TotalAmount | FormatAsCurrency }} did not process successfully. If you''d like, you can update your giving profile at 
    <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
{% else %}
    {% assign amount = Transaction.TotalAmount %}
    {% if amount < 0 %}{% assign amount = 0 | Minus:amount %}{% endif %}
    We just wanted to make you aware that your {{ Transaction.TransactionTypeValue.Value | Downcase }} payment on {{ Transaction.TransactionDateTime | Date:''M/d/yyyy'' }} in the amount of 
    {{ amount | FormatAsCurrency }} did not process successfully. 
    {% if Transaction.TransactionTypeValue.Value == ''Contribution'' %}
        If you''d like, you can re-submit your contribution at  
        <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
    {% endif %}
{% endif %}
</p>
<p>
    Below are the details of your transaction that we were unable to process.
</p>

<p>
<strong>Txn Code:</strong> {{ Transaction.TransactionCode }}<br/>
<strong>Status:</strong> {{ Transaction.Status }}<br/>
<strong>Status Message:</strong> {{ Transaction.StatusMessage }}
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}', char(13) + char(10), char(10))
WHERE [Guid] = '449232B5-9C6B-480E-A881-E317D0BC307E'

IF EXISTS ( SELECT [Id] FROM [SystemCommunication] WHERE [Guid] = '449232B5-9C6B-480E-A881-E317D0BC307E' )
BEGIN
	UPDATE [SystemCommunication] SET
		[Body] = REPLACE( [BODY], 
		-- bad code
		Replace('{% if Transaction.ScheduledTransaction %}
    We just wanted to make you aware that your gift to {{ ''Global'' | Attribute:''OrganizationName'' }} that was scheduled for {{ Transaction.TransactionDateTime | Date:''M/d/yyyy'' }} in the amount of 
    {{ Transaction.ScheduledTransaction.TotalAmount | FormatAsCurrency }} did not process successfully. If you''d like, you can update your giving profile at 
    <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
{% else %}
    {% assign amount = Transaction.TotalAmount %}
    {% if amount < 0="""" %}{%="""" assign="""" amount=""0"" |="""" minus:amount="""" %}{%="""" endif="""" %}="""" we="""" just="""" wanted="""" to="""" make="""" you="""" aware="""" that="""" your="""" {{="""" transaction.transactiontypevalue.value="""" |="""" downcase="""" }}="""" payment="""" on="""" {{="""" transaction.transactiondatetime="""" |="""" date:''m/d/yyyy''="""" }}="""" in="""" the="""" amount="""" of="""" {{="""" amount="""" |="""" formatascurrency="""" }}="""" did="""" not="""" process="""" successfully.="""" {%="""" if="""" transaction.transactiontypevalue.value=""="" ''contribution''="""" %}="""" if="""" you''d="""" like,="""" you="""" can="""" re-submit="""" your="""" contribution="""" at=""""><a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
    {% endif %}
{% endif %}', char(13) + char(10), char(10)),
		-- with good code
		Replace('{% if Transaction.ScheduledTransaction %}
    We just wanted to make you aware that your gift to {{ ''Global'' | Attribute:''OrganizationName'' }} that was scheduled for {{ Transaction.TransactionDateTime | Date:''M/d/yyyy'' }} in the amount of 
    {{ Transaction.ScheduledTransaction.TotalAmount | FormatAsCurrency }} did not process successfully. If you''d like, you can update your giving profile at 
    <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
{% else %}
    {% assign amount = Transaction.TotalAmount %}
    {% if amount < 0 %}{% assign amount = 0 | Minus:amount %}{% endif %}
    We just wanted to make you aware that your {{ Transaction.TransactionTypeValue.Value | Downcase }} payment on {{ Transaction.TransactionDateTime | Date:''M/d/yyyy'' }} in the amount of 
    {{ amount | FormatAsCurrency }} did not process successfully. 
    {% if Transaction.TransactionTypeValue.Value == ''Contribution'' %}
        If you''d like, you can re-submit your contribution at  
        <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
    {% endif %}
{% endif %}', char(13) + char(10), char(10)) )
	WHERE [Guid] = '449232B5-9C6B-480E-A881-E317D0BC307E'
END

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
