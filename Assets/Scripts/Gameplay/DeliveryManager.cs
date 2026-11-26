using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Elige al azar la casa que debe recibir el siguiente regalo y registra las entregas.
/// </summary>
public class DeliveryManager : MonoBehaviour
{
    [SerializeField] List<DeliveryHouse> casas = new List<DeliveryHouse>();

    public DeliveryHouse Destino { get; private set; }
    public int Entregados { get; private set; }

    /// <summary>Se dispara con el total de regalos entregados.</summary>
    public event Action<int> RegaloEntregado;
    public event Action<DeliveryHouse> DestinoCambiado;

    public IReadOnlyList<DeliveryHouse> Casas => casas;

    void Start()
    {
        foreach (var casa in casas) casa.Manager = this;
        ElegirNuevoDestino();
    }

    public void IntentarEntrega(DeliveryHouse casa, GiftCarrier carrier)
    {
        if (!enabled || casa != Destino || !carrier.Entregar()) return;
        Entregados++;
        RegaloEntregado?.Invoke(Entregados);
        ElegirNuevoDestino();
    }

    public void ElegirNuevoDestino()
    {
        if (casas.Count == 0) return;
        var anterior = Destino;
        DeliveryHouse nueva;
        do
        {
            nueva = casas[UnityEngine.Random.Range(0, casas.Count)];
        } while (casas.Count > 1 && nueva == anterior);

        if (anterior != null) anterior.SetDestino(false);
        Destino = nueva;
        Destino.SetDestino(true);
        DestinoCambiado?.Invoke(Destino);
    }

    /// <summary>Apaga todos los destinos (al terminar la partida).</summary>
    public void Detener()
    {
        foreach (var casa in casas) casa.SetDestino(false);
        enabled = false;
    }
}
