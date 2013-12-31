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
                double TreeFontSize = Convert.ToDouble(txtTreeNodeDefaultFontSize.Text);
                if (Math.Abs(TreeFontSize - SystemConfig.configArgus.TreeNodeDefaultFontSize) > 1e-3)
                {
                    SystemConfig.configArgus.TreeNodeDefaultFontSize = TreeFontSize;
                    SystemConfig.configArgus.IsArgumentsValueChanged = true;
                }

            }
            if (String.IsNullOrEmpty(txtRichTextEditorDefaultFontSize.Text) == false)
            {
                double editorFontSize = Convert.ToDouble(txtRichTextEditorDefaultFontSize.Text);
                if (Math.Abs(editorFontSize - SystemConfig.configArgus.RichTextEditorDefaultFontSize) > 1e-3)
                {
                    SystemConfig.configArgus.RichTextEditorDefaultFontSize = editorFontSize;
                    SystemConfig.configArgus.IsArgumentsValueChanged = true;
                }
            }
            Close();
        }
    }
}
