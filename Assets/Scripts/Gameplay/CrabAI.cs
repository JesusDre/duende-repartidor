using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Cangrejo que patrulla la playa. Si ve al duende cargando un regalo lo persigue,
/// y si lo alcanza le roba el regalo. Camina de lado, como buen cangrejo.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CrabAI : MonoBehaviour
{
    enum Estado { Patrullar, Perseguir, Huir }

    [Header("Patrulla")]
    [SerializeField] float radioPatrulla = 9f;
    [SerializeField] float velocidadPatrulla = 2.2f;
    [SerializeField] Vector2 esperaEnPunto = new Vector2(0.5f, 2f);

    [Header("Persecución")]
    [SerializeField] float rangoVision = 9f;
    [SerializeField] float rangoOlvido = 14f;
    [SerializeField] float velocidadPersecucion = 4.6f;
    [SerializeField] float distanciaRobo = 1.0f;
    [SerializeField] float descansoTrasRobo = 4f;

    [Header("Animación")]
    [SerializeField] Transform modelo;

    /// <summary>Se dispara cuando cualquier cangrejo roba un regalo.</summary>
    public static event Action<CrabAI> RegaloRobado;

    NavMeshAgent agente;
    Transform jugador;
    GiftCarrier carrier;
    Vector3 casa;
    Estado estado;
    float esperarHasta;
    float descansoHasta;
    float fase;
    Vector3 posModelo;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        casa = transform.position;
        if (modelo != null) posModelo = modelo.localPosition;
        fase = UnityEngine.Random.value * 10f;
    }

    void Start()
    {
        carrier = FindAnyObjectByType<GiftCarrier>();
        if (carrier != null) jugador = carrier.transform;
        CambiarEstado(Estado.Patrullar);
    }

    void Update()
    {
        if (!agente.isOnNavMesh) return;

        var gm = GameManager.Instance;
        if (gm != null && gm.EstadoActual != GameManager.Estado.Jugando)
        {
            agente.isStopped = true;
            Animar();
            return;
        }

        float dist = jugador != null ? Vector3.Distance(transform.position, jugador.position) : float.MaxValue;
        bool presa = carrier != null && carrier.TieneRegalo && Time.time >= descansoHasta;

        switch (estado)
        {
            case Estado.Patrullar:
                if (presa && dist <= rangoVision) { CambiarEstado(Estado.Perseguir); break; }
                if (!agente.pathPending && agente.remainingDistance <= 0.3f)
                {
                    if (esperarHasta <= 0f)
                        esperarHasta = Time.time + UnityEngine.Random.Range(esperaEnPunto.x, esperaEnPunto.y);
                    else if (Time.time >= esperarHasta)
                    {
                        esperarHasta = 0f;
                        IrAPuntoAleatorio();
                    }
                }
                break;

            case Estado.Perseguir:
                if (!presa || dist > rangoOlvido) { CambiarEstado(Estado.Patrullar); break; }
                agente.SetDestination(jugador.position);
                if (dist <= distanciaRobo && carrier.Perder())
                {
                    descansoHasta = Time.time + descansoTrasRobo;
                    RegaloRobado?.Invoke(this);
                    CambiarEstado(Estado.Huir);
                }
                break;

            case Estado.Huir:
                if (Time.time >= descansoHasta || (!agente.pathPending && agente.remainingDistance <= 0.3f))
                    CambiarEstado(Estado.Patrullar);
                break;
        }

        Animar();
    }

    void CambiarEstado(Estado nuevo)
    {
        estado = nuevo;
        agente.isStopped = false;
        switch (nuevo)
        {
            case Estado.Patrullar:
                agente.speed = velocidadPatrulla;
                IrAPuntoAleatorio();
                break;
            case Estado.Perseguir:
                agente.speed = velocidadPersecucion;
                break;
            case Estado.Huir:
                agente.speed = velocidadPersecucion;
                agente.SetDestination(casa);
                break;
        }
    }

    void IrAPuntoAleatorio()
    {
        for (int i = 0; i < 10; i++)
        {
            var p = casa + UnityEngine.Random.insideUnitSphere * radioPatrulla;
            if (NavMesh.SamplePosition(p, out var hit, 2f, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
                return;
            }
        }
    }

    void Animar()
    {
        if (modelo == null) return;
        float v = agente.velocity.magnitude;
        fase += Time.deltaTime * (4f + v * 5f);
        float rebote = Mathf.Abs(Mathf.Sin(fase)) * 0.05f * Mathf.Clamp01(v);
        modelo.localPosition = posModelo + Vector3.up * rebote;
        // Las pinzas "aplauden" cuando persigue
        float balanceo = Mathf.Sin(fase * 0.5f) * (estado == Estado.Perseguir ? 12f : 5f);
        modelo.localRotation = Quaternion.Euler(0f, 90f, balanceo);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? casa : transform.position, radioPatrulla);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoVision);
    }
}
