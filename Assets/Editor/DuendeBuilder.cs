using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Herramientas de editor que generan la estructura y las escenas del juego.
/// Se pueden usar desde el menú "Duende" o en modo batch con -executeMethod DuendeBuilder.ConstruirTodo.
/// </summary>
public static partial class DuendeBuilder
{
    public const string ScenesDir = "Assets/Scenes";
    public const string MainMenuScene = ScenesDir + "/MainMenu.unity";
    public const string GameScene = ScenesDir + "/Game.unity";

    static readonly string[] Folders =
    {
        "Assets/Scripts", "Assets/Scripts/Core", "Assets/Scripts/Player", "Assets/Scripts/Gameplay",
        "Assets/Scripts/UI", "Assets/Editor", "Assets/Models", "Assets/Materials", "Assets/Prefabs", ScenesDir,
    };

    [MenuItem("Duende/Construir todo")]
    public static void ConstruirTodo()
    {
        CrearEstructura();
        ConfigurarPS1();
        PrepararMateriales();
        CrearPrefabJugador();
        ConstruirEscenaGame();
        ConstruirEscenaMenu();
        VerificarOrientacionModelos();
    }

    /// <summary>Comprueba que el frente de los modelos (la nariz del duende) mire hacia +Z.</summary>
    static void VerificarOrientacionModelos()
    {
        var duende = AssetDatabase.LoadAssetAtPath<GameObject>(ModelsDir + "/Duende.fbx");
        var nariz = duende.transform.Find("Nariz");
        Debug.Log($"[DuendeBuilder] Nariz del duende en {nariz.position} (z > 0 significa que mira a +Z)");
    }

    [MenuItem("Duende/1. Crear estructura y escenas base")]
    public static void CrearEstructura()
    {
        foreach (var folder in Folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
                AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
            }
        }

        if (!File.Exists(MainMenuScene)) CrearEscenaVacia(MainMenuScene);
        if (!File.Exists(GameScene)) CrearEscenaVacia(GameScene);
        RegistrarEscenasEnBuild();
        AssetDatabase.SaveAssets();
        Debug.Log("[DuendeBuilder] Estructura y escenas base listas.");
    }

    static void CrearEscenaVacia(string path)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, path);
    }

    public static void RegistrarEscenasEnBuild()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScene, true),
            new EditorBuildSettingsScene(GameScene, true),
        };
    }
}
