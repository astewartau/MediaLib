using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;

namespace MediaLib
{

    /// <summary>
    /// Utility class for random number generation
    /// </summary>
    public static class Utility
    {
        private static Random _rng = new Random();

        /// <summary>
        /// Generate a random integer in a range
        /// </summary>
        /// <param name="min">Minimum possible random number</param>
        /// <param name="max">Maximum possible random number</param>
        /// <returns>A random integer between min and max</returns>
        public static int RandInt(int min, int max)
        {
            if (min > max) throw new ArgumentException("Minimum cannot be greater than maximum");
            return _rng.Next(min, max + 1);
        }

        /// <summary>
        /// Generate a random double in a range
        /// </summary>
        /// <param name="min">Minimum possible random number</param>
        /// <param name="max">Maximum possible random number</param>
        /// <returns>A random double between min and max</returns>
        public static double RandDouble(double min = 0, double max = 1)
        {
            if (min > max) throw new ArgumentException("Minimum cannot be greater than maximum");
            return _rng.NextDouble() * Math.Abs(max - min) + min;
        }
    }

    /// <summary>
    /// Contains methods for manipulating and drawing to a graphics window in a procedural style
    /// </summary>
    public static class Window
    {
        // Window
        private static RenderWindow _window = null;
        public static int Width => _window == null ? 0 : (int)_window.Size.X;
        public static int Height => _window == null ? 0 : (int)_window.Size.Y;
        public static bool IsOpen => _window == null ? false : _window.IsOpen;

        // Drawing
        private const string DEFAULT_FONT = "FantasqueSansMono-Regular.ttf";
        private static Color _backgroundColor = Color.Black;
        private static List<Drawable> _drawables = new List<Drawable>();
        private static Color _fillColor = Color.White;
        private static Color _lineColor = Color.White;
        private static int _lineThickness = 0;
        private static Font _font = null;
        private static float _nextTextPosition = 0;
        private static List<Sprite> _sprites = new List<Sprite>();
        private static Dictionary<string, Texture> _textureDict = new Dictionary<string, Texture>();

        // Interaction
        private static Key _keyPressed = Key.Unknown;
        private static Vector2i? _clickPos = null;
        private static Vector2i? _mousePos = null;

        // Audio
        private static List<Sound> _sounds = new List<Sound>();
        private static Music _music = null;

