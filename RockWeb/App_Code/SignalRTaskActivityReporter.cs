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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.SignalR;
using Rock.Utility;
using Rock.Web.UI;

namespace RockWeb
{
    /// <summary>
    /// A task activity reporter that forwards notifications to a SignalR Message Hub.
    /// </summary>
    /// <remarks>
    /// Implements the System.IProgress interface for compatibility with the .NET Task-based Asynchronous Pattern (TAP).
    /// </remarks>
    public class SignalRTaskActivityReporter : IProgress<TaskProgressMessage>
    {
        private enum ReporterStatusSpecifier
        {
            Ready,
            Started,
            Stopped
        }

        private System.Timers.Timer _timer = null;
        private Stopwatch _stopwatch = new Stopwatch();
        private bool _notificationsAreDelayed = false;
        private ReporterStatusSpecifier _status = ReporterStatusSpecifier.Ready;
        private TaskProgressMessage _lastProgressMessage = null;
        private IHubContext<ITaskActivityMessageHub> _messageHub;
        private ConcurrentQueue<ITaskActivityMessage> _reportQueue;
        private object _timerLock = new object();
        private object _processQueueLock = new object();

        #region Properties

        /// <summary>
        /// The delay before the first notification will be sent, in millseconds.
        /// Set this value to avoid sending unnecessary notification messages for shorter tasks.
        /// </summary>
        public int StartNotificationDelayMilliseconds { get; set; }

        /// <summary>
        /// The minimum interval after a notification is sent before sending any subsequent notifications.
        /// </summary>
        public int NotificationIntervalMilliseconds { get; set; }

        /// <summary>
        /// A flag indicating if progress notifications should be sent at every notification interval.
        /// If set to False, progress notifications will only be sent when the completion percentage is updated.
        /// </summary>
        public bool NotifyElapsedTime { get; set; }

        /// <summary>
        /// The number of milliseconds that have elapsed since the task began.
        /// </summary>
        public long ElapsedMilliseconds
        {
            get
            {
                return _stopwatch.ElapsedMilliseconds;
            }
        }

        #endregion

        #region Constructors

        public SignalRTaskActivityReporter()
        {
            // Default to send the first notification after 5.0s and then every 1.0s thereafter.
            StartNotificationDelayMilliseconds = 5000;
            NotificationIntervalMilliseconds = 1000;
            NotifyElapsedTime = true;
        }

        #endregion

        #region Event Handlers

