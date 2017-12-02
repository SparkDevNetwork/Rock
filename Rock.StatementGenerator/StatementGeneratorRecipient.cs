namespace Rock.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.StatementGenerator.StatementGeneratorRecipient" />
    public class StatementGeneratorRecipientResult : StatementGeneratorRecipient
    {
        public string Html { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StatementGeneratorRecipient
    {
        /// <summary>
        /// Gets or sets the GroupId of the Family to use as the Address.
        /// if PersonId is null, this is also the GivingGroupId to use when fetching transactions
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the person identifier for people that give as Individuals. If this is null, get the Transactions based on the GivingGroupId
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"GroupId:{GroupId}, PersonId:{PersonId}";
        }
    }
}
