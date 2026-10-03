using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ScramblyFoxDefense.EditorTools
{
    /// <summary>
    /// Size-focused WebGL player settings and a one-click build that reports output sizes.
    /// Brief constraints: static HTTP server without compression headers, so Brotli + Decompression Fallback.
    /// </summary>
    public static class BuildTools
    {
        public const string OutputDir = "Builds/WebGL";

        [MenuItem("Scrambly/Apply WebGL Size Settings")]
        public static void ApplySizeSettings()
        {
            var target = NamedBuildTarget.WebGL;

            PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.High);
            PlayerSettings.SetIl2CppCodeGeneration(target, Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.SetIl2CppCompilerConfiguration(target, Il2CppCompilerConfiguration.Master);
            PlayerSettings.stripEngineCode = true;

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.dataCaching = false;
            PlayerSettings.WebGL.nameFilesAsHashes = false;
            PlayerSettings.WebGL.showDiagnostics = false;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            PlayerSettings.WebGL.template = "APPLICATION:Minimal";

            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false; // otherwise the 2.8 MB logo texture ships anyway
            PlayerSettings.runInBackground = false;
            PlayerSettings.companyName = "Cayo Aguiar";
            PlayerSettings.productName = "Scrambly Fox Defense";

            UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;

            AssetDatabase.SaveAssets();
            Debug.Log("[BuildTools] WebGL size settings applied.");
        }

        [MenuItem("Scrambly/Build WebGL")]
        public static void BuildWebGL()
        {
            ApplySizeSettings();

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildTools] No enabled scenes in Build Settings.");
                return;
            }

            if (Directory.Exists(OutputDir)) Directory.Delete(OutputDir, true);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildTools] Build {summary.result} with {summary.totalErrors} errors.");
                return;
            }

            var text = DescribeOutput(report);
            File.WriteAllText(Path.Combine("Builds", "size-report.txt"), text);
            Debug.Log(text);
        }

        static string DescribeOutput(BuildReport report)
        {
            var sb = new StringBuilder();
            long total = 0;
            sb.AppendLine("[BuildTools] WebGL output (bytes):");
            foreach (var file in Directory.GetFiles(OutputDir, "*", SearchOption.AllDirectories).OrderByDescending(f => new FileInfo(f).Length))
            {
                long len = new FileInfo(file).Length;
                total += len;
                sb.AppendLine($"  {len,10}  {file.Replace('\\', '/').Substring(OutputDir.Length + 1)}");
            }
            sb.AppendLine($"  {total,10}  TOTAL");

            sb.AppendLine("Largest packed assets (uncompressed bytes):");
            var assets = report.packedAssets.SelectMany(p => p.contents)
                .GroupBy(c => c.sourceAssetPath)
                .Select(g => (path: g.Key, size: g.Sum(c => (long)c.packedSize)))
                .OrderByDescending(a => a.size)
                .Take(15);
            foreach (var a in assets) sb.AppendLine($"  {a.size,10}  {a.path}");
            return sb.ToString();
        }
    }
}
