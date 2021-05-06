﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AForge.Imaging;
using System.Linq;

namespace EmediaWPF
{
    public partial class MainWindow : Window
    {
        private string imageName;
        private ComplexImage complexImage;
        private const string clearFilePath = "../../";
        private bool Debug = true;

        public MainWindow()
        {
            InitializeComponent();
            ConsoleAllocator.ShowConsoleWindow();
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            TemporaryRSATest();

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Image"; // Default file name
            dlg.DefaultExt = ".png"; // Default file extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                imageName = dlg.FileName;
                var png = new PngParser(dlg.FileName);

                if (!Debug)
                {
                    Console.Clear();
                    png.PrintChunks();
                }

                var uri = new Uri(dlg.FileName);
                var image = new BitmapImage(uri);
                MainImage.Source = image;
                FourierImage.Source = FFT.FastFourierTransform(image, out complexImage);
            }

            PhaseFFTButton.IsEnabled = true;
            BackwardFFTButton.IsEnabled = true;
        }

        private void ClearFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                imageName = dlg.FileName;
                var png = new PngParser(dlg.FileName);

                if (!Debug)
                {
                    Console.Clear();
                    png.PrintChunks();
                }
                DirectoryInfo di = Directory.CreateDirectory(clearFilePath + "output");

                //png.SaveCriticalOnly(clearFilePath, "test.png");
                //lub
                png.SaveWithoutMetadata(clearFilePath + "output/", "Clear_" + System.IO.Path.GetFileName(dlg.FileName));
                var uri = new Uri(dlg.FileName);
                var image = new BitmapImage(uri);
                MainImage.Source = image;
                FourierImage.Source = FFT.FastFourierTransform(image);

                if (!Debug)
                    Console.WriteLine("Clear file saved at \"output\" directory!");
            }
        }

        private void BackwardFourierTransform_Click(object sender, RoutedEventArgs e)
        {
            FourierImage.Source = FFT.BackwardFourierTransform(complexImage);
        }

        private void FourierTransformPhase_Click(object sender, RoutedEventArgs e)
        {
            FourierImage.Source = FFT.FromFourierToPhase(complexImage, imageName);
        }

        private void TemporaryRSATest()
        {
            RandomNumberGenerator randomNumberGenerator = new RandomNumberGenerator();
            Random rng = new Random();
            for (int i = 0; i < 1000000; i++)
            {
                DataEncryption dataEncryption = new DataEncryption();
                byte[] data = randomNumberGenerator.GenerateRandomBytes(rng.Next(1, 15));
                var enc = dataEncryption.EncryptData(data);
                var dec = dataEncryption.DecryptData(enc);

                if (!data.SequenceEqual(dec))
                {
                    Console.WriteLine("Błąd dla i : " + i);
                    Console.WriteLine("Data: " + string.Join(" ", data));
                    Console.WriteLine("Enc: " + string.Join(" ", enc));
                    Console.WriteLine("Dec: " + string.Join(" ", dec));
                }
                else if (i % 100 == 0)
                    Console.WriteLine("Jesteśmy na i: " + i);
            }
        }
    }
}
