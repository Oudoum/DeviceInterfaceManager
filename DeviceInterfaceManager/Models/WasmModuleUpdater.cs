using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DeviceInterfaceManager.Models;

public class WasmModuleUpdater
{
    private const string WasmModuleFolder = "dim-event-module";
    private const string WasmModuleName = "DIM_WASM_Module.wasm";

    private string? _communityFolder;

    public static WasmModuleUpdater Create()
    {
        return new WasmModuleUpdater();
    }

    public async Task<string> InstallWasmModule()
    {
        if (!Directory.Exists(WasmModuleFolder))
        {
            return "Folder: \"" + WasmModuleFolder + "\" could not be located in the DIM directory!";
        }

        if (!await AutoDetectCommunityFolder())
        {
            return "Community folder could not be located!";
        }

        if (!await WasmModulesAreDifferent())
        {
            return "DIM Event WASM module is up to date!";
        }

        CopyFolder(new DirectoryInfo(WasmModuleFolder), new DirectoryInfo(Path.Combine(_communityFolder!, WasmModuleFolder)));

        return "DIM Event WASM module was successfully installed!";
    }

    private async Task<bool> AutoDetectCommunityFolder()
    {
        string searchPath = SearchPath(@"Microsoft Flight Simulator\UserCfg.opt", Environment.SpecialFolder.ApplicationData);

        if (!File.Exists(searchPath))
        {
            
            searchPath = SearchPath(@"Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\UserCfg.opt", Environment.SpecialFolder.LocalApplicationData);
            if (!File.Exists(searchPath))
            {
                return false;
            }
        }

        _communityFolder = await ExtractCommunityFolderFromUserCfg(searchPath);

        return _communityFolder is not null;
    }

    private static string SearchPath(string path, Environment.SpecialFolder specialFolder)
    {
        return Path.Combine(Environment.GetFolderPath(specialFolder), path);
    }

    private static async Task<string?> ExtractCommunityFolderFromUserCfg(string userCfg)
    {
        string? installedPackagesPath = null;

        using (StreamReader file = new(userCfg))
        {
            while (await file.ReadLineAsync() is { } line)
                if (line.Contains("InstalledPackagesPath"))
                {
                    installedPackagesPath = line[23..].TrimEnd('"');
                    break;
                }
        }

        if (string.IsNullOrEmpty(installedPackagesPath))
        {
            return null;
        }

        string communityFolderPath = Path.Combine(installedPackagesPath, "Community");
        return Directory.Exists(communityFolderPath) ? communityFolderPath : null;
    }

    private async Task<bool> WasmModulesAreDifferent()
    {
        string installedWasmPath = Path.Combine(_communityFolder!, WasmModuleFolder, "modules", WasmModuleName);
        if (!File.Exists(installedWasmPath))
        {
            return true;
        }

        string dimWasmPath = Path.Combine(WasmModuleFolder, "modules", WasmModuleName);
        string installedWasm = await CalculateMd5(installedWasmPath);
        string dimWasm = await CalculateMd5(dimWasmPath);

        return installedWasm != dimWasm;
    }

    private static async Task<string> CalculateMd5(string filename)
    {
        await using FileStream stream = File.OpenRead(filename);
        byte[] hashBytes = await MD5.Create().ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);

        foreach (FileInfo fi in source.GetFiles())
        {
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
        }

        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
            CopyFolder(diSourceSubDir, nextTargetSubDir);
        }
    }
}