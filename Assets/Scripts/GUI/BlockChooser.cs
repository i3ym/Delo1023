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
            go = new GameObject("BlockChooser", typeof(RectTransform), typeof(RawImage));
            image = go.GetComponent<RawImage>();

            rt = image.rectTransform;
            rt.SetParent(transform);
            rt.localScale = Vector3.one;
            rt.pivot = new Vector2(0, .5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(0f, -sizeBetween * i);

            images[i] = image;
            UpdateImage(images[i], i);

            UpdateTransparency(image);
        }
    }
    public void Update()
    {
        for (int i = 0; i < count; i++)
        {
            images[i].rectTransform.anchoredPosition -= new Vector2(0f, 10f);
        }
        UpdatePosition();

        for (int i = 0; i < count; i++)
        {
            UpdateTransparency(images[i]);
            UpdateImage(images[i], SelectedBlock + i);
        }
    }

    public void ChangeSelected(int selectedBlock)
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimationCoroutine(selectedBlock));
    }
    IEnumerator AnimationCoroutine(int selectedBlock)
    {
        const int speed = 10;

        Vector2 addpos = new Vector2(0f, speed * (SelectedBlock - selectedBlock));
        int added = Mathf.Abs(sizeBetween * (SelectedBlock - selectedBlock));

        while (added > 0)
        {
            added -= speed;

            foreach (RawImage image in images) image.rectTransform.anchoredPosition += addpos;

            UpdatePosition();

            for (int i = 0; i < count; i++)
            {
                UpdateTransparency(images[i]);
                UpdateImage(images[i], selectedBlock + i);
            }

            yield return null;
        }
    }
    void UpdatePosition()
    {
        while (images[0].rectTransform.anchoredPosition.y > sizeHalf * count)
        {
            foreach (RawImage image in images)
                image.rectTransform.anchoredPosition -= new Vector2(0f, sizeBetween);

            SelectedBlock++;
            if (SelectedBlock > Block.Blocks.Count) SelectedBlock = Block.Blocks.Count - 1;
        }
        while (images[count - 1].rectTransform.anchoredPosition.y < -sizeHalf * count)
        {
            foreach (RawImage image in images)
                image.rectTransform.anchoredPosition += new Vector2(0f, sizeBetween);

            SelectedBlock--;
            if (SelectedBlock < 0) SelectedBlock = Block.Blocks.Count - 1;
        }
    }
    void UpdateTransparency(RawImage image)
    {
        image.color = new Color(1f, 1f, 1f, 1f - Mathf.Abs(image.rectTransform.anchoredPosition.y) / 300f / count * 2f);
    }
    void UpdateImage(RawImage image, int block)
    {
        while (block < 0) block += Block.Blocks.Count;
        while (block > Block.Blocks.Count - 1) block -= Block.Blocks.Count;

        image.texture = Game.BlockRenders[Block.Blocks[block].Name];
    }
}