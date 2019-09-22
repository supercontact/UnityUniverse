using UnityEngine;
using SuperContact.MathExpression;

public class CustomTransform : Modifier {
    public string[] assignExpressions = new string[] { "X=x", "Y=y", "Z=z" };

    public CustomTransform() { }
    public CustomTransform(string expressionX, string expressionY, string expressionZ) {
        SetXYZExpressions(expressionX, expressionY, expressionZ);
    }
    public CustomTransform(params string[] assignExpressions) {
        this.assignExpressions = assignExpressions;
    }

    public void SetXYZExpressions(string expressionX, string expressionY, string expressionZ) {
        assignExpressions = new string[] { $"X={expressionX}", $"Y={expressionY}", $"Z={expressionZ}" };
    }

    public void Apply(RenderGeometry geometry) {
        geometry.ApplySpaceWarp(new SpaceWarp(assignExpressions));
    }
}
