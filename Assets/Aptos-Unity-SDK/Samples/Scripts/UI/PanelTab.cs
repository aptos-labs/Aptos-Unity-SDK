using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelTab : MonoBehaviour
{
    public PanelGroup panelGroup;
    public string tabName;
    public GameObject targetPanel;
    public bool isSelected;

    private Color selectedColor;
    private Color unselectedColor;

    private Button m_button;

    private void Awake()
    {
        m_button = this.GetComponent<Button>();
        m_button.onClick.AddListener(delegate { UIController.instance.Open(this); });

        selectedColor = m_button.colors.selectedColor;
        unselectedColor = m_button.colors.normalColor;
    }

    private void Start()
    {
        if (isSelected)
        {
            UIController.instance.Open(this);
        }
    }

    public void Selected()
    {
        isSelected = true;

        targetPanel.SetActive(true);

        ColorBlock _colorBlock = m_button.colors;
        _colorBlock.normalColor = selectedColor;
        m_button.colors = _colorBlock;
    }

    public void UnSelected()
    {
        isSelected = false;

        targetPanel.SetActive(false);

        ColorBlock _colorBlock = m_button.colors;
        _colorBlock.normalColor = unselectedColor;
        m_button.colors = _colorBlock;
    }
}

public enum PanelGroup
{
    mainPanel,
    addAccount
}
