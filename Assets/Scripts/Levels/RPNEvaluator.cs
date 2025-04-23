using System;
using System.Collections.Generic;
using UnityEngine;

public class RPNEvaluator
{
    public static int Evaluate(string expression, Dictionary<string, int> variables = null)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return 0;
        }
        
        if (int.TryParse(expression, out int simpleValue))
        {
            return simpleValue;
        }
        
        Stack<int> stack = new Stack<int>();
        
        string[] tokens = expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string token in tokens)
        {
            if (token == "+" || token == "-" || token == "*" || token == "/" || token == "%")
            {
                if (stack.Count < 2)
                {
                    return 0;
                }
                
                int b = stack.Pop();
                int a = stack.Pop();
                
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
                        if (b == 0)
                        {
                            return 0;
                        }
                        stack.Push(a / b);
                        break;
                    case "%":
                        if (b == 0)
                        {
                            return 0;
                        }
                        stack.Push(a % b);
                        break;
                }
            }
            else if (variables != null && variables.TryGetValue(token, out int value))
            {
                stack.Push(value);
            }
            else if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            else
            {
                stack.Push(0); 
            }
        }
        
        if (stack.Count != 1)
        {
            return stack.Count > 0 ? stack.Pop() : 0;
        }
        
        return stack.Pop();
    }
} 
