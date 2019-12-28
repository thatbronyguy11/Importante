using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class HierarchyIcons
{
    static HierarchyIcons() { EditorApplication.hierarchyWindowItemOnGUI += EvaluateIcons; }

    private static void EvaluateIcons(int instanceId, Rect selectionRect)
    {

        if (EditorPrefs.GetBool("HierarchyIconsKey"))
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null) return;

            IHierarchyIcon slotCon = go.GetComponent<IHierarchyIcon>();
            if (slotCon != null) DrawIcon(slotCon.EditorIconPath, selectionRect);
        }
    }

    private static void DrawIcon(string texName, Rect rect)
    {
        Rect r = new Rect(rect.x - 20f, rect.y + 2f, 14f, 14f);
        GUI.DrawTexture(r, GetTex(texName.Split(' ')[0]));
    }

    private static Texture2D GetTex(string name)
    {
        return (Texture2D)Resources.Load("Icons/" + name);
    }
}