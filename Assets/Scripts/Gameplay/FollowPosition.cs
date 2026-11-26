using UnityEngine;

/// <summary>Sigue la posición de un objetivo sin rotar (para la nevada que acompaña al jugador).</summary>
public class FollowPosition : MonoBehaviour
{
    [SerializeField] Transform objetivo;
    [SerializeField] Vector3 offset = new Vector3(0f, 12f, 0f);

    public void Configurar(Transform nuevoObjetivo, Vector3 nuevoOffset)
    {
        objetivo = nuevoObjetivo;
        offset = nuevoOffset;
    }

    void LateUpdate()
    {
        if (objetivo != null) transform.position = objetivo.position + offset;
    }
}
