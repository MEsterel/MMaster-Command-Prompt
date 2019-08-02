// MMASTER COMMAND FILE TEMPLATE
// MMaster (c) 2017-2019 Matthieu Badoy. All rights reserved.
// ----------------------------------------------------------

// REF "System.Windows.Forms.dll"
// Reference other assemblies with REF comments as above

using MMaster; // Must be implemented
using System;
using System.Windows.Forms;

namespace MyNamespace
{                
	public static class CommandClass
	{		
		[MMasterCommand("Shows a message box.")] // MMaster command declaration attribute: [MMasterCommand([string help = ""],[bool requiresAdminRights = false])]
		public static void CommandName(string message = null) // Call this command with 'CommandClass.CommandName'
		{
			// Edit code here
			Application.EnableVisualStyles();
			
			if (message == null)
			{
				message = CInput.ReadFromConsole("Message to print: ").ToString();
			}

			MessageBox.Show(message);
		}
	}
}

/// <information>
/// MMASTER AVAILABLE METHODS:
///
///- static object CInput.ReadFromConsole(string promptMessage = "", ConsoleInputType inputType = ConsoleInputType.String, bool canEscape = false, int maxChars = -1, char charMask = Char.MinValue)
///		Returns a user input as an object (return = string (by default)/int/double).
///		promptMessage (optional): prompt to show before user input.
///		inputType (optional): type of the input (inputType = ConsoleInputType.String/ConsoleInputType.Int/ConsoleInputType.Double).
///		canEscape (optional): if true and if the users presses ESCAPE, it escapes the Read and returns null.
///		maxChars (optional): number of maximum chars (if maxChars < 1, this parameter is ignored).
///		charMask (optional): replace characters with a specific char.
///
///- static ConsoleAnswer CInput.UserChoice(ConsoleAnswerType type)
///		Returns a user choice among options (return = ConsoleAnswer.Yes/ConsoleAnswer.No/ConsoleAnswer.Cancel/ConsoleAnswer.True/ConsoleAnswer.False).
///		type: type of options (type = ConsoleAnswerType.YesNo/ConsoleAnswerType.YesNoCancel/ConsoleAnswerType.TrueFalse)
///
///- static int UserPickInt(int maxNumber)
///		Returns a picked number between 0 and maxNumber.
///		maxNumber: maximum number available for choice.
///
///- static string CFormat.Indent(int nb)
///		Returns a string of white spaces (nb * white space).
///
///- static void CFormat.JumpLine()
///		Simply jumps a line.
///
///- static void CFormat.WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
///		Writes a line. It can be colored.
///		text: message to print.
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