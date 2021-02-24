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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel;
using Rock.Web.Cache;

namespace Rock.Obsidian.Blocks.Event
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Person Detail" )]
    [Category( "Obsidian > Event" )]
    [Description( "Block used to register for a registration instance." )]
    [IconCssClass( "fa fa-clipboard-list" )]

    public class RegistrationEntry : ObsidianBlockType
    {
        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianConfigurationValues()
        {
            var currentPerson = GetCurrentPerson();
            var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger();

            using ( var rockContext = new RockContext() )
            {
                var now = RockDateTime.Now;

                var registrationInstance = new RegistrationInstanceService( rockContext )
                    .Queryable( "RegistrationTemplate.Forms.Fields" )
                    .AsNoTracking()
                    .Where( r =>
                        r.Id == registrationInstanceId &&
                        r.IsActive &&
                        r.RegistrationTemplate != null &&
                        r.RegistrationTemplate.IsActive &&
                        ( !r.StartDateTime.HasValue || r.StartDateTime <= now ) &&
                        ( !r.EndDateTime.HasValue || r.EndDateTime > now ) )
                    .FirstOrDefault();

                var registrationTemplate = registrationInstance?.RegistrationTemplate;
                var forms = registrationTemplate?.Forms?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateForm>();
                var fields = forms.SelectMany( f => f.Fields ).OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateFormField>();
                var fieldAttributes = fields.Select( f => AttributeCache.Get( f.AttributeId ?? 0 ) ).Where( a => a != null ).ToList();

                return new
                {
                    registrationInstance = RegistrationInstanceViewModel.From( registrationInstance, currentPerson, false ),
                    registrationTemplate = RegistrationTemplateViewModel.From( registrationTemplate, currentPerson, false ),
                    registrationTemplateForms = forms.Select( f => RegistrationTemplateFormViewModel.From( f, currentPerson, false ) ),
                    registrationTemplateFormFields = fields.Select( f => RegistrationTemplateFormFieldViewModel.From( f, currentPerson, false ) ),
                    fieldAttributes = fieldAttributes.Select( a => AttributeViewModel.From( a, currentPerson, false ) )
                };
            }
        }
    }
}
