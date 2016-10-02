using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Person Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class PersonIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the record status.
        /// </summary>
        /// <value>
        /// The record status.
        /// </value>
        [RockIndexField]
        public string RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [RockIndexField( Boost = 2 )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        [RockIndexField( Boost = 2 )]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [RockIndexField( Boost = 5 )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>
        /// The suffix.
        /// </value>
        [RockIndexField]
        public string Suffix { get; set; }

        /// <summary>
        /// Gets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public override string IconCssClass
        {
            get
            {
                return "fa fa-user";
            }
        }

        /// <summary>
        /// Loads the by model.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static PersonIndex LoadByModel(Person person )
        {
            var personIndex = new PersonIndex();
            personIndex.SourceIndexModel = "Rock.Model.Person";

            personIndex.Id = person.Id;
            personIndex.FirstName = person.FirstName;
            personIndex.NickName = person.NickName;
            personIndex.LastName = person.LastName;
            //personIndex.NameLong = person.FirstName + " " + person.MiddleName + " " + person.LastName;
            personIndex.RecordStatus = person.RecordStatusValue != null ? person.RecordStatusValue.Value : "Unknown";
            personIndex.Suffix = person.SuffixValue.Value;

            AddIndexableAttributes( personIndex, person );

            return personIndex;
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="displayOptions"></param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null )
        {
            string url = "/Person/";

            if (displayOptions != null )
            {
                if ( displayOptions.ContainsKey( "Person.Url" ) )
                {
                    url = displayOptions["Person.Url"].ToString();
                }
            }

            return new FormattedSearchResult() { IsViewAllowed = true, FormattedResult = string.Format( "<a href='{0}{1}'>{2} {3} {4} <small>(Person)</small></a>"
                , url // 0
                , this.Id // 1
                , this.NickName // 2
                , this.LastName // 3
                , this.Suffix // 4 
                ) };
        }
    }
}
