using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;

    [SerializeField] private Button btnAutoplayWin;

    [SerializeField] private Button btnAutoplayLose;

    private UIMainManager m_mngr;


    private void Awake()
    {
        AutoAssignButtonsIfNeeded();

        if (btnMoves != null)
        {
            btnMoves.onClick.AddListener(OnClickMoves);
        }
        else
        {
            Debug.LogError("[UIPanelMain] Button Moves chưa được gán.");
        }

        if (btnTimer != null)
        {
            btnTimer.onClick.AddListener(OnClickTimer);
        }
        else
        {
            Debug.LogError("[UIPanelMain] Button Timer chưa được gán.");
        }

        if (btnAutoplayWin != null)
        {
            btnAutoplayWin.onClick.AddListener(OnClickAutoWin);
        }
        else
        {
            Debug.LogWarning("[UIPanelMain] Button Autoplay Win chưa được gán.");
        }

        if (btnAutoplayLose != null)
        {
            btnAutoplayLose.onClick.AddListener(OnClickAutoLose);
        }
        else
        {
            Debug.LogWarning("[UIPanelMain] Button Autoplay Lose chưa được gán.");
        }
    }

    private void OnDestroy()
    {
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        if (btnTimer) btnTimer.onClick.RemoveAllListeners();
        if (btnAutoplayWin) btnAutoplayWin.onClick.RemoveAllListeners();
        if (btnAutoplayLose) btnAutoplayLose.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickTimer()
    {
        m_mngr.LoadLevelTimer();
    }

    private void OnClickMoves()
    {
        m_mngr.LoadLevelMoves();
    }

    private void OnClickAutoWin()
    {
        m_mngr.LoadLevelAuto(BoardController.AutoPlayMode.Win);
    }

    private void OnClickAutoLose()
    {
        m_mngr.LoadLevelAuto(BoardController.AutoPlayMode.Lose);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void AutoAssignButtonsIfNeeded()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        if (buttons == null || buttons.Length == 0) return;

        if (btnMoves == null)
        {
            btnMoves = buttons.FirstOrDefault(b =>
                b != btnTimer &&
                b.name.ToLowerInvariant().Contains("move"));
        }

        if (btnTimer == null)
        {
            btnTimer = buttons.FirstOrDefault(b =>
                b != btnMoves &&
                b.name.ToLowerInvariant().Contains("time"));
        }

        if (btnMoves == null && buttons.Length > 0)
        {
            btnMoves = buttons[0];
        }

        if (btnTimer == null && buttons.Length > 1)
        {
            btnTimer = buttons[1];
        }

        if (btnAutoplayWin == null)
        {
            btnAutoplayWin = buttons.FirstOrDefault(b =>
                b != btnMoves &&
                b != btnTimer &&
                b.name.ToLowerInvariant().Contains("auto") &&
                b.name.ToLowerInvariant().Contains("win"));
        }

        if (btnAutoplayLose == null)
        {
            btnAutoplayLose = buttons.FirstOrDefault(b =>
                b != btnMoves &&
                b != btnTimer &&
                b != btnAutoplayWin &&
                b.name.ToLowerInvariant().Contains("auto") &&
                (b.name.ToLowerInvariant().Contains("lose") || b.name.ToLowerInvariant().Contains("fail")));
        }
    }
}
