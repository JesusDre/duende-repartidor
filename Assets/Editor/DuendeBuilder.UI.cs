using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Issue #9 (y #10): construcción de la interfaz con uGUI.
/// </summary>
public static partial class DuendeBuilder
{
    static readonly Color FondoPanel = new Color(0.08f, 0.12f, 0.18f, 0.72f);
    static readonly Color Dorado = new Color(1f, 0.82f, 0.25f);

    static Font Fuente => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    static Sprite SpriteRedondeado => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

    // ------------------------------------------------------------ utilidades
    public static Canvas CrearCanvas(string nombre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static void CrearEventSystem()
    {
        var es = new GameObject("EventSystem", typeof(EventSystem));
        es.AddComponent<InputSystemUIInputModule>();
    }

    public static RectTransform CrearRect(string nombre, Transform padre, Vector2 ancla, Vector2 pivote,
        Vector2 pos, Vector2 tam)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(padre, false);
        rt.anchorMin = rt.anchorMax = ancla;
        rt.pivot = pivote;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
        return rt;
    }

    public static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    public static Image CrearPanel(string nombre, Transform padre, Vector2 ancla, Vector2 pos, Vector2 tam, Color color)
    {
        var rt = CrearRect(nombre, padre, ancla, ancla, pos, tam);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = SpriteRedondeado;
        img.type = Image.Type.Sliced;
        img.color = color;
        return img;
    }

    public static Text CrearTexto(string nombre, Transform padre, string texto, int tam, Color color,
        TextAnchor alineacion = TextAnchor.MiddleCenter, FontStyle estilo = FontStyle.Bold)
    {
        var rt = CrearRect(nombre, padre, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Estirar(rt);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = Fuente;
        t.text = texto;
        t.fontSize = tam;
        t.color = color;
        t.alignment = alineacion;
        t.fontStyle = estilo;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var sombra = rt.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.6f);
        sombra.effectDistance = new Vector2(2f, -2f);
        return t;
    }

    // ------------------------------------------------------------------ HUD
    static HUD CrearHUD(Transform canvas)
    {
        var raiz = CrearRect("HUD", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Estirar(raiz);

        // Tiempo (arriba a la izquierda)
        var panelTiempo = CrearPanel("PanelTiempo", raiz, new Vector2(0f, 1f), new Vector2(150f, -70f),
            new Vector2(240f, 100f), FondoPanel);
        var etiquetaT = CrearTexto("Etiqueta", panelTiempo.transform, "TIEMPO", 22, Dorado);
        etiquetaT.rectTransform.offsetMin = new Vector2(0f, 55f);
        var tiempo = CrearTexto("Tiempo", panelTiempo.transform, "2:00", 52, Color.white);
        tiempo.rectTransform.offsetMax = new Vector2(0f, -22f);

        // Regalos (arriba a la derecha)
        var panelRegalos = CrearPanel("PanelRegalos", raiz, new Vector2(1f, 1f), new Vector2(-150f, -70f),
            new Vector2(240f, 100f), FondoPanel);
        var etiquetaR = CrearTexto("Etiqueta", panelRegalos.transform, "REGALOS", 22, Dorado);
        etiquetaR.rectTransform.offsetMin = new Vector2(0f, 55f);
        var regalos = CrearTexto("Regalos", panelRegalos.transform, "0 / 8", 52, Color.white);
        regalos.rectTransform.offsetMax = new Vector2(0f, -22f);

        // Objetivo (arriba al centro)
        var panelObj = CrearPanel("PanelObjetivo", raiz, new Vector2(0.5f, 1f), new Vector2(0f, -55f),
            new Vector2(720f, 70f), FondoPanel);
        var objetivo = CrearTexto("Objetivo", panelObj.transform, "Ve al taller de Santa por un regalo", 30, Color.white);

        // Aviso (centro)
        var avisoRt = CrearRect("Aviso", raiz, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 180f), new Vector2(900f, 100f));
        var aviso = CrearTexto("Texto", avisoRt, "¡Regalo entregado!", 64, Dorado);

        // Controles (abajo a la izquierda)
        var ayuda = CrearRect("Controles", raiz, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 25f),
            new Vector2(900f, 40f));
        CrearTexto("Texto", ayuda, "WASD mover · Shift correr · Espacio saltar · Mouse cámara · Esc pausa", 22,
            new Color(1f, 1f, 1f, 0.8f), TextAnchor.LowerLeft, FontStyle.Normal);

        var hud = raiz.gameObject.AddComponent<HUD>();
        var so = new SerializedObject(hud);
        so.FindProperty("textoTiempo").objectReferenceValue = tiempo;
        so.FindProperty("textoRegalos").objectReferenceValue = regalos;
        so.FindProperty("textoObjetivo").objectReferenceValue = objetivo;
        so.FindProperty("textoAviso").objectReferenceValue = aviso;
        so.ApplyModifiedPropertiesWithoutUndo();
        return hud;
    }
}