        /// <summary>
        /// Constructor to load the default font
        /// </summary>
        static Window()
        {
            try
            {
                _font = new Font($"assets/fonts/{DEFAULT_FONT}");
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Create a new graphics window with the specified dimensions and title
        /// </summary>
        /// <param name="title">The title of the window</param>
        /// <param name="width">The width of the window</param>
        /// <param name="height">The height of the window</param>
        /// <param name="fullScreen">Whether the window should be in fullscreen (ignore width/height)</param>
        /// <param name="antialiasingLevel">The antialiasing level of the window</param>
        public static void Create(string title = "Window", int width = 640, int height = 480, bool fullScreen = false, uint antialiasingLevel = 0)
        {
            ContextSettings contextSettings = new ContextSettings();
            contextSettings.AntialiasingLevel = antialiasingLevel;

            _window = new RenderWindow(
                mode: fullScreen ? VideoMode.DesktopMode : new VideoMode((uint)width, (uint)height),
                title: title,
                style: fullScreen ? Styles.Fullscreen : Styles.Default,
                settings: contextSettings
            );

            if (!fullScreen) _window.Position = new Vector2i(
                (int)VideoMode.DesktopMode.Width / 2 - width / 2,
                (int)VideoMode.DesktopMode.Height / 2 - height / 2
            );

            _window.Closed += new EventHandler(OnClose);
            _window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPress);
            _window.KeyReleased += new EventHandler<KeyEventArgs>(OnKeyRelease);
            _window.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(OnMouseButtonPress);
            _window.MouseWheelScrolled += new EventHandler<MouseWheelScrollEventArgs>(OnMouseWheelScroll);
            _window.MouseMoved += new EventHandler<MouseMoveEventArgs>(OnMouseMove);

            _window.Clear(_backgroundColor);
            _window.Display();
        }

        /// <summary>
        /// Set the background color of the window
        /// </summary>
        /// <param name="r">Red color component</param>
        /// <param name="g">Green color component</param>
        /// <param name="b">Blue color component</param>
        public static void SetBackgroundColor(byte r, byte g, byte b)
        {
            _backgroundColor = new Color(r, g, b);
            Update();
        }

        /// <summary>
        /// Set the fill color for later objects drawn
        /// </summary>
        /// <param name="r">Red color component</param>
        /// <param name="g">Green color component</param>
        /// <param name="b">Blue color component</param>
        /// <param name="a">Alpha component</param>
        public static void SetFillColor(byte r, byte g, byte b, byte a = 255)
        {
            _fillColor = new Color(r, g, b, a);
        }

        /// <summary>
        /// Set the line color for later objects drawn
        /// </summary>
        /// <param name="r">Red color component</param>
        /// <param name="g">Green color component</param>
        /// <param name="b">Blue color component</param>
        /// <param name="a">Alpha component</param>
        public static void SetLineColor(byte r, byte g, byte b, byte a = 255)
        {
            _lineColor = new Color(r, g, b, a);
        }

        /// <summary>
        /// Set the line thickness for later objects drawn
        /// </summary>
        /// <param name="lineThickness">Line thickness in pixels</param>
        public static void SetLineThickness(int lineThickness)
        {
            _lineThickness = lineThickness;
        }

        /// <summary>
        /// Set the font for later text drawn
        /// </summary>
        /// <param name="fontPath">The filepath of the font to use</param>
        public static void SetFont(string fontPath)
        {
            _font = new Font(fontPath);
        }

        /// <summary>
        /// Draw a line between the given coordinates
        /// </summary>
        /// <param name="x1">The first x-coordinate</param>
        /// <param name="y1">The first y-coordinate</param>
        /// <param name="x2">The second x-coordinate</param>
        /// <param name="y2">The second y-coordinate</param>
        public static void DrawLine(int x1, int y1, int x2, int y2)
        {
            if (_lineThickness == 0)
            {
                VertexArray vertexArray = new VertexArray(PrimitiveType.Lines);
                vertexArray.Append(new Vertex(new Vector2f(x1, y1), _lineColor));
                vertexArray.Append(new Vertex(new Vector2f(x2, y2), _lineColor));
                _drawables.Add(vertexArray);
            }
            else
            {
                // a line with thickness is actually a rectangle with a length determined
                // by pythagoras and a height determined by line thickness
                RectangleShape rect = new RectangleShape(
                    size: new Vector2f(
                        x: (float)Math.Sqrt(Math.Pow(Math.Abs(x2 - x1), 2) + Math.Pow(Math.Abs(y2 - y1), 2)),
                        y: _lineThickness
                    )
                );

                rect.FillColor = _lineColor;
                rect.Position = new Vector2f(x1 + _lineThickness/2, y1 - _lineThickness/2);

                // cartesian space calculation of vector angle
                rect.Rotation = (float)(-Math.Atan2(y1 - y2, x2 - x1) * 180 / Math.PI);
                _drawables.Add(rect);
            }

            Update();
        }

        /// <summary>
        /// Draw a rectangle with a width and height
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left corner</param>
        /// <param name="y">The y-coordinate of the top-left corner</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        /// <param name="centerOrigin">Whether the coordinate is the centre of the rectangle</param>
        /// <param name="rotation">Rotation of the rectangle in degrees</param>
        public static void DrawRect(int x, int y, float width, float height, bool centerOrigin = false, float rotation = 0)
        {
            RectangleShape rect = new RectangleShape(new Vector2f(width, height));

            if (centerOrigin) rect.Origin = new Vector2f(width / 2, height / 2);
            rect.Position = new Vector2f(x, y);
            rect.FillColor = _fillColor;
            rect.OutlineThickness = _lineThickness;
            rect.OutlineColor = _lineColor;
            rect.Rotation = rotation;

            _drawables.Add(rect);
            Update();
        }

        /// <summary>
        /// Draw a circle or polygon at a coordinate with a radius
        /// </summary>
        /// <param name="x">The x-coordinate of the shape</param>
        /// <param name="y">The y-coordinate of the shape</param>
        /// <param name="radius">The radius of the shape</param>
        /// <param name="numSides">Number of sides of polygon (0 for circle)</param>
        /// <param name="centerOrigin">Whether the coordinate is the centre of the shape</param>
        /// <param name="rotation">Rotation of the shape in degrees</param>
        public static void DrawCircle(int x, int y, int radius, int numSides = 0, bool centerOrigin = false, float rotation = 0)
        {
            CircleShape circle;
            if (numSides == 0) circle = new CircleShape(radius);
            else circle = new CircleShape(radius, (uint)numSides);

            if (centerOrigin) circle.Origin = new Vector2f(radius, radius);
            circle.Position = new Vector2f(x, y);
            circle.OutlineThickness = _lineThickness;
            circle.OutlineColor = _lineColor;
            circle.FillColor = _fillColor;
            circle.Rotation = rotation;

            _drawables.Add(circle);
            Update();
        }

        /// <summary>
        /// Draw a string of text at a coordinate
        /// </summary>
        /// <param name="message">The text to draw</param>
        /// <param name="x">The x-coordinate of where to start drawing text</param>
        /// <param name="y">The y-coordinate of where to start drawing text</param>
        /// <param name="charSize">The font size</param>
        /// <param name="fontPath">The path to the font file to use</param>
        /// <param name="centerOrigin">Whether the coordinate is the centre of the text</param>
        /// <param name="rotation">Rotation of the text in degrees</param>
        /// <param name="alpha">An alpha value imposed on the resultant text</param>    
        public static void DrawText(object message, int x, int y, uint charSize = 18, string fontPath = null, bool centerOrigin = false, float rotation = 0, byte alpha = 255)
        {
            if (_window == null) throw new InvalidOperationException("Window not created!");
            if (_font == null && fontPath == null) throw new InvalidOperationException("No font set!");

            Text text = new Text(message.ToString(), fontPath == null ? _font : new Font(fontPath), charSize);

            if (centerOrigin) text.Origin = new Vector2f(text.GetGlobalBounds().Width / 2, text.GetGlobalBounds().Height / 2);
            text.Position = new Vector2f(x, y);
            text.FillColor = _fillColor;
            text.Rotation = rotation;
            if (alpha != 255) text.FillColor = new Color(text.FillColor.R, text.FillColor.G, text.FillColor.B, alpha);

            _drawables.Add(text);
            Update();
        }

        /// <summary>
        /// Draw a string of text at the top-left of the window
        /// </summary>
        /// <param name="message">The text to draw</param>
        /// <param name="charSize">The font size</param>
        /// <param name="fontPath">The path to the font file to use</param>
        /// <param name="alpha">An alpha value imposed on the resultant text</param>
        public static void DrawText(object message, uint charSize = 18, string fontPath = null, byte alpha = 255)
        {
            if (_window == null) throw new InvalidOperationException("Window not created!");
            if (_font == null && fontPath == null) throw new InvalidOperationException("No font set!");

            Text text = new Text(message.ToString(), fontPath == null ? _font : new Font(fontPath), charSize);

            text.Position = new Vector2f(0, _nextTextPosition);
            text.FillColor = _fillColor;
            if (alpha != 255) text.FillColor = new Color(text.FillColor.R, text.FillColor.G, text.FillColor.B, alpha);

            _nextTextPosition += text.GetGlobalBounds().Height + 8;
            _drawables.Add(text);
            Update();
        }

        /// <summary>
        /// Draw an image from file to the window
        /// </summary>
        /// <param name="filePath">The filepath of the image</param>
        /// <param name="x">The x-coordinate of the top-left corner of the image</param>
        /// <param name="y">The y-coordinate of the top-left corner of the image</param>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <param name="centerOrigin">Whether the coordinate is the centre of the image</param>
        /// <param name="rotation">Rotation of the image in degrees</param>
        /// <param name="alpha">An imposed alpha value on the resultant image</param>
        public static void DrawImage(string filePath, int x = 0, int y = 0, int width = 0, int height = 0, bool centerOrigin = false, float rotation = 0, byte alpha = 255)
        {
            if (!_textureDict.ContainsKey(filePath)) _textureDict[filePath] = new Texture(filePath);
            Sprite sprite = new Sprite(_textureDict[filePath]);

            if (centerOrigin) sprite.Origin = new Vector2f(sprite.GetGlobalBounds().Width / 2, sprite.GetGlobalBounds().Height / 2);
            sprite.Rotation = rotation;
            if (width > 0 && height > 0) sprite.Scale = new Vector2f(width / sprite.GetGlobalBounds().Width, height / sprite.GetGlobalBounds().Height);
            sprite.Position = new Vector2f(x, y);
            if (alpha != 255) sprite.Color = new Color(sprite.Color.R, sprite.Color.G, sprite.Color.B, alpha);

            _sprites.Add(sprite);
            _drawables.Add(sprite);
            Update();
        }

        /// <summary>
        /// Play the given sound
        /// </summary>
        /// <param name="filePath">The filepath of the sound file</param>
        /// <param name="volume">The volume (0-100)</param>
        public static void PlaySound(string filePath, float volume = 100)
        {
            if (_sounds.Count == 256)
            {
                _sounds[0].Dispose();
                _sounds.RemoveAt(0);
            }

            Sound sound = new Sound(new SoundBuffer(filePath));
            sound.Volume = volume;
            sound.Pitch = 1;

            _sounds.Add(sound);
            sound.Play();
        }

        /// <summary>
        /// Play the given music file (only one at a time, loop)
        /// </summary>
        /// <param name="filePath">The filepath of the music file</param>
        /// <param name="volume">The volume (0-100)</param>
        public static void PlayMusic(string filePath, float volume = 100)
        {
            if (_music == null || _music.Status == SoundStatus.Playing)
            {
                _music = new Music(filePath);
                _music.Play();
            }
        }

        /// <summary>
        /// Wait for the specified number of miliseconds
        /// </summary>
        /// <param name="miliseconds">The number of miliseconds to wait</param>
        public static void Sleep(int miliseconds)
        {
            System.Threading.Thread.Sleep(miliseconds);
        }

        /// <summary>
        /// Halt processing until the window is closed
        /// </summary>
        public static void WaitUntilClose()
        {
            if (_window == null) throw new InvalidOperationException("Window not created!");
            while (_window.IsOpen)
            {
                Update();
            }
        }

        /// <summary>
        /// Pause execution and return the next keypress
        /// </summary>
        /// <returns>A Keyboard.Key value</returns>
        public static Key GetKeyPressed()
        {
            if (_window == null) throw new InvalidOperationException("Window not created!");
            while (_window.IsOpen && _keyPressed == (Key)Keyboard.Key.Unknown)
            {
                Update();
            }
            Key result = _keyPressed;
            _keyPressed = (Key)Keyboard.Key.Unknown;
            return result;
        }

        /// <summary>
        /// Pause execution and return the next mouse click location
        /// </summary>
        /// <returns>An integer array containing the x,y position of the mouse after a click</returns>
        public static int[] GetClickLocation()
        {
            if (_window == null) throw new InvalidOperationException("Window not created!");
            while (_window.IsOpen && _clickPos == null)
            {
                Update();
            }

            int[] result;
            if (_window.IsOpen)
            {
                result = new int[] { _clickPos.Value.X, _clickPos.Value.Y };
                _clickPos = null;
            }
            else
            {
                result = new int[] { 0, 0 };
            }

            return result;
        }

        /// <summary>
        /// Pause execution and return the next mouse location
        /// </summary>
        /// <returns>An integer array containing the x,y position of the mouse after a click</returns>
        public static int[] GetMouseMoveLocation()
        {
            if (_window == null) throw new InvalidOperationException("Window not created!");
            while (_window.IsOpen && _mousePos == null)
            {
                Update();
            }

            int[] result;
            if (_window.IsOpen)
            {
                result = new int[] { _mousePos.Value.X, _mousePos.Value.Y };
                _mousePos = null;
            }
            else
            {
                result = new int[] { 0, 0 };
            }

            return result;
        }

        /// <summary>
        /// Clear all drawable items from the window
        /// </summary>
        /// <param name="update">Whether or not to re-draw immediately</param>
        public static void Clear(bool update = true)
        {
            _nextTextPosition = 0;
            foreach (Sprite sprite in _sprites)
            {
                sprite.Dispose();
            }
            foreach (KeyValuePair<string, Texture> pair in _textureDict)
            {
                pair.Value.Dispose();
            }
            _textureDict.Clear();
            _sprites.Clear();
            _drawables.Clear();
            if (update) Update();
        }

        /// <summary>
        /// Update the window to refresh its display
        /// </summary>
        private static void Update()
        {
            if (_window == null) throw new InvalidOperationException("Window not created!");
            if (_window.IsOpen)
            {
                _window.DispatchEvents();
                _window.Clear(_backgroundColor);
                for (int i = 0; i < _drawables.Count; i++)
                {
                    _window.Draw(_drawables[i]);
                }
                _window.Display();
            }
        }

        /// <summary>
        /// OnClose event-handler to close the RenderWindow
        /// </summary>
        private static void OnClose(object sender, EventArgs eventArgs)
        {
            ((RenderWindow)sender).Close();
        }

        /// <summary>
        /// OnKeyPress event-handler
        /// </summary>
        private static void OnKeyPress(object sender, KeyEventArgs eventArgs)
        {
            _keyPressed = (Key)eventArgs.Code;
        }

        /// <summary>
        /// OnKeyRelease event-handler
        /// </summary>
        private static void OnKeyRelease(object sender, KeyEventArgs eventArgs)
        {
            if ((Key)eventArgs.Code == _keyPressed) { _keyPressed = Key.Unknown; }
        }

        /// <summary>
        /// OnMouseButtonPress event-handler
        /// </summary>
        private static void OnMouseButtonPress(object sender, MouseButtonEventArgs eventArgs)
        {
            _clickPos = new Vector2i(eventArgs.X, eventArgs.Y);
        }

        /// <summary>
        /// OnMouseWheelScroll event-handler
        /// </summary>
        private static void OnMouseWheelScroll(object sender, MouseWheelScrollEventArgs eventArgs)
        {
            _clickPos = new Vector2i(eventArgs.X, eventArgs.Y);
        }

        /// <summary>
        /// OnMouseMove event-handler
        /// </summary>
        private static void OnMouseMove(object sender, MouseMoveEventArgs eventArgs)
        {
            _mousePos = new Vector2i(eventArgs.X, eventArgs.Y);
        }
    }

