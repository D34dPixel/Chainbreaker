using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoopManager : MonoBehaviour
{   
    public static LoopManager instance;

    public GameObject fadeScreen;
    public GameObject loop;
    public AudioSource tickTock;
    public AudioClip rewindSFX;
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
        fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, 1);
        tickTock = GetComponent<AudioSource>();
        loop.SetActive(true);
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeIn()
    {
        tickTock.Play();

        SFXManager.instance.PlaySFXClip(rewindSFX, this.transform.position, 1f, this.transform);
        Time.timeScale = 0f;
        loop.SetActive(true);
        while (fadeScreen.GetComponent<Image>().color.a < 1)
        {
            fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(fadeScreen.GetComponent<Image>().color.a, 1, 0.1f));
            if (fadeScreen.GetComponent<Image>().color.a > 0.9)
                fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, 1);
            tickTock.volume = fadeScreen.GetComponent<Image>().color.a;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(2);
        SceneManager.LoadScene(0);
    }

    public IEnumerator FadeOut()
    {
        tickTock.Play();

        Time.timeScale = 0f;

        loop.SetActive(true);
        yield return new WaitForSecondsRealtime(3);
        while (fadeScreen.GetComponent<Image>().color.a > 0)
        {
            fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(0, fadeScreen.GetComponent<Image>().color.a, 0.9f));
            if (fadeScreen.GetComponent<Image>().color.a < 0.1)
                fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            tickTock.volume = fadeScreen.GetComponent<Image>().color.a;
            yield return new WaitForSecondsRealtime(0.05f);
        }
        loop.SetActive(false);

        Time.timeScale = 1f;
    }
}
