using KuittiParseri.Models;
using KuittiParseri.Services;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using PekkasParseri.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace KuittiParseri
{
    public partial class MainWindow : Window
    {
        private readonly ParseOrchestrator _orchestrator;

        public MainWindow()
        {
            InitializeComponent();
            _orchestrator = new ParseOrchestrator();
        }

        private void TestOcr_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF-tiedostot (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var extractor = new Services.OcrTextExtractor("./tessdata", "fin");
                    //var extractor = new OcrTextExtractor(@"D:\VisualStudioProjektit\KuittiParseri\tessdata", "fin");
                    string text = extractor.ExtractTextFromPdf(dialog.FileName);

                    // Näytetään vain ensimmäiset 500 merkkiä, ettei ikkuna räjähdä
                    string preview = text.Length > 500 ? text.Substring(0, 500) + "..." : text;

                    MessageBox.Show(preview, "OCR-tulos");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Virhe OCR-ajossa: {ex.Message}", "Virhe");
                }
            }
        }

        private void TestOrchestrator_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF-tiedostot (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var orchestrator = new KuittiParseri.Services.ParseOrchestrator();
                    var result = orchestrator.ParseFile(dialog.FileName);

                    // Näytetään tärkeimmät kentät
                    string msg =
                        $"Tiedosto: {result.FileName}\n" +
                        $"Nimi: {result.Name}\n" +
                        $"Palkkajakso: {result.PayPeriod}\n" +
                        $"Tunnit: {result.Hours}\n" +
                        $"WorkCodes: {result.WorkCodesSummary}\n" +
                        (string.IsNullOrWhiteSpace(result.Error) ? "" : $"Virhe: {result.Error}");

                    // Luetaan debug_mode.txt
                    string modeInfo = File.Exists("debug_mode.txt")
                        ? File.ReadAllText("debug_mode.txt")
                        : "Tuntematon";

                    // Lisätään emoji värikoodiksi
                    if (modeInfo.Contains("iTextSharp", StringComparison.OrdinalIgnoreCase))
                        modeInfo = "✅ " + modeInfo;
                    else if (modeInfo.Contains("OCR", StringComparison.OrdinalIgnoreCase))
                        modeInfo = "🔴 " + modeInfo;

                    MessageBox.Show(msg + "\n\n" + modeInfo, "Orchestrator-testi");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Virhe Orchestrator-ajossa: {ex.Message}", "Virhe");
                }
            }
        }


        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PDF-tiedostot (*.pdf)|*.pdf",
                Multiselect = true
            };

            if (dialog.ShowDialog() != true) return;

            var results = new List<FileResult>();

            foreach (var filePath in dialog.FileNames)
            {
                try
                {
                    var result = _orchestrator.ParseFile(filePath);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new FileResult
                    {
                        FileName = Path.GetFileName(filePath),
                        Code = string.Empty,
                        WorkCodesSummary = string.Empty,
                        Name = "Tuntematon",
                        Hours = 0,
                        PayPeriod = "Tuntematon",
                        Error = $"Virhe: {ex.Message}",
                        WorkCodes = new List<(string Code, string Description, double Hours)>()
                    });
                }
            }

            ResultsList.ItemsSource = results;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Valitse kansio, josta PDF-tiedostot luetaan",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() != true) return;

            var pdfFiles = Directory.GetFiles(dialog.SelectedPath, "*.pdf");
            var results = new List<FileResult>();

            foreach (var filePath in pdfFiles)
            {
                try
                {
                    var result = _orchestrator.ParseFile(filePath);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new FileResult
                    {
                        FileName = Path.GetFileName(filePath),
                        Code = string.Empty,
                        WorkCodesSummary = string.Empty,
                        Name = "Tuntematon",
                        Hours = 0,
                        PayPeriod = "Tuntematon",
                        Error = $"Virhe: {ex.Message}",
                        WorkCodes = new List<(string Code, string Description, double Hours)>()
                    });
                }
            }

            ResultsList.ItemsSource = results;
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Kuittiparseri\nVersio 1.0\n© 2025 Juha A Virtanen", "Tietoa");
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tulostustoiminto ei ole vielä käytössä.", "Tulosta raportti");
        }

        private void SaveToDb_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tallennus tietokantaan ei ole vielä käytössä.", "Tietokanta");
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    }
}