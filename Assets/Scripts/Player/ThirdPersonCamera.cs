using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cámara orbital en tercera persona: gira con el mouse (o stick derecho), sigue al
/// objetivo con suavidad y se acerca si hay una pared entre la cámara y el jugador.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Transform objetivo;
    [SerializeField] Vector3 offsetObjetivo = new Vector3(0f, 1.2f, 0f);

    [Header("Distancia")]
    [SerializeField] float distancia = 5.5f;
    [SerializeField] float distanciaMinima = 1.2f;
    [SerializeField] float radioColision = 0.25f;
    [SerializeField] LayerMask capasColision = ~0;

    [Header("Rotación")]
    [SerializeField] float sensibilidadMouse = 0.12f;
    [SerializeField] float sensibilidadStick = 160f;
    [SerializeField] float inclinacionMin = -10f;
    [SerializeField] float inclinacionMax = 65f;
    [SerializeField] float suavizado = 12f;

    float yaw;
    float pitch = 18f;
    float distanciaActual;

    /// <summary>Cuando es falso la cámara no lee el mouse y el cursor queda libre (menús).</summary>
    public bool ControlActivo { get; set; } = true;

    public Transform Objetivo
    {
        get => objetivo;
        set => objetivo = value;
    }

    void Start()
    {
        if (objetivo != null) yaw = objetivo.eulerAngles.y;
        distanciaActual = distancia;
        ColocarInmediato();
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        ActualizarCursor();
        if (ControlActivo) LeerRotacion();

        var rot = Quaternion.Euler(pitch, yaw, 0f);
        var foco = objetivo.position + offsetObjetivo;

        // Acerca la cámara si algo bloquea la vista
        float distDeseada = distancia;
        if (Physics.SphereCast(foco, radioColision, rot * Vector3.back, out var hit, distancia,
                capasColision, QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(objetivo))
        {
            distDeseada = Mathf.Max(distanciaMinima, hit.distance - 0.1f);
        }
        // Acercarse rápido, alejarse suave
        float k = distDeseada < distanciaActual ? 30f : 4f;
        distanciaActual = Mathf.Lerp(distanciaActual, distDeseada, 1f - Mathf.Exp(-k * Time.deltaTime));

        var posDeseada = foco + rot * Vector3.back * distanciaActual;
        transform.position = Vector3.Lerp(transform.position, posDeseada, 1f - Mathf.Exp(-suavizado * Time.deltaTime));
        transform.rotation = Quaternion.LookRotation(foco - transform.position);
    }

    void LeerRotacion()
    {
        Vector2 delta = Vector2.zero;
        if (Mouse.current != null) delta += Mouse.current.delta.ReadValue() * sensibilidadMouse;
        if (Gamepad.current != null) delta += Gamepad.current.rightStick.ReadValue() * sensibilidadStick * Time.deltaTime;

        yaw += delta.x;
        pitch = Mathf.Clamp(pitch - delta.y, inclinacionMin, inclinacionMax);
    }

    void ActualizarCursor()
    {
        var lockMode = ControlActivo ? CursorLockMode.Locked : CursorLockMode.None;
        if (Cursor.lockState != lockMode)
        {
            Cursor.lockState = lockMode;
            Cursor.visible = !ControlActivo;
        }
    }

    public void ColocarInmediato()
    {
        if (objetivo == null) return;
        var rot = Quaternion.Euler(pitch, yaw, 0f);
        var foco = objetivo.position + offsetObjetivo;
        transform.position = foco + rot * Vector3.back * distancia;
        transform.rotation = Quaternion.LookRotation(foco - transform.position);
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
