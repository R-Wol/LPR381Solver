using System;
using LPR381Solver.Core;
using LPR381Solver.IO;
using LPR381Solver.Models;
using LPR381Solver.Parsing;

namespace LPR381Solver
{
    // Entry point: a simple menu-driven console application.
    // Usage: solve.exe [optional path to an input file to load on startup]
    public class Program
    {
        private static LPModel currentModel;
        private static string currentInputPath;

        public static void Main(string[] args)
        {
            Console.WriteLine("====================================================");
            Console.WriteLine(" LPR381 - Linear & Integer Programming Solver");
            Console.WriteLine("====================================================");

            if (args.Length > 0)
            {
                TryLoadModel(args[0]);
            }

            bool running = true;
            while (running)
            {
                PrintMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        LoadModelFromUser();
                        break;
                    case "2":
                        RunPrimalSimplex();
                        break;
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                        Console.WriteLine();
                        Console.WriteLine("This algorithm/option is not part of this build yet - coming in a later stage.");
                        Console.WriteLine();
                        break;
                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Unrecognised option, please try again.");
                        break;
                }
            }

            Console.WriteLine("Goodbye.");
        }

        private static void PrintMenu()
        {
            Console.WriteLine();
            if (currentInputPath == null)
            {
                Console.WriteLine("No input file loaded.");
            }
            else
            {
                Console.WriteLine("Loaded model: " + currentInputPath);
            }
            Console.WriteLine("1. Load input file");
            Console.WriteLine("2. Solve using Primal Simplex Algorithm");
            Console.WriteLine("3. Solve using Revised Primal Simplex Algorithm      [coming soon]");
            Console.WriteLine("4. Solve using Branch & Bound Simplex Algorithm      [coming soon]");
            Console.WriteLine("5. Solve using Branch & Bound Knapsack Algorithm     [coming soon]");
            Console.WriteLine("6. Solve using Cutting Plane Algorithm               [coming soon]");
            Console.WriteLine("7. Sensitivity Analysis                              [coming soon]");
            Console.WriteLine("0. Exit");
            Console.Write("Select an option: ");
        }

        private static void LoadModelFromUser()
        {
            Console.Write("Enter path to input text file: ");
            string path = Console.ReadLine();
            TryLoadModel(path);
        }

        private static void TryLoadModel(string path)
        {
            try
            {
                currentModel = InputParser.ParseFile(path);
                currentInputPath = path;
                Console.WriteLine("Model loaded successfully: " + currentModel.VariableCount +
                                   " decision variable(s), " + currentModel.Constraints.Count + " constraint(s).");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load model: " + ex.Message);
                currentModel = null;
                currentInputPath = null;
            }
        }

        private static void RunPrimalSimplex()
        {
            if (currentModel == null)
            {
                Console.WriteLine("Please load an input file first (option 1).");
                return;
            }

            try
            {
                SolveResult result = PrimalSimplexSolver.Solve(currentModel);
                string report = OutputWriter.BuildReport(currentModel, "Primal Simplex Algorithm", result);

                Console.WriteLine();
                Console.WriteLine(report);

                Console.Write("Enter path to save the output report (blank = output.txt): ");
                string outputPath = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    outputPath = "output.txt";
                }

                OutputWriter.WriteToFile(outputPath, report);
                Console.WriteLine("Report written to " + outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while solving: " + ex.Message);
            }
        }
    }
}