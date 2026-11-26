using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Issue #12: luces navideñas, confeti al entregar, letrero del taller y post-proceso.
/// </summary>
public static partial class DuendeBuilder
{
    const string PerfilPostProceso = "Assets/Settings/Duende_PostProceso.asset";

    static Material MatFoco()
    {
        var path = $"{MaterialsDir}/M_Foco.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", new Color(2f, 1.6f, 0.4f));
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material MatParticula()
    {
        var path = $"{MaterialsDir}/M_Confeti.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", Color.white);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /// <summary>Crea focos a lo largo de una línea quebrada (coordenadas locales del padre).</summary>
    static ChristmasLights CrearSerie(string nombre, Transform padre, IList<Vector3> puntos, float separacion,
        float tamFoco = 0.2f)
    {
        var serie = new GameObject(nombre);
        serie.transform.SetParent(padre, false);
        var focos = new List<Renderer>();
        var mat = MatFoco();
        for (int s = 0; s < puntos.Count - 1; s++)
        {
            var a = puntos[s];
            var b = puntos[s + 1];
            int n = Mathf.Max(1, Mathf.RoundToInt(Vector3.Distance(a, b) / separacion));
            for (int i = 0; i < n; i++)
            {
                var foco = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foco.name = "Foco";
                Object.DestroyImmediate(foco.GetComponent<Collider>());
                foco.transform.SetParent(serie.transform, false);
                foco.transform.localPosition = Vector3.Lerp(a, b, (float)i / n);
                foco.transform.localScale = Vector3.one * tamFoco;
                var r = foco.GetComponent<Renderer>();
                r.sharedMaterial = mat;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                focos.Add(r);
            }
        }
        var luces = serie.AddComponent<ChristmasLights>();
        luces.Configurar(focos.ToArray());
        return luces;
    }

    static void DecorarCasa(Transform casa)
    {
        // Contorno del techo (frente y aleros). El techo empieza a 2.8 m y la cumbrera está a 4.3 m.
        const float y = 2.82f, cima = 4.32f, x = 2.22f, z = 2.24f;
        CrearSerie("LucesFrente", casa, new[]
        {
            new Vector3(-x, y, z), new Vector3(0f, cima, z), new Vector3(x, y, z),
        }, 0.38f);
        CrearSerie("LucesAleros", casa, new[]
        {
            new Vector3(x, y, z), new Vector3(x, y, -z), new Vector3(-x, y, -z), new Vector3(-x, y, z),
        }, 0.45f);
    }

    static void DecorarTaller(Transform taller)
    {
        const float y = 3.82f, cima = 5.62f, x = 3.42f, zf = 2.12f, zt = -3.12f;
        CrearSerie("LucesFrente", taller, new[]
        {
            new Vector3(-x, y, zf), new Vector3(0f, cima, zf), new Vector3(x, y, zf),
        }, 0.45f, 0.26f);
        CrearSerie("LucesAleros", taller, new[]
        {
            new Vector3(x, y, zf), new Vector3(x, y, zt), new Vector3(-x, y, zt), new Vector3(-x, y, zf),
        }, 0.55f, 0.26f);

        // Letrero con texto 3D sobre el portón
        var go = new GameObject("TextoLetrero");
        go.transform.SetParent(taller, false);
        go.transform.localPosition = new Vector3(0f, 3.6f, 2.66f);
        go.transform.localRotation = Quaternion.identity;
        var tm = go.AddComponent<TextMesh>();
        tm.text = "TALLER DE SANTA";
        tm.font = Fuente;
        tm.fontSize = 80;
        tm.characterSize = 0.05f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = new Color(1f, 0.85f, 0.3f);
        go.GetComponent<MeshRenderer>().sharedMaterial = Fuente.material;
        // TextMesh se lee al derecho desde el frente (+Z) si se gira 180°
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
    }

    static void DecorarPalmeras(Transform mundo)
    {
        var palmeras = mundo.Find("Palmeras");
        if (palmeras == null) return;
        int i = 0;
        foreach (Transform palmera in palmeras)
        {
            if (i++ % 2 == 1) continue; // la mitad de las palmeras lleva luces
            var puntos = new List<Vector3>();
            for (int k = 0; k <= 28; k++)
            {
                float t = k / 28f;
                float ang = t * Mathf.PI * 2f * 3.5f;
                float r = Mathf.Lerp(0.22f, 0.13f, t);
                puntos.Add(new Vector3(0.25f * t + Mathf.Cos(ang) * r, 0.4f + t * 3.6f, Mathf.Sin(ang) * r));
            }
            CrearSerie("Luces", palmera, puntos, 0.32f, 0.15f);
        }
    }

    static void AgregarConfeti(DeliveryManager entregas)
    {
        var mat = MatParticula();
        foreach (var casa in entregas.Casas)
        {
            var go = new GameObject("Confeti");
            go.transform.SetParent(casa.transform, false);
            go.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = 1.1f;
            main.maxParticles = 150;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.2f, 0.2f), new Color(1f, 0.85f, 0.2f))
            {
                mode = ParticleSystemGradientMode.TwoColors,
            };

            var emision = ps.emission;
            emision.rateOverTime = 0f;
            emision.SetBursts(new[] { new ParticleSystem.Burst(0f, 80), new ParticleSystem.Burst(0.25f, 40) });

            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Cone;
            forma.angle = 35f;
            forma.radius = 0.3f;

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.Billboard;

            var so = new SerializedObject(casa);
            so.FindProperty("confeti").objectReferenceValue = ps;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static VolumeProfile PerfilPost()
    {
        var perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PerfilPostProceso);
        if (perfil != null) return perfil;
        perfil = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(perfil, PerfilPostProceso);

        var bloom = perfil.Add<Bloom>(true);
        bloom.threshold.Override(1f);
        bloom.intensity.Override(1.1f);
        bloom.scatter.Override(0.65f);
        var vineta = perfil.Add<Vignette>(true);
        vineta.intensity.Override(0.28f);
        var color = perfil.Add<ColorAdjustments>(true);
        color.saturation.Override(12f);
        color.postExposure.Override(0.1f);
        var tono = perfil.Add<Tonemapping>(true);
        tono.mode.Override(TonemappingMode.Neutral);

        foreach (var componente in perfil.components) AssetDatabase.AddObjectToAsset(componente, perfil);
        AssetDatabase.SaveAssets();
        return perfil;
    }

    static void CrearPostProceso(Camera camara)
    {
        var go = new GameObject("PostProceso");
        var vol = go.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.sharedProfile = PerfilPost();
        camara.GetUniversalAdditionalCameraData().renderPostProcessing = true;
    }

    static void ConstruirAmbientacion(Transform mundo, Transform taller, List<Transform> casas,
        DeliveryManager entregas, Camera camara)
    {
        foreach (var casa in casas) DecorarCasa(casa);
        DecorarTaller(taller);
        DecorarPalmeras(mundo);
        AgregarConfeti(entregas);
        CrearPostProceso(camara);
    }
}
