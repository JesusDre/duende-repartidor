using System;
using UnityEngine;

/// <summary>
/// Zona frente al taller de Santa. Si el duende entra sin regalo, toma uno.
/// Necesita un Collider marcado como trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Workshop : MonoBehaviour
{
    [Tooltip("Marca visual de la zona; brilla cuando el duende necesita un regalo.")]
    [SerializeField] GameObject marcador;

    public event Action RegaloRecogido;

    GiftCarrier jugador;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        jugador = FindAnyObjectByType<GiftCarrier>();
        if (jugador != null)
        {
            jugador.CambioRegalo += ActualizarMarcador;
            ActualizarMarcador(jugador.TieneRegalo);
        }
    }

    void OnDestroy()
    {
        if (jugador != null) jugador.CambioRegalo -= ActualizarMarcador;
    }

    // OnTriggerStay permite recoger otro regalo sin salir de la zona.
    void OnTriggerStay(Collider other)
    {
        if (!enabled) return;
        var carrier = other.GetComponentInParent<GiftCarrier>();
        if (carrier != null && carrier.Recoger()) RegaloRecogido?.Invoke();
    }

    void ActualizarMarcador(bool tieneRegalo)
    {
        if (marcador != null) marcador.SetActive(!tieneRegalo);
    }
}
