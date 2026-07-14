using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelFader : MonoBehaviour
{
    private bool mFaded = true;
    public bool CycleFade = false;

    public float Duration = 0.4f;
    
    [Range(0f, 1f)]
    public float MaxAlpha = 1f; // <-- Nowy suwak w Inspektorze (domyślnie 1)

    private CanvasGroup canvasGroup;
    private Coroutine currentFadeRoutine; 

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; 
    }

    public void Fade()
    {
        if (canvasGroup == null)
            return;

        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
        }

        if (CycleFade)
        {
            currentFadeRoutine = StartCoroutine(DoCycleFade());
        }
        else
        {
            // Celujemy w MaxAlpha zamiast sztywnej jedynki
            float targetAlpha = mFaded ? MaxAlpha : 0f;
            currentFadeRoutine = StartCoroutine(DoFade(canvasGroup.alpha, targetAlpha));
            mFaded = !mFaded;
        }
    }

    private IEnumerator DoCycleFade()
    {
        // Krok 1: Pojawianie się do wyznaczonego limitu (MaxAlpha)
        yield return StartCoroutine(DoFade(canvasGroup.alpha, MaxAlpha));

        // Krok 2: Znikanie od wyznaczonego limitu do zera
        yield return StartCoroutine(DoFade(MaxAlpha, 0f));

        mFaded = true;
        currentFadeRoutine = null;
    }

    private IEnumerator DoFade(float start, float end)
    {
        float counter = 0f;

        while (counter < Duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / Duration);
            yield return null;
        }

        canvasGroup.alpha = end;
        
        if (!CycleFade)
        {
            currentFadeRoutine = null;
        }
    }
}