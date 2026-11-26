using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Issue #10: menú principal, pausa y pantallas de victoria y derrota.
/// </summary>
public static partial class DuendeBuilder
{
    static readonly Color RojoBoton = new Color(0.78f, 0.12f, 0.14f);
    static readonly Color VerdeBoton = new Color(0.12f, 0.50f, 0.25f);

    public static Button CrearBoton(string nombre, Transform padre, string texto, Vector2 pos, Vector2 tam,
        Color color, UnityAction accion)
    {
        var img = CrearPanel(nombre, padre, new Vector2(0.5f, 0.5f), pos, tam, color);
        img.raycastTarget = true;
        var boton = img.gameObject.AddComponent<Button>();
        var colores = boton.colors;
        colores.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        colores.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        colores.selectedColor = new Color(1.1f, 1.1f, 1.1f);
        boton.colors = colores;
        CrearTexto("Texto", img.transform, texto, 34, Color.white);
        UnityEventTools.AddPersistentListener(boton.onClick, accion);
        return boton;
    }

    static GameObject CrearFondoOscuro(string nombre, Transform canvas)
    {
        var rt = CrearRect(nombre, canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Estirar(rt);
        var fondo = rt.gameObject.AddComponent<Image>();
        fondo.color = new Color(0.03f, 0.05f, 0.1f, 0.6f);
        return rt.gameObject;
    }

    // ----------------------------------------------------- pantallas en Game
    static void CrearPantallaFinal(Transform canvas, GameObject hud)
    {
        var raiz = new GameObject("PantallaFinal", typeof(RectTransform));
        raiz.transform.SetParent(canvas, false);
        Estirar((RectTransform)raiz.transform);
        var end = raiz.AddComponent<EndScreen>();

        var panel = CrearFondoOscuro("Panel", raiz.transform);
        var caja = CrearPanel("Caja", panel.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 520f), FondoPanel);
        var titulo = CrearTexto("Titulo", caja.transform, "¡Navidad salvada!", 80, Dorado);
        titulo.rectTransform.offsetMin = new Vector2(0f, 300f);
        titulo.rectTransform.offsetMax = new Vector2(0f, -40f);
        var detalle = CrearTexto("Detalle", caja.transform, "", 34, Color.white, TextAnchor.MiddleCenter, FontStyle.Normal);
        detalle.rectTransform.offsetMin = new Vector2(40f, 170f);
        detalle.rectTransform.offsetMax = new Vector2(-40f, -200f);

        CrearBoton("Reintentar", caja.transform, "Reintentar", new Vector2(-190f, -170f), new Vector2(320f, 90f),
            VerdeBoton, end.Reintentar);
        CrearBoton("Menu", caja.transform, "Menú", new Vector2(190f, -170f), new Vector2(320f, 90f),
            RojoBoton, end.IrAlMenu);

        var so = new SerializedObject(end);
        so.FindProperty("panel").objectReferenceValue = panel;
        so.FindProperty("titulo").objectReferenceValue = titulo;
        so.FindProperty("detalle").objectReferenceValue = detalle;
        so.FindProperty("ocultarAlTerminar").objectReferenceValue = hud;
        so.ApplyModifiedPropertiesWithoutUndo();
        panel.SetActive(false);
    }

    static void CrearPausa(Transform canvas, GameObject jugador, Camera camara)
    {
        var raiz = new GameObject("Pausa", typeof(RectTransform));
        raiz.transform.SetParent(canvas, false);
        Estirar((RectTransform)raiz.transform);
        var pausa = raiz.AddComponent<PauseMenu>();

        var panel = CrearFondoOscuro("Panel", raiz.transform);
        var caja = CrearPanel("Caja", panel.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 520f), FondoPanel);
        var titulo = CrearTexto("Titulo", caja.transform, "Pausa", 70, Dorado);
        titulo.rectTransform.offsetMin = new Vector2(0f, 380f);
        titulo.rectTransform.offsetMax = new Vector2(0f, -20f);
        CrearBoton("Continuar", caja.transform, "Continuar", new Vector2(0f, 60f), new Vector2(380f, 85f), VerdeBoton, pausa.Continuar);
        CrearBoton("Reiniciar", caja.transform, "Reiniciar", new Vector2(0f, -45f), new Vector2(380f, 85f), FondoPanel * 1.6f, pausa.Reiniciar);
        CrearBoton("Menu", caja.transform, "Menú principal", new Vector2(0f, -150f), new Vector2(380f, 85f), RojoBoton, pausa.IrAlMenu);

