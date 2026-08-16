using System.Threading;
using Cysharp.Threading.Tasks;
using Discord.Sdk;
using UnityEngine;

namespace Audune.Social.Discord
{
  /// <summary>
  /// Class that defines a user in the Discord social provider.
  /// </summary>
  public sealed class DiscordUser : IUser
  {
    // Internal state
    private readonly DiscordSocialProvider _socialProvider;
    private readonly UserHandle _userHandle;


    /// <inheritdoc/>
    public SocialProvider socialProvider => _socialProvider;

    /// <inheritdoc/>
    public string name => _userHandle.Username();
    
    /// <inheritdoc/>
    public string displayName => _userHandle.DisplayName();
    
    /// <inheritdoc/>
    public UserStatus status => _userHandle.Status() switch {
      StatusType.Online => UserStatus.Online,
      StatusType.Streaming => UserStatus.Online,
      StatusType.Idle => UserStatus.Idle,
      StatusType.Dnd => UserStatus.DoNotDisturb,
      StatusType.Unknown => UserStatus.Unknown,
      _ => UserStatus.Offline,
    };


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="socialProvider">The source social provider of the user.</param>
    /// <param name="userHandle">The Discord user handle of the user.</param>
    internal DiscordUser(DiscordSocialProvider socialProvider, UserHandle userHandle)
    {
      _socialProvider = socialProvider;
      _userHandle = userHandle;
    }
    
    
    #region User implementation
    /// <inheritdoc/>
    public UniTask<Texture2D> GetAvatar(int size = 1024, CancellationToken cancellationToken = default)
    {
      return DiscordImageUtils.DownloadAvatarTexture(_userHandle, size, cancellationToken);
    }
    #endregion
  }
}
