using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CSVGenerator
{
    private List<string> columns; // N�zvy sloupc�
    private List<string[]> rows; // ��dky dat
    private string filePath; // Cesta k souboru

    public CSVGenerator(string filePath, params string[] columnNames)
    {
        this.filePath = filePath;
        columns = new List<string>(columnNames);
        rows = new List<string[]>();
    }

    /// <summary>
    /// P�id� nov� ��dek do CSV souboru. Po�ad� hodnot mus� odpov�dat po�ad� sloupc�.
    /// </summary>
    /// <param name="values">Hodnoty pro nov� ��dek</param>
    public void AddRow(params string[] values)
    {
        if (values.Length != columns.Count)
        {
            throw new ArgumentException("Po�et hodnot neodpov�d� po�tu sloupc�.");
        }

        rows.Add(values);
    }

    /// <summary>
    /// Ulo�� v�echna data do CSV souboru.
    /// </summary>
    public void SaveToFile()
    {
        StringBuilder sb = new StringBuilder();

        // Zapisujeme n�zvy sloupc�
        sb.AppendLine(string.Join(";", columns));

        // Zapisujeme data
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(";", row));
        }

        // Ulo�en� do souboru
        File.WriteAllText(filePath, sb.ToString());
    }
}
