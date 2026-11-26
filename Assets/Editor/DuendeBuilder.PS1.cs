using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Issue #14: configura URP para el estilo PS1 (render Forward, luces por vértice,
/// sin sombras ni antialiasing y pase de color reducido con dithering).
/// </summary>
public static partial class DuendeBuilder
{
    public const string ShaderPS1 = "Duende/PS1Lit";
    const string MatPosterize = MaterialsDir + "/M_PS1Posterize.mat";
    const string NombreFeature = "PS1Posterize";

    [MenuItem("Duende/0. Configurar estilo PS1")]
    public static void ConfigurarPS1()
    {
        var posterize = AssetDatabase.LoadAssetAtPath<Material>(MatPosterize);
        if (posterize == null)
        {
            posterize = new Material(Shader.Find("Duende/PS1Posterize"));
            AssetDatabase.CreateAsset(posterize, MatPosterize);
        }
        posterize.SetFloat("_Niveles", 32f); // 5 bits por canal, como la PS1
        posterize.SetFloat("_Dither", 0.85f);
        EditorUtility.SetDirty(posterize);

        foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset", new[] { "Assets/Settings" }))
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
            var so = new SerializedObject(asset);
            so.FindProperty("m_AdditionalLightsRenderingMode").intValue = (int)LightRenderingMode.PerVertex;
            so.FindProperty("m_MainLightShadowsSupported").boolValue = false;
            so.FindProperty("m_MSAA").intValue = 1;
            so.FindProperty("m_UpscalingFilter").intValue = (int)UpscalingFilterSelection.Point;
            so.ApplyModifiedPropertiesWithoutUndo();

            var renderers = so.FindProperty("m_RendererDataList");
            for (int i = 0; i < renderers.arraySize; i++)
            {
                var data = renderers.GetArrayElementAtIndex(i).objectReferenceValue as UniversalRendererData;
                if (data != null) ConfigurarRenderer(data, posterize);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[DuendeBuilder] Estilo PS1 configurado en URP.");
    }

    static void ConfigurarRenderer(UniversalRendererData data, Material posterize)
    {
        data.renderingMode = RenderingMode.Forward; // las luces por vértice necesitan Forward

        foreach (var f in data.rendererFeatures)
            if (f != null && f.name == NombreFeature) return;

        var feature = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
        feature.name = NombreFeature;
        feature.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.AfterRenderingPostProcessing;
        feature.fetchColorBuffer = true;
        feature.passMaterial = posterize;
        AssetDatabase.AddObjectToAsset(feature, data);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId);

        var so = new SerializedObject(data);
        var lista = so.FindProperty("m_RendererFeatures");
        lista.arraySize++;
        lista.GetArrayElementAtIndex(lista.arraySize - 1).objectReferenceValue = feature;
        var mapa = so.FindProperty("m_RendererFeatureMap");
        mapa.arraySize++;
        mapa.GetArrayElementAtIndex(mapa.arraySize - 1).longValue = localId;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
    }

    /// <summary>Sombra circular bajo un personaje (la PS1 no tenía sombras reales).</summary>
    public static void AgregarSombraBlob(Transform personaje, float radio)
    {
        var sombra = CrearDisco("SombraBlob", personaje, new Vector3(0f, 0.03f, 0f), radio,
            MatBrillo("M_Sombra", new Color(0.02f, 0.03f, 0.08f, 0.45f)));
        sombra.GetComponent<Renderer>().receiveShadows = false;
    }
}
