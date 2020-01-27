using UnityEditor;
using UnityEngine;
using Tagger;


public class ErrorWindow : EditorWindow
{
    private string _infoString = "\n";
    private GUILayoutOption _bannerHeight;
    private GUIStyle _centeredVersionLabel;
    private GUIStyle _publisherNameStyle;
    private GUIStyle _fixButton;
    private bool _stylesNotLoaded = true;
    
    internal static GUIContent PlayerTagTrue;
    internal static GUIContent PlayerTagFalse;

    internal static GUIContent DetectionTrue;
    internal static GUIContent DetectionFalse;
    internal static GUIContent DetectionUnknown;

    internal static GUIContent ReachTrue;
    internal static GUIContent ReachFalse;
    internal static GUIContent ReachUnknown;

    internal static GUIContent TagTrue;
    internal static GUIContent TagFalse;

    internal static GUIContent LayerTrue;
    internal static GUIContent LayerFalse;
    
    internal static GUIStyle helpbox;

    [MenuItem("Tools/Doors+/Detect Errors", false, 1)]
    private static void ShowWindow()
    {
        var myWindow = GetWindow<ErrorWindow>("Errors");
        myWindow.minSize = new Vector2(200, 200);
        myWindow.maxSize = myWindow.minSize;
        myWindow.titleContent = new GUIContent("Errors");
        myWindow.Show();
    }

    private void OnEnable()
    {
        _bannerHeight = GUILayout.Height(30);
    }

    private void LoadStyles()
    {
        _publisherNameStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            richText = true
        };

        _centeredVersionLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
        
        _fixButton = new GUIStyle((GUIStyle)"sv_label_3")
        {

        };
        
        helpbox = new GUIStyle(EditorStyles.helpBox)
        {
            alignment = TextAnchor.MiddleLeft,
            richText = true
         
        };

        
        PlayerTagTrue = IconContent(" Player Tag", "true", "");
        PlayerTagFalse = IconContent(" Player Tag", "false", "");

        DetectionTrue = IconContent(" Detection Script", "true", "");
        DetectionFalse = IconContent(" Detection Script", "false", "");
        DetectionUnknown = IconContent(" Detection Script", "help", "");

        ReachTrue = IconContent(" Reach Variable", "true", "");
        ReachFalse = IconContent(" Reach Variable", "false", "");
        ReachUnknown = IconContent(" Reach Variable", "help", "");

        TagTrue = IconContent(" Door Tag", "true", "");
        TagFalse = IconContent(" Door Tag", "false", "");

        LayerTrue = IconContent(" Trigger Zones Layer", "true", "");
        LayerFalse = IconContent(" Trigger Zones Layer", "false", "");
        
        _stylesNotLoaded = false;
    }

    private void OnGUI()
    {
        if (_stylesNotLoaded) LoadStyles();

        EditorGUILayout.Space();
        GUILayout.Label(new GUIContent("<size=20><b><color=#666666>Error Detection</color></b></size>"),
            _publisherNameStyle);
        EditorGUILayout.Space();

        bool playerError = GameObject.FindGameObjectWithTag("Player") == null;

        if (!playerError)
        {
            if (GUILayout.Button(PlayerTagTrue, helpbox))
                _infoString = "An object with the tag 'Player' was found.";

            var detectionError = GameObject.FindGameObjectWithTag("Player").GetComponent<DoorDetection>() == null;

            if (!detectionError)
            {
                if (GUILayout.Button(DetectionTrue, helpbox))
                    _infoString = "The detection script component was found attached to the player.";

                var reachError = GameObject.FindGameObjectWithTag("Player").GetComponent<DoorDetection>().Reach == 0;

                if (!reachError)
                {
                    if (GUILayout.Button(ReachTrue, helpbox))
                        _infoString = "The reach variable is not 0. \n";
                }

                else
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(ReachFalse, helpbox))
                    {
                        _infoString = "The reach variable is 0. \n";
                        EditorGUIUtility.PingObject(GameObject.FindGameObjectWithTag("Player").GetComponent<DoorDetection>());
                    }

                    if (GUILayout.Button("Fix")) ShowWindow();

                    EditorGUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                }
            }

            else
            {
                if (GUILayout.Button(DetectionFalse, helpbox))
                {
                    EditorGUIUtility.PingObject(GameObject.FindGameObjectWithTag("Player"));
                    _infoString = "The player doesn't have the detection script attached to it.";
                }

                if (GUILayout.Button(ReachUnknown, helpbox))
                {
                    _infoString = "There is no information on the reach variable.";
                    EditorGUIUtility.PingObject(GameObject.FindGameObjectWithTag("Player"));
                }
            }
        }

        else
        {
            if (GUILayout.Button(PlayerTagFalse, helpbox))
                _infoString = "There was no object found with the tag 'Player'.";

            if (GUILayout.Button(DetectionUnknown, helpbox))
                _infoString = "There is no information on the detection script.";

            if (GUILayout.Button(ReachUnknown, helpbox))
                _infoString = "There is no information on the reach variable.";
        }

        bool layerError = TagHelper.DoesLayerNotExist();

        if (!layerError)
        {
            if (GUILayout.Button(LayerTrue, helpbox))
                _infoString = ("The layer 'Trigger Zones' has been created.");
        }

        else if (layerError)
        {
            if (GUILayout.Button(LayerFalse, helpbox))
                _infoString = ("The layer 'Trigger Zones' has not yet been created.");
        }

        
        if(_infoString != "\n")EditorGUILayout.LabelField(_infoString, helpbox);

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Version 1.3.0", _centeredVersionLabel))
            Application.OpenURL("https://www.notion.so/doorsplus/877b05f850c94125808f367fe7369798?v=94d6d605aebd4c71ba2dd020e32577ec");
    }
    
    static GUIContent IconContent(string text, string icon, string tooltip)
    {
        Texture2D cached = (Texture2D)Resources.Load("Icons/" + icon);  
        return new GUIContent(text, cached, tooltip);
    }
}