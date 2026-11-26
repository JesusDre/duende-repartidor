using UnityEngine;

/// <summary>
/// Punto de entrega frente a la puerta de una casa. Cuando es la casa destino,
/// muestra un haz de luz visible desde lejos y una zona brillante en la puerta.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DeliveryHouse : MonoBehaviour
{
    [SerializeField] GameObject indicadores;
    [SerializeField] Light luz;
    [SerializeField] float pulsoVelocidad = 3f;
    [SerializeField] ParticleSystem confeti;

    public bool EsDestino { get; private set; }
    public DeliveryManager Manager { get; set; }

    float intensidadBase;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (luz != null) intensidadBase = luz.intensity;
        SetDestino(false);
    }

    public void SetDestino(bool valor)
    {
        EsDestino = valor;
        if (indicadores != null) indicadores.SetActive(valor);
        if (luz != null) luz.enabled = valor;
    }

    /// <summary>Lanza confeti al recibir un regalo.</summary>
    public void Celebrar()
    {
        if (confeti != null) confeti.Play();
    }

    void Update()
    {
        if (EsDestino && luz != null)
            luz.intensity = intensidadBase * (0.75f + 0.25f * Mathf.Sin(Time.time * pulsoVelocidad));
    }

    void OnTriggerStay(Collider other)
    {
        if (!EsDestino || Manager == null) return;
        var carrier = other.GetComponentInParent<GiftCarrier>();
        if (carrier != null) Manager.IntentarEntrega(this, carrier);
    }
}
