using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Toma capturas de la escena Game desde varias cámaras para revisar el nivel sin abrir el Editor.
/// Uso en batch: -executeMethod DuendeCaptura.Capturar -capturas &lt;carpeta&gt;
/// </summary>
public static class DuendeCaptura
{
    [MenuItem("Duende/Tomar capturas de revisión")]
    public static void Capturar()
    {
        var carpeta = "Logs/Capturas";
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-capturas") carpeta = args[i + 1];
        Directory.CreateDirectory(carpeta);

        EditorSceneManager.OpenScene(DuendeBuilder.GameScene, OpenSceneMode.Single);

        var spawn = GameObject.Find("PlayerSpawn");
        var origen = spawn != null ? spawn.transform.position : Vector3.zero;

        // El primer render en modo batch sale sin iluminar; se descarta.
        Tomar(Path.Combine(carpeta, "calentamiento.png"), Vector3.up * 10f, Vector3.zero, 60f);
        File.Delete(Path.Combine(carpeta, "calentamiento.png"));
        Tomar(Path.Combine(carpeta, "vista_aerea.png"), new Vector3(0f, 55f, -55f), Vector3.zero, 50f);
        Tomar(Path.Combine(carpeta, "vista_jugador.png"), origen + new Vector3(0f, 2.6f, -4.5f), origen + Vector3.up * 1.2f, 60f);
        Tomar(Path.Combine(carpeta, "vista_casa.png"), new Vector3(-6f, 3f, 12f),
            DuendeBuilder.DireccionCasa(0) * DuendeBuilder.RadioCasas + Vector3.up * 1.5f, 55f);
        // Pantalla final (se activa solo en memoria)
        var final = Object.FindAnyObjectByType<EndScreen>();
        if (final != null)
        {
            final.transform.Find("Panel").gameObject.SetActive(true);
            Tomar(Path.Combine(carpeta, "pantalla_final.png"), origen + new Vector3(0f, 2.6f, -4.5f), origen + Vector3.up * 1.2f, 60f);
        }

        // Menú principal desde su propia cámara
        EditorSceneManager.OpenScene(DuendeBuilder.MainMenuScene, OpenSceneMode.Single);
        var camMenu = Camera.main;
        camMenu.gameObject.SetActive(false);
        Tomar(Path.Combine(carpeta, "menu.png"), camMenu.transform.position,
            camMenu.transform.position + camMenu.transform.forward, camMenu.fieldOfView);

        Debug.Log($"[DuendeCaptura] Capturas guardadas en {Path.GetFullPath(carpeta)}");
    }

    static void Tomar(string ruta, Vector3 pos, Vector3 mirarA, float fov)
    {
        var go = new GameObject("CamaraCaptura");
        var cam = go.AddComponent<Camera>();
        cam.transform.position = pos;
        cam.transform.LookAt(mirarA);
        cam.fieldOfView = fov;
        cam.farClipPlane = 500f;
        cam.clearFlags = CameraClearFlags.Skybox;

        // Los canvas en modo Overlay no salen en Camera.Render: se pasan a modo cámara solo para la captura.
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!canvas.isRootCanvas || !canvas.gameObject.activeInHierarchy) continue;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1f;
        }

        const int w = 1280, h = 720;
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        Canvas.ForceUpdateCanvases();
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(go);
    }
}
