using UnityEngine;

/// <summary>Gira y hace flotar un objeto (regalos, adornos del menú).</summary>
public class Spinner : MonoBehaviour
{
    [SerializeField] float gradosPorSegundo = 90f;
    [SerializeField] float alturaFlotar = 0.1f;
    [SerializeField] float velocidadFlotar = 2.5f;
    [SerializeField] bool tiempoReal;

    Vector3 posInicial;
    float fase;

    void Awake()
    {
        posInicial = transform.localPosition;
        fase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float dt = tiempoReal ? Time.unscaledDeltaTime : Time.deltaTime;
        float t = (tiempoReal ? Time.unscaledTime : Time.time) * velocidadFlotar + fase;
        transform.Rotate(0f, gradosPorSegundo * dt, 0f, Space.World);
        transform.localPosition = posInicial + Vector3.up * Mathf.Sin(t) * alturaFlotar;
    }
}
