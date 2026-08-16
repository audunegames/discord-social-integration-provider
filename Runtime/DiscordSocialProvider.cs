using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Discord.Sdk;
using UnityEngine;

namespace Audune.Social.Discord
{
  /// <summary>
  /// Class that defines a social provider for the Discord Social API.
  /// </summary>
  [AddComponentMenu("Audune/Social/Discord Social Provider")]
  public sealed class DiscordSocialProvider : SocialProvider,
    IUserProvider,
    IIRichPresenceProvider
  {
    // Static variables
    private static DiscordSocialProvider _current;

    /// <summary>
    /// Returns the static instance of the Discord social provider.
    /// </summary>
    public static DiscordSocialProvider current => _current;
    
    
    // Variables
    [SerializeField, Tooltip("The Discord Application ID")]
    private ulong _discordApplicationId;
    
    // Internal state
    private Client _client;
    private LoggingSeverity _loggingSeverity = LoggingSeverity.None;
    private bool _initialized = false;
    
    private readonly Dictionary<Type, object> _richPresenceAdapters = new Dictionary<Type, object>();
    
    
    /// <summary>
    /// Returns the Discord Application ID.
    /// </summary>
    public ulong discordApplicationId => _discordApplicationId;

    /// <summary>
    /// Returns and sets the logging severity of the Discord client.
    /// </summary>
    public LoggingSeverity loggingSeverity {
      get => _loggingSeverity;
      set =>  _loggingSeverity = value;
    }
    
    /// <inheritdoc/>
    public override bool isInitialized => _initialized;
    
    
    /// <inheritdoc/>
    protected override void Awake()
    {
      base.Awake();
      
      // Set the static instance
      if (_current == null)
        _current = this;
      else
        Destroy(gameObject);
    }
    
    /// <inheritdoc/>
    public override async void OnEnableSocialProvider()
    {
      base.OnEnableSocialProvider();
      
      // Create the client
      _client = new Client();

      // Add event handlers
      _client.AddLogCallback(OnLogMessage, _loggingSeverity);
      
      // Set the application identifier
      _client.SetApplicationId(_discordApplicationId);
      
      // Check if the user is set
      var currentUser = await GetCurrentUser();
      if (currentUser == null)
      {
        Debug.LogError("[Discord] Could not initialize the Discord client", this);
        return;
      }
      
      // Set the initialized state
      _initialized = true;

      // Log the successful initialization
      Debug.Log("[Discord] Successfully initialized the Discord client", this);
    }
    
    /// <inheritdoc/>
    public override void OnDisableSocialProvider()
    {
      base.OnDisableSocialProvider();
      
      // Dispose of the client
      _client.Dispose();
      
      // Check if the client is initialized
      if (!_initialized)
        return;

      // Log the successful disposal
      Debug.Log("[Discord] Successfully disposed of the Discord client", this);
    }
    
    
    #region Managing rich presence adapters
    /// <summary>
    /// Registers the specified rich presence adapter for the specified data type.
    /// </summary>
    /// <param name="adapter">The rich presence adapter to register.</param>
    /// <typeparam name="TData">The type of the rich presence data to register the adapter for.</typeparam>
    public void RegisterRichPresenceAdapter<TData>(IDiscordRichPresenceAdapter<TData> adapter) where TData : IRichPresenceData
    {
      _richPresenceAdapters.Add(typeof(TData), adapter);
    }

    /// <summary>
    /// Unregisters the rich presence adapter for the specified data type.
    /// </summary>
    /// <typeparam name="TData">The type of the rich presence data to unregister the adapter for.</typeparam>
    public void UnregisterRichPresenceAdapter<TData>() where TData : IRichPresenceData
    {
      _richPresenceAdapters.Remove(typeof(TData));
    }
    #endregion
    
    #region User provider implementation
    /// <inheritdoc/>
    public UniTask<IUser> GetCurrentUser()
    {
      var completionSource = new UniTaskCompletionSource<IUser>();
      
      _client.GetDiscordClientConnectedUser(_discordApplicationId, (result, userHandle) => {
        if (!result.Successful() || userHandle == null)
          completionSource.TrySetResult(null);
        else
          completionSource.TrySetResult(new DiscordUser(this, userHandle));
      });

      return completionSource.Task;
    }
    #endregion
    
    #region Rich presence provider implementation
    /// <inheritdoc/>
    public void UpdateRichPresence(IRichPresenceData data)
    {
      // Check if the client is initialized
      if (!_initialized)
        return;
      
      // Get the adapter for the data
      if (!_richPresenceAdapters.TryGetValue(data.GetType(), out var adapterObject))
        throw new ArgumentException($"No adapter found for {data.GetType().Name}", nameof(data));

      try
      {
        // Create the activity
        var adapterType = typeof(IDiscordRichPresenceAdapter<>).MakeGenericType(data.GetType());
        var convertMethod = adapterType.GetMethod("Convert", new[] { data.GetType() });
        if (convertMethod == null)
          throw new ArgumentException($"Wrong adapter type found for {data.GetType()}", nameof(data));

        var activity = (Activity)convertMethod.Invoke(adapterObject, new object[] { data });
        
        // Update the rich presence
        _client.UpdateRichPresence(activity, OnLogResult);;
      }
      catch (Exception)
      {
        throw new ArgumentException($"Wrong adapter type found for {data.GetType()}", nameof(data));
      }
    }

    /// <inheritdoc/>
    public void ClearRichPresence()
    {
      // Check if the client is initialized
      if (!_initialized)
        return;
      
      // Update the rich presence
      _client.UpdateRichPresence(null, OnLogResult);
    }
    #endregion
    
    #region Event handlers
    /// <summary>
    /// Handles logging a message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="severity">The severity of the message to log.</param>
    private void OnLogMessage(string message, LoggingSeverity severity)
    {
      if (severity == LoggingSeverity.Error)
        Debug.LogError($"[Discord] {message}");
      else if (severity == LoggingSeverity.Warning)
        Debug.LogWarning($"[Discord] {message}");
      else
        Debug.Log($"[Discord] {message}");
    }
    
    /// <summary>
    /// Handles logging a client result
    /// </summary>
    /// <param name="result">The client result to log</param>
    private void OnLogResult(ClientResult result)
    {
      if (!result.Successful())
        Debug.LogError($"[Discord] Error in API call: (Code {result.ErrorCode()}) {result.Error()}");
    }
    #endregion
  }
}
