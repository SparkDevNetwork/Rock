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
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.Web.UI
{
    /// <summary>
    /// Block displaying some information about a person model
    /// </summary>
    [ContextAware( typeof( Person ) )]
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
                new AddPersonViewed.Message
                {
                    DateTimeViewed = RockDateTime.Now,
                    TargetPersonAliasId = Person.PrimaryAlias.Id,
                    ViewerPersonAliasId = CurrentPersonAlias.Id,
                    Source = RockPage.PageTitle,
                    IPAddress = Request.UserHostAddress
                }.Send();

                Context.AddOrReplaceItem( "PersonViewed", "Handled" );
            }
        }
    }
}