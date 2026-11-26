using UnityEngine;

/// <summary>
/// Serie de focos navideños que cambian de color en secuencia, como una guirnalda de luces.
/// </summary>
public class ChristmasLights : MonoBehaviour
{
    [SerializeField] Renderer[] focos;
    [SerializeField] Color[] colores =
    {
        new Color(2.4f, 0.2f, 0.2f), new Color(0.3f, 2.2f, 0.4f),
        new Color(2.4f, 1.7f, 0.3f), new Color(0.4f, 0.8f, 2.6f),
    };
    [SerializeField] float pasosPorSegundo = 2.5f;
    [SerializeField, Range(0f, 1f)] float brilloApagado = 0.25f;

    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock bloque;
    float desfase;

    public void Configurar(Renderer[] nuevosFocos) => focos = nuevosFocos;

    void Awake()
    {
        bloque = new MaterialPropertyBlock();
        desfase = Random.value * 10f;
    }

    void Update()
    {
        if (focos == null || colores.Length == 0) return;
        int paso = Mathf.FloorToInt(Time.time * pasosPorSegundo + desfase);
        for (int i = 0; i < focos.Length; i++)
        {
            if (focos[i] == null) continue;
            var color = colores[(i + paso) % colores.Length];
            // Uno de cada tres focos parpadea a contratiempo
            bool apagado = (i % 3 == 0) && (paso % 2 == 0);
            bloque.SetColor(BaseColor, apagado ? color * brilloApagado : color);
            focos[i].SetPropertyBlock(bloque);
        }
    }
}
