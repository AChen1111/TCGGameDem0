using System;
using System.Collections.Generic;

public interface IRowItem<TData>
{
    int RowCardCount { get; }
    void SetRowData(int rowIndex, List<TData> allData, int selectedIndex, Action<int> onSelected);
}
