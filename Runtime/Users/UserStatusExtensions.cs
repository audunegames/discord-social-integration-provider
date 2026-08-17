using Discord.Sdk;

namespace Audune.Social.Discord
{
  /// <summary>
  /// Class that defines extension methods for user statuses.
  /// </summary>
  internal static class UserStatusExtensions
  {
    /// <summary>
    /// Returns the user status for the specified <c>StatusType</c>.
    /// </summary>
    /// <param name="statusType">The <c>StatusType</c> for which to return the user status.</param>
    /// <returns>The user status for the specified <c>StatusType</c>.</returns>
    public static UserStatus ToUserStatus(this StatusType statusType)
    {
      return statusType switch {
        StatusType.Online => UserStatus.Online,
        StatusType.Streaming => UserStatus.Online,
        StatusType.Idle => UserStatus.Idle,
        StatusType.Dnd => UserStatus.DoNotDisturb,
        StatusType.Unknown => UserStatus.Unknown,
        _ => UserStatus.Offline,
      };
    }
  }
}
