using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento del duende en tercera persona: camina relativo a la cámara, gira suave
/// hacia donde se mueve, salta con Espacio y se balancea al caminar.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float velocidad = 6f;
    [SerializeField] float velocidadCorrer = 9f;
    [SerializeField] float aceleracion = 12f;
    [SerializeField] float velocidadGiro = 720f;

    [Header("Salto y gravedad")]
    [SerializeField] float alturaSalto = 1.4f;
    [SerializeField] float gravedad = -25f;

    [Header("Animación simple")]
    [Tooltip("Hijo con el modelo; se balancea al caminar.")]
    [SerializeField] Transform modelo;
    [SerializeField] float frecuenciaPaso = 9f;
    [SerializeField] float alturaPaso = 0.06f;
    [SerializeField] float inclinacionPaso = 6f;

    CharacterController controller;
    Transform camara;
    Vector3 velocidadPlano;
    float velocidadVertical;
    float fasePaso;
    Vector3 posModeloInicial;

    /// <summary>Se puede desactivar el control (por ejemplo al terminar la partida).</summary>
    public bool PuedeMoverse { get; set; } = true;

    /// <summary>Velocidad horizontal actual (la usan otros sistemas, como la cámara).</summary>
    public float VelocidadActual => velocidadPlano.magnitude;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (modelo != null) posModeloInicial = modelo.localPosition;
    }

    void Start()
    {
        if (Camera.main != null) camara = Camera.main.transform;
    }

    void Update()
    {
        Vector2 entrada = LeerMovimiento();
        bool correr = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        // Dirección relativa a la cámara (solo en el plano horizontal)
        Vector3 adelante = camara != null ? Vector3.ProjectOnPlane(camara.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 derecha = camara != null ? Vector3.ProjectOnPlane(camara.right, Vector3.up).normalized : Vector3.right;
        Vector3 deseada = (adelante * entrada.y + derecha * entrada.x);
        if (deseada.sqrMagnitude > 1f) deseada.Normalize();
        if (!PuedeMoverse) deseada = Vector3.zero;

        float vel = correr ? velocidadCorrer : velocidad;
        velocidadPlano = Vector3.MoveTowards(velocidadPlano, deseada * vel, aceleracion * vel * Time.deltaTime);

        if (deseada.sqrMagnitude > 0.01f)
        {
            var rot = Quaternion.LookRotation(deseada);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, velocidadGiro * Time.deltaTime);
        }

        // Gravedad y salto
        if (controller.isGrounded)
        {
            if (velocidadVertical < 0f) velocidadVertical = -2f;
            if (PuedeMoverse && SaltoPresionado())
                velocidadVertical = Mathf.Sqrt(alturaSalto * -2f * gravedad);
        }
        velocidadVertical += gravedad * Time.deltaTime;

        Vector3 movimiento = velocidadPlano + Vector3.up * velocidadVertical;
        controller.Move(movimiento * Time.deltaTime);

        AnimarPaso();
    }

    Vector2 LeerMovimiento()
    {
        Vector2 v = Vector2.zero;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.y -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1f;
        }
        var pad = Gamepad.current;
        if (pad != null && pad.leftStick.ReadValue().sqrMagnitude > v.sqrMagnitude)
            v = pad.leftStick.ReadValue();
        return v;
    }

    bool SaltoPresionado()
    {
        return (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
               || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }

    void AnimarPaso()
    {
        if (modelo == null) return;
        float t = Mathf.Clamp01(velocidadPlano.magnitude / velocidad);
        if (controller.isGrounded && t > 0.05f)
            fasePaso += Time.deltaTime * frecuenciaPaso * Mathf.Lerp(0.6f, 1.3f, t);

        float rebote = Mathf.Abs(Mathf.Sin(fasePaso)) * alturaPaso * t;
        float balanceo = Mathf.Sin(fasePaso) * inclinacionPaso * t;
        modelo.localPosition = posModeloInicial + Vector3.up * rebote;
        modelo.localRotation = Quaternion.Euler(t * 5f, 0f, balanceo);
    }

    /// <summary>Coloca al jugador en una posición sin que el CharacterController lo regrese.</summary>
    public void Teletransportar(Vector3 posicion, Quaternion rotacion)
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(posicion, rotacion);
        velocidadPlano = Vector3.zero;
        velocidadVertical = 0f;
        controller.enabled = true;
    }
}
