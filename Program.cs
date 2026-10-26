
#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

enum ExpenseCategory
{
    Food,
    Transport,
    Utilities,
    Entertainment,
    Other
}

sealed class Expense
{
    public string Description { get; }
    public decimal Amount { get; }
    public ExpenseCategory Category { get; }
    public DateOnly Date { get; }
    public string? Note { get; }

    public Expense(
        string description,
        decimal amount,
        ExpenseCategory category,
        DateOnly date,
        string? note)
    {
        Description = description;
        Amount = amount;
        Category = category;
        Date = date;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}

static class BudgetRules
{
    public static string ClassifyAmount(decimal amount)
    {
        if (amount < 25m)
            return "Small expense";

        if (amount < 100m)
            return "Medium expense";

        return "Large expense";
    }

    public static string BudgetStatus(decimal budget, decimal spent)
    {
        if (spent > budget)
            return "Over budget";

        decimal remaining = budget - spent;

        if (remaining / budget < 0.10m)
            return "Almost out";

        return "On track";
    }
}

class Program
{
    private const decimal MaxExpenseAmount = 1_000_000m;

    private static readonly List<Expense> Expenses = new();
    private static decimal? monthlyBudget;

    private static decimal OverallTotal =>
        Expenses.Sum(e => e.Amount);

    private static int ExpenseCount => Expenses.Count;

    private static Expense? HighestExpense =>
        Expenses.Count == 0
            ? null
            : Expenses.MaxBy(e => e.Amount);

    private static decimal CurrentMonthTotal =>
        Expenses
            .Where(e =>
                e.Date.Year == DateTime.Today.Year &&
                e.Date.Month == DateTime.Today.Month)
            .Sum(e => e.Amount);

    static void Main()
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n=== Expense Tracker ===");
            Console.WriteLine("1. Add expense");
            Console.WriteLine("2. View summary");
            Console.WriteLine("3. Set monthly budget");
            Console.WriteLine("4. Exit");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    AddExpense();
                    break;

                case "2":
                    ViewSummary();
                    break;

                case "3":
                    SetMonthlyBudget();
                    break;

                case "4":
                    running = false;
                    break;

