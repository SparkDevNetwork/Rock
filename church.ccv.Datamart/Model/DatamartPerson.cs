using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Runtime.Serialization;

namespace church.ccv.Datamart.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_Datamart_Person" )]
    [DataContract]
    public partial class DatamartPerson : Rock.Data.Entity<DatamartPerson>, Rock.Data.IRockEntity
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
        [Rock.Data.FieldType( Rock.SystemGuid.FieldType.SINGLE_SELECT, "values", "Male,Female" )]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the marital status.
        /// </summary>
        /// <value>
        /// The marital status.
        /// </value>
        [DataMember]
        [MaxLength( 15 )]
        [Rock.Data.FieldType( Rock.SystemGuid.FieldType.SINGLE_SELECT, "values", "Married,Single" )]
        public string MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the family role.
        /// </summary>
        /// <value>
        /// The family role.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        [Rock.Data.FieldType( Rock.SystemGuid.FieldType.SINGLE_SELECT, "values", "Adult,Child" )]
        public string FamilyRole { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        [Column( "Campus" )]
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        [Rock.Data.FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        /// <value>
        /// The connection status.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        [Column( "ConnectionStatus" )]
        public string ConnectionStatusName { get; set; }

        [DataMember]
        [Rock.Data.DefinedValue( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS )]
        public int? ConnectionStatusValueId { get; set; }

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
        public bool IsHeadOfHousehold { get; set; }

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
        public bool IsBaptized { get; set; }

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
        /// Gets or sets the giving2015.
        /// </summary>
        /// <value>
        /// The giving2015.
        /// </value>
        [DataMember]
        public decimal? Giving2015 { get; set; }

        /// <summary>
        /// Gets or sets the giving2014.
        /// </summary>
        /// <value>
        /// The giving2014.
        /// </value>
        [DataMember]
        public decimal? Giving2014 { get; set; }

        /// <summary>
        /// Gets or sets the giving2013.
        /// </summary>
        /// <value>
        /// The giving2013.
        /// </value>
        [DataMember]
        public decimal? Giving2013 { get; set; }

        /// <summary>
        /// Gets or sets the giving2012.
        /// </summary>
        /// <value>
        /// The giving2012.
        /// </value>
        [DataMember]
        public decimal? Giving2012 { get; set; }

        /// <summary>
        /// Gets or sets the giving2011.
        /// </summary>
        /// <value>
        /// The giving2011.
        /// </value>
        [DataMember]
        public decimal? Giving2011 { get; set; }

        /// <summary>
        /// Gets or sets the giving2010.
        /// </summary>
        /// <value>
        /// The giving2010.
        /// </value>
        [DataMember]
        public decimal? Giving2010 { get; set; }

        /// <summary>
        /// Gets or sets the giving2009.
        /// </summary>
        /// <value>
        /// The giving2009.
        /// </value>
        [DataMember]
        public decimal? Giving2009 { get; set; }

        /// <summary>
        /// Gets or sets the giving2008.
        /// </summary>
        /// <value>
        /// The giving2008.
        /// </value>
        [DataMember]
        public decimal? Giving2008 { get; set; }

        /// <summary>
        /// Gets or sets the giving2007.
        /// </summary>
        /// <value>
        /// The giving2007.
        /// </value>
        [DataMember]
        public decimal? Giving2007 { get; set; }

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
        /// Gets or sets the last attended date.
        /// </summary>
        /// <value>
        /// The last attended date.
        /// </value>
        [DataMember]
        public DateTime? LastAttendedDate { get; set; }

        /// <summary>
        /// Gets or sets the last public note.
        /// </summary>
        /// <value>
        /// The last public note.
        /// </value>
        [DataMember]
        public string LastPublicNote { get; set; }

        #endregion
    }
}
