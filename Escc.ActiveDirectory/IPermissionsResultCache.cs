namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Caches the result of permissions queries
    /// </summary>
    public interface IPermissionsResultCache
    {
        /// <summary>
        /// Saves the result of a check for whether a user is in a set of security groups.
        /// </summary>
        /// <param name="groupsToMatch">The groups to match.</param>
        /// <param name="result"><c>true</c> if the user was in the group; <c>false</c> otherwise</param>
        void SaveGroupMatchResult(string[] groupsToMatch, bool result);

        /// <summary>
        /// Gets the result of a security group check if it's already been done.
        /// </summary>
        /// <param name="groupsToMatch">The groups to match.</param>
        /// <returns></returns>
        bool? CheckGroupMatchResult(string[] groupsToMatch);
    }
}