// MMASTER COMMAND FILE TEMPLATE
// MMaster (c) 2017-2019 Matthieu Badoy. All rights reserved.
// ----------------------------------------------------------

// REF "System.Windows.Forms.dll"
// Reference other assemblies with REF comments as above

using MMaster; // /!\ MUST BE IMPLEMENTED! /!\
using System;

namespace MyNamespace
{
    // Mandatory library attribute vvv
    [MMasterLibrary("This is the help prompt of this library.", "Library")] // ARGS : HelpPrompt, CallName. Both of these args are optional.
    public class Library
    {
        // Mandatory command attribute vvv
        [MMasterCommand("Prints Hello!", "Command")] // ARGS : HelpPrompt, CallName. Both of these args are optional.
        public static void Command(string message) // Call this command with 'Library.Command'
        {
            // Edit code here
            CFormat.WriteLine("Hello " + message + "!", ConsoleColor.Blue);
        }
    }
}

/// <information>
/// MMASTER AVAILABLE STATIC METHODS:
///
///- object CInput.ReadFromConsole(string promptMessage = "", ConsoleInputType inputType = ConsoleInputType.String, bool canEscape = false, int maxChars = -1, char charMask = Char.MinValue)
///		Returns a user input as an object (return = string (by default)/int/double).
///		promptMessage (optional): prompt to show before user input.
///		inputType (optional): type of the input (inputType = ConsoleInputType.String/ConsoleInputType.Int/ConsoleInputType.Double).
///		canEscape (optional): if true and if the users presses ESCAPE, it escapes the Read and returns null.
///		maxChars (optional): number of maximum chars (if maxChars < 1, this parameter is ignored).
///		charMask (optional): replace characters with a specific char.
///
///- ConsoleAnswer CInput.UserChoice(ConsoleAnswerType type, bool canEscape = false)
///		Returns a user choice among options (return = ConsoleAnswer.Yes/ConsoleAnswer.No/ConsoleAnswer.Cancel/ConsoleAnswer.True/ConsoleAnswer.False/ConsoleAnswer.Escaped/ConsoleAnswer.Undefined(in case of error)).
///		type: type of options (type = ConsoleAnswerType.YesNo/ConsoleAnswerType.YesNoCancel/ConsoleAnswerType.TrueFalse)
///
///- int CInput.UserPickInt(int maxNumber)
///		Returns a picked number between 0 and maxNumber.
///		maxNumber: maximum number available for choice.
///
///- string CFormat.Indent(int nb)
///		Returns a string of white spaces (nb * white space).
///
///- void CFormat.JumpLine()
///		Simply jumps a line.
///
///- void CFormat.WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
///		Writes a line. It can be colored.
///		text: message to print.
///		color (optional): color of the line.
///		
/// - void CFormat.WriteLine(ConsoleColor color, params string[] text)
///		Writes as many colored lines as you want.
///		text: message to print (infinite argument, ad as many lines as you want).
///		color (optional): color of the line.
///
///- static void CFormat.Write(string text, ConsoleColor color = ConsoleColor.Gray)
///		Writes text. It can be colored.
///		text: message to print.
///		color (optional): color of the line.
///
///- static void CFormat.DrawProgressBar(double complete, double maxVal, int barSize, char progressCharacter, ConsoleColor primaryColor = ConsoleColor.Green,
///          ConsoleColor secondaryColor = ConsoleColor.DarkGreen)
///		Draws a progress bar.
///		complete: value of the progression.
///		maxVal: max value of the progression.
///		barSize: size of the progress bar in number of chars.
///		progressCharacter: char to draw the progress bar with (recommanded: ■).
///		primaryColor (optional): foreground color of the progress bar.
///		secondaryColor (optional): background color of the progress bar.
/// </information>
