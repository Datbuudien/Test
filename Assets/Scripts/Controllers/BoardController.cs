using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private const float ITEM_MOVE_DURATION = 0.25f;
    private const float CLEAR_DELAY = 0.15f;

    private Board m_board;
    private GameManager m_gameManager;
    private Camera m_cam;
    private BottomCollector m_bottomCollector;
    private Text m_statusText;
    private bool m_gameOver;
    private AutoPlayMode m_autoPlayMode = AutoPlayMode.None;
    private Coroutine m_autoCoroutine;

    public enum AutoPlayMode
    {
        None,
        Win,
        Lose
    }

    public void StartGame(GameManager gameManager, GameSettings gameSettings, Text statusText)
    {
        m_gameManager = gameManager;
        m_statusText = statusText;

        StopAutoplay();

        // Unsubscribe event handler cũ nếu có
        if (m_gameManager != null)
        {
            m_gameManager.StateChangedAction -= OnGameStateChange;
        }

        // Clear board cũ trước khi tạo mới
        if (m_board != null)
        {
            m_board.Clear();
            m_board = null;
        }

        // Xóa tất cả child objects cũ để tránh đè lên nhau
        while (this.transform.childCount > 0)
        {
            GameObject child = this.transform.GetChild(0).gameObject;
            DestroyImmediate(child);
        }

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);
        m_bottomCollector = m_board.GetBottomCollector();

        FillTopArea();

        UpdateStatusText();

        IsBusy = false;
        m_gameOver = false;
    }

    private void FillTopArea()
    {
        m_board.Fill();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                if (!m_gameOver)
                {
                    IsBusy = false;
                    if (m_autoPlayMode != AutoPlayMode.None && m_autoCoroutine == null)
                    {
                        m_autoCoroutine = StartCoroutine(AutoPlayRoutine());
                    }
                }
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                StopAutoplay();
                IsBusy = true;
                break;
        }
    }

    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;
        if (m_gameManager.State != GameManager.eStateGame.GAME_STARTED) return;
        if (m_autoPlayMode != AutoPlayMode.None) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null)
                {
                    HandleCellTap(cell);
                }
            }
        }
    }

    private void HandleCellTap(Cell cell)
    {
        if (m_gameOver) return;
        if (cell.BoardY == 0) return;
        if (cell.IsEmpty) return;

        if (!m_bottomCollector.HasEmptySlot())
        {
            CheckLoseCondition();
            return;
        }

        Cell target = m_bottomCollector.GetFirstEmpty();
        if (target == null)
        {
            CheckLoseCondition();
            return;
        }

        MoveItemToBottom(cell, target);
    }

    private void MoveItemToBottom(Cell source, Cell target)
    {
        Item item = source.Item;
        source.Free();

        target.Assign(item);

        IsBusy = true;
        item.SetSortingLayerHigher();
        item.View.DOMove(target.transform.position, ITEM_MOVE_DURATION).OnComplete(() =>
        {
            item.SetSortingLayerLower();
            StartCoroutine(ResolveAfterPlacement());
        });

        OnMoveEvent();
    }

    private IEnumerator ResolveAfterPlacement()
    {
        yield return new WaitForSeconds(ITEM_MOVE_DURATION);

        bool cleared = ClearTripletsIfAny();
        if (cleared)
        {
            yield return new WaitForSeconds(CLEAR_DELAY);
        }

        UpdateStatusText();

        if (!CheckWinCondition())
        {
            CheckLoseCondition();
        }

        IsBusy = false;
    }

    private bool ClearTripletsIfAny()
    {
        var groups = m_bottomCollector
            .OccupiedSlots()
            .Where(cell => cell.Item is NormalItem)
            .GroupBy(cell => ((NormalItem)cell.Item).ItemType)
            .Where(group => group.Count() == 3)
            .ToList();

        if (groups.Count == 0)
        {
            return false;
        }

        foreach (var group in groups)
        {
            foreach (var cell in group)
            {
                cell.ExplodeItem();
            }
        }

        return true;
    }

    private bool CheckWinCondition()
    {
        bool topEmpty = m_board.AreTopCellsEmpty();
        bool bottomEmpty = !m_bottomCollector.OccupiedSlots().Any();

        if (topEmpty && bottomEmpty)
        {
            ReportGameResult(GameManager.eGameResult.WIN);
            return true;
        }

        return false;
    }

    private void CheckLoseCondition()
    {
        if (m_gameOver) return;

        if (!m_bottomCollector.IsFull())
        {
            return;
        }

        bool hasTriplet = m_bottomCollector
            .OccupiedSlots()
            .Where(cell => cell.Item is NormalItem)
            .GroupBy(cell => ((NormalItem)cell.Item).ItemType)
            .Any(group => group.Count() == 3);

        if (!hasTriplet)
        {
            ReportGameResult(GameManager.eGameResult.LOSE);
        }
    }

    private void ReportGameResult(GameManager.eGameResult result)
    {
        if (m_gameOver) return;

        m_gameOver = true;
        IsBusy = false;
        StopAutoplay();
        m_gameManager.ReportResult(result);
    }

    private void UpdateStatusText()
    {
        if (m_statusText == null) return;

        int remainingTop = m_board.CountTopItems();
        int filledSlots = m_bottomCollector.OccupiedSlots().Count();
        int totalSlots = m_bottomCollector.Capacity;

        m_statusText.text = $"Ô còn lại: {remainingTop}\nKho đáy: {filledSlots}/{totalSlots}";
    }

    public void StartAutoplay(AutoPlayMode mode)
    {
        StopAutoplay();

        if (mode == AutoPlayMode.None)
        {
            return;
        }

        m_autoPlayMode = mode;

        if (!m_gameOver && m_gameManager.State == GameManager.eStateGame.GAME_STARTED)
        {
            m_autoCoroutine = StartCoroutine(AutoPlayRoutine());
        }
    }

    public void StopAutoplay()
    {
        if (m_autoCoroutine != null)
        {
            StopCoroutine(m_autoCoroutine);
            m_autoCoroutine = null;
        }

        m_autoPlayMode = AutoPlayMode.None;
    }

    private IEnumerator AutoPlayRoutine()
    {
        while (!m_gameOver && m_autoPlayMode != AutoPlayMode.None)
        {
            yield return new WaitUntil(() => !IsBusy || m_gameOver);
            if (m_gameOver || m_autoPlayMode == AutoPlayMode.None) break;

            if (m_gameManager.State != GameManager.eStateGame.GAME_STARTED)
            {
                yield return null;
                continue;
            }

            Cell target = m_autoPlayMode == AutoPlayMode.Win ? GetNextCellForAutoWin() : GetNextCellForAutoLose();

            if (target == null)
            {
                StopAutoplay();
                yield break;
            }

            HandleCellTap(target);

            yield return new WaitUntil(() => !IsBusy || m_gameOver);
            if (m_gameOver) break;

            yield return new WaitForSeconds(0.5f);
        }

        StopAutoplay();
    }

    private Cell GetNextCellForAutoWin()
    {
        var bottomCounts = GetBottomTypeCounts();

        foreach (var pending in bottomCounts.Where(pair => pair.Value > 0 && pair.Value < 3))
        {
            Cell cell = m_board.GetTopCells(pending.Key).FirstOrDefault();
            if (cell != null)
            {
                return cell;
            }
        }

        var topGroups = m_board.GetTopCellsGroupedByType()
            .OrderByDescending(pair => pair.Value.Count);

        foreach (var group in topGroups)
        {
            if (group.Value.Count > 0)
            {
                return group.Value.First();
            }
        }

        return null;
    }

    private Cell GetNextCellForAutoLose()
    {
        var bottomCounts = GetBottomTypeCounts();
        var topGroups = m_board.GetTopCellsGroupedByType()
            .OrderBy(pair => bottomCounts.ContainsKey(pair.Key) ? bottomCounts[pair.Key] : 0);

        foreach (var group in topGroups)
        {
            int countInBottom = bottomCounts.ContainsKey(group.Key) ? bottomCounts[group.Key] : 0;
            if (countInBottom < 2 && group.Value.Count > 0)
            {
                return group.Value.First();
            }
        }

        return null;
    }

    private Dictionary<NormalItem.eNormalType, int> GetBottomTypeCounts()
    {
        return m_bottomCollector
            .OccupiedSlots()
            .Select(cell => cell.Item as NormalItem)
            .Where(item => item != null)
            .GroupBy(item => item.ItemType)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    internal void Clear()
    {
        StopAutoplay();
        m_board.Clear();
    }

    private void OnDestroy()
    {
        if (m_gameManager != null)
        {
            m_gameManager.StateChangedAction -= OnGameStateChange;
        }
    }
}
