using System;

namespace SuperContact.MathExpression {

    public enum Operator {
        Plus = 1,
        PlusUnary = 101,
        Minus = 2,
        MinusUnary = 102,
        Times = 3,
        Divide = 4,
        Power = 5,
        Cos = 103,
        Sin = 104,
        Exp = 105,
        Log = 106,
    }

    public static class OperatorMethods {
        public static string String(this Operator op) {
            switch (op) {
            case Operator.Plus:
            case Operator.PlusUnary:
                return "+";
            case Operator.Minus:
            case Operator.MinusUnary:
                return "-";
            case Operator.Times:
                return "*";
            case Operator.Divide:
                return "/";
            case Operator.Power:
                return "^";
            case Operator.Cos:
                return "cos";
            case Operator.Sin:
                return "sin";
            case Operator.Exp:
                return "exp";
            case Operator.Log:
                return "log";
            default:
                throw new Exception($"Unknown operator enum {op}!");
            }
        }

        public static bool IsUnary(this Operator op) {
            return (int)op >= 100;
        }
    }
}