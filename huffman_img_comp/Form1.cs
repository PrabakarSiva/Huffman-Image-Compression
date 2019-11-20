using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace huffman_img_comp
{
    public partial class Form1 : Form
    {
        bool vdata;
        string path;
        Image image;
        Thread ImageThread;
        byte[] bytes;
        int[] intRepBytes;
        string filePathEncoded;
        string filePathTree;
        public Form1()
        {
            InitializeComponent();
            
        }

        
        private void label1_Click(object sender, EventArgs e)
        {
            // Prompt to get file names in order to save encoded data to a file
            string promptValue = Prompt.ShowDialog("Please enter the name you would like to use for the generated text file.", "Filename Prompt");
            string textfilePath = Path.Combine(Environment.CurrentDirectory, @"Data\", promptValue);

            // Build the Huffman tree
            HuffmanTree huffmanTree = new HuffmanTree();
            huffmanTree.Build(intRepBytes);

            // Encode
            BitArray encoded = huffmanTree.Encode(intRepBytes);

            // Saving to file and printing out for console output
            byte[] encodedData = new byte[encoded.Length / 8 + (encoded.Length % 8 == 0 ? 0 : 1)];
            encoded.CopyTo(encodedData, 0);
            File.WriteAllBytes(textfilePath + ".bin", encodedData);
            BitArray test = new BitArray(encodedData);

            Console.Write("Encoded Bistream: ");
            foreach (bool bit in encoded)
            {
                Console.Write((bit ? 1 : 0) + "");
            }
            Console.WriteLine();
            Console.WriteLine("Encoded bitstream is " + encoded.Length + " bits long.");

            // Saving the tree so it can be decoded later 
            promptValue += "Tree";
            textfilePath = Path.Combine(Environment.CurrentDirectory, @"Data\", promptValue);
            File.WriteAllBytes(textfilePath + ".bin", bytes);

            

        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            vdata = GetImage(out filename, e);
            if (vdata)
            {
                path = filename;
                ImageThread = new Thread(new ThreadStart(saveimage));
                ImageThread.Start();
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void saveimage()
        {
            //throw new NotImplementedException();
            image = new Bitmap(path);
        }

        private bool GetImage(out string filename, DragEventArgs e)
        {
            //throw new NotImplementedException();
            bool rtun = false;
            filename = string.Empty;
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileDrop") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is string))
                    {
                        filename = ((string[])data)[0];
                        string extension = Path.GetExtension(filename).ToLower();
                        if ((extension == ".jpg") || (extension == ".png") || (extension == ".gif") || (extension == ".bmp"))
                        {
                            rtun = true;
                        }
                    }
                }
            }
            return rtun;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            // Displaying the image on the panel and returning a bitmapped byte stream of the image
            if (vdata)
            {
                while (ImageThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(0);
                }
                panel1.BackgroundImage = image;
                var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                bytes = ms.ToArray();
                intRepBytes = new int[bytes.Length];
                Console.Write("Byte-Representation of Image: ");
                for (int i = 0; i < bytes.Length; i++)
                {
                    int x = 0;
                    Int32.TryParse(bytes.GetValue(i).ToString(), out x);
                    intRepBytes.SetValue(x, i);
                    Console.Write(intRepBytes.GetValue(i) + " ");
                }
                Console.WriteLine();
                Console.WriteLine("Total number of bytes in the image: " + bytes.Length);
                Console.WriteLine("Total number of bits in the image: " + bytes.Length * 8);

            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

            byte[] encodedBytes = File.ReadAllBytes(filePathEncoded);
            BitArray encodedBits = new BitArray(encodedBytes);

            foreach (bool bit in encodedBits)
            {
                Console.Write((bit ? 1 : 0) + "");
            }
            Console.WriteLine();
            Console.WriteLine("Encoded bitstream is " + encodedBits.Length + " bits long.");

            var treeBytes = File.ReadAllBytes(filePathTree);
            var bytesInInt = new int[treeBytes.Length];
            Console.Write("Byte stream used to build tree: ");
            for (int a = 0; a < treeBytes.Length; a++)
            {
                int y = 0;
                Int32.TryParse(treeBytes.GetValue(a).ToString(), out y);
                bytesInInt.SetValue(y, a);
                Console.Write(bytesInInt.GetValue(a) + " ");
            }

            HuffmanTree decodeTree = new HuffmanTree();
            decodeTree.Build(bytesInInt);
            string decoded = decodeTree.Decode(encodedBits);

            int[] decodeArray = new int[decoded.Split().Length - 1];
            String temp = "";
            int counter = 0;
            foreach (char c in decoded)
            {
                if (Char.IsWhiteSpace(c))
                {
                    int placeholder = 0;
                    Int32.TryParse(temp, out placeholder);
                    decodeArray.SetValue(placeholder, counter);
                    counter++;
                    temp = "";
                }
                else
                {
                    temp += c;
                }
            }
            foreach (var item in decodeArray)
            {
                Console.Write(item.ToString() + " ");
            }
            Console.WriteLine();
            byte[] bytes2 = decodeArray.Select(i => (byte)i).ToArray();
            Console.WriteLine(bytes2.Length);
            var imageMemoryStream = new MemoryStream(bytes2);
            Image imgFromStream = Image.FromStream(imageMemoryStream);
            pictureBox1.Image = imgFromStream;
            
        }
        private bool GetBin(out string filename, DragEventArgs e)
        {
            //throw new NotImplementedException();
            bool rtun = false;
            filename = string.Empty;
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileDrop") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is string))
                    {
                        filename = ((string[])data)[0];
                        string extension = Path.GetExtension(filename).ToLower();
                        if (extension == ".bin")
                        {
                            rtun = true;
                        }
                    }
                }
            }
            return rtun;
        }

        private void panel2_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            vdata = GetBin(out filename, e);
            if (vdata)
            {
                filePathEncoded = filename;
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void panel2_DragDrop(object sender, DragEventArgs e)
        {
            panel2.BackColor = Color.MediumSpringGreen;
            
        }

        private void panel3_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            vdata = GetBin(out filename, e);
            if (vdata)
            {
                filePathTree = filename;
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void panel3_DragDrop(object sender, DragEventArgs e)
        {
            panel3.BackColor = Color.MediumSpringGreen;
        }

    }


    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Width = 400, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 40, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
