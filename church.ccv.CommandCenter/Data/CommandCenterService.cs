using Rock.Data;

namespace church.ccv.CommandCenter.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandCenterService<T> : Rock.Data.Service<T> where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCenterService{T}"/> class.
        /// </summary>
        public CommandCenterService( CommandCenterContext context )
            : base( context )
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public virtual bool CanDelete( T item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

    }
}
