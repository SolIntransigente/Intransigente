using Dazed.Vitals.Content;
using Dazed.Vitals.Rendering;
using OpenTK.Mathematics;

namespace Dazed.Vitals
{
    public class Font
    {
        public const string FontDataExtension = ".fd";

        public readonly Texture2D FontTexture;
        private readonly Dictionary<char, Vector2> _charTexPos = new();
        public readonly float CharacterAspectRatio;

        public readonly static (string, char)[] ForbiddenCharNameToChar =
            [   ("tilde", '´'), ("asterisk", '*'), ("comilla", '"'), ("double_dot", ':'),
                ("slash_right_up", '/'), ("slash_right_down", '\\'), ("vertical_bar", '|'),
                ("less_than", '<'), ("greater_than", '>'), ("hash", '#'), ("percent", '%'),
                ("ampersand", '&'), ("at", '@'), ("caret", '^'), ("dollar", '$'), ("exclamation", '!'),
                ("question", '?')];

        private int CharAmountX => _nx;
        private int CharAmountY => _ny;
        private readonly int _nx, _ny;
        private readonly bool _isUppercaseOnly;

        public Vector2 TextureCharScale { get; }

        private static readonly Dictionary<string, Font?> FontInstances = new();

        public static Font? DefaultFont { get; internal set; }

        public static Font GetFontInstance(string assetRelativePath, bool forceNewInstance = false)
        {
            if (forceNewInstance)
            {
                Font tFont = new Font(assetRelativePath);
                return tFont;
            }
            if (!FontInstances.TryGetValue(assetRelativePath, out var font))
            {
                font = new Font(assetRelativePath);
                FontInstances.Add(assetRelativePath, font);
            }
            return font ?? throw new Exception("Font failed to be created with asset: " + assetRelativePath);
        }

        private Font(string fontDataPath)
        {
            //No testing for file existence, as to ensure the game throws an exception in case of the font not existing
            FontData data = Cereal.LoadData<FontData>(fontDataPath);

            FontTexture = Texture2D.GetTextureInstance(new TextureIdentification(data.TextureRelativePath, ImageDataType.RGBA, true));
            var charWidth = data.CharWidth;
            var charHeight = data.CharHeight;
            _isUppercaseOnly = data.IsUpperCaseOnly;

            CharacterAspectRatio = charWidth / (float)charHeight;

            //Number of characters in X
            _nx = (int)(FontTexture.Width / data.CharWidth);
            //Number of characters in Y
            _ny = (int)(FontTexture.Height / data.CharHeight);
         
            int currentX = 0, currentY = 0;
            foreach (var t in data.CharOrder)
            {
                if (currentX == _nx)
                {
                    currentX = 0;
                    currentY++;
                }
                char c = _isUppercaseOnly ? char.ToLower(t) : t;
                _charTexPos.Add(c, new(currentX, currentY));
                currentX++;
            }
            TextureCharScale = new Vector2(1 / (float)CharAmountX, 1 / (float)CharAmountY);
        }
        
        public Vector2 GetCharPosInAtlas(char c, out char? specialCharacter, bool normalized = true)
        {
            var chars = InterpretSpecialCharacter(c);
            specialCharacter = chars.special;
            if (chars.main == null)
                c = _isUppercaseOnly ? char.ToLower(c) : c;
            else
                c = chars.main.Value;
            if (!_charTexPos.TryGetValue(c, out Vector2 pos))
            {
                return Vector2.Zero;
            }
            if (normalized)
                pos = new Vector2(pos.X / _nx, pos.Y / _ny);
            return pos;
        }

        private (char? main, char? special) InterpretSpecialCharacter(char c)
        {
            return c switch
            {
                //Tilde
                'á' => ('a', '´'),
                'é' => ('e', '´'),
                'í' => ('i', '´'),
                'ó' => ('o', '´'),
                'ú' => ('u', '´'),
                //Dieresis
                'ä' => ('a', '¨'),
                'ë' => ('e', '¨'),
                'ï' => ('i', '¨'),
                'ö' => ('o', '¨'),
                'ü' => ('u', '¨'),
                _ => (null, null),
            };
        }
    }
}
