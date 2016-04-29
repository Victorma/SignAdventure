using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClickBook : MonoBehaviour
{

    public Animator thisbook, otherbook;
    public LevelFiller levelPage;
    public Level startLevel;

    public MenuReferences menu;

    void OnMouseUpAsButton()
    {
        thisbook.SetBool("Table", true);
        otherbook.SetBool("Show", false);

        levelPage.firstLevel = startLevel;

        PlayerPrefs.SetInt(startLevel.levelName, 1);
        PlayerPrefs.Save();

        menu.setMenu(2);

    }
}