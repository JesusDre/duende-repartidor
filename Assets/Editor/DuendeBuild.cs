using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Issue #13: genera los ejecutables del juego en la carpeta Builds/.
/// Uso en batch: -executeMethod DuendeBuild.Windows  (o DuendeBuild.Linux)
/// </summary>
public static class DuendeBuild
{
    static readonly string[] Escenas = { DuendeBuilder.MainMenuScene, DuendeBuilder.GameScene };

    [MenuItem("Duende/Build/Windows")]
    public static void Windows() =>
        Construir(BuildTarget.StandaloneWindows64, "Builds/Windows/DuendeRepartidor.exe");

    [MenuItem("Duende/Build/Linux")]
    public static void Linux() =>
        Construir(BuildTarget.StandaloneLinux64, "Builds/Linux/DuendeRepartidor.x86_64");

    static void Construir(BuildTarget destino, string ruta)
    {
        PlayerSettings.companyName = "UTEZ";
        PlayerSettings.productName = "Duende Repartidor";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;

        Directory.CreateDirectory(Path.GetDirectoryName(ruta));
        var opciones = new BuildPlayerOptions
        {
            scenes = Escenas,
            locationPathName = ruta,
            target = destino,
            options = BuildOptions.None,
        };
        var reporte = BuildPipeline.BuildPlayer(opciones);
        var resumen = reporte.summary;
        Debug.Log($"[DuendeBuild] {destino}: {resumen.result}, {resumen.totalSize / (1024 * 1024)} MB, " +
                  $"{resumen.totalErrors} errores → {Path.GetFullPath(ruta)}");
        if (resumen.result != BuildResult.Succeeded && Application.isBatchMode) EditorApplication.Exit(1);
    }
}
