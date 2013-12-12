//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Model;

namespace Rock.Web.UI
{
    /// <summary>
    /// A Block used on the person detail page
    /// </summary>
    [ContextAware( typeof(Person) )]
    public class PersonBlock : RockBlock
    {
        /// <summary>
        /// The current person being viewed
        /// </summary>
        public Person Person { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            Person = this.ContextEntity<Person>();

            if ( Person == null )
            {
                Person = new Person();
            }
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <param name="groupTypeGuid">The group type GUID.</param>
        /// <returns></returns>
        public IEnumerable<Group> PersonGroups( string groupTypeGuid )
        {
            return PersonGroups( new Guid( groupTypeGuid ) );
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Group> PersonGroups(Guid groupTypeGuid)
        {
            string itemKey = "RockGroups:" + groupTypeGuid.ToString();

            if ( Context.Items.Contains( itemKey ) )
            {
                return PersonGroups( (int)Context.Items[itemKey] );
            }

            var groupType = Rock.Web.Cache.GroupTypeCache.Read( groupTypeGuid );
            int groupTypeId = groupType != null ? groupType.Id : 0;
            Context.Items.Add( itemKey, groupTypeId );

            return PersonGroups( groupTypeId );
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Group> PersonGroups( int groupTypeId )
        {
            string itemKey = "RockGroups:" + groupTypeId.ToString();

            var groups = Context.Items[itemKey] as IEnumerable<Group>;
            if ( groups != null )
            {
                return groups;
            }

            if ( Person == null )
            {
                return null;
            }

            var service = new GroupMemberService();
            groups = service.Queryable()
                .Where( m =>
                    m.PersonId == Person.Id &&
                    m.Group.GroupTypeId == groupTypeId )
                .Select( m => m.Group )
                .OrderByDescending( g => g.Name );

            Context.Items.Add( itemKey, groups );

            return groups;
        }
    }

}