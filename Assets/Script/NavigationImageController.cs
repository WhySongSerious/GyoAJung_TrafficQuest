using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NavigationImageController : MonoBehaviour
{
    [SerializeField] Image R_image;
    [SerializeField] Image L_image;
    Image currentImage;

    void Start()
    {
        R_image.enabled = false;
        L_image.enabled = false;
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
            R_image.enabled = false;
            L_image.enabled = false;
            currentImage = R_image;
            StopAllCoroutines();
            StartCoroutine(PopImage());
        }

        if (collider.CompareTag("NavigateLeft"))
        {
            L_image.enabled = false;
            R_image.enabled = false;
            currentImage = L_image;
            StopAllCoroutines();
            StartCoroutine(PopImage());
        }
    }

}
