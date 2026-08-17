using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using LPR381Solver.Models;

namespace LPR381Solver.Parsing
{
    // Reads the plain-text input file format defined by the project brief and produces an LPModel.
    //
    // File format:
    //   Line 1      : max/min  +c1 +c2 ... +cn
    //   Line 2..m+1 : +a1 +a2 ... +an <=b      (one line per constraint; relation token has no
    //                                            space before the right-hand-side number)
    //   Last line   : sign restrictions, one token per variable (+, -, urs, int, bin)
    public static class InputParser
    {
        public static LPModel ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Input file not found: " + filePath);
            }

            string[] rawLines = File.ReadAllLines(filePath);
            List<string> lines = new List<string>();
            for (int i = 0; i < rawLines.Length; i++)
            {
                string trimmed = rawLines[i].Trim();
                if (trimmed.Length > 0)
                {
                    lines.Add(trimmed);
                }
            }

            if (lines.Count < 3)
            {
                throw new FormatException(
                    "Input file must contain an objective line, at least one constraint line, and a sign restriction line.");
            }

            LPModel model = new LPModel();

            // Line 1: objective function.
            ParseObjectiveLine(lines[0], model);

            // Last line: sign restrictions.
            ParseSignRestrictionLine(lines[lines.Count - 1], model);

            // Middle lines: constraints.
            for (int i = 1; i < lines.Count - 1; i++)
            {
                Constraint constraint = ParseConstraintLine(lines[i], model.VariableCount, i + 1);
                model.Constraints.Add(constraint);
            }

            Validate(model);
            return model;
        }

        private static void ParseObjectiveLine(string line, LPModel model)
        {
            string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                throw new FormatException("Objective line must contain 'max'/'min' followed by coefficients.");
            }

            string direction = tokens[0].ToLowerInvariant();
            if (direction == "max")
            {
                model.ObjectiveType = ObjectiveType.Maximize;
            }
            else if (direction == "min")
            {
                model.ObjectiveType = ObjectiveType.Minimize;
            }
            else
            {
                throw new FormatException("Objective line must start with 'max' or 'min', found '" + tokens[0] + "'.");
            }

            for (int i = 1; i < tokens.Length; i++)
            {
                model.ObjectiveCoefficients.Add(ParseSignedNumber(tokens[i], "objective coefficient"));
            }
        }

        private static Constraint ParseConstraintLine(string line, int nVars, int lineNumber)
        {
            string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != nVars + 1)
            {
                throw new FormatException(
                    "Constraint line " + lineNumber + " has " + (tokens.Length - 1) +
                    " coefficient(s) but the objective function defines " + nVars + " decision variable(s).");
            }

            List<double> coeffs = new List<double>();
            for (int i = 0; i < nVars; i++)
            {
                coeffs.Add(ParseSignedNumber(tokens[i], "constraint coefficient"));
            }

            RelationType relation;
            double rhs;
            ParseRelationToken(tokens[nVars], out relation, out rhs);

            return new Constraint(coeffs, relation, rhs);
        }

        private static void ParseSignRestrictionLine(string line, LPModel model)
        {
            string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != model.VariableCount)
            {
                throw new FormatException(
                    "Sign restriction line has " + tokens.Length + " entry(ies) but the objective function defines " +
                    model.VariableCount + " decision variable(s).");
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i].ToLowerInvariant();
                SignRestriction restriction;

                if (token == "+") restriction = SignRestriction.Positive;
                else if (token == "-") restriction = SignRestriction.Negative;
                else if (token == "urs") restriction = SignRestriction.Urs;
                else if (token == "int") restriction = SignRestriction.Int;
                else if (token == "bin") restriction = SignRestriction.Bin;
                else throw new FormatException("Unknown sign restriction token '" + tokens[i] + "'. Expected one of: +, -, urs, int, bin.");

                model.SignRestrictions.Add(restriction);
            }
        }

        // Parses a coefficient token that always carries an explicit leading sign, e.g. "+2", "-3.5".
        private static double ParseSignedNumber(string token, string context)
        {
            if (token.Length < 2 || (token[0] != '+' && token[0] != '-'))
            {
                throw new FormatException("Expected a signed " + context + " like '+2' or '-3.5', found '" + token + "'.");
            }

            double value;
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                throw new FormatException("Could not parse " + context + " '" + token + "' as a number.");
            }

            return value;
        }

        // Parses a combined relation+RHS token such as "<=40", ">=15.5", "=10".
        private static void ParseRelationToken(string token, out RelationType relation, out double rhs)
        {
            string opText;

            if (token.StartsWith("<="))
            {
                relation = RelationType.LessOrEqual;
                opText = "<=";
            }
            else if (token.StartsWith(">="))
            {
                relation = RelationType.GreaterOrEqual;
                opText = ">=";
            }
            else if (token.StartsWith("="))
            {
                relation = RelationType.Equal;
                opText = "=";
            }
            else
            {
                throw new FormatException("Constraint must end with a relation token starting with '<=', '>=' or '=', found '" + token + "'.");
            }

            string numberPart = token.Substring(opText.Length);
            if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out rhs))
            {
                throw new FormatException("Could not parse right-hand-side value from '" + token + "'.");
            }
        }

        private static void Validate(LPModel model)
        {
            if (model.VariableCount == 0)
            {
                throw new FormatException("Model must define at least one decision variable.");
            }
            if (model.Constraints.Count == 0)
            {
                throw new FormatException("Model must define at least one constraint.");
            }
            foreach (Constraint c in model.Constraints)
            {
                if (c.Coefficients.Count != model.VariableCount)
                {
                    throw new FormatException("All constraints must reference the same number of decision variables as the objective function.");
                }
            }
        }
    }
}
