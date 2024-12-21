using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CSVGenerator
{
    private List<string> columns; // Názvy sloupcù
    private List<string[]> rows; // Øádky dat
    private string filePath; // Cesta k souboru

    public CSVGenerator(string filePath, params string[] columnNames)
    {
        this.filePath = filePath;
        columns = new List<string>(columnNames);
        rows = new List<string[]>();
    }

    /// <summary>
    /// Pøidá nový øádek do CSV souboru. Poøadí hodnot musí odpovídat poøadí sloupcù.
    /// </summary>
    /// <param name="values">Hodnoty pro nový øádek</param>
    public void AddRow(params string[] values)
    {
        if (values.Length != columns.Count)
        {
            throw new ArgumentException("Poèet hodnot neodpovídá poètu sloupcù.");
        }

        rows.Add(values);
    }

    /// <summary>
    /// Uloží všechna data do CSV souboru.
    /// </summary>
    public void SaveToFile()
    {
        StringBuilder sb = new StringBuilder();

        // Zapisujeme názvy sloupcù
        sb.AppendLine(string.Join(";", columns));

        // Zapisujeme data
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(";", row));
        }

        // Uložení do souboru
        File.WriteAllText(filePath, sb.ToString());
    }
}
