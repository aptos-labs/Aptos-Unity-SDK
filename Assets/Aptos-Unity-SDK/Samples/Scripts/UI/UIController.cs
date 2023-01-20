using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    static public UIController instance { get; set; }

    public List<PanelTab> panelTabs;
    [Space]
    [SerializeField] private TMP_Text mainPanelTitle;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open(PanelTab _panelTab)
    {
        foreach (PanelTab _childPanelTab in panelTabs)
        {
            if (_childPanelTab.panelGroup == _panelTab.panelGroup)
            {
                _childPanelTab.UnSelected();
            }
        }

        _panelTab.Selected();

        if(_panelTab.panelGroup == PanelGroup.mainPanel)
        {
            mainPanelTitle.text = _panelTab.tabName;
        }
    }
}
