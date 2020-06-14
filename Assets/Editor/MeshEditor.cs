using Systems;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (GameLoader))]
public class MeshEditor : Editor {

    private GameLoader gameLoader;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Mesh"))
            gameLoader.Generate();

        string numIterationsString = gameLoader.ErosionIterationsCount.ToString();
        if (gameLoader.ErosionIterationsCount >= 1000) {
            numIterationsString = (gameLoader.ErosionIterationsCount/1000) + "k";
        }

        if (GUILayout.Button ("Erode (" + numIterationsString + " iterations)"))
            gameLoader.Erode();
    }

    void OnEnable () {
        gameLoader = (GameLoader) target;
        Tools.hidden = true;
    }

    void OnDisable () {
        Tools.hidden = false;
    }
}