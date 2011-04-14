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
    }

    public class CommentPerson
    {
        [TrackChanges]
        [Required( ErrorMessageResourceType = typeof( Rock.Framework.Properties.ValidationMessages ), ErrorMessageResourceName = "PersonFirstNameRequired" )]
        [StringLength( 12, ErrorMessageResourceType = typeof( Rock.Framework.Properties.ValidationMessages ), ErrorMessageResourceName = "PersonFirstNameLength" )]
        public string FirstName { get; set; }

        [TrackChanges]
        public string NickName { get; set; }

        [TrackChanges]
        public string LastName { get; set; }
    }
}
