namespace WindDragon;

internal class Program
{
    private static readonly string[] FilesToMatch =
    {
        "bak",
        "prev",
        "emevd.dcx.js",
        "_DSAS_PROJECT.json",
        "esdtoolconfig.json"
    };

    private static readonly string[] FoldersToMatch =
    {
        "recovery",
        "_DSAS_CACHE"
    };

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
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            foreach (string dir in directories)
            {
                string dirName = new DirectoryInfo(dir).Name;
                if (!Directory.Exists(dir) || !dirName.Contains(fileName)) continue;
                DeleteDirectory(dir);
            }
        }
        Console.WriteLine("Mod folder cleanup complete!");
        Console.ReadKey();
    }

    private static void Main(string[] args)
    {
        if (args.ElementAtOrDefault(0) != null)
        {
            CleanModFolder(args[0]);
        }
        else
        {
            string? folderPath;
            do
            {
                Console.Write("Please specify a desired mod folder path: ");
                folderPath = Console.ReadLine();
                if (string.IsNullOrEmpty(folderPath)) Console.Clear();
                else break;
            } while (string.IsNullOrEmpty(folderPath));
            CleanModFolder(folderPath);
        }
    }
}