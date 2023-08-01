using Microsoft.Win32;

namespace WindDragon;

internal class Program
{
    private static readonly string[] FilesToMatch =
    {
        "bak",
        "prev",
        "backup",
        ".js",
        "json"
    };

    private static readonly string[] FoldersToMatch =
    {
        "bak",
        "prev",
        "backup",
        "dcx",
        "recovery",
        "_DSAS_CACHE"
    };

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
        string[] directories = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        files = files.Where(i => !string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(i))).ToArray();
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            if (!File.Exists(file) || !FilesToMatch.Any(fileName.Contains)) continue;
            DeleteFile(file);
        }
        foreach (string dir in directories)
        {
            string dirName = new DirectoryInfo(dir).Name;
            if (!Directory.Exists(dir) || !FoldersToMatch.Any(dirName.Contains)) continue;
            DeleteDirectory(dir);
        }
        Console.WriteLine("Mod folder cleanup complete!");
        Console.ReadKey();
    }

    private static void MainMenu(IReadOnlyList<string> args)
    {
        while (true)
        {
            Console.Clear();
            if (args.ElementAtOrDefault(0) != null)
            {
                CleanModFolder(args[0]);
            }
            else
            {
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
            break;
        }
    }

    private static void Main(string[] args)
    {
        MainMenu(args);
    }
}