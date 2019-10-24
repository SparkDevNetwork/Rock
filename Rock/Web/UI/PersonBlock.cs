// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Web.UI
{
    /// <summary>
    /// Block displaying some information about a person model
    /// </summary>
    [ContextAware( typeof(Person) )]
    public abstract class PersonBlock : ContextEntityBlock
    {
        /// <summary>
        /// The current entity as a person being viewed
        /// </summary>
        public Person Person
        {
            get => Entity as Person;
            set => Entity = value;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Person == null )
            {
                var personId = PageParameter( "PersonId" ).AsIntegerOrNull();

                if ( personId.HasValue )
                {
                    Person = new PersonService( new RockContext() ).Get( personId.Value );
                    Person?.LoadAttributes();
                }

                if ( Person == null )
                {
                    Person = new Person();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack &&
                CurrentPersonAlias != null &&
                Context.Items["PersonViewed"] == null &&
                Person != null &&
                Person.PrimaryAlias != null &&
                Person.PrimaryAlias.Id != CurrentPersonAlias.Id )
            {
                var transaction = new PersonViewTransaction();
                transaction.DateTimeViewed = RockDateTime.Now;
                transaction.TargetPersonAliasId = Person.PrimaryAlias.Id;
                transaction.ViewerPersonAliasId = CurrentPersonAlias.Id;
                transaction.Source = RockPage.PageTitle;
                transaction.IPAddress = Request.UserHostAddress;
                RockQueue.TransactionQueue.Enqueue( transaction );

                Context.Items.Add( "PersonViewed", "Handled" );
            }
        }

        #region Methods

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <param name="groupTypeGuid">The group type GUID.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "This method moved to the campus badge.", false )]
        public IEnumerable<Group> PersonGroups( string groupTypeGuid )
        {
            return PersonGroups( new Guid( groupTypeGuid ) );
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "This method moved to the campus badge.", false )]
        public IEnumerable<Group> PersonGroups( Guid groupTypeGuid )
        {
            string itemKey = "RockGroups:" + groupTypeGuid.ToString();

            if ( Context.Items.Contains( itemKey ) )
            {
                return PersonGroups( ( int ) Context.Items[itemKey] );
            }

            var groupType = GroupTypeCache.Get( groupTypeGuid );
            int groupTypeId = groupType != null ? groupType.Id : 0;

            if ( !Context.Items.Contains( itemKey ) )
            {
                Context.Items.Add( itemKey, groupTypeId );
            }

            return PersonGroups( groupTypeId );
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "This method moved to the campus badge.", false )]
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

            using ( var rockContext = new RockContext() )
            {
                var service = new GroupMemberService( rockContext );
                groups = service.Queryable()
                    .Where( m =>
                        m.PersonId == Person.Id &&
                        m.Group.GroupTypeId == groupTypeId )
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                    .ThenByDescending( m => m.Group.Name )
                    .Select( m => m.Group )
                    .OrderByDescending( g => g.Name )
                    .ToList();

                if ( !Context.Items.Contains( itemKey ) )
                {
                    Context.Items.Add( itemKey, groups );
                }

                return groups;
            }
        }

        #endregion
    }
}
