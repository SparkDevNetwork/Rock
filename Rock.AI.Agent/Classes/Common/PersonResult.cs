using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Rock.Model;

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// A common POCO for storing information about a person. Apply only the properties that are needed for the specific use case.
    /// Null properties will not be serialized.
    /// </summary>
    class PersonResult
    {
        #region Ignored Properties
        // These properties exist to help with internal logic but they should not be serialized to JSON.

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        [JsonIgnore]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the primary family identifier.
        /// </summary>
        [JsonIgnore]
        public int? PrimaryFamilyId { get; set; }

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        [JsonIgnore]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the record type value identifier.
        /// </summary>
        [JsonIgnore]
        public int? RecordTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        [JsonIgnore]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the marital status unique identifier.
        /// </summary>
        [JsonIgnore]
        public Guid? MaritalStatusGuid { get; set; }


        #endregion

        #region Common Properties
        /// <summary>
        /// Gets or sets the stable identifier for the person (used by functions; avoid showing to end users).
        /// </summary>
        public string PersonKey {
            get
            {
                return this.PersonId.AsIdKey();
            }
        }

        /// <summary>
        /// Gets or sets the person's first/given name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's nickname.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the person's last/family name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's name suffix.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets the URL for the person's avatar image.
        /// </summary>
        public string AvatarUrl
        {
            get
            {
                var initials = this.FirstName.Left( 1 ) + this.LastName.Left( 1 );
                return Rock.Model.Person.GetPersonPhotoUrl(
                    initials,
                    this.PhotoId,
                    this.Age,
                    this.Gender,
                    this.RecordTypeValueId,
                    this.AgeClassification );
            }
        }

        /// <summary>
        /// Gets or sets the age classification.
        /// </summary>
        public AgeClassification AgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the spouse person result.
        /// </summary>
        public PersonResult Spouse { get; set; }

        /// <summary>
        /// Gets or sets the list of children in the family.
        /// </summary>
        public List<PersonResult> ChildrenInFamily { get; set; }

        /// <summary>
        /// Gets or sets the list of adults in the family.
        /// </summary>
        public List<PersonResult> AdultsInFamily { get; set; }

        /// <summary>
        /// Gets or sets the campus name.
        /// </summary>
        public string Campus { get; set; }

        /// <summary>
        /// Gets the campus key (hashed identifier).
        /// </summary>
        public string CampusKey
        {
            get
            {
                if ( !this.CampusId.HasValue )
                {
                    return null;
                }

                return this.CampusId.Value.AsIdKey();
            }
        }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the record status.
        /// </summary>
        public string RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the marital status.
        /// </summary>
        public string MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public Gender Gender { get; set; }
        #endregion
    }
}
