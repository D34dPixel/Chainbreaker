using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoopManager : MonoBehaviour
{   
    public static LoopManager instance;

    public GameObject FadeScreen;
    public GameObject Loop;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        Time.timeScale = 0f;
        FadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, 1);
        Loop.SetActive(true);
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeIn()
    {
        Time.timeScale = 0f;
        Loop.SetActive(true);
        while (FadeScreen.GetComponent<Image>().color.a < 1)
        {
            FadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(FadeScreen.GetComponent<Image>().color.a, 1, 0.1f));
            if (FadeScreen.GetComponent<Image>().color.a > 0.9)
                FadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, 1);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(2);
        SceneManager.LoadScene(0);
    }

    public IEnumerator FadeOut()
    {
        Time.timeScale = 0f;

        Loop.SetActive(true);
        yield return new WaitForSecondsRealtime(3);
        while (FadeScreen.GetComponent<Image>().color.a > 0)
        {
            FadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(0, FadeScreen.GetComponent<Image>().color.a, 0.9f));
            if (FadeScreen.GetComponent<Image>().color.a < 0.1)
                FadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            yield return new WaitForSecondsRealtime(0.05f);
        }
        Loop.SetActive(false);

        Time.timeScale = 1f;
    }
}
