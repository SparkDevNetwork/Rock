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

namespace Rock.Utility
{
    /// <summary>
    /// This class manages the Time Interval using in places like GroupSync model and IntervalPicker.
    /// </summary>
    public class TimeIntervalSetting
    {
        private const int MinutesPerDay = 1440;
        private const int MinutesPerHour = 60;

        private int _internalValue;
        private IntervalTimeUnit _intervalTimeUnit;

        /// <summary>
        /// Gets or sets the interval minutes.
        /// </summary>
        /// <value>
        /// The interval minutes.
        /// </value>
        public int IntervalValue { get => _internalValue; set { _internalValue = value; UpdateIntervalSettings(); } }

        /// <summary>
        /// Gets or sets the interval unit.
        /// </summary>
        /// <value>
        /// The interval unit.
        /// </value>
        public IntervalTimeUnit IntervalUnit { get => _intervalTimeUnit; set { _intervalTimeUnit = value; UpdateIntervalSettings(); } }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public int MaxValue { get; private set; }

        /// <summary>
        /// Gets the interval minutes.
        /// </summary>
        /// <value>
        /// The interval minutes.
        /// </value>
        public int? IntervalMinutes { get => GetPersistedScheduleIntervalMinutes(); }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeIntervalSetting"/> class.
        /// </summary>
        /// <param name="intervalMinutes">The interval minutes.</param>
        /// <param name="intervalUnit">The interval unit.</param>
        public TimeIntervalSetting( int? intervalMinutes, IntervalTimeUnit? intervalUnit ) : this( intervalMinutes, intervalUnit, 0 ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeIntervalSetting" /> class.
        /// </summary>
        /// <param name="intervalMinutes">The interval minutes.</param>
        /// <param name="intervalUnit">The interval unit.</param>
        /// <param name="defaultValue">The default value.</param>
        public TimeIntervalSetting( int? intervalMinutes, IntervalTimeUnit? intervalUnit, int? defaultValue )
        {
            UpdateIntervalSettings( intervalMinutes, intervalUnit, defaultValue );
        }

        private void UpdateIntervalSettings()
        {
            UpdateIntervalSettings( _internalValue, _intervalTimeUnit, null );
        }

        /// <summary>
        /// Updates the interval settings.
        /// </summary>
        /// <param name="intervalMinutes">The interval minutes.</param>
        /// <param name="intervalUnit">The interval unit.</param>
        /// <param name="defaultValue">The default value.</param>
        public void UpdateIntervalSettings( int? intervalMinutes, IntervalTimeUnit? intervalUnit, int? defaultValue )
        {
            // If persistence is enabled with no period or interval, set defaults.
            if ( intervalMinutes.GetValueOrDefault( 0 ) == 0 )
            {
                intervalUnit = intervalUnit.GetValueOrDefault( IntervalTimeUnit.Hour );
                intervalMinutes = defaultValue.GetValueOrDefault( 12 );
            }

            // If no schedule unit is selected, set the default.
            if ( intervalUnit == IntervalTimeUnit.None )
            {
                intervalUnit = IntervalTimeUnit.Hour;
            }

            // If no schedule unit is specified, determine the most appropriate unit based on the interval length.
            if ( intervalUnit == null )
            {
                var minutes = intervalMinutes.GetValueOrDefault( 0 );

                _internalValue = minutes;

                if ( minutes % MinutesPerDay == 0 )
                {
                    // Total minutes is a whole number of days.
                    intervalUnit = IntervalTimeUnit.Day;

                    _internalValue = minutes / MinutesPerDay;
                }
                else if ( minutes % MinutesPerHour == 0 )
                {
                    // Total minutes is a whole number of hours.
                    intervalUnit = IntervalTimeUnit.Hour;

                    _internalValue = minutes / MinutesPerHour;
                }
                else if ( minutes > MinutesPerDay )
                {
                    // Round to the nearest day.
                    intervalUnit = IntervalTimeUnit.Day;

                    _internalValue = minutes / MinutesPerDay;
                }
                else if ( minutes > MinutesPerHour )
                {
                    // Round to the nearest hour.
                    intervalUnit = IntervalTimeUnit.Hour;

                    _internalValue = minutes / MinutesPerHour;
                }
                else
                {
                    // Default to a measure of minutes.
                    intervalUnit = IntervalTimeUnit.Minute;
                }
            }

            _intervalTimeUnit = intervalUnit.GetValueOrDefault( IntervalTimeUnit.Hour );

            if ( _internalValue == 0 )
            {
                _internalValue = intervalMinutes ?? defaultValue.GetValueOrDefault( 0 );
            }

            if ( _intervalTimeUnit == IntervalTimeUnit.Day )
            {
                MaxValue = 31;
            }
            else if ( _intervalTimeUnit == IntervalTimeUnit.Minute )
            {
                MaxValue = 59;
            }
            else
            {
                MaxValue = 23;
            }

            if ( _internalValue > MaxValue )
            {
                _internalValue = MaxValue;
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if( _internalValue  == 0 || _intervalTimeUnit == IntervalTimeUnit.None )
            {
                return "";
            }

            return $"{_internalValue} {_intervalTimeUnit.ToString().ToLower().PluralizeIf(_internalValue > 1)}";
        }

        /// <summary>
        /// Calculates the persistence schedule interval for the current settings.
        /// </summary>
        /// <returns></returns>
        private int? GetPersistedScheduleIntervalMinutes()
        {
            if ( _intervalTimeUnit == IntervalTimeUnit.None )
            {
                return null;
            }

            var interval = _internalValue;

            if ( _intervalTimeUnit == IntervalTimeUnit.Day )
            {
                return interval * MinutesPerDay;
            }

            if ( _intervalTimeUnit == IntervalTimeUnit.Hour )
            {
                return interval * MinutesPerHour;
            }

            return interval;
        }
    }
}
