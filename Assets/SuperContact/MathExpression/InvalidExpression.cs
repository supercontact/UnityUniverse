using System.Collections.Generic;

namespace SuperContact.MathExpression {

    public class InvalidExpression : Expression {
        public ExpressionEvaluationException exception;

        public InvalidExpression(ExpressionEvaluationException exception) {
            this.exception = exception;
        }

        public override Expression Derivative(string variableName) {
            return new InvalidExpression(new ExpressionEvaluationException("Trying to calculate derivative of an invalid expression!"));
        }

        public override float Evaluate(Dictionary<string, float> variableValues = null) {
            throw exception;
        }

        public override Expression Simplify() {
            return this;
        }

        public override Expression Substitute(Dictionary<string, float> knownVariableValues) {
            return this;
        }

        public override Expression Substitute(Dictionary<string, Expression> variableExpressions) {
            return this;
        }

        public override ISet<string> GetVariables() {
            return new HashSet<string>();
        }

        public override string ToString() {
            return "[InvalidExpression]";
        }
    }
}