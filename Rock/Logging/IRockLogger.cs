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

using System;

namespace Rock.Logging
{
    /// <summary>
    /// The interface that RockLogger uses to log.
    /// </summary>
    public interface IRockLogger
    {
        /// <summary>
        /// Gets the log configuration.
        /// </summary>
        /// <value>
        /// The log configuration.
        /// </value>
        IRockLogConfiguration LogConfiguration { get; }

        /// <summary>
        /// Closes this instance and releases file locks.
        /// </summary>
        void Close();
        /// <summary>
        /// Debugs the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Debug( Exception exception, string messageTemplate );
        /// <summary>
        /// Debugs the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Debug( Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Debugs the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        void Debug( string messageTemplate );
        /// <summary>
        /// Debugs the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Debug( string domain, Exception exception, string messageTemplate );
        /// <summary>
        /// Debugs the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Debug( string domain, Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Debugs the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Debug( string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Debugs the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Debug( string domain, string messageTemplate );
        /// <summary>
        /// Debugs the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Debug( string domain, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Errors the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Error( Exception exception, string messageTemplate );
        /// <summary>
        /// Errors the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Error( Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Errors the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        void Error( string messageTemplate );
        /// <summary>
        /// Errors the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Error( string domain, Exception exception, string messageTemplate );
        /// <summary>
        /// Errors the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Error( string domain, Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Errors the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Error( string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Errors the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Error( string domain, string messageTemplate );
        /// <summary>
        /// Errors the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Error( string domain, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Fatals the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Fatal( Exception exception, string messageTemplate );
        /// <summary>
        /// Fatals the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Fatal( Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Fatals the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        void Fatal( string messageTemplate );
        /// <summary>
        /// Fatals the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Fatal( string domain, Exception exception, string messageTemplate );
        /// <summary>
        /// Fatals the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Fatal( string domain, Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Fatals the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Fatal( string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Fatals the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Fatal( string domain, string messageTemplate );
        /// <summary>
        /// Fatals the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Fatal( string domain, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Informations the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Information( Exception exception, string messageTemplate );
        /// <summary>
        /// Informations the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Information( Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Informations the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        void Information( string messageTemplate );
        /// <summary>
        /// Informations the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Information( string domain, Exception exception, string messageTemplate );
        /// <summary>
        /// Informations the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Information( string domain, Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Informations the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Information( string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Informations the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Information( string domain, string messageTemplate );
        /// <summary>
        /// Informations the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Information( string domain, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Verboses the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Verbose( Exception exception, string messageTemplate );
        /// <summary>
        /// Verboses the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Verbose( Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Verboses the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        void Verbose( string messageTemplate );
        /// <summary>
        /// Verboses the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Verbose( string domain, Exception exception, string messageTemplate );
        /// <summary>
        /// Verboses the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Verbose( string domain, Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Verboses the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Verbose( string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Verboses the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Verbose( string domain, string messageTemplate );
        /// <summary>
        /// Verboses the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Verbose( string domain, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Warnings the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Warning( Exception exception, string messageTemplate );
        /// <summary>
        /// Warnings the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Warning( Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Warnings the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        void Warning( string messageTemplate );
        /// <summary>
        /// Warnings the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Warning( string domain, Exception exception, string messageTemplate );
        /// <summary>
        /// Warnings the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Warning( string domain, Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Warnings the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Warning( string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Warnings the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        void Warning( string domain, string messageTemplate );
        /// <summary>
        /// Warnings the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void Warning( string domain, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        void WriteToLog( RockLogLevel logLevel, Exception exception, string messageTemplate );
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void WriteToLog( RockLogLevel logLevel, Exception exception, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        void WriteToLog( RockLogLevel logLevel, Exception exception, string domain, string messageTemplate );
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void WriteToLog( RockLogLevel logLevel, Exception exception, string domain, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="messageTemplate">The message template.</param>
        void WriteToLog( RockLogLevel logLevel, string messageTemplate );
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void WriteToLog( RockLogLevel logLevel, string messageTemplate, params object[] propertyValues );
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        void WriteToLog( RockLogLevel logLevel, string domain, string messageTemplate );
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        void WriteToLog( RockLogLevel logLevel, string domain, string messageTemplate, params object[] propertyValues );
    }
}