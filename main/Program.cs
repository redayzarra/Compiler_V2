﻿using Compiler.Parts;
using Compiler.Parts.Syntax;
using Compiler.Parts.Text;

namespace Compiler
{
    internal static class Program
    {
        private static void Main()
        {
            Welcome();
            var variables = new Dictionary<VariableSymbol, object>();
            var showTree = false;

            while (true)
            {
                Console.Write(">>> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (HandleCommand(line, ref showTree, variables))
                    continue; // Skip parsing and evaluating if HandleCommand processed a command

                var syntaxTree = SyntaxTree.Parse(line);
                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);

                if (showTree)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                    Console.WriteLine();
                }

                if (!result.Diagnostics.Any())
                    PrintWithColor($"{result.Value}", ConsoleColor.DarkGray);
                else
                    DisplayDiagnostics(result.Diagnostics, syntaxTree, line);

                Console.WriteLine();
            }
        }

        private static bool HandleCommand(string line, ref bool showTree, Dictionary<VariableSymbol, object> variables)
        {
            switch (line)
            {
                case "showTree()":
                    showTree = true;
                    PrintWithColor("Showing parse tree.", ConsoleColor.DarkGreen);
                    Console.WriteLine();
                    return true;
                case "hideTree()":
                    showTree = false;
                    PrintWithColor("Hiding parse tree.", ConsoleColor.DarkGreen);
                    Console.WriteLine();
                    return true;
                case "cls":
                case "clear()":
                    variables.Clear(); // Clear variables - depends if I want to
                    Welcome();
                    return true;
                case "run()":
                    PrintWithColor("Restarting compiler...", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    Environment.Exit(2);
                    return true;
                case "test()":
                    PrintWithColor("Running tests...", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    Environment.Exit(3);
                    return true;
                case "exit()":
                    Console.Clear();
                    Environment.Exit(0);
                    return true;
                default:
                    return false;
            }
        }

        private static void Welcome()
        {
            Console.Clear();
            PrintWithColor("Welcome to my compiler! Please type valid Python expressions.", ConsoleColor.Green);
            Console.WriteLine();
        }

        private static void PrintWithColor(string message, ConsoleColor color, bool inline = false)
        {
            Console.ForegroundColor = color;
            if (inline)
                Console.Write(message);
            else 
                Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void DisplayDiagnostics(IEnumerable<Diagnostic> diagnostics, SyntaxTree syntaxTree, string line)
        {
            var text = syntaxTree.Text;
            foreach (var diagnostic in diagnostics)
            {
                var lineIndex = text.GetLineIndex(diagnostic.Span.Start);
                var lineNumber = lineIndex + 1;
                var character = diagnostic.Span.Start - text.Lines[lineIndex].Start + 1;

                Console.WriteLine();
                PrintWithColor($"Line {lineNumber}, Char {character}: ", ConsoleColor.DarkRed, inline: true);
                PrintWithColor(diagnostic.ToString(), ConsoleColor.DarkGray);
                Console.WriteLine();
                HighlightErrorInLine(line, diagnostic.Span);
            }
        }

        private static void HighlightErrorInLine(string line, TextSpan span)
        {
            var prefix = line.Substring(0, span.Start);
            var error = line.Substring(span.Start, span.Length);
            var suffix = line.Substring(span.End);

            Console.Write("    ");
            Console.Write(prefix);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(error);
            Console.ResetColor();
            Console.Write(suffix);
            Console.WriteLine();
            Console.WriteLine(new string(' ', span.Start + 4) + new string('^', span.Length));
        }
    }
}

