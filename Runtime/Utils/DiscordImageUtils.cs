using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Discord.Sdk;
using UnityEngine;
using UnityEngine.Networking;

namespace Audune.Social.Discord
{
  /// <summary>
  /// Class that defines utility methods for images in the Discord social provider.
  /// </summary>
  public static class DiscordImageUtils
  {
    /// <summary>
    /// The base URL where images are stored on Discord.
    /// </summary>
    public const string imageBaseUrl = "https://cdn.discordapp.com/";


    #region Returning URLs for images
    /// <summary>
    /// Returns the avatar URL for the specified user.
    /// </summary>
    /// <param name="userHandle">The user handle of the user whose avatar URL to get.</param>
    /// <param name="size">The desired size in pixels of the avatar; defaults to 1024.</param>
    /// <returns>The avatar URL for the specified user.</returns>
    public static string GetAvatarUrl(UserHandle userHandle, int size = 1024)
    {
      return $"{imageBaseUrl}/avatars/{userHandle.Id().ToString(CultureInfo.InvariantCulture)}/{userHandle.Avatar()}.png?size={size.ToString(CultureInfo.InvariantCulture)}";
    }
    #endregion

    #region Downloading textures from URLs
    /// <summary>
    /// Returns a texture downloaded from the specified URL.
    /// </summary>
    /// <param name="url">The URL to fetch as a texture.</param>
    /// <param name="cancellationToken">The cancellation token for the web request.</param>
    /// <returns>A texture downloaded from the specified URL.</returns>
    public static async UniTask<Texture2D> DownloadTexture(string url, CancellationToken cancellationToken = default)
    {
      var request = UnityWebRequestTexture.GetTexture(url);
      await request.SendWebRequest().WithCancellation(cancellationToken);

      if (request.result != UnityWebRequest.Result.Success)
        return null;
      
      return DownloadHandlerTexture.GetContent(request);
    }
    
    /// <summary>
    /// Returns a texture containing the avatar for the specified user.
    /// </summary>
    /// <param name="userHandle">The user handle of the user whose avatar URL to get.</param>
    /// <param name="size">The desired size in pixels of the avatar; defaults to 1024.</param>
    /// <param name="cancellationToken">The cancellation token for the web request.</param>
    /// <returns>A texture containing the avatar for the specified user.</returns>
    public static UniTask<Texture2D> DownloadAvatarTexture(UserHandle userHandle, int size = 1024, CancellationToken cancellationToken = default)
    {
      var avatarUrl = GetAvatarUrl(userHandle, size);
      return DownloadTexture(avatarUrl, cancellationToken);
    }
    #endregion
  }
}
