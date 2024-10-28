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
using System.Linq;
using System.Threading;

using Rock.RealTime.Topics;
using Rock.ViewModels.Utility;

namespace Rock.Utility
{
    /// <summary>
    /// Provides task activity progress updates to clients.
    /// </summary>
    public class TaskActivityProgress : IDisposable
    {
        #region Fields

        /// <summary>
        /// The target that will receive the progress updates.
        /// </summary>
        private readonly ITaskActivityProgress _target;

        /// <summary>
        /// The timer that will provide periodic update callbacks.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// The queue that will contain any messages that need to be sent.
        /// </summary>
        private ConcurrentQueue<ITaskActivityProgressMessage> _messageQueue;

        /// <summary>
        /// The stopwatch that tracks how long the activity has been running.
        /// </summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// The internal status of this activity.
        /// </summary>
        private ActivityStatus _status = ActivityStatus.Waiting;

        /// <summary>
        /// <c>true</c> if initial notifications should be delayed until
        /// <see cref="StartNotificationDelayMilliseconds"/> has elapsed.
        /// </summary>
        private bool _notificationsAreDelayed;

        /// <summary>
        /// The last progress activity message that was sent.
        /// </summary>
        private string _lastProgressMessage;

        /// <summary>
        /// The last progress activity completed percentage that was sent.
        /// </summary>
        private decimal? _lastProgressPercentage;

        /// <summary>
        /// Ensures the queue is only processed by one thread at a time.
        /// </summary>
        private readonly object _processQueueLock = new object();

        /// <summary>
        /// The number of elapsed milliseconds in <see cref="_stopwatch"/> that must be
        /// achieved before the next update is sent.
        /// </summary>
        private long _nextNotificationMilliseconds = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the identifier for this task activity.
        /// </summary>
        /// <value>The identifier for this task activity.</value>
        public string TaskId { get; }

        /// <summary>
        /// Gets the friendly name of the task.
        /// </summary>
        /// <value>The friendly name of the task.</value>
        public string TaskName { get; }

        /// <summary>
        /// The delay before the first notification will be sent, in millseconds.
        /// Set this value to avoid sending unnecessary notification messages for shorter tasks.
        /// </summary>
        public int StartNotificationDelayMilliseconds { get; set; } = 5000;

        /// <summary>
        /// The minimum interval after a notification is sent before sending any subsequent notifications.
        /// </summary>
        public int NotificationIntervalMilliseconds { get; set; } = 1000;

