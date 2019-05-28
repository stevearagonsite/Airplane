using UnityEditor;
using UnityEngine;

using TMPro;

using static GenericsMethods.TextStyles;
using static GenericsMethods.GenericsEditor;

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
        var playerNameInput = _userLobbyPanels.playerNameInput;
        _userLobbyPanels.playerNameInput = (TMP_InputField)EditorGUILayout.ObjectField("TMP Username field:*", playerNameInput, typeof(TMP_InputField), true);

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
    }

    protected override void Subtitle(string value, GUIStyle style)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(value, style, GUILayout.ExpandWidth(true));
    }
}
