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
    /// <seealso cref="Rock.Data.Service{Rock.Model.RegistrationRegistrant}" />
    public partial class RegistrationRegistrantService
    {
        /// <summary>
        /// Gets the group placement registrants.
        /// </summary>
        /// <param name="getGroupPlacementRegistrantsParameters">The get group placement registrants parameters.</param>
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
        private RegistrationRegistrant RegistrationRegistrant { get; set; }
        private GetGroupPlacementRegistrantsParameters Options { get; set; }

        public GroupPlacementRegistrant( RegistrationRegistrant registrationRegistrant, GetGroupPlacementRegistrantsParameters options )
        {
            this.RegistrationRegistrant = registrationRegistrant;
            this.Options = options;
        }

        public int PersonId => RegistrationRegistrant.Person.Id;

        public string PersonName => RegistrationRegistrant.Person.FullName;

        public Gender PersonGender => RegistrationRegistrant.Person.Gender;

        public int RegistrantId => RegistrationRegistrant.Id;

        public string RegistrationInstanceName => RegistrationRegistrant.Registration.RegistrationInstance.Name;

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
        /// Gets the displayed attribute values.
        /// </summary>
        /// <value>
        /// The displayed attribute values.
        /// </value>
        public Dictionary<string, string> DisplayedAttributeValues
        {
            get
            {
                if ( RegistrationRegistrant.AttributeValues == null )
                {
                    RegistrationRegistrant.LoadAttributes();
                }

                return RegistrationRegistrant.AttributeValues.Values.ToDictionary( k => k.AttributeName, v => v.ValueFormatted );


                /*var displayedAttributeIds = this.Options.DisplayedAttributeIds ?? new int[0];

                if ( !displayedAttributeIds.Any() )
                {
                    return new List<AttributeValueCache>();
                }

                var displayedAttributeList = displayedAttributeIds.Select( a => AttributeCache.Get( a ) ).Where( a => a != null ).ToList();

                if ( RegistrationRegistrant.AttributeValues == null )
                {
                    RegistrationRegistrant.LoadAttributes();
                }

                return displayedAttributeList.Select( v => RegistrationRegistrant.AttributeValues.GetValueOrNull( v.Key ) ).ToList();
                */
            }
        }

    }

    public class GetGroupPlacementRegistrantsParameters
    {
        public int RegistrationTemplateId { get; set; }
        public int[] RegistrationTemplateInstanceIds { get; set; }

        public int? RegistrationInstanceId { get; set; }
        public bool IncludeFees { get; set; }
        public int? DataViewFilterId { get; set; }
        public int[] DisplayedAttributeIds { get; set; }
        //public int? RegistrationInstanceId { get; set; }
    }
}
