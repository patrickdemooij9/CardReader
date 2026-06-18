using CardReader.models;
using Microsoft.Win32;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        private int fileNameIndex = 0;
        private string[] fileNames = [];

        private SixLabors.ImageSharp.Image _image;

        private string _imageBase64 = string.Empty;
        private List<Dictionary<string, string>> _data = new List<Dictionary<string, string>>();

        private Dictionary<string, string>? _prefetchedResult = null;
        private int _prefetchedIndex = -1;
        private bool _prefetchedIsDoubleSided = false;

        private bool _currentCardIsDoubleSided = false;
        private string _backImageBase64 = string.Empty;

        private int? _selectedPresetId = null;
        private int _currentMode = -1;

        private string _gameName = "StarWarsUnlimited";

        private void ShowLoading(string status = "Processing with AI...")
        {
            Dispatcher.Invoke(() =>
            {
                LoadingStatusText.Text = status;
                LoadingIndicator.Visibility = Visibility.Visible;
            });
        }

        private void HideLoading()
        {
            Dispatcher.Invoke(() =>
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
            });
        }

        public MainWindow()
        {
            InitializeComponent();

            ModeSelectorBox.Items.Add(new ListBoxItem() { Content = "AI Read", Tag = 0 });
            ModeSelectorBox.Items.Add(new ListBoxItem() { Content = "Update image", Tag = 1 });
            ModeSelectorBox.Items.Add(new ListBoxItem() { Content = "Set variants", Tag = 2 });

            var config = GetConfig();
            foreach (var preset in config.Presets)
            {
                PresetComboBox.Items.Add(new ListBoxItem { Content = preset.Name, Tag = preset.Id });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.DefaultExt = ".png"; // Required file extension

            var result = fileDialog.ShowDialog();

            if (result.HasValue)
            {
                fileNameIndex = 0;
                fileNames = fileDialog.FileNames;

                HandleCurrentImage();

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

        private void HandleCurrentImage()
        {
            AddLatestDataToList();

            var currentImageFile = fileNames[fileNameIndex];
            using (var stream = new MemoryStream(File.ReadAllBytes(currentImageFile)))
            {
                stream.Position = 0;
                TestImage.Source = BitmapFrame.Create(
                    stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                FileNameLabel.Content = System.IO.Path.GetFileName(currentImageFile);
            }
            _imageBase64 = Convert.ToBase64String(File.ReadAllBytes(currentImageFile));

            // Update current mode
            _currentMode = (int?)((ModeSelectorBox.SelectedValue as ListBoxItem)?.Tag) ?? -1;

            // Display preset data if a preset is selected
            if (_selectedPresetId.HasValue)
            {
                var config = GetConfig();
                var preset = config.Presets.FirstOrDefault(it => it.Id == _selectedPresetId.Value);
                if (preset != null)
                {
                    var presetData = new List<Dictionary<string, string>>();
                    foreach (var variant in preset.Variants)
                    {
                        presetData.Add(variant.Properties);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        JsonExample.Text = JsonSerializer.Serialize(presetData, new JsonSerializerOptions { WriteIndented = true });
                    });

                    // In Mode 2 (Set variants), automatically search for the card based on AI read
                    if (_currentMode == 2)
                    {
                        var presetDataJson = JsonExample.Text;
                        _ = Task.Run(async () =>
                        {
                            await AutoSearchCardForPreset(currentImageFile, presetDataJson);
                        });
                    }
                }
                return;
            }

            var selectedValue = (ModeSelectorBox.SelectedValue as ListBoxItem).Tag;
            string? potentialBackBase64 = fileNameIndex + 1 < fileNames.Length
                ? Convert.ToBase64String(File.ReadAllBytes(fileNames[fileNameIndex + 1]))
                : null;

            // Create a new thread to currently handle the ReadWithAI call as it takes a while and we don't want to block the UI thread.
            _ = Task.Run(async () =>
            {
                if (selectedValue.Equals(0))
                {
                    ShowLoading("Reading image with AI...");
                    try
                    {
                        Dictionary<string, string> result;
                        bool isDoubleSided;

                        // Check if we have a prefetched result for the current image
                        if (_prefetchedResult != null && _prefetchedIndex == fileNameIndex)
                        {
                            result = _prefetchedResult;
                            isDoubleSided = _prefetchedIsDoubleSided;
                            _prefetchedResult = null;
                            _prefetchedIndex = -1;
                            _prefetchedIsDoubleSided = false;
                        }
                        else
                        {
                            (result, isDoubleSided) = await ReadWithAI(_imageBase64, potentialBackBase64);
                        }

                        _currentCardIsDoubleSided = isDoubleSided;
                        if (isDoubleSided)
                        {
                            _backImageBase64 = potentialBackBase64!;
                            var backImageFile = fileNames[fileNameIndex + 1];
                            Dispatcher.Invoke(() =>
                            {
                                using var stream = new MemoryStream(File.ReadAllBytes(backImageFile));
                                stream.Position = 0;
                                Image2.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            });
                        }
                        else
                        {
                            _backImageBase64 = string.Empty;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            JsonExample.Text = JsonSerializer.Serialize(new[] { result }, new JsonSerializerOptions { WriteIndented = true });

                            int nextFrontIndex = fileNameIndex + (isDoubleSided ? 2 : 1);
                            if (nextFrontIndex < fileNames.Length)
                            {
                                _ = PrefetchNextImage(nextFrontIndex);
                            }
                        });
                    }
                    finally
                    {
                        HideLoading();
                    }
                }
                /*else
                {
                    // Even in non-AI modes, prefetch for when we switch to AI mode
                    if (fileNameIndex < fileNames.Length - 1)
                    {
                        _ = PrefetchNextImage();
                    }
                }*/
            });
        }

        private async Task AutoSearchCardForPreset(string imagePath, string presetDataJson)
        {
            try
            {
                ShowLoading("Reading image with AI...");

                // Get the AI read result to extract name and subname
                var (aiResult, _) = await ReadWithAI(imagePath, Convert.ToBase64String(File.ReadAllBytes(imagePath)));

                // Extract name - use "Name" or similar field that exists in your AI result
                if (!aiResult.TryGetValue("Name", out var searchQuery) || string.IsNullOrWhiteSpace(searchQuery))
                {
                    HideLoading();
                    return;
                }

                // Optionally append subname if it exists
                if (aiResult.TryGetValue("Subname", out var subname) && !string.IsNullOrWhiteSpace(subname))
                {
                    searchQuery = $"{searchQuery} {subname}";
                }

                searchQuery = searchQuery.Trim().ToLower();

                ShowLoading("Searching database...");

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.PostAsJsonAsync<SearchPostModel>("https://sw-unlimited-db.com/api/cards/query", new SearchPostModel
                {
                    PageNumber = 1,
                    PageSize = 10,
                    Query = searchQuery
                });
                var result = await response.Content.ReadFromJsonAsync<SearchResponseModel>();

                if (result?.Items?.Length == 1)
                {
                    var firstResult = result.Items[0];
                    var data = new List<Dictionary<string, string>>();

                    var currentData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(presetDataJson) ?? [];
                    data = currentData;

                    foreach (var entry in data)
                    {
                        entry["ParentId"] = firstResult.BaseId.ToString();
                    }

                    Dispatcher.Invoke(() =>
                    {
                        JsonExample.Text = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    });
                }
            }
            catch
            {
                // If auto-search fails, just ignore it - the user can still manually search
            }
            finally
            {
                HideLoading();
            }
        }

        private async Task PrefetchNextImage(int nextFrontIndex)
        {
            try
            {
                if (nextFrontIndex < fileNames.Length)
                {
                    var nextImageFile = fileNames[nextFrontIndex];
                    var nextImageBase64 = Convert.ToBase64String(File.ReadAllBytes(nextImageFile));
                    string? nextBackImageBase64 = nextFrontIndex + 1 < fileNames.Length
                        ? Convert.ToBase64String(File.ReadAllBytes(fileNames[nextFrontIndex + 1]))
                        : null;

                    (_prefetchedResult, _prefetchedIsDoubleSided) = await ReadWithAI(nextImageBase64, nextBackImageBase64);
                    _prefetchedIndex = nextFrontIndex;
                }
            }
            catch
            {
                // If prefetch fails, just ignore it - the user can still manually fetch on the next image
                _prefetchedResult = null;
                _prefetchedIndex = -1;
                _prefetchedIsDoubleSided = false;
            }
        }

        private async Task<(Dictionary<string, string> Result, bool IsDoubleSided)> ReadWithAI(string frontImageBase64, string? backImageBase64 = null)
        {
            var googleAI = new GoogleAI(apiKey: "");
            var model = googleAI.GenerativeModel(model: Model.GeminiFlashLiteLatest);
            var generationConfig = new GenerationConfig()
            {
                ResponseMimeType = "application/json",
            };
            var config = GetConfig();

            var typeRequest = new GenerateContentRequest(config.InitialPrompt);
            typeRequest.AddPart(new InlineData
            {
                Data = frontImageBase64,
                MimeType = "image/png",
            });
            var typeResponse = await model.GenerateContent(typeRequest);

            var typeConfig = config.Types.FirstOrDefault(it => it.Type.Equals(typeResponse?.Text?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (typeConfig is null) throw new Exception("Failed to find type config");

            var prompt = File.ReadAllText($"prompts/{_gameName}/{typeConfig.PromptFile}");
            var propertiesStringBuilder = new StringBuilder();
            foreach (var property in typeConfig.Properties.Where(it => !it.Constant.HasValue))
            {
                propertiesStringBuilder.AppendLine($"- {property.Alias} ({property.Description})");
            }
            prompt = prompt.Replace("{properties}", propertiesStringBuilder.ToString());

            var request = new GenerateContentRequest(prompt);
            request.GenerationConfig = generationConfig;
            request.AddPart(new InlineData
            {
                Data = frontImageBase64,
                MimeType = "image/png",
            });

            var response = await model.GenerateContent(request);
            if (string.IsNullOrWhiteSpace(response?.Text))
            {
                return ([], false);
            }
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Text) ?? new Dictionary<string, string>();
            foreach (var constantValue in typeConfig.Properties.Where(it => it.Constant.HasValue))
            {
                result[constantValue.Alias] = constantValue.Constant!.Value.ToString();
            }
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

            if (typeConfig.IsDoubleSided && backImageBase64 != null && typeConfig.BackSidePromptFile != null && typeConfig.BackSideProperties != null)
            {
                var backPrompt = File.ReadAllText($"prompts/{_gameName}/{typeConfig.BackSidePromptFile}");
                var backPropertiesStringBuilder = new StringBuilder();
                foreach (var property in typeConfig.BackSideProperties.Where(it => !it.Constant.HasValue))
                {
                    backPropertiesStringBuilder.AppendLine($"- {property.Alias} ({property.Description})");
                }
                backPrompt = backPrompt.Replace("{properties}", backPropertiesStringBuilder.ToString());

                var backRequest = new GenerateContentRequest(backPrompt);
                backRequest.GenerationConfig = generationConfig;
                backRequest.AddPart(new InlineData
                {
                    Data = backImageBase64,
                    MimeType = "image/png",
                });

                var backResponse = await model.GenerateContent(backRequest);
                if (!string.IsNullOrWhiteSpace(backResponse?.Text))
                {
                    var backResult = JsonSerializer.Deserialize<Dictionary<string, string>>(backResponse.Text) ?? [];
                    foreach (var constantValue in typeConfig.BackSideProperties.Where(it => it.Constant.HasValue))
                    {
                        backResult[constantValue.Alias] = constantValue.Constant!.Value.ToString();
                    }
                    foreach (var nonCapsKey in typeConfig.BackSideProperties.Where(it => it.ToTitleCase))
                    {
                        if (backResult.TryGetValue(nonCapsKey.Alias, out string? backValue) && !string.IsNullOrWhiteSpace(backValue))
                        {
                            backResult[nonCapsKey.Alias] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(backValue.ToLower());
                        }
                    }
                    foreach (var splitKey in typeConfig.BackSideProperties.Where(it => !string.IsNullOrWhiteSpace(it.Split)))
                    {
                        if (backResult.TryGetValue(splitKey.Alias, out string? backTraits))
                        {
                            backResult[splitKey.Alias] = string.Join(",", backTraits.Split(splitKey.Split).Select(it => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(it.Trim().ToLower())));
                        }
                    }
                    foreach (var kvp in backResult)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                    result["back_image_base64"] = "[image]";
                }
            }

            result.Add("SWU Id", "[todo]");
            result.Add("TTS Id", "[todo]");
            result.Add("image_base64", "[image]");
            return (result, typeConfig.IsDoubleSided && backImageBase64 != null);
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            AddLatestDataToList();

            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = $"cards{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.json"
            };
            var result = saveFileDialog.ShowDialog();
            if (result.HasValue && !string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                var basePath = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                var baseFileName = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                var fileExtension = System.IO.Path.GetExtension(saveFileDialog.FileName);
                
                const int itemsPerFile = 5;
                var totalChunks = (int)Math.Ceiling((double)_data.Count / itemsPerFile);
                
                for (int i = 0; i < totalChunks; i++)
                {
                    var chunk = _data.Skip(i * itemsPerFile).Take(itemsPerFile).ToList();
                    var fileName = System.IO.Path.Combine(basePath, $"{baseFileName}_{i + 1}{fileExtension}");
                    File.WriteAllText(fileName, JsonSerializer.Serialize(chunk, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
        }

        private void AddLatestDataToList()
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>[]>(JsonExample.Text) ?? [];
                foreach (var entry in data)
                {
                    if (entry.ContainsKey("image_base64"))
                    {
                        entry["image_base64"] = _imageBase64;
                    }
                    if (entry.ContainsKey("back_image_base64"))
                    {
                        entry["back_image_base64"] = _backImageBase64;
                    }

                    _data.Add(entry);
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

        private void NextImageBtn_Click(object sender, RoutedEventArgs e)
        {
            int advance = _currentCardIsDoubleSided ? 2 : 1;
            if (fileNameIndex + advance < fileNames.Length)
            {
                fileNameIndex += advance;
                HandleCurrentImage();
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            var httpClient = new HttpClient();
            var query = SearchTxt.Text.Trim().ToLower();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var selectedMode = (ModeSelectorBox.SelectedValue as ListBoxItem).Tag;
            var currentData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(JsonExample.Text) ?? [];
            _ = Task.Run(async () =>
            {
                var response = await httpClient.PostAsJsonAsync<SearchPostModel>("https://api.sw-unlimited-db.com/api/cards/query", new SearchPostModel
                {
                    PageNumber = 1,
                    PageSize = 10,
                    Query = query,
                    VariantTypeIds = [0]
                });
                var result = await response.Content.ReadFromJsonAsync<SearchResponseModel>();
                if (result.Items.Length > 1)
                {
                    MessageBox.Show("Multiple results found, please refine your search.");
                    return;
                }
                if (result.Items.Length == 0)
                {
                    MessageBox.Show("No results found.");
                    return;
                }
                var firstResult = result.Items[0];

                var data = new List<Dictionary<string, string>>();
                byte[]? imageBytes = null;
                if (selectedMode.Equals(2))
                {
                    data = currentData;

                    foreach(var entry in data)
                    {
                        entry.Add("ParentId", firstResult.BaseId.ToString());
                    }
                }
                else
                {
                    var entry = new Dictionary<string, string>()
                    {
                        //{ "Id", firstResult.VariantId.ToString() },
                        { "Id", firstResult.BaseId.ToString() },
                        { "Name", firstResult.DisplayName },
                        { "image_base64", "[image]" }
                    };

                    foreach (var attribute in firstResult.Attributes)
                    {
                        var key = attribute.Value.Length > 1 ? $"{attribute.Key}_multiple" : attribute.Key;
                        entry.Add(key, string.Join(",", attribute.Value));
                    }

                    data.Add(entry);

                    imageBytes = await httpClient.GetByteArrayAsync(firstResult.ImageUrl.ImageUrl);
                }

                Dispatcher.Invoke(() =>
                {
                    JsonExample.Text = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

                    if (imageBytes != null)
                    {
                        using (var stream = new MemoryStream(imageBytes))
                        {
                            stream.Position = 0;
                            Image2.Source = BitmapFrame.Create(
                                stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        }
                    }
                });
            });
        }

        private AIConfigModel GetConfig()
        {
            var configJson = File.ReadAllText($"prompts/{_gameName}/Config.json");
            return JsonSerializer.Deserialize<AIConfigModel>(configJson) ?? throw new Exception("Failed to load config");
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var config = GetConfig();

            var listBoxItem = e.AddedItems.Cast<ListBoxItem>().FirstOrDefault();
            if (listBoxItem != null)
            {
                var presetId = (int?)listBoxItem.Tag;
                if (presetId.HasValue)
                {
                    _selectedPresetId = presetId.Value;
                }
            }
            else
            {
                // If nothing is selected, clear the preset
                _selectedPresetId = null;
            }

            // Update JsonExample immediately if a card is already loaded
            if (fileNames.Length > 0 && _selectedPresetId.HasValue)
            {
                var preset = config.Presets.FirstOrDefault(it => it.Id == _selectedPresetId.Value);
                if (preset != null)
                {
                    var presetData = new List<Dictionary<string, string>>();
                    foreach (var variant in preset.Variants)
                    {
                        presetData.Add(variant.Properties);
                    }
                    JsonExample.Text = JsonSerializer.Serialize(presetData, new JsonSerializerOptions { WriteIndented = true });
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await ReadWithAI(_imageBase64, _backImageBase64);
            });
        }
    }
}