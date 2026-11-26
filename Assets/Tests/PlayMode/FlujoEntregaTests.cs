using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Pruebas del ciclo principal: recoger un regalo en el taller y entregarlo en la casa destino.
/// </summary>
public class FlujoEntregaTests
{
    PlayerController jugador;
    GiftCarrier carrier;
    Workshop taller;
    DeliveryManager entregas;

    [UnitySetUp]
    public IEnumerator CargarEscena()
    {
        yield return SceneManager.LoadSceneAsync("Game");
        yield return null;
        jugador = Object.FindAnyObjectByType<PlayerController>();
        carrier = Object.FindAnyObjectByType<GiftCarrier>();
        taller = Object.FindAnyObjectByType<Workshop>();
        entregas = Object.FindAnyObjectByType<DeliveryManager>();
    }

    IEnumerator IrA(Vector3 pos)
    {
        jugador.Teletransportar(pos + Vector3.up * 0.1f, jugador.transform.rotation);
        for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
        yield return null;
    }

    [UnityTest]
    public IEnumerator LaEscenaTieneLasPiezasDelJuego()
    {
        Assert.IsNotNull(jugador, "Falta el jugador");
        Assert.IsNotNull(carrier, "Falta GiftCarrier en el jugador");
        Assert.IsNotNull(taller, "Falta la zona del taller");
        Assert.IsNotNull(entregas, "Falta DeliveryManager");
        Assert.AreEqual(6, entregas.Casas.Count);
        Assert.IsNotNull(entregas.Destino, "No se eligió casa destino");
        Assert.IsFalse(carrier.TieneRegalo);
        yield break;
    }

    [UnityTest]
    public IEnumerator RecogerEnTallerYEntregarEnCasaDestino()
    {
        yield return IrA(taller.transform.position);
        Assert.IsTrue(carrier.TieneRegalo, "El duende no tomó el regalo en el taller");

        var destino = entregas.Destino;
        yield return IrA(destino.transform.position);
        Assert.IsFalse(carrier.TieneRegalo, "El regalo no se entregó");
        Assert.AreEqual(1, entregas.Entregados);
        Assert.AreNotEqual(destino, entregas.Destino, "No se eligió una casa nueva");
    }

    [UnityTest]
    public IEnumerator UnaCasaQueNoEsDestinoNoRecibeRegalo()
    {
        yield return IrA(taller.transform.position);
        DeliveryHouse otra = null;
        foreach (var casa in entregas.Casas)
            if (casa != entregas.Destino) { otra = casa; break; }

        yield return IrA(otra.transform.position);
        Assert.IsTrue(carrier.TieneRegalo);
        Assert.AreEqual(0, entregas.Entregados);
    }

    [UnityTest]
    public IEnumerator ElJugadorCaminaHaciaAdelanteSinAtravesarElSuelo()
    {
        var inicio = jugador.transform.position;
        for (int i = 0; i < 30; i++) yield return null;
        Assert.Greater(jugador.transform.position.y, -0.2f, "El duende cayó a través del suelo");
        Assert.Less(Vector3.Distance(inicio, jugador.transform.position), 1f, "El duende se movió sin entrada");
    }
}

/// <summary>Pruebas de las condiciones de victoria y derrota (issue #8).</summary>
public class GameManagerTests
{
    [UnitySetUp]
    public IEnumerator CargarEscena()
    {
        yield return SceneManager.LoadSceneAsync("Game");
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeEmpiezaConTiempoCompletoYCeroRegalos()
    {
        var gm = GameManager.Instance;
        Assert.IsNotNull(gm);
        Assert.AreEqual(GameManager.Estado.Jugando, gm.EstadoActual);
        Assert.AreEqual(0, gm.Entregados);
        Assert.Greater(gm.TiempoRestante, gm.Duracion - 1f);
        yield break;
    }

    [UnityTest]
    public IEnumerator SePierdeCuandoSeAcabaElTiempo()
    {
        var gm = GameManager.Instance;
        GameManager.Estado? resultado = null;
        gm.PartidaTerminada += e => resultado = e;
        gm.FijarTiempoRestante(0.05f);
        yield return new WaitForSeconds(0.2f);
        Assert.AreEqual(GameManager.Estado.Derrota, gm.EstadoActual);
        Assert.AreEqual(GameManager.Estado.Derrota, resultado);
        Assert.IsFalse(Object.FindAnyObjectByType<PlayerController>().PuedeMoverse);
    }

    [UnityTest]
    public IEnumerator SeGanaAlEntregarLaMeta()
    {
        var gm = GameManager.Instance;
        var jugador = Object.FindAnyObjectByType<PlayerController>();
        var taller = Object.FindAnyObjectByType<Workshop>();
        var entregas = Object.FindAnyObjectByType<DeliveryManager>();

        for (int i = 0; i < gm.Meta; i++)
        {
            jugador.Teletransportar(taller.transform.position + Vector3.up * 0.1f, Quaternion.identity);
            for (int f = 0; f < 6; f++) yield return new WaitForFixedUpdate();
            jugador.Teletransportar(entregas.Destino.transform.position + Vector3.up * 0.1f, Quaternion.identity);
            for (int f = 0; f < 6; f++) yield return new WaitForFixedUpdate();
        }
        yield return null;
        Assert.AreEqual(gm.Meta, gm.Entregados);
        Assert.AreEqual(GameManager.Estado.Victoria, gm.EstadoActual);
    }
}

/// <summary>Pruebas de menús (issue #10).</summary>
public class MenusTests
{
    [UnityTest]
    public IEnumerator JugarDesdeElMenuCargaElJuego()
    {
        yield return SceneManager.LoadSceneAsync("MainMenu");
        yield return null;
        Object.FindAnyObjectByType<MainMenu>().Jugar();
        yield return null;
        yield return null;
        Assert.AreEqual("Game", SceneManager.GetActiveScene().name);
    }

    [UnityTest]
    public IEnumerator PausaCongelaYContinuarReanuda()
    {
        yield return SceneManager.LoadSceneAsync("Game");
        yield return null;
        var pausa = Object.FindAnyObjectByType<PauseMenu>();
        pausa.Pausar();
        Assert.AreEqual(0f, Time.timeScale);
        Assert.IsFalse(Object.FindAnyObjectByType<PlayerController>().PuedeMoverse);
        pausa.Continuar();
        Assert.AreEqual(1f, Time.timeScale);
        Assert.IsTrue(Object.FindAnyObjectByType<PlayerController>().PuedeMoverse);
    }

    [UnityTest]
    public IEnumerator AlPerderSeMuestraLaPantallaFinalYReintentarReinicia()
    {
        yield return SceneManager.LoadSceneAsync("Game");
        yield return null;
        GameManager.Instance.FijarTiempoRestante(0.01f);
        yield return null;
        yield return null;
        var final = Object.FindAnyObjectByType<EndScreen>();
        Assert.IsTrue(final.transform.Find("Panel").gameObject.activeSelf, "No apareció la pantalla final");
        final.Reintentar();
        yield return null;
        yield return null;
        Assert.AreEqual(GameManager.Estado.Jugando, GameManager.Instance.EstadoActual);
        Assert.Greater(GameManager.Instance.TiempoRestante, 100f);
    }
}
