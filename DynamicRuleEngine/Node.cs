using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRuleEngine
{
    public enum NodeType
    {
        Operand,
        And,
        Or
    }

    public class Node
    {
        public NodeType Type { get; set; } // Type of the node
        public Node Left { get; set; }      // Left child node
        public Node Right { get; set; }     // Right child node
        public string Value { get; set; }   // Condition or operator value

        // Constructor for operand nodes
        public Node(NodeType type, string value)
        {
            Type = type;
            Value = value;
            Left = null;
            Right = null;
        }

        // Constructor for operator nodes (AND/OR)
        public Node(NodeType type, Node left, Node right)
        {
            Type = type;
            Left = left;
            Right = right;
            Value = type == NodeType.And ? "AND" : "OR"; // Set the operator string based on the type
        }

        // Non-recursive method to print the node condition
        public void Print()
        {
            Stack<Node> stack = new Stack<Node>();
            Node current = this;

            while (stack.Count > 0 || current != null)
            {
                if (current != null)
                {
                    // Traverse to the leftmost node
                    stack.Push(current);
                    current = current.Left;
                }
                else
                {
                    // Process the node
                    current = stack.Pop();

                    if (current.Type == NodeType.Operand)
                    {
                        Console.Write(current.Value);
                    }
                    else if (current.Type == NodeType.And || current.Type == NodeType.Or)
                    {
                        Console.Write("(");
                        Console.Write($" {current.Value} ");
                        current = current.Right; // Move to the right child after printing operator
                        continue; // To skip the stack push for this node
                    }

                    // Move to the right child
                    current = current.Right;
                }
            }
        }

        // Non-recursive method to get the full condition as a string
        public string GetCondition()
        {
            Stack<Node> stack = new Stack<Node>();
            Node current = this;
            List<string> result = new List<string>();

            while (stack.Count > 0 || current != null)
            {
                if (current != null)
                {
                    stack.Push(current);
                    current = current.Left;
                }
                else
                {
                    current = stack.Pop();

                    if (current.Type == NodeType.Operand)
                    {
                        result.Add(current.Value);
                    }
                    else if (current.Type == NodeType.And || current.Type == NodeType.Or)
                    {
                        // Ensure there are at least two conditions to combine
                        if (result.Count < 2)
                        {
                            throw new InvalidOperationException("Not enough operands to combine.");
                        }

                        // Pop the last two operands
                        string rightCondition = result[result.Count - 1];
                        result.RemoveAt(result.Count - 1); // Remove last (right operand)

                        string leftCondition = result[result.Count - 1];
                        result.RemoveAt(result.Count - 1); // Remove one before last (left operand)

                        // Create combined condition without extra parentheses
                        string combinedCondition = $"({leftCondition} {current.Value} {rightCondition})";
                        result.Add(combinedCondition);
                    }

                    current = current.Right;
                }
            }

            // Return the final combined condition, if it exists
            return result.Count > 0 ? result[0] : string.Empty;
        }

        public string ToConditionString()
        {
            return GetCondition(); // Reuse GetCondition() for consistency
        }

        // Override ToString for easier debugging
        public override string ToString()
        {
            return GetCondition();
        }
    }
}
