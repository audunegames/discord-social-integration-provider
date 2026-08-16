using Discord.Sdk;

namespace Audune.Social.Discord
{
  /// <summary>
  /// Interface that defines an adapter for rich presence data.
  /// </summary>
  /// <typeparam name="TData">The type of the rich presence data.</typeparam>
  public interface IDiscordRichPresenceAdapter<TData> where TData : IRichPresenceData
  {
    /// <summary>
    /// Converts the specified rich presence data to a Discord activity.
    /// </summary>
    /// <param name="data">The rich presence data to convert.</param>
    public Activity Convert(TData data);
  }
}
