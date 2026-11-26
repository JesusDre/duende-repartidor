using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Renderiza el juego a baja resolución (como una PS1, unas 240 líneas) y lo escala
/// sin filtro para obtener píxeles grandes y nítidos. La UI se dibuja aparte, a resolución completa.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PS1Camera : MonoBehaviour
{
    [Tooltip("Líneas verticales de la imagen. La PS1 usaba 240.")]
    [SerializeField] int lineas = 240;

    UniversalRenderPipelineAsset asset;
    float escalaOriginal;
    UpscalingFilterSelection filtroOriginal;
    int altoAplicado;

    void OnEnable()
    {
        asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (asset == null) return;
        escalaOriginal = asset.renderScale;
        filtroOriginal = asset.upscalingFilter;
        asset.upscalingFilter = UpscalingFilterSelection.Point;
        Aplicar();
    }

    void Update()
    {
        if (asset != null && Screen.height != altoAplicado) Aplicar();
    }

    void Aplicar()
    {
        altoAplicado = Screen.height;
        asset.renderScale = Mathf.Clamp((float)lineas / Mathf.Max(1, Screen.height), 0.1f, 1f);
    }

    void OnDisable()
    {
        // Se restaura para no dejar modificado el asset en el Editor
        if (asset == null) return;
        asset.renderScale = escalaOriginal;
        asset.upscalingFilter = filtroOriginal;
    }
}
