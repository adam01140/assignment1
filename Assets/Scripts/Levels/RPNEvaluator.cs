using System;
using System.Collections.Generic;
using UnityEngine;

public class RPNEvaluator
{
    /// <summary>
    /// Evaluates a Reverse Polish Notation (RPN) expression
    /// </summary>
    /// <param name="expression">The RPN expression as a string</param>
    /// <param name="variables">Dictionary of variable names and their values</param>
    /// <returns>The result of the evaluation</returns>
    public static int Evaluate(string expression, Dictionary<string, int> variables = null)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return 0;
        }
        
        // If the expression is a simple integer, just return it
        if (int.TryParse(expression, out int simpleValue))
        {
            return simpleValue;
        }
        
        // Initialize stack for calculations
        Stack<int> stack = new Stack<int>();
        
        // Split the expression into tokens
        string[] tokens = expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string token in tokens)
        {
            // Check if token is an operator
            if (token == "+" || token == "-" || token == "*" || token == "/" || token == "%")
            {
                // Need at least two operands for operations
                if (stack.Count < 2)
                {
                    Debug.LogError($"Invalid RPN expression: {expression}. Not enough operands for operator {token}");
                    return 0;
                }
                
                // Pop the two operands (note: second operand was pushed first)
                int b = stack.Pop();
                int a = stack.Pop();
                
                // Perform the operation
                switch (token)
                {
                    case "+":
                        stack.Push(a + b);
                        break;
                    case "-":
                        stack.Push(a - b);
                        break;
                    case "*":
                        stack.Push(a * b);
                        break;
                    case "/":
                        // Check for division by zero
                        if (b == 0)
                        {
                            Debug.LogError($"Division by zero in RPN expression: {expression}");
                            return 0;
                        }
                        stack.Push(a / b);
                        break;
                    case "%":
                        // Check for modulo by zero
                        if (b == 0)
                        {
                            Debug.LogError($"Modulo by zero in RPN expression: {expression}");
                            return 0;
                        }
                        stack.Push(a % b);
                        break;
                }
            }
            // Check if token is a variable
            else if (variables != null && variables.TryGetValue(token, out int value))
            {
                stack.Push(value);
            }
            // Otherwise, try to parse token as a number
            else if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            else
            {
                Debug.LogWarning($"Unknown token in RPN expression: {token}");
                stack.Push(0); // Default to 0 for unknown tokens
            }
        }
        
        // The final result should be the only item on the stack
        if (stack.Count != 1)
        {
            Debug.LogWarning($"Invalid RPN expression: {expression}. Final stack has {stack.Count} items.");
            return stack.Count > 0 ? stack.Pop() : 0;
        }
        
        return stack.Pop();
    }
} 