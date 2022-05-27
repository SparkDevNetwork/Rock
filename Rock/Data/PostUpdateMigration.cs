using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Data
{
    /// <summary>
    /// Class PostUpdateMigration.
    /// </summary>
    internal abstract class PostUpdateMigration
    {
        public int CommandTimeoutSeconds { get; set; }

        public virtual bool RunAtStartup => true;

        //public abstract string RockVersion { get; }

        private static HashSet<PostUpdateMigration> _currentlyRunning = new HashSet<PostUpdateMigration>();

        public bool IsRunning => _currentlyRunning.Contains( this );

        public Task<PostUpdateMigration> StartUpdate()
        {
            if ( IsRunning )
            {
                return Task.FromResult( this );
            }

            _currentlyRunning.Add( this );

            var startedTask = new Task<PostUpdateMigration>( () =>
            {
                Update();
                return this;
            } );

            startedTask.ContinueWith( ( t ) =>
            {
                _currentlyRunning.Remove( this );
            } );

            startedTask.Start();

            return startedTask;
        }

        /// <summary>
        /// Determines whether this instance is complete.
        /// </summary>
        /// <returns><c>true</c> if this instance is complete; otherwise, <c>false</c>.</returns>
        public abstract bool IsComplete();

        /// <summary>
        /// Musts the complete before updating.
        /// </summary>
        /// <param name="currentRockVersion">The current rock version.</param>
        /// <param name="nextRockVersion">The next rock version.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public abstract bool MustCompleteBeforeUpdating( FileVersionInfo currentRockVersion, FileVersionInfo nextRockVersion );

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public abstract void Update();
    }
}
