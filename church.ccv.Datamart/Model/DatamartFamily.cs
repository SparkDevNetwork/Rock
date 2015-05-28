using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Runtime.Serialization;
using church.ccv.Datamart.Data;

namespace church.ccv.Datamart.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_Datamart_Family" )]
    [DataContract]
    public partial class DatamartFamily : Rock.Data.Entity<DatamartFamily>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the family identifier.
        /// </summary>
        /// <value>
        /// The family identifier.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int FamilyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the family.
        /// </summary>
        /// <value>
        /// The name of the family.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the Head of Household person identifier.
        /// </summary>
        /// <value>
        /// The Head of Household person identifier.
        /// </value>
        [DataMember]
        public int? HHPersonId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the Head of Household.
        /// </summary>
        /// <value>
        /// The first name of the Head of Household.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string HHFirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the Head of Household nick.
        /// </summary>
        /// <value>
        /// The name of the Head of Household nick.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string HHNickName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the Head of Household.
        /// </summary>
        /// <value>
        /// The last name of the Head of Household.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string HHLastName { get; set; }

        /// <summary>
        /// Gets or sets the full name of the Head of Household.
        /// </summary>
        /// <value>
        /// The full name of the Head of Household.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string HHFullName { get; set; }

        /// <summary>
        /// Gets or sets the Head of Household gender.
        /// </summary>
        /// <value>
        /// The Head of Household gender.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string HHGender { get; set; }

        /// <summary>
        /// Gets or sets the Head of Household member status.
        /// </summary>
        /// <value>
        /// The Head of Household member status.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string HHMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the Head of Household marital status.
        /// </summary>
        /// <value>
        /// The Head of Household marital status.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string HHMaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the Head of Household first visit.
        /// </summary>
        /// <value>
        /// The Head of Household first visit.
        /// </value>
        [DataMember]
        public DateTime? HHFirstVisit { get; set; }

        /// <summary>
        /// Gets or sets the Head of Household first activity.
        /// </summary>
        /// <value>
        /// The Head of Household first activity.
        /// </value>
        [DataMember]
        public DateTime? HHFirstActivity { get; set; }

        /// <summary>
        /// Gets or sets the Head of Household age.
        /// </summary>
        /// <value>
        /// The Head of Household age.
        /// </value>
        [DataMember]
        public int? HHAge { get; set; }

        /// <summary>
        /// Gets or sets the neighborhood identifier.
        /// </summary>
        /// <value>
        /// The neighborhood identifier.
        /// </value>
        [DataMember]
        public int? NeighborhoodId { get; set; }

        /// <summary>
        /// Gets or sets the name of the neighborhood.
        /// </summary>
        /// <value>
        /// The name of the neighborhood.
        /// </value>
        [DataMember]
        public string NeighborhoodName { get; set; }

        /// <summary>
        /// Gets or sets the in neighborhood group.
        /// </summary>
        /// <value>
        /// The in neighborhood group.
        /// </value>
        [DataMember]
        public bool? InNeighborhoodGroup { get; set; }

        /// <summary>
        /// Gets or sets the is era.
        /// </summary>
        /// <value>
        /// The is era.
        /// </value>
        [DataMember]
        public bool? IsEra { get; set; }

        /// <summary>
        /// Gets or sets the name of the nearest neighborhood group.
        /// </summary>
        /// <value>
        /// The name of the nearest neighborhood group.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string NearestNeighborhoodGroupName { get; set; }

        /// <summary>
        /// Gets or sets the nearest neighborhood group identifier.
        /// </summary>
        /// <value>
        /// The nearest neighborhood group identifier.
        /// </value>
        [DataMember]
        public int? NearestNeighborhoodGroupId { get; set; }

        /// <summary>
        /// Gets or sets the is serving.
        /// </summary>
        /// <value>
        /// The is serving.
        /// </value>
        [DataMember]
        public bool? IsServing { get; set; }

        /// <summary>
        /// Gets or sets the attendance16 week.
        /// </summary>
        /// <value>
        /// The attendance16 week.
        /// </value>
        [DataMember]
        public int? Attendance16Week { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        /// <value>
        /// The connection status.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the home phone.
        /// </summary>
        /// <value>
        /// The home phone.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string HomePhone { get; set; }

        /// <summary>
        /// Gets or sets the adult count.
        /// </summary>
        /// <value>
        /// The adult count.
        /// </value>
        [DataMember]
        public int? AdultCount { get; set; }

        /// <summary>
        /// Gets or sets the child count.
        /// </summary>
        /// <value>
        /// The child count.
        /// </value>
        [DataMember]
        public int? ChildCount { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the geo point.
        /// </summary>
        /// <value>
        /// The geo point.
        /// </value>
        [DataMember]
        public DbGeography GeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [DataMember]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [DataMember]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Campus { get; set; }

        /// <summary>
        /// Gets or sets the adult names.
        /// </summary>
        /// <value>
        /// The adult names.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string AdultNames { get; set; }

        /// <summary>
        /// Gets or sets the child names.
        /// </summary>
        /// <value>
        /// The child names.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string ChildNames { get; set; }

        /// <summary>
        /// Gets or sets the C2015 contrib.
        /// </summary>
        /// <value>
        /// The C2015 contrib.
        /// </value>
        [DataMember]
        public decimal? C2015Contrib { get; set; }

        /// <summary>
        /// Gets or sets the C2014 contrib.
        /// </summary>
        /// <value>
        /// The C2014 contrib.
        /// </value>
        [DataMember]
        public decimal? C2014Contrib { get; set; }

        /// <summary>
        /// Gets or sets the C2013 contrib.
        /// </summary>
        /// <value>
        /// The C2013 contrib.
        /// </value>
        [DataMember]
        public decimal? C2013Contrib { get; set; }

        /// <summary>
        /// Gets or sets the C2012 contrib.
        /// </summary>
        /// <value>
        /// The C2012 contrib.
        /// </value>
        [DataMember]
        public decimal? C2012Contrib { get; set; }

        /// <summary>
        /// Gets or sets the C2011 contrib.
        /// </summary>
        /// <value>
        /// The C2011 contrib.
        /// </value>
        [DataMember]
        public decimal? C2011Contrib { get; set; }

        /// <summary>
        /// Gets or sets the C2010 contrib.
        /// </summary>
        /// <value>
        /// The C2010 contrib.
        /// </value>
        [DataMember]
        public decimal? C2010Contrib { get; set; }

        /// <summary>
        /// Gets or sets the C2009 contrib.
        /// </summary>
        /// <value>
        /// The C2009 contrib.
        /// </value>
        [DataMember]
        public decimal? C2009Contrib { get; set; }

        /// <summary>
        /// Gets or sets the C2008 contrib.
        /// </summary>
        /// <value>
        /// The C2008 contrib.
        /// </value>
        [DataMember]
        public decimal? C2008Contrib { get; set; }

        /// <summary>
        /// Gets or sets the C2007 contrib.
        /// </summary>
        /// <value>
        /// The C2007 contrib.
        /// </value>
        [DataMember]
        public decimal? C2007Contrib { get; set; }

        #endregion
    }
}
