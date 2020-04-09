using System.Data.Entity;
using System.Linq;
using Rock.Data;

namespace com.bemaservices.DoorControl.DSX.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DoorLockService: Service<DoorLock>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoorLockService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public DoorLockService( RockContext context ) : base( context ) { }


        #region Public Methods
        /// <summary>
        /// Checks to see if a particular DoorLock Object exists in the DB
        /// </summary>
        /// <param name="doorLock">The door lock.</param>
        /// <returns></returns>
        public bool Exists( DoorLock doorLock )
        {
            RockContext rockContext = new RockContext();
            DoorLockService doorLockService = new DoorLockService( rockContext );

            var result = doorLockService.Queryable().AsNoTracking().Where( x => x.StartDateTime == doorLock.StartDateTime &&
                                                                                x.EndDateTime == doorLock.EndDateTime &&
                                                                                x.StartAction == doorLock.StartAction &&
                                                                                x.EndAction == doorLock.EndAction &&
                                                                                x.ReservationId == doorLock.ReservationId &&
                                                                                x.LocationId == doorLock.LocationId &&
                                                                                x.OverrideGroup == doorLock.OverrideGroup &&
                                                                                x.RoomName == doorLock.RoomName
                                                                            ).Any();

            return result;
        }

        #endregion Public Methods
    }
}
