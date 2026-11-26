using System;
using UnityEngine;

/// <summary>
/// Va en el jugador. Controla si el duende lleva un regalo y muestra el regalo sobre su cabeza.
/// Solo se puede cargar un regalo a la vez.
/// </summary>
public class GiftCarrier : MonoBehaviour
{
    [SerializeField] GameObject regaloVisual;

    public bool TieneRegalo { get; private set; }

    /// <summary>Se dispara cada vez que el duende toma, entrega o pierde un regalo.</summary>
    public event Action<bool> CambioRegalo;

    void Awake()
    {
        if (regaloVisual != null) regaloVisual.SetActive(false);
    }

    public bool Recoger()
    {
        if (TieneRegalo) return false;
        Actualizar(true);
        return true;
    }

    public bool Entregar()
    {
        if (!TieneRegalo) return false;
        Actualizar(false);
        return true;
    }

    /// <summary>El regalo se pierde (por ejemplo, si lo roba un cangrejo).</summary>
    public bool Perder()
    {
        if (!TieneRegalo) return false;
        Actualizar(false);
        return true;
    }

    void Actualizar(bool tiene)
    {
        TieneRegalo = tiene;
        if (regaloVisual != null) regaloVisual.SetActive(tiene);
        CambioRegalo?.Invoke(tiene);
    }
}
