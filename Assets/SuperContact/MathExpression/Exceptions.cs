using System;

namespace SuperContact.MathExpression {

    public class ExpressionParseException : Exception {
        public ExpressionParseException(String errorMessage) : base(errorMessage) { }
    }

    public class ExpressionEvaluationException : Exception {
        public ExpressionEvaluationException(String errorMessage) : base(errorMessage) { }
    }
}