        var so = new SerializedObject(pausa);
        so.FindProperty("panel").objectReferenceValue = panel;
        so.FindProperty("jugador").objectReferenceValue = jugador.GetComponent<PlayerController>();
        so.FindProperty("camara").objectReferenceValue = camara.GetComponent<ThirdPersonCamera>();
        so.ApplyModifiedPropertiesWithoutUndo();
        panel.SetActive(false);
    }

    // ---------------------------------------------------------- menú principal
    [MenuItem("Duende/5. Construir escena MainMenu")]
    public static void ConstruirEscenaMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        ConstruirAmbiente();

        // Escenita de fondo: claro nevado con el duende, un muñeco de nieve y regalos girando
        var isla = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        isla.name = "Nieve";
        isla.transform.position = new Vector3(0f, -0.5f, 0f);
        isla.transform.localScale = new Vector3(14f, 0.5f, 14f);
        isla.GetComponent<Renderer>().sharedMaterial = Mat("M_Nieve");
        var mar = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mar.name = "Mar";
        mar.transform.position = new Vector3(0f, -0.35f, 0f);
        mar.transform.localScale = new Vector3(40f, 1f, 40f);
        mar.GetComponent<Renderer>().sharedMaterial = Mat("M_Hielo");

        var duende = Modelo("Duende");
        duende.transform.SetPositionAndRotation(new Vector3(1.2f, 0f, 0f), Quaternion.Euler(0f, 200f, 0f));
        duende.transform.localScale = Vector3.one * 1.6f;
        var giro = duende.AddComponent<Spinner>();
        var sg = new SerializedObject(giro);
        sg.FindProperty("gradosPorSegundo").floatValue = 35f;
        sg.FindProperty("alturaFlotar").floatValue = 0f;
        sg.ApplyModifiedPropertiesWithoutUndo();

        var posiciones = new[] { new Vector3(2.8f, 0.6f, 0.8f), new Vector3(-0.3f, 0.9f, 1.2f), new Vector3(2.4f, 2.4f, 1.5f) };
        foreach (var p in posiciones)
        {
            var regalo = Modelo("Regalo");
            regalo.transform.position = p;
            regalo.transform.localScale = Vector3.one * 1.3f;
            regalo.AddComponent<Spinner>();
        }
        foreach (var p in new[] { new Vector3(4.5f, 0f, 6f), new Vector3(7.5f, 0f, 10f), new Vector3(-9f, 0f, 14f) })
        {
            var pino = Modelo("Pino");
            pino.transform.position = p;
        }

        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        var c = cam.AddComponent<Camera>();
        c.fieldOfView = 45f;
        cam.AddComponent<AudioListener>();
        cam.transform.position = new Vector3(-0.5f, 1.9f, -6f);
        cam.transform.LookAt(new Vector3(-0.8f, 1.2f, 0f));
        CrearPostProceso(c);
        var muneco = Modelo("MunecoNieve");
        muneco.transform.SetPositionAndRotation(new Vector3(-3.2f, 0f, 4f), Quaternion.Euler(0f, 160f, 0f));
        CrearNevada(cam.transform, new Vector3(0f, 10f, 6f), 26f, 260f);

        // Interfaz
        var canvas = CrearCanvas("UI");
        var menu = canvas.gameObject.AddComponent<MainMenu>();

        var tituloRt = CrearRect("Titulo", canvas.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(110f, -110f), new Vector2(900f, 140f));
        var titulo = CrearTexto("Texto", tituloRt, "Duende Repartidor", 104, Dorado, TextAnchor.MiddleLeft);
        var borde = titulo.gameObject.AddComponent<Outline>();
        borde.effectColor = new Color(0.45f, 0.05f, 0.05f, 0.9f);
        borde.effectDistance = new Vector2(4f, -4f);
        var subRt = CrearRect("Subtitulo", canvas.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(115f, -215f), new Vector2(900f, 60f));
        var sub = CrearTexto("Texto", subRt, "Santa se enfermó: ¡reparte tú!", 44, Color.white, TextAnchor.MiddleLeft, FontStyle.Italic);
        sub.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.5f);

        var botones = CrearRect("Botones", canvas.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(110f, -60f), new Vector2(420f, 360f));
        CrearBoton("Jugar", botones, "Jugar", new Vector2(210f, 110f), new Vector2(400f, 95f), VerdeBoton, menu.Jugar);
        CrearBoton("ComoJugar", botones, "Cómo jugar", new Vector2(210f, 0f), new Vector2(400f, 95f), FondoPanel * 1.6f, menu.AlternarInstrucciones);
        CrearBoton("Salir", botones, "Salir", new Vector2(210f, -110f), new Vector2(400f, 95f), RojoBoton, menu.Salir);

        var instr = CrearPanel("Instrucciones", canvas.transform, new Vector2(1f, 0.5f), new Vector2(-470f, -40f),
            new Vector2(820f, 560f), FondoPanel);
        var tInstr = CrearTexto("Titulo", instr.transform, "Cómo jugar", 52, Dorado);
        tInstr.rectTransform.offsetMin = new Vector2(0f, 460f);
        tInstr.rectTransform.offsetMax = new Vector2(0f, -15f);
        var cuerpo = CrearTexto("Cuerpo", instr.transform,
            "Es nochebuena, está nevando y Santa amaneció con gripa.\n" +
            "Tú, su duende de confianza, tienes que repartir 8 regalos en el pueblo antes de que amanezca.\n\n" +
            "1. Ve al taller de Santa y toma un regalo.\n" +
            "2. Llévalo a la casa que tiene el haz de luz.\n" +
            "3. Cuidado con los cangrejos: si te tocan, te roban el regalo.\n\n" +
            "WASD mover · Shift correr · Espacio saltar\nMouse cámara · Esc pausa",
            28, Color.white, TextAnchor.UpperLeft, FontStyle.Normal);
        cuerpo.horizontalOverflow = HorizontalWrapMode.Wrap;
        cuerpo.rectTransform.offsetMin = new Vector2(40f, 30f);
        cuerpo.rectTransform.offsetMax = new Vector2(-40f, -110f);

        var so = new SerializedObject(menu);
        so.FindProperty("panelInstrucciones").objectReferenceValue = instr.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();
        instr.gameObject.SetActive(false);

        CrearEventSystem();
        EditorSceneManager.SaveScene(scene, MainMenuScene);
        Debug.Log("[DuendeBuilder] Escena MainMenu construida.");
    }
}
