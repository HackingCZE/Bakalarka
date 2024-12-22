using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CSVGenerator
{
    private List<string[]> rows; // ��dky dat
    private string filePath; // Cesta k souboru

    public CSVGenerator(string filePath)
    {
        this.filePath = filePath;
        rows = new List<string[]>();
    }

    /// <summary>
    /// P�id� nov� ��dek do CSV souboru. Po�ad� hodnot mus� odpov�dat po�ad� sloupc�.
    /// </summary>
    /// <param name="values">Hodnoty pro nov� ��dek</param>
    public void AddRow(params string[] values)
    {

        rows.Add(values);
    }

    /// <summary>
    /// Ulo�� v�echna data do CSV souboru.
    /// </summary>
    public void SaveToFile()
    {
        StringBuilder sb = new StringBuilder();


        // Zapisujeme data
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(";", row));
        }

        // Ulo�en� do souboru
        File.WriteAllText(filePath, sb.ToString());
    }
}
