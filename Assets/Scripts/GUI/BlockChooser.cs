using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockChooser : MonoBehaviour
{
    const int count = 7;
    const int size = 200;
    const int sizeBetween = size + (size / 2);
    const int sizeHalf = sizeBetween / 2;

    int SelectedBlock = 0;
    int MoveY = 0;
    RawImage[] images;
    new RectTransform transform;
    Coroutine animationCoroutine;

    void Start()
    {
        transform = GetComponent<RectTransform>();

        images = new RawImage[count];

        GameObject go;
        RectTransform rt;
        RawImage image;
        for (int i = 0; i < count; i++)
        {
            go = new GameObject("BlockChooser" + i, typeof(RectTransform), typeof(RawImage));
            image = go.GetComponent<RawImage>();

            rt = image.rectTransform;
            rt.SetParent(transform);
            rt.localScale = Vector3.one;
            rt.pivot = new Vector2(0, .5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(0f, sizeBetween * (i - count / 2));

            images[i] = image;

            image.color = new Color(1f, 1f, 1f, 1f - Mathf.Abs(rt.anchoredPosition.y) / sizeBetween / count * 2f);
        }

        SelectedBlock = 0;

        UpdateImages();
    }

    public void ChangeSelected(int selectedBlock)
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);

        SelectedBlock = selectedBlock;
        animationCoroutine = StartCoroutine(AnimationCoroutine());
    }
    IEnumerator AnimationCoroutine()
    {
        const float speed = 2000f;

        Vector2 add = new Vector2();

        while (transform.anchoredPosition.x > -200f)
        {
            add.Set(-speed * Time.deltaTime, 0f);
            transform.anchoredPosition += add;

            yield return null;
        }

        UpdateImages();

        while (transform.anchoredPosition.x < 100f)
        {
            add.Set(speed * Time.deltaTime, 0f);
            transform.anchoredPosition += add;

            yield return null;
        }
    }

    void UpdateImages()
    {
        int block;
        for (int i = 0; i < count; i++)
        {
            block = SelectedBlock + i - count / 2;

            while (block < 0) block += Block.Blocks.Count;
            while (block > Block.Blocks.Count - 1) block -= Block.Blocks.Count;

            images[i].texture = Game.BlockRenders[Block.Blocks[block].Name];
        }
    }
    void UpdateImage(RawImage image, int block)
    {
        block -= count / 2;

        while (block < 0) block += Block.Blocks.Count;
        while (block > Block.Blocks.Count - 1) block -= Block.Blocks.Count;

        image.texture = Game.BlockRenders[Block.Blocks[block].Name];
    }
}