                default:
                    Console.WriteLine(
                        "Invalid option. Please enter 1, 2, 3, or 4.");
                    break;
            }
        }

        Console.WriteLine("\nThank you for using the Expense Tracker!");
        Console.WriteLine("Goodbye!");
    }

    private static void AddExpense()
    {
        Console.WriteLine("\n--- Add Expense ---");

        string description;

        do
        {
            Console.Write("Description: ");
            description = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(description))
                Console.WriteLine("Description cannot be empty.");

        } while (string.IsNullOrWhiteSpace(description));

        decimal amount;

        while (true)
        {
            Console.Write("Amount: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (decimal.TryParse(
                    input,
                    NumberStyles.AllowDecimalPoint |
                    NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out amount)
                && amount > 0m
                && amount <= MaxExpenseAmount)
            {
                break;
            }

            Console.WriteLine(
                $"Enter a valid amount greater than zero " +
                $"and no more than {MaxExpenseAmount:N0}.");
        }

        ExpenseCategory category;

        while (true)
        {
            Console.Write("Category (Food, Transport, Utilities, " +
                          "Entertainment, Other): ");

            string input = Console.ReadLine()?.Trim() ?? "";

            if (Enum.TryParse(input, true, out category) &&
                Enum.IsDefined(typeof(ExpenseCategory), category))
            {
                break;
            }

            Console.WriteLine(
                "Invalid category. Please try again.");
        }

        DateOnly date;

        while (true)
        {
            Console.Write("Date (yyyy-mm-dd, blank for today): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(input))
            {
                date = DateOnly.FromDateTime(DateTime.Today);
                break;
            }

            if (DateOnly.TryParseExact(
                    input,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out date)
                && date <= DateOnly.FromDateTime(DateTime.Today))
            {
                break;
            }

            Console.WriteLine(
                "Enter a valid date in yyyy-mm-dd format " +
                "that is not in the future.");
        }

        Console.Write("Optional note (press Enter to skip): ");
        string? note = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(note))
            note = null;
        else
            note = note.Trim();

        var expense = new Expense(
            description, amount, category, date, note);

        // Update the collection only after all inputs are valid.
        Expenses.Add(expense);

        Console.WriteLine("\nExpense added successfully!");
        Console.WriteLine($"Amount band: " +
                          $"{BudgetRules.ClassifyAmount(amount)}");
        Console.WriteLine($"Overall total: {OverallTotal:C}");
        Console.WriteLine($"Expense count: {ExpenseCount}");

        if (HighestExpense is Expense highest)
        {
            Console.WriteLine(
                $"Highest expense: {highest.Description} " +
                $"({highest.Amount:C})");
        }

        decimal categoryTotal = Expenses
            .Where(e => e.Category == category)
            .Sum(e => e.Amount);

        Console.WriteLine(
            $"{category} category total: {categoryTotal:C}");

        if (monthlyBudget.HasValue)
        {
            decimal remaining = monthlyBudget.Value -
                                CurrentMonthTotal;

            Console.WriteLine(
                $"Current-month budget status: " +
                $"{BudgetRules.BudgetStatus(
                    monthlyBudget.Value, CurrentMonthTotal)}");

            Console.WriteLine(
                $"Budget remaining: {remaining:C}");
        }
    }

    private static void ViewSummary()
    {
        Console.WriteLine("\n--- Expense Summary ---");

        if (Expenses.Count == 0)
        {
            Console.WriteLine(
                "No expenses recorded yet. Add an expense to get started.");
        }
        else
        {
            Console.WriteLine($"Expense count: {ExpenseCount}");
            Console.WriteLine($"Total spent: {OverallTotal:C}");
            Console.WriteLine(
                $"Average expense: {OverallTotal / ExpenseCount:C}");

            if (HighestExpense is Expense highest)
            {
                Console.WriteLine(
                    $"Highest single expense: {highest.Description} " +
                    $"({highest.Amount:C}) on " +
                    $"{highest.Date:yyyy-MM-dd}");
            }

            Console.WriteLine("\nTotals by category:");

            foreach (ExpenseCategory category
                     in Enum.GetValues<ExpenseCategory>())
            {
                decimal total = Expenses
                    .Where(e => e.Category == category)
                    .Sum(e => e.Amount);

                Console.WriteLine($"  {category}: {total:C}");
            }
        }

        Console.WriteLine("\n--- Monthly Budget ---");

        if (monthlyBudget.HasValue)
        {
            decimal spent = CurrentMonthTotal;
            decimal remaining = monthlyBudget.Value - spent;

            Console.WriteLine(
                $"Monthly budget: {monthlyBudget.Value:C}");
            Console.WriteLine(
                $"Spent this month: {spent:C}");
            Console.WriteLine(
                $"Remaining: {remaining:C}");
            Console.WriteLine(
                $"Status: {BudgetRules.BudgetStatus(
                    monthlyBudget.Value, spent)}");
        }
        else
        {
            Console.WriteLine("No monthly budget has been set.");
        }
    }

    private static void SetMonthlyBudget()
    {
        Console.WriteLine("\n--- Set Monthly Budget ---");

        while (true)
        {
            Console.Write("Enter your monthly budget: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (decimal.TryParse(
                    input,
                    NumberStyles.AllowDecimalPoint |
                    NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out decimal budget)
                && budget > 0m
                && budget <= MaxExpenseAmount)
            {
                monthlyBudget = budget;

                Console.WriteLine(
                    $"Monthly budget set to {budget:C}.");

                Console.WriteLine(
                    $"Spent this month: {CurrentMonthTotal:C}");

                Console.WriteLine(
                    $"Remaining: {budget - CurrentMonthTotal:C}");

                Console.WriteLine(
                    $"Status: {BudgetRules.BudgetStatus(
                        budget, CurrentMonthTotal)}");

                return;
            }

            Console.WriteLine(
                "Invalid budget. Enter a positive number " +
                $"no greater than {MaxExpenseAmount:N0}.");
        }
    }
}

