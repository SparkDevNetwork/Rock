namespace org.lakepointe.Checkin.SystemGuid
{
    /// <summary>
    /// Custom Group Role System Guids for LPC Check-in plugin
    /// </summary>
    public static class GroupRole
    {
        #region Known Relationships

        /// <summary>
        /// Guid for the Can Temporarily Check-in known relationship group role.
        /// A person who can temporarily be checked in by the owner of this known relationship role (i.e. Ted Decker can check in Brian Jones.
        /// Brian would be added to Ted's known relationships group with a role of 'Can temporarily checkin'.)
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_CAN_TEMPORARILY_CHECK_IN = "EFAEE6AE-6889-43D8-84F2-25154AACEF69";


        /// <summary>
        /// Guid for the Allow Temporary Check-in By known relationship group role.
        /// The person who can check-in the  the owner of this known relationship role (i.e.Brian Jones can be checked in by Ted Decker
        /// Ted would be added to Brian's known relationships group with a role of 'Allow Temporary Check-in By'.)
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_ALLOW_TEMPORARY_CHECK_IN_BY = "A7942CD0-E2BF-40AF-9127-0B3C21FBC7DF";
        #endregion

    }
}
