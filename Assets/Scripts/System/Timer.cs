using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : Module
{
    public float speed {get; private set;} = 1f;
    public IText secondText;

    public float currentTime {get; private set;} = 0;
    public int startTime {get; private set;} = 0;
    public bool isPaused {get; private set;} = true;
    public bool isDone {get; private set;} = false;

    public event Action onStartEvent;
    public event Action<float> onDoneEvent;


    // Update is called once per frame
    void Update()
    {
        if (!isPaused && !isDone) {
            currentTime = Mathf.Max(currentTime - speed * Time.deltaTime, 0);
            secondText?.SetText(GetTime().ToString());

            if (currentTime == 0) {
                Done();
            }
        }
    }

    public void SetSpeed(float spd) {
        speed = spd;
    }

    public void SetTimer(int sec, float spd = 1, bool start = true) {
        speed = spd;
        currentTime = sec;
        startTime = sec;
        isPaused = true;
        isDone = false;
        if (!start)
            return;
        
        StartTimer();
    }

    public int GetTime() {
        return Mathf.CeilToInt(currentTime);
    }

    public void StartTimer() {
        isPaused = false;
        isDone = false;
        onStartEvent?.Invoke();
    }   

    public void Pause() {
        isPaused = true;
    }

    public void Unpause() {
        isPaused = false;
    }

    public float Done() {
        float leftTime = currentTime;
        currentTime = 0;
        isPaused = true;
        isDone = true;
        onDoneEvent?.Invoke(leftTime);
        return leftTime;
    }

    public void Restart() {
        SetTimer(startTime, speed);
    }
}
