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

    //sets everything up
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
        //restarts the tick tock sound to sync it with the loop image
        tickTock.Play();
           
        //plays a time rewind sound and pauses everything else
        SFXManager.instance.PlaySFXClip(rewindSFX, this.transform.position, 1f, this.transform);
        Time.timeScale = 0f;
        loop.SetActive(true);

        //slowly fades in the black screen and the tick tock sound
        while (fadeScreen.GetComponent<Image>().color.a < 1)
        {
            fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(fadeScreen.GetComponent<Image>().color.a, 1, 0.1f));

            //prevents this from getting stuck in an infinite while
            if (fadeScreen.GetComponent<Image>().color.a > 0.9)
                fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, 1);

            tickTock.volume = fadeScreen.GetComponent<Image>().color.a;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(2);
        SceneManager.LoadScene(0); //restarts the scene, like time rewinding
    }

    public IEnumerator FadeOut()
    {       
        //syncs tick tock sound with loop image
        tickTock.Play();

        Time.timeScale = 0f;

        loop.SetActive(true);
        yield return new WaitForSecondsRealtime(3);

        //slowly fades out the black screen and tick tock sound
        while (fadeScreen.GetComponent<Image>().color.a > 0)
        {
            fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(0, fadeScreen.GetComponent<Image>().color.a, 0.9f));

            //also stops this from getting stuck in an infinite while
            if (fadeScreen.GetComponent<Image>().color.a < 0.1)
                fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            tickTock.volume = fadeScreen.GetComponent<Image>().color.a;
            yield return new WaitForSecondsRealtime(0.05f);
        }
        loop.SetActive(false);

        Time.timeScale = 1f;
    }
}
