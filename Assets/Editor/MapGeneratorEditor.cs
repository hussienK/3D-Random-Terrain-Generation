/* Hussien Kenaan
 * <20 - 7 - 2022>
 * 
 * the script is for generating the map from the editor for faster prototyping*/


using UnityEditor;
using UnityEngine;

//make editor work
[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //get the MapGenerator script from inspector
        MapGenerator mapGen = (MapGenerator)target;
        //when values changed and can audto update is enabled, update the map
        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
                mapGen.DrawMapInEditor();
        }
        //update the map when player click
        if (GUILayout.Button("Generate"))
        {
            mapGen.DrawMapInEditor();
        }
    }
}
