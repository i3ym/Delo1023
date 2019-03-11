using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    const int countX = 6;
    const int countY = 6;
    SVGImage[, ] images = new SVGImage[countX, countY];
    Button[] tabButtons;
    SVGImage[] tabImages;

    [SerializeField]
    Sprite tabOpen = null, tabClosed = null;

    void Start()
    {
        tabButtons = GetComponentsInChildren<Button>();
        tabImages = GetComponentsInChildren<SVGImage>();

        for (int i = 0; i < tabButtons.Length; i++)
            tabButtons[i].onClick.AddListener(new UnityAction(() => SetActiveTab(tabImages[i])));

        GameObject go;
        SVGImage img;
        for (int x = 0; x < countX; x++)
            for (int y = 0; y < countY; y++)
            {
                go = new GameObject("image_" + x + "," + y, typeof(RectTransform), typeof(RawImage));
                img = go.GetComponent<SVGImage>();

                images[x, y] = img;
            }
    }

    void SetActiveTab(SVGImage image)
    {
        foreach (SVGImage ri in images) ri.sprite = tabClosed;
        image.sprite = tabOpen;
    }
}