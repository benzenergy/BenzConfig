/*
 * BenzConfig
 * Copyright (C) 2024 benzenergy
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ver20
{
    public partial class MainWindow : Window
    {
        private const string SettingsFile = "settings.json";

        private double SummerPropCity;
        private double SummerPropHighway;
        private double SummerRatesCity;
        private double SummerRatesHighway;

        private double WinterPropCity;
        private double WinterPropHighway;
        private double WinterRatesCity;
        private double WinterRatesHighway;

        private CancellationTokenSource? _typingCtsSummer;
        private CancellationTokenSource? _typingCtsWinter;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();

            BtnAbout.Click += (s, e) =>
            {
                var about = new AboutWindow() { Owner = this };
                about.ShowDialog();
            };

            // Кнопка настройки лета
            BtnGearSummer.Click += (s, e) =>
            {
                var win = new SummerSettingsWindow(
                    SummerPropCity, SummerPropHighway, SummerRatesCity, SummerRatesHighway,
                    RecalculateSummer)
                { Owner = this };

                if (win.ShowDialog() == true)
                {
                    SummerPropCity = win.PropCityValue;
                    SummerPropHighway = win.PropHighwayValue;
                    SummerRatesCity = win.RatesCityValue;
                    SummerRatesHighway = win.RatesHighwayValue;

                    RecalculateSummer();
                }
            };

            // Кнопка настройки зимы
            BtnGearWinter.Click += (s, e) =>
            {
                var win = new WinterSettingsWindow(
                    WinterPropCity, WinterPropHighway, WinterRatesCity, WinterRatesHighway,
                    RecalculateWinter)
                { Owner = this };

                if (win.ShowDialog() == true)
                {
                    WinterPropCity = win.PropCityValue;
                    WinterPropHighway = win.PropHighwayValue;
                    WinterRatesCity = win.RatesCityValue;
                    WinterRatesHighway = win.RatesHighwayValue;

                    RecalculateWinter();
                }
            };

            BtnSummer.Click += BtnSummer_Click;
            BtnWinter.Click += BtnWinter_Click;
        }

        // Автоперерасчёт для лета
        private void RecalculateSummer()
        {
            BtnSummer_Click(this, new RoutedEventArgs());
        }

        // Автоперерасчёт для зимы
        private void RecalculateWinter()
        {
            BtnWinter_Click(this, new RoutedEventArgs());
        }

        // Ввод только цифр, запятой или точки
        private void NumberOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d.,]+$");
        }

        // Перетаскивание окна за шапку
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        // Свернуть окно
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Закрытие по кнопке "X"
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task TypeTextAsync(TextBox textBox, string text, int delayMs, CancellationToken token)
        {
            textBox.Clear();

            try
            {
                foreach (char c in text)
                {
                    token.ThrowIfCancellationRequested();
                    textBox.AppendText(c.ToString());
                    textBox.ScrollToEnd();
                    await Task.Delay(delayMs, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену
            }
        }

        private async void BtnSummer_Click(object? sender, RoutedEventArgs? e)
        {
            try
            {
                double distance = double.Parse(InputSummer.Text.Replace(',', '.'), CultureInfo.InvariantCulture);

                double roadCity = Math.Round(SummerPropCity * distance, 2);
                double roadHighway = Math.Round(SummerPropHighway * distance, 2);

                double resultCity = Math.Round(roadCity / 100 * SummerRatesCity, 2);
                double resultHighway = Math.Round(roadHighway / 100 * SummerRatesHighway, 2);

                double total = Math.Round(resultCity + resultHighway, 2);

                string resultText =
                    $"Общий расход: {total} л\n\n" +
                    $"Детализация\n" +
                    $"Пробег по городу: {roadCity} км\n" +
                    $"Пробег по трассе: {roadHighway} км\n\n" +
                    $"Нормы расхода\n" +
                    $"Город: {SummerRatesCity:F2} л на 100 км\n" +
                    $"Трасса: {SummerRatesHighway:F2} л на 100 км\n\n" +
                    $"Пропорции\n" +
                    $"Городской режим: {SummerPropCity * 100:F0}%\n" +
                    $"Трассовый режим: {SummerPropHighway * 100:F0}%";

                _typingCtsSummer?.Cancel();
                _typingCtsSummer = new CancellationTokenSource();

                await TypeTextAsync(OutSummer, resultText, 1, _typingCtsSummer.Token);
            }
            catch
            {
                var error = new ErrorWindow("Проверьте введённые данные") { Owner = this };
                error.ShowDialog();
            }
        }

        private async void BtnWinter_Click(object? sender, RoutedEventArgs? e)
        {
            try
            {
                double distance = double.Parse(InputWinter.Text.Replace(',', '.'), CultureInfo.InvariantCulture);

                double roadCity = Math.Round(WinterPropCity * distance, 2);
                double roadHighway = Math.Round(WinterPropHighway * distance, 2);

                double resultCity = Math.Round(roadCity / 100 * WinterRatesCity, 2);
                double resultHighway = Math.Round(roadHighway / 100 * WinterRatesHighway, 2);

                double total = Math.Round(resultCity + resultHighway, 2);

                string resultText =
                    $"Общий расход: {total} л\n\n" +
                    $"Детализация\n" +
                    $"Пробег по городу: {roadCity} км\n" +
                    $"Пробег по трассе: {roadHighway} км\n\n" +
                    $"Нормы расхода\n" +
                    $"Город: {WinterRatesCity:F2} л на 100 км\n" +
                    $"Трасса: {WinterRatesHighway:F2} л на 100 км\n\n" +
                    $"Пропорции\n" +
                    $"Городской режим: {WinterPropCity * 100:F0}%\n" +
                    $"Трассовый режим: {WinterPropHighway * 100:F0}%";

                _typingCtsWinter?.Cancel();
                _typingCtsWinter = new CancellationTokenSource();

                await TypeTextAsync(OutWinter, resultText, 1, _typingCtsWinter.Token);
            }
            catch
            {
                var error = new ErrorWindow("Проверьте введённые данные") { Owner = this };
                error.ShowDialog();
            }
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsFile))
            {
                SummerPropCity = 0.3;
                SummerPropHighway = 0.7;
                SummerRatesCity = 11.5;
                SummerRatesHighway = 8.5;

                WinterPropCity = 0.3;
                WinterPropHighway = 0.7;
                WinterRatesCity = 13.8;
                WinterRatesHighway = 10.2;
                return;
            }

            try
            {
                var json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings != null)
                {
                    SummerPropCity = settings.Summer.PropCity;
                    SummerPropHighway = settings.Summer.PropHighway;
                    SummerRatesCity = settings.Summer.RateCity;
                    SummerRatesHighway = settings.Summer.RateHighway;

                    WinterPropCity = settings.Winter.PropCity;
                    WinterPropHighway = settings.Winter.PropHighway;
                    WinterRatesCity = settings.Winter.RateCity;
                    WinterRatesHighway = settings.Winter.RateHighway;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка загрузки настроек", "Ошибка");
            }
        }

        private void SaveSettings()
        {
            var settings = new AppSettings
            {
                Summer = new SeasonSettings
                {
                    PropCity = SummerPropCity,
                    PropHighway = SummerPropHighway,
                    RateCity = SummerRatesCity,
                    RateHighway = SummerRatesHighway
                },
                Winter = new SeasonSettings
                {
                    PropCity = WinterPropCity,
                    PropHighway = WinterPropHighway,
                    RateCity = WinterRatesCity,
                    RateHighway = WinterRatesHighway
                }
            };

            File.WriteAllText(SettingsFile,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            base.OnClosing(e);
        }

        private void BtnGearWinter_Click(object sender, RoutedEventArgs e) { }

        private void InputWinter_TextChanged(object sender, TextChangedEventArgs e) { }

        private void BtnSummer_Click_1(object sender, RoutedEventArgs e) { }
    }
}
