using UnityEngine;

/// <summary>
/// Flecha que flota alrededor del duende y apunta al siguiente objetivo:
/// el taller si no lleva regalo, o la casa destino si lo lleva.
/// </summary>
public class GuideArrow : MonoBehaviour
{
    [SerializeField] Transform jugador;
    [SerializeField] float radio = 1.1f;
    [SerializeField] float altura = 0.25f;
    [SerializeField] float ocultarCercaDe = 4f;

    GiftCarrier carrier;
    DeliveryManager entregas;
    Workshop taller;
    Renderer[] renderers;

    void Start()
    {
        carrier = jugador.GetComponent<GiftCarrier>();
        entregas = FindAnyObjectByType<DeliveryManager>();
        taller = FindAnyObjectByType<Workshop>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void LateUpdate()
    {
        Transform objetivo = null;
        if (carrier != null && carrier.TieneRegalo && entregas != null && entregas.enabled && entregas.Destino != null)
            objetivo = entregas.Destino.transform;
        else if (taller != null && taller.enabled)
            objetivo = taller.transform;

        Vector3 dir = objetivo != null ? objetivo.position - jugador.position : Vector3.zero;
        dir.y = 0f;
        bool visible = objetivo != null && dir.magnitude > ocultarCercaDe;
        foreach (var r in renderers) r.enabled = visible;
        if (!visible) return;

        dir.Normalize();
        float flotar = Mathf.Sin(Time.time * 4f) * 0.08f;
        transform.position = jugador.position + dir * radio + Vector3.up * (altura + flotar);
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
