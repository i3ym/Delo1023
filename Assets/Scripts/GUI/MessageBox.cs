using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    public static bool Active = false;
    static TextMeshProUGUI stext;
    static Button sbuttonYes, sbuttonNo;
    static new RectTransform transform;
    static UnityAction resetAction;

    [SerializeField]
    TextMeshProUGUI text = null;
    [SerializeField]
    Button buttonYes = null, buttonNo = null;

    void Awake()
    {
        if (stext != null)
        {
            Destroy(this);
            return;
        }

        transform = GetComponent<RectTransform>();
        stext = text;
        sbuttonYes = buttonYes;
        sbuttonNo = buttonNo;

        resetAction = new UnityAction(() =>
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 1000f);
            sbuttonNo.onClick.RemoveAllListeners();
            sbuttonYes.onClick.RemoveAllListeners();
            Active = false;
        });

        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 1000f);
    }

    public static void Show(string text, Action actionYes, Action actionNo)
    {
        if (Active) return;

        Active = true;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
        stext.text = text;

        sbuttonYes.onClick.AddListener(new UnityAction(actionYes));
        sbuttonYes.onClick.AddListener(resetAction);
        sbuttonNo.onClick.AddListener(new UnityAction(actionNo));
        sbuttonNo.onClick.AddListener(resetAction);
    }
}