        private void _timer_Elapsed( object sender, System.Timers.ElapsedEventArgs e )
        {
            // Synchronise this timer event because it may be executed on multiple threads.
            if ( Monitor.TryEnter( _timerLock ) )
            {
                _timer.Stop();

                try
                {
                    // Add a time-based notification for this interval update.
                    if ( this.NotifyElapsedTime )
                    {
                        // If this is the first timer event, reconfigure the timer remove all previous progress updates from the queue.
                        if ( _notificationsAreDelayed )
                        {
                            _timer.Interval = NotificationIntervalMilliseconds;
                            _timer.AutoReset = true;

                            _notificationsAreDelayed = false;
                        }

                        TaskProgressMessage message;

                        if ( _lastProgressMessage == null )
                        {
                            message = TaskProgressMessage.New( 0, _stopwatch.ElapsedMilliseconds, "Running..." );
                        }
                        else
                        {
                            message = TaskProgressMessage.New( _lastProgressMessage.CompletionPercentage, _stopwatch.ElapsedMilliseconds, _lastProgressMessage.Message );
                        }

                        Report( message );
                    }

                    ProcessQueuedMessages();

                    if ( this.NotifyElapsedTime )
                    {
                        // Restart the timer to generate notifications at regular intervals.
                        _timer.Start();
                    }
                }
                finally
                {
                    Monitor.Exit( _timerLock );
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Report progress for the currently running task.
        /// </summary>
        /// <param name="completionPercentage"></param>
        /// <param name="activityMessage"></param>
        public void Report( decimal completionPercentage, string activityMessage = null )
        {
            var message = TaskProgressMessage.New( completionPercentage, _stopwatch.ElapsedMilliseconds, activityMessage );

            Report( message );
        }

        /// <summary>
        /// Report progress for the currently running task.
        /// </summary>
        /// <param name="currentCount"></param>
        /// <param name="totalCount"></param>
        /// <param name="activityMessage"></param>
        public void Report( int currentCount, int totalCount, string activityMessage = null )
        {
            var completionPercentage = decimal.Divide( currentCount, totalCount ) * 100;

            var message = TaskProgressMessage.New( completionPercentage, _stopwatch.ElapsedMilliseconds, activityMessage );

            Report( message );
        }


        /// <summary>
        /// Report progress for the currently running task.
        /// </summary>
        /// <param name="value"></param>
        public void Report( TaskProgressMessage value )
        {
            if ( _status == ReporterStatusSpecifier.Stopped )
            {
                // Ignore reports logged after the reporter has stopped.
                return;
            }
            else if ( _status == ReporterStatusSpecifier.Ready )
            {
                // If a report is received and the reporter is not already running, start it now.
                StartTask();
            }

            // Set the elapsed time.
            value.SetProgress( value.CompletionPercentage, _stopwatch.ElapsedMilliseconds, value.Message );

            QueueMessage( value );

            _lastProgressMessage = value;
        }

        /// <summary>
        /// Start the task.
        /// </summary>
        public void StartTask()
        {
            if ( _status == ReporterStatusSpecifier.Started )
            {
                return;
            }

            _status = ReporterStatusSpecifier.Started;

            // Initialize the timer to trigger time-based updates.
            if ( _timer != null )
            {
                _timer.Stop();

                _timer.Elapsed -= _timer_Elapsed;
            }

            _stopwatch.Restart();

            // Initialize the SignalR hub and notification message queue.
            _reportQueue = new ConcurrentQueue<ITaskActivityMessage>();

            _messageHub = GlobalHost.ConnectionManager.GetHubContext<TaskActivityMessageHub, ITaskActivityMessageHub>();

            var taskInfo = new TaskStatusSummary
            {
                IsStarted = true,
                StatusMessage = "Working...",
                ElapsedTime = 0
            };

            QueueMessage( taskInfo );

            // Initialize the timer for regulating progress notifications.
            // The timer fires once after the initial notification delay, and is then reconfigured for periodic notifications.
            _notificationsAreDelayed = this.StartNotificationDelayMilliseconds > 0;

            if ( this.NotifyElapsedTime || _notificationsAreDelayed )
            {
                _timer = new System.Timers.Timer();

                _timer.Interval = this.StartNotificationDelayMilliseconds;
                _timer.AutoReset = false;

                _timer.Elapsed += _timer_Elapsed;

                _timer.Start();
            }
        }

        private void QueueMessage( ITaskActivityMessage message )
        {
            // Add the message to the queue
            _reportQueue.Enqueue( message );

            ProcessQueuedMessages();
        }

        private long _nextNotificationMilliseconds = 0;

        private void ProcessQueuedMessages( bool forceProcessing = false, bool flushAllItems = false )
        {
            bool hasLock = false;

            if ( forceProcessing )
            {
                // Wait here until we can process the queue.
                Monitor.Enter( _processQueueLock, ref hasLock );
            }
            else
            {
                // If the minimum notification interval has not elapsed, do not process the queue.
                if ( _stopwatch.ElapsedMilliseconds < _nextNotificationMilliseconds )
                {
                    return;
                }

                // Try to lock the queue for processing, but exit if it is already being processed.
                Monitor.TryEnter( _processQueueLock, ref hasLock );
            }

            if ( !hasLock )
            {
                return;
            }

            try
            {
                // Send all of the queued messages.
                var messages = new List<ITaskActivityMessage>();

                // Retrieve items to process, up to a maximum of 100 items.
                bool getNextItem = true;
                int itemCount = 0;

                while ( getNextItem )
                {
                    ITaskActivityMessage message;

                    getNextItem = _reportQueue.TryDequeue( out message );

                    if ( getNextItem )
                    {
                        messages.Add( message );
                        itemCount++;

                        if ( !flushAllItems && itemCount >= 1000 )
                        {
                            getNextItem = false;
                        }
                    }
                }

                for ( int i = 0; i < messages.Count; i++ )
                {
                    ITaskActivityMessage thisMessage = messages[i];
                    ITaskActivityMessage nextMessage = null;

                    if ( thisMessage is TaskProgressMessage )
                    {
                        var progressMessage = (TaskProgressMessage)thisMessage;

                        // Only send this message if the next message in the queue is not also a progress message.
                        // This prevents the client from receiving a series of stale progress messages.
                        if ( i < messages.Count - 1 )
                        {
                            nextMessage = messages[i + 1];
                        }

                        if ( nextMessage != null && nextMessage is TaskProgressMessage )
                        {
                            continue;
                        }

                        // Set the elapsed time according to when the notification is sent, because this is more relevant to the client.
                        // This may be quite different from the actual task processing time if the queue is slow to process.
                        progressMessage.SetProgress( progressMessage.CompletionPercentage, _stopwatch.ElapsedMilliseconds, progressMessage.Message );

                        _messageHub.Clients.All.UpdateTaskProgress( (TaskProgressMessage)thisMessage );
                    }
                    else if ( thisMessage is TaskStatusSummary )
                    {
                        var infoMessage = (TaskStatusSummary)thisMessage;

                        if ( infoMessage.IsStarted && !infoMessage.IsFinished )
                        {
                            _messageHub.Clients.All.NotifyTaskStarted( infoMessage );
                        }
                        else
                        {
                            _messageHub.Clients.All.NotifyTaskComplete( infoMessage );
                        }
                    }
                }

                // Set the time at which the next notification can occur.
                _nextNotificationMilliseconds = _stopwatch.ElapsedMilliseconds + this.NotificationIntervalMilliseconds;
            }
            finally
            {
                Monitor.Exit( _processQueueLock );
            }
        }

        private void DestroyTimer()
        {
            lock ( _timerLock )
            {
                if ( _timer == null )
                {
                    return;
                }

                _timer.Stop();
                _timer.Elapsed -= _timer_Elapsed;
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Stop the currently running task and set the result.
        /// </summary>
        /// <param name="statusMessage"></param>
        /// <param name="hasErrors"></param>
        /// <param name="hasWarnings"></param>
        /// <param name="resultData"></param>
        public void StopTask( string statusMessage = null, bool hasErrors = false, bool hasWarnings = false, object resultData = null )
        {
            if ( _status != ReporterStatusSpecifier.Started )
            {
                return;
            }

            _status = ReporterStatusSpecifier.Stopped;

            _stopwatch.Stop();

            DestroyTimer();

            // Send a message to signal completion.
            if ( string.IsNullOrWhiteSpace( statusMessage ) )
            {
                statusMessage = "Task complete.";
            }

            var taskInfo = new TaskStatusSummary
            {
                IsStarted = true,
                IsFinished = true,
                HasErrors = hasErrors,
                HasWarnings = hasWarnings,
                StatusMessage = statusMessage,
                Data = resultData,
                ElapsedTime = _stopwatch.ElapsedMilliseconds
            };

            QueueMessage( taskInfo );

            // Flush the message queue.
            ProcessQueuedMessages( forceProcessing: true, flushAllItems: true );
        }

        #endregion

        /// <summary>
        /// An implementation of a progress reporter that is compatible can with the Task-based Asynchronous Pattern (TAP) library.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class SignalRProgressReporter : IProgress<TaskProgressMessage>
        {
            private readonly Action<TaskProgressMessage> _reportAction;

            public SignalRProgressReporter( Action<TaskProgressMessage> report )
            {
                _reportAction = report;
            }

            /// <summary>
            /// Report progress.
            /// </summary>
            /// <param name="value"></param>
            public void Report( TaskProgressMessage value )
            {
                if ( _reportAction == null )
                {
                    return;
                }

                _reportAction( value );
            }
        }
    }

    #region Helpers

    /// <summary>
    /// A helper class to provide functions for implementing a UI client to receive Task Activity notifications.
    /// </summary>
    public static class SignalRTaskActivityUiHelper
    {
        public enum ControlModeSpecifier
        {
            Hidden = 0,
            Progress = 1,
            Result = 2
        }

        /// <summary>
        /// Set the display mode for Task Activity controls.
        /// </summary>
        /// <param name="containerControl"></param>
        /// <param name="mode"></param>
        public static void SetTaskActivityControlMode( Control containerControl, ControlModeSpecifier mode )
        {
            var taskResultControls = FindControlRecursive( containerControl,
                x => ( x is WebControl ) && ( (WebControl)x ).CssClass.Contains( "js-global-task-result" ) );
            var resultDisplayValue = ( mode == ControlModeSpecifier.Result ? "block" : "none" );

            foreach ( WebControl c in taskResultControls )
            {
                c.Style.Add( "display", resultDisplayValue );
            }

            // Set visibility of controls marked with css classes: js-global-task-progress; js-global-task-progress-long-running
            var taskProgressControls = FindControlRecursive( containerControl,
                x => ( x is WebControl ) && ( (WebControl)x ).CssClass.Contains( "js-global-task-progress" ) );
            var progressDisplayValue = ( mode == ControlModeSpecifier.Progress ? "block" : "none" );

            foreach ( WebControl x in taskProgressControls )
            {
                x.Style.Add( "display", progressDisplayValue );
            }
        }

        /// <summary>
        /// Recursively find controls in a container that match a specified predicate.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static IEnumerable<Control> FindControlRecursive( Control parentControl, Func<Control, bool> predicate )
        {
            if ( predicate( parentControl ) )
            {
                yield return parentControl;
            }

            foreach ( Control child in parentControl.Controls )
            {
                foreach ( Control match in FindControlRecursive( child, predicate ) )
                {
                    yield return match;
                }
            }
        }

        /// <summary>
        /// Register a client-side script to handle task activity messages from the SignalRTaskActivityReporter.
        /// </summary>
        public static void AddActivityReporterScripts( RockPage page, string taskCompletedCallbackFunctionName = null )
        {
            // Add SignalR framework scripts.
            page.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", fingerprint: false );
            page.ClientScript.RegisterStartupScript( page.GetType(), "signalr", @"<script src=""/SignalR/hubs""></script>", false );

            // Add task activity notification script.
            string script = @"
<!-- SignalR Functions -->
$( function() {
    <!-- Create the SignalR proxy and message event handlers -->
    var proxy = $.connection.TaskActivityMessageHub;

    proxy.client.Reset = function () {
        <!--  Hide all task-related elements -->
        $( '.js-global-task-result' ).hide();
        $( '.js-global-task-progress' ).hide();
    };

    proxy.client.NotifyTaskStarted = function (taskInfo) {
        var log = $( ""[id$='_TaskActivityLog']"" );
        if ( taskInfo.HasActivityLog ) {
            log.show();
        }
        else {
            log.hide();
        }
        <!--  Set initial state of task-related elements -->
        $( '.js-global-task-result' ).hide();
        $( '.js-global-task-progress-long-running' ).hide();
        $( '.js-global-task-progress' ).show();
        <!--  Set elements associated with long-running tasks to appear after an initial delay of 5s. -->
        var barCtl = $( ""[id$='_TaskActivityBar']"" );
        if ( barCtl != null ) {
            setTimeout( function() {
                            barCtl.show();
                            $( '.js-global-task-progress-long-running' ).show();
                        }, 5000 );
        }
        proxy.client.UpdateTaskProgress({ Message: taskInfo.StatusMessage, CompletionPercentage: 0 });
    }

    proxy.client.NotifyTaskComplete = function (taskInfo) {
        var resultMessageCtl = $( ""[id$='_TaskActivityNotificationBox']"" );
        resultMessageCtl.html( taskInfo.StatusMessage );
        if ( taskInfo.HasErrors ) {
            resultMessageCtl.attr( 'class', 'alert alert-danger' );
        }
        else if ( taskInfo.HasWarnings ) {
            resultMessageCtl.attr( 'class', 'alert alert-warning' );
        }
        else {
            resultMessageCtl.attr( 'class', 'alert alert-success' );
        }

        <!--  Set final state of task-related elements -->
        $( '.js-global-task-progress' ).hide();
        $( '.js-global-task-progress-long-running' ).hide();
        $( '.js-global-task-result' ).show();

        <!-- Assign the callbackData object that will be used as the parameter for the callback function. -->
        var callbackData = taskInfo.Data;
        <taskCompletedCallbackFunction>
    }

    proxy.client.UpdateTaskProgress = function (taskActivity) {
        var messageCtl = $( ""[id$='_TaskActivityMessage']"" );
        if ( messageCtl != null ) {
            messageCtl.html( taskActivity.Message );
        }
        var elapsedTimeCtl = $( ""[id$='_TaskActivityTime']"" );
        if ( elapsedTimeCtl != null ) {
            elapsedTimeCtl.html( '[' + taskActivity.ElapsedTimeFormatted + ']' );
        }
        var barCtl = $( ""[id$='_TaskActivityBar']"" );
        if ( barCtl != null ) {
            var fillCtl = $( ""[id$='_TaskActivityBarFill']"" );
            fillCtl.css( 'width', taskActivity.CompletionPercentage + '%' );
            var percentCtl = $( ""[id$='_TaskActivityPercentage']"" );
            percentCtl.text( taskActivity.CompletionPercentage.toFixed( 1 ) + '% complete' );
        }
    }

    proxy.client.UpdateTaskLog = function (message) {
        var container = $( ""[id$='_messageContainer']"" );
        if ( !container.isVisible ) {
            return;
        }
        var maxBufferSize = 100;
        var messageCount = container.children().length;
        if ( messageCount >= maxBufferSize ) {
            container.children().slice( 0, messageCount - maxBufferSize + 1 ).remove();
        }
        container.append( message );
        var height = container[0].scrollHeight;
        container.scrollTop( height );
    }

    <!-- Hub disconnection handler -->
    $.connection.hub.disconnected(function () {
            $.connection.hub.start();
    });

    <!-- Start the SignalR hub proxy -->
    $.connection.hub.start();

    <!--  Set initial state of task-related elements -->
    $( '.js-global-task-result' ).hide();
    $( '.js-global-task-progress' ).hide();

    var progressMeterCtl = $( ""[id$='_TaskActivityBar']"" );
    if ( progressMeterCtl != null ) {
        progressMeterCtl.hide();
    }
})
";

            // Add an invocation of the specified callback function.
            var taskCompletedCallbackFunction = string.Empty;

            if ( !string.IsNullOrWhiteSpace( taskCompletedCallbackFunctionName ) )
            {
                taskCompletedCallbackFunction = string.Format( "{0}( callbackData );", taskCompletedCallbackFunctionName );
            }

            script = script.Replace( "<taskCompletedCallbackFunction>", taskCompletedCallbackFunction );

            page.ClientScript.RegisterStartupScript( page.GetType(), "task-activity", script, true );
        }
    }

    #endregion
}