using UnityEditor;
using UnityEngine;

/// <summary>
/// Issue #4: crea el prefab del duende jugable y lo coloca en la escena.
/// </summary>
public static partial class DuendeBuilder
{
    public const string PrefabsDir = "Assets/Prefabs";
    public const string PlayerPrefab = PrefabsDir + "/Jugador.prefab";

    [MenuItem("Duende/4. Crear prefab del jugador")]
    public static GameObject CrearPrefabJugador()
    {
        var raiz = new GameObject("Jugador");
        raiz.tag = "Player";

        var modelo = Modelo("Duende");
        PrefabUtility.UnpackPrefabInstance(modelo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        modelo.name = "Modelo";
        modelo.transform.SetParent(raiz.transform, false);
        modelo.transform.localScale = Vector3.one * 1.1f;

        var cc = raiz.AddComponent<CharacterController>();
        cc.height = 1.4f;
        cc.radius = 0.32f;
        cc.center = new Vector3(0f, 0.72f, 0f);
        cc.slopeLimit = 50f;
        cc.stepOffset = 0.35f;

        var pc = raiz.AddComponent<PlayerController>();
        var so = new SerializedObject(pc);
        so.FindProperty("modelo").objectReferenceValue = modelo.transform;
        so.ApplyModifiedPropertiesWithoutUndo();

        AgregarRegaloAlJugador(raiz);

        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, PlayerPrefab);
        Object.DestroyImmediate(raiz);
        return prefab;
    }

    static GameObject ColocarJugador(Transform spawn)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab) ?? CrearPrefabJugador();
        var jugador = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        // Mira hacia afuera del taller, de espaldas al portón
        jugador.transform.SetPositionAndRotation(spawn.position, Quaternion.LookRotation(Vector3.back));
        return jugador;
    }
}
