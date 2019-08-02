// Decompiled with JetBrains decompiler
// Type: MMaster.Program
// Assembly: MMaster, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adf6cf49a94e58fe
// MVID: FAC110E1-835B-44B3-9078-25DCBA4F0789
// Assembly location: C:\Users\Matthieu\Documents\Programmes\MMaster\MMaster.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MMaster
{
  internal class Program
  {
    private static string appName = Assembly.GetExecutingAssembly().GetName().Name;
    private static Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
    private static string bootMessage = Program.appName + " Command Prompt [version " + Program.appVersion.ToString() + "]" + Environment.NewLine + "(c) 2017-2018 Matthieu Badoy. All rights reserved.";
    private const string _commandNamespace = "MMaster.Commands";

    private static void Main(string[] args)
    {
      Console.Title = Program.appName;
      CFormat.WriteLine(Program.bootMessage, ConsoleColor.Cyan);
      CFormat.JumpLine();
      CommandsManager.LoadInternalCommands();
      CommandsManager.LoadExternalCommands();
      CFormat.JumpLine();
      CFormat.WriteLine("Type 'List' to get the list of available commands.", ConsoleColor.Gray);
      Program.Run();
    }

    private static void Run()
    {
      while (true)
      {
        object obj;
        do
        {
          CFormat.JumpLine();
          obj = CInput.ReadFromConsole("", ConsoleInputType.String, false, -1, char.MinValue);
        }
        while (string.IsNullOrWhiteSpace(obj.ToString()));
        try
        {
          Program.Execute(new CRawInputCommand(obj.ToString()));
        }
        catch (Exception ex)
        {
          CFormat.WriteLine(ex.Message, ConsoleColor.Gray);
        }
      }
    }

    private static void Execute(CRawInputCommand rawCommand)
    {
      if (rawCommand.LibraryClassType == (Type) null)
      {
        CFormat.WriteLine("This command does not exist.", ConsoleColor.Gray);
      }
      else
      {
        Dictionary<string, MethodInfo> source1;
        if (CommandsManager._internalCommandLibraries.ContainsKey(rawCommand.LibraryClassType))
          source1 = CommandsManager._internalCommandLibraries[rawCommand.LibraryClassType];
        else if (CommandsManager._externalCommandLibraries.ContainsKey(rawCommand.LibraryClassType))
        {
          source1 = CommandsManager._externalCommandLibraries[rawCommand.LibraryClassType];
        }
        else
        {
          CFormat.WriteLine("This command does not exist.", ConsoleColor.Gray);
          return;
        }
        if (source1.Any<KeyValuePair<string, MethodInfo>>((Func<KeyValuePair<string, MethodInfo>, bool>) (i => i.Key.ToLower().Equals(rawCommand.Name.ToLower()))))
        {
          rawCommand.Name = source1.Keys.Where<string>((Func<string, bool>) (i => i.ToLower().Equals(rawCommand.Name.ToLower()))).ToArray<string>()[0];
          IEnumerable<ParameterInfo> list = (IEnumerable<ParameterInfo>) ((IEnumerable<ParameterInfo>) source1[rawCommand.Name].GetParameters()).ToList<ParameterInfo>();
          List<object> objectList = new List<object>();
          IEnumerable<ParameterInfo> source2 = list.Where<ParameterInfo>((Func<ParameterInfo, bool>) (p => !p.IsOptional));
          IEnumerable<ParameterInfo> source3 = list.Where<ParameterInfo>((Func<ParameterInfo, bool>) (p => p.IsOptional));
          int num1 = source2.Count<ParameterInfo>();
          source3.Count<ParameterInfo>();
          int num2 = rawCommand.Arguments.Count<string>();
          if (num1 > num2)
          {
            CFormat.WriteLine("Missing required argument." + Environment.NewLine + CFormat.GetArgsFormat(rawCommand.FullName, list), ConsoleColor.Gray);
          }
          else
          {
            if (list.Count<ParameterInfo>() > 0)
            {
              foreach (ParameterInfo parameterInfo in list)
                objectList.Add(parameterInfo.DefaultValue);
              for (int index = 0; index < list.Count<ParameterInfo>(); ++index)
              {
                ParameterInfo parameterInfo = list.ElementAt<ParameterInfo>(index);
                Type parameterType = parameterInfo.ParameterType;
                try
                {
                  try
                  {
                    rawCommand.Arguments.ElementAt<string>(index);
                  }
                  catch
                  {
                    continue;
                  }
                  object obj = CFormat.CoerceArgument(parameterType, rawCommand.Arguments.ElementAt<string>(index));
                  objectList.RemoveAt(index);
                  objectList.Insert(index, obj);
                }
                catch (ArgumentException ex)
                {
                  throw new ArgumentException(string.Format("The value passed for argument '{0}' cannot be parsed to type '{1}'", (object) parameterInfo.Name, (object) parameterType.Name));
                }
              }
            }
            Assembly assembly = typeof (Program).Assembly;
            object[] parameters = (object[]) null;
            if (objectList.Count > 0)
              parameters = objectList.ToArray();
            try
            {
              source1[rawCommand.Name].Invoke((object) null, parameters);
            }
            catch (TargetInvocationException ex)
            {
              throw ex.InnerException;
            }
          }
        }
        else
          CFormat.WriteLine("This command does not exist.", ConsoleColor.Gray);
      }
    }
  }
}
