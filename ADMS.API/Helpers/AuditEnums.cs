namespace ADMS.API.Helpers
{
    /// <summary>
    /// Provides enumerations related to audit operations.
    /// </summary>
    public static class AuditEnums
    {
        /// <summary>
        /// Specifies the direction of an audit operation.
        /// </summary>
        public enum AuditDirection
        {
            /// <summary>
            /// Indicates the operation is moving or referencing from a source.
            /// </summary>
            From = 0,

            /// <summary>
            /// Indicates the operation is moving or referencing to a target.
            /// </summary>
            To = 1
        }

        /// <summary>
        /// Attempts to parse a string to an <see cref="AuditDirection"/> value in a case-insensitive manner.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="direction">When this method returns, contains the parsed <see cref="AuditDirection"/> value if parsing succeeded, or the default value if parsing failed.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryParseDirection(string value, out AuditDirection direction)
        {
            return Enum.TryParse(value, ignoreCase: true, out direction) &&
                   Enum.IsDefined(direction);
        }
    }
}