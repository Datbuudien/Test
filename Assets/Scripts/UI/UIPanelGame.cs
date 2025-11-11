using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGame : MonoBehaviour, IMenu
{
    public Text LevelConditionView;

    [SerializeField] private Button btnPause;
    [SerializeField] private Button btnAutoplayWin;
    [SerializeField] private Button btnAutoplayLose;
    [SerializeField] private Button btnStopAutoplay;

    private UIMainManager m_mngr;
    private BoardController.AutoPlayMode m_currentAutoplayMode = BoardController.AutoPlayMode.None;

    private void Awake()
    {
        AutoAssignButtonsIfNeeded();

        if (btnPause != null)
        {
            btnPause.onClick.AddListener(OnClickPause);
        }

        if (btnAutoplayWin != null)
        {
            btnAutoplayWin.onClick.AddListener(OnClickAutoWin);
        }

        if (btnAutoplayLose != null)
        {
            btnAutoplayLose.onClick.AddListener(OnClickAutoLose);
        }

        if (btnStopAutoplay != null)
        {
            btnStopAutoplay.onClick.AddListener(OnClickStopAutoplay);
        }

        UpdateAutoplayButtons();
    }

    private void OnDestroy()
    {
        if (btnPause) btnPause.onClick.RemoveAllListeners();
        if (btnAutoplayWin) btnAutoplayWin.onClick.RemoveAllListeners();
        if (btnAutoplayLose) btnAutoplayLose.onClick.RemoveAllListeners();
        if (btnStopAutoplay) btnStopAutoplay.onClick.RemoveAllListeners();
    }

    private void AutoAssignButtonsIfNeeded()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        if (buttons == null || buttons.Length == 0) return;

        if (btnAutoplayWin == null)
        {
            btnAutoplayWin = System.Array.Find(buttons, b =>
                b != btnPause &&
                b != btnAutoplayLose &&
                b != btnStopAutoplay &&
                (b.name.ToLowerInvariant().Contains("auto") || b.name.ToLowerInvariant().Contains("autoplay")) &&
                (b.name.ToLowerInvariant().Contains("win") || b.name.ToLowerInvariant().Contains("thang")));
        }

        if (btnAutoplayLose == null)
        {
            btnAutoplayLose = System.Array.Find(buttons, b =>
                b != btnPause &&
                b != btnAutoplayWin &&
                b != btnStopAutoplay &&
                (b.name.ToLowerInvariant().Contains("auto") || b.name.ToLowerInvariant().Contains("autoplay")) &&
                (b.name.ToLowerInvariant().Contains("lose") || b.name.ToLowerInvariant().Contains("thua") || b.name.ToLowerInvariant().Contains("fail")));
        }

        if (btnStopAutoplay == null)
        {
            btnStopAutoplay = System.Array.Find(buttons, b =>
                b != btnPause &&
                b != btnAutoplayWin &&
                b != btnAutoplayLose &&
                (b.name.ToLowerInvariant().Contains("stop") || b.name.ToLowerInvariant().Contains("dung")));
        }
    }

    private void OnClickPause()
    {
        m_mngr.ShowPauseMenu();
    }

    private void OnClickAutoWin()
    {
        if (m_currentAutoplayMode == BoardController.AutoPlayMode.Win)
        {
            OnClickStopAutoplay();
            return;
        }

        m_currentAutoplayMode = BoardController.AutoPlayMode.Win;
        m_mngr.StartAutoplay(BoardController.AutoPlayMode.Win);
        UpdateAutoplayButtons();
    }

    private void OnClickAutoLose()
    {
        if (m_currentAutoplayMode == BoardController.AutoPlayMode.Lose)
        {
            OnClickStopAutoplay();
            return;
        }

        m_currentAutoplayMode = BoardController.AutoPlayMode.Lose;
        m_mngr.StartAutoplay(BoardController.AutoPlayMode.Lose);
        UpdateAutoplayButtons();
    }

    private void OnClickStopAutoplay()
    {
        m_currentAutoplayMode = BoardController.AutoPlayMode.None;
        m_mngr.StopAutoplay();
        UpdateAutoplayButtons();
    }

    private void UpdateAutoplayButtons()
    {
        bool isAutoplayActive = m_currentAutoplayMode != BoardController.AutoPlayMode.None;

        if (btnAutoplayWin != null)
        {
            btnAutoplayWin.interactable = !isAutoplayActive || m_currentAutoplayMode == BoardController.AutoPlayMode.Win;
            
            Text winText = btnAutoplayWin.GetComponentInChildren<Text>();
            if (winText != null)
            {
                winText.text = m_currentAutoplayMode == BoardController.AutoPlayMode.Win ? "Auto Win (ON)" : "Auto Win";
            }
        }

        if (btnAutoplayLose != null)
        {
            btnAutoplayLose.interactable = !isAutoplayActive || m_currentAutoplayMode == BoardController.AutoPlayMode.Lose;
            
            Text loseText = btnAutoplayLose.GetComponentInChildren<Text>();
            if (loseText != null)
            {
                loseText.text = m_currentAutoplayMode == BoardController.AutoPlayMode.Lose ? "Auto Lose (ON)" : "Auto Lose";
            }
        }

        if (btnStopAutoplay != null)
        {
            btnStopAutoplay.gameObject.SetActive(isAutoplayActive);
        }
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        UpdateAutoplayButtons();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (m_mngr != null && m_mngr.GetGameManager() != null)
        {
            var gameManager = m_mngr.GetGameManager();
            if (gameManager.State == GameManager.eStateGame.GAME_OVER)
            {
                m_currentAutoplayMode = BoardController.AutoPlayMode.None;
                UpdateAutoplayButtons();
            }
        }
    }
}
