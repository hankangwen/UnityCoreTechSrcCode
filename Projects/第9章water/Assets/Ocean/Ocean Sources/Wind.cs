using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Ocean))]
public class Wind : MonoBehaviour {
    public float humidity;
    public float waveScale = 4;
    private Ocean ocean;
    public bool forceStorm = false;

    public float prevValue = 0.1f;
    public float nextValue = 0.4f;
    private float prevTime = 1;
    private const float timeFreq = 1f / 280f;

    IEnumerator Start()
    {
        ocean = gameObject.GetComponent<Ocean>();
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (forceStorm)
                humidity = 1f;
            else
                humidity = GetHumidity();

            if (ocean != null)
                ocean.SetWaves(Mathf.Lerp(0, waveScale, humidity));
        }
    }

    void ForceStorm(bool force)
    {
        forceStorm = force;
    }

    private float GetHumidity()
    {
        float time = Time.time;
        int intTime = (int)(time * timeFreq);
        int intPrevTime = (int)(prevTime * timeFreq);
        if (intTime != intPrevTime)
        {
            prevValue = nextValue;
            nextValue = Random.value;
        }
        prevTime = time;
        float frac = time * timeFreq - intTime;

        return Mathf.SmoothStep(prevValue, nextValue, frac);
    }
}
