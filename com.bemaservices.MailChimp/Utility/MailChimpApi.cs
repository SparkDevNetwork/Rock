using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MCNet = MailChimp.Net;
using MCInterfaces = MailChimp.Net.Interfaces;
using MCModels = MailChimp.Net.Models;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Web.Cache;
using System.Data.Entity;
using MailChimp.Net.Core;
using DotLiquid.Tags;
using MailChimp.Net.Models;

namespace com.bemaservices.MailChimp.Utility
{
    public class MailChimpApi
    {
        private MCInterfaces.IMailChimpManager _mailChimpManager;
        private string _apiKey;
        private DefinedValueCache _mailChimpAccount;

        public MailChimpApi( DefinedValueCache mailChimpAccount )
        {
            if ( !mailChimpAccount.DefinedType.Guid.ToString().Equals( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS, StringComparison.OrdinalIgnoreCase ) )
            {
                var newException = new Exception( "Defined Value is not of type Mail Chimp Account." );
                ExceptionLogService.LogException( newException );
            }
            else
            {
                _mailChimpAccount = mailChimpAccount;

                var apiKeyAttributeKey = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE ).Key;
                _apiKey = _mailChimpAccount.GetAttributeValue( apiKeyAttributeKey );

                if ( _apiKey.IsNullOrWhiteSpace() )
                {
                    var newException = new Exception( "No Api Key provided on the Mail Account Defined Value" );
                    ExceptionLogService.LogException( newException );
                }
                else
                {
                    _mailChimpManager = new MCNet.MailChimpManager( _apiKey );
                }
            }
        }

        public List<DefinedValue> GetMailChimpLists()
        {
            RockContext rockContext = new RockContext();
            DefinedValueService definedValueService = new DefinedValueService( rockContext );
            AttributeValueService attributeValueService = new AttributeValueService( rockContext );

            List<DefinedValue> mailChimpListValues = null;
            List<int?> mailChimpListDefinedValueIds = new List<int?>();

            if ( _mailChimpAccount is null || _apiKey.IsNullOrWhiteSpace() )
            {
                var newException = new Exception( "The Helper Class has not been properly intialized with a valid Mail Chimp Account Defined Value, or the Mail Chimp Account does not have an APIKEY." );
                ExceptionLogService.LogException( newException );
            }
            else
            {
                try
                {
                    mailChimpListDefinedValueIds = attributeValueService.GetByAttributeId( AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_AUDIENCE_ACCOUNT_ATTRIBUTE ).Id )
                                                  .Where( av => av.Value.Equals( _mailChimpAccount.Guid.ToString() ) ).Select( av => av.EntityId )
                                                  .ToList();

                    mailChimpListValues = definedValueService.GetByDefinedTypeGuid( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_AUDIENCES.AsGuid() )
                        .Where( v => mailChimpListDefinedValueIds.Contains( v.Id ) ).ToList();

                }
                catch ( Exception ex )
                {
                    string message = String.Format( "Error Grabbing Mailchimp Audiences from Rock. This is most likely due to the configured Mailchimp account being invalid or closed. Please check your configuration to verify you are using an active Mailchimp account." );
                    ExceptionLogService.LogException( new Exception( message ) );
                }

                try
                {
                    var mailChimpListCollection = _mailChimpManager.Lists.GetAllAsync().Result;

                    // Loop over each List from Mail Chimp and attempt to find the related Defined Value in Rock.  Then Update / Add those defined values into Rock
                    foreach ( var mailChimpList in mailChimpListCollection )
                    {
                        try
                        {
                            var mailChimpListValue = mailChimpListValues.Where( x => x.ForeignId == mailChimpList.WebId &&
                                                                                x.ForeignKey == MailChimp.Constants.ForeignKey )
                                                                    .FirstOrDefault();
                            if ( mailChimpListValue is null )
                            {
                                try
                                {
                                    mailChimpListValue = new DefinedValue();
                                    mailChimpListValue.ForeignId = mailChimpList.WebId;
                                    mailChimpListValue.ForeignKey = MailChimp.Constants.ForeignKey;
                                    mailChimpListValue.IsSystem = true;
                                    mailChimpListValue.DefinedTypeId = DefinedTypeCache.Get( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_AUDIENCES.AsGuid() ).Id;
                                    mailChimpListValue.Value = mailChimpList.Name;

                                    definedValueService.Add( mailChimpListValue );

                                    rockContext.SaveChanges();
                                }
                                catch ( Exception ex )
                                {
                                    string message = String.Format( "Error Adding {0} to Rock", mailChimpList.Name );
                                    ExceptionLogService.LogException( new Exception( message, ex ) );
                                }
                            }

                            try
                            {
                                UpdateMailChimpListDefinedValue( mailChimpList, ref mailChimpListValue, rockContext );
                            }
                            catch ( Exception ex )
                            {
                                string message = String.Format( "Error Updating {0}'s Defined Value", mailChimpList.Name );
                                ExceptionLogService.LogException( new Exception( message, ex ) );
                            }
                        }
                        catch ( Exception ex )
                        {
                            string message = String.Format( "Error Loading {0}", mailChimpList.Name );
                            ExceptionLogService.LogException( new Exception( message, ex ) );
                        }

                    }

                    try
                    {
                        // Look for any DefinedValues in Rock that are no longer in Mail Chimp and remove them.
                        var mailChimpListValuesToRemove = mailChimpListValues
                                                           .Where( x => !mailChimpListCollection.Any( y => y.WebId == x.ForeignId && x.ForeignKey == MailChimp.Constants.ForeignKey )
                                                           && mailChimpListDefinedValueIds.Contains( x.Id )
                                                           );

                        definedValueService.DeleteRange( mailChimpListValuesToRemove );

                        rockContext.SaveChanges();
                    }
                    catch ( Exception ex )
                    {
                        string message = String.Format( "Error Removing deleted lists from Rock" );
                        ExceptionLogService.LogException( new Exception( message, ex ) );
                    }

                    return definedValueService.GetByDefinedTypeGuid( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_AUDIENCES.AsGuid() )
                                           .Where( v => mailChimpListDefinedValueIds.Contains( v.Id ) )
                                           .ToList();
                }
                catch ( Exception ex )
                {
                    string message = String.Format( "Error Grabbing Mailchimp Lists from Mailchimp" );
                    ExceptionLogService.LogException( new Exception( message, ex ) );
                }
            }

