using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Model;
using Rock.Search;

namespace org.newpointe.ExtraSearch
{
    /// <summary>
    /// Searches for people with matching names, emails, or phone numbers
    /// </summary>
    [Description("Person Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Info")]
    public class Person : SearchComponent
    {

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults => new Dictionary<string, string> { { "SearchLabel", "Person Info" } };
        
        public class IntermediarySearchResult
        {
            public Rock.Model.Person Person { get; set; }
            public string ResultString { get; set; }
            public int Order { get; set; }
        }

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search(string searchterm)
        {
            return SearchPersons(searchterm, out bool reversed)
                .OrderBy(p => reversed ? p.Person.LastName : p.Person.NickName)
                .ThenBy(p => reversed ? p.Person.NickName : p.Person.LastName)
                .Select(rg => rg.ResultString);

        }

        public static IQueryable<IntermediarySearchResult> SearchPersons(string searchterm, out bool reversed_)
        {

            // Split on : and use second part as name filter
            // If has @ then only email
            // If has number then not name
            // If has space then not email
            // If has letter then not phone
            // (alternatively)
            // If no number then include name
            // If no space then include email
            // If no letter then include phone

            var terms = searchterm.Split(':');
            var search = terms[0].Trim().ToLower();
            var person = terms.Length > 1 ? terms[1].Trim().ToLower() : null;
            var rockContext = new RockContext();
            var personServ = new PersonService(rockContext);
            var searchQuery = personServ.Queryable().Take(0).Select(p => new IntermediarySearchResult { Person = null, ResultString = "", Order = 0 });
            var reversed = false;

            if (search.Contains('@'))
            {
                searchQuery = searchQuery.Union(personServ.Queryable().Where(p => p.Email.Contains(search)).Select(p => new IntermediarySearchResult { Person = p, ResultString = p.Email + " : " + (p.NickName + " " + p.LastName), Order = 0 }));
            }
            else
            {
                if (Regex.Matches(search, @"[0-9]").Count == 0)
                {
                    searchQuery = searchQuery.Union(personServ.GetByFullName(searchterm, true, false, true, out reversed).Select(p => new IntermediarySearchResult { Person = p, ResultString = reversed ? (p.LastName + ", " + p.NickName) : (p.NickName + " " + p.LastName), Order = 1 }));
                }
                if (Regex.Matches(search, @" ").Count == 0)
                {
                    searchQuery = searchQuery.Union(personServ.Queryable().Where(p => p.Email.Contains(search)).Select(p => new IntermediarySearchResult { Person = p, ResultString = p.Email + " : " + (reversed ? (p.LastName + ", " + p.NickName) : (p.NickName + " " + p.LastName)), Order = 2 }));
                }
                if (Regex.Matches(search, @"[a-zA-Z]").Count == 0)
                {
                    searchQuery = searchQuery.Union(new PhoneNumberService(rockContext).GetBySearchterm(search).Select(pn => new IntermediarySearchResult { Person = pn.Person, ResultString = pn.NumberFormatted + " : " + (reversed ? (pn.Person.LastName + ", " + pn.Person.NickName) : (pn.Person.NickName + " " + pn.Person.LastName)), Order = 3 }));
                }
            }
            reversed_ = reversed;

            if (!string.IsNullOrWhiteSpace(person))
            {
                searchQuery = searchQuery.Where(q => (q.Person.NickName + " " + q.Person.LastName).ToLower().Contains(person));
            }

            return searchQuery.GroupBy(r => r.Person)
                .Select(g => new IntermediarySearchResult
                {
                    Person = g.Key,
                    ResultString = g.OrderBy(o => o.Order).FirstOrDefault().ResultString,
                    Order = 0
                });
        }
    }
}
