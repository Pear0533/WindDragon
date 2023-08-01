using Microsoft.Win32;
using System.Reflection;

namespace WindDragon;

internal class Program
{
    private static readonly string AppRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
    private static readonly string BlacklistedFilesConfig = $"{AppRootPath}\\configs\\blacklisted_files.wdconfig";
    private static readonly string BlacklistedFoldersConfig = $"{AppRootPath}\\configs\\blacklisted_folders.wdconfig";
    private static readonly string WhitelistedFilesConfig = $"{AppRootPath}\\configs\\whitelisted_files.wdconfig";
    private static readonly string WhitelistedFoldersConfig = $"{AppRootPath}\\configs\\whitelisted_folders.wdconfig";

    private static List<string> BlacklistedFiles = new();
    private static List<string> BlacklistedFolders = new();
    private static List<string> WhitelistedFiles = new();
    private static List<string> WhitelistedFolders = new();

    private const string ClassesRegistryKey = "Software\\Classes";

    private static void RegisterContextMenu(IReadOnlyList<string> args)
    {
        Console.Write("Enter R to register, U to unregister, or anything else to exit from context menu registration.\n> ");
        string? option = Console.ReadLine();
        if (string.IsNullOrEmpty(option) || option != "R" && option != "U")
        {
            MainMenu(args);
            return;
        }
        option = option.ToUpper();
        Console.WriteLine("\n");
        try
        {
            RegistryKey? classes = Registry.CurrentUser.OpenSubKey(ClassesRegistryKey, true);
            if (classes == null)
            {
                Console.WriteLine($"Could not find the {ClassesRegistryKey} registry key.\n");
                Console.ReadKey();
                MainMenu(args);
                return;
            }
            switch (option)
            {
                case "R":
                {
                    string exeFilePath = Path.GetFullPath("WindDragon.exe");
                    RegistryKey exeFileRegKey = classes.CreateSubKey("*\\shell\\winddragon");
                    RegistryKey exeFileCommand = exeFileRegKey.CreateSubKey("command");
                    exeFileRegKey.SetValue(null, "WindDragon");
                    exeFileCommand.SetValue(null, $"\"{exeFilePath}\" \"%1\"");
                    RegistryKey exeDirKey = classes.CreateSubKey("directory\\shell\\winddragon");
                    RegistryKey exeDirCommand = exeDirKey.CreateSubKey("command");
                    exeDirKey.SetValue(null, "Clean with WindDragon");
                    exeDirCommand.SetValue(null, $"\"{exeFilePath}\" \"%1\"");
                    Console.WriteLine("WindDragon has been registered to the context menu!");
                    break;
                }
                case "U":
                    classes.DeleteSubKeyTree("*\\shell\\winddragon", false);
                    classes.DeleteSubKeyTree("directory\\shell\\winddragon", false);
                    Console.WriteLine("WindDragon has been unregistered from the context menu.");
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Context menu registration could not be changed.\n\nReason:\n\n{e}");
        }
        Console.ReadKey();
        MainMenu(args);
    }

    private static void DeleteDirectory(string path)
    {
        try
        {
            string dirName = new DirectoryInfo(path).Name;
            Console.WriteLine($"Deleting directory {dirName}...");
            Directory.Delete(path, true);
        }
        catch
        {
            Console.WriteLine($"Directory {Path.GetFileName(path)} could not be deleted.");
        }
    }

    private static void DeleteFile(string path)
    {
        try
        {
            string fileName = Path.GetFileName(path);
            Console.WriteLine($"Deleting file {fileName}...");
            File.Delete(path);
        }
        catch
        {
            Console.WriteLine($"File {Path.GetFileName(path)} could not be deleted.");
        }
    }

    private static void CleanModFolder(string path)
    {
        string modFolderName = new DirectoryInfo(path).Name;
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Mod folder {modFolderName} does not exist.");
            Console.ReadKey();
            return;
        }
        string[] directories = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        files = files.Where(i => !string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(i))).ToArray();
        files = files.Where(i => i.Split(Path.DirectorySeparatorChar).All(t => !BlacklistedFolders.Any(t.Contains))).ToArray();
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            if (BlacklistedFiles.Any(fileName.Contains)) continue;
            if (!File.Exists(file) || !WhitelistedFiles.Any(fileName.Contains)) continue;
            DeleteFile(file);
        }
        foreach (string dir in directories)
        {
            string dirName = new DirectoryInfo(dir).Name;
            if (BlacklistedFolders.Contains(dirName)) continue;
            if (!Directory.Exists(dir) || !WhitelistedFolders.Any(dirName.Contains)) continue;
            DeleteDirectory(dir);
        }
        Console.WriteLine("Mod folder cleanup complete!");
        Console.ReadKey();
    }

    private static List<string> ReadConfig(string path)
    {
        return File.ReadAllText(path).Split("\r\n").Where(i => !string.IsNullOrEmpty(i)).ToList();
    }

    private static bool ReadConfigs()
    {
        try
        {
            Console.Clear();
            Console.WriteLine("Reading WindDragon configuration files...\n");
            BlacklistedFiles = ReadConfig(BlacklistedFilesConfig);
            BlacklistedFolders = ReadConfig(BlacklistedFoldersConfig);
            WhitelistedFiles = ReadConfig(WhitelistedFilesConfig);
            WhitelistedFolders = ReadConfig(WhitelistedFoldersConfig);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to read one or more required configuration files.\n\nReason:\n\n{e}");
            return false;
        }
    }

    private static void MainMenu(IReadOnlyList<string> args)
    {
        while (true)
        {
            if (!ReadConfigs()) break;
            Console.Clear();
            if (args.ElementAtOrDefault(0) != null)
            {
                CleanModFolder(args[0]);
                break;
            }
            Console.WriteLine("Welcome to WindDragon!\n");
            Console.WriteLine("This tool is specially crafted to effortlessly tidy up DS3/ER mod folders.\n");
            Console.Write("Type 0 to specify a mod folder for cleaning, or 1 to modify WindDragon context menu registration.\n> ");
            string? option = Console.ReadLine();
            Console.WriteLine("\n");
            switch (option)
            {
                case "0":
                {
                    Console.Write("Please specify a desired mod folder path:\n> ");
                    string? folderPath = Console.ReadLine();
                    Console.WriteLine("\n");
                    if (string.IsNullOrEmpty(folderPath))
                    {
                        continue;
                    }
                    CleanModFolder(folderPath);
                    break;
                }
                case "1":
                    RegisterContextMenu(args);
                    break;
            }
        }
    }

    private static void Main(string[] args)
    {
        MainMenu(args);
    }
}