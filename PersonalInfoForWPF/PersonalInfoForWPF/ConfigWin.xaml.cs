using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SystemLibrary;

namespace PersonalInfoForWPF
{
    /// <summary>
    /// Interaction logic for ConfigWin.xaml
    /// </summary>
    public partial class ConfigWin : Window
    {
        public ConfigWin()
        {
            
            InitializeComponent();
            ShowConfigArgus();
        }

        private void ShowConfigArgus()
        {
            txtTreeNodeDefaultFontSize.Text = SystemConfig.configArgus.TreeNodeDefaultFontSize.ToString();
            txtRichTextEditorDefaultFontSize.Text = SystemConfig.configArgus.RichTextEditorDefaultFontSize.ToString();
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            if (String.IsNullOrEmpty(txtTreeNodeDefaultFontSize.Text) == false)
            {
                SystemConfig.configArgus.TreeNodeDefaultFontSize = Convert.ToDouble(txtTreeNodeDefaultFontSize.Text);
            }
            if (String.IsNullOrEmpty(txtRichTextEditorDefaultFontSize.Text) == false)
            {
                SystemConfig.configArgus.RichTextEditorDefaultFontSize = Convert.ToDouble(txtRichTextEditorDefaultFontSize.Text);
            }
            Close();
        }
    }
}
