using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelFader : MonoBehaviour
{
    private bool mFaded = true;

    public float Duration = 0.4f;
    CanvasGroup canvasGroup;
    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Ensure the panel starts fully transparent
    }
    public void Fade()
    {

        canvasGroup = GetComponent<CanvasGroup>();

        StartCoroutine(DoFade(canvasGroup, canvasGroup.alpha, mFaded ? 1 : 0));

        mFaded = !mFaded;
    }

    public IEnumerator DoFade(CanvasGroup canvasGroup, float start, float end)
    {
        float coutner = 0f;

        while (coutner < Duration)
        {
            coutner += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, coutner / Duration);
            yield return null;
        }
    }
}

