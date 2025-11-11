using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIMainManager : MonoBehaviour
{
    private IMenu[] m_menuList;

    private GameManager m_gameManager;

    private void Awake()
    {
        m_menuList = GetComponentsInChildren<IMenu>(true);
    }

    void Start()
    {
        for (int i = 0; i < m_menuList.Length; i++)
        {
            m_menuList[i].Setup(this);
        }
    }

    internal void ShowMainMenu()
    {
        m_gameManager.ClearLevel();
        m_gameManager.SetState(GameManager.eStateGame.MAIN_MENU);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_gameManager.State == GameManager.eStateGame.GAME_STARTED)
            {
                m_gameManager.SetState(GameManager.eStateGame.PAUSE);
            }
            else if (m_gameManager.State == GameManager.eStateGame.PAUSE)
            {
                m_gameManager.SetState(GameManager.eStateGame.GAME_STARTED);
            }
        }
    }

    internal void Setup(GameManager gameManager)
    {
        m_gameManager = gameManager;
        m_gameManager.StateChangedAction += OnGameStateChange;
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.SETUP:
                break;
            case GameManager.eStateGame.MAIN_MENU:
                ShowMenu<UIPanelMain>();
                break;
            case GameManager.eStateGame.GAME_STARTED:
                ShowMenu<UIPanelGame>();
                break;
            case GameManager.eStateGame.PAUSE:
                ShowMenu<UIPanelPause>();
                break;
            case GameManager.eStateGame.GAME_OVER:
                ShowGameOverMenu();
                break;
        }
    }

    private void ShowMenu<T>() where T : IMenu
    {
        for (int i = 0; i < m_menuList.Length; i++)
        {
            IMenu menu = m_menuList[i];
            if(menu is T)
            {
                menu.Show();
            }
            else
            {
                menu.Hide();
            }            
        }
    }

    private void ShowGameOverMenu()
    {
        // Ẩn tất cả panel trước
        for (int i = 0; i < m_menuList.Length; i++)
        {
            m_menuList[i].Hide();
        }

        // Ẩn tất cả GameObject có tên chứa "Win" hoặc "PanelWin" trong Canvas (nếu có panel win riêng)
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child != canvas.transform)
                {
                    string objName = child.name.ToLowerInvariant();
                    if ((objName.Contains("win") || objName.Contains("panelwin")) && 
                        child.GetComponent<UIPanelGameOver>() == null &&
                        child.GetComponent<IMenu>() == null)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        // Chỉ hiển thị UIPanelGameOver
        UIPanelGameOver gameOverPanel = m_menuList.FirstOrDefault(x => x is UIPanelGameOver) as UIPanelGameOver;
        if (gameOverPanel != null)
        {
            gameOverPanel.Show();
        }
    }

    internal Text GetLevelConditionView()
    {
        UIPanelGame game = m_menuList.Where(x => x is UIPanelGame).Cast<UIPanelGame>().FirstOrDefault();
        if (game)
        {
            return game.LevelConditionView;
        }

        return null;
    }

    internal void ShowPauseMenu()
    {
        m_gameManager.SetState(GameManager.eStateGame.PAUSE);
    }

    internal void LoadLevelMoves()
    {
        m_gameManager.LoadLevel(GameManager.eLevelMode.MOVES);
    }

    internal void LoadLevelTimer()
    {
        m_gameManager.LoadLevel(GameManager.eLevelMode.TIMER);
    }

    internal void LoadLevelAuto(BoardController.AutoPlayMode mode)
    {
        m_gameManager.LoadLevelAuto(mode);
    }

    internal void ShowGameMenu()
    {
        m_gameManager.SetState(GameManager.eStateGame.GAME_STARTED);
    }

    internal GameManager GetGameManager()
    {
        return m_gameManager;
    }

    internal void StartAutoplay(BoardController.AutoPlayMode mode)
    {
        if (m_gameManager != null)
        {
            m_gameManager.StartAutoplay(mode);
        }
    }

    internal void StopAutoplay()
    {
        if (m_gameManager != null)
        {
            m_gameManager.StopAutoplay();
        }
    }
}
