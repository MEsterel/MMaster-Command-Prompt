using MMaster.Properties;
using System;
using System.IO;
using System.Text;

namespace MMaster.Commands
{
  public static class CmdMngr
  {
    public const string _templateFileNameWithoutExtension = "MMaster Command File";

    [MMasterCommand("Reload '*.cs' external commands located in the application's directory.", false)]
    public static void Reload()
    {
      CommandsManager.ClearExternalCommands();
      CFormat.WriteLine("[CommandManager] Cleared loaded external commands.", ConsoleColor.Gray);
      CommandsManager.LoadExternalCommands();
    }

    [MMasterCommand("Load a file of external commands.", false)]
    public static void LoadFile(string path)
    {
      CommandsManager.LoadFile(path, true);
    }

    [MMasterCommand("Load '*.cs' files of external commands in a directory.", false)]
    public static void LoadDirectory(string path = null)
    {
      CommandsManager.LoadDirectory(path, true);
    }

    [MMasterCommand("Unload loaded external commands.", false)]
    public static void Unload()
    {
      CommandsManager.ClearExternalCommands();
      CFormat.WriteLine("[CommandManager] Cleared loaded external commands.", ConsoleColor.Gray);
    }

    [MMasterCommand("Get the list of the loaded files.", false)]
    public static void LoadedFiles()
    {
      if (CommandsManager._listLoadedFiles.Count == 0)
      {
        CFormat.WriteLine("[CommandManager] There are no external files loaded.", ConsoleColor.Gray);
      }
      else
      {
        CFormat.WriteLine("[CommandManager] List of loaded files: ", ConsoleColor.Gray);
        foreach (FileID listLoadedFile in CommandsManager._listLoadedFiles)
          CFormat.WriteLine(string.Format("{0}({1}) {2}", (object) CFormat.Indent(2), (object) listLoadedFile.ID, (object) listLoadedFile.Path), ConsoleColor.Gray);
      }
    }

    [MMasterCommand("Unload a file.", false)]
    public static void UnloadFile()
    {
      CmdMngr.LoadedFiles();
      CFormat.JumpLine();
      CFormat.WriteLine("Please enter the number ID of the file you want to unload.", ConsoleColor.Gray);
      int id = CInput.UserPickInt(CommandsManager._listLoadedFiles.Count - 1);
      if (id == -1)
        return;
      CommandsManager.UnloadFile(id);
    }

    [MMasterCommand("Create a template file of external commands. If path is not specified, file is saved in application's directory.", false)]
    public static void CreateTemplate(string path = null)
    {
      try
      {
        if (path == null)
          path = "MMaster Command File" + Path.GetExtension("*.cs");
        CFormat.WriteLine("[CommandManager] Creating template command file \"" + path + "\"", ConsoleColor.Gray);
        if (File.Exists(path))
        {
          CFormat.WriteLine("[CommandManager] A file named \"" + path + "\" already exists. Replace it?", ConsoleColor.Gray);
          if (CInput.UserChoice(ConsoleAnswerType.YesNo) == ConsoleAnswer.Yes)
          {
            using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
              streamWriter.Write(Settings.Default.FileTemplate);
          }
          else
          {
            int num = 1;
            string withoutExtension = Path.GetFileNameWithoutExtension(path);
            while (File.Exists(path))
            {
              path = Path.Combine(Path.GetDirectoryName(path), withoutExtension + " (" + (object) num + ")", Path.GetExtension(path));
              ++num;
            }
            using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
              streamWriter.Write(Settings.Default.FileTemplate);
          }
        }
        else
        {
          using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
            streamWriter.Write(Settings.Default.FileTemplate);
        }
        CFormat.WriteLine("[CommandManager] Template command file named \"" + path + "\" created!", ConsoleColor.Gray);
      }
      catch (Exception ex)
      {
        CFormat.WriteLine("[CommandManager] Could not create template file \"" + path + "\" Details: " + ex.Message, ConsoleColor.Red);
      }
    }
  }
}
