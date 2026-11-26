using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Issue #11: NavMesh de la isla y cangrejos que roban regalos.
/// </summary>
public static partial class DuendeBuilder
{
    public const string NavMeshAsset = ScenesDir + "/Game_NavMesh.asset";
    public const int NumCangrejos = 3;

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

    static void CrearCangrejos()
    {
        var padre = new GameObject("Cangrejos").transform;
        for (int i = 0; i < NumCangrejos; i++)
        {
            // Entre casas, a media distancia del taller
            var dir = Quaternion.Euler(0f, i * 120f + 60f, 0f) * Vector3.forward;
            var pos = dir * (RadioCasas * 0.75f);
            if (NavMesh.SamplePosition(pos, out var hit, 5f, NavMesh.AllAreas)) pos = hit.position;

            var raiz = new GameObject($"Cangrejo_{i + 1}");
            raiz.transform.SetParent(padre);
            raiz.transform.position = pos;

            var modelo = Modelo("Cangrejo");
            modelo.name = "Modelo";
            modelo.transform.SetParent(raiz.transform, false);
            modelo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f); // camina de lado
            modelo.transform.localScale = Vector3.one * 1.6f;

            var agente = raiz.AddComponent<NavMeshAgent>();
            agente.radius = 0.45f;
            agente.height = 0.6f;
            agente.speed = 2.2f;
            agente.angularSpeed = 360f;
            agente.acceleration = 14f;
            agente.stoppingDistance = 0.2f;
            agente.autoBraking = true;

            var ai = raiz.AddComponent<CrabAI>();
            var so = new SerializedObject(ai);
            so.FindProperty("modelo").objectReferenceValue = modelo.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
