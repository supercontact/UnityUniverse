using System.Collections.Generic;

namespace SuperContact.MathExpression {

    public class Value : Expression {
        public readonly float value;

        public Value(float value) {
            this.value = value;
        }

        public override float Evaluate(Dictionary<string, float> variableValues = null) {
            return value;
        }

        public override Expression Substitute(Dictionary<string, float> knownVariableValues) {
            return this;
        }

        public override Expression Substitute(Dictionary<string, Expression> variableExpressions) {
            return this;
        }

        public override Expression Derivative(string variableName) {
            return new Value(0);
        }

        public override Expression Simplify() {
            return this;
        }

        public override ISet<string> GetVariables() {
            return new HashSet<string>();
        }

        public override string ToString() {
            return value.ToString();
        }
    }
}