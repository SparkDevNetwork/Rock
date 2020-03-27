using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MailChimp.Net;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Web.Cache;
using System.Data.Entity;

namespace com.bemaservices.MailChimp.Utility
{
    class MailChimpApi
    {
        private IMailChimpManager _mailChimpManager;
        private string _apiKey;
        private DefinedValueCache _mailChimpAccount;

        public MailChimpApi( DefinedValueCache mailChimpAccount )
        {
            if( !mailChimpAccount.DefinedType.Guid.ToString().Equals(MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS, StringComparison.OrdinalIgnoreCase) )
            {
                var newException = new Exception( "Defined Value is not of type Mail Chimp Account." );
                ExceptionLogService.LogException( newException );
            }
            else
            {
                _mailChimpAccount = mailChimpAccount;

                var apiKeyAttributeKey = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE ).Key;
                var apiKey = _mailChimpAccount.GetAttributeValue( apiKeyAttributeKey );

                if( apiKey.IsNullOrWhiteSpace() )
                {
                    var newException = new Exception( "No Api Key provided on the Mail Account Defined Value" );
                    ExceptionLogService.LogException( newException );
                }
                else
                {
                    _mailChimpManager = new MailChimpManager( apiKey );
                }
            }
        }

        public List<DefinedValue> GetMailChimpLists()
        {
            List<DefinedValue> mailChimpListValues = null;

            if ( _mailChimpAccount is null || _apiKey.IsNullOrWhiteSpace() )
            {
                var newException = new Exception( "The Helper Class has not been properly intialized with a valid Mail Chimp Account Defined Value, or the Mail Chimp Account does not have an APIKEY." );
                ExceptionLogService.LogException( newException );
            }
            else
            {
                RockContext rc = new RockContext();
                DefinedValueService dvs = new DefinedValueService( rc );
                AttributeValueService avs = new AttributeValueService( rc );

                mailChimpListValues = dvs.GetByDefinedTypeGuid( new Guid( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_LISTS ) ).ToList();

                try
                {
                    var mailChimpListCollection = _mailChimpManager.Lists.GetAllAsync().Result;

                    // Loop over each List from Mail Chimp and attempt to find the related Defined Value in Rock.  Then Update / Add those defined values into Rock
                    foreach ( var mailChimpList in mailChimpListCollection )
                    {
                        var mailChimpListValue = mailChimpListValues.Where( x => x.ForeignId == Int32.Parse( mailChimpList.Id ) && x.ForeignKey == MailChimp.Constants.ForeignKey ).FirstOrDefault();
                        UpdateMailChimpListDefinedValue( mailChimpList, ref mailChimpListValue, rc );
                    }

                    // Look for any DefinedValues in Rock that are no longer in Mail Chimp and remove them.  We also need to remove any attribute Values assigned to these lists.
                    var mailChimpListValuesToRemove = mailChimpListValues.Where( x => !mailChimpListCollection.Any( y => Int32.Parse( y.Id ) == x.ForeignId && x.ForeignKey == MailChimp.Constants.ForeignKey ) );
                    var attributeValuesToRemove = avs.Queryable().Where( av => mailChimpListValuesToRemove.Any( dv => dv.Guid.ToString() == av.Value ) );

                    foreach ( var definedValue in mailChimpListValuesToRemove )
                    {
                        mailChimpListValues.Remove( definedValue );
                    }

                    avs.DeleteRange( attributeValuesToRemove );
                    dvs.DeleteRange( mailChimpListValuesToRemove );

                    rc.SaveChanges();
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex ); 
                }
                
            }

            return mailChimpListValues;
        }

        public void SyncMembers( DefinedValueCache mailChimpList )
        {
            RockContext rc = new RockContext();
            GroupMemberService gms = new GroupMemberService( rc );
            PersonService ps = new PersonService( rc );
            AttributeValueService avs = new AttributeValueService( rc );

            try
            {
                var mailChimpMembers = _mailChimpManager.Members.GetAllAsync( mailChimpList.ForeignId.ToString() ).Result;

                //Get all Groups that have an attribute set to this Mail Chimp List's Defined Value.
                var groupIds = avs.Queryable().AsNoTracking().Where( x => x.Value.Equals( mailChimpList.Guid.ToString(), StringComparison.OrdinalIgnoreCase ) && x.Attribute.EntityType.FriendlyName == Rock.Model.Group.FriendlyTypeName ).Select( x => x.EntityId );

                if ( groupIds.Any() )
                {
                    foreach ( var groupId in groupIds )
                    {
                        var rockGroupMembers = gms.GetByGroupId( groupId.Value );
                    }
                }
            }
            catch( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }
           

        }

        private void UpdateMailChimpListDefinedValue( List mailChimpList, ref DefinedValue mailChimpListValue, RockContext rockContext )
        {
            if( mailChimpListValue is null )
            {
                mailChimpListValue = new DefinedValue();
                mailChimpListValue.ForeignId = Int32.Parse( mailChimpList.Id );
                mailChimpListValue.ForeignKey = MailChimp.Constants.ForeignKey;
                mailChimpListValue.IsSystem = true;
            }

            var mailChimpAccountAttribute = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE );

            // Add the Mail Chimp Account to the List's Attribute.
            Rock.Attribute.Helper.SaveAttributeValue( mailChimpListValue, mailChimpAccountAttribute, _mailChimpAccount.Guid.ToString(), rockContext );

            mailChimpListValue.Value = mailChimpList.Name;
            mailChimpListValue.Description = mailChimpList.SubscribeUrlLong;
        }

    }
}
