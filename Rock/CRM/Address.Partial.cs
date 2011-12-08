namespace Rock.CRM
{
    public partial class Address
    {
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} {1} {2}, {3} {4}", 
                this.Street1, this.Street2, this.City, this.State, this.Zip );
        }
    }
}
