﻿// <copyright>
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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public sealed class RockDataViewFilterExpressionException : RockReportingException
    {
        private DataViewFilter _dataViewFilter;
        private readonly string _message;

        /// <summary>
        /// Sets the data filter, unless it has been set already
        /// Use this if this DataFilter wasn't known at the time the exception was raised.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        public void SetDataFilterIfNotSet( DataViewFilter dataViewFilter )
        {
            if ( _dataViewFilter != null )
            {
                return;
            }

            try
            {
                if ( dataViewFilter?.DataViewId != null )
                {
                    DataView = dataViewFilter.DataView;
                }
            }
            catch
            {
                // if we weren't able to lazy-load DataView (or some other problem), just ignore
                DataView = null;
            }

            _dataViewFilter = dataViewFilter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDataViewFilterExpressionException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RockDataViewFilterExpressionException( string message )
            : this( ( DataViewFilter ) null, message )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDataViewFilterExpressionException"/> class.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RockDataViewFilterExpressionException( DataViewFilter dataViewFilter, string message, Exception innerException )
            : base( message, innerException )
        {
            SetDataFilterIfNotSet( dataViewFilter );
            _message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDataViewFilterExpressionException" /> class.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        /// <param name="message">The message.</param>
        public RockDataViewFilterExpressionException( DataViewFilter dataViewFilter, string message )
            : this( dataViewFilter, message, null )
        {
        }

        /// <summary>
        /// Gets the DataView that produced the error
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        public DataView DataView { get; private set; }

        /// <summary>
        /// Gets the friendly message for the caller DataView.
        /// </summary>
        /// <param name="callerDataView">The caller data view.</param>
        /// <returns></returns>
        /// <value>
        /// The friendly message.
        /// </value>
        public string GetFriendlyMessage( DataView callerDataView )
        {
            if ( _dataViewFilter != null && _dataViewFilter.EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( _dataViewFilter.EntityTypeId.Value );
                if ( DataView != null && callerDataView != null && callerDataView?.Id != DataView?.Id )
                {
                    // if the DataView that generated this error isn't the same as the DataView that was run, it must have been from an "OtherDataView" filter
                    return $"An error has occurred within related DataView: {DataView}";
                }

                if ( entityType != null )
                {
                    var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                    if ( component != null )
                    {
                        var filterDescription = component.FormatSelection( entityType.GetEntityType(), _dataViewFilter.Selection );
                        if ( filterDescription.IsNotNullOrWhiteSpace() )
                        {
                            return $"An error has occurred with the data filter: {filterDescription}";
                        }
                    }
                }
            }

            return "An error has occurred in a data filter";
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                if ( _dataViewFilter != null )
                {
                    return $"{_message} [DataViewFilterId: {_dataViewFilter?.Id}, DataViewId: {DataView?.Id}]";
                }

                return _message;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Reporting.RockReportingException" />
    internal class RockReportException : RockReportingException
    {
        private readonly Report _report;
        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockReportException"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="message">The message.</param>
        public RockReportException( Report report, string message ) : base( message )
        {
            _report = report;
            _message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockReportException"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RockReportException( Report report, string message, Exception innerException ) : base( message, innerException )
        {
            _report = report;
            _message = message;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return $"{_message} [ReportId: {_report?.Id}]";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    internal class RockReportFieldExpressionException : RockReportingException
    {
        private readonly Report _report;
        private readonly ReportField _reportField;
        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockReportFieldExpressionException"/> class.
        /// </summary>
        /// <param name="reportField">The report field.</param>
        /// <param name="message">The message.</param>
        public RockReportFieldExpressionException( ReportField reportField, string message ) : base( message )
        {
            _report = _reportField?.Report;
            _reportField = reportField;
            _message = message;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return $"{_message} [ReportFieldId: {_reportField?.Id}, ReportId: {_report?.Id}]";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public abstract class RockReportingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockReportingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RockReportingException( string message ) :
                base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockReportingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public RockReportingException( string message, Exception innerException ) :
                base( message, innerException )
        {
        }
    }
}
