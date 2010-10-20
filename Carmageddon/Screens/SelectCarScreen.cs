using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.IO;
using OpenC1.HUD;
using OneAmEngine;

namespace OpenC1.Screens
{
    class SelectCarScreen : BaseMenuScreen
    {
        BasicEffect2 _effect;
        List<OpponentInfo> _opponents;
        public static SpriteFont _titleFont;

        public SelectCarScreen(BaseMenuScreen parent)
            : base(parent)
        {
            _titleFont = Engine.ContentManager.Load<SpriteFont>("content/LucidaConsole");

            SimpleCamera cam = Engine.Camera as SimpleCamera;
            cam.DrawDistance = 999999;

            _inAnimation = new FliPlayer(LoadAnimation("chcrcome.fli"));
            _inAnimation.Play(false);

            _outAnimation = new FliPlayer(LoadAnimation("chcraway.fli"));

            _effect = new BasicEffect2();
            _effect.LightingEnabled = false;
            _effect.TexCoordsMultiplier = 1;
            _effect.TextureEnabled = true;

            Engine.Camera.Position = new Vector3(-1.5f, 3.5f, 10);
            Engine.Camera.Orientation = new Vector3(0, -0.28f, -1);
            Engine.Camera.Update();
            _effect.View = Engine.Camera.View;
            _effect.Projection = Engine.Camera.Projection;

            _opponents = OpponentsFile.Instance.Opponents;
            if (GameVars.Emulation != EmulationMode.Demo)
            {
                // If we're not in demo mode, add car files in directory that havent been added to opponent.txt
                List<string> carFiles = new List<string>(Directory.GetFiles(GameVars.BasePath + "data\\cars"));
                carFiles.RemoveAll(a => !a.ToUpper().EndsWith(".TXT"));
                carFiles.Sort();
                carFiles.Reverse();
                foreach (string file in carFiles)
                {
                    string filename = Path.GetFileName(file);
                    if (!_opponents.Exists(a => a.FileName.Equals(filename, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        _opponents.Insert(0, new OpponentInfo { FileName = filename, Name = Path.GetFileNameWithoutExtension(filename), StrengthRating = 1 });
                    }
                }
            }
            
            _options.Add(new CarModelMenuOption(_effect, _opponents[0]));
        }

        public override void Update()
        {
            base.Update();

            if (_selectedOption == _options.Count - 1 && _opponents.Count > _selectedOption + 1)
            {
                _options.Add(new CarModelMenuOption(_effect, _opponents[_selectedOption + 1]));
            }
        }

        public override void Render()
        {
            GameVars.CurrentEffect = _effect;
            base.Render();
        }

        public override void OnOutAnimationFinished()
        {
            GameVars.SelectedCarFileName = _opponents[_selectedOption].FileName;
            ReturnToParent();
        }
    }

    class CarModelMenuOption : IMenuOption
    {
        BasicEffect2 _effect;
        VehicleModel _model;
        OpponentInfo _info;
        
        public CarModelMenuOption(BasicEffect2 effect, OpponentInfo info)
        {
            _effect = effect;
            _info = info;
            try
            {
                var carfile = new CarFile(GameVars.BasePath + "data\\cars\\" + info.FileName);
                _model = new VehicleModel(carfile, true);
            }
            catch (Exception ex)
            {
                _info.Name = ex.Message;
            }
        }

        public void RenderInSpriteBatch()
        {
            Engine.SpriteBatch.DrawString(SelectCarScreen._titleFont, _info.Name.ToUpperInvariant(), BaseHUDItem.ScaleVec2(0.22f, 0.17f), Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);
            Engine.SpriteBatch.DrawString(SelectCarScreen._titleFont, new String('_', _info.Name.Length), BaseHUDItem.ScaleVec2(0.22f, 0.175f), Color.Red, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);
        }

        public void RenderOutsideSpriteBatch()
        {
            if (_model == null)
                return;

            Engine.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            _effect.Begin(SaveStateMode.None);

            _model.Update();
            Engine.Device.RenderState.DepthBufferEnable = true;
            Engine.Device.RenderState.CullMode = CullMode.None;
            _model.Render(Matrix.CreateScale(1.2f) * Matrix.CreateRotationY(2.55f));

            _effect.End();
        }
    }
}
