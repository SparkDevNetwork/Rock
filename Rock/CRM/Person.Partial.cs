//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel.DataAnnotations;

using Rock.Data;

namespace Rock.CRM
{
    //[MetadataType(typeof(CommentPerson))]
    public partial class Person
    {
        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        [NotMapped]
        public DateTime BirthDate
        {
            // notes
            // if no birthday is available then DateTime.MinValue is returned
            // if no birth year is given then the birth year will be DateTime.MinValue.Year
            get
            {
                if ( BirthDay == null || BirthMonth == null )
                {
                    return DateTime.MinValue;
                }
                else
                {
                    string birthYear = ( BirthYear ?? DateTime.MinValue.Year ).ToString();
                    return Convert.ToDateTime( BirthMonth.ToString() + "/" + BirthDay.ToString() + "/" + birthYear );
                }
            }

            set
            {
                BirthMonth = value.Month;
                BirthDay = value.Day;
                BirthYear = value.Year;
            }
        }
    }

#pragma warning disable
    
    public class CommentPerson
    {
        [TrackChanges]
        [Required( ErrorMessage = "First Name must be between 1 and 12 characters" )]
        [StringLength( 12, ErrorMessage = "First Name is required" )]
        public string FirstName { get; set; }

        [TrackChanges]
        public string NickName { get; set; }

        [TrackChanges]
        public string LastName { get; set; }
    }

#pragma warning restore
}
