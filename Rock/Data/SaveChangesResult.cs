using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Result object for <see cref="DbContext.SaveChanges(SaveChangesArgs)"/>
    /// </summary>
    public sealed class SaveChangesResult
    {
        /// <summary>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are
        /// created for many-to-many relationships and relationships where there is no foreign
        /// key property included in the entity class (often referred to as independent associations).
        /// </summary>
        public int RecordsUpdated { get; set; }

        /// <summary>
        /// Achievement attempts that were affected by this SaveChanges call.
        /// </summary>
        public List<AchievementAttempt> AchievementAttempts { get; set; }
    }
}
