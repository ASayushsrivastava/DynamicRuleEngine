using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRuleEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            RuleEngine ruleEngine = new RuleEngine();

            // Connect to the database and show tables
            List<string> tables = GetDatabaseTables();
            if (tables.Count == 0)
            {
                Console.WriteLine("No tables found in the database. Exiting program.");
                return;
            }

            Console.WriteLine("Available Tables:");
            foreach (var table in tables)
            {
                Console.WriteLine($"- {table}");
            }

            Console.WriteLine("\nSelect a table to view columns:");
            string selectedTable = Console.ReadLine();
            List<string> columns = GetTableColumns(selectedTable);

            if (columns.Count == 0)
            {
                Console.WriteLine($"No columns found in the table '{selectedTable}'. Exiting program.");
                return;
            }

            Console.WriteLine($"\nColumns in {selectedTable}:");
            foreach (var column in columns)
            {
                Console.WriteLine($"- {column}");
            }

            // Show available actions
            while (true)
            {
                Console.WriteLine("\nChoose an action:");
                Console.WriteLine("1. Create New Rule");
                Console.WriteLine("2. Combine Existing Rules");
                Console.WriteLine("3. Fetch Data Based on Rule");
                Console.WriteLine("4. Show Existing Rules");
                Console.WriteLine("5. Exit");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        CreateNewRule(ruleEngine);
                        break;

                    case "2":
                        CombineExistingRules(ruleEngine);
                        break;

                    case "3":
                        FetchDataBasedOnRule(ruleEngine);
                        break;

                    case "4":
                        Console.WriteLine("Existing Rules:");
                        ruleEngine.PrintAllRules();
                        if (ruleEngine.GetRulesCount() == 0)
                        {
                            Console.WriteLine("No rules have been created yet.");
                        }
                        break;

                    case "5":
                        return; // Exit the program

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        // Method to create a new rule
        static void CreateNewRule(RuleEngine ruleEngine)
        {
            Console.WriteLine("Existing Rules:");
            ruleEngine.PrintAllRules();

            if (ruleEngine.GetRulesCount() == 0)
            {
                Console.WriteLine("No rules have been created yet.");
            }

            Console.WriteLine("\nEnter the first operand (e.g., a column name like 'age'):");
            string firstOperand = Console.ReadLine();

            Console.WriteLine("Enter the operator (e.g., '>', '<', '=', '>=', '<=', '<>'):");
            string operatorInput = Console.ReadLine();

            Console.WriteLine("Enter the second operand (e.g., a value like '30'):");
            string secondOperand = Console.ReadLine();

            // Construct the condition string
            string condition = $"{firstOperand} {operatorInput} {secondOperand}";
            Node newRule = ruleEngine.CreateRule(condition);

            Console.WriteLine("New Rule Created:");
            newRule.Print();
            Console.WriteLine();
        }

        // Method to combine existing rules
        static void CombineExistingRules(RuleEngine ruleEngine)
        {
            if (ruleEngine.GetRulesCount() < 2)
            {
                Console.WriteLine("Not enough rules available to combine. Please create more rules first.");
                return;
            }

            Console.WriteLine("Enter the index of the first rule to combine:");
            if (!int.TryParse(Console.ReadLine(), out int ruleIndex1) || ruleIndex1 < 0 || ruleIndex1 >= ruleEngine.GetRulesCount())
            {
                Console.WriteLine("Invalid index for the first rule.");
                return;
            }

            Console.WriteLine("Enter the index of the second rule to combine:");
            if (!int.TryParse(Console.ReadLine(), out int ruleIndex2) || ruleIndex2 < 0 || ruleIndex2 >= ruleEngine.GetRulesCount())
            {
                Console.WriteLine("Invalid index for the second rule.");
                return;
            }

            Console.WriteLine("Enter operation (AND/OR):");
            string operation = Console.ReadLine().ToUpper();

            Node combinedRule = ruleEngine.CombineRules(
                ruleEngine.GetRule(ruleIndex1),
                ruleEngine.GetRule(ruleIndex2),
                operation
            );

            Console.WriteLine("Combined Rule:");
            combinedRule.Print();
            Console.WriteLine();
        }

        // Method to fetch data from the database based on a rule
        static void FetchDataBasedOnRule(RuleEngine ruleEngine)
        {
            if (ruleEngine.GetRulesCount() == 0)
            {
                Console.WriteLine("No rules have been created yet. Please create a rule first.");
                return;
            }

            Console.Write("Enter the schema name (e.g., 'production', 'sales'): ");
            string schemaName = Console.ReadLine();

            Console.Write("Enter the table name (e.g., 'products'): ");
            string tableName = Console.ReadLine();

            Console.WriteLine("Select an option for fetching data:");
            Console.WriteLine("1. Use an existing rule");
            Console.WriteLine("2. Create a new rule");

            string fetchChoice = Console.ReadLine();
            string condition = string.Empty;

            if (fetchChoice == "1")
            {
                Console.WriteLine("Existing Rules:");
                ruleEngine.PrintAllRules();

                if (ruleEngine.GetRulesCount() == 0)
                {
                    Console.WriteLine("No rules found. Exiting data fetch operation.");
                    return;
                }

                Console.WriteLine("Enter the index of the rule you want to use:");
                if (!int.TryParse(Console.ReadLine(), out int ruleIndex) || ruleIndex < 0 || ruleIndex >= ruleEngine.GetRulesCount())
                {
                    Console.WriteLine("Invalid rule selection.");
                    return;
                }

                Node selectedRule = ruleEngine.GetRule(ruleIndex);
                condition = selectedRule.GetCondition(); // Use GetCondition method
            }
            else if (fetchChoice == "2")
            {
                Console.WriteLine("Creating a new rule.");
                condition = CreateNewRuleCondition(); // Use the method to create a new condition
                Node newRule = ruleEngine.CreateRule(condition);

                Console.WriteLine("New Rule Created:");
                newRule.Print();
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Invalid choice.");
                return;
            }
        }

        // Helper method to create new rule condition
        static string CreateNewRuleCondition()
        {
            Console.WriteLine("\nEnter the first operand (e.g., a column name like 'age'):");
            string firstOperand = Console.ReadLine();

            Console.WriteLine("Enter the operator (e.g., '>', '<', '=', '>=', '<=', '<>'):");
            string operatorInput = Console.ReadLine();

            Console.WriteLine("Enter the second operand (e.g., a value like '30'):");
            string secondOperand = Console.ReadLine();

            // Construct the condition string
            return $"{firstOperand} {operatorInput} {secondOperand}";
        }

        // Get all tables from the database
        static List<string> GetDatabaseTables()
        {
            List<string> tables = new List<string>();
            string connectionString = RuleEngine.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
                SqlCommand command = new SqlCommand(query, conn);
                try
                {
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tables.Add(reader["TABLE_NAME"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching tables: {ex.Message}");
                }
            }
            return tables;
        }

        // Get columns from the selected table
        static List<string> GetTableColumns(string tableName)
        {
            List<string> columns = new List<string>();
            string connectionString = RuleEngine.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName";
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@tableName", tableName);
                try
                {
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        columns.Add(reader["COLUMN_NAME"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching columns for table '{tableName}': {ex.Message}");
                }
            }
            return columns;
        }
    }

}
