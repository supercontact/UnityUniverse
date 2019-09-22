using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SuperContact.MathExpression;

public class SpaceWarp {
    public readonly Expression posX, posY, posZ;

    private Expression posXdx, posXdy, posXdz, posYdx, posYdy, posYdz, posZdx, posZdy, posZdz;
    private Dictionary<string, float> mapping = new Dictionary<string, float>();

    public SpaceWarp(Expression posX, Expression posY, Expression posZ, Dictionary<string, Expression> additionalExpressions = null) {
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        if (additionalExpressions != null) {
            this.posX = this.posX.SubstituteRecursive(additionalExpressions);
            this.posY = this.posY.SubstituteRecursive(additionalExpressions);
            this.posZ = this.posZ.SubstituteRecursive(additionalExpressions);
        }
    }

    public SpaceWarp(string posX, string posY, string posZ, Dictionary<string, string> additionalExpressions = null) {
        this.posX = ExpressionParser.Parse(posX);
        this.posY = ExpressionParser.Parse(posY);
        this.posZ = ExpressionParser.Parse(posZ);
        if (additionalExpressions != null) {
            Dictionary<string, Expression> additionalExpressionsParsed =
                additionalExpressions.ToDictionary((entry) => entry.Key, (entry) => ExpressionParser.Parse(entry.Value));
            this.posX = this.posX.SubstituteRecursive(additionalExpressionsParsed);
            this.posY = this.posY.SubstituteRecursive(additionalExpressionsParsed);
            this.posZ = this.posZ.SubstituteRecursive(additionalExpressionsParsed);
        }
    }

    public SpaceWarp(params string[] assignExpressions) {
        Dictionary<string, Expression> parsedAssignExpressions = ExpressionParser.ParseAssignExpressions(assignExpressions);
        posX = parsedAssignExpressions["X"];
        posY = parsedAssignExpressions["Y"];
        posZ = parsedAssignExpressions["Z"];
        if (parsedAssignExpressions.Count > 3) {
            parsedAssignExpressions.Remove("X");
            parsedAssignExpressions.Remove("Y");
            parsedAssignExpressions.Remove("Z");
            posX = posX.SubstituteRecursive(parsedAssignExpressions);
            posY = posY.SubstituteRecursive(parsedAssignExpressions);
            posZ = posZ.SubstituteRecursive(parsedAssignExpressions);
        }
        foreach (string v in posX.GetVariables().Union(posY.GetVariables()).Union(posZ.GetVariables())) {
            if (v != "x" && v != "y" && v != "z") {
                throw new ExpressionParseException($"Unknown variable '{v}' found in expressions");
            }
        }
    }

    public Vector3 Evaluate(Vector3 pos) {
        mapping["x"] = pos.x;
        mapping["y"] = pos.y;
        mapping["z"] = pos.z;
        return new Vector3(posX.Evaluate(mapping), posY.Evaluate(mapping), posZ.Evaluate(mapping));
    }

    public Vector3 EvaluateTangent(Vector3 pos, Vector3 tangent) {
        mapping["x"] = pos.x;
        mapping["y"] = pos.y;
        mapping["z"] = pos.z;
        return BuildDerivativeMatrix().MultiplyVector(tangent);
    }

    public Vector3 EvaluateNormal(Vector3 pos, Vector3 normal) {
        mapping["x"] = pos.x;
        mapping["y"] = pos.y;
        mapping["z"] = pos.z;
        return BuildDerivativeMatrix().CofactorMatrix().MultiplyVector(normal).normalized;
    }

    private void GenerateDerivatives() {
        posXdx = posX.Derivative("x");
        posXdy = posX.Derivative("y");
        posXdz = posX.Derivative("z");
        posYdx = posY.Derivative("x");
        posYdy = posY.Derivative("y");
        posYdz = posY.Derivative("z");
        posZdx = posZ.Derivative("x");
        posZdy = posZ.Derivative("y");
        posZdz = posZ.Derivative("z");
    }

    private Matrix4x4 BuildDerivativeMatrix() {
        Matrix4x4 M = new Matrix4x4();
        if (posXdx == null) GenerateDerivatives();
        M.m00 = posXdx.Evaluate(mapping);
        M.m01 = posXdy.Evaluate(mapping);
        M.m02 = posXdz.Evaluate(mapping);
        M.m10 = posYdx.Evaluate(mapping);
        M.m11 = posYdy.Evaluate(mapping);
        M.m12 = posYdz.Evaluate(mapping);
        M.m20 = posZdx.Evaluate(mapping);
        M.m21 = posZdy.Evaluate(mapping);
        M.m22 = posZdz.Evaluate(mapping);
        M.m33 = 1;
        return M;
    }
}
