using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Estado de la partida: tiempo restante, regalos entregados contra la meta, y victoria o derrota.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum Estado { Jugando, Victoria, Derrota }

    public static GameManager Instance { get; private set; }

    [SerializeField] float duracion = 120f;
    [SerializeField] int meta = 8;

    [Header("Referencias")]
    [SerializeField] DeliveryManager entregas;
    [SerializeField] PlayerController jugador;
    [SerializeField] ThirdPersonCamera camara;

    public Estado EstadoActual { get; private set; } = Estado.Jugando;
    public float TiempoRestante { get; private set; }
    public float Duracion => duracion;
    public int Meta => meta;
    public int Entregados => entregas != null ? entregas.Entregados : 0;

    public event Action<int, int> ProgresoCambiado; // entregados, meta
    public event Action<Estado> PartidaTerminada;

    void Awake()
    {
        Instance = this;
        TiempoRestante = duracion;
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (entregas != null) entregas.RegaloEntregado += AlEntregar;
        ProgresoCambiado?.Invoke(Entregados, meta);
    }

    void Update()
    {
        if (EstadoActual != Estado.Jugando) return;
        TiempoRestante = Mathf.Max(0f, TiempoRestante - Time.deltaTime);
        if (TiempoRestante <= 0f) Terminar(Estado.Derrota);
    }

    void AlEntregar(int total)
    {
        ProgresoCambiado?.Invoke(total, meta);
        if (total >= meta) Terminar(Estado.Victoria);
    }

    void Terminar(Estado resultado)
    {
        if (EstadoActual != Estado.Jugando) return;
        EstadoActual = resultado;
        if (entregas != null) entregas.Detener();
        if (jugador != null) jugador.PuedeMoverse = false;
        if (camara != null) camara.ControlActivo = false;
        PartidaTerminada?.Invoke(resultado);
    }

    // ------------------------------------------------------------ navegación
    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>Solo para pruebas: ajusta el tiempo restante.</summary>
    public void FijarTiempoRestante(float segundos) => TiempoRestante = segundos;
}
