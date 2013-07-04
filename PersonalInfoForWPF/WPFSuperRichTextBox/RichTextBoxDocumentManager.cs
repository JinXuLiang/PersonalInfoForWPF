using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using System.Windows.Documents;
using System.IO;
using System.Windows;
using System.Windows.Markup;


namespace WPFSuperRichTextBox
{
    /// <summary>
    /// 用于对RichTextBox的功能进行封装与增强
    /// </summary>
    class RichTextBoxDocumentManager
    {
        private RichTextBox _rtf = null;
        /// <summary>
        /// RichTextBoxDocumentManager管理的RichTextBox控件
        /// </summary>
        public RichTextBox RichTextBoxControl
        {
            get
            {
                return _rtf;
            }
            set
            {
                _rtf = value;
            }

        }

        public RichTextBoxDocumentManager(RichTextBox rtf)
        {
            if (rtf == null)
                throw new ArgumentNullException("必须传入一个有效的RichTextBox控件引用");
            _rtf = rtf;

           
            SaveFileDialog1.Filter = "FlowDocument的XAML包文件(*.xamlpackage)|*.xamlpackage|FlowDocument的纯XAML文件(*.xaml)|*.xaml|标准XAML代码文件（*.xaml）|*.xaml|RTF文件(*.rtf)|*.rtf|纯文本文件(*.txt)|*.txt|任意扩展名（以纯文本格式表达)|*.*";
            OpenFileDialogForImageFile.Filter = "所有可处理类型的文件|*.gif;*.jpg;*.jpeg;*.png;*.bmp|JPG文件(*.jpg;*.jpeg)|*.jpg;*.jpeg|Gif文件(*.gif)|*.gif|PNG文件(*.png)|*.png|BMP文件(*.bmp)|*.bmp|任意扩展名文件(*.*)|*.*";
            
            CurFileName = "无标题.xaml";

        }
        /// <summary>
        /// 当前正在编辑的文件名
        /// </summary>
        public string CurFileName
        {
            get;
            set;
        }

        #region "变量区"
        private WinForms.SaveFileDialog SaveFileDialog1 = new WinForms.SaveFileDialog();
        private WinForms.OpenFileDialog OpenFileDialogForImageFile = new WinForms.OpenFileDialog();

        #endregion

        #region "提取文档内容"
        /// <summary>
        /// 提取当前RichTextBox所显示内容的RTF字串
        /// </summary>
        /// <returns></returns>
        private String getRTFText()
        {

            TextRange range = new TextRange(_rtf.Document.ContentStart, _rtf.Document.ContentEnd);
            using (MemoryStream mem = new MemoryStream())
            {
                range.Save(mem, DataFormats.Rtf);
                String rtf = Encoding.UTF8.GetString(mem.ToArray());
                return rtf;
            }
            
        }
        /// <summary>
        /// 使用RTF字串更新RichTextBox内容
        /// </summary>
        private void setRTFText(String RTFText)
        {
            if (String.IsNullOrEmpty(RTFText))
            {
                ClearRichTextBox();
                return;
            }
            TextRange range = new TextRange(_rtf.Document.ContentStart, _rtf.Document.ContentEnd);
            using (MemoryStream mem = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(mem, Encoding.UTF8))
                {
                    writer.Write(RTFText);
                    writer.Flush();
                    mem.Seek(0, SeekOrigin.Begin);
                    range.Load(mem, DataFormats.Rtf);
                }

            }
        }
        /// <summary>
        /// 获取或设置RichTextBox的内容,使用RichText
        /// </summary>
        public String RTF
        {
            get { return getRTFText(); }
            set{
                setRTFText(value);
            }
        }
        /// <summary>
        ///  获取或设置RichTextBox的内容,使用纯文本
        /// </summary>
        public String Text
        {
            get
            {
                return getPlainText();
            }
            set
            {
                setPlainText(value);
            }
        }
       /// <summary>
       /// 获取当前RichTextBox所显示内容的纯文本字串
       /// </summary>
       /// <returns></returns>
        private String getPlainText()
        {
            TextRange range = new TextRange(_rtf.Document.ContentStart, _rtf.Document.ContentEnd);
            return range.Text;
        }

