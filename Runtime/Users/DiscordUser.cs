using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Discord.Sdk;
using UnityEngine;

namespace Audune.Social.Discord
{
  /// <summary>
  /// Class that defines a user in the Discord social provider.
  /// </summary>
  public sealed class DiscordUser : IUser, IEquatable<DiscordUser>
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

    /// <inheritdoc/>
    /// <remarks>
    /// A Discord user only knows if they are playing the current game.
    /// </remarks>
    public bool isPlaying => _userHandle.GameActivity() != null;

    /// <inheritdoc/>
    public bool isPlayingThisGame => isPlaying;


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
    
    #region Equatable implementation
    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return ReferenceEquals(this, obj) 
        || obj is IUser other && Equals(other);
    }
    
    /// <inheritdoc/>
    public bool Equals(IUser other)
    {
      return ReferenceEquals(this, other) 
        || other is DiscordUser discordUser && Equals(discordUser);
    }
    
    /// <inheritdoc/>
    public bool Equals(DiscordUser other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return Equals(_socialProvider, other._socialProvider)
        && Equals(_userHandle.Id(), other._userHandle.Id());
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return HashCode.Combine(_socialProvider, _userHandle.Id());
    }

    
    /// <summary>
    /// Returns if the specified Discord users equal each other.
    /// </summary>
    /// <param name="left">The left Discord user to check.</param>
    /// <param name="right">The right Discord user to check.</param>
    /// <returns>If the specified Discord users equal each other.</returns>
    public static bool operator ==(DiscordUser left, DiscordUser right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Returns if the specified Discord users do not equal each other.
    /// </summary>
    /// <param name="left">The left Discord user to check.</param>
    /// <param name="right">The right Discord user to check.</param>
    /// <returns>If the specified Discord users do not equal each other.</returns>
    public static bool operator !=(DiscordUser left, DiscordUser right)
    {
      return !(left == right);
    }
    #endregion
  }
}
