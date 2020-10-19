using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutUI : MonoBehaviour
{
    public float step = 0.01f;

    private Image image;
    private float alpha = 1;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        alpha -= step;
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        if (alpha <= 0) Destroy(transform.parent.gameObject);
    }
}
