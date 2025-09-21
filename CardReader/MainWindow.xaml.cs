using CardReader.models;
using Microsoft.Win32;
using Mscc.GenerativeAI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
using Rectangle = SixLabors.ImageSharp.Rectangle;

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

        private string _imageBase64 = string.Empty;
        private List<Dictionary<string, string>> _data = new List<Dictionary<string, string>>();

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
                AddLatestDataToList();
                using (var stream = new MemoryStream(File.ReadAllBytes(fileDialog.FileName)))
                {
                    stream.Position = 0;
                    TestImage.Source = BitmapFrame.Create(
                        stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }

                // Create a new thread to currently handle the ReadWithAI call as it takes a while and we don't want to block the UI thread.
                _ = Task.Run(async () =>
                {
                    var result = await ReadWithAI(fileDialog.FileName);

                    // If you need to update the UI after completion, use Dispatcher.Invoke:
                    Dispatcher.Invoke(() => 
                    {
                        JsonExample.Text = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true});
                    });
                });

                /*
                //SWU- 428x589, crop to 75, 35, 325, 25
                _image = SixLabors.ImageSharp.Image.Load(fileDialog.FileName);
                _image.Mutate(it => it.Resize(_image.Width * 4, _image.Height * 4));
                using var imageStream = new MemoryStream();
                //using var engine = new TesseractEngine(@"./data", "eng", EngineMode.Default);
                _image.SaveAsPng(imageStream);
                imageStream.Position = 0;

                //using var img = Pix.LoadFromMemory(imageStream.ToArray());
                //using var page = engine.Process(img);
                //var test = page.GetText();

                var values = new List<CardAbilityResult>();
                var imageWidth = _image.Width;
                var imageHeight = _image.Height;
                int GetLoc(double value, double fullValue) => (int)(fullValue / 100 * value);

                var abilities = JsonSerializer.Deserialize<CardAbility[]>(File.ReadAllText("configurations/StarWarsUnlimited.json")) ?? [];

                using var engine = new TesseractEngine(@"./data", "eng", EngineMode.Default);
                foreach (var ability in abilities)
                {
                    if (ability.Conditions.Count > 0)
                    {
                        if (!ability.Conditions.Any(entry => entry.Value.Contains(values.FirstOrDefault(it => it.Alias == entry.Key)?.Value)))
                        {
                            continue;
                        }
                    }

                    var xPos1 = GetLoc(ability.PosX1, imageWidth);
                    var xPos2 = GetLoc(ability.PosX2, imageWidth);
                    var yPos1 = GetLoc(ability.PosY1, imageHeight);
                    var yPos2 = GetLoc(ability.PosY2, imageHeight);

                    var location = new Rectangle(xPos1, yPos1, xPos2 - xPos1, yPos2 - yPos1);
                    var croppedLoc = _image.Clone((action) => action.Crop(location));

                    using var stream = new MemoryStream();
                    croppedLoc.SaveAsPng(stream);
                    stream.Position = 0;
                    System.IO.File.WriteAllBytes($"{ability.Alias}.png", stream.ToArray());
                    stream.Position = 0;

                    using var img = Pix.LoadFromMemory(stream.ToArray());
                    using var page = engine.Process(img);

                    var text = page.GetText();
                    if (ability.IgnoreWhitespace)
                    {
                        text = text.Trim().Replace("\n", "");
                    }
                    if (ability.IgnoreSpecialCharacters)
                    {
                        text = text.Replace(".", "").Replace("-", "").Replace(")", "").Replace(",", "");
                    }

                    values.Add(new CardAbilityResult { Alias = ability.Alias, Value = text, Location = location });

                    //TestLabel.Content = text;
                }

                foreach (var value in values)
                {
                    _image.Mutate(it => it.DrawLine(new SolidPen(SixLabors.ImageSharp.Color.BlueViolet, 3),
                        new PointF(value.Location.X, value.Location.Y),
                        new PointF(value.Location.X + value.Location.Width, value.Location.Y),
                        new PointF(value.Location.X + value.Location.Width, value.Location.Y + value.Location.Height),
                        new PointF(value.Location.X, value.Location.Y + value.Location.Height)));
                }
                using (var stream = new MemoryStream())
                {
                    _image.SaveAsJpeg(stream);
                    stream.Position = 0;
                    TestImage.Source = BitmapFrame.Create(
                        stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }*/

                /*var values = new List<string>();
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
                }*/

                /*ListBox.ItemsSource = values.Select(it => $"{it.Alias}:{it.Value}");
                ListBox.MouseDoubleClick += ShowImage;
                ListBox.PreviewMouseRightButtonUp += CopyAbility;*/
            }
        }

        private async Task<Dictionary<string, string>> ReadWithAI(string imagePath)
        {
            _imageBase64 = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            var googleAI = new GoogleAI(apiKey: "AIzaSyDYVjSB6VZ9yBRDPIAF3_K2h9EqUPPDfg8");
            var model = googleAI.GenerativeModel(model: Model.Gemini25Flash);
            var generationConfig = new GenerationConfig()
            {
                ResponseMimeType = "application/json",
            };
            var configJson = File.ReadAllText("prompts/StarWarsUnlimited/Config.json");
            var config = JsonSerializer.Deserialize<AIConfigModel>(configJson) ?? throw new Exception("Failed to load config");

            var typeRequest = new GenerateContentRequest(config.InitialPrompt);
            typeRequest.AddPart(new InlineData
            {
                Data = _imageBase64,
                MimeType = "image/png",
            });
            var typeResponse = await model.GenerateContent(typeRequest);

            var typeConfig = config.Types.FirstOrDefault(it => it.Type.Equals(typeResponse?.Text?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (typeConfig is null) throw new Exception("Failed to find type config");

            var prompt = File.ReadAllText($"prompts/StarWarsUnlimited/{typeConfig.PromptFile}");
            var propertiesStringBuilder = new StringBuilder();
            foreach (var property in typeConfig.Properties)
            {
                propertiesStringBuilder.AppendLine($"- {property.Alias} ({property.Description})");
            }
            prompt = prompt.Replace("{properties}", propertiesStringBuilder.ToString());

            var request = new GenerateContentRequest(prompt);
            request.GenerationConfig = generationConfig;
            request.AddPart(new InlineData
            {
                Data = _imageBase64,
                MimeType = "image/png",
            });

            var response = await model.GenerateContent(request);
            if (string.IsNullOrWhiteSpace(response?.Text))
            {
                return [];
            }
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Text) ?? new Dictionary<string, string>();
            foreach (var nonCapsKey in typeConfig.Properties.Where(it => it.ToTitleCase))
            {
                if (result.TryGetValue(nonCapsKey.Alias, out string? value) && !string.IsNullOrWhiteSpace(value))
                {
                    result[nonCapsKey.Alias] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
                }
            }
            foreach (var splitKey in typeConfig.Properties.Where(it => !string.IsNullOrWhiteSpace(it.Split)))
            {
                if (result.TryGetValue(splitKey.Alias, out string? traits))
                {
                    result[splitKey.Alias] = string.Join(",", traits.Split(splitKey.Split).Select(it => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(it.Trim().ToLower())));
                }
            }
            
            result.Add("Aspects_multiple", "[todo]");
            result.Add("SWU Id", "[todo]");
            result.Add("TTS Id", "[todo]");
            result.Add("Rarity", "[todo]");
            result.Add("image_base64", "[image]");
            return result;
        }

        private void CopyAbility(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            Clipboard.SetText(listBox.SelectedItem.ToString()!.Replace("\n", ""));
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

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            AddLatestDataToList();

            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = "cards.json"
            };
            var result = saveFileDialog.ShowDialog();
            if (result.HasValue && !string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                File.WriteAllText(saveFileDialog.FileName, JsonSerializer.Serialize(_data));
            }
        }

        private void AddLatestDataToList()
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonExample.Text);
                if (data != null)
                {
                    if (data.ContainsKey("image_base64"))
                    {
                        data["image_base64"] = _imageBase64;
                    }

                    _data.Add(data);
                }
                JsonExample.Text = string.Empty;
            }
            catch
            {
                //Ignored
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            _data = new List<Dictionary<string, string>>();
        }
    }
}