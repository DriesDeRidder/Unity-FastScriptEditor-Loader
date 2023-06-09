using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

//Quick editor tool that allows you to open a script without having to load the whole visual studio solution
public class FastOpenScriptEditor : EditorWindow
{
    //script to load
    public MonoScript scriptToEdit;

    //options
    public string[] options = new string[] { "Visual Studio Code", "Notepad++" };
    public int index = 0;

    //visual studio code
    //You can also use this path for visual studio code but this doesn't work when using this script in teams
    private static readonly string visualCodePathUser = "\"C:\\Users\\[YourUserName]\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe\"";

    private static string visualCodePath;
    private static readonly string visualCodeDownloadLink = "https://code.visualstudio.com/Download";

    //notepad
    private static readonly string notepadPath = "\"C:\\Program Files\\Notepad++\\notepad++.exe\"";
    private static readonly string notepadDownloadLink = "https://notepad-plus-plus.org/downloads/";

    //missing editor info
    private static string missingEditorInfo;
    private static string linkURL;
    private static string infoURL;

    public static void ShowWindow()
    {
        GetWindow(typeof(FastOpenScriptEditor));
    }

    private void OnGUI()
    {
        Rect popupRect = new(position.x + 20f, position.y + 60f, 260f, 100f);

        EditorWindow popup = GetWindowWithRect<EditorWindow>(popupRect, true, "Popup");
        popup.titleContent = new GUIContent("Fast Open script tool");
        popup.ShowPopup();

        GUIContent missingEditorContent = new(missingEditorInfo);

        GUILayout.Label(missingEditorContent, EditorStyles.boldLabel);

        GUIContent linkContent = new(infoURL);

        if (GUILayout.Button(linkContent, EditorStyles.linkLabel))
        {
            Application.OpenURL(linkURL);
            popup.Close();
        }
    }

    #region Open quick editor
    [MenuItem("Assets/Open With VSCode", false, 50)]
    private static void OpenWithVSCode()
    {
        OpenEditor(0);
    }


    [MenuItem("CONTEXT/MonoBehaviour/Edit Script With VSCode", priority = -100)]
    private static void OpenWithVSCodeFast(MenuCommand command)
    {
        MonoBehaviour scriptToLoad = command.context as MonoBehaviour;
        OpenEditor(0, MonoScript.FromMonoBehaviour(scriptToLoad));
    }

    [MenuItem("Assets/Open With Notepad++", false, 50)]
    private static void OpenWithNotepad()
    {
        OpenEditor(1);
    }

    [MenuItem("CONTEXT/MonoBehaviour/Edit Script With Notepad++", priority = -100)]
    private static void OpenWithNotepadFast(MenuCommand command)
    {
        MonoBehaviour scriptToLoad = command.context as MonoBehaviour;
        OpenEditor(1, MonoScript.FromMonoBehaviour(scriptToLoad));
    }

    public static void OpenEditor(int thisIndex)
    {
        FastOpenScriptEditor window = EditorWindow.GetWindow<FastOpenScriptEditor>();
        window.scriptToEdit = Selection.activeObject as MonoScript;
        window.OpenEditorIndex(thisIndex);
        window.Close();
    }

    public static void OpenEditor(int thisIndex, MonoScript scriptPath)
    {
        FastOpenScriptEditor window = EditorWindow.GetWindow<FastOpenScriptEditor>();
        window.scriptToEdit = scriptPath;
        window.OpenEditorIndex(thisIndex);
        window.Close();
    }

    public void OpenEditorIndex(int index)
    {
        string scriptPath = AssetDatabase.GetAssetPath(scriptToEdit);
        string editorPath = "";

        switch (index)
        {
            case 0:
                // VSCode path
                string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                editorPath = "\"" + Path.Combine(appData, "Programs", "Microsoft VS Code", "Code.exe") + "\"";
                break;
            case 1:
                // Notepad++ path
                editorPath = notepadPath;
                break;
        }

        if (editorPath == "" || editorPath == null)
        {
            UnityEngine.Debug.Log("No valid script path found");
            return;
        }

        // Add quotes around the script path so The process sees it as a single argument
        // in case there are spaces in the path 
        scriptPath = "\"" + scriptPath + "\"";

        Process.Start(editorPath, scriptPath);
    }
    #endregion


    /// <summary>
    /// Validates if the selected asset in the project window is a script
    /// and checks if notepad/VS code exists.
    /// If one of those returns false, the button will be greyed out
    /// </summary>
    #region validate buttons
    [MenuItem("Assets/Open With Notepad++", true)]
    private static bool ValidateOpenWithNotepadPath()
    {
        if (IsSelectedScript() == false) return false;

        bool notepadExists = File.Exists(notepadPath.Replace("\"", ""));

        if (notepadExists)
        {
            return true;
        }
        else
        {
            SetMissingEditorPopup("Notepad++", notepadDownloadLink);
            return false;
        }

    }

    // Only enable if the selection is a C# script
    [MenuItem("Assets/Open With VSCode", true)]
    private static bool ValidateOpenWithVSCodePath()
    {
        if (IsSelectedScript() == false) return false;

        string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        visualCodePath = "\"" + Path.Combine(appData, "Programs", "Microsoft VS Code", "Code.exe") + "\"";

        bool VisualCodeExists = File.Exists(visualCodePath.Replace("\"", ""));

        if (VisualCodeExists)
        {
            return true;
        }
        else
        {
            SetMissingEditorPopup("Visual Studio Code", visualCodeDownloadLink);
            return false;
        }
    }


    private static void SetMissingEditorPopup(string missingEditor, string missingEditorURL)
    {
        //In Preferences you can set this to false/true in case you'd like to get a popup with the missing
        //script editor
        if(FastOpenScriptEditorSettingsProvider.ShowMissingEditorPopup == false) return;

        missingEditorInfo = "It seems you don't have " + missingEditor + " installed";
        infoURL = "Click me to go to the " + missingEditor + " download page";
        linkURL = missingEditorURL;
        FastOpenScriptEditor window = EditorWindow.GetWindow<FastOpenScriptEditor>();
        window.Show();
    }

    private static bool IsSelectedScript()
    {
        return Selection.activeObject is MonoScript;
    }
    #endregion

}
