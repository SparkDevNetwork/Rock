using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegistrationRegistrantService
    {
        /// <summary>
        /// Gets the group placement registrants.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public List<GroupPlacementRegistrant> GetGroupPlacementRegistrants( GetGroupPlacementRegistrantsParameters options )
        {
            var rockContext = this.Context as RockContext;

            var registrationRegistrantService = new RegistrationRegistrantService( rockContext );
            var registrationRegistrantServiceQuery = registrationRegistrantService.Queryable();

            registrationRegistrantServiceQuery = registrationRegistrantServiceQuery
                .Where( a => a.Registration.RegistrationInstance.RegistrationTemplateId == options.RegistrationTemplateId );

            if ( options.RegistrationInstanceId.HasValue && options.RegistrationInstanceId > 0 )
            {
                registrationRegistrantServiceQuery = registrationRegistrantServiceQuery.Where( a => a.Registration.RegistrationInstanceId == options.RegistrationInstanceId.Value );
            }
            else
            {
                if ( options.RegistrationTemplateInstanceIds?.Any() == true )
                {
                    registrationRegistrantServiceQuery = registrationRegistrantServiceQuery.Where( a => options.RegistrationTemplateInstanceIds.Contains( a.Registration.RegistrationInstanceId ) );
                }
            }

            if ( options.DataViewFilterId.HasValue )
            {
                var dataFilter = new DataViewFilterService( new RockContext() ).Get( options.DataViewFilterId.Value );
                List<string> errorMessages = new List<string>();

                var expression = dataFilter.GetExpression( typeof( RegistrationRegistrant ), registrationRegistrantService, registrationRegistrantService.ParameterExpression, errorMessages );
                if ( expression != null )
                {
                    registrationRegistrantServiceQuery = registrationRegistrantServiceQuery.Where( registrationRegistrantService.ParameterExpression, expression );
                }
            }

            if ( options.RegistrantId.HasValue )
            {
                registrationRegistrantServiceQuery = registrationRegistrantServiceQuery.Where( a => a.Id == options.RegistrantId.Value );
            }

            registrationRegistrantServiceQuery = registrationRegistrantServiceQuery.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.NickName );

            var groupPlacementRegistrantList = registrationRegistrantServiceQuery.ToList()
                .Select( r => new GroupPlacementRegistrant( r, options ) )
                .ToList();

            return groupPlacementRegistrantList;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupPlacementRegistrant
    {
        /// <summary>
        /// Gets or sets the registration registrant.
        /// </summary>
        /// <value>
        /// The registration registrant.
        /// </value>
        private RegistrationRegistrant RegistrationRegistrant { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        private GetGroupPlacementRegistrantsParameters Options { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPlacementRegistrant"/> class.
        /// </summary>
        /// <param name="registrationRegistrant">The registration registrant.</param>
        /// <param name="options">The options.</param>
        public GroupPlacementRegistrant( RegistrationRegistrant registrationRegistrant, GetGroupPlacementRegistrantsParameters options )
        {
            this.RegistrationRegistrant = registrationRegistrant;
            this.Options = options;
        }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId => RegistrationRegistrant.Person.Id;

        /// <summary>
        /// Gets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName => RegistrationRegistrant.Person.FullName;

        /// <summary>
        /// Gets the person gender.
        /// </summary>
        /// <value>
        /// The person gender.
        /// </value>
        public Gender PersonGender => RegistrationRegistrant.Person.Gender;

        /// <summary>
        /// Gets the registrant identifier.
        /// </summary>
        /// <value>
        /// The registrant identifier.
        /// </value>
        public int RegistrantId => RegistrationRegistrant.Id;

        /// <summary>
        /// Gets the name of the registration instance.
        /// </summary>
        /// <value>
        /// The name of the registration instance.
        /// </value>
        public string RegistrationInstanceName => RegistrationRegistrant.Registration.RegistrationInstance.Name;

        /// <summary>
        /// Gets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        public Dictionary<string, decimal> Fees
        {
            get
            {
                if ( this.Options.IncludeFees )
                {
                    var feeInfo = RegistrationRegistrant.Fees.Select( a => new
                    {
                        FeeName = a.RegistrationTemplateFeeItem?.Name ?? "Fee",
                        a.TotalCost
                    } ).ToDictionary( a => a.FeeName, v => v.TotalCost );

                    return feeInfo;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, AttributeCache> Attributes
        {
            get
            {
                if ( RegistrationRegistrant.AttributeValues == null )
                {
                    RegistrationRegistrant.LoadAttributes();
                }

                var displayedAttributeValues = RegistrationRegistrant
                        .Attributes.Where( a => Options.DisplayedAttributeIds.Contains( a.Value.Id ) )
                        .ToDictionary( k => k.Key, v => v.Value );

                return displayedAttributeValues;
            }
        }


        /// <summary>
        /// Gets the displayed attribute values.
        /// </summary>
        /// <value>
        /// The displayed attribute values.
        /// </value>
        public Dictionary<string, AttributeValueCache> AttributeValues
        {
            get
            {
                if ( RegistrationRegistrant.AttributeValues == null )
                {
                    RegistrationRegistrant.LoadAttributes();
                }

                var displayedAttributeValues = RegistrationRegistrant
                    .AttributeValues.Where( a => Options.DisplayedAttributeIds.Contains( a.Value.AttributeId ) )
                    .ToDictionary( k => k.Key, v => v.Value );

                return displayedAttributeValues;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GetGroupPlacementRegistrantsParameters
    {
        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the registration template instance ids.
        /// </summary>
        /// <value>
        /// The registration template instance ids.
        /// </value>
        public int[] RegistrationTemplateInstanceIds { get; set; }

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        public int? RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the registrant identifier.
        /// </summary>
        /// <value>
        /// The registrant identifier.
        /// </value>
        public int? RegistrantId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include fees].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include fees]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeFees { get; set; }

        /// <summary>
        /// Gets or sets the data view filter identifier.
        /// </summary>
        /// <value>
        /// The data view filter identifier.
        /// </value>
        public int? DataViewFilterId { get; set; }

        /// <summary>
        /// Gets or sets the displayed attribute ids.
        /// </summary>
        /// <value>
        /// The displayed attribute ids.
        /// </value>
        public int[] DisplayedAttributeIds { get; set; } = new int[0];
    }
}
