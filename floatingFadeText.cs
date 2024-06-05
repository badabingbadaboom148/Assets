using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class floatingFadeText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SlideAndFade());
    }
    private void Update()
    {
        float translation = 10 * Time.deltaTime;

        transform.Translate(Vector3.up * translation);
    }
    public IEnumerator SlideAndFade()
    {
        Text textComponent = GetComponent<Text>(); // Assuming the script is attached to the same GameObject as the Text component
        float startAlpha = textComponent.color.a;

        float rate = 1.0f / 0.5f; // Adjust rate based on the desired duration of the effect
        float progress = 0.0f;

        while (progress < 0.5f)
        {
            transform.Translate(Vector2.up * (100 * Time.deltaTime));
            Color tmpColor = textComponent.color;
            textComponent.color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));

            progress += rate * Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject); // Destroying the GameObject this script is attached to
    }
}
