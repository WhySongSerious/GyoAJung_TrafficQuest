using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NavigationImageController : MonoBehaviour
{
    [SerializeField] Image R_image;
    [SerializeField] Image L_image;
    [SerializeField] Image F_image;
    Image currentImage;

    void Start()
    {
        ResetImage();
    }

    private void ResetImage()
    {
        R_image.enabled = false;
        L_image.enabled = false;
        F_image.enabled = false;
    }
    IEnumerator PopImage()
    {
        currentImage.enabled = true;
        yield return StartCoroutine(BlinkImage());
        currentImage.enabled = false;
    }

    IEnumerator BlinkImage()
    {
        Color color = currentImage.color;
        for (int i = 0; i < 6; i++)
        {
            Debug.Log("³²Àº È½¼ö: " + (5 - i));
            color.a = 0.3f;
            currentImage.color = color;
            yield return new WaitForSeconds(1f);
            color.a = 0.7f;
            currentImage.color = color;
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("NavigateRight"))
        {
            ResetImage();
            currentImage = R_image;
            StopAllCoroutines();
            StartCoroutine(PopImage());
        }

        if (collider.CompareTag("NavigateLeft"))
        {
            ResetImage();
            currentImage = L_image;
            StopAllCoroutines();
            StartCoroutine(PopImage());
        }

        if (collider.CompareTag("NavigateFront"))
        {
            ResetImage();
            currentImage = F_image;
            StopAllCoroutines();
            StartCoroutine(PopImage());
        }
    }

}
