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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.BulkExport
{
    /// <summary>
    /// Export record from ~/api/People/Export
    /// </summary>
    [RockClientInclude( "Export of Person record from ~/api/People/Export" )]
    public class PersonExport : ModelExport
    {
        /// <summary>
        /// The person
        /// </summary>
        private Person _person;
        private Uri _publicApplicationRootUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonExport"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="personIdHomeLocationsLookup">The person identifier home locations lookup.</param>
        /// <param name="publicApplicationRootUrl">The public application root URL.</param>
        public PersonExport( Person person, Dictionary<int, Location> personIdHomeLocationsLookup, string publicApplicationRootUrl )
            : base( person )
        {
            _person = person;
            if ( publicApplicationRootUrl.IsNotNullOrWhiteSpace() )
            {
                _publicApplicationRootUrl = new Uri( publicApplicationRootUrl );
            }

            this.HomeAddress = new LocationExport( personIdHomeLocationsLookup.GetValueOrNull( person.Id ) );
        }

        /// <summary>
        /// Gets the person alias ids.
        /// </summary>
        /// <value>
        /// The person alias ids.
        /// </value>
        [DataMember]
        public List<int> PersonAliasIds
        {
            get
            {
                // sort by PrimaryAliasId as the first one
                return _person.Aliases.OrderBy( a => a.AliasPersonId == Id ).Select( a => a.Id ).ToList();
            }
        }

        /// <summary>
        /// The Person's salutation title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [DataMember]
        public string Title
        {
            get
            {
                if ( _person.TitleValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.TitleValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [DataMember]
        public string FirstName => _person.FirstName;

        /// <summary>
        /// Gets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        [DataMember]
        public string NickName => _person.NickName;

        /// <summary>
        /// Gets the name of the middle.
        /// </summary>
        /// <value>
        /// The name of the middle.
        /// </value>
        [DataMember]
        public string MiddleName => _person.MiddleName;

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [DataMember]
        public string LastName => _person.LastName;

        /// <summary>
        /// Gets the suffix.
        /// </summary>
        /// <value>
        /// The suffix.
        /// </value>
        [DataMember]
        public string Suffix
        {
            get
            {
                if ( _person.SuffixValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.SuffixValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <value>
        /// The photo URL.
        /// </value>
        [DataMember]
        public string PhotoUrl
        {
            get
            {
                if ( _person.PhotoId.HasValue )
                {
                    if ( _publicApplicationRootUrl != null )
                    {
                        return new Uri( _publicApplicationRootUrl, _person.PhotoUrl ).AbsoluteUri;
                    }
                    else
                    {
                        return _person.PhotoUrl;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the birth day.
        /// </summary>
        /// <value>
        /// The birth day.
        /// </value>
        [DataMember]
        public int? BirthDay => _person.BirthDay;

        /// <summary>
        /// Gets the birth month.
        /// </summary>
        /// <value>
        /// The birth month.
        /// </value>
        [DataMember]
        public int? BirthMonth => _person.BirthMonth;

        /// <summary>
        /// Gets the birth year.
        /// </summary>
        /// <value>
        /// The birth year.
        /// </value>
        [DataMember]
        public int? BirthYear => _person.BirthYear;

        /// <summary>
        /// Gets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        [DataMember]
        public string Gender => _person.Gender.ConvertToString();

        /// <summary>
        /// Gets the marital status value identifier.
        /// </summary>
        /// <value>
        /// The marital status value identifier.
        /// </value>
        [DataMember]
        public int? MaritalStatusValueId => _person.MaritalStatusValueId;

        /// <summary>
        /// Gets the marital status.
        /// </summary>
        /// <value>
        /// The marital status.
        /// </value>
        [DataMember]
        public string MaritalStatus
        {
            get
            {
                if ( _person.MaritalStatusValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.MaritalStatusValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the anniversary date.
        /// </summary>
        /// <value>
        /// The anniversary date.
        /// </value>
        [DataMember]
        public DateTime? AnniversaryDate => _person.AnniversaryDate;

        /// <summary>
        /// Gets the graduation year.
        /// </summary>
        /// <value>
        /// The graduation year.
        /// </value>
        [DataMember]
        public int? GraduationYear => _person.GraduationYear;

        /// <summary>
        /// Gets the giving group identifier.
        /// </summary>
        /// <value>
        /// The giving group identifier.
        /// </value>
        [DataMember]
        public int? GivingGroupId => _person.GivingGroupId;

        /// <summary>
        /// Gets the giving identifier.
        /// </summary>
        /// <value>
        /// The giving identifier.
        /// </value>
        [DataMember]
        public string GivingId => _person.GivingId;

        /// <summary>
        /// Gets the giving leader identifier.
        /// </summary>
        /// <value>
        /// The giving leader identifier.
        /// </value>
        [DataMember]
        public int GivingLeaderId => _person.GivingLeaderId;

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [DataMember]
        public string Email => _person.Email;

        /// <summary>
        /// Gets the age classification.
        /// </summary>
        /// <value>
        /// The age classification.
        /// </value>
        [DataMember]
        public string AgeClassification => _person.AgeClassification.ConvertToString();

        /// <summary>
        /// Gets the primary family identifier.
        /// </summary>
        /// <value>
        /// The primary family identifier.
        /// </value>
        [DataMember]
        public int? PrimaryFamilyId => _person.PrimaryFamilyId;

        /// <summary>
        /// Gets the deceased date.
        /// </summary>
        /// <value>
        /// The deceased date.
        /// </value>
        [DataMember]
        public DateTime? DeceasedDate => _person.DeceasedDate;

        /// <summary>
        /// Gets a value indicating whether this instance is business.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is business; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsBusiness => _person.IsBusiness();

        /// <summary>
        /// Gets the home phone.
        /// </summary>
        /// <value>
        /// The home phone.
        /// </value>
        [DataMember]
        public string HomePhone
        {
            get
            {
                return _person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() )?.Number;
            }
        }

        /// <summary>
        /// Gets the mobile phone.
        /// </summary>
        /// <value>
        /// The mobile phone.
        /// </value>
        [DataMember]
        public string MobilePhone
        {
            get
            {
                return _person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.Number;
            }
        }

        /// <summary>
        /// Gets the home address.
        /// </summary>
        /// <value>
        /// The home address.
        /// </value>
        [DataMember]
        public LocationExport HomeAddress { get; private set; }

        /// <summary>
        /// Gets the record type value identifier.
        /// </summary>
        /// <value>
        /// The record type value identifier.
        /// </value>
        [DataMember]
        public int? RecordTypeValueId => _person.RecordTypeValueId;

        /// <summary>
        /// Gets the type of the record.
        /// </summary>
        /// <value>
        /// The type of the record.
        /// </value>
        [DataMember]
        public string RecordType
        {
            get
            {
                if ( _person.RecordTypeValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.RecordTypeValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the record status value identifier.
        /// </summary>
        /// <value>
        /// The record status value identifier.
        /// </value>
        [DataMember]
        public int? RecordStatusValueId => _person.RecordStatusValueId;

        /// <summary>
        /// Gets the record status.
        /// </summary>
        /// <value>
        /// The record status.
        /// </value>
        [DataMember]
        public string RecordStatus
        {
            get
            {
                if ( _person.RecordStatusValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.RecordStatusValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the record status last modified date time.
        /// </summary>
        /// <value>
        /// The record status last modified date time.
        /// </value>
        [DataMember]
        public DateTime? RecordStatusLastModifiedDateTime => _person.RecordStatusLastModifiedDateTime;

        /// <summary>
        /// Gets the record status reason value identifier.
        /// </summary>
        /// <value>
        /// The record status reason value identifier.
        /// </value>
        [DataMember]
        public int? RecordStatusReasonValueId => _person.RecordStatusReasonValueId;

        /// <summary>
        /// Gets the record status reason.
        /// </summary>
        /// <value>
        /// The record status reason.
        /// </value>
        [DataMember]
        public string RecordStatusReason
        {
            get
            {
                if ( _person.RecordStatusReasonValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.RecordStatusReasonValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the connection status value identifier.
        /// </summary>
        /// <value>
        /// The connection status value identifier.
        /// </value>
        [DataMember]
        public int? ConnectionStatusValueId => _person.ConnectionStatusValueId;

        /// <summary>
        /// Gets the connection status.
        /// </summary>
        /// <value>
        /// The connection status.
        /// </value>
        [DataMember]
        public string ConnectionStatus
        {
            get
            {
                if ( _person.ConnectionStatusValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.ConnectionStatusValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is deceased.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is deceased; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsDeceased => _person.IsDeceased;
    }
}
