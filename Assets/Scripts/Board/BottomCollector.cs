using System.Collections.Generic;
using System.Linq;

public class BottomCollector
{
    private readonly List<Cell> m_slots;

    public BottomCollector(IEnumerable<Cell> slots)
    {
        m_slots = slots
            .OrderBy(cell => cell.BoardX)
            .ToList();
    }

    public int Capacity => m_slots.Count;

    public IEnumerable<Cell> Slots => m_slots;

    public Cell GetFirstEmpty()
    {
        return m_slots.FirstOrDefault(slot => slot.IsEmpty);
    }

    public bool HasEmptySlot()
    {
        return m_slots.Any(slot => slot.IsEmpty);
    }

    public bool IsFull()
    {
        return !HasEmptySlot();
    }

    public IEnumerable<Cell> OccupiedSlots()
    {
        return m_slots.Where(slot => !slot.IsEmpty);
    }
}

