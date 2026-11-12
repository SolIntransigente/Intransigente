using Dazed.Vitals;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Font = Dazed.Vitals.Font;

namespace DazedGUITools.Windows.FontCreation
{
    [MainMenuWindow("Font creation")]
    public class FontCreationWindow() : WindowBaseClass("Font Creation", true)
    {
        //TODO: Add more characters
        private const string DefaultCharacterOrder = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789";

        string _fontName = "fontName";
        string _currentSetTexturePath = string.Empty;
        string _currentSetCharactersDirectory = string.Empty;
        char[] _characterOrder;
        string _manualCharacterOrder = string.Empty;
        int _charWidth, _charHeight;

        int _firstCharX, _firstCharY;
        int _atlasX, _atlasY;
        private bool _validAtlasSize = false;
        private bool? _result;
        private bool _createManual, _gotFileLength, _fontIsUpperCaseOnly;
        private string[] _availableFiles;

        protected override void Draw()
        {
            ImGui.Text("Font creation window");
            ImGui.InputText("Name", ref _fontName, 1024);
            ImGui.Checkbox("Is upper case only", ref _fontIsUpperCaseOnly);

            if (ImGui.Button(_createManual ? "Create automatically" : "Create manually"))
                _createManual = !_createManual;
            ImGui.Separator();
            bool compatibleValues = true;
            if (_createManual)
            {
                ImGui.TextColored(Vector4Colors.Green, "Create with manually assigned values from an already existing texture font.");
                ImGui.Spacing();
                ImGui.Text("Insert all characters present in the font atlas");
                if (ImGui.SmallButton("Default"))
                    _manualCharacterOrder = DefaultCharacterOrder;
                ImGui.SameLine();
                ImGui.InputText("Chars", ref _manualCharacterOrder, 1024);
                ImGui.Spacing();
                ImGui.Spacing();
                ImGuiFile.PathValidMessage(_currentSetTexturePath, out bool pathIsCompatible);
                ImGuiFile.InputFile("Texture path", ref _currentSetTexturePath);
                ImGui.Spacing();

                ImGui.InputInt("Width", ref _charWidth);
                ImGui.InputInt("Height", ref _charHeight);
                
                compatibleValues = pathIsCompatible && _fontName.Length > 0 && _characterOrder.Length > 0 && _charHeight > 0 && _charWidth > 0;
            }
            else
            {
                ImGui.TextColored(Vector4Colors.Blue, "Create automatically with individual characters' textures, compiled then into an atlas.");
                ImGui.Spacing();

                ImGuiFile.PathValidMessage(_currentSetCharactersDirectory, out bool pathIsCompatible);
                if (ImGuiFile.InputDirectory("All characters directory", ref _currentSetCharactersDirectory))
                    _gotFileLength = false;
                if (ImGui.Button("Reload"))
                    _gotFileLength = false;
                if (pathIsCompatible)
                {
                    if (!_gotFileLength)
                    {
                        _availableFiles = Directory.GetFiles(_currentSetCharactersDirectory, "*.png");
                        //Expecting all images to have the same width / height, while (width != height) may be true
                        PathUtils.GetPNGSize(_availableFiles[0], out _firstCharX, out _firstCharY);
                        _gotFileLength = true;

                        _validAtlasSize = false;
                        var maxValue = (int)MathF.Max(_firstCharX, _firstCharY);
                        for (int s = maxValue; s < 4096; s++)
                        {
                            int expectedX = _atlasX;
                            int expectedY = _atlasY;
                            if (s % _firstCharX == 0)
                                _atlasX = expectedX = s;
                            if (s % _firstCharY == 0)
                                _atlasY = expectedY = s;

                            //If amount in X * Amount in Y == the amount of characters, then those are valid values
                            float amountX = (expectedX / (float)_firstCharX);
                            float amountY = (expectedY / (float)_firstCharY);
                            if (amountX * amountY > _availableFiles.Length)
                            {
                                _validAtlasSize = true;
                                break;
                            }
                        }
                    }
                    if(_availableFiles.Length > 0)
                    {
                        ImGui.Separator();
                        ImGui.Text("The following files were found:");
                        for (int i = 0; i < _availableFiles.Length; i++)
                        {
                            ImGui.Text(Path.GetRelativePath(_currentSetCharactersDirectory, _availableFiles[i]));
                            if (i % 2 == 0 && i != _availableFiles.Length - 1)
                            {
                                ImGui.SameLine();
                                ImGui.Text(" | ");
                                ImGui.SameLine();
                            }
                        }
                        ImGui.Separator();
                        ImGui.Text($"There's a total of: '{_availableFiles.Length}' compatible files");
                        ImGui.Text($"The expected size of each texture is: X:<{_firstCharX}> | Y:<{_firstCharY}>");
                        if (_validAtlasSize)
                            ImGui.Text($"The atlas resulting in a size of: X:<{_atlasX}> | Y:<{_atlasY}>");
                        else
                            ImGui.TextColored(Vector4Colors.Red, $"But no proper size was found for the atlas");
                    }
                }
                compatibleValues = _fontName != string.Empty && _validAtlasSize && pathIsCompatible && _availableFiles.Length > 0;
            }
            ImGui.Separator();

            if (_result.HasValue)
                if (_result.Value)
                    ImGui.TextColored(Vector4Colors.Green, "Result: Successful");
                else
                    ImGui.TextColored(Vector4Colors.Red, "Result: Failed");

            if (!compatibleValues)
                ImGui.TextColored(Vector4Colors.Red, "One of the fields above has an incompatible value.");

            if(ImGui.Button("Create") && compatibleValues)
            {
                if (!_createManual)
                {
                    _result = CreateTexture(out string txtPath);
                    if (_result.Value)
                    {
                        _currentSetTexturePath = txtPath;
                        _result = CreateFontDataFile();
                    }
                }
                else
                {
                    _characterOrder = _manualCharacterOrder.ToCharArray();
                    _result = CreateFontDataFile();
                }
            }
            if (ImGui.Button("Close"))
                Close();
        }

