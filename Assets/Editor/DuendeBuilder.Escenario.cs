using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Issue #3: construye el pueblo tropical navideño de la escena Game.
/// </summary>
public static partial class DuendeBuilder
{
    public const string MaterialsDir = "Assets/Materials";
    public const string ModelsDir = "Assets/Models";

    public const float RadioIsla = 45f;
    public const float RadioCasas = 24f;
    public const int NumCasas = 6;
    public static readonly Vector3 PosTaller = new Vector3(0f, 0f, 0f);

    static readonly Dictionary<string, Color> Colores = new Dictionary<string, Color>
    {
        { "M_Rojo", new Color(0.80f, 0.08f, 0.10f) },
        { "M_Verde", new Color(0.10f, 0.55f, 0.20f) },
        { "M_VerdeOscuro", new Color(0.05f, 0.30f, 0.10f) },
        { "M_Piel", new Color(0.96f, 0.76f, 0.62f) },
        { "M_Blanco", new Color(0.95f, 0.95f, 0.95f) },
        { "M_Negro", new Color(0.03f, 0.03f, 0.03f) },
        { "M_Dorado", new Color(0.95f, 0.70f, 0.10f) },
        { "M_Cafe", new Color(0.40f, 0.24f, 0.12f) },
        { "M_Madera", new Color(0.62f, 0.42f, 0.22f) },
        { "M_Pared", new Color(0.98f, 0.88f, 0.70f) },
        { "M_Turquesa", new Color(0.15f, 0.70f, 0.70f) },
        { "M_Rosa", new Color(0.95f, 0.45f, 0.60f) },
        { "M_Naranja", new Color(0.95f, 0.45f, 0.10f) },
        { "M_Azul", new Color(0.15f, 0.35f, 0.85f) },
        { "M_Vidrio", new Color(0.55f, 0.80f, 0.95f) },
        { "M_Arena", new Color(0.93f, 0.82f, 0.58f) },
        { "M_Agua", new Color(0.10f, 0.55f, 0.75f) },
        { "M_Camino", new Color(0.80f, 0.66f, 0.45f) },
    };

