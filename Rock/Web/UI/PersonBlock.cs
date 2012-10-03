using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Crm;
using Rock.Groups;

namespace Rock.Web.UI
{
    /// <summary>
    /// A RockBlock used on the person detail page
    /// </summary>
    public class PersonBlock : RockBlock
    {
        /// <summary>
        /// The current person being viewed
        /// </summary>
        protected Person Person { get; set; }

        /// <summary>
        /// Gets a list of any context entities that the block requires.
        /// </summary>
        public override List<string> RequiredContext
        {
            get
            {
                var requiredContext = base.RequiredContext;
                requiredContext.Add( "Rock.Crm.Person" );
                return requiredContext;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.Person = CurrentPage.GetCurrentContext( "Rock.Crm.Person" ) as Rock.Crm.Person;
            if ( Person == null )
                Person = new Person();
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<Group> PersonGroups(Guid groupTypeGuid)
        {
            string itemKey = "RockGroups:" + groupTypeGuid.ToString();

            var groups = Context.Items[itemKey] as IEnumerable<Group>;
            if ( groups != null )
                return groups;

            if ( Person == null )
                return null;

            var service = new MemberService();
            groups = service.Queryable()
                .Where( m =>
                    m.PersonId == Person.Id &&
                    m.Group.GroupType.Guid == groupTypeGuid )
                .Select( m => m.Group )
                .OrderByDescending( g => g.Name );

            Context.Items.Add( itemKey, groups );

            return groups;
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<Group> PersonGroups( int groupTypeId )
        {
            string itemKey = "RockGroups:" + groupTypeId.ToString();

            var groups = Context.Items[itemKey] as IEnumerable<Group>;
            if ( groups != null )
                return groups;

            if ( Person == null )
                return null;

            var service = new MemberService();
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