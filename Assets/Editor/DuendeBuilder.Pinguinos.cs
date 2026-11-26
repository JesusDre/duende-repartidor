using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Issues #11 y #16: NavMesh del pueblo y pingüinos que roban regalos.
/// </summary>
public static partial class DuendeBuilder
{
    public const string NavMeshAsset = ScenesDir + "/Game_NavMesh.asset";
    public const int NumPinguinos = 3;

    static void HornearNavMesh(Transform mundo)
    {
        var surface = mundo.gameObject.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();

        AssetDatabase.DeleteAsset(NavMeshAsset);
        AssetDatabase.CreateAsset(surface.navMeshData, NavMeshAsset);
        Debug.Log("[DuendeBuilder] NavMesh horneado.");
    }

    static void CrearPinguinos()
    {
        var padre = new GameObject("Pinguinos").transform;
        for (int i = 0; i < NumPinguinos; i++)
        {
            // Entre casas, a media distancia del taller
            var dir = Quaternion.Euler(0f, i * 120f + 60f, 0f) * Vector3.forward;
            var pos = dir * (RadioCasas * 0.75f);
            if (NavMesh.SamplePosition(pos, out var hit, 5f, NavMesh.AllAreas)) pos = hit.position;

            var raiz = new GameObject($"Pinguino_{i + 1}");
            raiz.transform.SetParent(padre);
            raiz.transform.position = pos;

            var modelo = Modelo("Pinguino");
            modelo.name = "Modelo";
            modelo.transform.SetParent(raiz.transform, false);
            modelo.transform.localScale = Vector3.one * 1.25f;

            var agente = raiz.AddComponent<NavMeshAgent>();
            agente.radius = 0.4f;
            agente.height = 1.5f;
            agente.speed = 2.2f;
            agente.angularSpeed = 360f;
            agente.acceleration = 14f;
            agente.stoppingDistance = 0.2f;
            agente.autoBraking = true;

            var ai = raiz.AddComponent<PenguinAI>();
            var so = new SerializedObject(ai);
            so.FindProperty("modelo").objectReferenceValue = modelo.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
