using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
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
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

namespace CardReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Ability> _abilities = new List<Ability>
        {
            new Ability("Name", new SixLabors.ImageSharp.Rectangle(300, 1150, 900, 120)),
            new Ability("Type", new SixLabors.ImageSharp.Rectangle(320, 1300, 800, 100)),
            new Ability("Ability", new SixLabors.ImageSharp.Rectangle(170, 1430, 1200, 350)),
        };

        private SixLabors.ImageSharp.Image _image;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".png"; // Required file extension

            var result = fileDialog.ShowDialog();

            if (result.HasValue)
            {
                //SWU- 428x589, crop to 75, 35, 325, 25
                _image = SixLabors.ImageSharp.Image.Load(fileDialog.FileName);

                var values = new List<string>();
                foreach(var ability in _abilities)
                {
                    var image = SixLabors.ImageSharp.Image.Load(fileDialog.FileName);
                    image.Mutate(it => it.Resize(new SixLabors.ImageSharp.Size(1488, 2079))
                    .Grayscale()
                    .Crop(ability.Location));

                    using (var engine = new TesseractEngine(@"./data", "eng", EngineMode.Default))
                    {
                        using var stream = new MemoryStream();
                        image.SaveAsPng(stream);
                        stream.Position = 0;

                        using (var img = Pix.LoadFromMemory(stream.ToArray()))
                        {
                            using (var page = engine.Process(img))
                            {
                                var test = page.GetBoxText(0);
                                var test2 = page.GetWordStrBoxText(0);
                                var text = page.GetText();

                                values.Add(text);
                                
                                //TestLabel.Content = text;
                            }
                        }
                    }
                }

                ListBox.ItemsSource = values;
                ListBox.MouseDoubleClick += ShowImage;
            }
        }

        private void ShowImage(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            var ability = _abilities[listBox.SelectedIndex];

            _image.Mutate(it => it.Resize(new SixLabors.ImageSharp.Size(1488, 2079))
            .Grayscale()
            .Crop(ability.Location));

            using (var stream = new MemoryStream())
            {
                _image.SaveAsJpeg(stream);
                stream.Position = 0;
                TestImage.Source = BitmapFrame.Create(
                    stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }
    }
}