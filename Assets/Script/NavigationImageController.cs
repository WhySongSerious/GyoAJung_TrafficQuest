using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NavigationImageController : MonoBehaviour
{
    [SerializeField] Image image;

    public bool isPopImage;

    void Start()
    {
        Color color = image.color;
        color.a = 20;
        image.enabled = false;
        isPopImage = false;
    }
    IEnumerator PopImage()
    {
        image.enabled = true;
        BlinkImage();
        yield return new WaitForSeconds(10f);
        isPopImage = false;
        image.enabled = false;
    }

    void BlinkImage()
    {
        for(int i = 0; i < 10; i++)
        {
            Color color = image.color;
            color.a = 200;
            new WaitForSeconds(0.5f);
            color.a = 100;
            new WaitForSeconds(0.5f);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("NavigateRight") && isPopImage == false)
        {
            isPopImage = true;
            StartCoroutine(PopImage());
        }
    }

}
