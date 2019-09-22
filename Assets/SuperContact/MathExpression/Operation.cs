using System.Collections.Generic;
using System;
using UnityEngine;

namespace SuperContact.MathExpression {

    public class Operation : Expression {
        public readonly Operator op;
        public readonly bool isUnary;
        public readonly Expression leftChild, rightChild;

        public readonly bool isSimplified;

        public Operation(Operator op, Expression child, bool isSimplified = false) {
            this.op = op;
            rightChild = child;
            this.isSimplified = isSimplified;

            isUnary = true;
            if (!op.IsUnary()) {
                throw new Exception($"{op} is not a unary operator!");
            }
        }

        public Operation(Expression leftChild, Operator op, Expression rightChild, bool isSimplified = false) {
            this.op = op;
            this.leftChild = leftChild;
            this.rightChild = rightChild;
            this.isSimplified = isSimplified;

            isUnary = false;
            if (op.IsUnary()) {
                throw new Exception($"{op} is not a binary operator!");
            }
        }

        public override float Evaluate(Dictionary<string, float> variableValues = null) {
            switch (op) {
            case Operator.Plus:
                return leftChild.Evaluate(variableValues) + rightChild.Evaluate(variableValues);
            case Operator.PlusUnary:
                return rightChild.Evaluate(variableValues);
            case Operator.Minus:
                return leftChild.Evaluate(variableValues) - rightChild.Evaluate(variableValues);
            case Operator.MinusUnary:
                return -rightChild.Evaluate(variableValues);
            case Operator.Times:
                return leftChild.Evaluate(variableValues) * rightChild.Evaluate(variableValues);
            case Operator.Divide:
                return leftChild.Evaluate(variableValues) / rightChild.Evaluate(variableValues);
            case Operator.Power:
                return Mathf.Pow(leftChild.Evaluate(variableValues), rightChild.Evaluate(variableValues));
            case Operator.Cos:
                return Mathf.Cos(rightChild.Evaluate(variableValues));
            case Operator.Sin:
                return Mathf.Sin(rightChild.Evaluate(variableValues));
            case Operator.Exp:
                return Mathf.Exp(rightChild.Evaluate(variableValues));
            case Operator.Log:
                return Mathf.Log(rightChild.Evaluate(variableValues));
            default:
                throw new Exception($"Unknown operator {op}!");
            }
        }

        public override Expression Substitute(Dictionary<string, float> knownVariableValues) {
            Expression processedLeftChild = leftChild?.Substitute(knownVariableValues);
            Expression processedRightChild = rightChild.Substitute(knownVariableValues);
            if (isUnary) {
                return new Operation(op, processedRightChild);
            } else {
                return new Operation(processedLeftChild, op, processedRightChild);
            }
        }

        public override Expression Substitute(Dictionary<string, Expression> variableExpressions) {
            Expression processedLeftChild = leftChild?.Substitute(variableExpressions);
            Expression processedRightChild = rightChild.Substitute(variableExpressions);
            if (isUnary) {
                return new Operation(op, processedRightChild);
            } else {
                return new Operation(processedLeftChild, op, processedRightChild);
            }
        }

        public override Expression Derivative(string variableName) {
            switch (op) {
            case Operator.Plus:
                return new Operation(leftChild.Derivative(variableName), Operator.Plus, rightChild.Derivative(variableName)).Simplify();
            case Operator.PlusUnary:
                return rightChild.Derivative(variableName);
            case Operator.Minus:
                return new Operation(leftChild.Derivative(variableName), Operator.Minus, rightChild.Derivative(variableName)).Simplify();
            case Operator.MinusUnary:
                return new Operation(Operator.MinusUnary, rightChild.Derivative(variableName)).Simplify();
            case Operator.Times:
                return new Operation(
                    new Operation(leftChild.Derivative(variableName), Operator.Times, rightChild),
                    Operator.Plus,
                    new Operation(leftChild, Operator.Times, rightChild.Derivative(variableName)))
                    .Simplify();
            case Operator.Divide:
                return new Operation(
                    new Operation(
                        new Operation(leftChild.Derivative(variableName), Operator.Times, rightChild),
                        Operator.Minus,
                        new Operation(leftChild, Operator.Times, rightChild.Derivative(variableName))),
                    Operator.Divide,
                    new Operation(rightChild, Operator.Times, rightChild))
                    .Simplify();
            case Operator.Power:
                return new Operation(
                    new Operation(
                        new Operation(rightChild, Operator.Times, leftChild.Derivative(variableName)),
                        Operator.Times,
                        new Operation(
                            leftChild,
                            Operator.Power,
                            new Operation(rightChild, Operator.Minus, new Value(1)))),
                    Operator.Plus,
                    new Operation(
                        new Operation(
                            new Operation(Operator.Log, leftChild),
                            Operator.Times,
                            rightChild.Derivative(variableName)),
                        Operator.Times,
                        new Operation(leftChild, Operator.Power, rightChild)))
                    .Simplify();
            case Operator.Cos:
                return new Operation(
                    new Operation(new Value(0), Operator.Minus, new Operation(Operator.Sin, rightChild)),
                    Operator.Times,
                    rightChild.Derivative(variableName))
                    .Simplify();
            case Operator.Sin:
                return new Operation(
                    new Operation(Operator.Cos, rightChild),
                    Operator.Times,
                    rightChild.Derivative(variableName))
                    .Simplify();
            case Operator.Exp:
                return new Operation(
                    new Operation(Operator.Exp, rightChild),
                    Operator.Times,
                    rightChild.Derivative(variableName))
                    .Simplify();
            case Operator.Log:
                return new Operation(rightChild.Derivative(variableName), Operator.Divide, rightChild).Simplify();
            default:
                throw new Exception("Unknown operator!");
            }
        }

