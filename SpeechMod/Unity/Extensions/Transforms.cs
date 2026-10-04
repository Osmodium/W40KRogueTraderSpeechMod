using UnityEngine;

namespace SpeechMod.Unity.Extensions;

public static class Transforms
{
    extension(GameObject uiObject)
    {
        public void RectAlignTopLeft(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(0, 1);
            var anchorMax = new Vector2(0, 1);
            var pivot = new Vector2(0, 1);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }

        public void RectAlignTopMiddle(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(0.5f, 1);
            var anchorMax = new Vector2(0.5f, 1);
            var pivot = new Vector2(0.5f, 1);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }

        public void RectAlignTopRight(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(1, 1);
            var anchorMax = new Vector2(1, 1);
            var pivot = new Vector2(1, 1);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }

        public void RectAlignMiddleLeft(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(0, 0.5f);
            var anchorMax = new Vector2(0, 0.5f);
            var pivot = new Vector2(0, 0.5f);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }

        public void RectAlignMiddle(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(0.5f, 0.5f);
            var anchorMax = new Vector2(0.5f, 0.5f);
            var pivot = new Vector2(0.5f, 0.5f);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }

        public void RectAlignMiddleRight(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(1, 0.5f);
            var anchorMax = new Vector2(1, 0.5f);
            var pivot = new Vector2(1, 0.5f);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }

        public void RectAlignBottomLeft(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(0, 0);
            var anchorMax = new Vector2(0, 0);
            var pivot = new Vector2(0, 0);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }

        public void RectAlignBottomMiddle(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(0.5f, 0);
            var anchorMax = new Vector2(0.5f, 0);
            var pivot = new Vector2(0.5f, 0);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }

        public void RectAlignBottomRight(Vector2? anchoredPosition = null)
        {
            var anchorMin = new Vector2(1, 0);
            var anchorMax = new Vector2(1, 0);
            var pivot = new Vector2(1, 0);

            uiObject.SetRectAlign(anchorMin, anchorMax, pivot, anchoredPosition);
        }
        
        private void SetRectAlign(Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2? anchoredPosition = null)
        {
            var uiTransform = uiObject.GetComponent<RectTransform>();

            if (uiTransform == null)
                return;

            uiTransform.anchorMin = anchorMin;
            uiTransform.anchorMax = anchorMax;
            uiTransform.pivot = pivot;

            if (anchoredPosition.HasValue)
                uiTransform.anchoredPosition = anchoredPosition.Value;
        }
    }

    extension(RectTransform trans)
    {
        public void SetDefaultScale()
        {
            trans.localScale = new Vector3(1, 1, 1);
        }

        public void SetPivotAndAnchors(Vector2 aVec)
        {
            trans.pivot = aVec;
            trans.anchorMin = aVec;
            trans.anchorMax = aVec;
        }

        public Vector2 GetSize()
        {
            return trans.rect.size;
        }

        public float GetWidth()
        {
            return trans.rect.width;
        }

        public float GetHeight()
        {
            return trans.rect.height;
        }

        public void SetPositionOfPivot(Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
        }

        public void SetLeftBottomPosition(Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }

        public void SetLeftTopPosition(Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }

        public void SetRightBottomPosition(Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }

        public void SetRightTopPosition(Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }

        public void SetSize(Vector2 newSize)
        {
            var oldSize = trans.rect.size;
            var deltaSize = newSize - oldSize;
            trans.offsetMin -= new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
            trans.offsetMax += new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
        }

        public void SetWidth(float newSize)
        {
            trans.SetSize(new Vector2(newSize, trans.rect.size.y));
        }

        public void SetHeight(float newSize)
        {
            trans.SetSize(new Vector2(trans.rect.size.x, newSize));
        }
    }
}