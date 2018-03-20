using Net.Sgoliver.NRtfTree.Core;
using Net.Sgoliver.NRtfTree.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Composing RTF content. Using https://www.safaribooksonline.com/library/view/rtf-pocket-guide/9781449302047/ch01.html as reference
/// </summary>

namespace CoreSampleAnnotation.Reports.RTF
{
    public enum TextAlignement { Left, Centered }

    public class TextCell {
        public string Text { get; private set; }

        public TextAlignement TextAlignement { get; private set; }
        public bool IsBold { get; private set; }

        /// <summary>
        /// In Twips
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// In Twips
        /// </summary>
        public int BorderWidth { get; private set; }

        /// <param name="width">Twips</param>        
        public TextCell(string text, int width, int borderWidth = 35, TextAlignement horizontalAlignement = TextAlignement.Left, bool isBold = false) {
            Text = text;
            TextAlignement = horizontalAlignement;
            IsBold = isBold;
            Width = width;
            BorderWidth = borderWidth;
        }
    }

    /// <summary>
    /// Represents a single row
    /// </summary>
    public class ReportRow {
        public TextCell[] Cells { get; private set; }
        public ReportRow(TextCell[] cells) {
            this.Cells = cells;
        }
    }

    /// <summary>
    /// Table as an array of row
    /// </summary>
    public class ReportTable {
        public ReportRow[] Rows { get; private set; }
        
        public ReportTable(ReportRow[] rows) {
            Rows = rows;
        }
    }

    public static class Extensions {

        /// <param name="node">Where to append (add) results</param>        
        public static void AddKeyword(this RtfTreeNode node, string value, int parameter)
        {
            node.AppendChild(new RtfTreeNode(RtfNodeType.Keyword, value, true, parameter));
        }

        /// <param name="node">Where to append (add) results</param>        
        public static void AddKeyword(this RtfTreeNode node, string value)
        {
            node.AppendChild(new RtfTreeNode(RtfNodeType.Keyword, value, false, 0));
        }

        /// <param name="node">Where to append (add) results</param>        
        public static void AddCommand(this RtfTreeNode node, string value)
        {
            node.AppendChild(new RtfTreeNode(RtfNodeType.Control, value, false, 0));
        }

        /// <param name="node">Where to append (add) results</param>        
        public static void AddText(this RtfTreeNode node, string text) {
            foreach (char c in text)
            {
                uint cCode = (uint)c;
                if (cCode <= 225)
                {
                    node.AddKeyword(String.Format("\'{0:X}", cCode));
                }
                else if (cCode <= 32768)
                {
                    node.AddKeyword("uc", 1);
                    node.AddKeyword(String.Format("u{0}*", cCode));
                }
                else if (cCode <= 65535)
                {
                    node.AddKeyword("uc", 1);
                    node.AddKeyword(String.Format("u{0}*", cCode - 65536));
                }
                else
                    throw new NotSupportedException("Char code more than 16535 is not supported");
            }
        }

        /// <summary>
        /// Adds keywords that define single border
        /// </summary>
        /// <param name="width">border width in Twips</param>
        public static void AddSingleBorder(this RtfTreeNode node, int width)
        {
            node.AddKeyword("clbrdrt");
            node.AddKeyword("brdrw", width);
            node.AddKeyword("brdrs");

            node.AddKeyword("clbrdrl");
            node.AddKeyword("brdrw", width);
            node.AddKeyword("brdrs");

            node.AddKeyword("clbrdrb");
            node.AddKeyword("brdrw", width);
            node.AddKeyword("brdrs");

            node.AddKeyword("clbrdrr");
            node.AddKeyword("brdrw", width);
            node.AddKeyword("brdrs");
        }

        /// <summary>
        /// Appends the definition of single table row
        /// </summary>
        /// <param name="node">Where to append the rtf elemenets (consider this as output)</param>
        /// <param name="cells">the definitions of the cells</param>        
        public static void AddTableRow(this RtfTreeNode node, TextCell[] cells) {
            node.AddKeyword("trowd");
            node.AddKeyword("trgaph",400);

            int borderOffset = 0;

            foreach (var cell in cells)
            {
                borderOffset += cell.Width;
                node.AddSingleBorder(cell.BorderWidth);
                node.AddKeyword("cellx", borderOffset);
            }
            foreach (var cell in cells)
            {
                node.AddKeyword("pard");

                switch (cell.TextAlignement) {
                    case TextAlignement.Centered:
                        node.AddCommand("qc"); break;
                    case TextAlignement.Left:
                        node.AddCommand("ql"); break;
                    default:
                        throw new NotSupportedException("Unexpected alignment");
                }
                node.AddKeyword("intbl");
                node.AddText(cell.Text);
                node.AddKeyword("cell");
            }
            node.AddKeyword("row");
        }
    }

    
    public static class Report
    {        
        

        public static void FormRTFDocument(ReportTable table) {
            RtfTree tree = new RtfTree();

            string rtfBase = @"{\rtf1\ansi\deff0 {\fonttbl {\f0  Times New Roman;}}\fs32";
            tree.LoadRtfText(rtfBase);

            //Load an RTF document from a file
            RtfTreeNode grp = new RtfTreeNode(RtfNodeType.Group);

            foreach (ReportRow row in table.Rows)
            {
                grp.AddTableRow(row.Cells);
            }
            
            tree.RootNode.FirstChild.AppendChild(grp);

            tree.SaveRtf("test.rtf");
            
            //Get and print RTF code
            //Console.Write(tree.ToStringEx());
        }
    }
}

