using UnityEngine;
using UnityEngine.UI;

/// <summary>Muestra el panel de victoria o de derrota cuando termina la partida.</summary>
public class EndScreen : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Text titulo;
    [SerializeField] Text detalle;
    [SerializeField] Color colorVictoria = new Color(1f, 0.82f, 0.25f);
    [SerializeField] Color colorDerrota = new Color(1f, 0.45f, 0.4f);
    [SerializeField] GameObject ocultarAlTerminar;

    GameManager gm;

    void Start()
    {
        panel.SetActive(false);
        gm = GameManager.Instance;
        if (gm != null) gm.PartidaTerminada += Mostrar;
    }

    void OnDestroy()
    {
        if (gm != null) gm.PartidaTerminada -= Mostrar;
    }

    void Mostrar(GameManager.Estado estado)
    {
        bool gano = estado == GameManager.Estado.Victoria;
        titulo.text = gano ? "¡Navidad salvada!" : "¡Amaneció!";
        titulo.color = gano ? colorVictoria : colorDerrota;

        if (gano)
        {
            int seg = Mathf.CeilToInt(gm.TiempoRestante);
            detalle.text = $"Repartiste los {gm.Meta} regalos con {seg / 60}:{seg % 60:00} de sobra.\nNadie en el pueblo se quedó sin regalo.";
        }
        else
        {
            detalle.text = $"Entregaste {gm.Entregados} de {gm.Meta} regalos.\nEl sol salió antes de terminar el reparto.";
        }

        if (ocultarAlTerminar != null) ocultarAlTerminar.SetActive(false);
        panel.SetActive(true);
    }

    public void Reintentar() => gm.Reintentar();
    public void IrAlMenu() => gm.IrAlMenu();
}
