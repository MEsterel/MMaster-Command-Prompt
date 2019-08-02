using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MMaster.Commands
{
  public static class Default
  {
    [MMasterCommand("Get the help prompt for a specific command.", false)]
    public static void Help(string command = null)
    {
      if (command == null)
      {
        Default.List();
      }
      else
      {
        CRawInputCommand rawInput = new CRawInputCommand(command);
        if (rawInput.LibraryClassType == (Type) null)
        {
          CFormat.WriteLine("Help: This command does not exist.", ConsoleColor.Gray);
        }
        else
        {
          Dictionary<string, MethodInfo> source;
          if (CommandsManager._internalCommandLibraries.ContainsKey(rawInput.LibraryClassType))
            source = CommandsManager._internalCommandLibraries[rawInput.LibraryClassType];
          else if (CommandsManager._externalCommandLibraries.ContainsKey(rawInput.LibraryClassType))
          {
            source = CommandsManager._externalCommandLibraries[rawInput.LibraryClassType];
          }
          else
          {
            CFormat.WriteLine("Help: This command does not exist.", ConsoleColor.Gray);
            return;
          }
          if (source.Any<KeyValuePair<string, MethodInfo>>((Func<KeyValuePair<string, MethodInfo>, bool>) (i => i.Key.ToLower().Equals(rawInput.Name.ToLower()))))
          {
            rawInput.Name = source.Keys.Where<string>((Func<string, bool>) (i => i.ToLower().Equals(rawInput.Name.ToLower()))).ToArray<string>()[0];
            MethodInfo method = rawInput.LibraryClassType.GetMethod(rawInput.Name);
            object[] array = ((IEnumerable<object>) method.GetCustomAttributes(false)).Where<object>((Func<object, bool>) (a => a.GetType().Name == typeof (MMasterCommand).Name)).ToArray<object>();
            MMasterCommand mmasterCommand;
            try
            {
              mmasterCommand = (MMasterCommand) array[0];
            }
            catch
            {
              CFormat.WriteLine("Help: This command does not exist.", ConsoleColor.Gray);
              return;
            }
            if (mmasterCommand.HelpPrompt == "")
            {
              CFormat.WriteLine(CFormat.GetArgsFormat(rawInput.FullName, (IEnumerable<ParameterInfo>) method.GetParameters()), ConsoleColor.Gray);
            }
            else
            {
              StringBuilder stringBuilder = new StringBuilder();
              stringBuilder.AppendLine(mmasterCommand.HelpPrompt);
              stringBuilder.Append(CFormat.GetArgsFormat(rawInput.FullName, (IEnumerable<ParameterInfo>) method.GetParameters()));
              CFormat.WriteLine(stringBuilder.ToString(), ConsoleColor.Gray);
            }
          }
          else
            CFormat.WriteLine("This command does not exist.", ConsoleColor.Gray);
        }
      }
    }

    [MMasterCommand("Get the list of available commands.", false)]
    public static void List()
    {
      CFormat.WriteLine("For more information about a command, type 'Help <command>'.", ConsoleColor.Gray);
      CFormat.JumpLine();
      CFormat.WriteLine("[Internal commands]", ConsoleColor.Green);
      foreach (Type index in CommandsManager._internalLibraryTypes.Values)
      {
        if (CommandsManager._internalCommandLibraries[index].Values.Count != 0)
        {
          CFormat.WriteLine(index.Name, ConsoleColor.Yellow);
          foreach (MethodInfo methodInfo in CommandsManager._internalCommandLibraries[index].Values)
          {
            string str = " (";
            object[] array = ((IEnumerable<object>) methodInfo.GetCustomAttributes(false)).Where<object>((Func<object, bool>) (a => a.GetType().Name == typeof (MMasterCommand).Name)).ToArray<object>();
            try
            {
              MMasterCommand mmasterCommand = (MMasterCommand) array[0];
              str = str + mmasterCommand.HelpPrompt + ")";
            }
            catch
            {
            }
            CFormat.WriteLine(CFormat.Indent(3) + "." + methodInfo.Name + str, ConsoleColor.Gray);
          }
          CFormat.JumpLine();
        }
      }
      if (CommandsManager._externalLibraryTypes.Count == 0)
        return;
      CFormat.WriteLine("[External commands]", ConsoleColor.Green);
      int num = 1;
      foreach (Type index in CommandsManager._externalLibraryTypes.Values)
      {
        CFormat.WriteLine(index.Name, ConsoleColor.Yellow);
        foreach (MethodInfo methodInfo in CommandsManager._externalCommandLibraries[index].Values)
        {
          string str = " (";
          object[] array = ((IEnumerable<object>) methodInfo.GetCustomAttributes(false)).Where<object>((Func<object, bool>) (a => a.GetType().Name == typeof (MMasterCommand).Name)).ToArray<object>();
          try
          {
            MMasterCommand mmasterCommand = (MMasterCommand) array[0];
            str = str + mmasterCommand.HelpPrompt + ")";
          }
          catch
          {
          }
          CFormat.WriteLine(CFormat.Indent(3) + "." + methodInfo.Name + str, ConsoleColor.Gray);
        }
        if (num < CommandsManager._externalLibraryTypes.Values.Count)
          CFormat.JumpLine();
        ++num;
      }
    }

    [MMasterCommand("This command just does something.", false)]
    public static void DoSomething()
    {
      CFormat.WriteLine("Don't worry this is fake. ;)", ConsoleColor.Gray);
      CFormat.Write("Downloading A: ", ConsoleColor.Gray);
      for (int index = 0; index < 101; ++index)
      {
        CFormat.DrawProgressBar((double) index, 100.0, 20, '■', ConsoleColor.Green, ConsoleColor.DarkGreen);
        Thread.Sleep(10);
      }
      CFormat.JumpLine();
      CFormat.Write("Downloading B: ", ConsoleColor.Gray);
      for (int index = 0; index < 101; ++index)
      {
        CFormat.DrawProgressBar((double) index, 100.0, 20, '■', ConsoleColor.Green, ConsoleColor.DarkGreen);
        Thread.Sleep(10);
      }
      CFormat.JumpLine();
      CFormat.Write("Downloading C: ", ConsoleColor.Gray);
      for (int index = 0; index < 101; ++index)
      {
        CFormat.DrawProgressBar((double) index, 100.0, 20, '■', ConsoleColor.Green, ConsoleColor.DarkGreen);
        Thread.Sleep(10);
      }
      CFormat.JumpLine();
      CFormat.Write("Installing: ", ConsoleColor.Gray);
      for (int index = 0; index < 1001; ++index)
      {
        CFormat.DrawProgressBar((double) index, 1000.0, 20, '■', ConsoleColor.Green, ConsoleColor.DarkGreen);
        Thread.Sleep(10);
      }
      CFormat.JumpLine();
      CFormat.WriteLine("Done!", ConsoleColor.Gray);
    }

    [MMasterCommand("Exit the application.", false)]
    public static void Exit()
    {
      Environment.Exit(0);
    }
  }
}
