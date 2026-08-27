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
                        RunRevisedPrimalSimplex();
                        break;
                    case "4":
                        RunBranchAndBoundSimplex();
                        break;
                    case "5":
                        RunKnapsackBranchAndBound();
                        break;
                    case "6":
                        RunCuttingPlane();
                        break;
                    case "7":
                        RunSensitivityAnalysis();
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
            Console.WriteLine("3. Solve using Revised Primal Simplex Algorithm");
            Console.WriteLine("4. Solve using Branch & Bound Simplex Algorithm");
            Console.WriteLine("5. Solve using Branch & Bound Knapsack Algorithm");
            Console.WriteLine("6. Solve using Cutting Plane Algorithm");
            Console.WriteLine("7. Sensitivity Analysis");
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

        private static void RunRevisedPrimalSimplex()
        {
            if (currentModel == null)
            {
                Console.WriteLine("Please load an input file first (option 1).");
                return;
            }

            try
            {
                RevisedSolveResult result = RevisedPrimalSimplexSolver.Solve(currentModel);
                string report = OutputWriter.BuildRevisedReport(currentModel, "Revised Primal Simplex Algorithm", result);

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

        private static void RunKnapsackBranchAndBound()
        {
            if (currentModel == null)
            {
                Console.WriteLine("Please load an input file first (option 1).");
                return;
            }

            try
            {
                KnapsackSolveResult result = KnapsackBranchAndBoundSolver.Solve(currentModel);
                string report = KnapsackOutputWriter.BuildReport(currentModel, result);

                Console.WriteLine();
                Console.WriteLine(report);

                Console.Write("Enter path to save the output report (blank = output.txt): ");
                string outputPath = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(outputPath)) outputPath = "output.txt";

                KnapsackOutputWriter.WriteToFile(outputPath, report);
                Console.WriteLine("Report written to " + outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while solving: " + ex.Message);
            }
        }
        private static void RunBranchAndBoundSimplex()
        {
            if (currentModel == null) { Console.WriteLine("Please load an input file first (Option 1)"); return; }
            try
            {
                BranchAndBoundSolveResult result = BranchAndBoundSimplexSolver.Solve(currentModel);
                string report = BranchAndBoundOutputWriter.BuildReport(currentModel, result);

                Console.WriteLine();
                Console.WriteLine(report);

                Console.Write("Enter path to save the output report (blank = output.txt): ");
                string outputPath = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    outputPath = "output.txt";
                }

                BranchAndBoundOutputWriter.WriteToFile(outputPath, report);
                Console.WriteLine("Report written to " + outputPath);


            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while solving: " + ex.Message);
            }
        }

        private static void RunCuttingPlane()
        {
            if (currentModel == null)
            {
                Console.WriteLine("Please load an input file first (option 1).");
                return;
            }

            try
            {
                CuttingPlaneResult result = CuttingPlaneSolver.Solve(currentModel);
                string report = OutputWriter.BuildCuttingPlaneReport(currentModel, algorithmName: "Cutting Plane Algorithm", result);

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

        private static void RunSensitivityAnalysis()
        {
            if (currentModel == null)
            {
                Console.WriteLine("Please load an input file first (option 1).");
                return;
            }

            SolveResult primalResult = PrimalSimplexSolver.Solve(currentModel);
            if (primalResult.Status != SolveStatus.Optimal)
            {
                Console.WriteLine("Sensitivity analysis needs an optimal solution. Current status: " + primalResult.Status);
                return;
            }
            SimplexTableau finalTableau = SensitivityAnalyzer.FinalTableau(primalResult);
            Console.WriteLine();
            Console.WriteLine("SOlved via Primal Simplex. Objective value: " + primalResult.ObjectiveValue);
            Console.WriteLine("Columns: " + string.Join(", ", finalTableau.ColumnNames));

            bool inSubMenu = true;
            while (inSubMenu)
            {
                Console.WriteLine();
                Console.WriteLine("--- Sensitivity Analysis ---");
                Console.WriteLine("1. Range of a Non-Basic Variable");
                Console.WriteLine("2. Apply a change to a Non-Basic Variable");
                Console.WriteLine("3. Range of a Basic Variable");
                Console.WriteLine("4. Apply a change to a Basic Variable");
                Console.WriteLine("5. Range of a constraint's RHS");
                Console.WriteLine("6. Apply a change to a constraint's RHS");
                Console.WriteLine("7. Range of a coefficient in a Non-Basic column");
                Console.WriteLine("8. Apply a change to a coefficient in a Non-Basic column");
                Console.WriteLine("9. Add a new activity");
                Console.WriteLine("10. Add a new constraint");
                Console.WriteLine("11. Display shadow prices");
                Console.WriteLine("12. Duality (build, solve, verify)");
                Console.WriteLine("0. Back to main menu");
                Console.Write("Select an option: ");
                string choice = Console.ReadLine();
                try
                {
                    switch (choice) {
                        case "1":
                            {
                                int col = ReadColumnIndex(finalTableau);
                                RangeResult range = SensitivityAnalyzer.RangeNonBasicVariable(primalResult, col);
                                Console.WriteLine("Range for " + finalTableau.ColumnNames[col] + ": " + range.Describe());
                                break;
                            }
                        case "2":
                            {
                                int col = ReadColumnIndex(finalTableau);
                                Console.Write("New coefficient: ");
                                double newCoeff = double.Parse(Console.ReadLine());
                                SimplexTableau updated = SensitivityAnalyzer.ApplyNonBasicVariableChange(primalResult, col, newCoeff);
                                double newCz = updated.ComputeCjMinusZj()[col];
                                Console.WriteLine("New Cj-Zj for " + finalTableau.ColumnNames[col] + " = " + Math.Round(newCz, 3) +
                                                   (newCz > 1e-9 ? " -> solution is NO LONGER optimal, re-solve required." : " -> solution remains optimal."));
                                break;
                            }
                        case "3":
                            {
                                int row = ReadRowIndex(finalTableau);
                                RangeResult range = SensitivityAnalyzer.RangeBasicVariable(primalResult, row);
                                Console.WriteLine("Range for " + finalTableau.ColumnNames[finalTableau.Basis[row]] + ": " + range.Describe());
                                break;
                            }
                        case "4":
                            {
                                int row = ReadRowIndex(finalTableau);
                                Console.Write("New coefficient: ");
                                double newCoeff = double.Parse(Console.ReadLine());
                                SimplexTableau updated = SensitivityAnalyzer.ApplyBasicVariableChange(primalResult, row, newCoeff);
                                double[] cz = updated.ComputeCjMinusZj();
                                bool stillOptimal = true;
                                for (int j = 0; j < cz.Length; j++)
                                {
                                    if (cz[j] > 1e-9) stillOptimal = false;
                                }
                                Console.WriteLine(stillOptimal ? "Solution remains optimal." : "Solution is NO LONGER optimal, re-solve required.");
                                break;
                            }
                        case "5":
                            {
                                int constraintRow = ReadConstraintIndex(currentModel);
                                RangeResult range = SensitivityAnalyzer.RangeConstraintRhs(currentModel, primalResult, constraintRow);
                                Console.WriteLine("Range for RHS of constraint " + (constraintRow + 1) + ": " + range.Describe());
                                break;
                            }
                        case "6":
                            {
                                int constraintRow = ReadConstraintIndex(currentModel);
                                Console.Write("New RHS: ");
                                double newRhs = double.Parse(Console.ReadLine());
                                SimplexTableau updated = SensitivityAnalyzer.ApplyConstraintRhsChange(currentModel, primalResult, constraintRow, newRhs);
                                bool feasible = true;
                                for (int k = 0; k < updated.RowCount; k++)
                                {
                                    if (updated.B[k] < -1e-6) feasible = false;
                                }
                                Console.WriteLine("New basic values: " + FormatRow(updated.B));
                                Console.WriteLine(feasible ? "Still feasible - basis unchanged." : "INFEASIBLE at this RHS - re-solve required.");
                                break;
                            }
                        case "7":
                            {
                                int constraintRow = ReadConstraintIndex(currentModel);
                                int col = ReadColumnIndex(finalTableau);
                                double[] shadowPrices = SensitivityAnalyzer.ComputeShadowPrices(primalResult);
                                RangeResult range = SensitivityAnalyzer.RangeNonBasicColumnCoefficient(primalResult, shadowPrices, constraintRow, col);
                                Console.WriteLine("Range for coefficient (row " + (constraintRow + 1) + ", " + finalTableau.ColumnNames[col] + "): " + range.Describe());
                                break;
                            }
                        case "8":
                            {
                                int constraintRow = ReadConstraintIndex(currentModel);
                                int col = ReadColumnIndex(finalTableau);
                                Console.Write("New coefficient: ");
                                double newCoeff = double.Parse(Console.ReadLine());
                                SimplexTableau updated = SensitivityAnalyzer.ApplyNonBasicColumnCoefficientChange(primalResult, constraintRow, col, newCoeff);
                                double newCz = updated.ComputeCjMinusZj()[col];
                                Console.WriteLine("New Cj-Zj for " + finalTableau.ColumnNames[col] + " = " + Math.Round(newCz, 3) +
                                                   (newCz > 1e-9 ? " -> solution is NO LONGER optimal, re-solve required." : " -> solution remains optimal."));
                                break;
                            }
                        case "9":
                            {
                                Console.Write("New activity's objective coefficient: ");
                                double objCoeff = double.Parse(Console.ReadLine());
                                double[] constraintCoeffs = new double[currentModel.Constraints.Count];
                                for (int i = 0; i < constraintCoeffs.Length; i++)
                                {
                                    Console.Write("Coefficient in constraint " + (i + 1) + ": ");
                                    constraintCoeffs[i] = double.Parse(Console.ReadLine());
                                }
                                SolveResult newResult = SensitivityStructuralChanges.AddNewActivity(currentModel, objCoeff, constraintCoeffs, SignRestriction.Positive);
                                Console.WriteLine("Re-solved with new activity. Status: " + newResult.Status + ", Objective: " + newResult.ObjectiveValue);
                                Console.WriteLine("(previous objective was " + primalResult.ObjectiveValue + ")");
                                break;
                            }
                        case "10":
                            {
                                double[] coeffs = new double[currentModel.VariableCount];
                                for (int i = 0; i < coeffs.Length; i++)
                                {
                                    Console.Write("Coefficient for x" + (i + 1) + ": ");
                                    coeffs[i] = double.Parse(Console.ReadLine());
                                }
                                Console.Write("Relation (<=, >=, =): ");
                                RelationType relation = ParseRelation(Console.ReadLine());
                                Console.Write("RHS: ");
                                double rhs = double.Parse(Console.ReadLine());
                                SolveResult newResult = SensitivityStructuralChanges.AddNewConstraint(currentModel, coeffs, relation, rhs);
                                Console.WriteLine("Re-solved with new constraint. Status: " + newResult.Status + ", Objective: " + newResult.ObjectiveValue);
                                Console.WriteLine("(previous objective was " + primalResult.ObjectiveValue + ")");
                                break;
                            }
                        case "11":
                            {
                                double[] shadowPrices = SensitivityAnalyzer.ComputeShadowPrices(primalResult);
                                for (int i = 0; i < shadowPrices.Length; i++)
                                {
                                    Console.WriteLine("Shadow price, constraint " + (i + 1) + ": " + Math.Round(shadowPrices[i], 3));
                                }
                                break;
                            }
                        case "12":
                            {
                                LPModel dualModel = DualityAnalyzer.BuildDualModel(currentModel);
                                SolveResult dualResult = DualityAnalyzer.SolveDual(currentModel);
                                Console.WriteLine("Dual objective type: " + dualModel.ObjectiveType);
                                Console.WriteLine("Dual solved. Status: " + dualResult.Status + ", Objective: " + dualResult.ObjectiveValue);
                                Console.WriteLine(DualityAnalyzer.VerifyDuality(primalResult, dualResult));
                                break;
                            }
                        case "0":
                            inSubMenu = false;
                            break;
                        default:
                            Console.WriteLine("Unrecognised option, please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
        private static int ReadColumnIndex(SimplexTableau tableau) {
            Console.Write("Enter column name (e.g., x1, x2, ...): ");
            string name = Console.ReadLine();
            for (int j = 0; j < tableau.ColumnCount; j++)
            {
                if (tableau.ColumnNames[j] == name) { 
                    return j;
                }
            }
            throw new Exception("Column '" + name + "' not found.");
        }
        private static int ReadRowIndex(SimplexTableau tableau)
        {
            Console.Write("Enter row number (1-" + tableau.RowCount + "): ");
            int row = int.Parse(Console.ReadLine()) - 1;
            if (row < 0 || row >= tableau.RowCount) throw new Exception("Row out of range.");
            return row;
        }
        private static int ReadConstraintIndex(LPModel model) {
            Console.Write("Enter contraint number (1-)" + model.Constraints.Count + "): ");
            int idx = int.Parse(Console.ReadLine()) - 1;
            if (idx < 0 || idx >= model.Constraints.Count) throw new Exception("Constraint index out of range.");
            return idx;
        }

        private static RelationType ParseRelation(string text) {
            if (text == "<=") {
                return RelationType.LessOrEqual;
            }
            if (text == ">=") {
                return RelationType.GreaterOrEqual;
            }
            if (text == "=")
            {
                return RelationType.Equal;
            }
            throw new Exception("Relation type must be <=, >=, or = ");
        }

        private static string FormatRow(double[] values)
        {
            string[] parts = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                parts[i] = Math.Round(values[i], 3).ToString();
            }
            return string.Join(", ", parts);
        }
    }
}