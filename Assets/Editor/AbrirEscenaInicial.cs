using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Al abrir el proyecto, si el Editor muestra una escena vacía sin guardar, abre el menú principal.
/// Además, el botón Play siempre arranca desde MainMenu, sin importar qué escena esté abierta.
/// </summary>
[InitializeOnLoad]
static class AbrirEscenaInicial
{
    static AbrirEscenaInicial()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var inicio = AssetDatabase.LoadAssetAtPath<SceneAsset>(DuendeBuilder.MainMenuScene);
            EditorSceneManager.playModeStartScene = inicio;

            var activa = SceneManager.GetActiveScene();
            bool vacia = string.IsNullOrEmpty(activa.path) && !activa.isDirty && activa.rootCount <= 2;
            if (vacia && inicio != null) EditorSceneManager.OpenScene(DuendeBuilder.MainMenuScene);
        };
    }
}
