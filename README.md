# LPR381-LP-IP-Solver
 
A menu-driven C# (.NET) console application, **`solve.exe`**, that solves Linear Programming (LP) and Integer Programming (IP) models from a text-based input file, displays canonical forms and full tableau/iteration output, and performs post-optimal sensitivity analysis.

## Overview
Operations Research uses mathematical models to support decision making under resource constraints. This project implements a program that:
- Accepts a linear or integer programming model from an input text file.
- Solves it using a chosen algorithm (Simplex, Branch & Bound, Cutting Plane, or Knapsack Branch & Bound).
- Displays the canonical form and all solution iterations.
- Performs a range of sensitivity analysis operations on the optimal solution.
- Exports all results to an output text file.
## Project Requirements
Minimum requirements:
- Menu-driven console application (`solve.exe`), built as a Visual Studio project in C#.
- Accepts a **random (variable) number of decision variables**.
- Accepts a **random (variable) number of constraints**.
- Code must include comments and follow programming best practices.
- Reads the model from an input text file and writes all results to an output text file.
## Input File Format
The input is a plain text file describing the LP/IP model (not a canonical or relaxed form).
 
**Line 1 — Objective function:**
`max`/`min`, followed by a sign (`+`/`-`) and coefficient for each decision variable.
 
**Following lines — one per constraint:**
Sign and coefficient for each decision variable (same order as the objective function), a relational operator (`<=`, `>=`, `=`), and the right-hand-side value.
 
**Final line — Sign restrictions:**
One symbol per decision variable, in order: `+`, `-`, `urs`, `int`, or `bin`.
 
### Example (Knapsack IP)
```
max +2 +3 +3 +5 +2 +4
+11 +8 +6 +14 +10 +10 <=40
bin bin bin bin bin bin
```
 
## Output File Format
- Contains the canonical form of the model.
- Contains all tableau/iteration steps of the algorithm selected to solve it.
- All decimal values rounded to **three decimal places**.
## Algorithms
The program must offer a choice of the following algorithms:
- **Primal Simplex Algorithm** — canonical form + all tableau iterations.
- **Revised Primal Simplex Algorithm** — canonical form + all Product Form and Price Out iterations.
- **Branch & Bound Simplex Algorithm** (or Revised variant) — with backtracking, generation of all sub-problems, fathoming of all nodes, all tableau iterations per sub-problem, and the best candidate solution displayed.
- **Cutting Plane Algorithm** (or Revised variant) — canonical form + all Product Form and Price Out iterations.
- **Branch & Bound Knapsack Algorithm** — with backtracking, generation of all sub-problems, fathoming of all nodes, all iterations, and the best candidate solution displayed.
## Sensitivity Analysis
Post-optimal analysis options to be supported:
- Range and apply-a-change for a selected **Non-Basic Variable**.
- Range and apply-a-change for a selected **Basic Variable**.
- Range and apply-a-change for a selected **constraint right-hand-side**.
- Range and apply-a-change for a selected **variable in a Non-Basic Variable column**.
- Add a new activity to an optimal solution.
- Add a new constraint to an optimal solution.
- Display shadow prices.
- **Duality:** apply duality to the model, solve the dual, and verify strong/weak duality.
## Special Cases
The program must detect and correctly resolve/report:
- Infeasible models.
- Unbounded models.
 
## Tech Stack
- **Language:** C#
- **Platform:** .NET (Visual Studio project)
- **Output:** Console application, built as `solve.exe`
## Getting Started
> Setup instructions will be added once the project structure is in place.
 
## Team
| Name | Role |
|------|------|
| Ruan Wolmarans  | TBC  |
| Viljoen Steenkamp  | TBC  |