        private void setPlainText(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                ClearRichTextBox();
            }
            else
            {
                _rtf.Document.Blocks.Clear();
                Paragraph p = new Paragraph();
                Run run = new Run(text);
                p.Inlines.Add(run);
                _rtf.Document.Blocks.Add(p);
            }
        }


        public void ClearRichTextBox()
        {
            _rtf.Document.Blocks.Clear();
        }
        #endregion
        #region "文件功能"

        /// <summary>
        /// 保存文档到文件中
        /// </summary>
        /// <param name="SaveAll">是仅保存选中部分，还是全部保存</param>
        public void SaveToFile(bool SaveAll)
        {
            try
            {
                //根据情况确定默认保存文件名
                if (SaveAll)
                    this.SaveFileDialog1.FileName = System.IO.Path.GetFileNameWithoutExtension(CurFileName);
                else
                    this.SaveFileDialog1.FileName = System.IO.Path.GetFileNameWithoutExtension(CurFileName) + "_Selection";

                WinForms.DialogResult ret = this.SaveFileDialog1.ShowDialog();
                if (ret == WinForms.DialogResult.OK)
                {
                    string FileName = SaveFileDialog1.FileName;

                    string FileExt = System.IO.Path.GetExtension(FileName).ToUpper(); //文件扩展名

                    TextRange range = null;
                    if (SaveAll == false)
                        range = _rtf.Selection;
                    else
                        range = new TextRange(_rtf.Document.ContentStart, _rtf.Document.ContentEnd);

                    using (FileStream fs = new FileStream(FileName, FileMode.Create))
                    {
                        switch (FileExt)
                        {
                            case ".XAMLPACKAGE":
                                range.Save(fs, DataFormats.XamlPackage);
                                break;
                            case ".XAML":
                                if (SaveFileDialog1.FilterIndex == 3)
                                    range.Save(fs, DataFormats.Text);//保存XAML的代码文件
                                else
                                    range.Save(fs, DataFormats.Xaml);//按流文档保存当前文本
                                break;
                            case ".RTF":
                                range.Save(fs, DataFormats.Rtf);
                                break;
                            case ".TXT":
                                range.Save(fs, DataFormats.Text);
                                break;
                            default:
                                range.Save(fs, DataFormats.Text);
                                break;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 新建文档
        /// </summary>
        public void NewDocument()
        {
            try
            {
            TextRange range = new TextRange(_rtf.Document.ContentStart, _rtf.Document.ContentEnd);

            if (MySuperEditorHelper.IsPrintableString(range.Text))
            {
                MessageBoxResult ret = MessageBox.Show("保存旧文档吗?", "保存文档", MessageBoxButton.YesNo);
                if (ret == MessageBoxResult.Yes)
                    SaveToFile(true);
            }

            range.Text = "";
            CurFileName = "无标题";
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }


        /// <summary>
        /// 处理打开文档或插入文档
        /// </summary>
        /// <param name="IsInsertFile"></param>
        public void LoadOrInsertFile(bool IsInsertFile, string FileName)
        {
            
            try
            {
                if (File.Exists(FileName) == false)
                    return;

                string FileExt = System.IO.Path.GetExtension(FileName).ToUpper(); //文件扩展名

                TextRange range = null;
                if (IsInsertFile)
                    range = _rtf.Selection;
                else
                {
                    range = new TextRange(_rtf.Document.ContentStart, _rtf.Document.ContentEnd);
                    //用户输入了些东西
                    if (MySuperEditorHelper.IsPrintableString(range.Text))
                    {
                        MessageBoxResult ret = MessageBox.Show("保存旧文档吗?", "保存文档", MessageBoxButton.YesNo);
                        if (ret == MessageBoxResult.Yes)
                            SaveToFile(true);
                    }
                    CurFileName = FileName;
                    
                }
                //打开文件
                using (FileStream fs = new FileStream(FileName, FileMode.Open))
                {
                    switch (FileExt)
                    {
                        case ".XAMLPACKAGE":
                            range.Load(fs, DataFormats.XamlPackage);
                            break;
                        case ".XAML":
                            range.Load(fs, DataFormats.Xaml);
                            break;
                        case ".RTF":
                            range.Load(fs, DataFormats.Rtf);
                            break;
                        case ".TXT":
                        default:
                            //由于TextRange对象不能直接装入Unicode字符，所以，采用其他方法进行处理
                            range.Text = MySuperEditorHelper.LoadStringFromTextFile(fs);
                            break;

                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region "项目列表"

        /// <summary>
        /// 获取所选择部分文本所对应的List元素
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public List FindListAncestor(DependencyObject element)
        {
            while (element != null)
            {
                List list = element as List;
                if (list != null)
                {
                    return list;
                }

                element = LogicalTreeHelper.GetParent(element);
            }

            return null;
        }

        /// <summary>
        /// 获取选中部分的项目符号
        /// </summary>
        /// <returns></returns>
        public TextMarkerStyle GetSelectionListType()
        {
            List list = FindListAncestor(_rtf.Selection.Start.Parent);
            if (list == null)
                return TextMarkerStyle.None;
            else
                return list.MarkerStyle;
        }
        #endregion

        #region "XAML功能"
        /// <summary>
        /// 根据XAML刷新RichTextBox的内容(是整个文档)
        /// </summary>
        public void RefreshRichTextBoxFromXAML(string XAML)
        {
            MemoryStream mem = new MemoryStream();
            StreamWriter sw = new StreamWriter(mem);
            sw.Write(XAML);
            sw.Flush();
            mem.Seek(0, SeekOrigin.Begin);
            Object obj = XamlReader.Load(mem);
            _rtf.Document = obj as FlowDocument;
            sw.Close();
        }

        /// <summary>
        /// 获取RichTextBox所显示的完整文档所对应的XAML
        /// </summary>
        /// <returns></returns>
        public string GetFlowDocumentXAML()
        {
            
            //缩进XAML
            string IndentXAML = XAMLHelper.IndentXaml(XamlWriter.Save(_rtf.Document));
            return IndentXAML;
        }

        /// <summary>
        /// 获取当前选中部分的XAML，如果没选中，返回空串
        /// </summary>
        /// <returns></returns>
        public string GetSelectionXAML()
        {
            if (_rtf.Selection.IsEmpty)
                return "";
            return XAMLHelper.TextRange_GetXml(_rtf.Selection);

        }

        /// <summary>
        /// 将当前选中的内容替换为XAML,如果没选中，不做任何事
        /// </summary>
        /// <param name="XAML"></param>
        public void ReplaceSelectionWithXAML(string XAML)
        {
            if (_rtf.Selection.IsEmpty)
                return ;
            XAMLHelper.TextRange_SetXml(_rtf.Selection, XAML);
        }
        #endregion

        #region "上标和下标等"

        public void SetSelectionBaselineAlignment(BaselineAlignment value)
        {
            _rtf.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, value);
        }

        public BaselineAlignment GetSelectionBaselineAlignment()
        {
            object obj = _rtf.Selection.GetPropertyValue(Inline.BaselineAlignmentProperty);
            if (obj != null)
                return (BaselineAlignment)obj;
            else
                return BaselineAlignment.Baseline;
            
        }
        #endregion

        #region "处理字符指针"
        /// <summary>
        /// 获取RichTextBox中光标最近的一个字符(向前)
        /// </summary>
        /// <param name="rtf"></param>
        /// <param name="curLoc"></param>
        /// <returns></returns>
        public TextRange GetPrevChar(TextPointer curLoc)
        {

            //文字指针向前移一个字符
            TextPointer nextLoc = curLoc.GetPositionAtOffset(-1);
            if (nextLoc == null) //找不到对应位置
                return null;

            //现在可获取插入光标最近一个字符
            TextRange nearestChar = new TextRange(curLoc, nextLoc);

            return nearestChar;
        }
        /// <summary>
        /// 获取RichTextBox中光标最近的一个字符(向后)
        /// </summary>
        /// <param name="rtf"></param>
        /// <param name="curLoc"></param>
        /// <returns></returns>
        public TextRange GetNextChar(TextPointer curLoc)
        {
            //文字指针向前移一个字符
            TextPointer nextLoc = curLoc.GetPositionAtOffset(1);
            if (nextLoc == null) //找不到对应位置
                return null;

            //现在可获取插入光标最近一个字符
            TextRange nearestChar = new TextRange(curLoc, nextLoc);

            return nearestChar;
        }

        #endregion


        #region "插入图片"
        /// <summary>
        /// 向RichTextBox中插入图片
        /// </summary>
        public void InsertImageToRichTextBox()
        {
            if (OpenFileDialogForImageFile.ShowDialog() == WinForms.DialogResult.OK)
            {
                string FileName = OpenFileDialogForImageFile.FileName;
                // Create an image object, in this case from a file.
                System.Drawing.Image image = System.Drawing.Image.FromFile(FileName);

                // 备份剪贴板的数据
                IDataObject data = Clipboard.GetDataObject();

                //Object clipBoardContents = null;
                //获取剪贴板上的数据类型
                string theFormat = string.Empty;
                string[] formats = data.GetFormats(false);
                Dictionary<string, object> datas = new Dictionary<string, object>();
                //备份数据
                foreach (string format in formats)
                {
                    object datavalue = data.GetData(format);

                    if (datavalue != null)
                    {
                        datas.Add(format, datavalue);
                    }
                }

                // 将图片放置到剪贴板上
                WinForms.Clipboard.SetImage(image);

                // 插入到RichTextBox中
                _rtf.Paste();
                //将其设置为左对齐
                EditingCommands.AlignLeft.Execute(null, _rtf);
                // 清空剪贴板
                Clipboard.Clear();

                // 还原原始剪贴板数据

                IDataObject newData = new DataObject();
                foreach (string format in formats)
                {

                    object o = datas[format];
                    newData.SetData(format, o);
                }
                WinForms.Clipboard.SetDataObject(newData);
            }
        }
        #endregion
    }
}
