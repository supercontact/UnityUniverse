using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Modifier {
    void Apply(RenderGeometry geometry);
}
