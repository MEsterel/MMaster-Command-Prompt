using MMaster.Commands;
using System;
using System.Linq;
using System.Reflection;

namespace MMaster
{
    public static class CInput
    {
        private static string[] _commandHistory = new string[1]
        {
            ""
        };

        private static bool insertMode = false;
        private static string _readBuffer = "";
        private static int historyIndex = 0;
        private static string newReadBuffer = "";
        internal const string _badCommandMessage = "This command does not exist.";
        private static int cursorPos;
        private static bool tabAllowed;

        public static object ReadFromConsole(
          string promptMessage = "",
          ConsoleInputType inputType = ConsoleInputType.String,
          bool canEscape = false,
          int maxChars = -1,
          char charMask = '\0')
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            bool maskFlag = charMask > char.MinValue;
            bool runFlag = true;
            promptMessage = promptMessage == "" ? "MMaster> " : promptMessage;
            cursorPos = 0;
            historyIndex = 0;
            tabAllowed = true;
            _readBuffer = "";
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(promptMessage);

            while (runFlag)
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);


                // READ BUFFER
                if (consoleKeyInfo.Key == ConsoleKey.Enter)
                {
                    ValidateHistory();
                    if (!String.IsNullOrEmpty(_readBuffer))
                        AddToHistory(_readBuffer);

                    Console.ForegroundColor = foregroundColor;
                    CFormat.JumpLine();

                    switch (inputType)
                    {
                        case ConsoleInputType.String:
                            return _readBuffer;

                        case ConsoleInputType.Int:
                            if (String.IsNullOrEmpty(_readBuffer))
                            {
                                return null;
                            }
                            return int.Parse(_readBuffer);

                        case ConsoleInputType.Double:
                            if (String.IsNullOrEmpty(_readBuffer))
                            {
                                return null;
                            }
                            return double.Parse(_readBuffer.Replace(".", ","));
                    }
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Backspace)
                {
                    ValidateHistory();
                    if (cursorPos > 0)
                    {
                        if (cursorPos != _readBuffer.Length)
                        {
                            string str2 = _readBuffer.Substring(0, cursorPos - 1);
                            string str3 = _readBuffer.Substring(cursorPos, _readBuffer.Length - cursorPos);
                            _readBuffer = str2 + str3;
                            MoveCursorBack();
                            UserWrite(str3 + " ");
                            for (int index = 0; index < str3.Length + 1; ++index)
                                MoveCursorBack();
                        }
                        else
                        {
                            _readBuffer = _readBuffer.Substring(0, _readBuffer.Length - 1);
                            MoveCursorBack();
                            UserWrite(" ");
                            MoveCursorBack();
                        }
                    }
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Delete)
                {
                    ValidateHistory();
                    if (cursorPos < _readBuffer.Length)
                    {
                        string str2 = _readBuffer.Substring(0, cursorPos);
                        string str3 = _readBuffer.Substring(cursorPos + 1, _readBuffer.Length - cursorPos - 1);
                        _readBuffer = str2 + str3;
                        UserWrite(str3 + " ");
                        for (int index = 0; index < str3.Length + 1; ++index)
                            MoveCursorBack();
                    }
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Home)
                {
                    for (int cursorPos = CInput.cursorPos; cursorPos > 0; --cursorPos)
                        MoveCursorBack();
                }
                else if (consoleKeyInfo.Key == ConsoleKey.End)
                {
                    for (int cursorPos = CInput.cursorPos; cursorPos < _readBuffer.Length; ++cursorPos)
                        MoveCursorAhead();
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Escape)
                {
                    if (canEscape)
                        return null;
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Insert)
                {
                    insertMode = !insertMode;
                    Console.CursorSize = !insertMode ? 1 : 100;
                }
                else if (consoleKeyInfo.Key != ConsoleKey.Spacebar && consoleKeyInfo.KeyChar == char.MinValue)
                {
                    if (consoleKeyInfo.Key == ConsoleKey.RightArrow && cursorPos < _readBuffer.Length)
                        MoveCursorAhead();
                    else if (consoleKeyInfo.Key == ConsoleKey.LeftArrow && cursorPos > 0)
                        MoveCursorBack();
                    else if (consoleKeyInfo.Key == ConsoleKey.UpArrow)
                        OlderHistory();
                    else if (consoleKeyInfo.Key == ConsoleKey.DownArrow)
                        NewerHistory();
                }
                // Tab auto-completition
                else if (consoleKeyInfo.Key == ConsoleKey.Tab)
                {
                    if (String.IsNullOrEmpty(_readBuffer) || !tabAllowed || _readBuffer.Contains(" "))
                        continue;

                    if (!_readBuffer.Contains(".")) // If looking for a library OR default command
                    {
                        //Try internal libraries first
                        string result = CommandManager.InternalLibraryCallNames.Keys.FirstOrDefault(x => x.ToLower().StartsWith(_readBuffer.ToLower()));
                        //Then try external libraries
                        if (result == null)
                        {
                            result = CommandManager.ExternalLibraryCallNames.Keys.FirstOrDefault(x => x.ToLower().StartsWith(_readBuffer.ToLower()));
                        }
                        // If still null, no result at all in libraries
                        if (result != null)
                        {
                            for (int i = 0; i < _readBuffer.Length; i++)
                            {
                                MoveCursorBack();
                            }
                            UserWrite(result);
                            _readBuffer = result;
                        }

                        // Then trying in Default library, the only one that can be called without library.
                        result = CommandManager.InternalLibraries[typeof(Default)].Keys.FirstOrDefault(x => x.ToLower().StartsWith(_readBuffer.ToLower()));

                        if (result == null)
                            continue;
                        else
                        {
                            for (int i = 0; i < _readBuffer.Length; i++)
                            {
                                MoveCursorBack();
                            }
                            UserWrite(result);
                            _readBuffer = result;
                        }
                    }
                    else // If the buffer contains a point.
                    {
                        // PARSE LIBRARY
                        Type library = CParsedInput.ParseLibrary(_readBuffer.Split('.')[0]);
                        string commandInputLower = _readBuffer.Split('.')[1].ToLower();

                        if (library == null || commandInputLower == "")
                            continue;

                        // Try internal libraries
                        string result = null;
                        if (CommandManager.InternalLibraries.ContainsKey(library))
                        {
                            result = CommandManager.InternalLibraries[library].Keys.FirstOrDefault(x => x.ToLower().StartsWith(commandInputLower));
                        }
                        // Then try external
                        else
                        {
                            result = CommandManager.ExternalLibraries[library].Keys.FirstOrDefault(x => x.ToLower().StartsWith(commandInputLower));
                        }

                        // If result found
                        if (result != null)
                        {
                            for (int i = 0; i < _readBuffer.Length; i++)
                            {
                                MoveCursorBack();
                            }

                            string libraryCallName = library.GetCustomAttribute<MMasterLibrary>().CallName;
                            UserWrite(libraryCallName + "." + result);
                            _readBuffer = libraryCallName + "." + result;
                        }


                        
                    }

                    continue;
                }
                else
                {
                    CInput.ValidateHistory();
                    if (maxChars <= 0 || CInput._readBuffer.Length < maxChars)
                    {
                        char keyChar;
                        switch (inputType)
                        {
                            case ConsoleInputType.Int:
                                keyChar = consoleKeyInfo.KeyChar;
                                int num1;
                                if (!int.TryParse(keyChar.ToString(), out _))
                                {
                                    keyChar = consoleKeyInfo.KeyChar;
                                    if (!(keyChar.ToString() != "-"))
                                    {
                                        keyChar = consoleKeyInfo.KeyChar;
                                        num1 = !(keyChar.ToString() == "-") ? 0 : ((uint)CInput._readBuffer.Length > 0U ? 1 : 0);
                                    }
                                    else
                                        num1 = 1;
                                }
                                else
                                    num1 = 0;
                                if (num1 == 0)
                                    break;
                                continue;
                            case ConsoleInputType.Double:
                                keyChar = consoleKeyInfo.KeyChar;
                                int num2;
                                if (!int.TryParse(keyChar.ToString(), out _))
                                {
                                    keyChar = consoleKeyInfo.KeyChar;
                                    if (!(keyChar.ToString() != "."))
                                    {
                                        keyChar = consoleKeyInfo.KeyChar;
                                        if (!(keyChar.ToString() == ".") || !CInput._readBuffer.Contains("."))
                                            goto label_54;
                                    }
                                    keyChar = consoleKeyInfo.KeyChar;
                                    if (!(keyChar.ToString() != "-"))
                                    {
                                        keyChar = consoleKeyInfo.KeyChar;
                                        num2 = !(keyChar.ToString() == "-") ? 0 : (CInput._readBuffer.Length != 0 ? 1 : (CInput._readBuffer.Contains(".") ? 1 : 0));
                                        goto label_55;
                                    }
                                    else
                                    {
                                        num2 = 1;
                                        goto label_55;
                                    }
                                }
                            label_54:
                                num2 = 0;
                            label_55:
                                if (num2 == 0)
                                    break;
                                continue;
                        }
                        if (CInput.cursorPos != CInput._readBuffer.Length && !CInput.insertMode) // If in the word, insert mode off
                        {
                            string str2 = CInput._readBuffer.Substring(0, CInput.cursorPos);
                            keyChar = consoleKeyInfo.KeyChar;
                            string str3 = keyChar.ToString();
                            string str4 = CInput._readBuffer.Substring(CInput.cursorPos, CInput._readBuffer.Length - CInput.cursorPos);
                            CInput._readBuffer = str2 + str3 + str4;
                        }
                        else if (CInput.cursorPos != CInput._readBuffer.Length && CInput.insertMode) // If in the word, insert mode on
                        {
                            string str2 = CInput._readBuffer.Substring(0, CInput.cursorPos);
                            keyChar = consoleKeyInfo.KeyChar;
                            string str3 = keyChar.ToString();
                            string str4 = CInput._readBuffer.Substring(CInput.cursorPos + 1, CInput._readBuffer.Length - CInput.cursorPos - 1);
                            CInput._readBuffer = str2 + str3 + str4;
                        }
                        else
                        {
                            string readBuffer = CInput._readBuffer;
                            keyChar = consoleKeyInfo.KeyChar;
                            string str2 = keyChar.ToString();
                            CInput._readBuffer = readBuffer + str2;
                        }


                        // PRINT TO SCREEN
                        if (maskFlag)
                        {
                            CInput.UserWrite(charMask.ToString());
                        }
                        else
                        {
                            keyChar = consoleKeyInfo.KeyChar;
                            CInput.UserWrite(keyChar.ToString());
                            if (CInput.cursorPos != CInput._readBuffer.Length && !CInput.insertMode)
                            {
                                string s = CInput._readBuffer.Substring(CInput.cursorPos, CInput._readBuffer.Length - CInput.cursorPos);
                                CInput.UserWrite(s);
                                for (int index = 0; index < s.Length; ++index)
                                    CInput.MoveCursorBack();
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static ConsoleAnswer UserChoice(ConsoleAnswerType type, bool canEscape = false)
        {
            switch (type)
            {
                case ConsoleAnswerType.YesNo:
                    object object1 = CInput.ReadFromConsole("(YES / NO): ", ConsoleInputType.String, canEscape, 3, char.MinValue);
                    if (object1 == null)
                    {
                        CFormat.WriteLine("[Canceled]", ConsoleColor.DarkYellow);
                        return ConsoleAnswer.Escaped;
                    }
                    string str1 = object1.ToString().ToLower();
                    if (str1 == "y" || str1 == "yes")
                        return ConsoleAnswer.Yes;
                    if (str1 == "n" || str1 == "no")
                        return ConsoleAnswer.No;
                    return CInput.UserChoice(type);
                    

                case ConsoleAnswerType.YesNoCancel:
                    object object2 = CInput.ReadFromConsole("(YES / NO / CANCEL): ", ConsoleInputType.String, canEscape, 6, char.MinValue);
                    if (object2 == null)
                    {
                        CFormat.WriteLine("[Canceled]", ConsoleColor.DarkYellow);
                        return ConsoleAnswer.Escaped;
                    }
                    string str2 = object2.ToString().ToLower();
                    if (str2 == "y" || str2 == "yes")
                        return ConsoleAnswer.Yes;
                    if (str2 == "n" || str2 == "no")
                        return ConsoleAnswer.No;
                    if (str2 == "c" || str2 == "cancel")
                        return ConsoleAnswer.Cancel;
                    return CInput.UserChoice(type);

                case ConsoleAnswerType.TrueFalse:
                    object object3 = CInput.ReadFromConsole("(TRUE / FALSE): ", ConsoleInputType.String, canEscape, 5, char.MinValue);
                    if (object3 == null)
                    {
                        CFormat.WriteLine("[Canceled]", ConsoleColor.DarkYellow);
                        return ConsoleAnswer.Escaped;
                    }
                    string str3 = object3.ToString().ToLower();
                    if (str3 == "t" || str3 == "true")
                        return ConsoleAnswer.Yes;
                    if (str3 == "f" || str3 == "false")
                        return ConsoleAnswer.No;
                    return CInput.UserChoice(type);

                default:
                    CFormat.WriteLine("[CInput] ERROR: Could not get user choice, specifed answer type does not exist.", ConsoleColor.Red);
                    return ConsoleAnswer.Undefined;
            }
        }

        public static int UserPickInt(int maxNumber)
        {
            object obj = CInput.ReadFromConsole("Enter a number between 0 and " + maxNumber + ": ", ConsoleInputType.Int, true, maxNumber.ToString().Length, char.MinValue);
            if (obj == null)
            {
                CFormat.WriteLine("[Canceled]", ConsoleColor.DarkYellow);
                return -1;
            }
            if ((int)obj >= 0 && (int)obj <= maxNumber)
                return (int)obj;
            CInput.UserPickInt(maxNumber);
            return -1;
        }

        private static void ValidateHistory()
        {
            if (CInput.newReadBuffer == "")
                return;
            CInput._readBuffer = CInput.newReadBuffer;
            CInput.historyIndex = 0;
            CInput.newReadBuffer = "";
        }

        private static void AddToHistory(string s)
        {
            if (_commandHistory[_commandHistory.Length - 1] == s)
                return; // Do not add a duplicate.

            CInput._commandHistory = (CInput._commandHistory).Concat<string>(new string[1] { s }).ToArray<string>();
        }

        private static void OlderHistory()
        {
            if (CInput.historyIndex + 2 > CInput._commandHistory.Length || CInput._commandHistory.Length <= 1)
                return;

            ++CInput.historyIndex;
            CInput.newReadBuffer = CInput._commandHistory[CInput._commandHistory.Length - CInput.historyIndex];
            for (int cursorPos = CInput.cursorPos; cursorPos > 0; --cursorPos)
            {
                CInput.MoveCursorBack();
                CInput.UserWrite(" ");
                CInput.MoveCursorBack();
            }
            CInput.UserWrite(CInput.newReadBuffer);
        }

        private static void NewerHistory()
        {
            switch (CInput.historyIndex)
            {
                case 0:
                    break;

                case 1:
                    --CInput.historyIndex;
                    CInput.newReadBuffer = "";
                    for (int cursorPos = CInput.cursorPos; cursorPos > 0; --cursorPos)
                    {
                        CInput.MoveCursorBack();
                        CInput.UserWrite(" ");
                        CInput.MoveCursorBack();
                    }
                    CInput.UserWrite(CInput._readBuffer);
                    break;

                default:
                    --CInput.historyIndex;
                    CInput.newReadBuffer = CInput._commandHistory[CInput._commandHistory.Length - CInput.historyIndex];
                    for (int cursorPos = CInput.cursorPos; cursorPos > 0; --cursorPos)
                    {
                        CInput.MoveCursorBack();
                        CInput.UserWrite(" ");
                        CInput.MoveCursorBack();
                    }
                    CInput.UserWrite(CInput.newReadBuffer);
                    break;
            }
        }

        private static void MoveCursorBack()
        {
            if (Console.CursorLeft > 0)
            {
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                --CInput.cursorPos;
            }
            else
            {
                if (Console.CursorTop <= 0)
                    return;
                Console.SetCursorPosition(Console.WindowWidth - 1, Console.CursorTop - 1);
                --CInput.cursorPos;
            }
        }

        private static void MoveCursorAhead()
        {
            if (Console.CursorLeft < Console.WindowWidth - 1)
            {
                Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                ++CInput.cursorPos;
            }
            else
            {
                if (Console.CursorTop >= Console.BufferHeight)
                    return;
                Console.SetCursorPosition(0, Console.CursorTop + 1);
                ++CInput.cursorPos;
            }
        }

        private static void UserWrite(string s)
        {
            Console.Write(s);
            CInput.cursorPos += s.Length;
        }
    }
}