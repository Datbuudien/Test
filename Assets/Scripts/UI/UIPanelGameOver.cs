using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGameOver : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Text txtResult;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnClose.onClick.AddListener(OnClickClose);
    }

    private void OnDestroy()
    {
        if (btnClose) btnClose.onClick.RemoveAllListeners();
    }

    private void OnClickClose()
    {
        m_mngr.ShowMainMenu();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        UpdateResultText();
        this.gameObject.SetActive(true);
    }

    private void UpdateResultText()
    {
        if (txtResult == null || m_mngr == null) return;

        string message = "Kết thúc!";
        var gameManager = m_mngr.GetGameManager();
        if (gameManager != null)
        {
            switch (gameManager.LastResult)
            {
                case GameManager.eGameResult.WIN:
                    message = "WIN";
                    break;
                case GameManager.eGameResult.LOSE:
                    message ="HEHE LOSER";
                    break;
            }
        }

        txtResult.text = message;
    }
}
