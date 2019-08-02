using System;
using System.Collections.Generic;
using System.Linq;

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

        public static object ReadFromConsole(
          string promptMessage = "",
          ConsoleInputType inputType = ConsoleInputType.String,
          bool canEscape = false,
          int maxChars = -1,
          char charMask = '\0')
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            bool flag1 = charMask > char.MinValue;
            bool flag2 = false;
            string str1 = promptMessage == "" ? "MMaster> " : promptMessage;
            CInput.cursorPos = 0;
            CInput.historyIndex = 0;
            CInput._readBuffer = "";
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(str1);
            while (!flag2)
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);

                if (consoleKeyInfo.Key == ConsoleKey.Enter)
                {
                    CInput.ValidateHistory();
                    CInput.AddToHistory(CInput._readBuffer);
                    Console.ForegroundColor = foregroundColor;
                    CFormat.WriteLine("", ConsoleColor.Gray);
                    switch (inputType)
                    {
                        case ConsoleInputType.String:
                            return CInput._readBuffer;

                        case ConsoleInputType.Int:
                            return int.Parse(CInput._readBuffer);

                        case ConsoleInputType.Double:
                            return double.Parse(CInput._readBuffer.Replace(".", ","));
                    }
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Backspace)
                {
                    CInput.ValidateHistory();
                    if (CInput.cursorPos > 0)
                    {
                        if (CInput.cursorPos != CInput._readBuffer.Length)
                        {
                            string str2 = CInput._readBuffer.Substring(0, CInput.cursorPos - 1);
                            string str3 = CInput._readBuffer.Substring(CInput.cursorPos, CInput._readBuffer.Length - CInput.cursorPos);
                            CInput._readBuffer = str2 + str3;
                            CInput.MoveCursorBack();
                            CInput.UserWrite(str3 + " ");
                            for (int index = 0; index < str3.Length + 1; ++index)
                                CInput.MoveCursorBack();
                        }
                        else
                        {
                            CInput._readBuffer = CInput._readBuffer.Substring(0, CInput._readBuffer.Length - 1);
                            CInput.MoveCursorBack();
                            CInput.UserWrite(" ");
                            CInput.MoveCursorBack();
                        }
                    }
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Delete)
                {
                    CInput.ValidateHistory();
                    if (CInput.cursorPos < CInput._readBuffer.Length)
                    {
                        string str2 = CInput._readBuffer.Substring(0, CInput.cursorPos);
                        string str3 = CInput._readBuffer.Substring(CInput.cursorPos + 1, CInput._readBuffer.Length - CInput.cursorPos - 1);
                        CInput._readBuffer = str2 + str3;
                        CInput.UserWrite(str3 + " ");
                        for (int index = 0; index < str3.Length + 1; ++index)
                            CInput.MoveCursorBack();
                    }
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Home)
                {
                    for (int cursorPos = CInput.cursorPos; cursorPos > 0; --cursorPos)
                        CInput.MoveCursorBack();
                }
                else if (consoleKeyInfo.Key == ConsoleKey.End)
                {
                    for (int cursorPos = CInput.cursorPos; cursorPos < CInput._readBuffer.Length; ++cursorPos)
                        CInput.MoveCursorAhead();
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Escape)
                {
                    if (canEscape)
                        return null;
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Insert)
                {
                    CInput.insertMode = !CInput.insertMode;
                    Console.CursorSize = !CInput.insertMode ? 1 : 100;
                }
                else if (consoleKeyInfo.Key != ConsoleKey.Spacebar && consoleKeyInfo.KeyChar == char.MinValue)
                {
                    if (consoleKeyInfo.Key == ConsoleKey.RightArrow && CInput.cursorPos < CInput._readBuffer.Length)
                        CInput.MoveCursorAhead();
                    else if (consoleKeyInfo.Key == ConsoleKey.LeftArrow && CInput.cursorPos > 0)
                        CInput.MoveCursorBack();
                    else if (consoleKeyInfo.Key == ConsoleKey.UpArrow)
                        CInput.OlderHistory();
                    else if (consoleKeyInfo.Key == ConsoleKey.DownArrow)
                        CInput.NewerHistory();
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
                        if (CInput.cursorPos != CInput._readBuffer.Length && !CInput.insertMode)
                        {
                            string str2 = CInput._readBuffer.Substring(0, CInput.cursorPos);
                            keyChar = consoleKeyInfo.KeyChar;
                            string str3 = keyChar.ToString();
                            string str4 = CInput._readBuffer.Substring(CInput.cursorPos, CInput._readBuffer.Length - CInput.cursorPos);
                            CInput._readBuffer = str2 + str3 + str4;
                        }
                        else if (CInput.cursorPos != CInput._readBuffer.Length && CInput.insertMode)
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
                        if (flag1)
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

        public static ConsoleAnswer UserChoice(ConsoleAnswerType type)
        {
            switch (type)
            {
                case ConsoleAnswerType.YesNo:
                    string lower1 = CInput.ReadFromConsole("(YES / NO): ", ConsoleInputType.String, false, -1, char.MinValue).ToString().ToLower();
                    if (lower1.ToLower() == "y" || lower1.ToLower() == "yes")
                        return ConsoleAnswer.Yes;
                    if (lower1.ToLower() == "n" || lower1.ToLower() == "no")
                        return ConsoleAnswer.No;
                    return CInput.UserChoice(type);

                case ConsoleAnswerType.YesNoCancel:
                    string lower2 = CInput.ReadFromConsole("(YES / NO / CANCEL): ", ConsoleInputType.String, false, -1, char.MinValue).ToString().ToLower();
                    if (lower2.ToLower() == "y" || lower2.ToLower() == "yes")
                        return ConsoleAnswer.Yes;
                    if (lower2.ToLower() == "n" || lower2.ToLower() == "no")
                        return ConsoleAnswer.No;
                    if (lower2.ToLower() == "c" || lower2.ToLower() == "cancel")
                        return ConsoleAnswer.Cancel;
                    return CInput.UserChoice(type);

                case ConsoleAnswerType.TrueFalse:
                    string lower3 = CInput.ReadFromConsole("(TRUE / FALSE): ", ConsoleInputType.String, false, -1, char.MinValue).ToString().ToLower();
                    if (lower3.ToLower() == "t" || lower3.ToLower() == "true")
                        return ConsoleAnswer.Yes;
                    if (lower3.ToLower() == "f" || lower3.ToLower() == "false")
                        return ConsoleAnswer.No;
                    return CInput.UserChoice(type);

                default:
                    CFormat.WriteLine("Could not get user choice, specifed answer type does not exist.", ConsoleColor.Red);
                    return ConsoleAnswer.Undefined;
            }
        }

        public static int UserPickInt(int maxNumber)
        {
            object obj = CInput.ReadFromConsole("Enter a number between 0 and " + maxNumber + ": ", ConsoleInputType.Int, true, maxNumber.ToString().Length, char.MinValue);
            if (obj == null)
            {
                CFormat.WriteLine("Canceled.", ConsoleColor.Gray);
                return -1;
            }
            if ((int)obj >= 0 && (int)obj <= maxNumber)
                return (int)obj;
            CInput.UserPickInt(maxNumber);
            return -1;
        }

        private static void ValidateHistory()
        {
            if (!(CInput.newReadBuffer != ""))
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