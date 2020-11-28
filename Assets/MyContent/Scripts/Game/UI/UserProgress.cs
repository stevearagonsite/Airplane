using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Utils;
public class UserProgress : MonoBehaviour
{
    private static UserProgress Instance;
    private float _maxValueProgress;
    private float _currentValueProgress;
    [Header("UI Objects")]
    public Image progress;
    public TMP_Text text;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            Instance = this;
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _maxValueProgress = Math.Abs(progress.rectTransform.offsetMax.x);
        progress.fillAmount = 0;
    }

    /// <summary>
    /// Set text to show in loading view.
    /// </summary>
    public void SetText(string value)
    {
        if (value == null) return;
        text.SetText(value);
    }
    
    /// <summary>
    /// Set value between 0 to 1, for the progress bar.
    /// </summary>
    public void SetProgress(float value)
    {
        if (value < 0) return;
        _currentValueProgress = value;
        progress.fillAmount = _currentValueProgress;
        progress.rectTransform.SetRight(_maxValueProgress * _currentValueProgress);
    }
}
