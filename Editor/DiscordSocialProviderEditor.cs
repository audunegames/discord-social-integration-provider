using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Audune.Social.Discord.Editor
{
  // Class that defines an editor for a StreamingAssets locale loader
  [CustomEditor(typeof(DiscordSocialProvider))]
  public class DiscordSocialProviderEditor : UnityEditor.Editor
  {
    // Constants
    private const string _applicationSettingsURL = "https://discord.com/developers/applications/{0}";
    private const string _overviewURL = "https://docs.discord.com/developers/discord-social-sdk/overview";
    private const string _apiDocumentationURL = "https://discord.com/developers/docs/social-sdk/index.html";
    
    
    // Properties of the editor
    private SerializedProperty _priority;
    private SerializedProperty _executionMode;
    private SerializedProperty _discordApplicationId;

    // Foldout state of the editor
    private bool _applicationDetailsFoldout = true;
    private bool _executionSettingsFoldout = false;

    // Return the target object of the editor
    public new DiscordSocialProvider target => serializedObject.targetObject as DiscordSocialProvider;


    // OnEnable is called when the component becomes enabled
    protected void OnEnable()
    {
      // Initialize the properties
      _priority = serializedObject.FindProperty("_priority");
      _executionMode = serializedObject.FindProperty("_executionMode");
      _discordApplicationId = serializedObject.FindProperty("_discordApplicationId");
    }

    // Draw the inspector GUI
    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      EditorGUI.BeginChangeCheck();

      _applicationDetailsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_applicationDetailsFoldout, "Application Details");
      if (_applicationDetailsFoldout)
      {
        EditorGUILayout.PropertyField(_discordApplicationId, new GUIContent("Application ID", _discordApplicationId.tooltip));
        
        EditorGUILayout.Space();
        
        if (_discordApplicationId.ulongValue != 0 && GUILayout.Button("Open Application Settings"))
          Application.OpenURL(string.Format(_applicationSettingsURL, _discordApplicationId.ulongValue.ToString(CultureInfo.InvariantCulture)));

        EditorGUILayout.Space();
      }
      EditorGUI.EndFoldoutHeaderGroup();

      _executionSettingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_executionSettingsFoldout, "Execution Settings");
      if (_executionSettingsFoldout)
      {
        EditorGUILayout.PropertyField(_priority);
        EditorGUILayout.PropertyField(_executionMode);
      }
      EditorGUI.EndFoldoutHeaderGroup();
      
      EditorGUILayout.Space();
      
      if (GUILayout.Button("Open Discord Social SDK Overview"))
        Application.OpenURL(_overviewURL);
      if (GUILayout.Button("Open Discord Social SDK API Documentation"))
        Application.OpenURL(_apiDocumentationURL);

      if (EditorGUI.EndChangeCheck())
        serializedObject.ApplyModifiedProperties();
    }
  }
}