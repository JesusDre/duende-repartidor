using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Botones del menú principal.</summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] string escenaJuego = "Game";
    [SerializeField] GameObject panelInstrucciones;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
    }

    public void Jugar() => SceneManager.LoadScene(escenaJuego);

    public void AlternarInstrucciones()
    {
        if (panelInstrucciones != null) panelInstrucciones.SetActive(!panelInstrucciones.activeSelf);
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
