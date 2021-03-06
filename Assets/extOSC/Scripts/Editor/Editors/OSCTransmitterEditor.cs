﻿/* Copyright (c) 2019 ExT (V.Sigalkin) */

using UnityEditor;
using UnityEngine;

using System.Net;

namespace extOSC.Editor
{
    [CustomEditor(typeof(OSCTransmitter))]
    public class OSCTransmitterEditor : UnityEditor.Editor
    {
        #region Static Private Vars

        private static readonly GUIContent _hostContent = new GUIContent("Remote Host:");

        private static readonly GUIContent _portContent = new GUIContent("Remote Port:");

        private static readonly GUIContent _mapBundleContent = new GUIContent("Map Bundle:");

		private static readonly GUIContent _advancedContent = new GUIContent("Advanced Settings:");

		private static readonly GUIContent _localPortModeContent = new GUIContent("Local Port Mode:");

        private static readonly GUIContent _localHostModeContent = new GUIContent("Local Host Mode:");

        private static readonly GUIContent _sourceReceiverContent = new GUIContent("Source Receiver:");

        private static readonly GUIContent _localHostContent = new GUIContent("Local Host:");

        private static readonly GUIContent _localPortContent = new GUIContent("Local Port:");

        private static readonly GUIContent _inGameContent = new GUIContent("In Game Controls:");

        private static readonly GUIContent _inEditorContent = new GUIContent("In Editor Controls:");

        private static string _advancedHelp = "Currently \"Advanced settings\" are not available for UWP (WSA).";

        private static string _fromReceiverHelp = "\"FromReceiver\" option is deprecated. Use \"Source Receiver\" settings.";

        #endregion

        #region Private Vars

        private SerializedProperty _remoteHostProperty;

        private SerializedProperty _remotePortProperty;

        private SerializedProperty _autoConnectProperty;

        private SerializedProperty _workInEditorProperty;

        private SerializedProperty _mapBundleProperty;

        private SerializedProperty _useBundleProperty;

        private SerializedProperty _closeOnPauseProperty;

        private SerializedProperty _sourceReceiverProperty;

        private SerializedProperty _localHostModeProperty;

        private SerializedProperty _localHostProperty;

		private SerializedProperty _localPortModeProperty;

		private SerializedProperty _localPortProperty;

        private OSCTransmitter _transmitter;

        private string _localHostCache;

        #endregion

        #region Unity Methods

        protected void OnEnable()
        {
            _transmitter = target as OSCTransmitter;
            _localHostCache = OSCUtilities.GetLocalHost();

            _remoteHostProperty = serializedObject.FindProperty("remoteHost");
            _remotePortProperty = serializedObject.FindProperty("remotePort");
            _autoConnectProperty = serializedObject.FindProperty("autoConnect");
            _workInEditorProperty = serializedObject.FindProperty("workInEditor");
            _mapBundleProperty = serializedObject.FindProperty("mapBundle");
            _useBundleProperty = serializedObject.FindProperty("useBundle");
            _closeOnPauseProperty = serializedObject.FindProperty("closeOnPause");
            _sourceReceiverProperty = serializedObject.FindProperty("localReceiver");
            _localHostModeProperty = serializedObject.FindProperty("localHostMode");
            _localHostProperty = serializedObject.FindProperty("localHost");
            _localPortModeProperty = serializedObject.FindProperty("localPortMode");
			_localPortProperty = serializedObject.FindProperty("localPort");

            if (!Application.isPlaying && !_transmitter.IsAvailable && _workInEditorProperty.boolValue)
            {
                _transmitter.Connect();
            }
        }

