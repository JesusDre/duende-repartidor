using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra el tiempo restante, los regalos entregados y el objetivo actual.
/// </summary>
public class HUD : MonoBehaviour
{
    [SerializeField] Text textoTiempo;
    [SerializeField] Text textoRegalos;
    [SerializeField] Text textoObjetivo;
    [SerializeField] Text textoAviso;

    [SerializeField] Color colorNormal = Color.white;
    [SerializeField] Color colorUrgente = new Color(1f, 0.3f, 0.25f);
    [SerializeField] float segundosUrgentes = 15f;

    GameManager gm;
    GiftCarrier carrier;
    float avisoHasta;

    void Start()
    {
        gm = GameManager.Instance;
        carrier = FindAnyObjectByType<GiftCarrier>();

        if (gm != null)
        {
            gm.ProgresoCambiado += AlCambiarProgreso;
            gm.PartidaTerminada += AlTerminar;
            AlCambiarProgreso(gm.Entregados, gm.Meta);
        }
        if (carrier != null)
        {
            carrier.CambioRegalo += AlCambiarRegalo;
            AlCambiarRegalo(carrier.TieneRegalo);
        }
        if (textoAviso != null) textoAviso.enabled = false;
        CrabAI.RegaloRobado += AlRobarRegalo;
    }

    void AlRobarRegalo(CrabAI cangrejo) => MostrarAviso("¡Un cangrejo te robó el regalo!", 2f);

    void OnDestroy()
    {
        if (gm != null)
        {
            gm.ProgresoCambiado -= AlCambiarProgreso;
            gm.PartidaTerminada -= AlTerminar;
        }
        if (carrier != null) carrier.CambioRegalo -= AlCambiarRegalo;
        CrabAI.RegaloRobado -= AlRobarRegalo;
    }

    void Update()
    {
        if (gm == null) return;

        float t = gm.TiempoRestante;
        int min = Mathf.FloorToInt(t / 60f);
        int seg = Mathf.FloorToInt(t % 60f);
        textoTiempo.text = $"{min}:{seg:00}";

        bool urgente = t <= segundosUrgentes && gm.EstadoActual == GameManager.Estado.Jugando;
        textoTiempo.color = urgente ? colorUrgente : colorNormal;
        float escala = urgente ? 1f + 0.12f * Mathf.Abs(Mathf.Sin(Time.time * 6f)) : 1f;
        textoTiempo.transform.localScale = Vector3.one * escala;

        if (textoAviso != null && textoAviso.enabled)
        {
            float resto = avisoHasta - Time.time;
            textoAviso.enabled = resto > 0f;
            var c = textoAviso.color;
            c.a = Mathf.Clamp01(resto / 0.4f);
            textoAviso.color = c;
            textoAviso.transform.localScale = Vector3.one * (1f + 0.15f * Mathf.Clamp01(resto - 1.1f) / 0.4f);
        }
    }

    void AlCambiarProgreso(int entregados, int meta)
    {
        textoRegalos.text = $"{entregados} / {meta}";
        if (entregados > 0) MostrarAviso("¡Regalo entregado!");
    }

    void AlCambiarRegalo(bool tieneRegalo)
    {
        textoObjetivo.text = tieneRegalo
            ? "Lleva el regalo a la casa que brilla"
            : "Ve al taller de Santa por un regalo";
    }

    void AlTerminar(GameManager.Estado estado)
    {
        textoObjetivo.text = "";
    }

    /// <summary>Muestra un mensaje breve en el centro de la pantalla.</summary>
    public void MostrarAviso(string mensaje, float duracion = 1.5f)
    {
        if (textoAviso == null) return;
        textoAviso.text = mensaje;
        textoAviso.enabled = true;
        avisoHasta = Time.time + duracion;
    }
}
