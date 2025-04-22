using UnityEngine;
using System.Collections.Generic;

public class RPNEvaluatorTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("===== RPN EVALUATOR TEST =====");
        
        // Test basic operations
        TestRPN("5 3 +", 8);
        TestRPN("10 5 -", 5);
        TestRPN("4 3 *", 12);
        TestRPN("20 4 /", 5);
        TestRPN("10 3 %", 1);
        
        // Test more complex expressions
        TestRPN("5 3 + 2 *", 16);
        TestRPN("10 2 * 5 + 3 -", 22);
        TestRPN("10 5 2 * +", 20);
        
        // Test with variables
        Dictionary<string, int> vars = new Dictionary<string, int>
        {
            { "base", 20 },
            { "wave", 3 }
        };
        
        TestRPN("base 5 wave * +", 35, vars);
        TestRPN("wave 3 /", 1, vars);
        TestRPN("base wave *", 60, vars);
        
        // Test some of the actual expressions from levels.json
        TestRPN("5 wave +", 8, vars);  // zombie count in wave 3 for Easy
        TestRPN("wave 3 /", 1, vars);  // skeleton count in wave 3 for Easy
        TestRPN("wave 5 / 1 wave 5 % - *", 0, vars);  // warlock count in wave 3 for Easy
        TestRPN("30 wave *", 90, vars);  // skeleton HP in wave 3 for Easy
        
        Debug.Log("===== RPN EVALUATOR TEST COMPLETE =====");
    }
    
    void TestRPN(string expression, int expected, Dictionary<string, int> variables = null)
    {
        int result = RPNEvaluator.Evaluate(expression, variables);
        
        if (result == expected)
        {
            Debug.Log($"✅ Expression '{expression}' = {result}, as expected");
        }
        else
        {
            Debug.LogError($"❌ Expression '{expression}' = {result}, but expected {expected}");
        }
    }
} 