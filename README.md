# MMaster
A command prompt that expands through simple C# files (*.cs) compiled on startup.

This allows the development of little ideas on the go by creating simple scripts thanks to lite editors (VS Code for instance).

## Adaptative

The MMaster command prompt is a terminal that compiles your cs files just-in-time, so you can develop little tools to be called with "commands" when needed. This makes MMaster a terminal with endless possibilites.

## Easy to use

Building a command for the MMaster command prompt is so easy that it almost won't change your dev habits: just add the *MMasterLibrary* attribute on top of the class containing your commands which are actually *public static void* methods with the *MMasterCommand* attribute.

## Pre-built command line tools

Thanks to the implemented CFormat and CInput classes, you don't have to think about the user-experience: managed color writing, password masking, int picking, Yes/No questions and more!

## Lite & portable

The MMaster command prompt weights **less than 1 Mo**, so you can take it with you where-ever you want!
