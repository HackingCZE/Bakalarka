using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CSVGenerator
{
    private List<string[]> rows; // Øádky dat
    private string filePath; // Cesta k souboru

    public CSVGenerator(string filePath)
    {
        this.filePath = filePath;
        rows = new List<string[]>();
    }

    /// <summary>
    /// Pøidá nový øádek do CSV souboru. Poøadí hodnot musí odpovídat poøadí sloupcù.
    /// </summary>
    /// <param name="values">Hodnoty pro nový øádek</param>
    public void AddRow(params string[] values)
    {

        rows.Add(values);
    }

    /// <summary>
    /// Uloží všechna data do CSV souboru.
    /// </summary>
    public void SaveToFile()
    {
        StringBuilder sb = new StringBuilder();


        // Zapisujeme data
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(";", row));
        }

        // Uložení do souboru
        File.WriteAllText(filePath, sb.ToString());
    }
}
