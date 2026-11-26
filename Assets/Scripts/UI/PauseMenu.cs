using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Pausa con Esc (o Start en control): congela el tiempo y libera el cursor.</summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] PlayerController jugador;
    [SerializeField] ThirdPersonCamera camara;

    public bool Pausado { get; private set; }

    void Start() => panel.SetActive(false);

    void Update()
    {
        bool tecla = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                     || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
        if (!tecla) return;
        var gm = GameManager.Instance;
        if (gm != null && gm.EstadoActual != GameManager.Estado.Jugando) return;
        if (Pausado) Continuar(); else Pausar();
    }

    public void Pausar()
    {
        Pausado = true;
        Time.timeScale = 0f;
        panel.SetActive(true);
        if (jugador != null) jugador.PuedeMoverse = false;
        if (camara != null) camara.ControlActivo = false;
    }

    public void Continuar()
    {
        Pausado = false;
        Time.timeScale = 1f;
        panel.SetActive(false);
        if (jugador != null) jugador.PuedeMoverse = true;
        if (camara != null) camara.ControlActivo = true;
    }

    public void Reiniciar() => GameManager.Instance.Reintentar();
    public void IrAlMenu() => GameManager.Instance.IrAlMenu();
}
