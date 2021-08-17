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
using System.Collections.Generic;

namespace Rock.Data
{
    /// <summary>
    /// Implementation of a save hook that provides access to the strongly
    /// typed entity and RockContext.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <remarks>
    /// Note: Your save hook class must be in the same assembly as
    /// <typeparamref name="TEntity"/>.
    /// </remarks>
    /// <seealso cref="Rock.Data.IEntitySaveHook" />
    public abstract class EntitySaveHook<TEntity> : IEntitySaveHook
        where TEntity : IEntity
    {
        /// <summary>
        /// Gets the low level entry that identifies this save operation.
        /// </summary>
        /// <value>
        /// The low level entry that identifies this save operation.
        /// </value>
        protected IEntitySaveEntry Entry { get; private set; }

        /// <summary>
        /// Gets the entity being saved.
        /// </summary>
        /// <value>
        /// The entity being saved.
        /// </value>
        protected TEntity Entity => ( TEntity ) Entry.Entity;

        /// <summary>
        /// Gets the rock context the save operation is being performed in.
        /// This may be <c>null</c> if a custom database context is being used.
        /// </summary>
        /// <value>
        /// The rock context the save operation is being performed in.
        /// </value>
        protected RockContext RockContext => Entry.DataContext as RockContext;

        /// <summary>
        /// Gets the database context the save operation is being performed in.
        /// </summary>
        /// <value>
        /// The database context the save operation is being performed in.
        /// </value>
        protected DbContext DbContext => ( Data.DbContext ) Entry.DataContext;

        /// <summary>
        /// Gets the original values the entity was loaded with. Only valid
        /// if <see cref="PreSaveState"/> has the value <see cref="EntityContextState.Modified"/>.
        /// </summary>
        /// <value>
        /// The original values the entity was loaded with.
        /// </value>
        protected IReadOnlyDictionary<string, object> OriginalValues => Entry.OriginalValues;

        /// <summary>
        /// Gets the current state of the entity. Inside the PostSave methods
        /// this will probably not contain the value you expect as it would
        /// be updated to reflect the new state of the entity. Use the
        /// <see cref="PreSaveState"/> property in those cases.
        /// </summary>
        /// <value>
        /// The current state of the entity.
        /// </value>
        protected EntityContextState State => Entry.State;

        /// <summary>
        /// Gets the state of the entity just before the save operation started.
        /// Useful in PostSave methods to determine what type of operation was
        /// performed.
        /// </summary>
        /// <value>
        /// The state of the entity just before the save operation started.
        /// </value>
        protected EntityContextState PreSaveState => Entry.PreSaveState;

        /// <summary>
        /// Called before the save operation is executed.
        /// </summary>
        protected virtual void PreSave()
        {
        }

        /// <summary>
        /// Saves the failed.
        /// </summary>
        /// <remarks>
        /// This method is only called if <see cref="PreSave()"/> returns
        /// without error.
        /// </remarks>
        protected virtual void SaveFailed()
        {
        }

        /// <summary>
        /// Called after the save operation has been executed
        /// </summary>
        /// <remarks>
        /// This method is only called if <see cref="PreSave()"/> returns
        /// without error.
        /// </remarks>
        protected virtual void PostSave()
        {
        }

        /// <inheritdoc/>
        public void PreSave( IEntitySaveEntry entry )
        {
            Entry = entry;
            PreSave();
        }

        /// <inheritdoc/>
        public void SaveFailed( IEntitySaveEntry entry )
        {
            Entry = entry;
            SaveFailed();
        }

        /// <inheritdoc/>
        public void PostSave( IEntitySaveEntry entry )
        {
            Entry = entry;
            PostSave();
        }
    }
}
