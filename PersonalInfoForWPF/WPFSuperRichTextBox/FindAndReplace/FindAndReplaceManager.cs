using System;
using System.Windows;
using System.Windows.Documents;

namespace WPFSuperRichTextBox
{
    /// <summary>
    /// 搜索选项
    /// </summary>
    class FindOptions
    {
        /// <summary>
        /// 区分大小写?
        /// </summary>
        public bool MatchCase = false;
        /// <summary>
        /// 全字匹配?
        /// </summary>
        public bool MatchWholeWord = false;
    }

    /// <summary>
    /// 在FlowDocument中查找与替换.
    /// </summary>
    sealed class FindAndReplaceManager
    {
        /// <summary>
        /// 要处理的FlowDocument
        /// </summary>
        private FlowDocument inputDocument;

        public FindAndReplaceManager(FlowDocument inputDocument)
        {
            if (inputDocument == null)
            {
                throw new ArgumentNullException("需要给构造函数传入一个有效的FlowDocument对象");
            }
            this.inputDocument = inputDocument;
            this.currentPosition = inputDocument.ContentStart;
        }

        private TextPointer currentPosition;
        /// <summary>
        /// 当前文字指针
        /// </summary>
        public TextPointer CurrentPosition
        {
            get
            {
                return currentPosition;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.CompareTo(inputDocument.ContentStart) < 0 || value.CompareTo(inputDocument.ContentEnd) > 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                currentPosition = value;
            }
        }

        /// <summary>
        /// 按照搜索选项查找下一个匹配项
        /// </summary>
        /// <param name="input">要查找的字串</param>
        /// <param name="findOptions">搜索选项</param>
        /// <returns>找到的匹配项</returns>
        /// <remarks>
        /// 此方法将移动文字指针
        /// </remarks>
        public TextRange FindNext(String input, FindOptions findOptions)
        {
            TextRange textRange = GetTextRangeFromPosition(ref currentPosition, input, findOptions);
            return textRange;
        }

        /// <summary>
        /// 根据搜索选项在文档中查找并替换字串
        /// </summary>
        /// <param name="input">要查找的字串</param>
        /// <param name="replacement">用于替换的字串</param>
        /// <param name="findOptions">搜索选项 </param>
        /// <returns>刚刚完成替换工作的匹配项</returns>
        /// <remarks>
        /// 此方法将移动文字指针
        /// </remarks>
        public TextRange Replace(String input, String replacement, FindOptions findOptions)
        {
            TextRange textRange = FindNext(input, findOptions);
            if (textRange != null)
            {
                textRange.Text = replacement;
            }
            return textRange;
        }

        /// <summary>
        /// 根据搜索选项在文档中查找并替换全部匹配字串
        /// </summary>
        /// <param name="input">要查找的字串</param>
        /// <param name="replacement">用于替换的字串</param>
        /// <param name="findOptions">搜索选项 </param>
        /// <returns>进行了替换的次数</returns>
        /// <remarks>
        /// 此方法将移动文字指针到最后位置
        /// </remarks>
        public int ReplaceAll(String input, String replacement, FindOptions findOptions)
        {
            int count = 0;
            currentPosition = inputDocument.ContentStart;
            while (currentPosition.CompareTo(inputDocument.ContentEnd) < 0)
            {
                TextRange textRange = Replace(input, replacement, findOptions);
                if (textRange != null)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Find the corresponding <see cref="TextRange"/> instance 
        /// representing the input string given a specified text pointer position.
        /// </summary>
        /// <param name="position">the current text position</param>
        /// <param name="textToFind">input text</param>
        /// <param name="findOptions">the search option</param>
        /// <returns>An <see cref="TextRange"/> instance represeneting the matching string withing the text container.</returns>
        public TextRange GetTextRangeFromPosition(ref TextPointer position, String input, FindOptions findOptions)
        {
            Boolean matchCase = findOptions.MatchCase;
            Boolean matchWholeWord = findOptions.MatchWholeWord;

            TextRange textRange = null;

            while (position != null)
            {
                if (position.CompareTo(inputDocument.ContentEnd) == 0) //到了文档结尾
                {
                    break;
                }
                //是文本元素
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    String textRun = position.GetTextInRun(LogicalDirection.Forward);  //读取文本
                    StringComparison stringComparison = matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
                  //进行查找
                    Int32 indexInRun = textRun.IndexOf(input, stringComparison);

                    if (indexInRun >= 0)  //找到了
                    {
                        position = position.GetPositionAtOffset(indexInRun);  //移动文字指针到开头
                        TextPointer nextPointer = position.GetPositionAtOffset(input.Length); //设定匹配项尾文字指针
                        textRange = new TextRange(position, nextPointer); 

                        if (matchWholeWord) //是全字匹配的话
                        {
                            if (IsWholeWord(textRange)) // 测试匹配项是否是一个单词
                            {
                                // 是一个完整的单词
                                break;
                            }
                            else
                            {
                                // 找到的不是一个完整的单词,继续在本Run元素中查找
                                position = position.GetPositionAtOffset(input.Length);
                                return GetTextRangeFromPosition(ref position, input, findOptions);
                            }
                        }
                        else
                        {
                            //不要求全字匹配
                            position = position.GetPositionAtOffset(input.Length);
                            break;
                        }
                    }
                    else
                    {
                        // 没找到匹配项,移到当前的Run元素之后 "textRun".
                        position = position.GetPositionAtOffset(textRun.Length);
                    }
                }
                else
                {
                    //如果当前位置不是文本类型的元素,继续"前进",跳过这些非文本元素.
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

            return textRange;
        }

        /// <summary>
        /// 判断是否是一个合法的英文字符:数字,字母或下划线
        /// </summary>
        /// <param name="character">被判断的字符</param>
        /// <returns></returns>
        private Boolean IsWordChar(Char character)
        {
            return Char.IsLetterOrDigit(character) || character == '_';
        }

        /// <summary>
        ///查找指定的textRange中是否为一个完整的单词
        ///这主要是通过检查第一个和最后一个字符是否是有效的英文单词字符决定的.
        /// </summary>
        /// <param name="textRange"><see cref="TextRange"/>instance to test</param>
        /// <returns>test result</returns>
        private Boolean IsWholeWord(TextRange textRange)
        {
            Char[] chars = new Char[1];

            if (textRange.Start.CompareTo(inputDocument.ContentStart) == 0 || textRange.Start.IsAtLineStartPosition)
            {
                textRange.End.GetTextInRun(LogicalDirection.Forward, chars, 0, 1);
                return !IsWordChar(chars[0]);
            }
            else if (textRange.End.CompareTo(inputDocument.ContentEnd) == 0)
            {
                textRange.Start.GetTextInRun(LogicalDirection.Backward, chars, 0, 1);
                return !IsWordChar(chars[0]);
            }
            else
            {
                textRange.End.GetTextInRun(LogicalDirection.Forward, chars, 0, 1);
                if (!IsWordChar(chars[0]))
                {
                    textRange.Start.GetTextInRun(LogicalDirection.Backward, chars, 0, 1);
                    return !IsWordChar(chars[0]);
                }
            }

            return false;
        }
    }
}
