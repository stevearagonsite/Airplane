using System;
using UnityEditor;
using UnityEngine;

using TMPro;

using static EditorGenerics.TextStyles;
using static EditorGenerics.GenericsEditor;
using UnityEngine.UI;

[CustomEditor(typeof(UserLobbyPanels))]
public class EditorLobbyPanels : InspectorEditor
{
    private UserLobbyPanels _userLobbyPanels;
    private const string _TITLE = "PANEL MANAGER";
    private const string _SUBTITLE_PANELS = "PANELS";
    private const string _SUBTITLE_ELEMENTS = "ELEMENTS";

    private const string _ERROR_ENABLE_IN_RUN_MODE = "this can't edited in run-mode.";
    private const string _ERROR_OBJECT_NULL = "Set the field to avoid errors.";
    private const string _WARNING_OBJECT_TO_DISABLE = "Set disable this GameObject.";
    private const string _WARNING_OBJECT_TO_ENABLE = "Set active this GameObject.";

    private void OnEnable()
    {
        _userLobbyPanels = (UserLobbyPanels)target;
    }


    public override void OnInspectorGUI()
    {
        // The return can to stop the editing in execution mode.
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox(_ERROR_ENABLE_IN_RUN_MODE, MessageType.Error);
            return;
        }

        /** TITLE **/
        Spaces(2);
        Rect(3, Color.black);
        Title(_TITLE);
        Rect(3, Color.black);
        EditorGUILayout.Space();

        /** PANELS **/
        Subtitle(_SUBTITLE_PANELS);
        FieldPanels();

