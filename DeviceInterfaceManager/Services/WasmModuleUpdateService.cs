using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DeviceInterfaceManager.Services;

public class WasmModuleUpdateService
{
    private const string WasmModuleFolder = "dim-event-module";
    private const string WasmModuleName = "DIM_WASM_Module.wasm";

    private string? _communityFolder;

    public static WasmModuleUpdateService Create()
    {
        return new WasmModuleUpdateService();
    }

    public async Task<string> InstallWasmModule()
    {
        if (!Directory.Exists(WasmModuleFolder))
        {
            return "Folder: \"" + WasmModuleFolder + "\" could not be located in the DIM directory!";
        }

        bool is2020Found = false;
        bool is2020Updated = false;
        if (await AutoDetectCommunityFolder("FlightSimulator"))
        {
            if (await WasmModulesAreDifferent())
            {
                CopyFolder(new DirectoryInfo(WasmModuleFolder), new DirectoryInfo(Path.Combine(_communityFolder!, WasmModuleFolder)));
                is2020Updated = true;
            }

            is2020Found = true;
        }

        bool is2024Found = false;
        bool is2024Updated = false;
        if (await AutoDetectCommunityFolder("Limitless", " 2024"))
        {
            if (await WasmModulesAreDifferent())
            {
                CopyFolder(new DirectoryInfo(WasmModuleFolder), new DirectoryInfo(Path.Combine(_communityFolder!, WasmModuleFolder)));
                is2024Updated = true;
            }

            is2024Found = true;
        }

        if (!is2020Found && !is2024Found)
        {
            return "Community folder could not be located!";
        }

        return is2024Updated switch
        {
            false when !is2020Updated => "DIM Event WASM module is up to date!",
            false when is2020Updated => "DIM Event WASM module was successfully installed for 2020!",
            true when is2020Updated => "DIM Event WASM module was successfully installed for 2020 and 2024!",
            true => "DIM Event WASM module was successfully installed for 2024!",
            _ => "DIM Event WASM module update status is unknown."
        };
    }

    private async Task<bool> AutoDetectCommunityFolder(string name, string version = "")
    {
        string searchPath = SearchPath($@"Microsoft Flight Simulator{version}\UserCfg.opt", Environment.SpecialFolder.ApplicationData);

        if (!File.Exists(searchPath))
        {
            searchPath = SearchPath($@"Packages\Microsoft.{name}_8wekyb3d8bbwe\LocalCache\UserCfg.opt", Environment.SpecialFolder.LocalApplicationData);
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
        return Convert.ToHexStringLower(hashBytes);
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