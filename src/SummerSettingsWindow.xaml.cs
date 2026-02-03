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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ver20
{
    public partial class SummerSettingsWindow : Window
    {
        public double PropCityValue { get; private set; } = 0.3;
        public double PropHighwayValue { get; private set; } = 0.7;
        public double RatesCityValue { get; private set; } = 11.5;
        public double RatesHighwayValue { get; private set; } = 8.5;

        private readonly Action RecalculateCallback;

        public SummerSettingsWindow(double propCity, double propHighway, double ratesCity, double ratesHighway, Action recalc)
        {
            InitializeComponent();
            RecalculateCallback = recalc;

            PropCity.Text = (propCity * 100).ToString("F0");
            PropHighway.Text = (propHighway * 100).ToString("F0");
            RatesCity.Text = ratesCity.ToString(CultureInfo.InvariantCulture);
            RatesHighway.Text = ratesHighway.ToString(CultureInfo.InvariantCulture);

            PropCity.TextChanged += (s, e) =>
            {
                if (double.TryParse(PropCity.Text, out double city))
                {
                    if (city >= 0 && city <= 100)
                    {
                        PropHighway.Text = (100 - city).ToString("F0");
                        PropCity.ClearValue(Border.BorderBrushProperty);
                        PropHighway.ClearValue(Border.BorderBrushProperty);
                        ErrorTextCity.Visibility = Visibility.Collapsed;
                        ErrorTextHighway.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        PropCity.BorderBrush = Brushes.Red;
                        ErrorTextCity.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    PropCity.BorderBrush = Brushes.Red;
                    ErrorTextCity.Visibility = Visibility.Visible;
                }
            };

            PropHighway.TextChanged += (s, e) =>
            {
                if (double.TryParse(PropHighway.Text, out double highway))
                {
                    if (highway >= 0 && highway <= 100)
                    {
                        ErrorTextHighway.Visibility = Visibility.Collapsed;
                        PropHighway.ClearValue(Border.BorderBrushProperty);
                    }
                    else
                    {
                        PropHighway.BorderBrush = Brushes.Red;
                        ErrorTextHighway.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    PropHighway.BorderBrush = Brushes.Red;
                    ErrorTextHighway.Visibility = Visibility.Visible;
                }
            };

            BtnSave.Click += (s, e) =>
            {
                bool cityValid = double.TryParse(PropCity.Text, out double city) && city >= 0 && city <= 100;
                bool highwayValid = double.TryParse(PropHighway.Text, out double highway) && highway >= 0 && highway <= 100;
                bool ratesCityValid = double.TryParse(RatesCity.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double ratesC);
                bool ratesHighwayValid = double.TryParse(RatesHighway.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double ratesH);

                if (!cityValid) { ErrorTextCity.Visibility = Visibility.Visible; PropCity.BorderBrush = Brushes.Red; }
                if (!highwayValid) { ErrorTextHighway.Visibility = Visibility.Visible; PropHighway.BorderBrush = Brushes.Red; }
                if (!ratesCityValid || !ratesHighwayValid)
                {
                    MessageBox.Show("Неверные нормы расхода", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!cityValid || !highwayValid) return;

                PropCityValue = city / 100.0;
                PropHighwayValue = 1.0 - PropCityValue;
                RatesCityValue = ratesC;
                RatesHighwayValue = ratesH;

                DialogResult = true;

                RecalculateCallback?.Invoke();
            };
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NumberOnly(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            char c = e.Text[0];

            if (c == ',')
            {
                int caret = textBox.CaretIndex;
                textBox.Text = textBox.Text.Insert(caret, ".");
                textBox.CaretIndex = caret + 1;
                e.Handled = true;
                return;
            }

            if (!char.IsDigit(c) && c != '.') { e.Handled = true; return; }

            if (c == '.' && textBox.Text.Contains('.')) e.Handled = true;
        }
    }
}