        /// <summary>
        /// A flag indicating if progress notifications should be sent at every notification interval.
        /// If set to False, progress notifications will only be sent when the completion percentage is updated.
        /// </summary>
        public bool PeriodicNotification { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskActivityProgress"/> class.
        /// </summary>
        /// <param name="target">The target that will receive the progress updates.</param>
        public TaskActivityProgress( ITaskActivityProgress target )
            : this( target, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskActivityProgress"/> class.
        /// </summary>
        /// <param name="target">The target that will receive the progress updates.</param>
        /// <param name="taskName">The user friendly name of the task being performed.</param>
        public TaskActivityProgress( ITaskActivityProgress target, string taskName )
            : this( target, taskName, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskActivityProgress"/> class.
        /// </summary>
        /// <param name="target">The target that will receive the progress updates.</param>
        /// <param name="taskName">The user friendly name of the task being performed.</param>
        /// <param name="taskId">If not <c>null</c> or empty then this value will be used as the task identifier.</param>
        public TaskActivityProgress( ITaskActivityProgress target, string taskName, string taskId )
        {
            _target = target;
            TaskId = taskId.IsNotNullOrWhiteSpace() ? taskId : Guid.NewGuid().ToString();
            TaskName = taskName ?? "Task";
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DestroyTimer();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Report progress for the currently running task.
        /// </summary>
        /// <param name="currentCount">The number of items that have been processed.</param>
        /// <param name="totalCount">The total number of items that will be processed by the task.</param>
        /// <param name="activityMessage">A message to include with the progress update.</param>
        public void ReportProgressUpdate( long currentCount, long totalCount, string activityMessage = null )
        {
            ReportProgressUpdate( currentCount / ( decimal ) totalCount * 100, activityMessage );
        }

        /// <summary>
        /// Report progress for the currently running task.
        /// </summary>
        /// <param name="completionPercentage">A value for the completion percentage between 0 and 100.</param>
        /// <param name="activityMessage">A message to include with the progress update.</param>
        public void ReportProgressUpdate( decimal completionPercentage, string activityMessage = null )
        {
            var message = new TaskActivityProgressUpdateBag
            {
                TaskId = TaskId,
                CompletionPercentage = completionPercentage,
                ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
                Message = activityMessage
            };

            ReportProgressUpdate( message );
        }

        /// <summary>
        /// Report progress for the currently running task.
        /// </summary>
        /// <param name="value"></param>
        private void ReportProgressUpdate( TaskActivityProgressUpdateBag value )
        {
            if ( _status == ActivityStatus.Stopped )
            {
                // Ignore reports logged after the reporter has stopped.
                return;
            }
            else if ( _status == ActivityStatus.Waiting )
            {
                // If a report is received and the reporter is not already running, start it now.
                StartTask();
            }

            EnqueueMessage( value );

            _lastProgressMessage = value.Message;
            _lastProgressPercentage = value.CompletionPercentage;
        }

        /// <summary>
        /// Report progress for the currently running task.
        /// </summary>
        /// <param name="message">The message to be logged in the verbose log viewer.</param>
        public void LogMessage( string message = null )
        {
            var logMessage = new TaskActivityProgressLogBag
            {
                TaskId = TaskId,
                Message = message
            };

            EnqueueMessage( logMessage );
        }

        /// <summary>
        /// Start the task.
        /// </summary>
        public void StartTask( string statusMessage = null )
        {
            if ( _status != ActivityStatus.Waiting )
            {
                return;
            }

            _status = ActivityStatus.Started;
            _stopwatch.Start();

            // Initialize the SignalR hub and notification message queue.
            _messageQueue = new ConcurrentQueue<ITaskActivityProgressMessage>();

            var taskInfo = new TaskActivityProgressStatusBag
            {
                TaskId = TaskId,
                IsStarted = true,
                Message = statusMessage ?? "Working...",
                ElapsedMilliseconds = 0
            };

            EnqueueMessage( taskInfo );

            // Initialize the timer for regulating progress notifications.
            // The timer fires once after the initial notification delay, and is then reconfigured for periodic notifications.
            _notificationsAreDelayed = StartNotificationDelayMilliseconds > 0;

            if ( PeriodicNotification || _notificationsAreDelayed )
            {
                _timer = new Timer( TimerElapsed );
                _timer.Change( StartNotificationDelayMilliseconds, PeriodicNotification ? NotificationIntervalMilliseconds : Timeout.Infinite );
            }
        }

        /// <summary>
        /// Enqueues the message and attempts to process the queue if it is time.
        /// </summary>
        /// <param name="message">The message to be enqueued.</param>
        private void EnqueueMessage( ITaskActivityProgressMessage message )
        {
            // Add the message to the queue
            _messageQueue.Enqueue( message );

            ProcessQueuedMessages();
        }

        /// <summary>
        /// Processes the queued messages for sending.
        /// </summary>
        /// <param name="forceProcessing">if set to <c>true</c> then processing will happen even if it isn't time yet.</param>
        private void ProcessQueuedMessages( bool forceProcessing = false )
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
                var messages = new List<ITaskActivityProgressMessage>( 10 );

                // Retrieve all queued items so we can process them.
                while ( _messageQueue.TryDequeue( out var message ) )
                {
                    messages.Add( message );
                }

                for ( int i = 0; i < messages.Count; i++ )
                {
                    var thisMessage = messages[i];

                    if ( thisMessage is TaskActivityProgressUpdateBag updateBag )
                    {
                        // Only send this message if the next message in the queue is not also a progress message.
                        // This prevents the client from receiving a series of stale progress messages.
                        if ( i < messages.Count - 1 )
                        {
                            if ( messages[i + 1] is TaskActivityProgressUpdateBag )
                            {
                                continue;
                            }
                        }

                        // Set the elapsed time according to when the notification is sent, because this is more relevant to the client.
                        // This may be quite different from the actual task processing time if the queue is slow to process.
                        updateBag.ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

                        _target.UpdateTaskProgress( updateBag );
                    }
                    else if ( thisMessage is TaskActivityProgressStatusBag statusBag )
                    {
                        if ( statusBag.IsStarted && !statusBag.IsFinished )
                        {
                            _target.TaskStarted( statusBag );
                        }
                        else
                        {
                            _target.TaskCompleted( statusBag );
                        }
                    }
                    else if ( thisMessage is TaskActivityProgressLogBag logBag )
                    {
                        _target.UpdateTaskLog( logBag );
                    }
                }

                // Set the time at which the next notification can occur.
                _nextNotificationMilliseconds = _stopwatch.ElapsedMilliseconds + NotificationIntervalMilliseconds;
            }
            finally
            {
                Monitor.Exit( _processQueueLock );
            }
        }

        /// <summary>
        /// Destroys the timer so it is invalid and no longer fires.
        /// </summary>
        private void DestroyTimer()
        {
            if ( _timer == null )
            {
                return;
            }

            _timer.Dispose();
            _timer = null;
        }

        /// <summary>
        /// Stop the currently running task and set the result.
        /// </summary>
        /// <param name="statusMessage"></param>
        /// <param name="errors"></param>
        /// <param name="warnings"></param>
        /// <param name="resultData"></param>
        public void StopTask( string statusMessage = null, IEnumerable<string> errors = null, IEnumerable<string> warnings = null, object resultData = null )
        {
            if ( _status != ActivityStatus.Started )
            {
                return;
            }

            _status = ActivityStatus.Stopped;

            _stopwatch.Stop();

            DestroyTimer();

            // Send a message to signal completion.
            if ( string.IsNullOrWhiteSpace( statusMessage ) )
            {
                statusMessage = "Task complete.";
            }

            var taskInfo = new TaskActivityProgressStatusBag
            {
                TaskId = TaskId,
                TaskName = TaskName,
                IsStarted = true,
                IsFinished = true,
                Errors = errors?.ToList(),
                Warnings = warnings?.ToList(),
                Message = statusMessage,
                Data = resultData,
                ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds
            };

            EnqueueMessage( taskInfo );

            // Flush the message queue.
            ProcessQueuedMessages( forceProcessing: true );
        }

        /// <summary>
        /// Called when the timer period has elapsed. 
        /// </summary>
        /// <param name="_state">The timer state object, not used.</param>
        private void TimerElapsed( object _state )
        {
            // Add a time-based notification for this interval update.
            if ( PeriodicNotification && _status == ActivityStatus.Started )
            {
                // If this is the first timer event, reconfigure the
                // timer remove all previous progress updates from
                // the queue.
                if ( _notificationsAreDelayed )
                {
                    _notificationsAreDelayed = false;
                    while ( _messageQueue.TryDequeue( out _ ) )
                        ;
                }

                var message = new TaskActivityProgressUpdateBag
                {
                    TaskId = TaskId,
                    CompletionPercentage = _lastProgressPercentage ?? 0,
                    ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
                    Message = _lastProgressMessage ?? "Running..."
                };

                ReportProgressUpdate( message );
            }

            ProcessQueuedMessages();
        }

        #endregion

        private enum ActivityStatus
        {
            Waiting,
            Started,
            Stopped
        }
    }
}