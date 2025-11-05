using System.Diagnostics;
using System.ServiceProcess;

namespace StarResonanceDpsAnalysis.WinForm.Plugin
{
    public static class NpcapInstaller
    {
        /// <summary>
        /// 仅用于 Npcap 0.96 等支持 /S 的版本。新版社区版(≥0.97)不支持静默安装。
        /// </summary>
        public static async Task<int> InstallNpcapSilentAsync(
            string installerPath,
            string extraArgs = "/winpcap_mode=yes /loopback_support=yes /admin_only=no")
        {
            if (!File.Exists(installerPath))
                throw new FileNotFoundException("未找到 Npcap 安装包", installerPath);

            // 可选：防呆——检测版本，避免对新版本传 /S 弹窗卡死
            var ver = TryGetVersion(installerPath);
            if (ver != null && ver > new Version(0, 96))
                throw new InvalidOperationException($"该安装包版本为 {ver}，社区版不支持静默安装。");

            var psi = new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = $"/S {extraArgs}",
                UseShellExecute = true,
                Verb = "runas" // 需要管理员权限
            };

            using var p = Process.Start(psi)!;
            await p.WaitForExitAsync();
            return p.ExitCode; // 通常 0 为成功
        }

        public static bool IsNpcapInstalled()
            => ServiceController.GetServices()
               .Any(s => s.ServiceName.Equals("npcap", StringComparison.OrdinalIgnoreCase));

        public static async Task<int> UninstallNpcapSilentAsync()
        {
            string uninst = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Npcap", "uninstall.exe");

            if (!File.Exists(uninst)) return 0;

            var psi = new ProcessStartInfo
            {
                FileName = uninst,
                Arguments = "/S",
                UseShellExecute = true,
                Verb = "runas"
            };
            using var p = Process.Start(psi)!;
            await p.WaitForExitAsync();
            return p.ExitCode;
        }

        private static Version? TryGetVersion(string path)
        {
            var fvi = FileVersionInfo.GetVersionInfo(path);
            string v = fvi.FileVersion ?? fvi.ProductVersion ?? "";
            var cleaned = new string((v + ".0.0.0").TakeWhile(ch => char.IsDigit(ch) || ch == '.').ToArray());
            return Version.TryParse(cleaned, out var ver) ? ver : null;
        }
    }
}