    // ------------------------------------------------------------ materiales
    public static Material Mat(string nombre)
    {
        var path = $"{MaterialsDir}/{nombre}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", Colores[nombre]);
        mat.SetFloat("_Smoothness", nombre == "M_Agua" ? 0.85f : nombre == "M_Vidrio" ? 0.7f : 0.15f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /// <summary>
    /// Crea los materiales URP y hace que cada FBX use el material del proyecto
    /// con el mismo nombre en lugar del material incrustado.
    /// </summary>
    [MenuItem("Duende/2. Preparar materiales y modelos")]
    public static void PrepararMateriales()
    {
        foreach (var nombre in Colores.Keys) Mat(nombre);
        AssetDatabase.SaveAssets();

        foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { ModelsDir }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName,
                ModelImporterMaterialSearch.Local);
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.SaveAndReimport();
        }
        Debug.Log("[DuendeBuilder] Materiales URP asignados a los modelos.");
    }

    public static GameObject Modelo(string nombre)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelsDir}/{nombre}.fbx");
        if (prefab == null) throw new System.Exception($"No se encontró el modelo {nombre}.fbx");
        return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    }

    // -------------------------------------------------------------- escena
    [MenuItem("Duende/3. Construir escena Game")]
    public static void ConstruirEscenaGame()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        ConstruirAmbiente();
        var mundo = new GameObject("Mundo").transform;
        ConstruirIsla(mundo);
        var taller = ConstruirTaller(mundo);
        var casas = ConstruirCasas(mundo);
        ConstruirPalmeras(mundo);
        ConstruirLimites(mundo);
        HornearNavMesh(mundo);

        var spawn = new GameObject("PlayerSpawn").transform;
        spawn.position = PosTaller + new Vector3(0f, 0.1f, -7f);

        var jugador = ColocarJugador(spawn);
        var camara = CrearCamara(jugador);
        CrearZonaTaller(taller);
        var entregas = CrearPuntosDeEntrega(casas);
        CrearFlechaGuia(jugador);
        CrearCangrejos();
        var gm = CrearGameManager(entregas, jugador, camara);

        var canvas = CrearCanvas("UI");
        var hud = CrearHUD(canvas.transform);
        CrearPantallaFinal(canvas.transform, hud.gameObject);
        CrearPausa(canvas.transform, jugador, camara);
        CrearEventSystem();

        EditorSceneManager.SaveScene(scene, GameScene);
        Debug.Log("[DuendeBuilder] Escena Game construida.");
    }

    static GameManager CrearGameManager(DeliveryManager entregas, GameObject jugador, Camera camara)
    {
        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        var so = new SerializedObject(gm);
        so.FindProperty("entregas").objectReferenceValue = entregas;
        so.FindProperty("jugador").objectReferenceValue = jugador.GetComponent<PlayerController>();
        so.FindProperty("camara").objectReferenceValue = camara.GetComponent<ThirdPersonCamera>();
        so.ApplyModifiedPropertiesWithoutUndo();
        return gm;
    }

    static Camera CrearCamara(GameObject jugador)
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        var cam = go.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.farClipPlane = 400f;
        go.AddComponent<AudioListener>();
        var tpc = go.AddComponent<ThirdPersonCamera>();
        tpc.Objetivo = jugador.transform;
        // Detrás del jugador, que mira hacia +Z (al taller)
        go.transform.position = jugador.transform.position + new Vector3(0f, 2.9f, -5.2f);
        go.transform.LookAt(jugador.transform.position + Vector3.up * 1.2f);
        return cam;
    }

    static void ConstruirAmbiente()
    {
        // Atardecer tropical: sol bajo y anaranjado, cielo rosado.
        var sol = new GameObject("Sol").AddComponent<Light>();
        sol.type = LightType.Directional;
        sol.color = new Color(1f, 0.78f, 0.55f);
        sol.intensity = 1.3f;
        sol.shadows = LightShadows.Soft;
        sol.transform.rotation = Quaternion.Euler(28f, -35f, 0f);
        RenderSettings.sun = sol;

        var cielo = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Cielo.mat");
        if (cielo == null)
        {
            cielo = new Material(Shader.Find("Skybox/Procedural"));
            AssetDatabase.CreateAsset(cielo, $"{MaterialsDir}/Cielo.mat");
        }
        cielo.SetColor("_SkyTint", new Color(0.55f, 0.45f, 0.60f));
        cielo.SetColor("_GroundColor", new Color(0.25f, 0.45f, 0.55f));
        cielo.SetFloat("_AtmosphereThickness", 1.1f);
        cielo.SetFloat("_Exposure", 1.1f);
        RenderSettings.skybox = cielo;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.85f, 0.65f, 0.70f);
        RenderSettings.ambientEquatorColor = new Color(0.65f, 0.55f, 0.50f);
        RenderSettings.ambientGroundColor = new Color(0.35f, 0.30f, 0.25f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.95f, 0.70f, 0.65f);
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance = 250f;
    }

    static void ConstruirIsla(Transform mundo)
    {
        var isla = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        isla.name = "Isla";
        isla.transform.SetParent(mundo);
        isla.transform.localScale = new Vector3(RadioIsla * 2f, 0.5f, RadioIsla * 2f);
        isla.transform.position = new Vector3(0f, -0.5f, 0f);
        isla.GetComponent<Renderer>().sharedMaterial = Mat("M_Arena");
        isla.isStatic = true;
        // El collider de cilindro es una cápsula; se cambia por una caja plana para caminar bien.
        Object.DestroyImmediate(isla.GetComponent<Collider>());
        var piso = isla.AddComponent<BoxCollider>();
        piso.size = new Vector3(1f, 2f, 1f);

        var mar = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mar.name = "Mar";
        mar.transform.SetParent(mundo);
        mar.transform.position = new Vector3(0f, -0.35f, 0f);
        mar.transform.localScale = new Vector3(60f, 1f, 60f);
        mar.GetComponent<Renderer>().sharedMaterial = Mat("M_Agua");
        Object.DestroyImmediate(mar.GetComponent<Collider>());
        mar.isStatic = true;

        // Caminos de tierra del taller a cada casa
        for (int i = 0; i < NumCasas; i++)
        {
            var dir = DireccionCasa(i);
            var camino = GameObject.CreatePrimitive(PrimitiveType.Cube);
            camino.name = $"Camino_{i + 1}";
            camino.transform.SetParent(mundo);
            var largo = RadioCasas - 6f;
            camino.transform.position = dir * (4f + largo / 2f) + Vector3.up * 0.005f;
            camino.transform.rotation = Quaternion.LookRotation(dir);
            camino.transform.localScale = new Vector3(2.2f, 0.02f, largo);
            camino.GetComponent<Renderer>().sharedMaterial = Mat("M_Camino");
            Object.DestroyImmediate(camino.GetComponent<Collider>());
            camino.isStatic = true;
        }
    }

    public static Vector3 DireccionCasa(int i)
    {
        // Seis casas en círculo alrededor del taller: 30°, 90°, 150°...
        float ang = 30f + i * 360f / NumCasas;
        return Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
    }

    static Transform ConstruirTaller(Transform mundo)
    {
        var taller = Modelo("Taller");
        taller.name = "Taller";
        taller.transform.SetParent(mundo);
        taller.transform.position = PosTaller;
        // El frente de los modelos mira hacia +Z; el portón del taller mira al spawn (-Z).
        taller.transform.rotation = Quaternion.LookRotation(Vector3.back);
        var col = taller.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 2f, -0.5f);
        col.size = new Vector3(6.2f, 4f, 4.7f);
        MarcarEstatico(taller);
        return taller.transform;
    }

    static List<Transform> ConstruirCasas(Transform mundo)
    {
        var casas = new List<Transform>();
        for (int i = 0; i < NumCasas; i++)
        {
            var casa = Modelo("Casa");
            casa.name = $"Casa_{i + 1}";
            casa.transform.SetParent(mundo);
            var dir = DireccionCasa(i);
            casa.transform.position = dir * RadioCasas;
            casa.transform.rotation = Quaternion.LookRotation(-dir); // la puerta mira al taller
            var col = casa.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.5f, 0f);
            col.size = new Vector3(3.7f, 3f, 3.7f);
            MarcarEstatico(casa);
            casas.Add(casa.transform);
        }
        return casas;
    }

    static void ConstruirPalmeras(Transform mundo)
    {
        var padre = new GameObject("Palmeras").transform;
        padre.SetParent(mundo);
        var rnd = new System.Random(2512);
        int colocadas = 0, intentos = 0;
        while (colocadas < 28 && intentos++ < 1000)
        {
            float ang = (float)rnd.NextDouble() * 360f;
            float r = 9f + (float)rnd.NextDouble() * (RadioIsla - 12f);
            var pos = Quaternion.Euler(0f, ang, 0f) * Vector3.forward * r;
            if (LejosDeEdificios(pos) == false) continue;

            var palmera = Modelo("Palmera");
            palmera.name = $"Palmera_{++colocadas}";
            palmera.transform.SetParent(padre);
            palmera.transform.position = pos;
            palmera.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
            float s = 0.8f + (float)rnd.NextDouble() * 0.5f;
            palmera.transform.localScale = Vector3.one * s;
            var col = palmera.AddComponent<CapsuleCollider>();
            col.radius = 0.25f;
            col.height = 4f;
            col.center = new Vector3(0.1f, 2f, 0f);
            MarcarEstatico(palmera);
        }
    }

    static bool LejosDeEdificios(Vector3 pos)
    {
        if (Vector3.Distance(pos, PosTaller) < 8f) return false;
        if (Vector3.Distance(pos, PosTaller + Vector3.back * 7f) < 4f) return false;
        for (int i = 0; i < NumCasas; i++)
        {
            var dir = DireccionCasa(i);
            if (Vector3.Distance(pos, dir * RadioCasas) < 6f) return false;
            // No bloquear los caminos
            var proy = Vector3.Dot(pos, dir);
            if (proy > 0f && proy < RadioCasas && Vector3.Distance(pos, dir * proy) < 2.5f) return false;
        }
        return true;
    }

    static void ConstruirLimites(Transform mundo)
    {
        // Paredes invisibles en el borde de la isla
        var padre = new GameObject("Limites").transform;
        padre.SetParent(mundo);
        const int lados = 32;
        float largo = 2f * Mathf.PI * RadioIsla / lados + 1f;
        for (int i = 0; i < lados; i++)
        {
            var dir = Quaternion.Euler(0f, i * 360f / lados, 0f) * Vector3.forward;
            var pared = new GameObject($"Limite_{i}");
            pared.transform.SetParent(padre);
            pared.transform.position = dir * (RadioIsla - 1f) + Vector3.up * 2f;
            pared.transform.rotation = Quaternion.LookRotation(dir);
            pared.AddComponent<BoxCollider>().size = new Vector3(largo, 6f, 1f);
        }
    }

    static void MarcarEstatico(GameObject go)
    {
        foreach (var t in go.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;
    }
}
