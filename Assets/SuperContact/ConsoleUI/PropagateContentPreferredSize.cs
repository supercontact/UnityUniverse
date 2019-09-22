using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropagateContentPreferredSize : MonoBehaviour, ILayoutElement, ILayoutGroup {

    public RectTransform content;
    public float additionalWidth = 0;
    public float additionalHeight = 0;

    public float minWidth => LayoutUtility.GetMinWidth(content) + additionalWidth;

    public float preferredWidth => LayoutUtility.GetPreferredWidth(content) + additionalWidth;

    public float flexibleWidth => LayoutUtility.GetFlexibleWidth(content);

    public float minHeight => LayoutUtility.GetMinHeight(content) + additionalHeight;

    public float preferredHeight => LayoutUtility.GetPreferredHeight(content) + additionalHeight;

    public float flexibleHeight => LayoutUtility.GetFlexibleHeight(content);

    public int layoutPriority => 1;

    public void CalculateLayoutInputHorizontal() { }

    public void CalculateLayoutInputVertical() { }

    public void SetLayoutHorizontal() {
        UpdateLayout();
    }

    public void SetLayoutVertical() {
        UpdateLayout();
    }

    public void UpdateLayout() {
        LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
    }
}
