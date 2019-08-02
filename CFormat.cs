// Decompiled with JetBrains decompiler
// Type: MMaster.CFormat
// Assembly: MMaster, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adf6cf49a94e58fe
// MVID: FAC110E1-835B-44B3-9078-25DCBA4F0789
// Assembly location: C:\Users\Matthieu\Documents\Programmes\MMaster\MMaster.exe

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MMaster
{
  public static class CFormat
  {
    internal static string GetArgsFormat(
      string FullMethodName,
      IEnumerable<ParameterInfo> ParamInfoList)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("Usage: " + FullMethodName);
      foreach (ParameterInfo paramInfo in ParamInfoList)
      {
        if (!paramInfo.HasDefaultValue)
          stringBuilder.Append(string.Format(" <{0} {1}>", (object) paramInfo.ParameterType.ToString(), (object) paramInfo.Name));
        else if (paramInfo.DefaultValue == null)
          stringBuilder.Append(string.Format(" [optional {0} {1}]", (object) paramInfo.ParameterType.ToString(), (object) paramInfo.Name));
        else if (paramInfo.DefaultValue.GetType() == typeof (string))
          stringBuilder.Append(string.Format(" [optional {0} {1} = \"{2}\"]", (object) paramInfo.ParameterType.ToString(), (object) paramInfo.Name, paramInfo.DefaultValue));
        else
          stringBuilder.Append(string.Format(" [optional {0} {1} = {2}]", (object) paramInfo.ParameterType.ToString(), (object) paramInfo.Name, paramInfo.DefaultValue));
      }
      return stringBuilder.ToString();
    }

    internal static object CoerceArgument(Type requiredType, string inputValue)
    {
      TypeCode typeCode = Type.GetTypeCode(requiredType);
      string message = string.Format("Cannnot coerce the input argument {0} to required type {1}", (object) inputValue, (object) requiredType.Name);
      object obj;
      switch (typeCode)
      {
        case TypeCode.Boolean:
          bool result1;
          if (!bool.TryParse(inputValue, out result1))
            throw new ArgumentException(message);
          obj = (object) result1;
          break;
        case TypeCode.Char:
          char result2;
          if (!char.TryParse(inputValue, out result2))
            throw new ArgumentException(message);
          obj = (object) result2;
          break;
        case TypeCode.Byte:
          byte result3;
          if (!byte.TryParse(inputValue, out result3))
            throw new ArgumentException(message);
          obj = (object) result3;
          break;
        case TypeCode.Int16:
          short result4;
          if (!short.TryParse(inputValue, out result4))
            throw new ArgumentException(message);
          obj = (object) result4;
          break;
        case TypeCode.UInt16:
          ushort result5;
          if (!ushort.TryParse(inputValue, out result5))
            throw new ArgumentException(message);
          obj = (object) result5;
          break;
        case TypeCode.Int32:
          int result6;
          if (!int.TryParse(inputValue, out result6))
            throw new ArgumentException(message);
          obj = (object) result6;
          break;
        case TypeCode.UInt32:
          uint result7;
          if (!uint.TryParse(inputValue, out result7))
            throw new ArgumentException(message);
          obj = (object) result7;
          break;
        case TypeCode.Int64:
          long result8;
          if (!long.TryParse(inputValue, out result8))
            throw new ArgumentException(message);
          obj = (object) result8;
          break;
        case TypeCode.UInt64:
          ulong result9;
          if (!ulong.TryParse(inputValue, out result9))
            throw new ArgumentException(message);
          obj = (object) result9;
          break;
        case TypeCode.Single:
          float result10;
          if (!float.TryParse(inputValue, out result10))
            throw new ArgumentException(message);
          obj = (object) result10;
          break;
        case TypeCode.Double:
          double result11;
          if (!double.TryParse(inputValue, out result11))
            throw new ArgumentException(message);
          obj = (object) result11;
          break;
        case TypeCode.Decimal:
          Decimal result12;
          if (!Decimal.TryParse(inputValue, out result12))
            throw new ArgumentException(message);
          obj = (object) result12;
          break;
        case TypeCode.DateTime:
          DateTime result13;
          if (!DateTime.TryParse(inputValue, out result13))
            throw new ArgumentException(message);
          obj = (object) result13;
          break;
        case TypeCode.String:
          obj = (object) inputValue;
          break;
        default:
          throw new ArgumentException(message);
      }
      return obj;
    }

    public static string Indent(int nb)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < nb; ++index)
        stringBuilder.Append(' ');
      return stringBuilder.ToString();
    }

    public static void DrawProgressBar(
      double complete,
      double maxVal,
      int barSize,
      char progressCharacter,
      ConsoleColor primaryColor = ConsoleColor.Green,
      ConsoleColor secondaryColor = ConsoleColor.DarkGreen)
    {
      Console.CursorVisible = false;
      int cursorLeft = Console.CursorLeft;
      double num1 = complete / maxVal;
      int num2 = (int) Math.Floor(num1 / (1.0 / (double) barSize));
      string empty1 = string.Empty;
      string empty2 = string.Empty;
      for (int index = 0; index < num2; ++index)
        empty1 += progressCharacter.ToString();
      for (int index = 0; index < barSize - num2; ++index)
        empty2 += progressCharacter.ToString();
      Console.ForegroundColor = primaryColor;
      CFormat.Write(empty1, ConsoleColor.Gray);
      Console.ForegroundColor = secondaryColor;
      CFormat.Write(empty2, ConsoleColor.Gray);
      Console.ForegroundColor = primaryColor;
      CFormat.Write(string.Format(" {0}%", (object) (num1 * 100.0).ToString("N2")), ConsoleColor.Gray);
      Console.ResetColor();
      Console.CursorVisible = true;
      Console.CursorLeft = cursorLeft;
    }

    public static void JumpLine()
    {
      CFormat.WriteLine("", ConsoleColor.Gray);
    }

    public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
    {
      if (Console.ForegroundColor != color)
        Console.ForegroundColor = color;
      Console.WriteLine(text);
    }

    public static void Write(string text, ConsoleColor color = ConsoleColor.Gray)
    {
      if (Console.ForegroundColor != color)
        Console.ForegroundColor = color;
      Console.Write(text);
    }
  }
}
