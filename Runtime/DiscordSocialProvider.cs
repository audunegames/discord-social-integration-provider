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
    [SerializeField, Tooltip("The minimal severity of messages to log")]
    private LoggingSeverity _loggingSeverity = LoggingSeverity.None;
    
    // Internal state
    private Client _client;
    private DiscordUser _currentUser;
    
    private readonly Dictionary<Type, object> _richPresenceAdapters = new Dictionary<Type, object>();
    
    
    /// <summary>
    /// Returns the Discord Application ID.
    /// </summary>
    internal ulong discordApplicationId => _discordApplicationId;
    
    /// <inheritdoc/>
    public override bool isInitialized => _client != null;
    
    
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
    public override void OnEnableSocialProvider()
    {
      base.OnEnableSocialProvider();
      
      // Create the client
      _client = new Client();

      // Add event handlers
      _client.AddLogCallback(OnLogMessage, _loggingSeverity);
      _client.SetStatusChangedCallback((status, error, errorCode) => Debug.Log($"{Client.StatusToString(status)}, {Client.ErrorToString(error)}, {errorCode}"));
      
      // Set the application identifier
      _client.SetApplicationId(_discordApplicationId);
    }
    
    /// <inheritdoc/>
    public override void OnDisableSocialProvider()
    {
      base.OnDisableSocialProvider();
      
      // Check if the client is initialized
      if (_client == null)
        return;
      
      // Dispose of the client
      _client.Dispose();
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
    public async UniTask<IUser> GetCurrentUser()
    {
      var completed = false;
      IUser currentUser = null;
      
      _client.GetDiscordClientConnectedUser(_discordApplicationId, (result, userHandle) => {
        currentUser = result.Successful() && userHandle != null ? new DiscordUser(this, userHandle) : null;
        completed = true;
      });
      
      await UniTask.WaitUntil(() => completed);
      return currentUser;
    }
    #endregion
    
    #region Rich presence provider implementation
    /// <inheritdoc/>
    public void UpdateRichPresence(IRichPresenceData data)
    {
      // Check if the client is initialized
      if (_client == null)
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
      if (_client == null)
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
        Debug.LogError($"[Discord] {result.Error()}");
    }
    #endregion
  }
}
