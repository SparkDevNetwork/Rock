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
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GradePicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfWeekPicker"/> class.
        /// </summary>
        public GradePicker()
            : base()
        {
            Label = GlobalAttributesCache.Read().GetValue( "core.GradeLabel" );

            PopulateItems();
        }

        /// <summary>
        /// Populates the items.
        /// </summary>
        private void PopulateItems()
        {
            this.Items.Clear();
            
            // add blank item as first item
            this.Items.Add( new ListItem() );

            var schoolGrades = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
            if ( schoolGrades != null )
            {
                foreach ( var schoolGrade in schoolGrades.DefinedValues.OrderByDescending( a => a.Value.AsInteger() ) )
                {
                    ListItem listItem = new ListItem();
                    if (UseAbbreviation) 
                    {
                        string abbreviation = schoolGrade.GetAttributeValue( "Abbreviation" );
                        listItem.Text = string.IsNullOrWhiteSpace( abbreviation ) ? schoolGrade.Description : abbreviation;
                    }
                    else
                    {
                        listItem.Text = schoolGrade.Description;
                    }

                    listItem.Value = UseGradeOffsetAsValue ? schoolGrade.Value : schoolGrade.Guid.ToString();

                    this.Items.Add( listItem );
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use grade offset as value].
        /// True uses the DefinedValue.Value as the ListItem.Value.
        /// False (default) uses the DefinedValue.Guid as the ListItem.Value.
        /// </summary>
        /// <value>
        /// <c>true</c> if [use grade offset as value]; otherwise, <c>false</c>.
        /// </value>
        public bool UseGradeOffsetAsValue
        {
            get
            {
                return ViewState["UseGradeOffsetAsValue"] as bool? ?? false; ;
            }

            set
            {
                if ( ( ViewState["UseGradeOffsetAsValue"] as bool? ?? false ) != value )
                {
                    ViewState["UseGradeOffsetAsValue"] = value;
                    PopulateItems();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use abbreviation].
        /// True uses the DefinedValue.GetAttributeValue("Abbreviation") as the ListItem.Text.
        /// False (default) uses the DefinedValue.Description as the ListItem.Text;
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use abbreviation]; otherwise, <c>false</c>.
        /// </value>
        public bool UseAbbreviation
        {
            get
            {
                return ViewState["UseAbbreviation"] as bool? ?? false;
            }

            set
            {
                if ( ( ViewState["UseAbbreviation"] as bool? ?? false ) != value )
                {
                    ViewState["UseAbbreviation"] = value;
                    PopulateItems();
                }
            }
        }

        /// <summary>
        /// Gets the maximum grade offset for grades that are defined 
        /// For example, 12 (Kindergarten) would be the max grade offset (or whatever the lowest grade level is)
        /// </summary>
        /// <value>
        /// The maximum grade offset.
        /// </value>
        public int MaxGradeOffset
        {
            get
            {
                var schoolGrades = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
                return schoolGrades.DefinedValues.Select( a => a.Value.AsInteger() ).Max();
            }
        }

        /// <summary>
        /// Gets the javascript for year picker.
        /// </summary>
        /// <param name="ypGraduationYear">The yp graduation year.</param>
        /// <returns></returns>
        public string GetJavascriptForYearPicker( YearPicker ypGraduationYear)
        {
            DateTime gradeTransitionDate = GlobalAttributesCache.Read().GetValue( "GradeTransitionDate" ).AsDateTime() ?? new DateTime( RockDateTime.Now.Year, 6, 1 );

            // add a year if the next graduation mm/dd won't happen until next year
            int gradeOffsetRefactor = ( RockDateTime.Now < gradeTransitionDate ) ? 0 : 1;

            string gradeSelectionScriptFormat = @"
    $('#{0}').change(function(){{
        var selectedGradeOffsetValue = $(this).val();
        if ( selectedGradeOffsetValue == '') {{
            $('#{1}').val('');
        }} else {{
            $('#{1}').val( {2} + ( {3} + parseInt( selectedGradeOffsetValue ) ) );
        }} 
    }});

    $('#{1}').change(function(){{
        var selectedYearValue = $(this).val();
        if (selectedYearValue == '') {{
            $('#{0}').val('');
        }} else {{
            var gradeOffset = ( parseInt( selectedYearValue ) - {4} ) - {3};
            if (gradeOffset >= 0 ) {{
                $('#{0}').val(gradeOffset.toString());

                // if there is a gap in gradeOffsets (grade is combined), keep trying if we haven't hit an actual offset yet
                while (!$('#{0}').val() && gradeOffset <= {5}) {{
                    $('#{0}').val(gradeOffset++);
                }}
            }} else {{
                $('#{0}').val('');
            }}
        }}
    }});";
            string script = string.Format(
                gradeSelectionScriptFormat,
                this.ClientID,     // {0}
                ypGraduationYear.ClientID,       // {1}
                gradeTransitionDate.Year,   // {2}
                gradeOffsetRefactor,   // {3}
                RockDateTime.Now.Year, // {4}
                this.MaxGradeOffset // {5}
                );

            return script;
        }

        /// <summary>
        /// Gets or sets the selected grade value unique identifier.
        /// </summary>
        /// <value>
        /// The selected grade value unique identifier.
        /// </value>
        public DefinedValueCache SelectedGradeValue
        {
            get
            {
                return DefinedValueCache.Read( this.SelectedValue.AsGuid() );
            }
            set
            {
                if ( value != null )
                {
                    this.SetValue( value.Guid );
                }
                else
                {
                    if ( this.Items.Count > 0 )
                    {
                        this.SelectedIndex = 0;
                    }
                }
            }
        }
    }
}