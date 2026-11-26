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

    /// <summary>
    /// Imita lo que ve el jugador: la escena 3D a 240 líneas escalada sin filtro y la UI nítida encima.
    /// </summary>
    static void Tomar(string ruta, Vector3 pos, Vector3 mirarA, float fov)
    {
        const int w = 1280, h = 720, lineas = 240;
        int anchoBajo = Mathf.RoundToInt(lineas * (float)w / h);

        var go = new GameObject("CamaraCaptura");
        var cam = go.AddComponent<Camera>();
        cam.transform.position = pos;
        cam.transform.LookAt(mirarA);
        cam.fieldOfView = fov;
        cam.farClipPlane = 500f;
        cam.clearFlags = CameraClearFlags.Skybox;

        // 1) Escena 3D a baja resolución, sin la UI
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases) c.enabled = false;
        var bajo = new RenderTexture(anchoBajo, lineas, 24, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point };
        cam.targetTexture = bajo;
        cam.Render();
        cam.targetTexture = null;
        foreach (var c in canvases) c.enabled = true;

        // 2) La imagen baja va de fondo en un canvas y la UI se dibuja encima a resolución completa
        var fondoCanvas = new GameObject("FondoCaptura", typeof(RectTransform)).AddComponent<Canvas>();
        var fondo = new GameObject("Imagen", typeof(RectTransform)).AddComponent<UnityEngine.UI.RawImage>();
        fondo.transform.SetParent(fondoCanvas.transform, false);
        fondo.texture = bajo;
        var rt = fondo.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var uiGo = new GameObject("CamaraUI");
        var uiCam = uiGo.AddComponent<Camera>();
        uiCam.clearFlags = CameraClearFlags.SolidColor;
        uiCam.backgroundColor = Color.black;
        uiCam.cullingMask = 0;
        fondoCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        fondoCanvas.worldCamera = uiCam;
        fondoCanvas.planeDistance = 5f;
        fondoCanvas.sortingOrder = -100;
        foreach (var canvas in canvases)
        {
            if (!canvas.isRootCanvas || !canvas.gameObject.activeInHierarchy) continue;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCam;
            canvas.planeDistance = 1f;
        }

        var grande = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        uiCam.targetTexture = grande;
        Canvas.ForceUpdateCanvases();
        uiCam.cullingMask = ~0;
        // Solo se dibujan los canvas: se ocultan temporalmente los renderers 3D
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var estaban = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) { estaban[i] = renderers[i].enabled; renderers[i].enabled = false; }
        var cielo = RenderSettings.skybox;
        uiCam.Render();
        for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = estaban[i];
        RenderSettings.skybox = cielo;

        RenderTexture.active = grande;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        RenderTexture.active = null;
        uiCam.targetTexture = null;

        Object.DestroyImmediate(fondoCanvas.gameObject);
        Object.DestroyImmediate(uiGo);
        Object.DestroyImmediate(bajo);
        Object.DestroyImmediate(grande);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(go);
    }
}
