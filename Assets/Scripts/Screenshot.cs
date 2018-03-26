using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Screenshot : MonoBehaviour {

    public Text textScreenshot;
    public const float shotRate = 3.0f;         // One shot every xx second(s).
    public int countBegin = 10000;
    public int count = 0;
    public bool shouldStop = false;
    public bool storeInChore = false;
    public const string pathChore = "D:/Ziqian Chen's Project/Choregraphe/testImage_unity/html";

    IEnumerator AutoScreenshot()
    {
        string filePath;
        string fileNamePrefix;

        if (storeInChore)   // Store in Choregraphe directory.
        {
            filePath = pathChore;
            fileNamePrefix = "Chore_";
        }
        else   // Store locally in the unity project.
        {
            string timeBegin = System.DateTime.Now.ToString("yyyyMMdd_hhmmss");
            string newPath = "_screenshots/begins_at_" + timeBegin;

            try
            {
                if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath);
            }
            catch (IOException ex)
            {
                print(ex.Message);
            }

            filePath = newPath;
            fileNamePrefix = "Unity_";
        }

        for (;;)
        {
            Application.CaptureScreenshot(filePath + "/" + fileNamePrefix + (countBegin + count++) + ".png", 1);
            yield return new WaitForSeconds(shotRate);
            if (shouldStop)     break;
        }
    }


    // Use this for initialization
    void Start () {
        Application.CaptureScreenshot("screenshotStart.png", 1);
        
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetKeyDown("space"))
        {
            string timeNow = System.DateTime.Now.ToString("yyyyMMdd_hhmmss");
            print(System.DateTime.Now);
            Application.CaptureScreenshot("_screenshots/screenshot_space_" + timeNow + ".png", 1);
        }

        if (Input.GetKeyDown("return"))
        {
            storeInChore = false;
            shouldStop = false;
            textScreenshot.text = "Auto-screenshot: one every " + shotRate + " second(s)";
            StartCoroutine("AutoScreenshot");
        }

        if (Input.GetKeyDown("escape"))
        {
            shouldStop = true;
            textScreenshot.text = "Auto-screenshot stopped.";
            countBegin += 10000;
            count = 0;
        }

        if (Input.GetKeyDown("p"))
        {
            shouldStop = false;
            storeInChore = true;
            textScreenshot.text = "Auto-screenshot in Choregraphe:\none every " + shotRate + " second(s)";
            StartCoroutine("AutoScreenshot");
        }

    }

    // D:\Ziqian Chen's Project\Choregraphe\testImage_unity\html

}
