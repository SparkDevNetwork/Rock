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
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Rock.Utility
{
    #region Hub

    /// <summary>
    /// A hub for sending/receiving SignalR messages relating to the progress and status of long-running tasks.
    /// </summary>
    [HubName( "TaskActivityMessageHub" )]
    public class TaskActivityMessageHub : Hub<ITaskActivityMessageHub>
    {
        /// <summary>
        /// Send a notification to add a message to the task log.
        /// </summary>
        /// <param name="message"></param>
        public void UpdateTaskLog( string message )
        {
            Clients.All.UpdateTaskLog( message );
        }

        /// <summary>
        /// Send a notification to update the progress of the task.
        /// </summary>
        /// <param name="message"></param>
        public void UpdateTaskProgress( TaskProgressMessage message )
        {
            Clients.All.UpdateTaskProgress( message );
        }

        /// <summary>
        /// Send a notification that the task has started.
        /// </summary>
        /// <param name="message"></param>
        public void NotifyTaskStarted( TaskStatusSummary message )
        {
            Clients.All.NotifyTaskStarted( message );
        }

        /// <summary>
        /// Send a notification that the task is completed.
        /// </summary>
        /// <param name="message"></param>
        public void NotifyTaskComplete( TaskStatusSummary message )
        {
            Clients.All.NotifyTaskComplete( message );
        }
    }

    /// <summary>
    /// A hub for sending/receiving SignalR messages relating to the progress and status of long-running tasks.
    /// </summary>
    public interface ITaskActivityMessageHub
    {
        /// <summary>
        /// Send a notification to add a message to the task log.
        /// </summary>
        /// <param name="message"></param>
        void UpdateTaskLog( string message );

        /// <summary>
        /// Send a notification to update the progress of the task.
        /// </summary>
        /// <param name="message"></param>
        void UpdateTaskProgress( TaskProgressMessage message );

        /// <summary>
        /// Send a notification that the task has started.
        /// </summary>
        /// <param name="message"></param>
        void NotifyTaskStarted( TaskStatusSummary message );

        /// <summary>
        /// Send a notification that the task is completed.
        /// </summary>
        /// <param name="message"></param>
        void NotifyTaskComplete( TaskStatusSummary message );
    }

    #endregion

    #region Messages

    /// <summary>
    /// Identifies a Task activity message of any type.
    /// </summary>
    public interface ITaskActivityMessage
    {
        /// <summary>
        /// A unique identifier for the task to which this activity relates.
        /// </summary>
        string TaskId { get; }

        /// <summary>
        /// The category of information contained in this message.
        /// </summary>
        string MessageType { get; }

        /// <summary>
        /// The message associated with this progress report.
        /// </summary>
        string SummaryText { get; }

        /// <summary>
        /// The elapsed time for the task when this message was generated, measured in milliseconds.
        /// </summary>
        long ElapsedTime { get; }
    }

    /// <summary>
    /// A message to indicate the progress of a running Task.
    /// </summary>
    public class TaskProgressMessage : ITaskActivityMessage
    {
        /// <summary>
        /// A unique identifier for the task to which this activity relates.
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// The category of information contained in this message.
        /// </summary>
        public string MessageType { get { return "Progress"; } }

        /// <summary>
        /// The message associated with this progress report.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Additional detail for the progress report.
        /// </summary>
        public string Detail { get; private set; }

        /// <summary>
        /// The measure of completion for this task, as a percentage
        /// </summary>
        public decimal CompletionPercentage { get; private set; }

        /// <summary>
        /// The total time elapsed for this task, measured in milliseconds.
        /// </summary>
        public long ElapsedTime { get; private set; }

        /// <summary>
        /// The total time elapsed for this task, in a human-readable time format.
        /// </summary>
        public string ElapsedTimeFormatted { get; private set; }

        /// <summary>
        /// Set the measure of progress for this task.
        /// </summary>
        /// <param name="completedCount"></param>
        /// <param name="totalCount"></param>
        /// <param name="elapsedMilliseconds"></param>
        /// <param name="message"></param>
        public void SetProgress( long completedCount, long totalCount, long elapsedMilliseconds = 0, string message = null )
        {
            if ( totalCount <= 0 )
            {
                CompletionPercentage = 0;
            }
            else
            {
                CompletionPercentage = decimal.Divide( completedCount, totalCount ) * 100;
            }

            this.Message = message;

            if ( elapsedMilliseconds != 0 )
            {
                ElapsedTime = elapsedMilliseconds;

                var timeInterval = new TimeSpan( 0, 0, 0, 0, (int)ElapsedTime );

                ElapsedTimeFormatted = string.Format( "{0:00}:{1:00}s", Math.Truncate( timeInterval.TotalMinutes ), timeInterval.Seconds );
            }
        }

        /// <summary>
        /// Set the measure of progress for this task.
        /// </summary>
        /// <param name="completionPercentage"></param>
        /// <param name="elapsedMilliseconds"></param>
        /// <param name="message"></param>
        public void SetProgress( decimal completionPercentage, long elapsedMilliseconds = 0, string message = null )
        {
            SetProgress( (long)( 100 * ( completionPercentage / 100 ) ), 100, elapsedMilliseconds, message );
        }

        /// <summary>
        /// Determines if this message describes equivalent progress to another message.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsEquivalent( TaskProgressMessage obj )
        {
            var report = obj as TaskProgressMessage;

            if ( report == null )
            {
                return false;
            }

            if ( CompletionPercentage == report.CompletionPercentage
                && Message == report.Message
                && Detail == report.Detail )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Create a new instance of the TaskProgressMessage.
        /// </summary>
        /// <param name="completionPercentage"></param>
        /// <param name="elapsedMilliseconds"></param>
        /// <param name="activityMessage"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TaskProgressMessage New( decimal completionPercentage, long elapsedMilliseconds = 0, string activityMessage = null, params object[] args )
        {
            var message = new TaskProgressMessage();

            if ( activityMessage != null
                 && args != null )
            {
                activityMessage = string.Format( activityMessage, args );
            }

            message.SetProgress( completionPercentage, elapsedMilliseconds, activityMessage );

            return message;
        }

        /// <summary>
        /// Create a new instance of the TaskProgressMessage.
        /// </summary>
        /// <param name="currentCount"></param>
        /// <param name="totalCount"></param>
        /// <param name="elapsedMilliseconds"></param>
        /// <param name="activityMessage"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TaskProgressMessage New( int currentCount, int totalCount, long elapsedMilliseconds = 0, string activityMessage = null, params object[] args )
        {
            var completionPercentage = decimal.Divide( currentCount, totalCount ) * 100;

            return New( completionPercentage, elapsedMilliseconds, activityMessage, args );
        }

        #region ITaskActivityMessage implementation
        string ITaskActivityMessage.SummaryText
        {
            get
            {
                return Message;
            }
        }

        #endregion
    }

    /// <summary>
    /// A summary of the current status of a Task.
    /// </summary>
    public class TaskStatusSummary : ITaskActivityMessage
    {
        /// <summary>
        /// A unique identifier for the task to which this activity relates.
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// The general nature of the information contained in this message.
        /// </summary>
        public string MessageType { get { return "Info"; } }

        /// <summary>
        /// The name of the task.
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// The description of the task.
        /// </summary>
        public string TaskDescription { get; set; }

        /// <summary>
        /// The path to the log file associated with this task.
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// A flag indicating if this task is logging activity.
        /// </summary>
        public bool HasActivityLog { get; set; }

        /// <summary>
        /// A flag indicating if this task has started.
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// A flag indicating if this task is currently running.
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// A flag indicating if this task is finished.
        /// </summary>
        public bool IsFinished { get; set; }

        /// <summary>
        /// A message indicating the current state of this task.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// A flag indicating if this task has encountered any errors.
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        /// A flag indicating if this task has encountered any warning conditions.
        /// </summary>
        public bool HasWarnings { get; set; }

        /// <summary>
        /// Additional custom data related to the status of this task.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The total time elapsed for this task, measured in milliseconds.
        /// </summary>
        public long ElapsedTime { get; set; }

        string ITaskActivityMessage.SummaryText
        {
            get
            {
                var message = string.IsNullOrWhiteSpace( TaskName ) ? "Task" : TaskName;

                if ( IsFinished )
                {
                    message += " Finished";
                }
                else if ( IsRunning )
                {
                    message += " Running";
                }
                else
                {
                    message += " Started";
                }

                if ( !string.IsNullOrWhiteSpace( StatusMessage ) )
                {
                    message += ": " + StatusMessage;
                }

                return message;
            }
        }

        #endregion
    }
}