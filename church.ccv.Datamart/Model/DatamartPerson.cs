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
    [Table( "_church_ccv_Datamart_Person" )]
    [DataContract]
    public partial class DatamartPerson : Rock.Data.Entity<DatamartPerson>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        [DataMember]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the family identifier.
        /// </summary>
        /// <value>
        /// The family identifier.
        /// </value>

        [DataMember]
        public int FamilyId { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        [DataMember]
        public Guid PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [DataMember( IsRequired = true )]
        [MaxLength( 50 )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the name of the middle.
        /// </summary>
        /// <value>
        /// The name of the middle.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        [DataMember]
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the grade.
        /// </summary>
        /// <value>
        /// The grade.
        /// </value>
        [DataMember]
        public int? Grade { get; set; }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        [DataMember]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        [DataMember]
        [MaxLength( 10 )]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the marital status.
        /// </summary>
        /// <value>
        /// The marital status.
        /// </value>
        [DataMember]
        [MaxLength( 15 )]
        public string MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the family role.
        /// </summary>
        /// <value>
        /// The family role.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string FamilyRole { get; set; }

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
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

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
        /// Gets or sets the anniversary date.
        /// </summary>
        /// <value>
        /// The anniversary date.
        /// </value>
        [DataMember]
        public DateTime? AnniversaryDate { get; set; }

        /// <summary>
        /// Gets or sets the anniversary years.
        /// </summary>
        /// <value>
        /// The anniversary years.
        /// </value>
        [DataMember]
        public int? AnniversaryYears { get; set; }

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
        [MaxLength( 100 )]
        public string NeighborhoodName { get; set; }

        /// <summary>
        /// Gets or sets the taken starting point.
        /// </summary>
        /// <value>
        /// The taken starting point.
        /// </value>
        [DataMember]
        public bool? TakenStartingPoint { get; set; }

        /// <summary>
        /// Gets or sets the starting point date.
        /// </summary>
        /// <value>
        /// The starting point date.
        /// </value>
        [DataMember]
        public DateTime? StartingPointDate { get; set; }

        /// <summary>
        /// Gets or sets the in neighborhood group.
        /// </summary>
        /// <value>
        /// The in neighborhood group.
        /// </value>
        [DataMember]
        public bool? InNeighborhoodGroup { get; set; }

        /// <summary>
        /// Gets or sets the neighborhood group identifier.
        /// </summary>
        /// <value>
        /// The neighborhood group identifier.
        /// </value>
        [DataMember]
        public int? NeighborhoodGroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the neighborhood group.
        /// </summary>
        /// <value>
        /// The name of the neighborhood group.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string NeighborhoodGroupName { get; set; }

        /// <summary>
        /// Gets or sets the nearest group identifier.
        /// </summary>
        /// <value>
        /// The nearest group identifier.
        /// </value>
        [DataMember]
        public int? NearestGroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the nearest group.
        /// </summary>
        /// <value>
        /// The name of the nearest group.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string NearestGroupName { get; set; }

        /// <summary>
        /// Gets or sets the first visit date.
        /// </summary>
        /// <value>
        /// The first visit date.
        /// </value>
        [DataMember]
        public DateTime? FirstVisitDate { get; set; }

        /// <summary>
        /// Gets or sets the is staff.
        /// </summary>
        /// <value>
        /// The is staff.
        /// </value>
        [DataMember]
        public bool? IsStaff { get; set; }

        /// <summary>
        /// Gets or sets the photo URL.
        /// </summary>
        /// <value>
        /// The photo URL.
        /// </value>
        [DataMember]
        [MaxLength( 150 )]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the is serving.
        /// </summary>
        /// <value>
        /// The is serving.
        /// </value>
        [DataMember]
        public bool? IsServing { get; set; }

        /// <summary>
        /// Gets or sets the is era.
        /// </summary>
        /// <value>
        /// The is era.
        /// </value>
        [DataMember]
        public bool? IsEra { get; set; }

        /// <summary>
        /// Gets or sets the serving areas.
        /// </summary>
        /// <value>
        /// The serving areas.
        /// </value>
        [DataMember]
        [MaxLength( 350 )]
        public string ServingAreas { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [DataMember]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person alias identifier.
        /// </summary>
        /// <value>
        /// The created by person alias identifier.
        /// </value>
        [DataMember]
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias identifier.
        /// </summary>
        /// <value>
        /// The modified by person alias identifier.
        /// </value>
        [DataMember]
        public int? ModifiedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the name of the spouse.
        /// </summary>
        /// <value>
        /// The name of the spouse.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string SpouseName { get; set; }

        /// <summary>
        /// Gets or sets the is head of household.
        /// </summary>
        /// <value>
        /// The is head of household.
        /// </value>
        [DataMember]
        [MaxLength( 5 )]
        public string IsHeadOfHousehold { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        [DataMember]
        [MaxLength( 150 )]
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
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [DataMember]
        [MaxLength( 150 )]
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
        /// Gets or sets the cell phone.
        /// </summary>
        /// <value>
        /// The cell phone.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string CellPhone { get; set; }

        /// <summary>
        /// Gets or sets the work phone.
        /// </summary>
        /// <value>
        /// The work phone.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string WorkPhone { get; set; }

        /// <summary>
        /// Gets or sets the is baptized.
        /// </summary>
        /// <value>
        /// The is baptized.
        /// </value>
        [DataMember]
        [MaxLength( 5 )]
        public string IsBaptized { get; set; }

        /// <summary>
        /// Gets or sets the baptism date.
        /// </summary>
        /// <value>
        /// The baptism date.
        /// </value>
        [DataMember]
        public DateTime? BaptismDate { get; set; }

        /// <summary>
        /// Gets or sets the last contribution date.
        /// </summary>
        /// <value>
        /// The last contribution date.
        /// </value>
        [DataMember]
        public DateTime? LastContributionDate { get; set; }

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

        [DataMember]
        public decimal? C2007Contrib { get; set; }

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

        #endregion
    }
}
