using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OverlayMenu : MonoBehaviour
{
    Player player;

    UIDocument overlayDocument;
    VisualElement overlayVE;

    Label crystalLabel;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();

        overlayDocument = GetComponent<UIDocument>();

        FindElements();
    }

    void Start()
    {
        overlayVE.style.display = DisplayStyle.None;
    }

    void Update()
    {
        UpdateText(player.tempData.Crystals.ToString());
    }

    void FindElements()
    {
        overlayVE = overlayDocument.rootVisualElement;
        
        crystalLabel = overlayVE.Q<Label>("Crystal");
    }

    void UpdateText(string newText)
    {
        crystalLabel.text = newText;
    }
}
