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

using System.ComponentModel;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Crm.PersonDetail.PersonEditControl;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Displays a control that navigates to the person edit page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Person Edit" )]
    [Category( "CRM > Person Edit" )]
    [Description( "Allows you to navigate to the person edit page." )]
    [IconCssClass( "ti ti-pencil" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "3D8A8B2C-7221-4157-A89D-5777FC44284E" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "0F574631-3CCF-4E92-9524-4B7D0605A9E3" )]
    [Rock.SystemGuid.BlockTypeGuid( "8C94620B-0FC1-4C39-9474-1714546E7D9E" )]
    public class PersonEditControl : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new PersonEditControlOptionsBag();

            var person = GetPerson();

            // Only expose the edit link when there is a person to edit and the
            // current user is authorized to edit this block.
            if ( person != null && BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                box.EditPageUrl = RequestContext.ResolveRockUrl( $"~/Person/{person.IdKey}/Edit" );
            }

            return box;
        }

        /// <summary>
        /// Gets the person to be edited, preferring the block context and
        /// falling back to the person identified by the page parameter.
        /// </summary>
        /// <returns>The <see cref="Person"/> to edit, or <c>null</c> if one could not be determined.</returns>
        private Person GetPerson()
        {
            var contextPerson = RequestContext.GetContextEntity<Person>();

            if ( contextPerson != null )
            {
                return contextPerson;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );

            return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        #endregion Methods
    }
}