        protected void OnDisable()
        {
            if (_transmitter == null)
                _transmitter = target as OSCTransmitter;

            if (!Application.isPlaying && _transmitter.IsAvailable)
            {
                _transmitter.Close();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // LOGO
            GUILayout.Space(10);
            OSCEditorLayout.DrawLogo();
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Active: " + _transmitter.IsAvailable, EditorStyles.boldLabel);

            // SETTINGS BLOCK
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Transmitter Settings:", EditorStyles.boldLabel);

            // SETTINGS BOX
            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();

            IPAddress tempAddress;

            var remoteFieldColor = IPAddress.TryParse(_remoteHostProperty.stringValue, out tempAddress) ? Color.white : Color.red;

            // REMOTE HOST
            GUI.color = remoteFieldColor;
            EditorGUILayout.PropertyField(_remoteHostProperty, _hostContent);
            GUI.color = Color.white;

            // REMOTE PORT
            EditorGUILayout.PropertyField(_remotePortProperty, _portContent);

            // MAP BUNDLE
            EditorGUILayout.PropertyField(_mapBundleProperty, _mapBundleContent);

            // USE BUNDLE
            GUI.color = _useBundleProperty.boolValue ? Color.green : Color.red;
            if (GUILayout.Button("Use Bundle"))
            {
                _useBundleProperty.boolValue = !_useBundleProperty.boolValue;
            }
            GUI.color = Color.white;

            // SETTINGS BOX END
            EditorGUILayout.EndVertical();

            // PARAMETETS BLOCK
            EditorGUILayout.BeginHorizontal("box");

            GUI.color = _autoConnectProperty.boolValue ? Color.green : Color.red;
            if (GUILayout.Button("Auto Connect"))
            {
                _autoConnectProperty.boolValue = !_autoConnectProperty.boolValue;
            }
            GUI.color = Color.white;

            GUI.color = _closeOnPauseProperty.boolValue? Color.green : Color.red;
            if (GUILayout.Button("Close On Pause"))
            {
                _closeOnPauseProperty.boolValue = !_closeOnPauseProperty.boolValue;
            }
            GUI.color = Color.white;

            // PARAMETERS BLOCK END
            EditorGUILayout.EndHorizontal();

			// ADVANCED SETTIGS BOX
			EditorGUILayout.LabelField(_advancedContent, EditorStyles.boldLabel);
			GUILayout.BeginVertical("box");

	        if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.WSA)
	        {
		        GUI.color = Color.yellow;
				EditorGUILayout.HelpBox(_advancedHelp, MessageType.Info);
		        GUI.color = Color.white;
	        }

            EditorGUILayout.PropertyField(_sourceReceiverProperty, _sourceReceiverContent);

            var sourceReceiver = _transmitter.SourceReceiver;
            if (sourceReceiver != null)
            {
                var localHost = sourceReceiver.LocalHostMode == OSCLocalHostMode.Any ? _localHostCache : sourceReceiver.LocalHost;
                var localPort = sourceReceiver.LocalPort.ToString();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_localHostContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                EditorGUILayout.SelectableLabel(localHost, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_localPortContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                EditorGUILayout.SelectableLabel(localPort, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Or...", EditorStyles.boldLabel);

                // LOCAL HOST MODE
                EditorGUILayout.PropertyField(_localHostModeProperty, _localHostModeContent);

                if (_transmitter.LocalHostMode == OSCLocalHostMode.Any)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(_localHostContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    EditorGUILayout.SelectableLabel(_localHostCache,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    GUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.PropertyField(_localHostProperty, _localHostContent);
                }

                // LOCAL PORT MODE
                EditorGUILayout.PropertyField(_localPortModeProperty, _localPortModeContent);

                // LOCAL PORT
                if (_transmitter.LocalPortMode == OSCLocalPortMode.FromRemotePort)
                {
                    // LOCAL FROM REMOTE PORT
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(_localPortContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    EditorGUILayout.SelectableLabel(_transmitter.RemotePort.ToString(),
                        GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    GUILayout.EndHorizontal();
                }
                else if (_transmitter.LocalPortMode == OSCLocalPortMode.FromReceiver)
                {
                    EditorGUILayout.HelpBox(_fromReceiverHelp, MessageType.Warning);
                }
                else if (_transmitter.LocalPortMode == OSCLocalPortMode.Custom)
                {
                    // LOCAL PORT
                    EditorGUILayout.PropertyField(_localPortProperty, _localPortContent);
                }
            }

            EditorGUILayout.EndVertical();


            // CONTROLS
            EditorGUILayout.LabelField(Application.isPlaying ? _inGameContent : _inEditorContent, EditorStyles.boldLabel);

            if (Application.isPlaying) DrawControlsInGame();
            else DrawControlsInEditor();

            // CONTROLS END
            EditorGUILayout.EndVertical();

            // EDITOR BUTTONS
            GUI.color = Color.white;

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        protected void DrawControlsInGame()
        {
            EditorGUILayout.BeginHorizontal("box");

            GUI.color = _transmitter.IsAvailable ? Color.green : Color.red;
            var connection = GUILayout.Button(_transmitter.IsAvailable ? "Connected" : "Disconnected");

            GUI.color = Color.yellow;
            EditorGUI.BeginDisabledGroup(!_transmitter.IsAvailable);
            var reconnect = GUILayout.Button("Reconnect");
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            if (connection)
            {
                if (_transmitter.IsAvailable) _transmitter.Close();
                else _transmitter.Connect();
            }

            if (reconnect)
            {
                if (_transmitter.IsAvailable) _transmitter.Close();
                _transmitter.Connect();
            }
        }

        protected void DrawControlsInEditor()
        {
            EditorGUILayout.BeginHorizontal("box");

            GUI.color = _workInEditorProperty.boolValue ? Color.green : Color.red;
            var connection = GUILayout.Button("Work In Editor");

            GUI.color = Color.yellow;
            EditorGUI.BeginDisabledGroup(!_workInEditorProperty.boolValue);
            var reconect = GUILayout.Button("Reconnect");
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            if (connection)
            {
                _workInEditorProperty.boolValue = !_workInEditorProperty.boolValue;

                if (_workInEditorProperty.boolValue)
                {
                    if (_transmitter.IsAvailable) _transmitter.Close();

                    _transmitter.Connect();
                }
                else
                {
                    if (_transmitter.IsAvailable) _transmitter.Close();
                }
            }

            if (reconect)
            {
                if (!_workInEditorProperty.boolValue) return;

                if (_transmitter.IsAvailable) _transmitter.Close();

                _transmitter.Connect();
            }
        }

        #endregion
    }
}