    /// <summary>
    /// Represents a Keyboard Key (duplicate of SFML Key enum)
    /// </summary>
    public enum Key
    {
        Unknown = -1,
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6,
        H = 7,
        I = 8,
        J = 9,
        K = 10,
        L = 11,
        M = 12,
        N = 13,
        O = 14,
        P = 15,
        Q = 16,
        R = 17,
        S = 18,
        T = 19,
        U = 20,
        V = 21,
        W = 22,
        X = 23,
        Y = 24,
        Z = 25,
        Num0 = 26,
        Num1 = 27,
        Num2 = 28,
        Num3 = 29,
        Num4 = 30,
        Num5 = 31,
        Num6 = 32,
        Num7 = 33,
        Num8 = 34,
        Num9 = 35,
        Escape = 36,
        LControl = 37,
        LShift = 38,
        LAlt = 39,
        LSystem = 40,
        RControl = 41,
        RShift = 42,
        RAlt = 43,
        RSystem = 44,
        Menu = 45,
        LBracket = 46,
        RBracket = 47,
        Semicolon = 48,
        SemiColon = 48,
        Comma = 49,
        Period = 50,
        Quote = 51,
        Slash = 52,
        Backslash = 53,
        BackSlash = 53,
        Tilde = 54,
        Equal = 55,
        Hyphen = 56,
        Dash = 56,
        Space = 57,
        Enter = 58,
        Return = 58,
        Backspace = 59,
        BackSpace = 59,
        Tab = 60,
        PageUp = 61,
        PageDown = 62,
        End = 63,
        Home = 64,
        Insert = 65,
        Delete = 66,
        Add = 67,
        Subtract = 68,
        Multiply = 69,
        Divide = 70,
        Left = 71,
        Right = 72,
        Up = 73,
        Down = 74,
        Numpad0 = 75,
        Numpad1 = 76,
        Numpad2 = 77,
        Numpad3 = 78,
        Numpad4 = 79,
        Numpad5 = 80,
        Numpad6 = 81,
        Numpad7 = 82,
        Numpad8 = 83,
        Numpad9 = 84,
        F1 = 85,
        F2 = 86,
        F3 = 87,
        F4 = 88,
        F5 = 89,
        F6 = 90,
        F7 = 91,
        F8 = 92,
        F9 = 93,
        F10 = 94,
        F11 = 95,
        F12 = 96,
        F13 = 97,
        F14 = 98,
        F15 = 99,
        Pause = 100,
        KeyCount = 101
    }
}
