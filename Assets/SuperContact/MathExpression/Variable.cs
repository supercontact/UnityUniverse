using System.Collections.Generic;

namespace SuperContact.MathExpression {

    public class Variable : Expression {
        public readonly string name;

        public Variable(string name) {
            this.name = name;
        }

        public override float Evaluate(Dictionary<string, float> variableValues = null) {
            if (variableValues == null || !variableValues.TryGetValue(name, out float value)) {
                throw new ExpressionEvaluationException($"The value of variable {name} is not set!");
            }
            return value;
        }

        public override Expression Substitute(Dictionary<string, float> knownVariableValues) {
            if (knownVariableValues.TryGetValue(name, out float value)) {
                return new Value(value);
            }
            return this;
        }

        public override Expression Substitute(Dictionary<string, Expression> variableExpressions) {
            if (variableExpressions.TryGetValue(name, out Expression expr)) {
                return expr;
            }
            return this;
        }

        public override Expression Derivative(string variableName) {
            return variableName == name ? new Value(1) : new Value(0);
        }

        public override Expression Simplify() {
            return this;
        }

        public override ISet<string> GetVariables() {
            return new HashSet<string> { name };
        }

        public override string ToString() {
            return name;
        }
    }
}