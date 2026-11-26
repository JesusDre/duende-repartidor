using UnityEditor;
using UnityEngine;

/// <summary>
/// Issues #6 y #7: zona del taller, regalo sobre el duende y puntos de entrega.
/// </summary>
public static partial class DuendeBuilder
{
    /// <summary>Material sin iluminación y semitransparente para zonas y marcadores.</summary>
    public static Material MatBrillo(string nombre, Color color)
    {
        var path = $"{MaterialsDir}/{nombre}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Surface", 1f); // Transparente
        mat.SetFloat("_Blend", 0f);   // Alpha
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.SetFloat("_Cull", 0f);    // Ambas caras
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /// <summary>Disco plano brillante que marca una zona en el suelo.</summary>
    public static GameObject CrearDisco(string nombre, Transform padre, Vector3 posLocal, float radio, Material mat)
    {
        var disco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disco.name = nombre;
        Object.DestroyImmediate(disco.GetComponent<Collider>());
        disco.transform.SetParent(padre, false);
        disco.transform.localPosition = posLocal;
        disco.transform.localScale = new Vector3(radio * 2f, 0.02f, radio * 2f);
        var r = disco.GetComponent<Renderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return disco;
    }

    // ---------------------------------------------------------------- #6
    static void AgregarRegaloAlJugador(GameObject jugador)
    {
        var visual = Modelo("Regalo");
        PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        visual.name = "RegaloCargado";
        visual.transform.SetParent(jugador.transform, false);
        visual.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        visual.transform.localScale = Vector3.one * 1.1f;
        foreach (var r in visual.GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        visual.AddComponent<Spinner>();

        var carrier = jugador.AddComponent<GiftCarrier>();
        var so = new SerializedObject(carrier);
        so.FindProperty("regaloVisual").objectReferenceValue = visual;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Workshop CrearZonaTaller(Transform taller)
    {
        var zona = new GameObject("ZonaTaller");
        zona.transform.SetParent(taller, false);
        // Frente del portón (el modelo mira hacia +Z local)
        zona.transform.localPosition = new Vector3(0f, 0f, 3.6f);
        var col = zona.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0f, 1f, 0f);
        col.size = new Vector3(4.5f, 2f, 2.4f);

        var marcador = CrearDisco("Marcador", zona.transform, new Vector3(0f, 0.03f, 0f), 1.6f,
            MatBrillo("M_ZonaTaller", new Color(1f, 0.85f, 0.2f, 0.45f)));
        var ws = zona.AddComponent<Workshop>();
        var so = new SerializedObject(ws);
        so.FindProperty("marcador").objectReferenceValue = marcador;
        so.ApplyModifiedPropertiesWithoutUndo();
        return ws;
    }

    // ---------------------------------------------------------------- #7
    static DeliveryManager CrearPuntosDeEntrega(System.Collections.Generic.List<Transform> casas)
    {
        var matZona = MatBrillo("M_ZonaEntrega", new Color(0.3f, 1f, 0.5f, 0.45f));
        var matHaz = MatBrillo("M_HazEntrega", new Color(1f, 0.9f, 0.35f, 0.22f));
        var manager = new GameObject("Entregas").AddComponent<DeliveryManager>();
        var lista = new SerializedObject(manager).FindProperty("casas");

        for (int i = 0; i < casas.Count; i++)
        {
            var punto = new GameObject("PuntoEntrega");
            punto.transform.SetParent(casas[i], false);
            punto.transform.localPosition = new Vector3(0f, 0f, 2.9f); // frente a la puerta
            var col = punto.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 1f, 0f);
            col.size = new Vector3(2.6f, 2f, 1.8f);

            var indicadores = new GameObject("Indicadores");
            indicadores.transform.SetParent(punto.transform, false);
            CrearDisco("Zona", indicadores.transform, new Vector3(0f, 0.03f, 0f), 1.3f, matZona);
            var haz = CrearDisco("Haz", indicadores.transform, new Vector3(0f, 15f, 0f), 0.7f, matHaz);
            haz.transform.localScale = new Vector3(1.4f, 15f, 1.4f);

            var luz = new GameObject("Luz").AddComponent<Light>();
            luz.transform.SetParent(punto.transform, false);
            luz.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            luz.type = LightType.Point;
            luz.color = new Color(1f, 0.85f, 0.4f);
            luz.range = 9f;
            luz.intensity = 4f;

            var casa = punto.AddComponent<DeliveryHouse>();
            var so = new SerializedObject(casa);
            so.FindProperty("indicadores").objectReferenceValue = indicadores;
            so.FindProperty("luz").objectReferenceValue = luz;
            so.ApplyModifiedPropertiesWithoutUndo();

            lista.arraySize = i + 1;
            lista.GetArrayElementAtIndex(i).objectReferenceValue = casa;
        }
        lista.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return manager;
    }

    static void CrearFlechaGuia(GameObject jugador)
    {
        var flecha = Modelo("Flecha");
        flecha.name = "FlechaGuia";
        foreach (var r in flecha.GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var guia = flecha.AddComponent<GuideArrow>();
        var so = new SerializedObject(guia);
        so.FindProperty("jugador").objectReferenceValue = jugador.transform;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
