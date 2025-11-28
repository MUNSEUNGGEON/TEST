using System;
using System.Data;

namespace Novacos_AIManager.ViewModel
{
    public sealed class QueryColumn
    {
        public string Header { get; }
        public string? SourceColumn { get; }
        public Type? DataType { get; }

        private readonly Func<int, object?>? _valueFactory;

        private QueryColumn(string header, string? sourceColumn, Func<int, object?>? valueFactory, Type? dataType)
        {
            Header = header;
            SourceColumn = sourceColumn;
            _valueFactory = valueFactory;
            DataType = dataType;
        }

        public static QueryColumn FromSource(string sourceColumn, string? header = null)
        {
            return new QueryColumn(header ?? sourceColumn, sourceColumn, null, null);
        }

        public static QueryColumn RowNumber(string header = "No")
        {
            return new QueryColumn(header, null, index => index + 1, typeof(int));
        }

        public object? GetValue(DataRow row, int rowIndex)
        {
            if (SourceColumn != null)
            {
                return row.Table.Columns.Contains(SourceColumn) ? row[SourceColumn] : null;
            }

            return _valueFactory?.Invoke(rowIndex);
        }
    }
}
