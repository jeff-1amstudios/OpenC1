using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace PlatformEngine
{
    public enum Actions
    {
        MoveForwardsPrimary,
        MoveForwardsAlternate,
        MoveBackwardsPrimary,
        MoveBackwardsAlternate,
        StrafeRightPrimary,
        StrafeRightAlternate,
        StrafeLeftPrimary,
        StrafeLeftAlternate,
        RunPrimary,
        RunAlternate,
        CrouchPrimary,
        CrouchAlternate,
        JumpPrimary,
        JumpAlternate
    };

    public enum MouseInputMode
    {
        FPS,
        FreeMouse
    }

    public class InputProvider : GameComponent
    {
        private GamePadState _gamePadState, _previousGamePadState;

        private KeyboardState _keyboardState, _previousKeyboardState;
        private MouseState _mouseState;
        private Vector2 _mouseDelta;
        private float _mouseMultiplier;

        private Vector2[] _mouseSmoothingCache;
        private const int MOUSE_SMOOTHING_CACHE_SIZE = 15;
        private const float MOUSE_SMOOTHING_SENSITIVITY = 0.5f;
        private const float SENSITIVITY = 1.0f;
        private MouseInputMode _mouseInputMode;
        private float _perFrameMultiplier;
        

        public InputProvider(Game game)
            : base(game)
        {
            _mouseDelta = new Vector2();
            _mouseMultiplier = 0.3f;
            // Initialize the mouse smoothing cache.
            _mouseSmoothingCache = new Vector2[MOUSE_SMOOTHING_CACHE_SIZE];
            _mouseInputMode = MouseInputMode.FPS;

            Vector2 screenCenter = GetScreenCenter();
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
        }

        public GamePadState GamePadState
        {
            get { return _gamePadState; }
        }

        public MouseState MouseState
        {
            get { return _mouseState; }
        }

        public override void Update(GameTime gameTime)
        {
            float frameTime = Engine.ElapsedSeconds;
            _perFrameMultiplier = frameTime * SENSITIVITY;
            
            _previousKeyboardState = _keyboardState;
            _previousGamePadState = _gamePadState;
            _keyboardState = Keyboard.GetState();
            _gamePadState = GamePad.GetState(PlayerIndex.One);

            if (_previousKeyboardState == null)
                _previousKeyboardState = _keyboardState;

            _mouseState = Mouse.GetState();

            if (_keyboardState.IsKeyDown(Keys.LeftAlt))
            {
                MouseMode = MouseInputMode.FreeMouse;
            }
            else
            {
                MouseMode = MouseInputMode.FPS;

                Vector2 screenCenter = GetScreenCenter();
                _mouseDelta.X = (-1 * (screenCenter.X - _mouseState.X)) * _mouseMultiplier;
                _mouseDelta.Y = (screenCenter.Y - _mouseState.Y) * _mouseMultiplier;
                SmoothMouseMovement();

                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
            }
            
            base.Update(gameTime);
        }

        private Vector2 GetScreenCenter()
        {
            GameWindow window = Engine.Game.Window;
            return new Vector2(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
        }

        public float MoveForward
        {
            get
            {
                if (_gamePadState.ThumbSticks.Left.Y != 0)
                    return _gamePadState.ThumbSticks.Left.Y * _perFrameMultiplier;
                else if (_keyboardState.IsKeyDown(Keys.W))
                    return 1.0f * _perFrameMultiplier;
                else if (_keyboardState.IsKeyDown(Keys.S))
                    return -1.0f * _perFrameMultiplier;
                else
                    return 0.0f;
            }
        }
        public float Strafe
        {
            get
            {
                if (_gamePadState.ThumbSticks.Left.X != 0)
                    return _gamePadState.ThumbSticks.Left.X * _perFrameMultiplier;
                else if (_keyboardState.IsKeyDown(Keys.A))
                    return -1.0f * _perFrameMultiplier;
                else if (_keyboardState.IsKeyDown(Keys.D))
                    return 1.0f * _perFrameMultiplier;
                else
                    return 0.0f;
            }
        }

        public float LeftThumbDelta
        {
            get
            {
                if (_gamePadState.ThumbSticks.Right.Y != 0)
                    return _gamePadState.ThumbSticks.Right.Y * _perFrameMultiplier;
                else
                {
                    return _mouseDelta.Y * _perFrameMultiplier;
                }
            }
        }

        public float RightThumbDelta
        {
            get
            {
                if (_gamePadState.ThumbSticks.Right.X != 0)
                    return _gamePadState.ThumbSticks.Right.X * _perFrameMultiplier;
                else
                {
                    return _mouseDelta.X * _perFrameMultiplier;
                }
            }
        }


        public bool WasPressed(Keys key)
        {
            return _previousKeyboardState.IsKeyDown(key) && !_keyboardState.IsKeyDown(key);
        }

        public bool WasPressed(Buttons button)
        {
            return _previousGamePadState.IsButtonDown(button) && !_gamePadState.IsButtonDown(button);
        }

        public bool IsKeyDown(Keys key)
        {
            return _keyboardState.IsKeyDown(key);
        }

        public MouseInputMode MouseMode
        {
            get { return _mouseInputMode; }
            set
            {
                if (_mouseInputMode == MouseInputMode.FreeMouse && value == MouseInputMode.FPS)
                {
                    Vector2 screenCenter = GetScreenCenter();
                    Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
                    _mouseState = Mouse.GetState();
                }
                else if (_mouseInputMode == MouseInputMode.FPS && value == MouseInputMode.FreeMouse)
                {
                    _mouseDelta = new Vector2();
                }
                _mouseInputMode = value;                    
            }
        }

        public Vector2 MousePosition
        {
            get { return new Vector2(_mouseState.X, _mouseState.Y); }
        }


        /// <summary>
        /// Filters the mouse movement based on a weighted sum of mouse
        /// movement from previous frames to ensure that the mouse movement
        /// this frame is smooth.
        /// </summary>
        private void SmoothMouseMovement()
        {
            // Shuffle all the entries in the cache.
            // Newer entries at the front. Older entries towards the back.
            for (int i = _mouseSmoothingCache.Length - 1; i > 0; --i)
            {
                _mouseSmoothingCache[i].X = _mouseSmoothingCache[i - 1].X;
                _mouseSmoothingCache[i].Y = _mouseSmoothingCache[i - 1].Y;
            }

            // Store the current mouse movement entry at the front of cache.
            _mouseSmoothingCache[0].X = _mouseDelta.X;
            _mouseSmoothingCache[0].Y = _mouseDelta.Y;

            float averageX = 0.0f;
            float averageY = 0.0f;
            float averageTotal = 0.0f;
            float currentWeight = 1.0f;

            // Filter the mouse movement with the rest of the cache entries.
            // Use a weighted average where newer entries have more effect than
            // older entries (towards the back of the cache).
            for (int i = 0; i < _mouseSmoothingCache.Length; ++i)
            {
                averageX += _mouseSmoothingCache[i].X * currentWeight;
                averageY += _mouseSmoothingCache[i].Y * currentWeight;
                averageTotal += 1.0f * currentWeight;
                currentWeight *= MOUSE_SMOOTHING_SENSITIVITY;
            }

            // Calculate the new smoothed mouse movement.
            _mouseDelta.X = averageX / averageTotal;
            _mouseDelta.Y = averageY / averageTotal;
        }
    }
}
