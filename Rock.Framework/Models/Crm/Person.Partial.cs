using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations;

namespace Rock.Models.Crm
{
    //[MetadataType(typeof(CommentPerson))]
    public partial class Person
    {
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

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
}
