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
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class SchedulePicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = "?getCategorizedItems=true&showUnnamedEntityItems=false&showCategoriesThatHaveNoChildren=false";
            ItemRestUrlExtraParams += "&entityTypeId=" + EntityTypeCache.Read( Rock.SystemGuid.EntityType.SCHEDULE.AsGuid() ).Id;
            this.IconCssClass = "fa fa-calendar";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        public void SetValue( Schedule schedule )
        {
            if ( schedule != null )
            {
                ItemId = schedule.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = schedule.Category;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemName = schedule.Name;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="schedules">The schedules.</param>
        public void SetValues( IEnumerable<Schedule> schedules )
        {
            var scheduleList = schedules.ToList();

            if ( scheduleList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var schedule in scheduleList )
                {
                    if ( schedule != null )
                    {
                        ids.Add( schedule.Id.ToString() );
                        names.Add( schedule.Name );
                        var parentCategory = schedule.Category;

                        while ( parentCategory != null )
                        {
                            parentCategoryIds += parentCategory.Id.ToString() + ",";
                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var schedule = new ScheduleService( new RockContext() ).Get( int.Parse( ItemId ) );
            SetValue( schedule );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var schedules = new ScheduleService( new RockContext() ).Queryable().Where( g => ItemIds.Contains( g.Id.ToString() ) );
            this.SetValues( schedules );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/Categories/GetChildren/"; }
        }
    }
}