        private bool CreateTexture(out string path)
        {
            path = string.Empty;
            char[] allCharacters = new char[_availableFiles.Length];

            for(int i = 0; i < _availableFiles.Length; i++)
            {
                string noExtensionName = Path.GetFileNameWithoutExtension(Path.GetRelativePath(_currentSetCharactersDirectory, _availableFiles[i]));
                if (noExtensionName.Length == 0)
                    continue;
                bool isLowerCase = noExtensionName.Length > 1 && noExtensionName[1] == '_';
                //Expecting file name to be a single character
                //While the "_" suffix is for lowercase characters
                if (noExtensionName.Length == 1 || isLowerCase)
                    if (!_fontIsUpperCaseOnly)
                        allCharacters[i] = isLowerCase ? char.ToLower(noExtensionName[0]) : char.ToUpper(noExtensionName[0]);
                    else
                        allCharacters[i] = noExtensionName[0];
                else
                    allCharacters[i] = InterpretToCharacter(noExtensionName);
            }
            int charAmountX = (int)MathF.Round(_atlasX / _firstCharX);
            int charAmountY = (int)MathF.Round(_atlasY / _firstCharY);
            int count = 0;
            Bitmap texture = new(_atlasX, _atlasY);
            for (int y = 0; y < charAmountY; y++)
            {
                int pixelOffsetY = y * _firstCharY;
                for (int x = 0; x < charAmountX; x++)
                {
                    int pixelOffsetX = x * _firstCharX;
                    try
                    {
                        count++;
                        Bitmap charImage = new(_availableFiles[y * charAmountX + x]);
                        for (int pixelX = 0; pixelX < _firstCharX - 1; pixelX++)
                            for (int pixelY = 0; pixelY < _firstCharY - 1; pixelY++)
                                texture.SetPixel(pixelX + pixelOffsetX, pixelY + pixelOffsetY,
                                    charImage.GetPixel(pixelX, pixelY));
                        charImage.Dispose();
                        count++;
                    }
                    catch(IndexOutOfRangeException ex)
                    {
                        Scriptorium.Warn($"Characters length limit reached, this is normal when image size <{charAmountX * charAmountY}> is greater than the character count <{allCharacters.Length}>.");
                        continue;
                    }
                }
            }
            path = Path.Combine(MainWindow.ProjectFolder, CommonPaths.RelativeFontsFolder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, _fontName + ".png");
            if (!Path.Exists(path))
                using (var f = File.Create(path));
            texture.Save(path, ImageFormat.Png);
            texture.Dispose();

            _characterOrder = allCharacters;
            _charWidth = _firstCharX;
            _charHeight = _firstCharY;

            return true;
        }

        private char InterpretToCharacter(string noExtensionName) => Font.ForbiddenCharNameToChar.First(x => x.Item1.Equals(noExtensionName, StringComparison.CurrentCultureIgnoreCase)).Item2;

        private bool CreateFontDataFile()
        {
            try
            {
                string txtRelativePath = PathUtils.ToRelativePath(_currentSetTexturePath);
                string baseDirectory = Path.Combine(MainWindow.ProjectFolder, CommonPaths.ResourcesFolderName, Directory.GetParent(txtRelativePath).Name);
                string fontDataPath = Path.Combine(baseDirectory, _fontName + Font.FontDataExtension);
                FontData data = new(txtRelativePath, _charWidth, _charHeight, _characterOrder, _fontIsUpperCaseOnly);
                string content = JsonConvert.SerializeObject(data);
                if (!File.Exists(fontDataPath))
                {
                    using var f = File.Create(fontDataPath);
                    f.Dispose();
                }
                File.WriteAllText(fontDataPath, content);
                return true;
            }
            catch(Exception ex)
            {
                Scriptorium.LogException(ex);
                return false;
            }
        }
    }
}
