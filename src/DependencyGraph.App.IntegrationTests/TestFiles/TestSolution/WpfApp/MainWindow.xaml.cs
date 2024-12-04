// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
using ClassLibrary;

namespace WpfApp
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private Calculator _calculator;

    public MainWindow()
    {
      InitializeComponent();
      _calculator = new Calculator();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      LabelResult.Content = _calculator.Add(int.Parse(TextBoxX.Text), int.Parse(TextBoxY.Text));
    }
  }
}
