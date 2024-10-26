using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRuleEngine
{
    public class RuleEngine
    {
        // Store created rules
        private List<Node> rules = new List<Node>();

        // Connection String to Microsoft SQL Server BikeStores database
        public static string GetConnectionString()
        {
            return "Server=AYUSH\\SQLEXPRESS; Database=BikeStores; Trusted_Connection=True;";
        }

        // Create a single rule (convert from string to AST Node)
        public Node CreateRule(string condition)
        {
            if (IsValidCondition(condition))
            {
                Node ruleNode = new Node(NodeType.Operand, condition); // Use NodeType.Operand
                rules.Add(ruleNode);
                return ruleNode;
            }
            else
            {
                Console.WriteLine("Invalid condition. Please provide a boolean expression (e.g., 'store_id = 1').");
                return null;
            }
        }

        // Check if the condition is valid
        private bool IsValidCondition(string condition)
        {
            return !string.IsNullOrWhiteSpace(condition) &&
                   (condition.Contains("=") || condition.Contains(">") || condition.Contains("<") || condition.Contains("!="));
        }

        // Combine two rules using AND or OR operators
        public Node CombineRules(Node rule1, Node rule2, string operation)
        {
            NodeType opType;

            // Determine the operation type
            if (operation.Equals("AND", StringComparison.OrdinalIgnoreCase))
            {
                opType = NodeType.And;
            }
            else if (operation.Equals("OR", StringComparison.OrdinalIgnoreCase))
            {
                opType = NodeType.Or;
            }
            else
            {
                throw new ArgumentException("Invalid operation. Use 'AND' or 'OR'.");
            }

            // Create an operator node combining the two rules
            Node operatorNode = new Node(opType, rule1, rule2);
            rules.Add(operatorNode); // Add the new combined rule to the list of rules

            return operatorNode;
        }

        public static void CombineExistingRules(RuleEngine ruleEngine)
        {
            if (ruleEngine.GetRulesCount() < 2)
            {
                Console.WriteLine("Not enough rules available to combine. Please create more rules first.");
                return;
            }

            Console.WriteLine("Enter the index of the first rule to combine:");
            if (!int.TryParse(Console.ReadLine(), out int ruleIndex1) || ruleIndex1 < 0 || ruleIndex1 >= ruleEngine.GetRulesCount())
            {
                Console.WriteLine("Invalid rule index.");
                return;
            }

            Console.WriteLine("Enter the index of the second rule to combine:");
            if (!int.TryParse(Console.ReadLine(), out int ruleIndex2) || ruleIndex2 < 0 || ruleIndex2 >= ruleEngine.GetRulesCount())
            {
                Console.WriteLine("Invalid rule index.");
                return;
            }

            Console.WriteLine("Enter operation (AND/OR):");
            string operation = Console.ReadLine();

            // Retrieve the rules to combine
            Node rule1 = ruleEngine.GetRule(ruleIndex1);
            Node rule2 = ruleEngine.GetRule(ruleIndex2);

            if (rule1 == null || rule2 == null)
            {
                Console.WriteLine("Invalid rule selection. Please try again.");
                return;
            }

            // Combine the rules
            Node combinedRule = ruleEngine.CombineRules(rule1, rule2, operation);
            Console.WriteLine("Combined Rule:");
            Console.WriteLine(combinedRule); // Print the combined rule
        }

        // Fetch data from SQL Server based on conditions
        public void FetchDataFromDatabase(string schemaName, string tableName, int ruleIndex)
        {
            if (ruleIndex < 0 || ruleIndex >= rules.Count)
            {
                Console.WriteLine("Invalid rule index.");
                return;
            }

            // Get the condition string from the selected rule
            Node selectedRule = rules[ruleIndex];
            string condition = selectedRule.Value;

            if (string.IsNullOrWhiteSpace(condition))
            {
                Console.WriteLine("Condition cannot be empty.");
                return;
            }

            string query = $"SELECT * FROM {schemaName}.{tableName} WHERE {condition}";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand command = new SqlCommand(query, conn);
                try
                {
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        Console.WriteLine("No data found for the specified condition.");
                        return;
                    }

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader[i] + "\t");
                        }
                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // Retrieve a specific rule based on index
        public Node GetRule(int index)
        {
            if (index >= 0 && index < rules.Count)
            {
                return rules[index];
            }
            else
            {
                Console.WriteLine("Invalid rule index.");
                return null;
            }
        }

        // Print all existing rules
        public void PrintAllRules()
        {
            if (rules.Count == 0)
            {
                Console.WriteLine("No rules available.");
                return;
            }

            Console.WriteLine("Listing All Rules:");
            for (int i = 0; i < rules.Count; i++)
            {
                Console.Write($"{i}: ");
                Console.WriteLine(rules[i]); // Simply print the value of the Node
            }
        }

        // Get the count of existing rules
        public int GetRulesCount()
        {
            return rules.Count;
        }
    }
}