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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;

namespace Rock.Model
{
    public partial class RegistrationTemplateFormService
    {
        /// <summary>
        /// Tries to load fields from the database for any registration template forms that are missing fields.
        /// <para>
        /// "Missing" in this context means the <see cref="RegistrationTemplateForm.Fields"/> collection is <see langword="null"/> or empty.
        /// </para>
        /// </summary>
        /// <param name="forms">The forms whose fields should be loaded if missing.</param>
        /// <returns>A Tuple indicating if any forms were missing fields, and more details if so.</returns>
        [RockInternal( "1.15.2" )]
        public ( bool wereFieldsMissing, string details ) TryLoadMissingFields( IList<RegistrationTemplateForm> forms )
        {
            var formIds = ( forms ?? new List<RegistrationTemplateForm>() )
                .Where( f => f.Fields?.Any() != true && f.Id > 0 )
                .Select( f => f.Id )
                .ToList();

            if ( !formIds.Any() )
            {
                return ( false, null );
            }

            var preLoadMessage = $"{formIds.Count} RegistrationTemplateForm(s) missing Fields (Form IDs: {formIds.AsDelimited( ", " )}).";

            // Try to load the fields from the database in bulk.
            var fieldsByFormId = new RegistrationTemplateFormFieldService( Context as RockContext )
                .Queryable()
                .Include( f => f.Attributes )
                .Where( f => formIds.Contains( f.RegistrationTemplateFormId ) )
                .GroupBy( f => f.RegistrationTemplateFormId )
                .ToDictionary( g => g.Key, g => g.ToList() );

            foreach ( var formFields in fieldsByFormId )
            {
                var form = forms.FirstOrDefault( f => f.Id == formFields.Key );
                if ( form != null )
                {
                    form.Fields = formFields.Value;
                }
            }

            // Re-check to see if we successfully loaded the fields for all forms.
            formIds = forms.Where( f => f.Fields?.Any() != true && f.Id > 0 )
                .Select( f => f.Id )
                .ToList();

            var formCount = formIds.Count;
            var formsRemainString = formCount == 0
                ? "all Forms now have Fields."
                : $"{formCount} Form(s) still missing Fields (Form IDs: {formIds.AsDelimited( ", " )}).";

            var postLoadMessage = $"After attempting to load Fields, {formsRemainString}";

            return ( true, $"{preLoadMessage} {postLoadMessage}" );
        }
    }
}
