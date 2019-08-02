// Decompiled with JetBrains decompiler
// Type: MMaster.Properties.Settings
// Assembly: MMaster, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adf6cf49a94e58fe
// MVID: FAC110E1-835B-44B3-9078-25DCBA4F0789
// Assembly location: C:\Users\Matthieu\Documents\Programmes\MMaster\MMaster.exe

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MMaster.Properties
{
  [CompilerGenerated]
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.1.0.0")]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default
    {
      get
      {
        Settings defaultInstance = Settings.defaultInstance;
        return defaultInstance;
      }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("// MMASTER COMMAND FILE TEMPLATE\r\n// MMaster (c) 2017-2018 Matthieu Badoy. All rights reserved.\r\n// ----------------------------------------------------------\r\n\r\n// REF \"System.Windows.Forms.dll\"\r\n// Reference other assemblies with REF comments as above\r\n\r\nusing MMaster; // Must be implemented\r\nusing System;\r\nusing System.Windows.Forms;\r\n\r\nnamespace MyNamespace\r\n{                \r\n\tpublic static class CommandClass\r\n\t{\t\t\r\n\t\t[MMasterCommand(\"Shows a message box.\")] // MMaster command declaration attribute: [MMasterCommand([string help = \"\"],[bool requiresAdminRights = false])]\r\n\t\tpublic static void CommandName(string message = null) // Call this command with 'CommandClass.CommandName'\r\n\t\t{\r\n\t\t\t// Edit code here\r\n\t\t\tApplication.EnableVisualStyles();\r\n\t\t\t\r\n\t\t\tif (message == null)\r\n\t\t\t{\r\n\t\t\t\tmessage = CInput.ReadFromConsole(\"Message to print: \").ToString();\r\n\t\t\t}\r\n\r\n\t\t\tMessageBox.Show(message);\r\n\t\t}\r\n\t}\r\n}\r\n\r\n/// <information>\r\n/// MMASTER AVAILABLE METHODS:\r\n///\r\n///- static object CInput.ReadFromConsole(string promptMessage = \"\", ConsoleInputType inputType = ConsoleInputType.String, bool canEscape = false, int maxChars = -1, char charMask = Char.MinValue)\r\n///\t\tReturns a user input as an object (return = string (by default)/int/double).\r\n///\t\tpromptMessage (optional): prompt to show before user input.\r\n///\t\tinputType (optional): type of the input (inputType = ConsoleInputType.String/ConsoleInputType.Int/ConsoleInputType.Double).\r\n///\t\tcanEscape (optional): if true and if the users presses ESCAPE, it escapes the Read and returns null.\r\n///\t\tmaxChars (optional): number of maximum chars (if maxChars < 1, this parameter is ignored).\r\n///\t\tcharMask (optional): replace characters with a specific char.\r\n///\r\n///- static ConsoleAnswer CInput.UserChoice(ConsoleAnswerType type)\r\n///\t\tReturns a user choice among options (return = ConsoleAnswer.Yes/ConsoleAnswer.No/ConsoleAnswer.Cancel/ConsoleAnswer.True/ConsoleAnswer.False).\r\n///\t\ttype: type of options (type = ConsoleAnswerType.YesNo/ConsoleAnswerType.YesNoCancel/ConsoleAnswerType.TrueFalse)\r\n///\r\n///- static int UserPickInt(int maxNumber)\r\n///\t\tReturns a picked number between 0 and maxNumber.\r\n///\t\tmaxNumber: maximum number available for choice.\r\n///\r\n///- static string CFormat.Indent(int nb)\r\n///\t\tReturns a string of white spaces (nb * white space).\r\n///\r\n///- static void CFormat.JumpLine()\r\n///\t\tSimply jumps a line.\r\n///\r\n///- static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)\r\n///\t\tWrites a line. It can be colored.\r\n///\t\ttext: message to print.\r\n///\t\tcolor (optional): color of the line.\r\n///\r\n///- static void Write(string text, ConsoleColor color = ConsoleColor.Gray)\r\n///\t\tWrites text. It can be colored.\r\n///\t\ttext: message to print.\r\n///\t\tcolor (optional): color of the line.\r\n///\r\n///- static void DrawProgressBar(double complete, double maxVal, int barSize, char progressCharacter, ConsoleColor primaryColor = ConsoleColor.Green,\r\n///          ConsoleColor secondaryColor = ConsoleColor.DarkGreen)\r\n///\t\tDraws a progress bar.\r\n///\t\tcomplete: value of the progression.\r\n///\t\tmaxVal: max value of the progression.\r\n///\t\tbarSize: size of the progress bar in number of chars.\r\n///\t\tprogressCharacter: char to draw the progress bar with (recommanded: ■).\r\n///\t\tprimaryColor (optional): foreground color of the progress bar.\r\n///\t\tsecondaryColor (optional): background color of the progress bar.\r\n/// </information>")]
    public string FileTemplate
    {
      get
      {
        return (string) this[nameof (FileTemplate)];
      }
      set
      {
        this[nameof (FileTemplate)] = (object) value;
      }
    }
  }
}
