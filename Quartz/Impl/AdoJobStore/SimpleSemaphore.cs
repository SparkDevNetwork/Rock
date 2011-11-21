#region License
/* 
 * All content copyright Terracotta, Inc., unless otherwise indicated. All rights reserved. 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not 
 * use this file except in compliance with the License. You may obtain a copy 
 * of the License at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0 
 *   
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations 
 * under the License.
 * 
 */
#endregion

using System;
using System.Globalization;
using System.Threading;

using Quartz.Collection;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Util;


namespace Quartz.Impl.AdoJobStore
{
	/// <summary> 
	/// Internal in-memory lock handler for providing thread/resource locking in 
    /// order to protect resources from being altered by multiple threads at the 
    /// same time.
	/// </summary>
	/// <author>James House</author>
	/// <author>Marko Lahma (.NET)</author>
	public class SimpleSemaphore : ISemaphore
	{
	    private const string KeyThreadLockOwners = "qrtz_ssemaphore_lock_owners";

		private readonly HashSet<string> locks = new HashSet<string>();

	    public SimpleSemaphore()
	    {
	        
	    }


	    /// <summary>
        /// Gets the thread locks.
        /// </summary>
        /// <value>The thread locks.</value>
		private static HashSet<string> ThreadLocks
		{
			get
			{
				HashSet<string> threadLocks = LogicalThreadContext.GetData<HashSet<string>>(KeyThreadLockOwners);
				if (threadLocks == null)
				{
					threadLocks = new HashSet<string>();
					LogicalThreadContext.SetData(KeyThreadLockOwners, threadLocks);
				}
				return threadLocks;
			}
		}

		/// <summary> 
		/// Grants a lock on the identified resource to the calling thread (blocking
		/// until it is available).
		/// </summary>
		/// <returns>True if the lock was obtained.</returns>
		public virtual bool ObtainLock(DbMetadata metadata, ConnectionAndTransactionHolder conn, string lockName)
		{
			lock (this)
			{
				lockName = String.Intern(lockName);

				if (!IsLockOwner(conn, lockName))
				{
					while (locks.Contains(lockName))
					{
						try
						{
							Monitor.Wait(this);
						}
						catch (ThreadInterruptedException)
						{
							
						}
					}

					ThreadLocks.Add(lockName);
					locks.Add(lockName);
				}

				return true;
			}
		}

		/// <summary> Release the lock on the identified resource if it is held by the calling
		/// thread.
		/// </summary>
		public virtual void ReleaseLock(ConnectionAndTransactionHolder conn, string lockName)
		{
			lock (this)
			{
				lockName = String.Intern(lockName);

				if (IsLockOwner(conn, lockName))
				{
					ThreadLocks.Remove(lockName);
					locks.Remove(lockName);
					Monitor.PulseAll(this);
				}
			}
		}

		/// <summary> 
		/// Determine whether the calling thread owns a lock on the identified
		/// resource.
		/// </summary>
		public virtual bool IsLockOwner(ConnectionAndTransactionHolder conn, string lockName)
		{
			lock (this)
			{
				lockName = String.Intern(lockName);

				return ThreadLocks.Contains(lockName);
			}
		}

        /// <summary>
        /// Whether this Semaphore implementation requires a database connection for
        /// its lock management operations.
        /// </summary>
        /// <value></value>
        /// <seealso cref="IsLockOwner"/>
        /// <seealso cref="ObtainLock"/>
        /// <seealso cref="ReleaseLock"/>
	    public bool RequiresConnection
	    {
            get { return false; }
	    }
	}
}