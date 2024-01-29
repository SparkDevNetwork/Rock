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

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Person
    {
        /// <summary>
        /// Save hook implementation for <see cref="Person"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Person>
        {
            private History.HistoryChangeList HistoryChanges { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) this.RockContext;

                var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
                var deceased = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() );

                if ( inactiveStatus != null && deceased != null )
                {
                    bool isInactive = ( this.Entity.RecordStatusValueId.HasValue && this.Entity.RecordStatusValueId.Value == inactiveStatus.Id ) ||
                        ( this.Entity.RecordStatusValue != null && this.Entity.RecordStatusValue.Id == inactiveStatus.Id );
                    bool isReasonDeceased = ( this.Entity.RecordStatusReasonValueId.HasValue && this.Entity.RecordStatusReasonValueId.Value == deceased.Id ) ||
                        ( this.Entity.RecordStatusReasonValue != null && this.Entity.RecordStatusReasonValue.Id == deceased.Id );

                    this.Entity.IsDeceased = isInactive && isReasonDeceased;

                    if ( isInactive )
                    {
                        var recordStatusValueId = this.Entity.RecordStatusValueId;
                        int? oldRecordStatusValueId = null;
                        if ( Entry.State == EntityContextState.Modified )
                        {
                            oldRecordStatusValueId = this.Entry.OriginalValues[nameof( Person.RecordStatusValueId )].ToStringSafe().AsIntegerOrNull();
                        }

                        if ( oldRecordStatusValueId != Entity.RecordStatusValueId )
                        {
                            // If person was just inactivated, update the group member status for all their group memberships to be inactive
                            foreach ( var groupMember in new GroupMemberService( rockContext )
                                .Queryable()
                                .Where( m =>
                                    m.PersonId == this.Entity.Id &&
                                    m.GroupMemberStatus != GroupMemberStatus.Inactive &&
                                    !m.Group.GroupType.IgnorePersonInactivated ) )
                            {
                                groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                            }

                            // Also update the person's connection requests
                            int[] aliasIds = Entity.Aliases.Select( a => a.Id ).ToArray();
                            foreach ( var connectionRequest in new ConnectionRequestService( rockContext )
                                .Queryable()
                                .Where( c =>
                                     aliasIds.Contains( c.PersonAliasId ) &&
                                     c.ConnectionState != ConnectionState.Inactive &&
                                     c.ConnectionState != ConnectionState.Connected ) )
                            {
                                Rock.Logging.RockLogger.Log.Debug( Rock.Logging.RockLogDomains.Crm, $"Person.PreSave() setting connection requests Inactive for Person.Id {this.Entity.Id} and ConnectionRequest.Id = {connectionRequest.Id}" );
                                connectionRequest.ConnectionState = ConnectionState.Inactive;
                            }
                        }
                    }
                }

                this.Entity.RecordTypeValueId = this.Entity.RecordTypeValueId ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                this.Entity.RecordStatusValueId = this.Entity.RecordStatusValueId ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

                if ( !this.Entity.IsBusiness() && !this.Entity.ConnectionStatusValueId.HasValue )
                {
                    this.Entity.ConnectionStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() ).Id;
                }

                if ( string.IsNullOrWhiteSpace( this.Entity.NickName ) )
                {
                    this.Entity.NickName = this.Entity.FirstName;
                }

                // Make sure the GivingId is correct.
                if ( this.Entity.GivingId != ( this.Entity.GivingGroupId.HasValue ? $"G{this.Entity.GivingGroupId.Value}" : $"P{this.Entity.Id}" ) )
                {
                    this.Entity.GivingId = this.Entity.GivingGroupId.HasValue ? $"G{this.Entity.GivingGroupId.Value}" : $"P{this.Entity.Id}";
                }

                if ( this.Entity.PhotoId.HasValue )
                {
                    int? oldPhotoId = null;
                    if ( Entry.State == EntityContextState.Modified )
                    {
                        oldPhotoId = this.Entry.OriginalValues[nameof( Person.PhotoId )].ToStringSafe().AsIntegerOrNull();
                    }

                    var isPhotoIdModified = Entry.State == EntityContextState.Modified &&
                                            ( ( oldPhotoId.HasValue && oldPhotoId.Value != this.Entity.PhotoId.Value ) || !oldPhotoId.HasValue );
                    if ( Entry.State == EntityContextState.Added || isPhotoIdModified )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( this.Entity.PhotoId.Value );
                        if ( binaryFile != null && binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = false;
                        }
                    }
                }

                // ensure person has a PersonAlias/PrimaryAlias
                if ( this.Entry.State != EntityContextState.Deleted )
                {
                    if ( !this.Entity.Aliases.Any() || !this.Entity.Aliases.Any( a => a.AliasPersonId == this.Entity.Id ) )
                    {
                        this.Entity.Aliases.Add( new PersonAlias { AliasPerson = this.Entity, AliasPersonGuid = this.Entity.Guid, Guid = System.Guid.NewGuid() } );
                    }
                }

                if ( this.Entry.State == EntityContextState.Modified || this.Entry.State == EntityContextState.Added )
                {
                    this.Entity.FirstName = this.Entity.FirstName.StandardizeQuotes();
                    this.Entity.LastName = this.Entity.LastName.StandardizeQuotes();
                    this.Entity.NickName = this.Entity.NickName.StandardizeQuotes();

                    // Remove extra spaces between words (Issue #2990)
                    this.Entity.FirstName = this.Entity.FirstName != null ? Regex.Replace( this.Entity.FirstName, @"\s+", " " ).Trim() : null;
                    this.Entity.LastName = this.Entity.LastName != null ? Regex.Replace( this.Entity.LastName, @"\s+", " " ).Trim() : null;
                    this.Entity.NickName = this.Entity.NickName != null ? Regex.Replace( this.Entity.NickName, @"\s+", " " ).Trim() : null;
                    this.Entity.MiddleName = this.Entity.MiddleName != null ? Regex.Replace( this.Entity.MiddleName, @"\s+", " " ).Trim() : null;
                }

                if ( this.Entity.AnniversaryDate.HasValue )
                {
                    if ( this.Entity.MaritalStatusValueId != DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED ).Id )
                    {
                        this.Entity.AnniversaryDate = null;
                    }
                    else
                    {
                        DateTime? oldAnniversaryDate = null;
                        if ( Entry.State == EntityContextState.Modified )
                        {
                            oldAnniversaryDate = this.Entry.OriginalValues[nameof( Person.AnniversaryDate )].ToStringSafe().AsDateTime();
                        }

                        if ( oldAnniversaryDate != this.Entity.AnniversaryDate )
                        {
                            var spouse = this.Entity.GetSpouse( rockContext );
                            if ( spouse != null && spouse.AnniversaryDate != this.Entity.AnniversaryDate )
                            {
                                spouse.AnniversaryDate = this.Entity.AnniversaryDate;
                            }
                        }
                    }
                }

                // Calculates the BirthDate and sets it
                this.Entity.BirthDate = this.Entity.CalculateBirthDate();
                this.Entity.BirthDateKey = this.Entity.BirthDate?.ToString( "yyyyMMdd" ).AsIntegerOrNull();
                this.Entity.Age = GetAge( this.Entity.BirthDate, this.Entity.DeceasedDate );
                this.Entity.AgeBracket = GetAgeBracket( this.Entity.Age );

                this.Entity.CalculateSignals();

                if ( this.Entity.IsValid )
                {
                    new SaveMetaphoneTransaction( this.Entity ).Enqueue();
                }

                HistoryChanges = new History.HistoryChangeList();

                switch ( this.Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Person" ).SetNewValue( this.Entity.FullName );

                            History.EvaluateChange( HistoryChanges, "Record Type", ( int? ) null, this.Entity.RecordTypeValue, this.Entity.RecordTypeValueId );
                            History.EvaluateChange( HistoryChanges, "Record Status", ( int? ) null, this.Entity.RecordStatusValue, this.Entity.RecordStatusValueId );
                            History.EvaluateChange( HistoryChanges, "Record Status Reason", ( int? ) null, this.Entity.RecordStatusReasonValue, this.Entity.RecordStatusReasonValueId );
                            History.EvaluateChange( HistoryChanges, "Inactive Reason Note", string.Empty, this.Entity.InactiveReasonNote );
                            History.EvaluateChange( HistoryChanges, "Connection Status", ( int? ) null, this.Entity.ConnectionStatusValue, this.Entity.ConnectionStatusValueId );
                            History.EvaluateChange( HistoryChanges, "Review Reason", ( int? ) null, this.Entity.ReviewReasonValue, this.Entity.ReviewReasonValueId );
                            History.EvaluateChange( HistoryChanges, "Review Reason Note", string.Empty, this.Entity.ReviewReasonNote );
                            History.EvaluateChange( HistoryChanges, "Deceased", ( bool? ) null, this.Entity.IsDeceased );
                            History.EvaluateChange( HistoryChanges, "Title", ( int? ) null, this.Entity.TitleValue, this.Entity.TitleValueId );
                            History.EvaluateChange( HistoryChanges, "First Name", string.Empty, this.Entity.FirstName );
                            History.EvaluateChange( HistoryChanges, "Nick Name", string.Empty, this.Entity.NickName );
                            History.EvaluateChange( HistoryChanges, "Middle Name", string.Empty, this.Entity.MiddleName );
                            History.EvaluateChange( HistoryChanges, "Last Name", string.Empty, this.Entity.LastName );
                            History.EvaluateChange( HistoryChanges, "Suffix", ( int? ) null, this.Entity.SuffixValue, this.Entity.SuffixValueId );
                            History.EvaluateChange( HistoryChanges, "Birth Date", null, this.Entity.BirthDate );
                            History.EvaluateChange( HistoryChanges, "Gender", null, this.Entity.Gender );
                            History.EvaluateChange( HistoryChanges, "Marital Status", ( int? ) null, this.Entity.MaritalStatusValue, this.Entity.MaritalStatusValueId );
                            History.EvaluateChange( HistoryChanges, "Anniversary Date", null, this.Entity.AnniversaryDate );
                            History.EvaluateChange( HistoryChanges, "Graduation Year", null, this.Entity.GraduationYear );
                            History.EvaluateChange( HistoryChanges, "Giving Id", null, this.Entity.GivingId );
                            History.EvaluateChange( HistoryChanges, "Email", string.Empty, this.Entity.Email );
                            History.EvaluateChange( HistoryChanges, "Email Active", ( bool? ) null, this.Entity.IsEmailActive );
                            History.EvaluateChange( HistoryChanges, "Email Note", string.Empty, this.Entity.EmailNote );
                            History.EvaluateChange( HistoryChanges, "Email Preference", null, this.Entity.EmailPreference );
                            History.EvaluateChange( HistoryChanges, "Communication Preference", null, this.Entity.CommunicationPreference );
                            History.EvaluateChange( HistoryChanges, "System Note", string.Empty, this.Entity.SystemNote );

                            if ( this.Entity.PhotoId.HasValue )
                            {
                                HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, "Photo" );
                            }

                            // ensure a new person has an Alternate Id
                            int alternateValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
                            var personSearchKeyService = new PersonSearchKeyService( rockContext );
                            PersonSearchKey personSearchKey = new PersonSearchKey()
                            {
                                PersonAlias = this.Entity.Aliases.First(),
                                SearchTypeValueId = alternateValueId,
                                SearchValue = PersonSearchKeyService.GenerateRandomAlternateId( true, rockContext )
                            };
                            personSearchKeyService.Add( personSearchKey );

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            History.EvaluateChange( HistoryChanges, "Record Type", Entry.OriginalValues[nameof( Person.RecordTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.RecordTypeValue, Entity.RecordTypeValueId );
                            History.EvaluateChange( HistoryChanges, "Record Status", Entry.OriginalValues[nameof( Person.RecordStatusValueId )].ToStringSafe().AsIntegerOrNull(), Entity.RecordStatusValue, Entity.RecordStatusValueId );
                            History.EvaluateChange( HistoryChanges, "Record Status Reason", Entry.OriginalValues[nameof( Person.RecordStatusReasonValueId )].ToStringSafe().AsIntegerOrNull(), Entity.RecordStatusReasonValue, Entity.RecordStatusReasonValueId );
                            History.EvaluateChange( HistoryChanges, "Inactive Reason Note", Entry.OriginalValues[nameof( Person.InactiveReasonNote )].ToStringSafe(), Entity.InactiveReasonNote );
                            History.EvaluateChange( HistoryChanges, "Connection Status", Entry.OriginalValues[nameof( Person.ConnectionStatusValueId )].ToStringSafe().AsIntegerOrNull(), Entity.ConnectionStatusValue, Entity.ConnectionStatusValueId );
                            History.EvaluateChange( HistoryChanges, "Review Reason", Entry.OriginalValues[nameof( Person.ReviewReasonValueId )].ToStringSafe().AsIntegerOrNull(), Entity.ReviewReasonValue, Entity.ReviewReasonValueId );
                            History.EvaluateChange( HistoryChanges, "Review Reason Note", Entry.OriginalValues[nameof( Person.ReviewReasonNote )].ToStringSafe(), Entity.ReviewReasonNote );
                            History.EvaluateChange( HistoryChanges, "Deceased", Entry.OriginalValues[nameof( Person.IsDeceased )].ToStringSafe().AsBoolean(), Entity.IsDeceased );
                            History.EvaluateChange( HistoryChanges, "Title", Entry.OriginalValues[nameof( Person.TitleValueId )].ToStringSafe().AsIntegerOrNull(), Entity.TitleValue, Entity.TitleValueId );
                            History.EvaluateChange( HistoryChanges, "First Name", Entry.OriginalValues[nameof( Person.FirstName )].ToStringSafe(), Entity.FirstName );
                            History.EvaluateChange( HistoryChanges, "Nick Name", Entry.OriginalValues[nameof( Person.NickName )].ToStringSafe(), Entity.NickName );
                            History.EvaluateChange( HistoryChanges, "Middle Name", Entry.OriginalValues[nameof( Person.MiddleName )].ToStringSafe(), Entity.MiddleName );
                            History.EvaluateChange( HistoryChanges, "Last Name", Entry.OriginalValues[nameof( Person.LastName )].ToStringSafe(), Entity.LastName );
                            History.EvaluateChange( HistoryChanges, "Suffix", Entry.OriginalValues[nameof( Person.SuffixValueId )].ToStringSafe().AsIntegerOrNull(), Entity.SuffixValue, Entity.SuffixValueId );
                            History.EvaluateChange( HistoryChanges, "Birth Date", Entry.OriginalValues[nameof( Person.BirthDate )].ToStringSafe().AsDateTime(), Entity.BirthDate );
                            History.EvaluateChange( HistoryChanges, "Gender", Entry.OriginalValues[nameof( Person.Gender )].ToStringSafe().ConvertToEnum<Gender>(), Entity.Gender );
                            History.EvaluateChange( HistoryChanges, "Marital Status", Entry.OriginalValues[nameof( Person.MaritalStatusValueId )].ToStringSafe().AsIntegerOrNull(), Entity.MaritalStatusValue, Entity.MaritalStatusValueId );
                            History.EvaluateChange( HistoryChanges, "Anniversary Date", Entry.OriginalValues[nameof( Person.AnniversaryDate )].ToStringSafe().AsDateTime(), Entity.AnniversaryDate );
                            History.EvaluateChange( HistoryChanges, "Graduation Year", Entry.OriginalValues[nameof( Person.GraduationYear )].ToStringSafe().AsIntegerOrNull(), Entity.GraduationYear );
                            History.EvaluateChange( HistoryChanges, "Giving Id", Entry.OriginalValues[nameof( Person.GivingId )].ToStringSafe(), Entity.GivingId );
                            History.EvaluateChange( HistoryChanges, "Email", Entry.OriginalValues[nameof( Person.Email )].ToStringSafe(), Entity.Email );
                            History.EvaluateChange( HistoryChanges, "Email Active", Entry.OriginalValues[nameof( Person.IsEmailActive )].ToStringSafe().AsBoolean(), Entity.IsEmailActive );
                            History.EvaluateChange( HistoryChanges, "Email Note", Entry.OriginalValues[nameof( Person.EmailNote )].ToStringSafe(), Entity.EmailNote );
                            History.EvaluateChange( HistoryChanges, "Email Preference", Entry.OriginalValues[nameof( Person.EmailPreference )].ToStringSafe().ConvertToEnum<EmailPreference>(), Entity.EmailPreference );
                            History.EvaluateChange( HistoryChanges, "Communication Preference", Entry.OriginalValues[nameof( Person.CommunicationPreference )].ToStringSafe().ConvertToEnum<CommunicationType>(), Entity.CommunicationPreference );
                            History.EvaluateChange( HistoryChanges, "System Note", Entry.OriginalValues[nameof( Person.SystemNote )].ToStringSafe(), Entity.SystemNote );

                            int? originalPhotoId = Entry.OriginalValues[nameof( Person.PhotoId )].ToStringSafe().AsIntegerOrNull();
                            if ( originalPhotoId.HasValue )
                            {
                                if ( Entity.PhotoId.HasValue )
                                {
                                    if ( Entity.PhotoId.Value != originalPhotoId.Value )
                                    {
                                        HistoryChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Photo" );
                                    }
                                }
                                else
                                {
                                    HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, "Photo" );
                                }
                            }
                            else if ( Entity.PhotoId.HasValue )
                            {
                                HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, "Photo" );
                            }

                            if ( Entry.OriginalValues[nameof( Person.Email )].ToStringSafe() != Entity.Email )
                            {
                                var currentEmail = Entry.OriginalValues[nameof( Person.Email )].ToStringSafe();
                                if ( !string.IsNullOrEmpty( currentEmail ) )
                                {
                                    var personSearchKeyService = new PersonSearchKeyService( rockContext );
                                    var searchTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL.AsGuid() );
                                    if ( !personSearchKeyService.Queryable().Any( a => a.PersonAlias.PersonId == Entity.Id && a.SearchTypeValueId == searchTypeValue.Id && a.SearchValue.Equals( currentEmail, StringComparison.OrdinalIgnoreCase ) ) )
                                    {
                                        PersonSearchKey personSearchKey = new PersonSearchKey()
                                        {
                                            PersonAliasId = Entity.PrimaryAliasId.Value,
                                            SearchTypeValueId = searchTypeValue.Id,
                                            SearchValue = currentEmail
                                        };

                                        personSearchKeyService.Add( personSearchKey );
                                    }
                                }
                            }

                            if ( Entry.OriginalValues[nameof( Person.RecordStatusValueId )].ToStringSafe().AsIntegerOrNull() != Entity.RecordStatusValueId )
                            {
                                Entity.RecordStatusLastModifiedDateTime = RockDateTime.Now;

                                var activeStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                                if ( this.Entity.RecordStatusValueId == activeStatus.Id )
                                {
                                    this.Entity.ReviewReasonValueId = null;
                                }
                            }

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, null );

                            // If PersonRecord is getting deleted, don't do any of the remaining presavechanges
                            return;
                        }
                }
                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                if ( HistoryChanges?.Any() == true )
                {
                    HistoryService.SaveChanges(
                        ( RockContext ) this.RockContext,
                        typeof( Person ),
                        SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                        this.Entity.Id,
                        HistoryChanges,
                        this.Entity.FullName,
                        null,
                        null,
                        true,
                        this.Entity.ModifiedByPersonAliasId );
                }

                base.PostSave();

                // If the person was just added then update the GivingId to prevent "P0" values
                if ( this.Entity.GivingId == "P0" )
                {
                    PersonService.UpdateGivingId( this.Entity.Id, RockContext );
                }

                // If the person was just added then the _primaryAliasId will be null ergo the value will be null
                // in the database so update.
                if ( !this.Entity._primaryAliasId.HasValue )
                {
                    PersonService.UpdatePrimaryAlias( this.Entity.Id, this.Entity.PrimaryAliasId.Value, RockContext );
                }

                if ( this.Entity.Age.HasValue && ( this.PreSaveState == EntityContextState.Added || this.Entity.Age != Entry.OriginalValues[nameof( Person.Age )].ToStringSafe().AsIntegerOrNull() ) )
                {
                    PersonService.UpdateFamilyMemberRoleByAge( this.Entity.Id, this.Entity.Age.Value, RockContext );
                }

                // NOTE: This is also done on GroupMember.PostSaveChanges in case Role or family membership changes
                PersonService.UpdatePersonAgeClassification( this.Entity.Id, RockContext );
                PersonService.UpdatePrimaryFamily( this.Entity.Id, RockContext );
                PersonService.UpdateGivingLeaderId( this.Entity.Id, RockContext );
                PersonService.UpdateGroupSalutations( this.Entity.Id, RockContext );
            }
        }
    }
}