            return null;
        }

        public void SyncMembers( DefinedValueCache mailChimpList, int? daysToSyncUpdates = null )
        {
            Dictionary<int, MCModels.Member> mailChimpMemberLookUp = new Dictionary<int, MCModels.Member>();
            var mailChimpListIdAttributeKey = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_AUDIENCE_ID_ATTRIBUTE.AsGuid() ).Key;
            var mailChimpListId = mailChimpList.GetAttributeValue( mailChimpListIdAttributeKey );

            DateTime? dateLimit = null;
            if ( daysToSyncUpdates.HasValue )
            {
                dateLimit = RockDateTime.Now.AddDays( daysToSyncUpdates.Value * -1 );
            }

            try
            {
                int offset = 0;
                bool moreRecordsToFetch = true;
                var memberRequest = new MemberRequest();
                var mailChimpMembers = new List<MCModels.Member>();
                memberRequest.Limit = 1000;

                if ( dateLimit.HasValue )
                {
                    memberRequest.SinceLastChanged = dateLimit.ToISO8601DateString();
                }
                try
                {
                    while ( moreRecordsToFetch )
                    {
                        memberRequest.Offset = offset;

                        var result = _mailChimpManager.Members.GetAllAsync( mailChimpListId, memberRequest ).Result;

                        if ( result.Count() > 0 )
                        {
                            mailChimpMembers.AddRange( result );
                            offset += 1000;
                        }
                        else
                        {
                            moreRecordsToFetch = false;
                        }

                    }
                }
                catch ( Exception ex )
                {
                    string message = String.Format( "Error occurred pulling records from Mailchimp Audience '{0}'", mailChimpList.Value );
                    ExceptionLogService.LogException( new Exception( message, ex ) );
                }

                var mailChimpMembersNotAdded = new List<Member>();

                //Get all Groups that have an attribute set to this Mail Chimp List's Defined Value.
                var groupIds = new AttributeValueService( new RockContext() ).Queryable().AsNoTracking()
                    .Where( x => x.Value.Equals( mailChimpList.Guid.ToString(), StringComparison.OrdinalIgnoreCase ) &&
                                 x.Attribute.EntityType.FriendlyName == Rock.Model.Group.FriendlyTypeName )
                    .Select( x => x.EntityId )
                    .Where( x => x.HasValue )
                    .Distinct()
                    .ToList();

                //Match all the mailChimpMembers to people in Rock.
                foreach ( var member in mailChimpMembers )
                {
                    try
                    {
                        RockContext rockContext = new RockContext();
                        GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                        GroupService groupService = new GroupService( rockContext );
                        rockContext.Database.CommandTimeout = 600;

                        var rockPerson = GetRockPerson( member );
                        if ( rockPerson.IsNotNull() )
                        {
                            mailChimpMemberLookUp.AddOrIgnore( rockPerson.Id, member );

                            if ( groupIds.Any() )
                            {
                                foreach ( var groupId in groupIds )
                                {
                                    var group = groupService.Get( groupId.Value );
                                    if ( group != null )
                                    {
                                        try
                                        {
                                            var groupMember = group.Members.Where( m => m.PersonId == rockPerson.Id ).FirstOrDefault();
                                            if ( groupMember.IsNull() )
                                            {
                                                groupMember = new GroupMember { PersonId = rockPerson.Id, GroupId = group.Id };
                                                groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId ?? group.GroupType.Roles.First().Id;
                                                groupMemberService.Add( groupMember );
                                            }
                                            groupMember.GroupMemberStatus = GetRockGroupMemberStatus( member.Status );
                                            var mailChimpMember = member;
                                            SyncPerson( ref rockPerson, ref mailChimpMember, mailChimpListId );
                                            rockContext.SaveChanges();
                                        }
                                        catch ( Exception ex )
                                        {
                                            string message = String.Format( "Error Adding Person #{0} to Group '{1}'", rockPerson.Id, group.Name );
                                            ExceptionLogService.LogException( new Exception( message, ex ) );
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            mailChimpMembersNotAdded.Add( member );
                        }
                    }
                    catch ( Exception ex )
                    {
                        string message = String.Format( "Error Grabbing record #{0} with email {1} for Mailchimp Audience '{2}'", member.Id, member.EmailAddress, mailChimpList.Value );
                        ExceptionLogService.LogException( new Exception( message, ex ) );
                    }
                }

                if ( mailChimpMembersNotAdded.Any() )
                {
                    ExceptionLogService.LogException( new Exception( mailChimpMembersNotAdded.Count().ToString() + " Mailchimp Members not added." ) );
                }


                if ( groupIds.Any() )
                {
                    foreach ( var groupId in groupIds.Where( g => g.HasValue ).Distinct().ToList() )
                    {
                        try
                        {
                            RockContext rockContext = new RockContext();
                            GroupService groupService = new GroupService( rockContext );
                            rockContext.Database.CommandTimeout = 600;

                            // Filter out Inactive and Archived Group Members for adding new members to Mail Chimp.
                            var memberList = groupService
                                .Queryable()
                                .Where( g => g.Id == groupId.Value )
                                .SelectMany( g => g.Members )
                                .Where( gm => gm.GroupMemberStatus != GroupMemberStatus.Inactive &&
                                             !gm.IsArchived &&
                                             ( !dateLimit.HasValue || gm.ModifiedDateTime >= dateLimit ) )
                                .ToList();

                            if ( memberList.Any() )
                            {
                                foreach ( var groupMember in memberList )
                                {
                                    try
                                    {
                                        if ( !mailChimpMemberLookUp.ContainsKey( groupMember.PersonId ) )
                                        {
                                            AddPersonToMailChimp( groupMember.Person, mailChimpListId );
                                            rockContext.SaveChanges();
                                        }
                                    }
                                    catch ( Exception ex )
                                    {
                                        string message = String.Format( "Error Adding Person #{0} to Mailchimp Audience '{1}'", groupMember.Person.Id, mailChimpList.Value );
                                        ExceptionLogService.LogException( new Exception( message, ex ) );
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            string message = String.Format( "Error occurred adding members of Group #{0} to Mailchimp Audience '{1}'", groupId, mailChimpList.Value );
                            ExceptionLogService.LogException( new Exception( message, ex ) );
                        }

                    }
                }
            }
            catch ( Exception ex )
            {
                string message = String.Format( "Error occurred importing Mailchimp Audience '{0}'", mailChimpList.Value );
                ExceptionLogService.LogException( new Exception( message, ex ) );
            }

        }

        private bool AddPersonToMailChimp( Person person, string mailChimpListId )
        {
            bool foundMember = false;
            bool addedPerson = false;

            try
            {
                foundMember = _mailChimpManager.Members.ExistsAsync( mailChimpListId, person.Email, null, false ).Result;

                if ( !foundMember )
                {
                    RockContext rockContext = new RockContext();
                    var emailTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL ).Id;
                    var EmailAddresses = person.GetPersonSearchKeys( rockContext ).AsNoTracking().Where( k => k.SearchTypeValueId == emailTypeId ).Select( x => x.SearchValue );
                    foreach ( var email in EmailAddresses )
                    {
                        try
                        {
                            foundMember = _mailChimpManager.Members.ExistsAsync( mailChimpListId, email, null, false ).Result;
                            if ( foundMember )
                            {
                                // if the Person is found using an alternate email address, their email address needs to be updated in mail chimp.
                                var member = new MCModels.Member
                                {
                                    EmailAddress = person.Email,
                                    StatusIfNew = MCModels.Status.Subscribed,
                                    Id = _mailChimpManager.Members.Hash( email )
                                };
                                break;
                            }
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            if ( !foundMember )
            {
                MCModels.Member member = null;
                SyncPerson( ref person, ref member, mailChimpListId );
                addedPerson = true;
            }

            return addedPerson;
        }

        private void UpdateMailChimpListDefinedValue( MCModels.List mailChimpList, ref DefinedValue mailChimpListValue, RockContext rockContext )
        {
            mailChimpListValue.Value = mailChimpList.Name;
            mailChimpListValue.Description = mailChimpList.SubscribeUrlLong;

            var mailChimpAccountAttribute = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_AUDIENCE_ACCOUNT_ATTRIBUTE );
            var mailChimpIdAttribute = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_AUDIENCE_ID_ATTRIBUTE );

            Rock.Attribute.Helper.SaveAttributeValue( mailChimpListValue, mailChimpAccountAttribute, _mailChimpAccount.Guid.ToString(), rockContext );
            Rock.Attribute.Helper.SaveAttributeValue( mailChimpListValue, mailChimpIdAttribute, mailChimpList.Id, rockContext );
        }

        private Person GetRockPerson( MCModels.Member member )
        {
            Person person = null;
            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );

            var firstName = member.MergeFields["FNAME"].ToString().Left( 50 );
            var lastName = member.MergeFields["LNAME"].ToString().Left( 50 );
            var email = member.EmailAddress;
            var mailchimpForeignKey = String.Format( "Mailchimp_{0}", member.Id );

            string emailNote = null;
            bool isEmailActive = GetIsEmailActive( member.Status, out emailNote );

            // Check if there's a person in the DB who has already been created with a foreign key.  This will only match people added via the mail chimp plugin.
            person = personService.Queryable().AsNoTracking().Where( p => p.ForeignKey == mailchimpForeignKey ).FirstOrDefault();

            if ( person.IsNull() )
            {
                var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, null, null, null, null, null );
                // Use find persons vs find person because if there are multile matches, we'll just use the first match vs creating a new person.
                person = personService.FindPersons( personQuery, false ).OrderBy( p => p.Id ).FirstOrDefault();
            }

            if ( person.IsNull() )
            {
                person = personService.Queryable().AsNoTracking().Where( p => p.Email == email ).OrderBy( p => p.Id ).FirstOrDefault();
            }


            if ( person.IsNull() )
            {
                // Add New Person
                person = new Person();
                person.FirstName = firstName.FixCase();
                person.LastName = lastName.FixCase();
                person.IsEmailActive = isEmailActive;
                person.Email = email;
                person.EmailPreference = GetRockEmailPrefernce( member.Status );
                person.EmailNote = emailNote;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                person.ForeignKey = mailchimpForeignKey;

                if ( !person.Email.IsValidEmail() )
                {
                    ExceptionLogService.LogException( new Exception( "Could not Add Mailchimp Member because their email address isn't valid(" + person.Email + ")" ) );
                    return null;
                }

                var familyGroup = PersonService.SaveNewPerson( person, rockContext, null, false );
                if ( familyGroup != null && familyGroup.Members.Any() )
                {
                    person = familyGroup.Members.Select( m => m.Person ).First();
                }

            }

            rockContext.SaveChanges();


            return person;
        }

        private void SyncPerson( ref Person rockPerson, ref MCModels.Member mailChimpMember, string mailChimpListId )
        {
            if ( mailChimpMember.IsNull() )
            {
                if ( rockPerson.Email.IsNotNullOrWhiteSpace() )
                {
                    mailChimpMember = new MCModels.Member { EmailAddress = rockPerson.Email, StatusIfNew = MCModels.Status.Subscribed };
                }
                else
                {
                    Exception ex = new Exception( rockPerson.FullName + " was not synced to Mail Chimp because they do not have an email address" );
                    ExceptionLogService.LogException( ex );
                }
            }
            else
            {
                // If the Email Addresses Match, Check the Mail Chimp's Email Status to Update the Rock record.
                // There's a chance they won't match, because Rock matches on Search Keys which could have contained the old email address.
                // If They Don't Match, Update Mail Chimp to the Rock Person's Email Address
                if ( mailChimpMember.EmailAddress.Equals( rockPerson.Email, StringComparison.OrdinalIgnoreCase ) )
                {
                    string emailNote;
                    rockPerson.EmailPreference = GetRockEmailPrefernce( mailChimpMember.Status );
                    rockPerson.IsEmailActive = GetIsEmailActive( mailChimpMember.Status, out emailNote );
                    rockPerson.EmailNote = emailNote;
                }
                else
                {
                    mailChimpMember.EmailAddress = rockPerson.Email;
                }
            }

            try
            {
                if ( mailChimpMember.IsNotNull() )
                {                   
                    var firstName = mailChimpMember.MergeFields.ContainsKey("FNAME") ? mailChimpMember.MergeFields["FNAME"].ToString().Left( 50 ) : "";
                    var lastName = mailChimpMember.MergeFields.ContainsKey("LNAME") ? mailChimpMember.MergeFields["LNAME"].ToString().Left( 50 ): "";

                    if ( firstName != rockPerson.NickName || lastName != rockPerson.LastName )
                    {
                        // Update Mail Chimp with the Rock Person's First And Last Name
                        mailChimpMember.MergeFields.AddOrReplace( "FNAME", rockPerson.NickName );
                        mailChimpMember.MergeFields.AddOrReplace( "LNAME", rockPerson.LastName );

                        var result = _mailChimpManager.Members.AddOrUpdateAsync( mailChimpListId, mailChimpMember ).Result;
                    }
                }
            }
            catch ( System.AggregateException e )
            {
                foreach ( var ex in e.InnerExceptions )
                {
                    if ( ex is MCNet.Core.MailChimpException )
                    {
                        var mailChimpException = ex as MCNet.Core.MailChimpException;
                        ExceptionLogService.LogException( new Exception( mailChimpException.Message + mailChimpException.Detail ) );
                    }
                    else
                    {
                        ExceptionLogService.LogException( ex );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        private EmailPreference GetRockEmailPrefernce( MCModels.Status mailChimpEmailStatus )
        {
            EmailPreference preference = EmailPreference.EmailAllowed;

            switch ( mailChimpEmailStatus )
            {
                case MCModels.Status.Unsubscribed:
                    preference = EmailPreference.NoMassEmails;
                    break;
                case MCModels.Status.Cleaned:
                    preference = EmailPreference.DoNotEmail;
                    break;
            }

            return preference;
        }

        private bool GetIsEmailActive( MCModels.Status mailChimpEmailStatus, out string emailNote )
        {
            bool isActive = true;
            emailNote = null;

            switch ( mailChimpEmailStatus )
            {
                case MCModels.Status.Cleaned:
                    isActive = false;
                    emailNote = "Email was marked as cleaned in Mail Chimp";
                    break;
            }

            return isActive;
        }

        private GroupMemberStatus GetRockGroupMemberStatus( MCModels.Status mailChimpEmailStatus )
        {
            GroupMemberStatus memberStatus = GroupMemberStatus.Inactive;

            switch ( mailChimpEmailStatus )
            {
                case MCModels.Status.Subscribed:
                    memberStatus = GroupMemberStatus.Active;
                    break;
                case MCModels.Status.Pending:
                    memberStatus = GroupMemberStatus.Pending;
                    break;
            }

            return memberStatus;
        }
    }
}
