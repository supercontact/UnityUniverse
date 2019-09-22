using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperContact.MathExpression {

    public class ExpressionParser {
        public static Expression Parse(string expression, bool simplify = true) {
            expression = SanitizeExpressionString(expression);
            EnsureMatchingParentheses(expression);

            var postfixList = new List<Expression>();
            int currentIndex = 0;
            Infix2Postfix(expression, ref currentIndex, postfixList);
            Expression result = Postfix2Expression(postfixList, expression);
            return simplify ? result.Simplify() : result;
        }

        public static Dictionary<string, Expression> ParseAssignExpressions(string combinedExpressions) {
            var result = new Dictionary<string, Expression>();
            return ParseAssignExpressions(combinedExpressions.Split(','));
        }

        public static Dictionary<string, Expression> ParseAssignExpressions(params string[] expressions) {
            var result = new Dictionary<string, Expression>();
            foreach (string assignExpression in expressions) {
                string[] variableNameAndExpression = assignExpression.Split('=');
                if (variableNameAndExpression.Length != 2) {
                    throw new ExpressionParseException($"Assign expression should have the form '[variableName] = [expression]', but received '{assignExpression}'");
                }
                string variableName = SanitizeExpressionString(variableNameAndExpression[0]);
                result.Add(variableName, Parse(variableNameAndExpression[1]));
            }
            return result;
        }

        private static void Infix2Postfix(string expression, ref int currentIndex, List<Expression> result) {
            Stack<Operation> operatorStack = new Stack<Operation>();
            bool lastTokenIsValue = false;
            while (currentIndex < expression.Length) {
                if (expression[currentIndex] == '(') {
                    currentIndex++;
                    Infix2Postfix(expression, ref currentIndex, result);
                    lastTokenIsValue = true;
                } else if (expression[currentIndex] == ')') {
                    currentIndex++;
                    break;
                } else {
                    Expression token = GetToken(expression, ref currentIndex);
                    if (token is Value || token is Variable) {
                        result.Add(token);
                        lastTokenIsValue = true;
                    } else {
                        Operation op = token as Operation;
                        if (!lastTokenIsValue) {
                            if (op.op == Operator.Plus) {
                                op = new Operation(Operator.PlusUnary, null);
                            }
                            if (op.op == Operator.Minus) {
                                op = new Operation(Operator.MinusUnary, null);
                            }
                        }
                        while (operatorStack.Count > 0 && !op.isUnary && GetOperatorPriority(operatorStack.Peek().op) >= GetOperatorPriority(op.op)) {
                            result.Add(operatorStack.Pop());
                        }
                        operatorStack.Push(op);
                        lastTokenIsValue = false;
                    }
                }
            }
            while (operatorStack.Count > 0) {
                result.Add(operatorStack.Pop());
            }
        }

        private static Expression Postfix2Expression(List<Expression> postfixList, string originalExpression) {
            if (postfixList.Count == 0) throw new ExpressionParseException($"Expression is empty");

            Stack<Expression> expressionStack = new Stack<Expression>();
            foreach (Expression e in postfixList) {
                if (e is Operation) {
                    Operation op = e as Operation;
                    if (op.isUnary) {
                        if (expressionStack.Count < 1) {
                            throw new ExpressionParseException($"No more element to use for unary operation '{op.op.String()}' in expression '{originalExpression}'");
                        }
                        Expression rightChild = expressionStack.Pop();
                        expressionStack.Push(new Operation(op.op, rightChild));
                    } else {
                        if (expressionStack.Count < 2) {
                            throw new ExpressionParseException($"No more element to use for binary operation '{op.op.String()}' in expression '{originalExpression}'");
                        }
                        Expression rightChild = expressionStack.Pop();
                        Expression leftChild = expressionStack.Pop();
                        expressionStack.Push(new Operation(leftChild, op.op, rightChild));
                    }
                } else {
                    expressionStack.Push(e);
                }
            }
            if (expressionStack.Count != 1) {
                throw new ExpressionParseException($"Operator count and operand count do not match in expression '{originalExpression}'");
            }
            return expressionStack.Pop();
        }

        private static Expression GetToken(string str, ref int index) {
            int startIndex = index;
            if (IsDigit(str[index])) {
                index++;
                while (index < str.Length && IsDigit(str[index])) {
                    index++;
                }
                if (index < str.Length && IsLetter(str[index])) {
                    throw new ExpressionParseException($"Find unexpected letter '{str[index]}' after a number in expression '{str}'");
                }
                return new Value(float.Parse(str.Substring(startIndex, index - startIndex)));
            } else if (IsOperatorSpecialCharacter(str[index])) {
                char op = str[index++];
                if (index < str.Length && IsBinaryOperatorSpecialCharacter(str[index])) {
                    throw new ExpressionParseException($"Find unexpected letter '{str[index]}' after an operator in expression '{str}'");
                }

                switch (op) {
                case '+':
                    return new Operation(null, Operator.Plus, null);
                case '-':
                    return new Operation(null, Operator.Minus, null);
                case '*':
                    return new Operation(null, Operator.Times, null);
                case '/':
                    return new Operation(null, Operator.Divide, null);
                case '^':
                    return new Operation(null, Operator.Power, null);
                default:
                    throw new Exception($"Unknown operator special character '{op}'!");
                }
            } else if (IsLetter(str[index])) {
                index++;
                while (index < str.Length && (IsLetter(str[index]) || IsDigit(str[index]))) {
                    index++;
                }
                string name = str.Substring(startIndex, index - startIndex);
                switch (name) {
                case "cos":
                    return new Operation(Operator.Cos, null);
                case "sin":
                    return new Operation(Operator.Sin, null);
                case "exp":
                    return new Operation(Operator.Exp, null);
                case "log":
                    return new Operation(Operator.Log, null);
                case "PI":
                    return new Value(Mathf.PI);
                default:
                    return new Variable(str.Substring(startIndex, index - startIndex));
                }
            } else if (str[index] == '(' || str[index] == ')') {
                throw new Exception("Parenthesis should not be handled by GetToken()!");
            } else {
                throw new ExpressionParseException($"Char '{str[index]}' not recognized in expression '{str}'");
            }
        }

        private static String SanitizeExpressionString(string expression) {
            return expression.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
        }

        private static bool IsDigit(char c) {
            return (c >= '0' && c <= '9') || c == '.';
        }

        private static bool IsOperatorSpecialCharacter(char c) {
            return "+-*/^".Contains(c.ToString());
        }

        private static bool IsBinaryOperatorSpecialCharacter(char c) {
            return "*/^".Contains(c.ToString());
        }

        private static bool IsLetter(char c) {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private static int GetOperatorPriority(Operator op) {
            if (op == Operator.Plus || op == Operator.Minus || op == Operator.PlusUnary || op == Operator.MinusUnary) return 1;
            if (op == Operator.Times || op == Operator.Divide) return 2;
            if (op == Operator.Power) return 3;
            if (op == Operator.Cos || op == Operator.Sin || op == Operator.Exp || op == Operator.Log) return 4;
            return -1;
        }

        private static void EnsureMatchingParentheses(String expression) {
            int parentheseLevel = 0;
            foreach (char c in expression) {
                if (c == '(') parentheseLevel++;
                if (c == ')') parentheseLevel--;
                if (parentheseLevel < 0) {
                    throw new ExpressionParseException($"Unexpected extra right parenthese found in expression '{expression}'");
                }
            }
            if (parentheseLevel != 0) {
                throw new ExpressionParseException($"Numbers of left and right parentheses don't match in expression '{expression}'");
            }
        }
    }
}