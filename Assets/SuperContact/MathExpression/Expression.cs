using System.Collections.Generic;

namespace SuperContact.MathExpression {

    public abstract class Expression {
        public abstract float Evaluate(Dictionary<string, float> variableValues = null);

        public abstract Expression Substitute(Dictionary<string, float> knownVariableValues);

        public abstract Expression Substitute(Dictionary<string, Expression> variableExpressions);

        public Expression SubstituteRecursive(Dictionary<string, Expression> variableExpressions) {
            Expression result = this;
            ISet<string> variables;
            do {
                result = result.Substitute(variableExpressions);
                variables = result.GetVariables();
                variables.IntersectWith(variableExpressions.Keys);
            } while (variables.Count > 0);
            return result;
        }

        public abstract Expression Derivative(string variableName);

        public abstract Expression Simplify();

        public abstract ISet<string> GetVariables();
    }
}