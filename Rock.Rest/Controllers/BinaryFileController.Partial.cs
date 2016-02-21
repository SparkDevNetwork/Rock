// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class BinaryFileController
    {
        #region Post

        /// <summary>
        /// Adds a new person and puts them into a new family
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route("api/BinaryFiles/Image")]
        public System.Net.Http.HttpResponseMessage PostImage(Person person)
        {
            SetProxyCreation(true);

            CheckCanEdit(person);

            if (!person.IsValid)
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join(",", person.ValidationResults.Select(r => r.ErrorMessage).ToArray()));
            }

            System.Web.HttpContext.Current.Items.Add("CurrentPerson", GetPerson());
            PersonService.SaveNewPerson(person, (Rock.Data.RockContext)Service.Context, null, false);

            return ControllerContext.Request.CreateResponse(HttpStatusCode.Created, person.Id);
        }

        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route("api/BinaryFiles/Image/{binaryFileId}")]
        public void PutImage(int binaryFileId)
        {
            SetProxyCreation(true);

            var rockContext = (RockContext)Service.Context;
            var existingPerson = Service.Get(id);
            if (existingPerson != null)
            {
                var changes = new List<string>();
                History.EvaluateChange(changes, "Record Status", DefinedValueCache.GetName(existingPerson.RecordStatusValueId), DefinedValueCache.GetName(person.RecordStatusValueId));
                History.EvaluateChange(changes, "Inactive Reason", DefinedValueCache.GetName(existingPerson.RecordStatusReasonValueId), DefinedValueCache.GetName(person.RecordStatusReasonValueId));
                History.EvaluateChange(changes, "Title", DefinedValueCache.GetName(existingPerson.TitleValueId), DefinedValueCache.GetName(person.TitleValueId));
                History.EvaluateChange(changes, "First Name", existingPerson.FirstName, person.FirstName);
                History.EvaluateChange(changes, "Nick Name", existingPerson.NickName, person.NickName);
                History.EvaluateChange(changes, "Middle Name", existingPerson.MiddleName, person.MiddleName);
                History.EvaluateChange(changes, "Last Name", existingPerson.LastName, person.LastName);
                History.EvaluateChange(changes, "Suffix", DefinedValueCache.GetName(existingPerson.SuffixValueId), DefinedValueCache.GetName(person.SuffixValueId));
                History.EvaluateChange(changes, "Birth Month", existingPerson.BirthMonth, person.BirthMonth);
                History.EvaluateChange(changes, "Birth Day", existingPerson.BirthDay, person.BirthDay);
                History.EvaluateChange(changes, "Birth Year", existingPerson.BirthYear, person.BirthYear);
                History.EvaluateChange(changes, "Graduation Year", existingPerson.GraduationYear, person.GraduationYear);
                History.EvaluateChange(changes, "Anniversary Date", existingPerson.AnniversaryDate, person.AnniversaryDate);
                History.EvaluateChange(changes, "Gender", existingPerson.Gender, person.Gender);
                History.EvaluateChange(changes, "Marital Status", DefinedValueCache.GetName(existingPerson.MaritalStatusValueId), DefinedValueCache.GetName(person.MaritalStatusValueId));
                History.EvaluateChange(changes, "Connection Status", DefinedValueCache.GetName(existingPerson.ConnectionStatusValueId), DefinedValueCache.GetName(person.ConnectionStatusValueId));
                History.EvaluateChange(changes, "Email", existingPerson.Email, person.Email);
                History.EvaluateChange(changes, "Email Active", existingPerson.IsEmailActive, person.IsEmailActive);
                History.EvaluateChange(changes, "Email Preference", existingPerson.EmailPreference, person.EmailPreference);

                if (person.GivingGroupId != existingPerson.GivingGroupId)
                {
                    string oldGivingGroupName = existingPerson.GivingGroup != null ? existingPerson.GivingGroup.Name : string.Empty;
                    string newGivingGroupName = person.GivingGroup != null ? person.GivingGroup.Name : string.Empty;
                    if (person.GivingGroupId.HasValue && string.IsNullOrWhiteSpace(newGivingGroupName))
                    {
                        var givingGroup = new GroupService(rockContext).Get(person.GivingGroupId.Value);
                        newGivingGroupName = givingGroup != null ? givingGroup.Name : string.Empty;
                    }
                    History.EvaluateChange(changes, "Giving Group", oldGivingGroupName, newGivingGroupName);
                }

                if (changes.Any())
                {
                    System.Web.HttpContext.Current.Items.Add("CurrentPerson", GetPerson());

                    int? modifiedByPersonAliasId = person.ModifiedAuditValuesAlreadyUpdated ? person.ModifiedByPersonAliasId : (int?)null;

                    HistoryService.SaveChanges(
                        rockContext,
                        typeof(Person),
                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                        person.Id,
                        changes,
                        true,
                        modifiedByPersonAliasId);
                }
            }

            base.Put(id, person);
        }

        #endregion
    }
}