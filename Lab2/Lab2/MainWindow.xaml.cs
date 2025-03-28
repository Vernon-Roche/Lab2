using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public const int KEY_LENGTH = 39;
    private readonly HashSet<char> availableKeyChars = ['0', '1'];
    private string _fullKey = "";
    private byte[] _result;
    private LFSRAlgorythm _LFSRAlgorythm = new();
    byte[] _currentBytes;
    public MainWindow()
    {
        InitializeComponent();
        DataObject.AddPastingHandler(tbKey, tbKey_Pasting);
        DataObject.AddPastingHandler(tbSourceText, tbSourceText_Pasting);
    }

    private void tbKey_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (((TextBox)sender).Text.Length + e.Text.Length > KEY_LENGTH)
        {
            e.Handled = true;
        }
        else
        {
            foreach (char c in e.Text)
            {
                if (!availableKeyChars.Contains(c))
                {
                    e.Handled = true;
                }
            }
        }
    }
    private void tbKey_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true; // Блокируем пробел
        }
    }

    private void tbKey_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            string text = (string)e.DataObject.GetData(DataFormats.Text);
            string filteredText = new string(text.Where(c => availableKeyChars.Contains(c)).ToArray());

            if (filteredText != text)
            {
                Dispatcher.BeginInvoke(() =>
                    MessageBox.Show("Попытка вставки некорректных данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error)
                );
                e.CancelCommand();
            }
            else
            {
                if (((TextBox)sender).Text.Length + text.Length > KEY_LENGTH)
                {
                    Dispatcher.BeginInvoke(() =>
                        MessageBox.Show("Превышена максимальная длина ключа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error)
                    );
                    e.CancelCommand();
                }
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void btProcessText_Click(object sender, RoutedEventArgs e)
    {
        if (tbFullKey.Text == "")
        {
            MessageBox.Show("Сначала сгенерируйте ключ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            byte[] result = _LFSRAlgorythm.Encrypt(_currentBytes);
            _result = result;
            string resultString = "";
            int bytesLength = _LFSRAlgorythm.FullKeyLength % 8 == 0 ? _LFSRAlgorythm.FullKeyLength / 8 : _LFSRAlgorythm.FullKeyLength / 8 + 1;

            if (result.Length <= 20)
            {

                int bitePosition = _LFSRAlgorythm.FullKeyLength;
                for (int i = bytesLength - 1; i >= 0; i--)
                {
                    for (int mask = 0b00000001; mask < byte.MaxValue && --bitePosition >= 0; mask <<= 1)
                    {
                        resultString = ((result[i] & mask) == 0 ? "0" : "1") + resultString;
                    }
                }
            }
            else
            {
                resultString = ConvertToString(result);
            }
                //int bitePosition = bytesLength * 8 - 1;
                //for (int i = bytesLength; i >= 0 && (bitePosition >= (bytesLength * 8 - _LFSRAlgorythm.FullKeyLength)); i--)
                //{
                //    int mask = 0b00000001;
                //    while (bitePosition >= (bytesLength * 8 - _LFSRAlgorythm.FullKeyLength))
                //    {
                //        resultString = ((result[i] & mask) > 0 ? "1" : "0") + resultString;
                //        mask <<= 1;
                //        if (mask > byte.MaxValue)
                //            break;
                //    }
                //}
                tbResult.Text = resultString;
        }
    }

    private void btGenerateFullKey_Click(object sender, RoutedEventArgs e)
    {
        if (tbKey.Text.Length != KEY_LENGTH)
        {
            MessageBox.Show($"Ключ должен состоять из {KEY_LENGTH} символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            if (!(_currentBytes is null))
            {
                string fullKey = _LFSRAlgorythm.GenerateFullKey(tbKey.Text, _currentBytes.Length * 8);
                tbFullKey.Text = fullKey;
            }
            else
            {
                MessageBox.Show("Сначала откройте файл.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void tbSourceText_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        foreach (char c in e.Text)
        {
            if (!availableKeyChars.Contains(c))
            {
                e.Handled = true;
            }
        }
    }

    private void tbSourceText_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true; // Блокируем пробел
        }
    }

    private void tbSourceText_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            string text = (string)e.DataObject.GetData(DataFormats.Text);
            string filteredText = new string(text.Where(c => availableKeyChars.Contains(c)).ToArray());

            if (filteredText != text)
            {
                Dispatcher.BeginInvoke(() =>
                    MessageBox.Show("Попытка вставки некорректных данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error)
                );
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void miOpen_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            string filePath = openFileDialog.FileName;
            if (filePath != null)
                LoadDataFromFile(filePath, sender);
        }
    }

    private void miSave_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        if (saveFileDialog.ShowDialog() == true)
        {
            string filePath = saveFileDialog.FileName;
            SaveDataToFile(filePath, sender);
        }
    }
    private void LoadDataFromFile(string filePath, object sender)
    {
        TextBox destTextBox = tbResult;
        
        try
        {
            ClearFields();
            _currentBytes = File.ReadAllBytes(filePath);
            string bytesAsString = ConvertToString(_currentBytes);

            tbSourceText.Text = bytesAsString;
        }
        catch (Exception)
        {
            MessageBox.Show($"Ошибка при загрузке файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void SaveDataToFile(string filePath, object sender)
    {
        File.WriteAllBytes(filePath, _result);
    }

    private void ClearFields()
    {
        tbFullKey.Text = "";
        tbResult.Text = "";
    }

    static private string ConvertToString(byte[] bytes)
    {
        string result = "";
        if (bytes.Length > 20)
        {
            for (int i = 0; i < 10; i++)
            {
                int mask = 0b10000000;
                for (int j = 0; j < 8; j++)
                {
                    result += (bytes[i] & mask) == 0 ? "0" : "1";
                    mask >>= 1;
                }
            }
            result += "...";
            for (int i = bytes.Length - 10; i < bytes.Length; i++)
            {
                int mask = 0b10000000;
                for (int j = 0; j < 8; j++)
                {
                    result += (bytes[i] & mask) == 0 ? "0" : "1";
                    mask >>= 1;
                }
            }
        }
        else
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                int mask = 0b10000000;
                for (int j = 0; j < 8; j++)
                {
                    result += (bytes[i] & mask) == 0 ? "0" : "1";
                    mask >>= 1;
                }
            }
        }
        return result;
    }

    private void tbKey_TextChanged(object sender, TextChangedEventArgs e)
    {
        tbResult.Text = "";
        tbFullKey.Text = "";
    }
}