        public override Expression Simplify() {
            if (isSimplified) return this;

            Expression simplifiedLeftChild = leftChild?.Simplify();
            Expression simplifiedRightChild = rightChild.Simplify();
            Operation semiSimplifiedOperation = isUnary ?
                new Operation(op, simplifiedRightChild) :
                new Operation(simplifiedLeftChild, op, simplifiedRightChild);

            return semiSimplifiedOperation.SimplifyAssumingChildrenAreSimplified();
        }

        private Expression SimplifyAssumingChildrenAreSimplified() {
            if (rightChild is InvalidExpression) {
                return rightChild;
            }
            if (leftChild is InvalidExpression) {
                return leftChild;
            }
            if ((isUnary && rightChild is Value) ||
                (!isUnary && leftChild is Value && rightChild is Value)) {
                try {
                    return new Value(Evaluate());
                } catch (ExpressionEvaluationException exception) {
                    return new InvalidExpression(exception);
                }
            }
            if (op == Operator.Plus) {
                if (leftChild is Value && Mathf.Approximately(leftChild.Evaluate(), 0)) {
                    return rightChild;
                }
                if (rightChild is Value && Mathf.Approximately(rightChild.Evaluate(), 0)) {
                    return leftChild;
                }
            }
            if (op == Operator.PlusUnary) {
                return rightChild;
            }
            if (op == Operator.Minus) {
                if (rightChild is Value && Mathf.Approximately(rightChild.Evaluate(), 0)) {
                    return leftChild;
                }
            }
            if (op == Operator.Times) {
                if (leftChild is Value) {
                    float leftValue = leftChild.Evaluate();
                    if (Mathf.Approximately(leftValue, 0)) {
                        return new Value(0);
                    }
                    if (Mathf.Approximately(leftValue, 1)) {
                        return rightChild;
                    }
                }
                if (rightChild is Value) {
                    float rightValue = rightChild.Evaluate();
                    if (Mathf.Approximately(rightValue, 0)) {
                        return new Value(0);
                    }
                    if (Mathf.Approximately(rightValue, 1)) {
                        return leftChild;
                    }
                }
            }
            if (op == Operator.Divide) {
                if (leftChild is Value && Mathf.Approximately(leftChild.Evaluate(), 0)) {
                    return new Value(0);
                }
                if (rightChild is Value) {
                    float rightValue = rightChild.Evaluate();
                    if (Mathf.Approximately(rightValue, 0)) {
                        return new InvalidExpression(new ExpressionEvaluationException("Divide by zero!"));
                    }
                    if (Mathf.Approximately(rightValue, 1)) {
                        return leftChild;
                    }
                }
            }
            if (op == Operator.Power) {
                if (leftChild is Value) {
                    float leftValue = leftChild.Evaluate();
                    if (Mathf.Approximately(leftValue, 0)) {
                        return new Value(0);
                    }
                    if (Mathf.Approximately(leftValue, 1)) {
                        return new Value(1);
                    }
                }
                if (rightChild is Value) {
                    float rightValue = rightChild.Evaluate();
                    if (Mathf.Approximately(rightValue, 0)) {
                        return new Value(1);
                    }
                    if (Mathf.Approximately(rightValue, 1)) {
                        return leftChild;
                    }
                }
            }
            return isUnary ?
                new Operation(op, rightChild, true /* isSimplified */) :
                new Operation(leftChild, op, rightChild, true /* isSimplified */);
        }

        public override ISet<string> GetVariables() {
            if (isUnary) {
                return rightChild.GetVariables();
            } else {
                ISet<string> result = leftChild.GetVariables();
                result.UnionWith(rightChild.GetVariables());
                return result;
            }
        }

        public override string ToString() {
            string opStr = op.String();
            if (isUnary) {
                return $"{opStr}({rightChild})";
            } else {
                return $"({leftChild}){opStr}({rightChild})";
            }
        }
    }
}
