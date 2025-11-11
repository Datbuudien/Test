using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
    }

    public enum eGameResult
    {
        NONE,
        WIN,
        LOSE
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }

    public eGameResult LastResult { get; private set; } = eGameResult.NONE;

    private GameSettings m_gameSettings;

    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private Coroutine m_gameOverRoutine;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }

    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode, BoardController.AutoPlayMode autoPlayMode = BoardController.AutoPlayMode.None)
    {
        _ = mode;

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        Text statusText = m_uiMenu.GetLevelConditionView();
        m_boardController.StartGame(this, m_gameSettings, statusText);

        LastResult = eGameResult.NONE;

        if (autoPlayMode != BoardController.AutoPlayMode.None)
        {
            m_boardController.StartAutoplay(autoPlayMode);
        }

        State = eStateGame.GAME_STARTED;
    }

    public void LoadLevelAuto(BoardController.AutoPlayMode autoPlayMode)
    {
        LoadLevel(eLevelMode.MOVES, autoPlayMode);
    }

    public void GameOver()
    {
        ReportResult(eGameResult.LOSE);
    }

    internal void ReportResult(eGameResult result)
    {
        if (State == eStateGame.GAME_OVER) return;

        LastResult = result;

        if (m_gameOverRoutine != null)
        {
            StopCoroutine(m_gameOverRoutine);
        }

        m_gameOverRoutine = StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }

        if (m_gameOverRoutine != null)
        {
            StopCoroutine(m_gameOverRoutine);
            m_gameOverRoutine = null;
        }

        LastResult = eGameResult.NONE;
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController != null && m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = eStateGame.GAME_OVER;

        m_gameOverRoutine = null;
    }

    public void StartAutoplay(BoardController.AutoPlayMode mode)
    {
        if (m_boardController != null && State == eStateGame.GAME_STARTED)
        {
            m_boardController.StartAutoplay(mode);
        }
    }

    public void StopAutoplay()
    {
        if (m_boardController != null)
        {
            m_boardController.StopAutoplay();
        }
    }
}
