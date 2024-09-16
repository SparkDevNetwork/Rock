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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Registration
    {
        #region Properties
        
        /// <summary>
        /// Gets a boolean value indicating whether this registration has an active payment plan.
        /// </summary>
        /// <remarks>
        /// <strong>This is an internal API</strong> that supports the Rock
        /// infrastructure and not subject to the same compatibility standards
        /// as public APIs. It may be changed or removed without notice in any
        /// release and should therefore not be directly used in any plug-ins.
        /// </remarks>
        /// <value>
        /// A boolean value indicating whether a payment plan is configured.
        /// </value>
        [NotMapped]
        [LavaVisible]
        [RockInternal( "1.16.6" )]
        public virtual bool IsPaymentPlanActive
        {
            get
            {
                return this.PaymentPlanFinancialScheduledTransaction?.IsActive == true;
            }
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        [NotMapped]
        public virtual int? PersonId
        {
            get { return PersonAlias != null ? PersonAlias.PersonId : ( int? ) null; }
        }

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual decimal TotalCost
        {
            get
            {
                if ( Registrants != null )
                {
                    return Registrants.Sum( r => r.TotalCost );
                }

                return 0.0M;
            }
        }

        /// <summary>
        /// Gets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual decimal DiscountedCost
        {
            get
            {
                var discountedCost = 0.0m;
                if ( Registrants != null )
                {
                    foreach ( var registrant in Registrants )
                    {
                        discountedCost += registrant.DiscountedCost( DiscountPercentage, DiscountAmount );
                    }
                }

                return discountedCost;
            }
        }

        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <value>
        /// The total paid.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual decimal TotalPaid
        {
            get
            {
                return this.GetTotalPaid();
            }
        }

        /// <summary>
        /// Gets the balance due.
        /// </summary>
        /// <value>
        /// The balance due.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual decimal BalanceDue
        {
            get
            {
                return DiscountedCost - TotalPaid;
            }
        }

        /// <summary>
        /// Gets the registration template identifier.
        /// NOTE: this is needed so that Registration Attributes can have a RegistrationTemplateId qualifier
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual int? RegistrationTemplateId
        {
            get
            {
                if ( this.RegistrationInstance == null )
                {
                    return new RegistrationInstanceService( new RockContext() ).GetSelect( this.RegistrationInstanceId, a => a.RegistrationTemplateId );
                }

                return this.RegistrationInstance.RegistrationTemplateId;
            }
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.FinancialTransactionDetail">payments</see>.
        /// </summary>
        /// <value>
        /// The payments.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual IQueryable<FinancialTransactionDetail> Payments
        {
            get
            {
                return this.GetPayments();
            }
        }

        #endregion Navigation Properties

        #region Methods
        /// <summary>
        /// Gets a summary of the registration
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <returns></returns>
        public string GetSummary( RegistrationInstance registrationInstance = null )
        {
            var result = new StringBuilder();
            result.Append( "Event registration payment" );

            var instance = registrationInstance ?? RegistrationInstance;
            if ( instance != null )
            {
                result.AppendFormat( " for {0} [ID:{1}]", instance.Name, instance.Id );
                if ( instance.RegistrationTemplate != null )
                {
                    result.AppendFormat( " (Template: {0} [ID:{1}])", instance.RegistrationTemplate.Name, instance.RegistrationTemplate.Id );
                }
            }

            string registrationPerson = PersonAlias != null && PersonAlias.Person != null ?
                PersonAlias.Person.FullName :
                string.Format( "{0} {1}", FirstName, LastName );

            result.AppendFormat(
                @".
Registration By: {0} Total Cost/Fees:{1}
",
                registrationPerson,
                DiscountedCost.FormatAsCurrency() );

            var registrantPersons = new List<string>();
            if ( Registrants != null )
            {
                foreach ( var registrant in Registrants.Where( r => r.PersonAlias != null && r.PersonAlias.Person != null ) )
                {
                    registrantPersons.Add( $"{registrant.PersonAlias.Person.FullName} Cost/Fees:{registrant.DiscountedCost( DiscountPercentage, DiscountAmount ).FormatAsCurrency()}" );
                }
            }

            result.AppendFormat( "Registrants: {0}", registrantPersons.AsDelimited( ", " ) );

            return result.ToString();
        }

        /// <summary>
        /// Gets the confirmation recipient as either an email to the person that registered, or as an anonymous email to the specified confirmation email if it is different than the person's email.
        /// </summary>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <returns></returns>
        public RockMessageRecipient GetConfirmationRecipient( Dictionary<string, object> mergeObjects )
        {
            var person = this.PersonAlias?.Person;
            string personEmail = person?.Email;

            var confirmationEmail = this.ConfirmationEmail;
            if ( personEmail == confirmationEmail )
            {
                return new RockEmailMessageRecipient( person, mergeObjects );
            }
            else
            {
                return RockEmailMessageRecipient.CreateAnonymous( confirmationEmail, mergeObjects );
            }
        }

        /// <summary>
        /// Saves the person notes and history.
        /// </summary>
        /// <param name="registrationPerson">The person that created the registration</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="previousRegistrantPersonIds">The person ids that have already registered prior to this registration</param>
        public void SavePersonNotesAndHistory( Person registrationPerson, int? currentPersonAliasId, List<int> previousRegistrantPersonIds )
        {
            SavePersonNotesAndHistory( registrationPerson.FirstName, registrationPerson.LastName, currentPersonAliasId, previousRegistrantPersonIds );
        }

        /// <summary>
        /// Saves the person notes and history.
        /// </summary>
        /// <param name="registrationPersonFirstName">First name of the registration person.</param>
        /// <param name="registrationPersonLastName">Last name of the registration person.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="previousRegistrantPersonIds">The previous registrant person ids.</param>
        public void SavePersonNotesAndHistory( string registrationPersonFirstName, string registrationPersonLastName, int? currentPersonAliasId, List<int> previousRegistrantPersonIds )
        {
            // Setup Note settings
            Registration registration = this;
            NoteTypeCache noteType = null;
            using ( RockContext rockContext = new RockContext() )
            {
                RegistrationInstance registrationInstance = registration.RegistrationInstance ?? new RegistrationInstanceService( rockContext ).Get( registration.RegistrationInstanceId );
                RegistrationTemplate registrationTemplate = registrationInstance.RegistrationTemplate ?? new RegistrationTemplateService( rockContext ).Get( registrationInstance.RegistrationTemplateId );

                if ( registrationTemplate != null && registrationTemplate.AddPersonNote )
                {
                    noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.PERSON_EVENT_REGISTRATION.AsGuid() );
                    if ( noteType != null )
                    {
                        var noteService = new NoteService( rockContext );
                        var personAliasService = new PersonAliasService( rockContext );

                        Person registrar = null;
                        if ( registration.PersonAliasId.HasValue )
                        {
                            registrar = personAliasService.GetPerson( registration.PersonAliasId.Value );
                        }

                        var registrantNames = new List<string>();

                        // Get each registrant
                        foreach ( var registrantPersonAliasId in registration.Registrants
                            .Where( r => r.PersonAliasId.HasValue )
                            .Select( r => r.PersonAliasId.Value )
                            .ToList() )
                        {
                            var registrantPerson = personAliasService.GetPerson( registrantPersonAliasId );
                            if ( registrantPerson != null && ( previousRegistrantPersonIds == null || !previousRegistrantPersonIds.Contains( registrantPerson.Id ) ) )
                            {
                                var noteText = new StringBuilder();
                                noteText.AppendFormat( "Registered for {0}", registrationInstance.Name );

                                string registrarFullName = string.Empty;

                                if ( registrar != null && registrar.Id != registrantPerson.Id )
                                {
                                    registrarFullName = string.Format( " by {0}", registrar.FullName );
                                    registrantNames.Add( registrantPerson.FullName );
                                }

                                if ( registrar != null && ( registrationPersonFirstName != registrar.NickName || registrationPersonLastName != registrar.LastName ) )
                                {
                                    registrarFullName = string.Format( " by {0}", registrationPersonFirstName + " " + registrationPersonLastName );
                                }

                                noteText.Append( registrarFullName );

                                if ( noteText.Length > 0 )
                                {
                                    var note = new Note();
                                    note.NoteTypeId = noteType.Id;
                                    note.IsSystem = false;
                                    note.IsAlert = false;
                                    note.IsPrivateNote = false;
                                    note.EntityId = registrantPerson.Id;
                                    note.Caption = registrationInstance.Name;
                                    note.Text = noteText.ToString();
                                    if ( registrar == null )
                                    {
                                        note.CreatedByPersonAliasId = currentPersonAliasId;
                                    }
                                    else
                                    {
                                        note.CreatedByPersonAliasId = registrar.PrimaryAliasId;
                                    }

                                    noteService.Add( note );
                                }

                                var changes = new History.HistoryChangeList();
                                changes.AddChange( History.HistoryVerb.Registered, History.HistoryChangeType.Record, null );
                                HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( Person ),
                                    Rock.SystemGuid.Category.HISTORY_PERSON_REGISTRATION.AsGuid(),
                                    registrantPerson.Id,
                                    changes,
                                    registrationInstance.Name,
                                    typeof( Registration ),
                                    registration.Id,
                                    false,
                                    currentPersonAliasId,
                                    rockContext.SourceOfChange );
                            }
                        }

                        if ( registrar != null && registrantNames.Any() )
                        {
                            string namesText = string.Empty;
                            if ( registrantNames.Count >= 2 )
                            {
                                int lessOne = registrantNames.Count - 1;
                                namesText = registrantNames.Take( lessOne ).ToList().AsDelimited( ", " ) +
                                    " and " +
                                    registrantNames.Skip( lessOne ).Take( 1 ).First() + " ";
                            }
                            else
                            {
                                namesText = registrantNames.First() + " ";
                            }

                            var note = new Note();
                            note.NoteTypeId = noteType.Id;
                            note.IsSystem = false;
                            note.IsAlert = false;
                            note.IsPrivateNote = false;
                            note.EntityId = registrar.Id;
                            note.Caption = registrationInstance.Name;
                            note.Text = string.Format( "Registered {0} for {1}", namesText, registrationInstance.Name );
                            noteService.Add( note );

                            var changes = new History.HistoryChangeList();
                            changes.AddChange( History.HistoryVerb.Registered, History.HistoryChangeType.Record, namesText );

                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_REGISTRATION.AsGuid(),
                                registrar.Id,
                                changes,
                                registrationInstance.Name,
                                typeof( Registration ),
                                registration.Id,
                                false,
                                currentPersonAliasId,
                                rockContext.SourceOfChange );
                        }

                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( PersonAlias != null && PersonAlias.Person != null )
            {
                return PersonAlias.Person.FullName;
            }

            string personName = string.Format( "{0} {1}", FirstName, LastName );
            return string.IsNullOrWhiteSpace( personName ) ? "Registration" : personName.Trim();
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override ISecured ParentAuthority
        {
            get
            {
                return RegistrationInstance != null ? RegistrationInstance : base.ParentAuthority;
            }
        }

        #endregion Methods
    }
    
    /// <summary>
    /// A Registration-PaymentPlan pair.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is an internal API</strong> that supports the Rock
    /// infrastructure and not subject to the same compatibility standards
    /// as public APIs. It may be changed or removed without notice in any
    /// release and should therefore not be directly used in any plug-ins.
    /// </para>
    /// </remarks>
    [RockInternal( "1.16.6" )]
    public class RegistrationPaymentPlanPair
    {
        /// <summary>
        /// Gets or sets the registration.
        /// </summary>
        public Registration Registration { get; set; }

        /// <summary>
        /// Gets or sets the payment plan.
        /// </summary>
        /// <value>
        /// Can be <see langword="null"/> if the registration does not have a payment plan.
        /// </value>
        public PaymentPlan PaymentPlan { get; set; }
    }
}