        /** ELEMENTS **/
        Rect(1, Color.gray);
        Subtitle(_SUBTITLE_ELEMENTS, h7);
        FieldElements();
    }

    private void FieldElements()
    {
        /** LOGIN **/
        var inputPlayerName = _userLobbyPanels.inputPlayerName;
        _userLobbyPanels.inputPlayerName = (TMP_InputField)EditorGUILayout.ObjectField("TMP Username field:*", inputPlayerName, typeof(TMP_InputField), true);

        /** CREATE ROOM **/
        var inputRoomName = _userLobbyPanels.inputRoomName;
        _userLobbyPanels.inputRoomName = (TMP_InputField)EditorGUILayout.ObjectField("TMP Roomname field:*", inputRoomName, typeof(TMP_InputField), true);
        var inputMaxPlayers = _userLobbyPanels.inputMaxPlayers;
        _userLobbyPanels.inputMaxPlayers = (TMP_InputField)EditorGUILayout.ObjectField("TMP Max-players field:*", inputMaxPlayers, typeof(TMP_InputField), true);

        /** LIST ROOM **/
        var roomListContent = _userLobbyPanels.roomListContent;
        _userLobbyPanels.roomListContent = (GameObject)EditorGUILayout.ObjectField("TMP RoomList-content:*", roomListContent, typeof(GameObject), true);

        /** INSIDE ROOM **/
        var startGameButton = _userLobbyPanels.startGameButton;
        _userLobbyPanels.startGameButton = (Button)EditorGUILayout.ObjectField("TMP StartGame Button:*", startGameButton, typeof(Button), true);

        /** LOADING **/
        var progress = _userLobbyPanels.progress;
        _userLobbyPanels.progress = (UserProgress)EditorGUILayout.ObjectField("Progress:*", progress, typeof(UserProgress), true);
    }

    private void FieldPanels()
    {
        var style = h6;

        /** LOGIN **/
        var loginPanel = _userLobbyPanels.loginPanel;
        EditorGUILayout.LabelField("Panel-login*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.loginPanel = (GameObject)EditorGUILayout.ObjectField("",(GameObject) _userLobbyPanels.loginPanel, typeof(GameObject), true);
        if (!loginPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (!loginPanel.activeSelf && loginPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_ENABLE, MessageType.Warning);
        EditorGUILayout.Space();

        /** SELECTION **/
        var selectionPanel = _userLobbyPanels.selectionPanel;
        EditorGUILayout.LabelField("Panel-selection*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.selectionPanel = (GameObject)EditorGUILayout.ObjectField("", selectionPanel, typeof(GameObject), true);
        if (!selectionPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (selectionPanel.activeSelf && selectionPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_DISABLE, MessageType.Warning);
        EditorGUILayout.Space();

        /** CREATE ROOM **/
        var createRoomPanel = _userLobbyPanels.createRoomPanel;
        EditorGUILayout.LabelField("Panel-create room*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.createRoomPanel = (GameObject)EditorGUILayout.ObjectField("", _userLobbyPanels.createRoomPanel, typeof(GameObject), true);
        if (!createRoomPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (createRoomPanel.activeSelf && createRoomPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_DISABLE, MessageType.Warning);
        EditorGUILayout.Space();

        /** RANDOM ROOM **/
        var randomRoomPanel = _userLobbyPanels.randomRoomPanel;
        EditorGUILayout.LabelField("Panel-random room*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.randomRoomPanel = (GameObject)EditorGUILayout.ObjectField("", _userLobbyPanels.randomRoomPanel, typeof(GameObject), true);
        if (!randomRoomPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (randomRoomPanel.activeSelf && randomRoomPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_DISABLE, MessageType.Warning);
        EditorGUILayout.Space();

        /** ROOMS LIST **/
        var listRoomPanel = _userLobbyPanels.listRoomPanel;
        EditorGUILayout.LabelField("Panel-rooms list*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.listRoomPanel = (GameObject)EditorGUILayout.ObjectField("", _userLobbyPanels.listRoomPanel, typeof(GameObject), true);
        if (!listRoomPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (listRoomPanel.activeSelf && listRoomPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_DISABLE, MessageType.Warning);
        EditorGUILayout.Space();
        
        /** HOW TO PLAY**/
        var howToPlayPanel = _userLobbyPanels.howToPlayPanel;
        EditorGUILayout.LabelField("How-to-play*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.howToPlayPanel = (GameObject)EditorGUILayout.ObjectField("", _userLobbyPanels.howToPlayPanel, typeof(GameObject), true);
        if (!howToPlayPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (howToPlayPanel.activeSelf && howToPlayPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_DISABLE, MessageType.Warning);
        EditorGUILayout.Space();
        
        /** CREDITS**/
        var creditsPanel = _userLobbyPanels.creditsPanel;
        EditorGUILayout.LabelField("Credits*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.creditsPanel = (GameObject)EditorGUILayout.ObjectField("", _userLobbyPanels.creditsPanel, typeof(GameObject), true);
        if (!creditsPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (creditsPanel.activeSelf && creditsPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_DISABLE, MessageType.Warning);
        EditorGUILayout.Space();

        /** INSIDE ROOM **/
        var insideRoomPanel = _userLobbyPanels.insideRoomPanel;
        EditorGUILayout.LabelField("Panel-inside room*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.insideRoomPanel = (GameObject)EditorGUILayout.ObjectField("", _userLobbyPanels.insideRoomPanel, typeof(GameObject), true);
        if (!insideRoomPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (insideRoomPanel.activeSelf && insideRoomPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_DISABLE, MessageType.Warning);
        EditorGUILayout.Space();
        
        /** LOADING ROOM **/
        var loadingPanel = _userLobbyPanels.LoadingPanel;
        EditorGUILayout.LabelField("Loading*", style, GUILayout.ExpandWidth(true));
        _userLobbyPanels.LoadingPanel = (GameObject)EditorGUILayout.ObjectField("", _userLobbyPanels.LoadingPanel, typeof(GameObject), true);
        if (!loadingPanel) EditorGUILayout.HelpBox(_ERROR_OBJECT_NULL, MessageType.Error);
        if (loadingPanel.activeSelf && loadingPanel) EditorGUILayout.HelpBox(_WARNING_OBJECT_TO_DISABLE, MessageType.Warning);
        EditorGUILayout.Space();
    }

    protected override void Subtitle(string value, GUIStyle style)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(value, style, GUILayout.ExpandWidth(true));
    }
}
