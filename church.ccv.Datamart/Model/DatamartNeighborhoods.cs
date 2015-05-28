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
    [Table( "_church_ccv_Datamart_Neighborhoods" )]
    [DataContract]
    public partial class DatamartNeighborhoods : Rock.Data.Entity<DatamartNeighborhoods>
    {
        /// <summary>
        /// Gets or sets the neighborhood identifier (Group.Id)
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
        /// Gets or sets the name of the neighborhood leader.
        /// </summary>
        /// <value>
        /// The name of the neighborhood leader.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string NeighborhoodLeaderName { get; set; }

        /// <summary>
        /// Gets or sets the neighborhood leader identifier.
        /// </summary>
        /// <value>
        /// The neighborhood leader identifier.
        /// </value>
        [DataMember]
        public int? NeighborhoodLeaderId { get; set; }

        /// <summary>
        /// Gets or sets the name of the neighborhood pastor.
        /// </summary>
        /// <value>
        /// The name of the neighborhood pastor.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string NeighborhoodPastorName { get; set; }

        /// <summary>
        /// Gets or sets the neighborhood pastor identifier.
        /// </summary>
        /// <value>
        /// The neighborhood pastor identifier.
        /// </value>
        [DataMember]
        public int? NeighborhoodPastorId { get; set; }

        /// <summary>
        /// Gets or sets the household count.
        /// </summary>
        /// <value>
        /// The household count.
        /// </value>
        [DataMember]
        public int? HouseholdCount { get; set; }

        /// <summary>
        /// Gets or sets the adult count.
        /// </summary>
        /// <value>
        /// The adult count.
        /// </value>
        [DataMember]
        public int? AdultCount { get; set; }

        /// <summary>
        /// Gets or sets the adults in groups.
        /// </summary>
        /// <value>
        /// The adults in groups.
        /// </value>
        [DataMember]
        public int? AdultsInGroups { get; set; }

        /// <summary>
        /// Gets or sets the adults in groups percentage.
        /// </summary>
        /// <value>
        /// The adults in groups percentage.
        /// </value>
        [DataMember]
        public decimal AdultsInGroupsPercentage { get; set; }

        /// <summary>
        /// Gets or sets the adults baptized.
        /// </summary>
        /// <value>
        /// The adults baptized.
        /// </value>
        [DataMember]
        public int? AdultsBaptized { get; set; }

        /// <summary>
        /// Gets or sets the adults baptized percentage.
        /// </summary>
        /// <value>
        /// The adults baptized percentage.
        /// </value>
        [DataMember]
        public decimal AdultsBaptizedPercentage { get; set; }

        /// <summary>
        /// Gets or sets the adult member count.
        /// </summary>
        /// <value>
        /// The adult member count.
        /// </value>
        [DataMember]
        public int? AdultMemberCount { get; set; }

        /// <summary>
        /// Gets or sets the adult members in groups.
        /// </summary>
        /// <value>
        /// The adult members in groups.
        /// </value>
        [DataMember]
        public int? AdultMembersInGroups { get; set; }

        /// <summary>
        /// Gets or sets the adult members in groups percentage.
        /// </summary>
        /// <value>
        /// The adult members in groups percentage.
        /// </value>
        [DataMember]
        public decimal AdultMembersInGroupsPercentage { get; set; }

        /// <summary>
        /// Gets or sets the adult attendees.
        /// </summary>
        /// <value>
        /// The adult attendees.
        /// </value>
        [DataMember]
        public int? AdultAttendees { get; set; }

        /// <summary>
        /// Gets or sets the adult attendees in groups.
        /// </summary>
        /// <value>
        /// The adult attendees in groups.
        /// </value>
        [DataMember]
        public int? AdultAttendeesInGroups { get; set; }

        /// <summary>
        /// Gets or sets the adult attendees in groups percentage.
        /// </summary>
        /// <value>
        /// The adult attendees in groups percentage.
        /// </value>
        [DataMember]
        public decimal AdultAttendeesInGroupsPercentage { get; set; }

        /// <summary>
        /// Gets or sets the adult visitors.
        /// </summary>
        /// <value>
        /// The adult visitors.
        /// </value>
        [DataMember]
        public int? AdultVisitors { get; set; }

        /// <summary>
        /// Gets or sets the adult participants.
        /// </summary>
        /// <value>
        /// The adult participants.
        /// </value>
        [DataMember]
        public int? AdultParticipants { get; set; }

        /// <summary>
        /// Gets or sets the children count.
        /// </summary>
        /// <value>
        /// The children count.
        /// </value>
        [DataMember]
        public int? ChildrenCount { get; set; }

        /// <summary>
        /// Gets or sets the group count.
        /// </summary>
        /// <value>
        /// The group count.
        /// </value>
        [DataMember]
        public int? GroupCount { get; set; }
    }
}
